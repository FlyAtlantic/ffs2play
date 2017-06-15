using System;
using System.IO;
using System.Timers;
using System.Drawing;
using System.Net;
using ProtoBuf;

namespace ffs2play
{
	public partial class Peer
	{
		/// <summary>
		/// Réception d'un datagramme entrant sur le socket udp
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceiveMessage(object sender, UDPReceiveEvent evt)
		{
			if (m_EP==null)
			{
				if (m_InternalIP.Contains(evt.Client.Address))
				{
					m_EP = new IPEndPoint(evt.Client.Address, m_Port);
				}
				else return;
			}
			else if ((!evt.Client.Address.Equals(m_EP.Address)) || (evt.Client.Port!=m_Port)) return;
			DateTime Now = evt.Time;
			MemoryStream stream = new MemoryStream(evt.Data);
			BinaryReader reader = new BinaryReader(stream);
			try
			{
				byte Code = reader.ReadByte();
				switch ((Protocol)Code)
				{
					case Protocol.PING:
						{
							SendPong(Now);
							break;
						}
					case Protocol.PONG:
						{
							m_Latence = TimeSpan.FromMilliseconds((Now - m_LastPing).TotalMilliseconds/2);
							m_Counter = 0;
							DateTime TimePong = Outils.TimeFromUnixTimestamp(reader.ReadInt64());
							if (m_MiniPing > m_Latence.TotalMilliseconds)
							{
								m_Decalage = Now - m_Latence - TimePong;
								m_MiniPing = m_Latence.TotalMilliseconds;
							}
							if (!m_OnLine)
							{
#if DEBUG
								Log.LogMessage("Peer [" + CallSign + "] Passage en état OnLine", Color.DarkBlue, 1);
#endif
								m_OnLine = true;
								m_Distance = -1;
								m_Counter_In = 0;
								m_Counter_Out = 0;
							}
							if (m_Version == 0) RequestVersion();
							P2P.UpdateListItem(m_CallSign);
							break;
						}
					case Protocol.CHAT:
						{
							string Message = reader.ReadString();
							P2P.AddLineChat(CallSign, Message);
							m_SC.SendScrollingText(CallSign + " : " + Message);
							break;
						}
					case Protocol.DATA:
						{
							if (m_Version == PROTO_VERSION)
							{
								m_Mutex.WaitOne();
								try
								{
									byte CounterIn = reader.ReadByte();
									// Si le compteur reçu est supérieur nous avons une donnée récente
									if (((CounterIn - m_Counter_In) > 0) || (m_Counter_In == 255))
									{
										m_OldData.Clone(m_Data);
										int Len = reader.ReadInt32();
                                        reader.BaseStream.Seek(2, 0);

                                        m_Data = (AirData)Serializer.Deserialize<AirData>(reader.BaseStream);
										m_Data.TimeStamp += m_Decalage;
										if (m_Data.TimeStamp == m_OldData.TimeStamp)
										{
#if DEBUG
											Log.LogMessage("Peer[" + CallSign + "] Data double détectée", Color.DarkBlue, 1);
#endif
											return;
										}
										if ((m_Spawned >= 4) && (m_Spawned < 5)) m_Spawned++;
										m_RefreshRate = Now - m_LastData;
										m_LastData = Now;
										m_RemoteRefreshRate = m_Data.TimeStamp - m_OldData.TimeStamp;
										m_Distance = Outils.distance(m_Data.Latitude, m_Data.Longitude, m_SendData.Latitude, m_SendData.Longitude, 'N');
#if DEBUG
										if ((CounterIn - m_Counter_In) > 1) Log.LogMessage("Peer [" + CallSign + "] Paquets Udp Manquants =" + (CounterIn - m_Counter_In - 1).ToString(), Color.DarkViolet, 1);
#endif
										m_Refresh = true;
									}
#if DEBUG
									else
									{
										Log.LogMessage("Peer [" + CallSign + "] Paquets Udp ignoré en retard de " + (m_Counter_In - CounterIn).ToString(), Color.DarkViolet, 1);
									}
#endif
									m_Counter_In = CounterIn;
								}
								finally
								{
									m_Mutex.ReleaseMutex();
								}
							}
							break;
						}
					case Protocol.VERSION:
						{
							m_Version = reader.ReadByte();
							if (m_Version == PROTO_VERSION)
							{
								m_Data.Title = reader.ReadString();
								if (m_Spawned >= 3)
								{
									Spawn_AI(false);
								}
								m_Data.Type = reader.ReadString();
								m_Data.Model = reader.ReadString();
								m_Data.Category = reader.ReadString();
							}
							P2P.UpdateListItem(m_CallSign);
#if DEBUG
							Log.LogMessage("Peer [" + CallSign + "] reçu numéro de version = " + m_Version.ToString(), Color.DarkBlue, 1);
#endif
							break;
						}
					case Protocol.REQ_VERSION:
						{
							SendVersion();
							break;
						}
				}
			}
			catch (Exception e)
			{
				Log.LogMessage("Peer [" + CallSign + "] Erreur d'analyse du datagramme : " + e.Message, Color.DarkViolet, 0);
			}
		}


		/// <summary>
		/// Mise à jour des données vers le peer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="evt"></param>
		private void OnHeartBeat(object source, ElapsedEventArgs evt)
		{
			SendPing();
			TimeSpan IntervalAI = Outils.Now - m_LastAIUpdate;
			if ((m_Spawned >=6) && (IntervalAI.TotalSeconds > 20))
			{
				Spawn_AI(false);
#if DEBUG
				Log.LogMessage("Peer [" + CallSign + "] HeartBeat Détection de freeze de l'AI, recréation de l'objet", Color.DarkBlue);
#endif
			}
		}

		/// <summary>
		/// Event Frame Simulateur
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OnSCReceiveEventFrame(object sender, SCManagerEventFrame e)
		{
			if (m_Spawned >= 5) Update_AI(e.Time, e.FrameRate);
		}

		/// <summary>
		/// Réception d'un AI Object crée
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAICreated(object sender, SCManagerEventAICreated e)
		{
			if (e.Tail == m_Tail)
			{
				if ((e.Object_ID != 0)&&(m_ObjectID==0))
				{
					m_ObjectID = e.Object_ID;
					m_Spawned = 4;
					//m_SC.Update_AI(m_ObjectID, DEFINITIONS_ID.AI_INIT, m_InitAI);
					m_LastAIUpdate = Outils.Now;
#if DEBUG
					Log.LogMessage("Peer [" + CallSign + "] Reçu Object_ID = " + m_ObjectID.ToString(), Color.DarkBlue, 1);
#endif
				}
				else
				{
					Log.LogMessage("Peer [" + CallSign + "] reçu un avion non demandé de la simconnect", Color.DarkViolet);
				}
			}
		}

		/// <summary>
		/// Réception d'une suppression d'objet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAIRemoved(object sender, SCManagerEventObjectDeleted e)
		{
			if (e.Object_ID == m_ObjectID)
			{
				m_ObjectID = 0;
				m_Spawned = 0;
#if DEBUG
				Log.LogMessage("Peer [" + CallSign + "] Reçu un removed Object de la simconnect " + e.Object_ID.ToString(), Color.DarkBlue, 1);
#endif
			}
		}
		/// <summary>
		/// Réception d'un Event SimConnect
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSCReceiveEvent(object sender, SCManagerEventSCEvent e)
		{
			//SendEvent(e.Evt_Id, e.Data);
		}


		private void OnSCReceiveAIUpdate(object sender, SCManagerEventAIUpdate e)
		{
			if (m_ObjectID != e.ObjectID) return;
			m_LastAIUpdate =e.Time;
			m_Ecart = Outils.distance(m_Data.Latitude, m_Data.Longitude, e.Data.Latitude, e.Data.Longitude, 'N');
			if (m_Spawned >= 6)
			{
				if ((m_Ecart > 0.2))
				{
					Spawn_AI(false);
#if DEBUG
					Log.LogMessage("Peer [" + CallSign + "] Détection de freeze de l'AI, recréation de l'objet", Color.DarkBlue);
#endif
					return;
				}
			}
		}

		/// <summary>
		/// Réception de la mise à jour de la position et de l'atitude de l'avion de l'hôte
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStateChange(object sender, AnalyseurStateEvent e)
		{
			bool Title_Changed = false;
			if (m_SendData.Title != e.Data.Title)
			{
				Title_Changed = true;
			}
			m_SendData.Clone(e.Data);
			if ((e.Data.TimeStamp - m_LastStateEvent).TotalMilliseconds < m_SendInterval) return;
			if ((!m_OnLine) || (m_EP == null) || (m_Version < 1)) return;
			//Test du changement d'appareil
			if (Title_Changed)
			{
				SendVersion();
			}
			m_LastStateEvent = e.Data.TimeStamp;
			// Gestion de l'affichage de l'AI
			m_bSpawnable = (m_Distance < Properties.Settings.Default.P2PRadius) && (m_Distance >= 0);
			if (m_bSpawnable)
			{
				m_SendInterval = (uint)Properties.Settings.Default.P2PRate;
			}
			else
			{
				m_SendInterval = 5000;
			}
			if (m_bSpawnable && Visible && (m_Spawned == 0))
			{
				Spawn_AI(true);
#if DEBUG
				Log.LogMessage("Peer [" + CallSign + "] Construction de l'objet AI", Color.DarkBlue, 1);
#endif
			}
			if (((!m_bSpawnable) || (!Visible)) && (m_Spawned > 0))
			{
				Spawn_AI(false);
#if DEBUG
				Log.LogMessage("Peer [" + CallSign + "] Destruction de l'objet AI Visible=" + Visible.ToString() + " , Spawnable = " + m_bSpawnable.ToString(), Color.DarkBlue, 1);
#endif
			}

			SendData();
		}

		/// <summary>
		/// En cas de non création de l'AI par Simconnect, cela arrive lorsque le Title fourni n'est correct,
		/// Nous créons un AI générique de remplacement
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimerCreateAI(object sender, ElapsedEventArgs e)
		{
			m_TimerCreateAI.Stop();
			if ((m_Spawned >= 2) && (m_ObjectID == 0))
			{
				m_Spawned = 2;
				if (!m_TrySimpleAI)
				{
					m_TrySimpleAI = true;
#if DEBUG
					Log.LogMessage("Peer [" + CallSign + "] Simmconnect n'a pas crée l'AI demandé. On test avec un AI non pilotable", Color.DarkBlue, 1);
#endif
				}
				else
				{
					m_AIResolution.Titre = "Mooney Bravo";
					m_AIResolution.CG_Height = 3.7;
					m_AIResolution.Pitch = 2.9;
#if DEBUG
					Log.LogMessage("Peer [" + CallSign + "] Simmconnect n'a pas crée l'AI demandé. L'avion sera affiché avec un générique", Color.DarkBlue, 1);
#endif
				}
				CreateAI();
			}
			else
			{
#if DEBUG
				Log.LogMessage("Peer [" + CallSign + "] La simconnect a bien crée l'avion demandé", Color.DarkBlue, 1);
#endif
			}
		}
	}
}
