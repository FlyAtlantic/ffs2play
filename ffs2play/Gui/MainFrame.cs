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
 * MainFrame.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Globalization;
using ffs2play.Properties;

namespace ffs2play
{
	/// <summary>
	/// Classe de la fenêtre principale
	/// </summary>
	public partial class ffs2play : Form
    {
        /// <summary>
        /// Membres de la classe
        /// </summary>

		public enum GraphType
		{
			Alt,
			Vario,
			Vitesse
		}

        // Etat de l'initialisation de la fenêtre
        private bool windowInitialized;

		private ApplicationSettingsBase settings;

		private System.Resources.ResourceManager RM = null;
		private CultureInfo EnglishCulture = new CultureInfo("en-US");
		private CultureInfo FrenchCulture = new CultureInfo("fr-FR");
		// Controleur SimConnect
		SCManager scm;

		//Gestionnaire de P2P
		P2PManager P2P;

		// Controleur de Pirep
		PirepManager pm;

		// Analyseur de vol
		AnalyseurManager Analyse;

		// Gestionnaire de log
		Logger log;

		// Gestionnaire de Mapping AI
		AIMapping m_Mapping;

        /// <summary>
        /// Constructeur de la fenêtre principale
        /// </summary>
        public ffs2play()
        {

			// Définition de la culture par défaut
			Thread.CurrentThread.CurrentUICulture = FrenchCulture;
			// LeResourceManager prend en paramètre : nom_du_namespace.nom_de_la_ressource_principale
			RM = new System.Resources.ResourceManager("Localisation.Form1", typeof(ffs2play).Assembly);
			// Initialisation de contenu de la fenêtre
			InitializeComponent();
#if DEBUG
	#if P3D
			Text = "ffs2play P3D (version Debug)";
	#else
			Text = "ffs2play (version Debug)";
	#endif
#else
	#if P3D
			Text = "ffs2play P3D";
	#else
			Text = "ffs2play";
	#endif
#endif
			pInstance = this;
			// Préparation du bouton de connexion au simulateur
			ILConnexionFS.Images.Add(Resources.plane_red);
			ILConnexionFS.Images.SetKeyName(0, "Déconnecté");
			ILConnexionFS.Images.Add(Resources.plane_green);
			ILConnexionFS.Images.SetKeyName(1, "Connecté");
			btnConnectFS.ImageIndex = 0;

			// Préparation du bouton de connexion au serveur web
			ilServeur.Images.Add(Resources.connect2_R);
			ilServeur.Images.SetKeyName(0, "Déconnecté");
			ilServeur.Images.Add(Resources.connect2_V);
			ilServeur.Images.SetKeyName(1, "Connecté");
			btnConPirep.ImageIndex = 0;

			// Configuration par défaut de la fenêtre
			WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.WindowsDefaultBounds;
			SetAllowUnsafeHeaderParsing20();

			// Copy user settings from previous application version if necessary
			settings = Settings.Default;
            if ((bool)settings["UpgradeRequired"] == true)
			{
				// upgrade the settings to the latest version
				settings.Upgrade();

				// clear the upgrade flag
				settings["UpgradeRequired"] = false;
				settings.Save();
			}
			else
			{
				// the settings are up to date
			}
            // On vérifie des les données de taille et de position de la fenêtre ne sont pas vides
            // Et qu'elles sont toujours valides avec la configuration d'écran actuelle
            // Ceci pour éviter une fenêtre en dehors des limites
            if (Properties.Settings.Default.WindowPosition != Rectangle.Empty &&
                IsVisibleOnAnyScreen(Properties.Settings.Default.WindowPosition))
            {
                // Définition de la position et de la taille de la fenêtre
                StartPosition = FormStartPosition.Manual;
                DesktopBounds = Properties.Settings.Default.WindowPosition;

                // Définition de l'état de la fenêtre pour être maximisée
                // ou réduite selon la sauvegarde
                WindowState = Properties.Settings.Default.WindowState;
            }
            else
            {
                // Réinitialisation de la position à la valeur par défaut
                StartPosition = FormStartPosition.WindowsDefaultLocation;

                // Nous pouvons alors définir la taille enrégistrée si celle ci 
                // Existe
                if (Properties.Settings.Default.WindowPosition != Rectangle.Empty)
                {
                    Size = Properties.Settings.Default.WindowPosition.Size;
                }
            }
            // Mise à jour de l'option Météo Automatique
            cbEnaAutoWeather.Checked= Settings.Default.MetarAutoEnable;
            // La fenêtre est configurée
            windowInitialized = true;

			log = Logger.Instance;
			log.LogMessage("ffs2play démarré");

			// Creation du contrôleur d'interface avec le simulateur
			scm = SCManager.Instance;

			// Création du contrôleur de mapping des AI

			m_Mapping = AIMapping.Instance;

			// Création du contrôleur d'inteface avec le serveur web
			pm = PirepManager.Instance;

			// Création du contrôleur d'analyse de vol
			Analyse = AnalyseurManager.Instance;

            // Création du contrôleur de connexion P2P
            P2P = P2PManager.Instance;
			if (Properties.Settings.Default.P2PEnable) P2P.Init(true);
			AcceptButton = btnSend;
			string[] arguments = Environment.GetCommandLineArgs();
			scm.openConnection();

            //Vérification de la version
            string CheckVersionURL;
#if P3D
            CheckVersionURL = "http://download.ffsimulateur2.fr/ffs2playp3d.php";
#else
            CheckVersionURL = "http://download.ffsimulateur2.fr/ffs2play.php";
#endif
            if (Properties.Settings.Default.Beta)
            {
                CheckVersionURL += "?beta";
            }
            HTTPRequestThread CheckVersion = new HTTPRequestThread(CheckVersionURL, CheckVersionCallBack);

            CheckVersion.Start();
			if (!Properties.Settings.Default.LogVisible)
			{
				scMainWindow.Panel2Collapsed = true;
				scMainWindow.Panel2.Hide();
			}
        }

		private void CheckVersionCallBack (HttpWebResponse Reponse)
		{
			sVersion Actual = Outils.GetVersion();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc = pm.GetResultXml(ref Reponse);
			XmlNode Version = pm.GetFirstElement(ref xmlDoc, "version");
			if (Version != null)
			{
				int SiteMajor = Convert.ToInt32(Version.Attributes["major"].Value);
				int SiteMinor = Convert.ToInt32(Version.Attributes["minor"].Value);
				int SiteBuild = Convert.ToInt32(Version.Attributes["build"].Value);
                Byte Beta = Convert.ToByte(Version.Attributes["beta"].Value);
				bool maj = false;
				bool compat = true;
				string Message = "";
                if (Beta==1) Message = "!!! Attention, Nouvelle Version Bêta disponible !!!"+ Environment.NewLine+ Environment.NewLine;
				if (SiteMajor > Actual.Major)
				{
					Message += "Cette version de FFS2Play n'est pas supportée par le site. souhaitez vous procéder à la mise à jour ?";
					maj = true;
					compat = false;
				}
				else if ((SiteMinor > Actual.Minor) || ((SiteMinor == Actual.Minor) && (SiteBuild > Actual.Build)))
				{
					Message += "Cette version de FFS2Play n'est plus a jour. souhaitez vous procéder à la mise à jour ?";
					maj = true;
				}

				if (maj)
				{
					DialogResult dialogResult = MessageBox.Show(Message, "Mise à jour de FFS2Play", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						XmlNode Url = pm.GetFirstElement(ref xmlDoc, "url");
						if (Url != null)
						{
							Process.Start(Url.InnerText);
						}
						pm.AutoExit = true;
						Invoke(new Action(() => { Close(); }));
						return;
					}
					if (!compat) return;
				}
			}
		}


		public bool SetAllowUnsafeHeaderParsing20()
		{
			//Get the assembly that contains the internal class
			Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
			if (aNetAssembly != null)
			{
				//Use the assembly in order to get the internal type for the internal class
				Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
				if (aSettingsType != null)
				{
					//Use the internal static property to get an instance of the internal settings class.
					//If the static instance isn't created allready the property will create it for us.
					object anInstance = aSettingsType.InvokeMember("Section",
					  BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

					if (anInstance != null)
					{
						//Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
						FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
						if (aUseUnsafeHeaderParsing != null)
						{
							aUseUnsafeHeaderParsing.SetValue(anInstance, true);
							return true;
						}
					}
				}
			}
			return false;
		}

        /// <summary>
        /// S'assure que le rectangle est visible sur le ou les écrans
        /// </summary>
        /// <param name="rect">Rect représente la surface à tester</param>
        /// <returns>Retourne vrai si le rectangle est entierement visible</returns>
        private bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }



        /// <summary>
        /// Evénement sur la fermeture de l'application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
        }

        /// <summary>
        /// Gestion d'un événement de redimensionnement
        /// De la fenêtre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResize(object sender, EventArgs e)
        {
            TrackWindowState();
        }
        /// <summary>
        /// Gestion d'un évenement de déplacement de
        /// la fenêtre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMove(object sender, EventArgs e)
        {
            TrackWindowState();
        }
        /// <summary>
        /// Méthode pour suivre les modification de la géométrie et 
        /// de la position de la fenêtre
        /// </summary>
        private void TrackWindowState()
        {
            // Don't record the window setup, otherwise we lose the persistent values!
            if (!windowInitialized) { return; }

            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPosition = this.DesktopBounds;
            }

			if (WindowState == FormWindowState.Minimized)
			{
			}
        }
	
		/// <summary>
		/// Click sur le menu quitter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Fermeture de l'application
			Close();
		}

        /// <summary>
        /// Fermeture du programme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ffs2play_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!pm.AutoExit)
			{
				DialogResult dialogResult = MessageBox.Show("Etes vous sûr de vouloir quitter ffs2play?", "Confirmation de sortie", MessageBoxButtons.YesNo);
				if (dialogResult == DialogResult.No)
				{
					e.Cancel = true;
					return;
				}
			}
			P2P.Init(false);
			// On ferme la connexion avec simconnect
			scm.closeConnection();
			// Sauvegarde du status de la fenêtre seulement normal ou maximised)
			switch (WindowState)
			{
				case FormWindowState.Normal:
				case FormWindowState.Maximized:
					Properties.Settings.Default.WindowState = WindowState;
					break;

				default:
					Properties.Settings.Default.WindowState = FormWindowState.Normal;
					break;
			}
			// Sauvegarde de la configuration
			settings.Save();
			if (pm.IsConnected())
			{
				pm.AutoExit = true;
				e.Cancel = true;
				pm.Disconnect(PirepManager.Disco_mode.Close);
			}
		}

        /// <summary>
        /// Boite de dialogue "A Propos"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void aProposToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox ABox = new AboutBox();
			ABox.ShowDialog( );
		}

        /// <summary>
        /// Gestion du menu contextuel dans la zone de log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void rtbLogZone_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{   //click event
				ContextMenu contextMenu = new ContextMenu();
				MenuItem menuItem = new MenuItem("Sauvegarde sous...");
				menuItem.Click += new EventHandler(LogSaveAs);
				contextMenu.MenuItems.Add(menuItem);
				menuItem = new MenuItem("Copier");
				menuItem.Click += new EventHandler(LogCopy);
				contextMenu.MenuItems.Add(menuItem);
				menuItem = new MenuItem("Effacer");
				menuItem.Click += new EventHandler(LogClear);
				contextMenu.MenuItems.Add(menuItem);
				rtbLogZone.ContextMenu = contextMenu;
			}
		}

        /// <summary>
        /// Click sur la fonction copier du menu contextuel du log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void LogCopy(object sender, EventArgs e)
		{
			bool All = false;

			if (rtbLogZone.SelectionLength == 0)
			{
				rtbLogZone.SelectAll();
				All = true;
			}
			System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, rtbLogZone.SelectedText);
			if (All) rtbLogZone.DeselectAll();
		}

        /// <summary>
        /// Click sur la fonction Clear du menu contextuel du log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void LogClear(object sender, EventArgs e)
		{
			rtbLogZone.Clear();
		}

        /// <summary>
        /// Click sur la fonction Sauvegarder sous du menu contextuel du log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void LogSaveAs(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				sfd.Filter = "log files (*.rtf)|*.rtf|All file (*.*)|*.*";
				sfd.FilterIndex = 0;

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					rtbLogZone.SaveFile(sfd.FileName);
				}
			}
		}

        /// <summary>
        /// Affichage de la boîte de dialogue du profil utilisateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void profilsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult Result;
			dlgConServeur ConPirep = new dlgConServeur();
			Result = ConPirep.ShowDialog();
		}

 		private static ffs2play pInstance = null;

		public static Control getControl(string ControlName)
		{
			if (pInstance != null)
			{
				Control[] reference = pInstance.Controls.Find(ControlName, true);
				if (reference.Length>0)
				{
					return reference[0];
				}
			}
			return null;
		}
        /// <summary>
        /// Affichage de la boîte de dialogue de gestion P2P
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnP2P_Click(object sender, EventArgs e)
        {
            DialogResult Result;
            dlgP2P P2P = new dlgP2P();
            Result = P2P.ShowDialog();
        }

		private void btnSend_Click(object sender, EventArgs e)
		{
			P2P.Send_Tchat(tb_TextChat.Text);
			P2P.AddLineChat(pm.Get_UserName(), tb_TextChat.Text);
			tb_TextChat.Clear();
		}

		private void tb_TextChat_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				P2P.Send_Tchat(tb_TextChat.Text);
				P2P.AddLineChat(pm.Get_UserName(), tb_TextChat.Text);
				tb_TextChat.Clear();
			}
		}

		private void btnDebug_Click(object sender, EventArgs e)
		{
			DialogResult Result;
			dlgLogger Logger = new dlgLogger(this);
			Result = Logger.ShowDialog();
		}

		public bool LogVisible
		{
			get { return Settings.Default.LogVisible; }
			set
			{
				if (value == Settings.Default.LogVisible) return;
				if (value)
				{
					scMainWindow.Panel2.Show();
					scMainWindow.Panel2Collapsed = false;
				}
				else
				{
					scMainWindow.Panel2Collapsed = true;
					scMainWindow.Panel2.Hide();
				}
				Settings.Default.LogVisible = value;
				Settings.Default.Save();
			}
		}

		private void btnAide_Click(object sender, EventArgs e)
		{
			Process.Start("https://ffs2play.fr/page/documentation");
		}

        /// <summary>
        /// Gestion de la mise à jour automatique de la météo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbEnaAutoWeather_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.MetarAutoEnable = cbEnaAutoWeather.Checked;
            settings.Save();
        }

        /// <summary>
        /// Envoie de la méto via le champs de saisie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnvoyerMetar_Click(object sender, EventArgs e)
        {
            if (tbMetarManuel.Text.Length>4)
            {
                pm.SendMetar(tbMetarManuel.Text);
            }
        }
    }
}
