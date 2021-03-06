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
 * PirepManager.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Windows.Forms;
using System.Timers;
using System.Xml;
using System.Drawing;
using ffs2play.Properties;

namespace ffs2play
{

    /// <summary>
    /// Représente un utilisateur PIREP
    /// </summary>
	[Serializable()]
	public struct PirepUser
	{
		public string Profil;
		public string Login;
		public string Password;
		public string URL;
	}

    /// <summary>
    /// Représente un Aéroport
    /// </summary>
	public struct Airport
	{
		public string ICAO;
		public double Longitude;
		public double Latitude;
	}

    /// <summary>
    /// Classe de gestion de PIREPs
    /// </summary>
	public partial class PirepManager
	{
        private enum XmlCode
        {
            hello,
            verify,
            liveupdate,
			atc,
			syncai,
			sendai,
			askai,
			logout
        }
		private System.Timers.Timer LiveUpdateTimer;
		private System.Timers.Timer ATCTimer;
		private ToolTip ButtonsTips;
		private SCManager SCM;
		private P2PManager P2P;

		// Etat de la connexion
		private bool m_bConnected;
		private Logger Log;
		private AIMapping Mapping;

		public bool AutoExit;
		private bool m_AIManagement;
		private bool m_SyncAIDone;
		public bool DelayedExit;
		private AnalyseurManager Analyse;
        private string m_sURL;
        private string Key;

        private string m_sAESKey;
        private string Crypted_AESKey;
		private long LastGoodUpdate;
		private PirepUser User;
        private XmlDocument SendHello;
        private XmlDocument SendVerify;
        private XmlDocument SendUpdate;
		private XmlDocument SendAtc;
		private XmlDocument SendSyncAI;
		private XmlDocument SendAI;

        public string Get_UserName()
		{
			if (IsConnected()) return User.Login;
			else return "";
		}

		/// <summary>
		/// Constructeur du Singleton
		/// </summary>              		
		private PirepManager()
        {
			DelayedExit = false;
			DelayReco = false;
			AutoExit = false;
			m_SyncAIDone = false;
			Log = Logger.Instance;
			LastGoodUpdate = DateTimeEx.UtcNowMilli;
            Mapping = AIMapping.Instance;
			try
			{
				if ((m_btnConnect = ffs2play.getControl("btnConPirep") as Button) != null) m_btnConnect.Click += btnConPirep_Click;
				else throw new InvalidOperationException("Le bouton btnConPirep n'existe pas");
                if ((m_rtbDecryptedMetar = ffs2play.getControl("rtbDecryptedMetar") as RichTextBox) == null) throw new InvalidOperationException("Le contrôle richttextbox rtbDecryptedMetar n'existe pas");
                if ((m_tbMetar = ffs2play.getControl("tbMetarManuel") as TextBox) == null) throw new InvalidOperationException("Le contrôle tbMetarManuel n'existe pas");
            }
			catch (Exception e)
            {
				Log.LogMessage("Erreur Constructeur PirepManager : " + e.Message);
			}
			m_bConnected = false;
			m_AIManagement = false;
			m_sURL = "";
			Analyse = AnalyseurManager.Instance;
			LiveUpdateTimer = new System.Timers.Timer(30000);
			LiveUpdateTimer.Elapsed += new ElapsedEventHandler(OnLiveUpdate);
			LiveUpdateTimer.Start();
			ATCTimer = new System.Timers.Timer(5000);
			ATCTimer.Elapsed += new ElapsedEventHandler(OnATCUpdate);
			SCM = SCManager.Instance;
			P2P = P2PManager.Instance;
            AEStoPHPCryptography aes = new AEStoPHPCryptography();
            aes.GenerateRandomKeys();
            m_sAESKey = aes.EncryptionKeyString;
#if DEBUG
            Log.LogMessage("PManager: AES Key = " + m_sAESKey, Color.DarkRed, 2);
#endif
            ButtonsTips = new ToolTip();
		}

        /// <summary>
        /// Retourne l'instance du singleton
        /// </summary>
        public static PirepManager Instance
		{
			get
			{
				return Nested.instance;
			}
		}

		public bool DelayReco { get; private set; }

		/// <summary>
		/// Classe d'instance unique du signleton
		/// </summary>
		class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly PirepManager instance = new PirepManager();
		}

        /// <summary>
        /// Méthode de connexion au serveur web
        /// </summary>
        /// <param name="pUser"></param>
        public void Connect(PirepUser pUser)
        {
            if (!m_bConnected)
            {
                try
                {
                    // On crée un thread pour opérer la connexion de manière asynchrone, sans bloquer
                    // La collecte de donnée du thread principal
                    User = pUser;
                    m_sURL = "http://" + User.URL + "/action.php";
                    XmlBuild(ref SendHello, XmlCode.hello);
                    Log.LogMessage("Connexion en cours...");
#if DEBUG
					Log.LogMessage("PManager: Demande de connexion =\n " + Beautify(SendHello), Color.DarkRed, 2);
#endif
                    HTTPRequestThread Requete = new HTTPRequestThread(SendHello, m_sURL, HelloCB);
                    Requete.Start();
                }
                catch (Exception e)
                {
                    // Nous le signalons par un message
                    Log.LogMessage("La connexion au serveur a échoué : " + e.Message, Color.Violet);
                }
            }
        }

        public enum Disco_mode
		{
			Normal,
			Reco,
			Close
		}

        /// <summary>
        /// Méthode de déconnexion au serveur web
        /// </summary>
		public void Disconnect(Disco_mode Mode = Disco_mode.Normal)
		{
			if (m_bConnected)
			{
				ATCTimer.Stop();
				OnLiveUpdate(null, null);
				XmlDocument SendLogout=new XmlDocument();
				XmlBuild(ref SendLogout, XmlCode.logout);
				Log.LogMessage("Déconnexion en cours...");
#if DEBUG
				Log.LogMessage("PManager: Demande de déconnexion =\n " + Beautify(SendLogout), Color.DarkRed, 2);
#endif
				m_sURL = "http://" + User.URL + "/action.php";
				HTTPRequestThread Requete = new HTTPRequestThread(SendLogout, m_sURL,DisconnectCB);
				Requete.Start();
				switch (Mode)
				{
					case (Disco_mode.Close):
						DelayedExit = true;
						break;
					case (Disco_mode.Reco):
						DelayReco = true;
						break;
				}
			}
		}

		/// <summary>
		/// Méthode cyclique du timer
		/// Gestion des tâches périodiques
		/// Pour les demandes de données sur le simulateur
		/// </summary>
		/// <param name="source"></param>
		/// <param name="evt"></param>
		private void OnLiveUpdate(object source, ElapsedEventArgs evt)
		{
			if (IsConnected() && SCM.IsConnected())
			{
				XmlBuild(ref SendUpdate, XmlCode.liveupdate);
				XmlNodeList Nodes = SendUpdate.GetElementsByTagName("liveupdate");
				foreach (XmlNode node in Nodes)
				{
					foreach (XmlNode child in node.ChildNodes)
					{
						switch (child.Name)
						{
							case "pilotID":
								child.InnerText = User.Login;
								break;
							case "registration":
								child.InnerText = Analyse.GetLastState().Title;
								break;
							case "latitude":
								child.InnerText = string.Format("{0:0.00000}", Analyse.GetLastState().Latitude);
								break;
							case "longitude":
								child.InnerText = string.Format("{0:0.00000}", Analyse.GetLastState().Longitude);
								break;
							case "heading":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().Heading);
								break;
							case "altitude":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().Altitude);
								break;
							case "groundSpeed":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().GSpeed);
								break;
							case "iaspeed":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().IASSpeed);
								break;
							case "squawk":
								child.InnerText = string.Format("{0:0.}", Outils.ConvertToBinaryCodedDecimal(Analyse.GetLastState().Squawk));
								break;
							case "onground":
								child.InnerText = string.Format("{0:0.}", Convert.ToInt32(Analyse.GetLastState().OnGround));
								break;
							case "sim":
								child.InnerText = SCM.GetVersion().ToString();
								break;
						}
					}
				}
#if DEBUG
				Log.LogMessage("PManager: Mise à jour de la position =\n " + Beautify(SendUpdate), Color.DarkRed,2);
#endif
				HTTPRequestThread Requete = new HTTPRequestThread(SendUpdate, m_sURL, LiveUpdateCB);
				Requete.Start();
			}
		}

		/// <summary>
		/// Méthode cyclique du timer
		/// Gestion des tâches périodiques
		/// Pour les demandes de données sur le simulateur
		/// </summary>
		/// <param name="source"></param>
		/// <param name="evt"></param>
		private void OnATCUpdate(object source, ElapsedEventArgs evt)
		{
			if (IsConnected() && SCM.IsConnected())
			{
				int Transponder = Outils.ConvertToBinaryCodedDecimal(Analyse.GetLastState().Squawk);
				if ((Transponder == 1200) || (Transponder == 7000)) return;
				XmlBuild(ref SendAtc, XmlCode.atc);
				XmlNodeList Nodes = SendAtc.GetElementsByTagName("atc");
				foreach (XmlNode node in Nodes)
				{
					foreach (XmlNode child in node.ChildNodes)
					{
						switch (child.Name)
						{
							case "latitude":
								child.InnerText = string.Format("{0:0.00000}", Analyse.GetLastState().Latitude);
								break;
							case "longitude":
								child.InnerText = string.Format("{0:0.00000}", Analyse.GetLastState().Longitude);
								break;
							case "heading":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().Heading);
								break;
							case "altitude":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().Altitude);
								break;
							case "groundSpeed":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().GSpeed);
								break;
							case "iaspeed":
								child.InnerText = string.Format("{0:0.}", Analyse.GetLastState().IASSpeed);
								break;
							case "squawk":
								child.InnerText = string.Format("{0:0.}", Outils.ConvertToBinaryCodedDecimal(Analyse.GetLastState().Squawk));
								break;
						}
					}
				}
#if DEBUG
				Log.LogMessage("PManager: ATCUpdate =\n " + Beautify(SendAtc), Color.DarkRed, 2);
#endif
				HTTPRequestThread Requete = new HTTPRequestThread(SendAtc, m_sURL, ATCCB);
				Requete.Start();
			}
		}

        /// <summary>
        /// Met à jour le moteur Météo
        /// </summary>
        /// <param name="Metar"></param>

        private void MetarUpdate(ref XmlNode MetarNode)
        {
            if ((MetarNode == null) || (!Settings.Default.MetarAutoEnable)) return;
            DateTime MetarTime = new DateTime();
            string sMetar = "";
            foreach (XmlNode node in MetarNode.ChildNodes)
            {
                string Name = node.Name;
                foreach (XmlAttribute Att in node.Attributes)
                {
                    switch (Att.Name)
                    {
                        case "time":
                            {
                                try
                                {
                                    DateTime.TryParse (Att.Value,out MetarTime);
                                }
                                catch (FormatException e)
                                {
                                    Log.LogMessage("PManager: Metar date error : " + e.Message, Color.DarkViolet);
                                }
                                break;
                            }
                        case "raw":
                            {
                                sMetar = Att.Value;
                                break;
                            }
                    }
                }
#if DEBUG
                Log.LogMessage("PManager: reçu Metar " + Name + " heure = " + MetarTime.ToUniversalTime() + " Contenu = " + sMetar, Color.DarkBlue, 2);
#endif
                m_tbMetar.Invoke(new Action(() => { m_tbMetar.Text = sMetar; }));
                SendMetar(sMetar);
                break;
            }
            
        }

        public void SendMetar (string pMetar)
        {
            SCM.SendWeatherObservation(pMetar);
        }

		/// <summary>
		/// Construit le patron 
		/// </summary>
		/// <param name="Doc"></param>
		/// <param name="Code"></param>
		private void XmlBuild (ref XmlDocument Doc, XmlCode Code)
        {
            if (Doc == null) Doc = new XmlDocument();
            else Doc.RemoveAll();
			sVersion Actual = Outils.GetVersion();
            //Déclaration de la norme utilisée
            XmlDeclaration xmlDeclaration = Doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = Doc.DocumentElement;
            Doc.InsertBefore(xmlDeclaration, root);

            //Création de la racine
            XmlElement xmlffs2play = Doc.CreateElement(string.Empty, "ffs2play", string.Empty);
            Doc.AppendChild(xmlffs2play);

            //Création de la version
            XmlElement xmlVersion = Doc.CreateElement(string.Empty, "version", string.Empty);
            xmlVersion.InnerText = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_" + Actual.Major.ToString() + "_" + Actual.Minor.ToString() + "_" + Actual.Build.ToString();
            xmlffs2play.AppendChild(xmlVersion);

			//Activation du P2P;
			XmlElement xmlP2P = Doc.CreateElement(string.Empty, "P2P", string.Empty);
			xmlP2P.InnerText = Properties.Settings.Default.P2PEnable.ToString();
			xmlffs2play.AppendChild(xmlP2P);

			//Création du switch
			XmlElement xmlSwitch = Doc.CreateElement(string.Empty, "switch", string.Empty);
            xmlffs2play.AppendChild(xmlSwitch);

            //Création du data
            XmlElement xmlData = Doc.CreateElement(string.Empty, "data", string.Empty);
            xmlData.InnerText = Code.ToString();
            xmlSwitch.AppendChild(xmlData);

            //Création du key
            XmlElement xmlKey = Doc.CreateElement(string.Empty, "key", string.Empty);
            xmlKey.InnerText = Key;
            xmlSwitch.AppendChild(xmlKey);

            switch (Code)
            {
                case XmlCode.hello:
                    XmlElement xmlHello = Doc.CreateElement(string.Empty, "hello", string.Empty);
                    xmlffs2play.AppendChild(xmlHello);
                    //Création de la balise pilotID
                    XmlElement xmlLogin = Doc.CreateElement(string.Empty, "pilotID", string.Empty);
                    xmlLogin.InnerText = User.Login;
                    xmlHello.AppendChild(xmlLogin);
                    break;
                case XmlCode.verify:
                    //Création de la balise verify
                    XmlElement xmlVerify = Doc.CreateElement(string.Empty, "verify", string.Empty);
                    xmlffs2play.AppendChild(xmlVerify);

                    //Création de la balise AES
                    XmlElement xmlAES = Doc.CreateElement(string.Empty, "AES", string.Empty);
                    xmlAES.InnerText = Crypted_AESKey;
                    xmlVerify.AppendChild(xmlAES);

                    //Création de la balise pilotID
                    XmlElement xmlPilotID = Doc.CreateElement(string.Empty, "pilotID", string.Empty);
                    xmlPilotID.InnerText = User.Login;
                    xmlVerify.AppendChild(xmlPilotID);

                    //Création de la balise password
                    XmlElement xmlPassword = Doc.CreateElement(string.Empty, "password", string.Empty);
                    xmlPassword.InnerText = Outils.EncryptMessage(Outils.Decrypt(User.Password), m_sAESKey);
                    xmlVerify.AppendChild(xmlPassword);

					//Envoi du port en écoute
					XmlElement xmlPort = Doc.CreateElement(string.Empty, "port", string.Empty);
					xmlPort.InnerText = P2P.Port.ToString();
					xmlVerify.AppendChild(xmlPort);

					//Envoi du port en écoute
					XmlElement xmlLocalIP = Doc.CreateElement(string.Empty, "local_ip", string.Empty);
					xmlLocalIP.InnerText = P2P.LocalIPSerialized;
					xmlVerify.AppendChild(xmlLocalIP);
					break;
                case XmlCode.liveupdate:
                    //Création de la balise liveupdate
                    XmlElement xmlLiveUpdate2 = Doc.CreateElement(string.Empty, "liveupdate", string.Empty);
                    xmlffs2play.AppendChild(xmlLiveUpdate2);
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "registration", string.Empty)); //Création de la balise registration
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "latitude", string.Empty)); //Création de la balise latitude
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "longitude", string.Empty)); //Création de la balise longitude
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "heading", string.Empty)); //Création de la balise heading
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "altitude", string.Empty)); //Création de la balise altitude
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "groundSpeed", string.Empty)); //Création de la balise groundSpeed
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "iaspeed", string.Empty)); //Création de la balise iaspeed
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "squawk", string.Empty)); //Création de la balise squawk
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "onground", string.Empty)); //Création de la balise onground
					xmlLiveUpdate2.AppendChild(Doc.CreateElement(string.Empty, "sim", string.Empty)); //Création de la balise simulateur
					break;
				case XmlCode.atc:
					//Création de la balise liveupdate
					XmlElement xmlAtc = Doc.CreateElement(string.Empty, "atc", string.Empty);
					xmlffs2play.AppendChild(xmlAtc);
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "latitude", string.Empty)); //Création de la balise latitude
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "longitude", string.Empty)); //Création de la balise longitude
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "heading", string.Empty)); //Création de la balise heading
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "altitude", string.Empty)); //Création de la balise altitude
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "groundSpeed", string.Empty)); //Création de la balise groundSpeed
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "iaspeed", string.Empty)); //Création de la balise iaspeed
					xmlAtc.AppendChild(Doc.CreateElement(string.Empty, "squawk", string.Empty)); //Création de la balise squawk
					break;
				case XmlCode.syncai:
					XmlElement xmlSyncAI = Doc.CreateElement(string.Empty, "syncai", string.Empty);
					xmlffs2play.AppendChild(xmlSyncAI);
					xmlSyncAI.AppendChild(Doc.CreateElement(string.Empty, "md5list", string.Empty));
					break;
				case XmlCode.sendai:
					XmlElement xmlSendAI = Doc.CreateElement(string.Empty, "sendai", string.Empty);
					xmlffs2play.AppendChild(xmlSendAI);
					break;
			}
        }
		
	}
}
