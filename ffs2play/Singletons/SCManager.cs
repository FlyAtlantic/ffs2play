using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
#if P3D
using LockheedMartin.Prepar3D.SimConnect;
#else
using Microsoft.FlightSimulator.SimConnect;
#endif
using System.Runtime.InteropServices;
using System.Threading;

namespace ffs2play
{
	public enum SIM_VERSION
	{
		FSX,
		FSX_STEAM,
		P3D_V2,
		P3D_V3,
        P3D_V4,
		UNKNOWN
	}

    public enum GROUP_ID
    {
        JOYSTICK,
		AI
    }

    /// <summary>
    /// Enumération des évenements du simulateur à traiter
    /// </summary>
    public enum EVENT_ID
    {
		PERIODIQUE,
		PAUSE,
        PARKING_BRAKE,
        SIMSTART,
        SIMSTOP,
        OVERSPEED,
        STALLING,
        CRASH,
        SLEW,
        PUSHBACK,
        ON_GROUND,
		GEAR_HANDLE_POSITION,
		FLAPS_HANDLE_PERCENT,
		FLAPS_HANDLE_INDEX,
		ALTIMETER_SETTING,
		LANDING_LIGHT,
		BEACON_LIGHT,
		STROBE_LIGHT,
		NAV_LIGHT,
		RECOGNITION_LIGHT,
		SMOKE_ENABLE,
		FREEZE_LATLONG,
		FREEZE_ALTITUDE,
		FREEZE_ATTITUDE,
		OBJECT_REMOVED,
		SLEW_ON,
		ADDED_AIRCRAFT,
		REMOVED_AIRCRAFT,
		FRAME,
		TEXT_SCROLL,
		SLIDER = 100,
		XAXIS,
		YAXIS,
		RZAXIS,
		HAT,
		SLIDER2 = 110,
		XAXIS2,
		YAXIS2,
		RZAXIS2,
		HAT2,
		SLIDER3 = 120,
		XAXIS3,
		YAXIS3,
		RZAXIS3,
		HAT3,
		UNUSED = 0xFFFFFFF,
    }

	/// <summary>
	/// Enumération des requêtes
	/// à partir de 100, réservé pour les requetes de création et destruction d'avion
	/// </summary>
    public enum REQUESTS_ID
    {
		PERIODIQUE,
		PAUSE,
		PARKING_BRAKE,
		SIMSTART,
		SIMSTOP,
		OVERSPEED,
		STALLING,
		CRASH,
		SLEW,
		PUSHBACK,
		ON_GROUND,
		GEAR_HANDLE_POSITION,
		FLAPS_HANDLE_PERCENT,
		FLAPS_HANDLE_INDEX,
		ALTIMETER_SETTING,
		LANDING_LIGHT,
		BEACON_LIGHT,
		STROBE_LIGHT,
		NAV_LIGHT,
		RECOGNITION_LIGHT,
		SMOKE_ENABLE,
		SYSTEM,
		JOY_INFO,
		AI_RELEASE,
		AI_CREATE = 0x100000,
		AI_UPDATE = 0x200000,
	}

    public enum DEFINITIONS_ID
    {
        PERIODIQUE,
        PARKING_BRAKE,
        OVERSPEED,
        STALLING,
        CRASH,
        SLEW,
        PUSHBACK,
        ON_GROUND,
		GEAR_HANDLE_POSITION,
		FLAPS_HANDLE_PERCENT,
		FLAPS_HANDLE_INDEX,
		ALTIMETER_SETTING,
		LANDING_LIGHT,
		BEACON_LIGHT,
		STROBE_LIGHT,
		NAV_LIGHT,
		RECOGNITION_LIGHT,
		SMOKE_ENABLE,
		IS_SLEW_ACTIVE,
		AI_MOVE,
		AI_INIT
	}
    /// <summary>
    /// Structure d'importation des données envoyées par
    /// la librairie simconnect
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DonneesAvion
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
		public string Title;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
		public string Type;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
		public string Model;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
		public string Category;
		public double AvionAltitude;
		public double SolAltitude;
		public double Vario;
		public double Direction;
		public int TimeFactor;
		public double Longitude;
		public double Latitude;
		public double GSpeed;
		public double TASSpeed;
		public double FuelQty;
		public double TotalFuelCapacity;
		public double FuelWeightGallon;
		public double AvionPitch;
		public double AvionBank;
		public double AvionG;
		public double AvionPoids;
		public double Realism;
		public double AmbiantWindVelocity;
		public double AmbiantWindDirection;
		public uint AmbiantPrecipState;
		public double SeaLevelPressure;
		public double IASSpeed;
		public double ElevatorPos;
		public double AileronPos;
		public double RudderPos;
		public int Door1Pos;
		public int Door2Pos;
		public int Door3Pos;
		public int Door4Pos;
		public int NumEngine;
		public int StateEng1;
		public int StateEng2;
		public int StateEng3;
		public int StateEng4;
		public int ThrottleEng1;
		public int ThrottleEng2;
		public int ThrottleEng3;
		public int ThrottleEng4;
		public int Squawk;
	}

	/// <summary>
	/// Structure de mise à jour AI
	/// la librairie simconnect
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AIMoveStruct
	{
		public double Latitude;
		public double Longitude;
		public double Altitude;
		public double Pitch;
		public double Bank;
		public double Heading;
		public double ElevatorPos;
		public double AileronPos;
		public double RudderPos;
		public int Door1Pos;
		public int Door2Pos;
		public int Door3Pos;
		public int Door4Pos;
		public int StateEng1;
		public int StateEng2;
		public int StateEng3;
		public int StateEng4;
		public int ThrottleEng1;
		public int ThrottleEng2;
		public int ThrottleEng3;
		public int ThrottleEng4;
		public int Squawk;
		public int GearPos;
		public int FlapsIndex;
		public int LandingLight;
		public int StrobeLight;
		public int BeaconLight;
		public int NavLight;
		public int RecoLight;
		public int Smoke;
	}

	public partial class SCManager : IDisposable
	{
		private TimeSpan m_VariableRate;
		private DateTime m_LastVariable;
		private TimeSpan m_SimRate;
		private DateTime m_LastSim;
		// Pointeur sur la librairie de simconnect
		private SimConnect m_scConnection;

		// Etat de la connexion
		private bool m_bConnected;

		// Identifiant de la session simconnect
		const int WM_USER_SIMCONNECT = 0x402;

#if P3D
		// Nombre de joysticks
		private uint m_iNbJoystick;
#endif

		private Logger Log;

		// Tableau dynamique des AI
		private Dictionary<uint, string> AIProcess;

		private bool m_SimStart;

		public bool SimStart
		{
			get { return m_SimStart; }
		}

		private int m_AppVersionMajor;

		public int AppVersionMajor
		{
			get { return m_AppVersionMajor; }
		}

		private Thread m_Thread;

		private const float m_TimeToScroll = 10.0F;
		private List<string> m_MessageBuffer;
        private string m_Last_Metar;

		/// <summary>
		/// Constructeur du Singleton
		/// </summary>
		private SCManager()
		{
			// Initialisation de l'état de connexion avec le simulateur
			m_bConnected = false;
			m_scConnection = null;
			m_SimStart = false;
			m_VariableRate = TimeSpan.FromMilliseconds(100);
			m_LastVariable = Outils.Now;
			//m_Thread.IsBackground = true;
			AIProcess = new Dictionary<uint, string>();
			Log = Logger.Instance;
			m_MessageBuffer = new List<string>();
			try
			{
				if ((m_btnConnectFS = ffs2play.getControl("btnConnectFS") as Button) != null) m_btnConnectFS.Click += btnConnectFS_Click;
				else throw new System.InvalidOperationException("Le bouton btnConnectFS n'existe pas");
			}
			catch (Exception e)
			{
				Log.LogMessage("Erreur Constructeur SCManager : " + e.Message);
			}
		}

		public static SCManager Instance
		{
			get
			{
				return Nested.instance;
			}
		}

		class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly SCManager instance = new SCManager();
		}

		public new void Dispose()
		{
			closeConnection();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Méthodes de la classe
		/// </summary>

		protected void MainThread ()
		{
			while (m_bConnected)
			{
				m_scConnection.ReceiveMessage();
				Thread.Sleep(1);
			}
		}

        private Button m_btnConnectFS;

        /// <summary>
        /// Gestion de l'évenement de click sur le bouton de
        /// Connexion ou de déconnexion sur le simulateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnectFS_Click(object sender, EventArgs e)
        {
            if (!m_bConnected) openConnection();
            else closeConnection();
        }

        /// <summary>
        /// Retourne la version du simulateur
        /// </summary>
        /// <returns></returns>
        public SIM_VERSION GetVersion()
		{
			if (!m_bConnected) return SIM_VERSION.UNKNOWN;
			switch (m_AppVersionMajor)
			{
				case 2:
					return SIM_VERSION.P3D_V2;
				case 3:
					return SIM_VERSION.P3D_V3;
                case 4:
                    return SIM_VERSION.P3D_V4;
				case 10:
					return SIM_VERSION.FSX;
				default:
					return SIM_VERSION.FSX_STEAM;
			}
		}

		/// <summary>
		/// Retourne l'état de connexion
		/// </summary>
		/// <returns></returns>
		public bool IsConnected()
		{
			return m_bConnected;
		}

		/// <summary>
		/// Connexion au simulateur
		/// </summary>
        public void openConnection()
        {
            if (!m_bConnected)
            {
				try
				{
					//Création d'une instance pour acceder à la librairie simconnect
					m_scConnection = new SimConnect("ffs2play", IntPtr.Zero, WM_USER_SIMCONNECT, null, 0);
					if (m_btnConnectFS != null)
					{
						m_btnConnectFS.ImageIndex = 1;
						// Mémorisation de l'état de connexion
					}
					// Mémorisation de l'état de connexion
					m_bConnected = true;
					// Initialisation des types de donnée à récupérer sur simconnect
					initDataRequest();
					// Démarrage du thread 
					m_Thread = new Thread(MainThread);
					m_Thread.Start();
				}
				catch (COMException e)
				{
					// Si l'initialisation de la librairie simconnect à échoué,
					// Nous le signalons par un message
					Log.LogMessage("La connexion au FS a échoué.");
#if DEBUG
					Log.LogMessage("SCManager: openConnection Exception = " + e.Message, Color.DarkViolet, 1);
#endif
				}
				catch (ThreadStateException e)
				{
					Log.LogMessage("SCManager: ThreadException Exception , " + e.Message,Color.DarkViolet, 1);
				}
            }
        }

		/// <summary>
		/// Méthode de fermeture d'une instance simconnect
		/// </summary>
		public void closeConnection()
		{
			if ((m_scConnection!=null) || (m_bConnected))
			{
				// On change la couleur du bouton en rouge
				m_btnConnectFS.Invoke(new Action(() => {
					m_btnConnectFS.ImageIndex = 0;
				}));
				m_bConnected = false;
				if (m_Thread != null)
				{
					m_Thread.Abort();
					m_Thread.Join();
					m_Thread = null;
				}
				m_scConnection = null;
				Log.LogMessage("La connexion au simulateur à été fermée");
			}
		}

        /// <summary>
        /// Initialisation des données à récupérer du simulateur
        /// </summary>
        private void initDataRequest()
        {
 			// Programmation des callbacks
			// Définition de la méthode à appeller sur un évenement de démarrage du simulateur*/
			m_scConnection.OnRecvOpen += OnRecvOpen;
			// Définition de la méthode à appeller sur un arrêt du simulateur
			m_scConnection.OnRecvQuit += OnRecvQuit;
			// Définition de la méthode à appeller d'une exception
			m_scConnection.OnRecvException += OnRecvException;
			// Définition de la méthode à apperller pour la récéption des données
			m_scConnection.OnRecvSimobjectData += OnRecvSimobjectData;
			m_scConnection.OnRecvEvent += OnRecvEvent;
			m_scConnection.OnRecvSystemState += OnRecvSystemState;
			m_scConnection.OnRecvAssignedObjectId += OnRecvAssignedObjectId;
			m_scConnection.OnRecvEventObjectAddremove += OnRecvEventObjectAddremove;
			m_scConnection.OnRecvEventFrame += OnRecvEventFrame;
            m_scConnection.OnRecvWeatherObservation += OnRecvWeatherObservation;

#if P3D
			m_scConnection.OnRecvJoystickDeviceInfo += new SimConnect.RecvJoystickDeviceInfoEventHandler(OnRecvJoystickDeviceInfo);
#else
			//Création des notification sur le joystick
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.SLIDER, "");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.XAXIS, "");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.YAXIS, "");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.RZAXIS, "");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.HAT, "");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.FREEZE_LATLONG, "FREEZE_LATITUDE_LONGITUDE_SET");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.FREEZE_ALTITUDE, "FREEZE_ALTITUDE_SET");
			m_scConnection.MapClientEventToSimEvent(EVENT_ID.FREEZE_ATTITUDE, "FREEZE_ATTITUDE_SET");


			m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, EVENT_ID.SLIDER, false);
			m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, EVENT_ID.XAXIS, false);
			m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, EVENT_ID.YAXIS, false);
			m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, EVENT_ID.RZAXIS, false);
			m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, EVENT_ID.HAT, false);

			m_scConnection.SetNotificationGroupPriority(GROUP_ID.JOYSTICK, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);

			m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:0:slider", EVENT_ID.SLIDER, 0, EVENT_ID.SLIDER, 1, false);
			m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:0:XAxis", EVENT_ID.XAXIS, 0, EVENT_ID.XAXIS, 1, false);
			m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:0:YAxis", EVENT_ID.YAXIS, 0, EVENT_ID.YAXIS, 1, false);
			m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:0:RzAxis", EVENT_ID.RZAXIS, 0, EVENT_ID.RZAXIS, 1, false);
			m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:0:POV", EVENT_ID.HAT, 0, EVENT_ID.HAT, 1, false);

			m_scConnection.SetInputGroupState(GROUP_ID.JOYSTICK, 1);
#endif
            // Données périodiques
            m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "TITLE", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "ATC TYPE", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "ATC MODEL", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "CATEGORY", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "VERTICAL SPEED", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "SIMULATION RATE", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "FUEL TOTAL QUANTITY WEIGHT", "Pounds", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "FUEL TOTAL CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "FUEL WEIGHT PER GALLON", "Pounds", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "G FORCE", "Gforce", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "TOTAL WEIGHT", "Pounds", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "REALISM", "Number", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AMBIENT WIND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AMBIENT WIND DIRECTION", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AMBIENT PRECIP STATE", "Mask", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "ELEVATOR POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "AILERON POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "RUDDER POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "EXIT OPEN:0", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "EXIT OPEN:1", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "EXIT OPEN:2", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "EXIT OPEN:3", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "NUMBER OF ENGINES", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG COMBUSTION:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG COMBUSTION:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG COMBUSTION:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG COMBUSTION:4", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG THROTTLE LEVER POSITION:1", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG THROTTLE LEVER POSITION:2", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG THROTTLE LEVER POSITION:3", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "GENERAL ENG THROTTLE LEVER POSITION:4", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PERIODIQUE, "TRANSPONDER CODE:1", "BCO16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

			// On associe notre structure à la définition simconnect
			m_scConnection.RegisterDataDefineStruct<DonneesAvion>(DEFINITIONS_ID.PERIODIQUE);

			// Structure pour déplacer un AI
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "ELEVATOR POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "AILERON POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "RUDDER POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "EXIT OPEN:0", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "EXIT OPEN:1", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "EXIT OPEN:2", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "EXIT OPEN:3", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG COMBUSTION:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG COMBUSTION:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG COMBUSTION:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG COMBUSTION:4", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG THROTTLE LEVER POSITION:1", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG THROTTLE LEVER POSITION:2", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG THROTTLE LEVER POSITION:3", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GENERAL ENG THROTTLE LEVER POSITION:4", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "TRANSPONDER CODE:1", "BCO16", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "GEAR HANDLE POSITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "FLAPS HANDLE INDEX", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "LIGHT RECOGNITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.AI_MOVE, "SMOKE ENABLE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			


			// On associe notre structure à la définition simconnect
			m_scConnection.RegisterDataDefineStruct<AIMoveStruct>(DEFINITIONS_ID.AI_MOVE);

			// Données sur changement
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PARKING_BRAKE, "BRAKE PARKING INDICATOR", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.OVERSPEED, "OVERSPEED WARNING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.STALLING, "STALL WARNING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.CRASH, "CRASH SEQUENCE", "enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.SLEW, "IS SLEW ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.PUSHBACK, "PUSHBACK STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.ON_GROUND, "SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.GEAR_HANDLE_POSITION, "GEAR HANDLE POSITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.FLAPS_HANDLE_PERCENT, "FLAPS HANDLE PERCENT", "Percent", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.FLAPS_HANDLE_INDEX, "FLAPS HANDLE INDEX", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.ALTIMETER_SETTING, "KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.LANDING_LIGHT, "LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.STROBE_LIGHT, "LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.BEACON_LIGHT, "LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.NAV_LIGHT, "LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.RECOGNITION_LIGHT, "LIGHT RECOGNITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
			m_scConnection.AddToDataDefinition(DEFINITIONS_ID.SMOKE_ENABLE, "SMOKE ENABLE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

			// Définition de la méthode à apeller pour la récéption des données
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.PERIODIQUE, DEFINITIONS_ID.PERIODIQUE, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.PARKING_BRAKE, DEFINITIONS_ID.PARKING_BRAKE, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.OVERSPEED, DEFINITIONS_ID.OVERSPEED, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.STALLING, DEFINITIONS_ID.STALLING, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.CRASH, DEFINITIONS_ID.CRASH, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.SLEW, DEFINITIONS_ID.SLEW, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.PUSHBACK, DEFINITIONS_ID.PUSHBACK, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.ON_GROUND, DEFINITIONS_ID.ON_GROUND, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.GEAR_HANDLE_POSITION, DEFINITIONS_ID.GEAR_HANDLE_POSITION, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.FLAPS_HANDLE_PERCENT, DEFINITIONS_ID.FLAPS_HANDLE_PERCENT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.FLAPS_HANDLE_INDEX, DEFINITIONS_ID.FLAPS_HANDLE_INDEX, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.ALTIMETER_SETTING, DEFINITIONS_ID.ALTIMETER_SETTING, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.LANDING_LIGHT, DEFINITIONS_ID.LANDING_LIGHT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.STROBE_LIGHT, DEFINITIONS_ID.STROBE_LIGHT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.BEACON_LIGHT, DEFINITIONS_ID.BEACON_LIGHT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.NAV_LIGHT, DEFINITIONS_ID.NAV_LIGHT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.RECOGNITION_LIGHT, DEFINITIONS_ID.RECOGNITION_LIGHT, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
			m_scConnection.RequestDataOnSimObject(REQUESTS_ID.SMOKE_ENABLE, DEFINITIONS_ID.SMOKE_ENABLE, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

			// Subscribe to system events
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.PAUSE, "Pause");
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.SIMSTART, "simstart");
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.SIMSTOP, "simstop");
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.FRAME, "Frame");
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.ADDED_AIRCRAFT, "ObjectAdded");
			m_scConnection.SubscribeToSystemEvent(EVENT_ID.REMOVED_AIRCRAFT, "ObjectRemoved");
			m_scConnection.RequestSystemState(REQUESTS_ID.SYSTEM, "sim");
#if P3D
			m_scConnection.RequestJoystickDeviceInfo(REQUESTS_ID.JOY_INFO);
#endif
		}

        private void OnRecvWeatherObservation(SimConnect sender, SIMCONNECT_RECV_WEATHER_OBSERVATION data)
        {
#if DEBUG
            Log.LogMessage("SCManager: Reçu un Metar = " + data.szMetar, Color.Blue, 2);
#endif
        }
#if P3D
		private void OnRecvJoystickDeviceInfo(SimConnect sender, SIMCONNECT_RECV_JOYSTICK_DEVICE_INFO info)
		{
			m_iNbJoystick = info.dwArraySize;
#if DEBUG
			Log.LogMessage("SCManager: Nombre de joystick = " + m_iNbJoystick, Color.Blue, 1);
#endif
			if (m_iNbJoystick < 1) return;
			foreach (SIMCONNECT_DATA_JOYSTICK_DEVICE_INFO joy in info.rgData)
			{
				uint num = joy.dwNumber;
				//Création des notification sur le joystick
#if DEBUG
				Log.LogMessage("SCManager:Joystick n°" + num.ToString() + " , Nom = " + joy.szName, Color.Blue, 1);
#endif
				m_scConnection.MapClientEventToSimEvent((EVENT_ID)((uint)EVENT_ID.SLIDER + (num * 10)), "");
				m_scConnection.MapClientEventToSimEvent((EVENT_ID)((uint)EVENT_ID.XAXIS + (num * 10)), "");
				m_scConnection.MapClientEventToSimEvent((EVENT_ID)((uint)EVENT_ID.YAXIS + (num * 10)), "");
				m_scConnection.MapClientEventToSimEvent((EVENT_ID)((uint)EVENT_ID.RZAXIS + (num * 10)), "");
				m_scConnection.MapClientEventToSimEvent((EVENT_ID)((uint)EVENT_ID.HAT + (num * 10)), "");
				m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:" + num.ToString() + ":slider", (EVENT_ID)((uint)EVENT_ID.SLIDER + (num * 10)), 0, (EVENT_ID)((uint)EVENT_ID.SLIDER + (num * 10)), 1, false);
				m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:" + num.ToString() + ":XAxis", (EVENT_ID)((uint)EVENT_ID.XAXIS + (num * 10)), 0, (EVENT_ID)((uint)EVENT_ID.XAXIS + (num * 10)), 1, false);
				m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:" + num.ToString() + ":YAxis", (EVENT_ID)((uint)EVENT_ID.YAXIS + (num * 10)), 0, (EVENT_ID)((uint)EVENT_ID.YAXIS + (num * 10)), 1, false);
				m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:" + num.ToString() + ":RzAxis", (EVENT_ID)((uint)EVENT_ID.RZAXIS + (num * 10)), 0, (EVENT_ID)((uint)EVENT_ID.RZAXIS + (num * 10)), 1, false);
				m_scConnection.MapInputEventToClientEvent(GROUP_ID.JOYSTICK, "joystick:" + num.ToString() + ":POV", (EVENT_ID)((uint)EVENT_ID.HAT + (num * 10)), 0, (EVENT_ID)((uint)EVENT_ID.HAT + (num * 10)), 1, false);
				m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, (EVENT_ID)((uint)EVENT_ID.SLIDER + (num * 10)), false);
				m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, (EVENT_ID)((uint)EVENT_ID.XAXIS + (num * 10)), false);
				m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, (EVENT_ID)((uint)EVENT_ID.YAXIS + (num * 10)), false);
				m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, (EVENT_ID)((uint)EVENT_ID.RZAXIS + (num * 10)), false);
				m_scConnection.AddClientEventToNotificationGroup(GROUP_ID.JOYSTICK, (EVENT_ID)((uint)EVENT_ID.HAT + (num * 10)), false);
			}

			m_scConnection.SetNotificationGroupPriority(GROUP_ID.JOYSTICK, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);
			m_scConnection.SetInputGroupState(GROUP_ID.JOYSTICK, 1);
		}
#endif
        private void OnRecvEventObjectAddremove(SimConnect sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data)
		{
			switch ((EVENT_ID)data.uEventID)
			{
				case EVENT_ID.ADDED_AIRCRAFT:
#if DEBUG
					Log.LogMessage("SCManager: Objet crée de type " + data.eObjType.ToString() + " avec le n° : " + data.dwData, Color.Blue,1);
#endif
					break;
				case EVENT_ID.REMOVED_AIRCRAFT:
#if DEBUG
					Log.LogMessage("SCManager: Objet supprimé de type " + data.eObjType.ToString() + " avec le n° : " + data.dwData, Color.Blue, 1);
#endif
					FireSCAIRemoved(new SCManagerEventObjectDeleted(data.dwData));
					break;
			}
		}

		/// <summary>
		/// Evénement affichage d'une Frame sur le simulateur
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void OnRecvEventFrame(SimConnect sender, SIMCONNECT_RECV_EVENT_FRAME data)
		{
			FireSCReceiveEventFrame(new SCManagerEventFrame(data.fFrameRate, data.fSimSpeed));
		}

		/// <summary>
		/// Evénement reçu la confirmation de la création d'un ObjectAI
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data)
		{
			string Tail;
			if( AIProcess.TryGetValue(data.dwRequestID,out Tail))
			{
#if DEBUG
				Log.LogMessage("SCManager: OnRecvAssignedObjectId Request = " + data.dwRequestID.ToString() + " , Name = " + Tail + " , Object_ID = " + data.dwObjectID.ToString(), Color.DarkViolet, 2);
#endif
				m_scConnection.AIReleaseControl(data.dwObjectID, REQUESTS_ID.AI_RELEASE);
				m_scConnection.TransmitClientEvent(data.dwObjectID, EVENT_ID.FREEZE_LATLONG, 1, (GROUP_ID)SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
				m_scConnection.TransmitClientEvent(data.dwObjectID, EVENT_ID.FREEZE_ALTITUDE, 1, (GROUP_ID)SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
				m_scConnection.TransmitClientEvent(data.dwObjectID, EVENT_ID.FREEZE_ATTITUDE, 1, (GROUP_ID)SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);

				FireSCAICreated(new SCManagerEventAICreated(Tail, data.dwObjectID));
				AIProcess.Remove(data.dwRequestID);
				//if (GetVersion() < SIM_VERSION.P3D_V2)
					m_scConnection.RequestDataOnSimObject((REQUESTS_ID)((uint)REQUESTS_ID.AI_UPDATE + data.dwObjectID), DEFINITIONS_ID.AI_MOVE, data.dwObjectID, SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

			}
			else
			{
				Log.LogMessage("SCManager: OnRecvAssignedObjectId erreur, requete AI inconnue = " + data.dwRequestID.ToString() , Color.DarkViolet);
			}
		}

		/// <summary>
		/// Réception des Events d'état système de SimConnect
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void OnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
		{
#if DEBUG
			Log.LogMessage("SCManager: OnRecvSystemState = " + data.dwInteger.ToString(), Color.DarkViolet, 2);
#endif
			SCManagerEventSCEvent evt = new SCManagerEventSCEvent();
			if (data.dwInteger == 1)
			{
				evt.Evt_Id = EVENT_ID.SIMSTART;
				m_SimStart = true;
			}
			else
			{
				evt.Evt_Id = EVENT_ID.SIMSTOP;
				m_SimStart = false;
			}
			FireSCReceiveEvent(evt);
		}

		/// <summary>
		/// Réception des Events de donnée des objets
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
			switch ((REQUESTS_ID)data.dwRequestID)
			{
				case REQUESTS_ID.PERIODIQUE:
                    m_LastSim = Outils.Now;
                    m_SimRate = m_LastSim - m_LastSim;
					if (m_LastSim >= (m_LastVariable+m_VariableRate))
					{
						m_LastVariable = m_LastSim;
						FireSCReceiveVariable(new SCManagerEventSCVariable((DonneesAvion)data.dwData[0]));
					}
					break;
				default:

					if  ((REQUESTS_ID)data.dwRequestID >= REQUESTS_ID.AI_UPDATE)
						FireSCReceiveAIUpdate(new SCManagerEventAIUpdate((AIMoveStruct)data.dwData[0], data.dwObjectID));
					else FireSCReceiveEvent(new SCManagerEventSCEvent((EVENT_ID)data.dwRequestID, (uint)data.dwData[0]));
					break;
			}
		}

		/// <summary>
		/// Réception des Events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
        private void OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
#if DEBUG
			Log.LogMessage("SCManager: OnRecvEvent = " + data.uEventID.ToString(), Color.DarkViolet, 2);
#endif
			if (data.uEventID == (uint)EVENT_ID.TEXT_SCROLL)
			{
#if DEBUG
				Log.LogMessage("SCManager: Text Scroll Event = " + ((SIMCONNECT_TEXT_RESULT) data.dwData).ToString() , Color.DarkViolet, 1);
#endif
				if ((SIMCONNECT_TEXT_RESULT)data.dwData == SIMCONNECT_TEXT_RESULT.TIMEOUT )
				{
					m_MessageBuffer.RemoveAt(0);
					if (m_MessageBuffer.Count > 0) SendText(m_MessageBuffer[0]);
				}
				return;
			}
			switch ((EVENT_ID)data.uEventID)
			{
				case EVENT_ID.SIMSTART:
					m_SimStart = true;
					break;
				case EVENT_ID.SIMSTOP:
					m_SimStart = false;
					break;
			}
			FireSCReceiveEvent(new SCManagerEventSCEvent((EVENT_ID)data.uEventID, (uint)data.dwData));
        }

        /// <summary>
        /// Gestion d'une exception simconnect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
#if DEBUG
			Log.LogMessage("SCManager: Erreur SimConnect: " + ((SIMCONNECT_EXCEPTION)(data.dwException)).ToString() +
				" , Send ID = " + data.dwSendID.ToString() +
				" , Index = " + data.dwIndex.ToString(), Color.DarkViolet,2);

#endif
			//closeConnection();
        }

        /// <summary>
        /// Gestion d'un démarage du simulateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
			Log.LogMessage("La connexion au simulateur a réussie");
			m_AppVersionMajor = (int)data.dwApplicationVersionMajor;
			SCManagerEventOpen evt = new SCManagerEventOpen();
			evt.ApplicationVersionMajor = data.dwApplicationVersionMajor;
			evt.ApplicationVersionMinor = data.dwApplicationVersionMinor;
			evt.ApplicationBuildMajor = data.dwApplicationBuildMajor;
			evt.ApplicationBuildMinor = data.dwApplicationBuildMajor;
			evt.SimConnectVersionMajor = data.dwSimConnectVersionMajor;
			evt.SimConnectVersionMinor = data.dwSimConnectVersionMinor;
			evt.SimConnectBuildMajor = data.dwSimConnectBuildMajor;
			evt.SimConnectBuildMinor = data.dwSimConnectBuildMinor;
			FireSCReceiveOpen(evt);
#if DEBUG
			Log.LogMessage("SCManager: Version du simulateur = " + m_AppVersionMajor.ToString() , Color.DarkBlue, 1);
#endif
		}

		/// <summary>
		/// Gestion d'un arrêt du simulateur
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
			Log.LogMessage("Le simulateur à quitté");
            closeConnection();
        }

		//private void OnRecvScrollingTimer()

		/// <summary>
		/// Affiche un message défilant sur le simulateur
		/// </summary>
		/// <param name="Message"></param>
        public void SendScrollingText(string Message)
        {
			if (m_bConnected)
			{
				m_MessageBuffer.Add(Message);
				if (m_MessageBuffer.Count < 2) SendText(Message);
			}
        }

		private void SendText(string Message)
		{
			m_scConnection.Text(SIMCONNECT_TEXT_TYPE.SCROLL_WHITE, m_TimeToScroll, EVENT_ID.TEXT_SCROLL, Message);
		}

		/// <summary>
		/// Création d'un avion AI
		/// </summary>
		/// <param name="Aircraft"></param>
		/// <param name="Tail"></param>
		/// <param name="data"></param>
		public bool Create_AI (string Aircraft, string Tail, ref SIMCONNECT_DATA_INITPOSITION data, bool NonFlyable=false)
		{
			if (m_bConnected)
			{
#if DEBUG
				// Si l'initialisation a échoué, on le signal
				Log.LogMessage("SCManager: Create_AI Title = " + Aircraft + " , Tail = " + Tail, Color.DarkViolet, 2);
#endif
				uint index = (uint)AIProcess.Count + (uint)REQUESTS_ID.AI_CREATE;
				AIProcess.Add(index, Tail);
				if (!NonFlyable) m_scConnection.AICreateNonATCAircraft(Aircraft, Tail, data, (REQUESTS_ID)index);
				else m_scConnection.AICreateSimulatedObject(Aircraft, data, (REQUESTS_ID)index);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Mise à jour de la position d'un AI
		/// </summary>
		/// <param name="Object_ID"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Update_AI (uint Object_ID, DEFINITIONS_ID def, object data)
		{
			if (m_bConnected && (Object_ID>0))
			{
#if DEBUG
				// Si l'initialisation a échoué, on le signal
				Log.LogMessage("SCManager: Update_AI", Color.DarkViolet, 2);
#endif
				m_scConnection.SetDataOnSimObject(def, Object_ID, SIMCONNECT_DATA_SET_FLAG.DEFAULT,data );
				return true;
			}
			return false;
		}

		/// <summary>
		/// Envoi d'un event vers un AI
		/// </summary>
		/// <param name="Object_ID"></param>
		/// <param name="evt"></param>
		/// <param name="Data"></param>
		/// <returns></returns>
		public bool EventToAI(uint Object_ID, EVENT_ID evt, uint Data)
		{
			if (m_bConnected)
			{
				m_scConnection.TransmitClientEvent(Object_ID, evt, Data, GROUP_ID.AI, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Suppression d'un AI
		/// </summary>
		/// <param name="ObjectID"></param>
		/// <param name="Nom"></param>
		/// <returns></returns>
		public bool Remove_AI (ref uint ObjectID)
		{
			if (m_bConnected)
			{
				try
				{
					//uint index = (uint)AIProcess.Count + (uint)REQUESTS_ID.AI_REMOVE;
					m_scConnection.AIRemoveObject(ObjectID, (REQUESTS_ID)((uint)REQUESTS_ID.AI_UPDATE + ObjectID));
					ObjectID = 0;
				}
				catch (Exception e)
				{
					// Si l'initialisation a échoué, on le signal
					Log.LogMessage("SCManager: DeleteAircraft :" + e.Message, Color.DarkViolet);
				}
				return true;
			}
			return false;
		}

        public void SendWeatherObservation(string Metar)
        {
            if (m_bConnected)
            {
                if (Metar != m_Last_Metar)
                {
                    m_Last_Metar = Metar;
                    m_scConnection.WeatherSetModeCustom();
                    m_scConnection.WeatherSetModeGlobal();
                    Metar = Metar.Replace(Metar.Substring(0, 4), "GLOB");
#if DEBUG
                    Log.LogMessage("SCManager : Send metar = " + Metar, Color.Blue, 2);
#endif
                    m_scConnection.WeatherSetObservation(10, Metar);
                }
            }
        }

		/// <summary>
		/// Déclenche une notification en internet d'un événement reçu du simulateur
		/// </summary>
		/// <param name="e"></param>
        protected virtual void FireSCReceiveEvent(SCManagerEventSCEvent e)
        {
			OnSCReceiveEvent?.Invoke(this, e);
		}

		/// <summary>
		/// Déclenche une notification en interne d'une mise à jour des variable reçu du simulateur
		/// </summary>
		/// <param name="e"></param>
        protected virtual void FireSCReceiveVariable(SCManagerEventSCVariable e)
        {
			OnSCReceiveVariable?.Invoke(this, e);
		}

		protected virtual void FireSCReceiveAIUpdate(SCManagerEventAIUpdate e)
		{
			OnSCReceiveAIUpdate?.Invoke(this, e);
		}

		protected virtual void FireSCAICreated(SCManagerEventAICreated e)
		{
			OnAICreated?.Invoke(this, e);
		}

		protected virtual void FireSCAIRemoved(SCManagerEventObjectDeleted e)
		{
			OnAIRemoved?.Invoke(this, e);
		}

		protected virtual void FireSCReceiveEventFrame(SCManagerEventFrame e)
		{
			OnSCReceiveEventFrame?.Invoke(this, e);
		}

		protected virtual void FireSCReceiveOpen(SCManagerEventOpen e)
		{
			OnSCReceiveOpen?.Invoke(this, e);
		}

		public event EventHandler<SCManagerEventSCEvent> OnSCReceiveEvent;

        public event EventHandler<SCManagerEventSCVariable> OnSCReceiveVariable;

		public event EventHandler<SCManagerEventAIUpdate> OnSCReceiveAIUpdate;

		public event EventHandler<SCManagerEventAICreated> OnAICreated;

		public event EventHandler<SCManagerEventObjectDeleted> OnAIRemoved;

		public event EventHandler<SCManagerEventFrame> OnSCReceiveEventFrame;

		public event EventHandler<SCManagerEventOpen> OnSCReceiveOpen;
	}

	public class SCManagerEventOpen : EventArgs
	{
		public uint ApplicationVersionMajor;
		public uint ApplicationVersionMinor;
		public uint ApplicationBuildMajor;
		public uint ApplicationBuildMinor;
		public uint SimConnectVersionMajor;
		public uint SimConnectVersionMinor;
		public uint SimConnectBuildMajor;
		public uint SimConnectBuildMinor;
	}

	public class SCManagerEventFrame : EventArgs
	{
		public SCManagerEventFrame(float pFrameRate = 0, float pSimSpeed=0)
		{
			FrameRate = pFrameRate;
			SimSpeed = pSimSpeed;
			Time = Outils.Now;
		}
		public float FrameRate;
		public float SimSpeed;
		public DateTime Time;
	}

	public class SCManagerEventAICreated : EventArgs
	{
		public SCManagerEventAICreated() { }
		public SCManagerEventAICreated (string pTail, uint pObject_ID)
		{
			Tail = pTail;
			Object_ID = pObject_ID;
		}
		public string Tail;
		public uint Object_ID;
	}

	public class SCManagerEventObjectDeleted : EventArgs
	{
		public SCManagerEventObjectDeleted() { }
		public SCManagerEventObjectDeleted (uint pObject_ID)
		{
			Object_ID = pObject_ID;
		}
		public uint Object_ID;
	}

    public class SCManagerEventSCEvent : EventArgs
    {
		public SCManagerEventSCEvent()
		{
			Time = Outils.Now;
		}
		public SCManagerEventSCEvent (EVENT_ID pEvt_ID, uint pData)
		{
			Evt_Id = pEvt_ID;
			Data = (int)pData;
			Time = Outils.Now;
		}
		public EVENT_ID Evt_Id;
		public int Data;
		public DateTime Time;
	}

    public class SCManagerEventSCVariable : EventArgs
    {
		public SCManagerEventSCVariable (DonneesAvion pData)
		{
			Data = pData;
			Time = Outils.Now;
		}
		public DonneesAvion Data;
		public DateTime Time;
    }

	public class SCManagerEventAIUpdate : EventArgs
	{
		public SCManagerEventAIUpdate()
		{
			Time = Outils.Now;
		}
		public SCManagerEventAIUpdate(AIMoveStruct pData, uint pObjectID)
		{
			Data = pData;
			ObjectID = pObjectID;
			Time = Outils.Now;
		}
		public AIMoveStruct Data;
		public uint ObjectID;
		public DateTime Time;
	}
}
