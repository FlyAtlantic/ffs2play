/****************************************************************************
**
** Copyright (C) 2017 FSFranceSimulateur team.
** Contact: https://github.com/ffs2/ffs2play
**
** FFS2Play is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 3 of the License, or
** (at your option) any later version.
**
** FFS2Play is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** The license is as published by the Free Software
** Foundation and appearing in the file LICENSE.GPL3
** included in the packaging of this software. Please review the following
** information to ensure the GNU General Public License requirements will
** be met: https://www.gnu.org/licenses/gpl-3.0.html.
****************************************************************************/

/****************************************************************************
 * Peer.Event.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

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
            // Si l'adresse du peer est nulle cela signifie que le wazzhup update a détecté un PEER local
			if (m_EP==null)
			{
                // Si le peer à une addresse locale qui correspond 
                // Il s'agit d'un client sur le même réseau local
                // On prend alors cette IP locale comme référence
				if (m_InternalIP.Contains(evt.Client.Address))
				{
					m_EP = new IPEndPoint(evt.Client.Address, m_Port);
				}
				else return;
			}
			else if ((!evt.Client.Address.Equals(m_EP.Address)) || (evt.Client.Port!=m_Port)) return;
			BinaryReader reader = new BinaryReader(new MemoryStream(evt.Data));
			try
			{
				switch ((Protocol)reader.ReadByte())
				{
					case Protocol.PING:
						{
							SendPong(evt.Time);
							break;
						}
					case Protocol.PONG:
						{
							m_Latence = TimeSpan.FromMilliseconds((evt.Time - m_LastPing).TotalMilliseconds/2);
							m_Counter = 0;
							DateTime TimePong = DateTimeEx.TimeFromUnixTimestamp(reader.ReadInt64());
							if (m_MiniPing > m_Latence.TotalMilliseconds)
							{
								m_Decalage = evt.Time - m_Latence - TimePong;
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
                                if (m_bBlockData) return;
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
										if (m_Data.TimeStamp <= m_OldData.TimeStamp)
										{
#if DEBUG
											Log.LogMessage("Peer[" + CallSign + "] Donées en retard ignorées", Color.DarkBlue, 1);
#endif
											return;
										}
										if ((m_Spawned >= 4) && (m_Spawned < 5)) m_Spawned++;
										m_RefreshRate = evt.Time - m_LastData;
										m_LastData = evt.Time;
										m_RemoteRefreshRate = m_Data.TimeStamp - m_OldData.TimeStamp;
										m_Distance = Outils.distance(m_Data.Latitude, m_Data.Longitude, m_SendData.Latitude, m_SendData.Longitude, 'N');
#if DEBUG
										if ((CounterIn - m_Counter_In) > 1) Log.LogMessage("Peer [" + CallSign + "] Paquets Udp Manquants =" + (CounterIn - m_Counter_In - 1).ToString(), Color.DarkViolet, 1);
#endif
                                        RefreshData();
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
            if (m_SC.GetVersion() >= SIM_VERSION.P3D_V2) return;
			TimeSpan IntervalAI = DateTimeEx.UtcNow - m_LastAIUpdate;
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
                    m_SC.Freeze_AI(m_ObjectID, m_Data.OnGround);
                    m_LastAIUpdate = DateTimeEx.UtcNow;
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

        /// <summary>
        /// Event sur récéption des données de l'AI sur le simulateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

		private void OnSCReceiveAIUpdate(object sender, SCManagerEventAIUpdate e)
		{
			if (m_ObjectID != e.ObjectID) return;
			m_LastAIUpdate =e.Time;
            m_AISimData = e.Data;
            m_Ecart = Outils.distance(m_Data.Latitude, m_Data.Longitude, m_AISimData.Latitude, m_AISimData.Longitude, 'N');
            if (m_SC.GetVersion() >= SIM_VERSION.P3D_V2) return;
            if (m_Spawned >= 6)
			{
				if ((m_Ecart > 0.4))
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
            //On envoi les données aux peers
            SendData();
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
