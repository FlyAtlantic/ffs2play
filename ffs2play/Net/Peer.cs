﻿/****************************************************************************
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
 * Peer.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

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
    /// <summary>
    /// Protocol de communication entre PEER
    /// </summary>
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

    /// <summary>
    /// Classe représentant un PEER
    /// </summary>
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
        private AIUpdateStruct m_AISimData;
		private long m_LastPing;
		private long m_LastData;
		private long m_SendInterval;
		private long m_LastStateEvent;
		private UDPServer Server;
		private Logger Log;
		private P2PManager P2P;
		private long m_Latence;
		private double m_Distance;
		private long m_RefreshRate;
		private long m_RemoteRefreshRate;
		private long m_Decalage;
		private long m_MiniPing;
		private uint m_ObjectID;
		private byte m_Version;
		private int m_Spawned;
		private AnalyseurManager m_Analyseur;
		public const byte PROTO_VERSION = 14;
		private long m_PredictiveTime;
		private List<double> m_FrameRateArray;
		//Synchronisation des demandes de AI
		private System.Timers.Timer m_TimerCreateAI;
		private AIResol m_AIResolution;
		private double m_Ecart;
		private long m_LastAIUpdate;
		private bool m_bDisabled;
		private Mutex m_Mutex;
		private bool m_bSelected;
		private bool m_bLocal;
		private int m_sel_iplocal;
		private bool m_TrySimpleAI;
		private long m_LastRender;
		private double m_DeltaAltitude;
		private double m_DeltaLongitude;
		private double m_DeltaLatitude;
		private double m_DeltaHeading;
		private double m_DeltaPitch;
		private double m_DeltaBank;
		public bool Visible;
		private bool m_bSpawnable;
        private bool m_bBlockData;
        private float m_old_fps;
        private double m_old_GND_AI;

		/// <summary>
		/// Constructeur d'un PEER P2P
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
            m_bBlockData = false;
        }

        /// <summary>
        /// Bloquage de la réception des données du PEER
        /// Utilisé en mode debug pour simuler un PEER ne recevant plus de données
        /// </summary>

        public bool BlockData
        {
            get { return m_bBlockData; }
            set { m_bBlockData = value; }
        }

        /// <summary>
        /// Correspondance de l'AI
        /// </summary>

		public AIResol AIResolution
		{
			get { return m_AIResolution; }
		}

        /// <summary>
        /// Affichable
        /// </summary>

		public bool Spawnable
		{
			get { return m_bSpawnable; }
		}

        /// <summary>
        /// Bloquaqe du PEER
        /// </summary>

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

        /// <summary>
        /// Selection du PEER par le parent (P2PManager)
        /// </summary>

		public bool Selected
		{
			get { return m_bSelected; }
			set { m_bSelected = value; }
		}

        /// <summary>
        /// initialise et démarre l'entité PEER
        /// </summary>

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
			m_LastData = DateTimeEx.UtcNowMilli;
			m_LastStateEvent = m_LastData;
			m_Counter = 0;
			m_Counter_In = 0;
			m_Counter_Out = 0;
			m_Distance = -1;
			m_OnLine = false;
			m_SendInterval = 5000;
			m_Ecart = 0;
			m_ObjectID = 0;
			m_Version = 0;
			m_Spawned = 0;
			//m_Refresh = false;
			m_MiniPing = 1000;
			//m_PredictiveTime = 1000;
			if (Properties.Settings.Default.P2PInfoEnable)
				m_SC.SendScrollingText(CallSign + " est connecté au réseau P2P");
			m_bDisabled = false;
		}

        /// <summary>
        /// Bloque et inhibe un PEER tout en le maintenant existant
        /// </summary>
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

		public long LastAIUpdate
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

		public long Decalage

		{
			get { return m_Decalage; }
		}

		public long Latence
		{
			get { return m_Latence; }
		}

		public double Distance
		{
			get { return m_Distance; }
		}

		public long RefreshRate
		{
			get { return m_RefreshRate; }
		}

		public long RemoteRefreshRate
		{
			get { return m_RemoteRefreshRate; }
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
				try
				{
					Init.Airspeed = Convert.ToUInt32(m_Data.IASSpeed);
				}
				catch (Exception ex)
				{
					Init.Airspeed = 0;
				}
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
        /// Recalcul le prédicteur lors de la réception de nouvelles données
        /// </summary>

        private void RefreshData()
        {
            //DateTime Maintenant = DateTime.UtcNow;
            if ((m_ObjectID == 0) || (m_Version != PROTO_VERSION)) return;
            //Si c'est le premier positionnement, on initialise les données avec la position brute
            if (m_Spawned == 5)
            {
                m_AIData.Altitude = m_Data.Altitude;
                m_AIData.Latitude = m_Data.Latitude;
                m_AIData.Longitude = m_Data.Longitude;
                m_AIData.Heading = m_Data.Heading;
                m_AIData.Pitch = m_Data.Pitch;
                m_AIData.Bank = m_Data.Bank;
                m_SC.Freeze_AI(m_ObjectID, m_Data.OnGround);
				m_LastRender = DateTimeEx.UtcNowMilli;
                if (m_Spawned == 5) m_Spawned = 6;
            }
            //Nous avons des données toutes fraiches et nous sommes prêt à calculer l'extrapolation
            if (m_Spawned >= 6)
            {
                double CoefHoriz = 0;
                m_PredictiveTime = m_RemoteRefreshRate*3;
                //Calcul de l'extrapolation
                long Retard =m_LastData - m_Data.TimeStamp; //Calcul du retard absolu de la donnée
                if (Retard < 0) Retard = 0;
                //double Periode = m_RemoteRefreshRate.TotalMilliseconds; //Calcul de la période entre deux données
                if (m_RemoteRefreshRate > 0)
                {
                    // On mémorise la position actuelle de l'extrapolation
                    m_ActualPos.TimeStamp = m_LastRender;
                    m_ActualPos.Altitude = m_AIData.Altitude;
                    m_ActualPos.Latitude = m_AIData.Latitude;
                    m_ActualPos.Longitude = m_AIData.Longitude;
                    m_ActualPos.Heading = m_AIData.Heading;
                    m_ActualPos.Bank = m_AIData.Bank;
                    m_ActualPos.Pitch = m_AIData.Pitch;

                    m_FuturData.TimeStamp = m_LastRender + m_PredictiveTime;
                    CoefHoriz = (double)(m_FuturData.TimeStamp - m_Data.TimeStamp) / (double)m_RemoteRefreshRate;
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
                m_Spawned = 7;
                m_ActualPos.Pitch = m_AIData.Pitch;
                if (m_Data.OnGround != m_OldData.OnGround) m_SC.Freeze_AI(m_ObjectID,m_Data.OnGround);
            }
        }
		/// <summary>
		/// Mise à jour de la position et de l'atitude de l'AI
		/// </summary>
		/// <param name="FrameRate"></param>
		private void Update_AI(long Time, float FPS)
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
					m_AIData.SpoilerPos = m_Data.SpoilerPos;
					m_AIData.ParkingBrakePos = m_Data.ParkingBrakePos;
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
					//Si on a suffisement de donnée (new + old) on utilise l'interpolation
					if (m_Spawned >= 7)
					{
						float FPSAvg = FPS;
						if (m_old_fps != 0)
						{
							FPSAvg = (FPS + m_old_fps) / 2;
						}
						m_old_fps = FPS;
						double Temps = 1000 / FPSAvg;
                        double CoefInterpol = Temps / (m_FuturData.TimeStamp - m_ActualPos.TimeStamp);
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
                    if (m_Data.OnGround)
                    {
                        if ((m_AIData.Altitude > 10) && (m_AISimData.SolAltitude!=0))
					    {
                            // Mise à jour de l'altitude sol de référence
                            m_old_GND_AI = m_AISimData.SolAltitude;
							m_AIData.Altitude = m_AISimData.SolAltitude + m_AIResolution.CG_Height;
                        }
                    }
                    if (m_Spawned >= 7) m_SC.Update_AI(m_ObjectID, DEFINITIONS_ID.AI_MOVE, m_AIData);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                if (m_bSelected)
                {
                    Log.LogMessage("Peer [" + m_CallSign + "] Update_AI Eception occured : " + e.Message, Color.DarkBlue, 2);
                }
#endif
            }
            finally
			{
                m_Mutex.ReleaseMutex();
                m_LastRender = Time;
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
        /// Classe de donnée du PEER à un instant donné
        /// </summary>
        [ProtoContract]
        class AirData
        {
            public AirData()
            {
                TimeStamp = DateTimeEx.UtcNowMilli;
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
				SpoilerPos = 0;
				ParkingBrakePos = 0;
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
				SpoilerPos = Object.SpoilerPos;
				ParkingBrakePos = Object.ParkingBrakePos;
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
				SpoilerPos = Object.SpoilerPos;
				ParkingBrakePos = Object.ParkingBrakePos;
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
            public long TimeStamp;
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
			[ProtoMember(40)]
			public double SpoilerPos;
			[ProtoMember(41)]
			public double ParkingBrakePos;
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