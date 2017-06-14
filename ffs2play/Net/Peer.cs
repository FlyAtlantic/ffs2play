using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
#if P3D
using LockheedMartin.Prepar3D.SimConnect;
#else
using Microsoft.FlightSimulator.SimConnect;
#endif
using System.Collections.Generic;
using System.Threading;
using ProtoBuf;

namespace ffs2play
{
	enum Protocol
	{
		PING,
		PONG,
		CHAT,
		DATA,
		VERSION,
		EVENT,
		REQ_VERSION
	}

	public struct Deltas
	{
		public double Longitude;
		public double Latitude;
		public double Altitude;
		public double Heading;
		public double Pitch;
		public double Bank;
	}

	public partial class Peer : IDisposable
	{
		private System.Timers.Timer HeartBeat;
		private int m_Counter;
		private byte m_Counter_In;
		private byte m_Counter_Out;
		private bool m_OnLine;
		private string m_CallSign;
		private string m_Tail;
		private SCManager m_SC;
		private AIMapping m_AIMapping;
		private IPEndPoint m_EP;
		private List<IPAddress> m_InternalIP;
		private int m_Port;
		private IPAddress m_ExternalIP;
		private PirepManager m_PM;
		private AirData m_Data;
		private AirData m_OldData;
		private AirData m_SendData;
		private AirData m_FuturData;
		private AirData m_ActualPos;
		private AIMoveStruct m_AIData;
		private DateTime m_LastPing;
		private DateTime m_LastData;
		private uint m_SendInterval;
		private DateTime m_LastStateEvent;
		private UDPServer Server;
		private Logger Log;
		private P2PManager P2P;
		private TimeSpan m_Latence;
		private double m_Distance;
		private TimeSpan m_RefreshRate;
		private TimeSpan m_RemoteRefreshRate;
		private TimeSpan m_Decalage;
		private double m_MiniPing;
		private uint m_ObjectID;
		private byte m_Version;
		private int m_Spawned;
		private bool m_Refresh;
		private AnalyseurManager m_Analyseur;
		public const byte PROTO_VERSION = 13;
		private double m_PredictiveTime;
		private List<double> m_FrameRateArray;
		//Synchronisation des demandes de AI
		private System.Timers.Timer m_TimerCreateAI;
		private AIResol m_AIResolution;
		private int m_FrameCount;
		private double m_Ecart;
		private DateTime m_LastAIUpdate;
		private bool m_bDisabled;
		private Mutex m_Mutex;
		private bool m_bSelected;
		private bool m_bLocal;
		private int m_sel_iplocal;
		private bool m_TrySimpleAI;
		private DateTime m_LastRender;
		private double m_DeltaAltitude;
		private double m_DeltaLongitude;
		private double m_DeltaLatitude;
		private double m_DeltaHeading;
		private double m_DeltaPitch;
		private double m_DeltaBank;
		public bool Visible;
		private bool m_bSpawnable;
#if DEBUG
        private string Trace;
#endif
        private double m_old_fps;

		/// <summary>
		/// Constructeur d'un Pair P2P
		/// </summary>
		/// <param name="pServer"></param>
		public Peer(ref UDPServer pServer, IPAddress pExternalIP, int pPort, string CallSign, bool pDisabled = false, bool pLocal = false, List<IPAddress> pInternalIP = null)
		{
			m_CallSign = CallSign;
			P2P = P2PManager.Instance;
			m_AIMapping = AIMapping.Instance;
			Server = pServer;
			m_Data = new AirData();
			m_OldData = new AirData();
			m_SendData = new AirData();
			m_FuturData = new AirData();
			m_ActualPos = new AirData();
			m_AIData = new AIMoveStruct();
			m_ExternalIP = pExternalIP;
			m_InternalIP = pInternalIP;
			m_Port = pPort;
			Log = Logger.Instance;
			m_bLocal = pLocal;
			if (!m_bLocal) m_EP = new IPEndPoint(m_ExternalIP, m_Port);
			else m_EP = null;
			m_SC = SCManager.Instance;
			m_Analyseur = AnalyseurManager.Instance;
			m_PM = PirepManager.Instance;
			m_FrameRateArray = new List<double>();
			m_bDisabled = true;
			m_Mutex = new Mutex();
			m_sel_iplocal = 0;
			m_TrySimpleAI = false;
			Disabled = pDisabled;
			Visible = false;
			m_bSpawnable = false;
		}

		public AIResol AIResolution
		{
			get { return m_AIResolution; }
		}

		public bool Spawnable
		{
			get { return m_bSpawnable; }
		}

		public bool Disabled
		{
			get { return m_bDisabled; }
			set
			{
				if (m_bDisabled == value) return;
				else
				{
					if (value) Stop();
					else Start();
				}
			}
		}

		public bool Selected
		{
			get { return m_bSelected; }
			set { m_bSelected = value; }
		}

		private void Start ()
		{
			if (!m_bDisabled) return;
			Server.OnReceiveMessage += OnReceiveMessage;
			m_SC.OnAICreated += OnAICreated;
			m_SC.OnAIRemoved += OnAIRemoved;
			m_SC.OnSCReceiveEventFrame += OnSCReceiveEventFrame;
			m_SC.OnSCReceiveEvent += OnSCReceiveEvent;
			m_SC.OnSCReceiveAIUpdate += OnSCReceiveAIUpdate;
			m_Analyseur.OnStateChange += OnStateChange;
			HeartBeat = new System.Timers.Timer();
			HeartBeat.Elapsed += OnHeartBeat;
			HeartBeat.Interval = 5000;
			HeartBeat.Start();
			m_TimerCreateAI = new System.Timers.Timer(5000);
			m_TimerCreateAI.Elapsed += OnTimerCreateAI;
			m_LastData = DateTime.UtcNow;
			m_LastStateEvent = DateTime.UtcNow;
			m_Counter = 0;
			m_Counter_In = 0;
			m_Counter_Out = 0;
			m_Distance = -1;
			m_OnLine = false;
			m_SendInterval = 5000;
			m_FrameCount = 0;
			m_Ecart = 0;
			m_ObjectID = 0;
			m_Version = 0;
			m_Spawned = 0;
			m_Refresh = false;
			m_MiniPing = 1000;
			m_PredictiveTime = 1000;
			if (Properties.Settings.Default.P2PInfoEnable)
				m_SC.SendScrollingText(CallSign + " est connecté au réseau P2P");
			m_bDisabled = false;
		}

		private void Stop ()
		{
			if (m_bDisabled) return;
			Spawn_AI(false);
			HeartBeat.Stop();
			HeartBeat.Dispose();
			m_TimerCreateAI.Stop();
			m_TimerCreateAI.Dispose();
			m_Analyseur.OnStateChange -= OnStateChange;
			m_SC.OnSCReceiveEvent -= OnSCReceiveEvent;
			m_SC.OnSCReceiveEventFrame -= OnSCReceiveEventFrame;
			m_SC.OnAICreated -= OnAICreated;
			m_SC.OnAIRemoved -= OnAIRemoved;
			Server.OnReceiveMessage -= OnReceiveMessage;
			m_SC.OnSCReceiveAIUpdate -= OnSCReceiveAIUpdate;
			if (Properties.Settings.Default.P2PInfoEnable)
				m_SC.SendScrollingText(CallSign + " vient de quitter le réseau P2P");
			m_bDisabled = true;
			m_Spawned = 0;
			P2P.UpdateListItem(m_CallSign);
		}

		// Flag: Has Dispose already been called?
		bool disposed = false;
		// Instantiate a SafeHandle instance.
		SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

		// Public implementation of Dispose pattern callable by consumers.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				Stop();
				handle.Dispose();
			}

			// Free any unmanaged objects here.
			//
			disposed = true;
		}

		public DateTime LastAIUpdate
		{
			get { return m_LastAIUpdate; }
		}

		public int Spawned
		{
			get { return m_Spawned; }
		}

		public uint ObjectId
		{
			get { return m_ObjectID; }
		}

		public IPAddress ExternalIP
		{
			get { return m_ExternalIP; }
		}

		public string CallSign
		{
			get { return m_CallSign; }
		}

		public int Port
		{
			get { return m_Port; }
		}

		public bool IsOnline
		{
			get { return m_OnLine; }
		}

		public double Altitude
		{
			get { return m_Data.Altitude; }
		}

		public double Decalage

		{
			get { return m_Decalage.TotalMilliseconds; }
		}

		public int Latence
		{
			get { return m_Latence.Milliseconds; }
		}

		public double Distance
		{
			get { return m_Distance; }
		}

		public double RefreshRate
		{
			get { return m_RefreshRate.TotalMilliseconds; }
		}

		public double RemoteRefreshRate
		{
			get { return m_RemoteRefreshRate.TotalMilliseconds; }
		}

		public double Latitude
		{
			get { return m_Data.Latitude; }
		}

		public double Longitude
		{
			get { return m_Data.Longitude; }
		}

		public double Vitesse
		{
			get { return m_Data.IASSpeed; }
		}

		public double Direction
		{
			get { return m_Data.Heading; }
		}

		public double BankAngle
		{
			get { return m_Data.Bank; }
		}

		public double PitchAngle
		{
			get { return m_Data.Pitch; }
		}

		public bool OnGround
		{
			get { return m_Data.OnGround; }
		}

		public uint Version
		{
			get { return m_Version; }
		}

		public string Titre
		{
			get { return m_Data.Title; }
		}

		public string Model
		{
			get { return m_Data.Model; }
		}

		public string Type
		{
			get { return m_Data.Type; }
		}

		public double Elevator
		{
			get { return m_Data.ElevatorPos; }
		}

		public double Aileron
		{
			get { return m_Data.AileronPos; }
		}

		public double Rudder
		{
			get { return m_Data.RudderPos; }
		}


		public int FrameCount
		{
			get { return m_FrameCount; }
		}

		public double Ecart
		{
			get { return m_Ecart; }
		}

		public int Squawk
		{
			get { return m_Data.Squawk; }
		}
		/// <summary>
		/// Gestion de la création et de la destruction de l'AI
		/// </summary>
		/// <param name="Flag"></param>
		private void Spawn_AI(bool Flag)
		{
			if (Flag && (m_ObjectID == 0) && (m_Spawned == 0))
			{
				m_TrySimpleAI = false;
#if DEBUG
				Log.LogMessage("Peer [" + m_CallSign + "] Requete locale de résolution Title = " + m_Data.Title + " , Model = " + m_Data.Model, Color.DarkBlue, 1);
#endif
				m_AIResolution = m_AIMapping.SolveTitle(m_Data.Title, m_Data.Model, m_Data.Type);
				if (m_AIResolution.Titre != "Mooney Bravo")
				{
#if DEBUG
					Log.LogMessage("Peer [" + m_CallSign + "] Requete locale a trouvé une résolution", Color.DarkBlue, 1);
#endif
				}
				m_Spawned = 2;
				CreateAI();
			}
			if ((!Flag) && (m_Spawned > 0))
			{
				m_Spawned = 0;
				m_TimerCreateAI.Stop();
				if (m_ObjectID != 0)
				{
					if (m_SC.Remove_AI(ref m_ObjectID))
					{
#if DEBUG
						Log.LogMessage("Peer [" + m_CallSign + "] Suppression de l'avion", Color.DarkBlue, 1);
#endif
					}
					else Log.LogMessage("Peer [" + m_CallSign + "] Erreur lors de la suppression de l'avion", Color.DarkViolet);
				}
			}
		}

		/// <summary>
		/// Demande de création d'AI sur Simconnect
		/// </summary>
		private void CreateAI()
		{
			if (m_Spawned == 2)
			{
				if (m_ObjectID != 0)
				{
					m_SC.Remove_AI(ref m_ObjectID);
				}
				if (m_CallSign.Length > 8) m_Tail = m_CallSign.Remove(8);
				else m_Tail = m_CallSign;
				SIMCONNECT_DATA_INITPOSITION Init;
				Init.Altitude = m_Data.Altitude;
				Init.Latitude = m_Data.Latitude;
				Init.Longitude = m_Data.Longitude;
				Init.Heading = m_Data.Heading;
				Init.Pitch = m_Data.Pitch;
				Init.Bank = m_Data.Bank;
				Init.OnGround = Convert.ToUInt32(m_Data.OnGround);
				Init.Airspeed = Convert.ToUInt32(m_Data.IASSpeed);
				if (m_Data.Category == "Helicopter") m_TrySimpleAI = true;
				m_TimerCreateAI.Start();
#if DEBUG
				Log.LogMessage("Peer [" + m_CallSign + "] Demande de création de l'avion avec le Title " + m_AIResolution.Titre, Color.DarkBlue, 1);
#endif
				m_SC.Create_AI(m_AIResolution.Titre, m_Tail, ref Init, m_TrySimpleAI);
				m_Spawned = 3;
			}
		}
		/// <summary>
		/// Mise à jour de la position et de l'atitude de l'AI
		/// </summary>
		/// <param name="FrameRate"></param>
		private void Update_AI(DateTime Time, float FPS)
		{
			m_Mutex.WaitOne();
			try
			{
				//DateTime Maintenant = DateTime.UtcNow;
				if ((m_ObjectID == 0) || (m_Version != PROTO_VERSION)) return;

				//On synchronise les données event
				if (m_Spawned >= 5)
				{
					m_AIData.AileronPos = m_Data.AileronPos;
					m_AIData.ElevatorPos = m_Data.ElevatorPos;
					m_AIData.RudderPos = m_Data.RudderPos;
					m_AIData.Door1Pos = m_Data.Door1Pos;
					m_AIData.Door2Pos = m_Data.Door2Pos;
					m_AIData.Door3Pos = m_Data.Door3Pos;
					m_AIData.Door4Pos = m_Data.Door4Pos;
					m_AIData.StateEng1 = Convert.ToInt32(m_Data.StateEng1);
					m_AIData.StateEng2 = Convert.ToInt32(m_Data.StateEng2);
					m_AIData.StateEng3 = Convert.ToInt32(m_Data.StateEng3);
					m_AIData.StateEng4 = Convert.ToInt32(m_Data.StateEng4);
					m_AIData.ThrottleEng1 = m_Data.ThrottleEng1;
					m_AIData.ThrottleEng2 = m_Data.ThrottleEng2;
					m_AIData.ThrottleEng3 = m_Data.ThrottleEng3;
					m_AIData.ThrottleEng4 = m_Data.ThrottleEng4;
					m_AIData.Squawk = m_Data.Squawk;
					m_AIData.GearPos = Convert.ToInt32(m_Data.GearPos);
					m_AIData.FlapsIndex = m_Data.FlapsIndex;
					m_AIData.LandingLight = Convert.ToInt32(m_Data.LandingLight);
					m_AIData.StrobeLight = Convert.ToInt32(m_Data.StrobeLight);
					m_AIData.BeaconLight = Convert.ToInt32(m_Data.BeaconLight);
					m_AIData.NavLight = Convert.ToInt32(m_Data.NavLight);
					m_AIData.RecoLight = Convert.ToInt32(m_Data.RecoLight);
					m_AIData.Smoke = Convert.ToInt32(m_Data.Smoke);
#if DEBUG
					Trace += "|" + Time.Millisecond.ToString();
#endif
					//Si on a suffisement de donnée (new + old) on utilise l'interpolation
					if (m_Spawned >= 7)
					{
						if (m_old_fps !=0)
						{
							FPS = (FPS + (float)m_old_fps) / 2;
						}
						m_old_fps = FPS;
						double TempsFPS = 1000 / FPS;
						//double TempsMes = (Time - m_LastRender).TotalMilliseconds;
						double CoefInterpol = TempsFPS / (m_FuturData.TimeStamp - m_ActualPos.TimeStamp).TotalMilliseconds;
#if DEBUG
						Trace += " : " + string.Format("{0:0.000}", CoefInterpol);
#endif
						m_LastRender = Time;
						m_AIData.Altitude += m_DeltaAltitude * CoefInterpol;
						m_AIData.Latitude += m_DeltaLatitude * CoefInterpol;
						m_AIData.Longitude += m_DeltaLongitude * CoefInterpol;
						m_AIData.Heading += m_DeltaHeading * CoefInterpol;
						if (m_AIData.Heading < 0) m_AIData.Heading += 360;
						if (m_AIData.Heading >= 360) m_AIData.Heading -= 360;
						m_AIData.Pitch += m_DeltaPitch * CoefInterpol;
						if (m_AIData.Pitch < -180) m_AIData.Pitch += 360;
						if (m_AIData.Pitch >= 180) m_AIData.Pitch -= 360;
						m_AIData.Bank += m_DeltaBank * CoefInterpol;
						if (m_AIData.Bank < -180) m_AIData.Bank += 360;
						if (m_AIData.Bank >= 180) m_AIData.Bank -= 360;
					}

					//Nous avons des données toutes fraiches et nous sommes prêt à calculer l'extrapolation
					if (m_Refresh && (m_Spawned >= 6))
					{
						double CoefHoriz = 0;
						m_PredictiveTime = m_RemoteRefreshRate.TotalMilliseconds * 5;
						//Calcul de l'extrapolation
						double Retard = (m_LastData - m_Data.TimeStamp).TotalMilliseconds; //Calcul du retard absolu de la donnée
						if (Retard < 0) Retard = 0;
						double Periode = m_RemoteRefreshRate.TotalMilliseconds; //Calcul de la période entre deux données
						if (Periode > 0)
						{
							// On mémorise la position actuelle de l'extrapolation
							m_ActualPos.TimeStamp = Time;
							m_ActualPos.Altitude = m_AIData.Altitude;
							m_ActualPos.Latitude = m_AIData.Latitude;
							m_ActualPos.Longitude = m_AIData.Longitude;
							m_ActualPos.Heading = m_AIData.Heading;
							m_ActualPos.Bank = m_AIData.Bank;

							m_FuturData.TimeStamp = Time + TimeSpan.FromMilliseconds(m_PredictiveTime);
							CoefHoriz = (m_FuturData.TimeStamp - m_Data.TimeStamp).TotalMilliseconds / Periode;
							m_FuturData.Altitude = m_Data.Altitude + ((m_Data.Altitude - m_OldData.Altitude) * CoefHoriz);
							m_FuturData.Longitude = m_Data.Longitude + ((m_Data.Longitude - m_OldData.Longitude) * CoefHoriz);
							m_FuturData.Latitude = m_Data.Latitude + ((m_Data.Latitude - m_OldData.Latitude) * CoefHoriz);
							double Gam_Heading = m_Data.Heading - m_OldData.Heading;
							if (Gam_Heading < -180) Gam_Heading += 360;
							if (Gam_Heading > 180) Gam_Heading -= 360;
							m_FuturData.Heading = m_Data.Heading + (Gam_Heading * CoefHoriz);
							if (m_FuturData.Heading < 0) m_FuturData.Heading += 360;
							if (m_FuturData.Heading >= 360) m_FuturData.Heading -= 360;
							double Gam_Pitch = m_Data.Pitch - m_OldData.Pitch;
							if (Gam_Pitch < -180) Gam_Pitch += 360;
							if (Gam_Pitch > 180) Gam_Pitch -= 360;
							m_FuturData.Pitch = m_Data.Pitch + (Gam_Pitch * CoefHoriz);
							if (m_FuturData.Pitch < -180) m_FuturData.Pitch += 360;
							if (m_FuturData.Pitch >= 180) m_FuturData.Pitch -= 360;
							double Gam_Bank = m_Data.Bank - m_OldData.Bank;
							if (Gam_Bank < -180) Gam_Bank += 360;
							if (Gam_Bank > 180) Gam_Bank -= 360;
							m_FuturData.Bank = m_Data.Bank + (Gam_Bank * CoefHoriz);
							if (m_FuturData.Bank < -180) m_FuturData.Bank += 360;
							if (m_FuturData.Bank >= 180) m_FuturData.Bank -= 360;
							//Calcul des deltas de correction
							m_DeltaAltitude = m_FuturData.Altitude - m_ActualPos.Altitude;
							m_DeltaLatitude = m_FuturData.Latitude - m_ActualPos.Latitude;
							m_DeltaLongitude = m_FuturData.Longitude - m_ActualPos.Longitude;
							m_DeltaHeading = m_FuturData.Heading - m_ActualPos.Heading;
							if (m_DeltaHeading < -180) m_DeltaHeading += 360;
							if (m_DeltaHeading >= 180) m_DeltaHeading -= 360;
							m_DeltaPitch = m_FuturData.Pitch - m_ActualPos.Pitch;
							if (m_DeltaPitch < -180) m_DeltaPitch += 360;
							if (m_DeltaPitch >= 180) m_DeltaPitch -= 360;
							m_DeltaBank = m_FuturData.Bank - m_ActualPos.Bank;
							if (m_DeltaBank < -180) m_DeltaBank += 360;
							if (m_DeltaBank >= 180) m_DeltaBank -= 360;
						}
						m_Refresh = false;
						m_Spawned = 7;
						m_ActualPos.Pitch = m_AIData.Pitch;
#if DEBUG
						if (m_bSelected)
						{
							Log.LogMessage("Peer [" + m_CallSign + "] Interpolation : " + Trace, Color.DarkBlue, 2);
							Trace="";
							Log.LogMessage("Peer [" + m_CallSign + "] DeltaTime = " + Periode.ToString() +
							" Coef Horiz = " + CoefHoriz.ToString() +
							" Predictive = " + m_PredictiveTime.ToString() +
							" Décalage = " + m_Decalage.TotalMilliseconds.ToString() +
							" Remote Refresh rate = " + m_RemoteRefreshRate.TotalMilliseconds.ToString() +
							" Retard = " + Retard.ToString() +
							" DeltaLat = " + (m_Data.Latitude - m_OldData.Latitude).ToString() +
							" DeltaLon = " + (m_Data.Longitude - m_OldData.Longitude).ToString() +
							" DeltaAlt = " + (m_Data.Altitude - m_OldData.Altitude).ToString(), Color.DarkBlue, 2);
						}
#endif
					}

					//Si c'est le premier positionnement, on initialise les données avec la position brute
					if ((m_Spawned == 5) && m_Refresh)
					{
						m_AIData.Altitude = m_Data.Altitude;
						m_AIData.Latitude = m_Data.Latitude;
						m_AIData.Longitude = m_Data.Longitude;
						m_AIData.Heading = m_Data.Heading;
						m_AIData.Pitch = m_Data.Pitch;
						m_AIData.Bank = m_Data.Bank;
						m_LastRender = Time;
						if (m_Spawned == 5) m_Spawned = 6;
					}
					if (m_AIData.Altitude > 10)
					{
						double AlitudeMini = (m_SendData.Altitude - m_SendData.AltitudeSol) + m_AIResolution.CG_Height;
						double AlitudeMaxi = (m_SendData.Altitude - m_SendData.AltitudeSol) + m_AIResolution.CG_Height;//+3
						if (m_AIData.Altitude < AlitudeMini)
						{
							m_AIData.Altitude = AlitudeMini;
						}
						else if (m_Data.OnGround && (m_AIData.Altitude > AlitudeMaxi))
						{
							m_AIData.Altitude = AlitudeMaxi;
						}
						if(m_Data.OnGround && (m_AIResolution.Pitch>0 ))
						{
							m_AIData.Pitch = -m_AIResolution.Pitch;
						}
					}
					m_FrameCount++;
					if (m_FrameCount > 1000) m_FrameCount = 0;
					m_SC.Update_AI(m_ObjectID, DEFINITIONS_ID.AI_MOVE, m_AIData);
				}
			}
			finally
			{
				m_Mutex.ReleaseMutex();
			}
		}

		public void ResetAI()
		{
			if (m_Spawned >= 3)
			{
				Spawn_AI(false);
			}
		}

        /// <summary>
        /// Classe de donnée de l'appareil à un instant donné
        /// </summary>
        [ProtoContract]
        class AirData
        {
            public AirData()
            {
                TimeStamp = DateTime.UtcNow;
                Title = "";
                Model = "";
                Type = "";
                Category = "";
                Altitude = 0;
                AltitudeSol = 0;
                IASSpeed = 0;
                Heading = 0;
                Longitude = 0;
                Latitude = 0;
                Pitch = 0;
                Bank = 0;
                Squawk = 0;
                //Etat
                OnGround = false;
                ElevatorPos = 0;
                AileronPos = 0;
                RudderPos = 0;
                Door1Pos = 0;
                Door2Pos = 0;
                Door3Pos = 0;
                Door4Pos = 0;
                ThrottleEng1 = 0;
                ThrottleEng2 = 0;
                ThrottleEng3 = 0;
                ThrottleEng4 = 0;
                FlapsIndex = 0;
                GearPos = false;
                LandingLight = false;
                StrobeLight = false;
                BeaconLight = false;
                NavLight = false;
                RecoLight = false;
                StateEng1 = false;
                StateEng2 = false;
                StateEng3 = false;
                StateEng4 = false;
                Smoke = false;
            }
            public AirData(AirData Object)
            {
                Clone(Object);
            }

            public AirData(AircraftState Object)
            {
                Clone(Object);
            }
            public void Clone(AirData Object)
            {
                TimeStamp = Object.TimeStamp;
                Title = Object.Title;
                Model = Object.Model;
                Type = Object.Type;
                Category = Object.Category;
                Altitude = Object.Altitude;
                AltitudeSol = Object.AltitudeSol;
                Heading = Object.Heading;
                Longitude = Object.Longitude;
                Latitude = Object.Latitude;
                IASSpeed = Object.IASSpeed;
                Pitch = Object.Pitch;
                Bank = Object.Bank;
                Squawk = Object.Squawk;
                ElevatorPos = Object.ElevatorPos;
                AileronPos = Object.AileronPos;
                RudderPos = Object.RudderPos;
                Door1Pos = Object.Door1Pos;
                Door2Pos = Object.Door2Pos;
                Door3Pos = Object.Door3Pos;
                Door4Pos = Object.Door4Pos;
                FlapsIndex = Object.FlapsIndex;
                NumEngine = Object.NumEngine;
                ThrottleEng1 = Object.ThrottleEng1;
                ThrottleEng2 = Object.ThrottleEng2;
                ThrottleEng3 = Object.ThrottleEng3;
                ThrottleEng4 = Object.ThrottleEng4;

                //Etat
                OnGround = Object.OnGround;
                GearPos = Object.GearPos;
                LandingLight = Object.LandingLight;
                BeaconLight = Object.BeaconLight;
                StrobeLight = Object.StrobeLight;
                NavLight = Object.NavLight;
                RecoLight = Object.RecoLight;
                StateEng1 = Object.StateEng1;
                StateEng2 = Object.StateEng2;
                StateEng3 = Object.StateEng3;
                StateEng4 = Object.StateEng4;
                Smoke = Object.Smoke;
            }

            public void Clone(AircraftState Object)
            {
                TimeStamp = Object.TimeStamp;
                Title = Object.Title;
                Model = Object.Model;
                Type = Object.Type;
                Category = Object.Category;
                Altitude = Object.Altitude;
                AltitudeSol = Object.AltitudeSol;
                Heading = Object.Heading;
                Longitude = Object.Longitude;
                Latitude = Object.Latitude;
                IASSpeed = Object.IASSpeed;
                Pitch = Object.Pitch;
                Bank = Object.Bank;
                Squawk = Object.Squawk;
                ElevatorPos = Object.ElevatorPos;
                AileronPos = Object.AileronPos;
                RudderPos = Object.RudderPos;
                Door1Pos = Object.Door1Pos;
                Door2Pos = Object.Door2Pos;
                Door3Pos = Object.Door3Pos;
                Door4Pos = Object.Door4Pos;
                FlapsIndex = Object.FlapsIndex;
                NumEngine = Object.NumEngine;
                ThrottleEng1 = Object.ThrottleEng1;
                ThrottleEng2 = Object.ThrottleEng2;
                ThrottleEng3 = Object.ThrottleEng3;
                ThrottleEng4 = Object.ThrottleEng4;

                //Etat
                OnGround = Object.OnGround;
                GearPos = Object.GearPos;
                LandingLight = Object.LandingLight;
                BeaconLight = Object.BeaconLight;
                StrobeLight = Object.StrobeLight;
                NavLight = Object.NavLight;
                RecoLight = Object.RecoLight;
                StateEng1 = Object.StateEng1;
                StateEng2 = Object.StateEng2;
                StateEng3 = Object.StateEng3;
                StateEng4 = Object.StateEng4;
                Smoke = Object.Smoke;
            }
            //Temps réel
            [ProtoMember(1)]
            public DateTime TimeStamp;
            [ProtoMember(2)]
            public string Title;
            [ProtoMember(3)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string Model;
            [ProtoMember(4)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string Type;
            [ProtoMember(5)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
            public string Category;
            [ProtoMember(6)]
            public double Altitude;
            [ProtoMember(7)]
            public double AltitudeSol;
            [ProtoMember(8)]
            public double Heading;
            [ProtoMember(9)]
            public double Longitude;
            [ProtoMember(10)]
            public double Latitude;
            [ProtoMember(11)]
            public double IASSpeed;
            [ProtoMember(12)]
            public double Pitch;
            [ProtoMember(13)]
            public double Bank;
            [ProtoMember(14)]
            public int Squawk;
            [ProtoMember(15)]
            public double ElevatorPos;
            [ProtoMember(16)]
            public double AileronPos;
            [ProtoMember(17)]
            public double RudderPos;
            [ProtoMember(18)]
            public int Door1Pos;
            [ProtoMember(19)]
            public int Door2Pos;
            [ProtoMember(20)]
            public int Door3Pos;
            [ProtoMember(21)]
            public int Door4Pos;
            [ProtoMember(22)]
            public int FlapsIndex;
            [ProtoMember(23)]
            public int NumEngine;
            [ProtoMember(24)]
            public int ThrottleEng1;
            [ProtoMember(25)]
            public int ThrottleEng2;
            [ProtoMember(26)]
            public int ThrottleEng3;
            [ProtoMember(27)]
            public int ThrottleEng4;
            //Etat
            [ProtoMember(28)]
            public bool OnGround;
            [ProtoMember(29)]
            public bool GearPos;
            [ProtoMember(30)]
            public bool LandingLight;
            [ProtoMember(31)]
            public bool BeaconLight;
            [ProtoMember(32)]
            public bool StrobeLight;
            [ProtoMember(33)]
            public bool NavLight;
            [ProtoMember(34)]
            public bool RecoLight;
            [ProtoMember(35)]
            public bool StateEng1;
            [ProtoMember(36)]
            public bool StateEng2;
            [ProtoMember(37)]
            public bool StateEng3;
            [ProtoMember(38)]
            public bool StateEng4;
            [ProtoMember(39)]
            public bool Smoke;
        }

    }


}