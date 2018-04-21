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
 * AnalyseurManager.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ffs2play
{
	public partial class AnalyseurManager
	{
		public List<AircraftState> Enregistrement;
		private AircraftState Dernier;
		public AircraftState ADernier;
		private Logger Log;
		private SCManager SCM;
		private bool m_FirstScan;
		private AnalyseurManager()
		{
			Log = Logger.Instance;
			try
			{
                // Récupération du pointeur sur les variables du simulateur
                if ((m_lvDonneesFS = ffs2play.getControl("lvDonneesFS") as ListViewEx) == null)
                    throw new InvalidOperationException("La liste m_lvDonneesFS n'existe pas");
            }
			catch (Exception e)
			{
				Log.LogMessage("Erreur Constructeur Analyseur : " + e.Message,Color.DarkViolet,0);
			}
            SCM = SCManager.Instance;
            SCM.OnSCReceiveEvent += OnSCReceiveEvent;
            SCM.OnSCReceiveVariable += OnSCReceiveVariable;
			Dernier = new AircraftState();
			ADernier = new AircraftState(Dernier);
			m_FirstScan = true;

		}

        public static AnalyseurManager Instance
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

			internal static readonly AnalyseurManager instance = new AnalyseurManager();
		}

        private void OnSCReceiveVariable(object sender, SCManagerEventSCVariable e)
        {
            if (m_lvDonneesFS != null)
            {
                UdpateVar(e.Data,e.Time);
                RempliTableau();
            }
        }

        private void OnSCReceiveEvent(object sender, SCManagerEventSCEvent e)
        {
			if (e.Evt_Id == EVENT_ID.PERIODIQUE) return;
			if ((uint)e.Evt_Id < 100)
			{
				switch (e.Evt_Id)
				{
					case EVENT_ID.PARKING_BRAKE:
#if DEBUG
						if (e.Data > 0) Log.LogMessage("AnalyseManager: Le frein de parking est activé", Color.DarkViolet, 1);
						else Log.LogMessage("AnalyseManager: Le frein de parking est désactivé", Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.PARKING_BRAKE, e.Data);
						break;

					case EVENT_ID.OVERSPEED:
#if DEBUG
						if (e.Data > 0) Log.LogMessage("AnalyseManager: L'avion est en survitesse", Color.DarkViolet, 1);
						else Log.LogMessage("AnalyseManager: L'avion n'est plus en survitesse", Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.OVERSPEED, e.Data);
						break;

					case EVENT_ID.STALLING:
#if DEBUG
						if (e.Data > 0) Log.LogMessage("AnalyseManager: L'avion est en décrochage", Color.DarkViolet, 1);
						else Log.LogMessage("AnalyseManager: L'avion n'est plus en décrochage", Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.STALLING, e.Data);
						break;

					case EVENT_ID.CRASH:
#if DEBUG
						switch (e.Data)
						{
							case 11:
								Log.LogMessage("AnalyseManager: L'avion vient de se crasher", Color.DarkViolet, 1);
								break;
							default:
								break;
						}
#endif
						UpdateEvent(EVENT_ID.CRASH, e.Data);
						break;

					case EVENT_ID.SLEW:
#if DEBUG
						if (e.Data > 0) Log.LogMessage("AnalyseManager: La transposition est activée", Color.DarkViolet, 1);
						else Log.LogMessage("AnalyseManager: La transposition est désactivée", Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.SLEW, e.Data);
						break;

					case EVENT_ID.PUSHBACK:
#if DEBUG
						Log.LogMessage("AnalyseManager: Repoussage = " + e.Data.ToString(), Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.PUSHBACK, e.Data);
						break;

					case EVENT_ID.ON_GROUND:
						if ((e.Data >= 0) && (e.Data <= 1))
						{
#if DEBUG
							if (e.Data == 1) Log.LogMessage("AnalyseManager: L'avion est au sol.", Color.DarkViolet, 1);
							else Log.LogMessage("AnalyseManager: L'avion a décollé.", Color.DarkViolet, 1);
#endif
							UpdateEvent(EVENT_ID.ON_GROUND, e.Data);
						}
						break;

					case EVENT_ID.SIMSTART:
						UpdateEvent(EVENT_ID.SIMSTART, 1);
#if DEBUG
						Log.LogMessage("AnalyseManager: SIMSTART Event", Color.DarkViolet, 1);
#endif
						break;

					case EVENT_ID.SIMSTOP:
						UpdateEvent(EVENT_ID.SIMSTART, 0);
#if DEBUG
						Log.LogMessage("AnalyseManager: SIMSTOP Event", Color.DarkViolet, 1);
#endif
						break;

					case EVENT_ID.PAUSE:
						if (e.Data == 1)
						{
#if DEBUG
							Log.LogMessage("AnalyseManager: La pause est activée", Color.DarkViolet, 1);
#endif
							UpdateEvent(EVENT_ID.PAUSE, 1);
						}
						if (e.Data == 0)
						{
#if DEBUG
							Log.LogMessage("AnalyseManager: La pause est désactivée", Color.DarkViolet, 1);
#endif
							UpdateEvent(EVENT_ID.PAUSE, 0);
						}
						break;
					case EVENT_ID.FLAPS_HANDLE_PERCENT:
#if DEBUG
						Log.LogMessage("AnalyseManager: Position des volets changés : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.FLAPS_HANDLE_INDEX:
						UpdateEvent(EVENT_ID.FLAPS_HANDLE_INDEX, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Levier des volets changés : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.GEAR_HANDLE_POSITION:
						UpdateEvent(EVENT_ID.GEAR_HANDLE_POSITION, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Position du train changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.ALTIMETER_SETTING:
#if DEBUG
						Log.LogMessage("AnalyseManager: Réglage Altimètre changé : " + e.Data, Color.DarkViolet, 1);
#endif
						UpdateEvent(EVENT_ID.ALTIMETER_SETTING, e.Data);
						break;
					case EVENT_ID.LANDING_LIGHT:
						UpdateEvent(EVENT_ID.LANDING_LIGHT, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat feux d'attérissage changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.STROBE_LIGHT:
						UpdateEvent(EVENT_ID.STROBE_LIGHT, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat du feux à éclat changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.BEACON_LIGHT:
						UpdateEvent(EVENT_ID.BEACON_LIGHT, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat du feux rouge changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.NAV_LIGHT:
						UpdateEvent(EVENT_ID.NAV_LIGHT, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat du feux de navigation changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.RECOGNITION_LIGHT:
						UpdateEvent(EVENT_ID.RECOGNITION_LIGHT, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat du feux de reconnaissance changé : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					case EVENT_ID.SMOKE_ENABLE:
						UpdateEvent(EVENT_ID.SMOKE_ENABLE, e.Data);
#if DEBUG
						Log.LogMessage("AnalyseManager: Etat de la fumée changée : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
					default:
#if DEBUG
						Log.LogMessage("AnalyseManager: Recu un évenement non géré ID=" + (uint)e.Evt_Id + " : " + ((EVENT_ID)(e.Evt_Id)).ToString() + " : " + e.Data, Color.DarkViolet, 1);
#endif
						break;
				}
			}
        }

		public void UdpateVar(DonneesAvion Donnees,long Time)
		{
			Dernier.TimeStamp = Time;
			Dernier.Title = Donnees.Title;
			Dernier.Model = Donnees.Model;
			Dernier.Type = Donnees.Type;
			Dernier.Category = Donnees.Category;
			Dernier.Altitude = Donnees.AvionAltitude;
			Dernier.AltitudeSol = Donnees.SolAltitude;
			Dernier.PoidsAvion = Donnees.AvionPoids;
			Dernier.Heading = Donnees.Direction;
			Dernier.GSpeed = Donnees.GSpeed;
			Dernier.TASSpeed = Donnees.TASSpeed;
			Dernier.IASSpeed = Donnees.IASSpeed;
			Dernier.Latitude = Donnees.Latitude;
			Dernier.Longitude = Donnees.Longitude;
			Dernier.TimeFactor = Donnees.TimeFactor;
			Dernier.Vario = Donnees.Vario*60;
			Dernier.Fuel = Donnees.FuelQty;
			Dernier.TotalFuelCapacity = Donnees.FuelWeightGallon * Donnees.TotalFuelCapacity;
			Dernier.GForce = Donnees.AvionG;
			Dernier.Pitch = Donnees.AvionPitch;
			Dernier.Bank = Donnees.AvionBank;
			Dernier.AmbiantWindVelocity = Donnees.AmbiantWindVelocity;
			Dernier.AmbiantWindDirection = Donnees.AmbiantWindDirection;
			Dernier.SeaLevelPressure = Donnees.SeaLevelPressure;
			Dernier.Squawk = Donnees.Squawk;
			Dernier.Realism = (int)(Donnees.Realism*100);
			Dernier.AmbiantPrecipState = Donnees.AmbiantPrecipState;
			Dernier.ElevatorPos = Donnees.ElevatorPos;
			Dernier.AileronPos = Donnees.AileronPos;
			Dernier.RudderPos = Donnees.RudderPos;
			Dernier.SpoilerPos = Donnees.SpoilerPos;
			Dernier.ParkingBrakePos = Donnees.ParkingBrakePos;
			Dernier.Door1Pos = Donnees.Door1Pos;
			Dernier.Door2Pos = Donnees.Door2Pos;
			Dernier.Door3Pos = Donnees.Door3Pos;
			Dernier.Door4Pos = Donnees.Door4Pos;
			Dernier.NumEngine = Donnees.NumEngine;
			Dernier.ThrottleEng1 = Donnees.ThrottleEng1;
			Dernier.ThrottleEng2 = Donnees.ThrottleEng2;
			Dernier.ThrottleEng3 = Donnees.ThrottleEng3;
			Dernier.ThrottleEng4 = Donnees.ThrottleEng4;
			Dernier.StateEng1 = Convert.ToBoolean(Donnees.StateEng1);
			Dernier.StateEng2 = Convert.ToBoolean(Donnees.StateEng2);
			Dernier.StateEng3 = Convert.ToBoolean(Donnees.StateEng3);
			Dernier.StateEng4 = Convert.ToBoolean(Donnees.StateEng4);
			if (m_FirstScan)
			{
				ADernier.Clone(Dernier);
				m_FirstScan = false;
			}
			FireStateChange(Dernier);
			ADernier.Clone(Dernier);
		}

		private void UpdateEvent(EVENT_ID Event, int Valeur)
		{
			switch (Event)
			{
				case EVENT_ID.PARKING_BRAKE:
					Dernier.ParkingBrake = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.PAUSE:
					Dernier.Pause = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.OVERSPEED:
					Dernier.OverSpeed = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.STALLING:
					Dernier.Stalling = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.CRASH:
					if (Valeur > 0)
					{
						Dernier.Crash = true;
					}
					else Dernier.Crash = false;
					break;
				case EVENT_ID.SLEW:
					Dernier.Slew = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.PUSHBACK:
					Dernier.Pushback = (Valeur <3);
					break;
				case EVENT_ID.SIMSTART:
					Dernier.RunState = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.ON_GROUND:
					Dernier.OnGround = Convert.ToBoolean(Valeur);
					if (Dernier.OnGround)
					{
						if (Dernier.Vario < -800)
						{
							Dernier.Crash = true;
						}
					}
					break;
				case EVENT_ID.ALTIMETER_SETTING:
					Dernier.AltimeterSetting = Valeur;
					break;
				case EVENT_ID.FLAPS_HANDLE_INDEX:
					Dernier.FlapsIndex = Valeur;
					break;
				case EVENT_ID.GEAR_HANDLE_POSITION:
					Dernier.GearPos = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.LANDING_LIGHT:
					Dernier.LandingLight = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.BEACON_LIGHT:
					Dernier.BeaconLight = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.STROBE_LIGHT:
					Dernier.StrobeLight = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.NAV_LIGHT:
					Dernier.NavLight = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.RECOGNITION_LIGHT:
					Dernier.RecoLight = Convert.ToBoolean(Valeur);
					break;
				case EVENT_ID.SMOKE_ENABLE:
					Dernier.Smoke = Convert.ToBoolean(Valeur);
					break;
				default:
					break;
			}
		}

        public AircraftState GetLastState()
        {
            if (Dernier != null)
            {
                return Dernier;
            }
            return null;
        }

		protected virtual void FireStateChange(AircraftState State)
		{
			AnalyseurStateEvent e = new AnalyseurStateEvent();
			e.Data = State;
			OnStateChange?.Invoke(this, e);
		}

		public event EventHandler<AnalyseurStateEvent> OnStateChange;
	}

public class AnalyseurStateEvent : EventArgs
{
		public AircraftState Data;
}

	/// <summary>
	/// Classe de donnée de l'appareil à un instant donné
	/// </summary>
	[Serializable()]
	public class AircraftState
	{
		public AircraftState()
		{
			TimeStamp = DateTimeEx.UtcNowMilli;
			Title = "";
			Model = "";
			Type = "";
			Category = "";
			Altitude = 0;
			AltitudeSol = 0;
            PoidsAvion = 0;
            Vario = 0;
			Heading = 0;
			TimeFactor = 1;
			Longitude = 0;
			Latitude = 0;
			GSpeed = 0;
			TASSpeed = 0;
			IASSpeed = 0;
			Fuel = 0;
			TotalFuelCapacity = 0;
			GForce = 0;
			Pitch = 0;
			Bank = 0;
			AmbiantWindVelocity = 0;
			AmbiantWindDirection = 0;
			AltimeterSetting = 0;
			SeaLevelPressure = 0;
			XAxis = 0;
            YAxis = 0;
            Slider = 0;
            RZAxis = 0;
            HatAxis = 0;
			Squawk=0;
			Realism = 0;
			AmbiantPrecipState = 0;
            //Etat
            ParkingBrake = false;
			Pause = false;
			Crash = false;
			RunState = true;
			Pushback = false;
			OverSpeed = false;
			Stalling = false;
			Slew = false;
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
			NumEngine = 0;
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
		public AircraftState(AircraftState Object)
		{
			Clone(Object);
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
            PoidsAvion = Object.PoidsAvion;
            Vario = Object.Vario;
			Heading = Object.Heading;
			TimeFactor = Object.TimeFactor;
			Longitude = Object.Longitude;
			Latitude = Object.Latitude;
			GSpeed = Object.GSpeed;
			TASSpeed = Object.TASSpeed;
			IASSpeed = Object.IASSpeed;
			Fuel = Object.Fuel;
			TotalFuelCapacity = Object.TotalFuelCapacity;
			GForce = Object.GForce;
			Pitch = Object.Pitch;
			Bank = Object.Bank;
			AmbiantWindVelocity = Object.AmbiantWindDirection;
			AmbiantWindDirection = Object.AmbiantWindDirection;
			AltimeterSetting = Object.AltimeterSetting;
			SeaLevelPressure = Object.SeaLevelPressure;
            XAxis = Object.XAxis;
            YAxis = Object.YAxis;
            Slider = Object.Slider;
            RZAxis = Object.RZAxis;
            HatAxis = Object.HatAxis;
			Squawk = Object.Squawk;
			Realism = Object.Realism;
			AmbiantPrecipState = Object.AmbiantPrecipState;
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
			ParkingBrake = Object.ParkingBrake;
			Pause = Object.Pause;
			Crash = Object.Crash;
			RunState = Object.RunState;
			Pushback = Object.Pushback;
			OverSpeed = Object.OverSpeed;
			Stalling = Object.Stalling;
			Slew = Object.Slew;
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
		public long TimeStamp;
		public string Title;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
		public string Model;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
		public string Type;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
		public string Category;
		public double Altitude;
		public double AltitudeSol;
        public double PoidsAvion;
		public double TotalFuelCapacity;
		public double Vario;
		public double Heading;
		public int TimeFactor;
		public double Longitude;
		public double Latitude;
		public double GSpeed;
		public double TASSpeed;
		public double IASSpeed;
		public double Fuel;
		public double GForce;
		public double Pitch;
		public double Bank;
		public double AmbiantWindVelocity;
		public double AmbiantWindDirection;
		public double SeaLevelPressure;
        public int  XAxis;
        public int YAxis;
        public int Slider;
        public int RZAxis;
        public int HatAxis;
		public int Squawk;
		public int Realism;
		public uint AmbiantPrecipState;
		public int AltimeterSetting;
		public double ElevatorPos;
		public double AileronPos;
		public double RudderPos;
		public double SpoilerPos;
		public double ParkingBrakePos;
		public int Door1Pos;
		public int Door2Pos;
		public int Door3Pos;
		public int Door4Pos;
		public int FlapsIndex;
		public int NumEngine;
		public int ThrottleEng1;
		public int ThrottleEng2;
		public int ThrottleEng3;
		public int ThrottleEng4;
		//Etat
		public bool ParkingBrake;
		public bool Pause;
		public bool Crash;
		public bool RunState;
		public bool Pushback;
		public bool OverSpeed;
		public bool Slew;
		public bool OnGround;
		public bool Stalling;
		public bool GearPos;
		public bool LandingLight;
		public bool BeaconLight;
		public bool StrobeLight;
		public bool NavLight;
		public bool RecoLight;
		public bool StateEng1;
		public bool StateEng2;
		public bool StateEng3;
		public bool StateEng4;
		public bool Smoke;
	}
}
