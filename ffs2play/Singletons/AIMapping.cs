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
 * AIMapping.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Data.SQLite;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using System.Collections.Specialized;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors;

namespace ffs2play
{
	public struct AIResol
	{
		public string Titre;
		public double CG_Height;
		public double Pitch;
	}
	class AIMapping
	{
		private Logger Log;
		private SCManager m_SC;
		private Thread m_Thread;
		private SQLiteConnection m_DB;
		private string m_DatabasePath;
		private StringCollection m_AircraftPath;
		private string[] BaseInstallation;
		private bool m_Initialized;
		private string m_Table;
		private int m_NbAdded;
		private int m_NbUpdated;
		public bool IsInit
		{
			get { return m_Initialized; }
		}

		public static AIMapping Instance
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

			internal static readonly AIMapping instance = new AIMapping();
		}

        /// <summary>
        /// Retrouve l'entrée dans la base de registre du simulateur
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="SubKey"></param>
        /// <returns></returns>

        private string FindInstallationPath (string Key)
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\" + Key);
                if (registryKey == null)
                {
                    registryKey = Registry.LocalMachine.OpenSubKey("Software\\" + Key);
                    if (registryKey == null)
                    {
#if DEBUG
                        Log.LogMessage("AIMapping : Simulateur " + Key + " introuvable" , Color.DarkOliveGreen,1);
#endif
                        return "";
                    }
                }
                // 1ere recherche de la clé "Install_Path"
                string Reponse = (string)registryKey.GetValue("Install_Path");
                if (Reponse == null)
                {
#if DEBUG
                    Log.LogMessage("AIMapping : Clé \"Install_Path\" introuvable dans " + Key, Color.DarkOliveGreen, 1);
#endif
                    // 2ème recherche de la clé "SetupPath"
                    Reponse = (string)registryKey.GetValue("SetupPath");
                    if (Reponse == null)
                    {
#if DEBUG
                        Log.LogMessage("AIMapping : Clé \"SetupPath\" introuvable dans " + Key, Color.DarkOliveGreen, 1);
#endif
                        // 3ème recherche de la clé "AppPath"
                        Reponse = (string)registryKey.GetValue("AppPath");
                        if (Reponse == null)
                        {
#if DEBUG
                            Log.LogMessage("AIMapping : Clé \"AppPath\" introuvable dans " + Key, Color.DarkOliveGreen, 1);
#endif
                            return "";
                        }
                    }
                }
                if (!Reponse.EndsWith("\\")) Reponse += "\\";
                return Reponse;
            }
            catch (ObjectDisposedException e)
            {
                Log.LogMessage("AIMapping : Erreur accès à la base de registre = " + e.Message, Color.DarkViolet);
                return "";
            }
        }

		private AIMapping()
		{
			m_Initialized = false;
			Log = Logger.Instance;
			m_SC = SCManager.Instance;
			m_SC.OnSCReceiveOpen += OnSCReceiveOpen;
			// On localise l'installateur de chaque simulateur
			BaseInstallation = new string[5];
			//Check for FSX Installation
			if (Properties.Settings.Default.AIScanFoldersFSX.Count < 1)
			{
                BaseInstallation[0] = FindInstallationPath("Microsoft\\Microsoft Games\\Flight Simulator\\10.0");
                if (BaseInstallation[0].Length >0)
				{
					Properties.Settings.Default.AIScanFoldersFSX.Add(BaseInstallation[0] + "SimObjects\\Airplanes");
					Properties.Settings.Default.AIScanFoldersFSX.Add(BaseInstallation[0] + "SimObjects\\Rotorcraft");
					Properties.Settings.Default.Save();
#if DEBUG
					Log.LogMessage("AIMapping : Répertoire d'installation FSX trouvé :  = " + BaseInstallation[0], Color.DarkBlue, 1);
#endif
				}
			}

			//Vérification installation de FSX Steam
			if (Properties.Settings.Default.AIScanFoldersFSXSE.Count < 1)
			{
                BaseInstallation[1] = FindInstallationPath("DovetailGames\\FSX");
                if (BaseInstallation[1].Length > 0)
                {
 					if (!BaseInstallation[1].EndsWith("\\")) BaseInstallation[1] += "\\";
					Properties.Settings.Default.AIScanFoldersFSXSE.Add(BaseInstallation[1] + "SimObjects\\Airplanes");
					Properties.Settings.Default.AIScanFoldersFSXSE.Add(BaseInstallation[1] + "SimObjects\\Rotorcraft");
					Properties.Settings.Default.Save();
#if DEBUG
					Log.LogMessage("AIMapping : Répertoire d'installation FSX STEAM trouvé :  = " + BaseInstallation[1], Color.DarkOliveGreen, 1);
#endif
				}
			}
			//Vérification installation de P3D
			if (Properties.Settings.Default.AIScanFoldersP3DV2.Count < 1)
			{
                BaseInstallation[2] = FindInstallationPath("Lockheed Martin\\Prepar3D v2");
                if (BaseInstallation[2].Length > 0)
                {
					Properties.Settings.Default.AIScanFoldersP3DV2.Add(BaseInstallation[2] + "SimObjects\\Airplanes");
					Properties.Settings.Default.AIScanFoldersP3DV2.Add(BaseInstallation[2] + "SimObjects\\Rotorcraft");
					Properties.Settings.Default.Save();
#if DEBUG
					Log.LogMessage("AIMapping : Répertoire d'installation Prepar3D v2 trouvé :  = " + BaseInstallation[2], Color.DarkOliveGreen, 1);
#endif
				}
			}
			if (Properties.Settings.Default.AIScanFoldersP3DV3.Count < 1)
			{
                BaseInstallation[3] = FindInstallationPath("Lockheed Martin\\Prepar3D v3");
                if (BaseInstallation[3].Length > 0)
                {
                    Properties.Settings.Default.AIScanFoldersP3DV3.Add(BaseInstallation[3] + "SimObjects\\Airplanes");
					Properties.Settings.Default.AIScanFoldersP3DV3.Add(BaseInstallation[3] + "SimObjects\\Rotorcraft");
					Properties.Settings.Default.Save();
#if DEBUG
					Log.LogMessage("AIMapping : Répertoire d'installation Prepar3D v3 trouvé :  = " + BaseInstallation[3], Color.DarkOliveGreen, 1);
#endif
				}
			}
            if (Properties.Settings.Default.AIScanFoldersP3DV4.Count < 1)
            {
                BaseInstallation[4] = FindInstallationPath("Lockheed Martin\\Prepar3D v4");
                if (BaseInstallation[4].Length > 0)
                {
                   Properties.Settings.Default.AIScanFoldersP3DV4.Add(BaseInstallation[4] + "SimObjects\\Airplanes");
                   Properties.Settings.Default.AIScanFoldersP3DV4.Add(BaseInstallation[4] + "SimObjects\\Rotorcraft");
                   Properties.Settings.Default.Save();
#if DEBUG
                   Log.LogMessage("AIMapping : Répertoire d'installation Prepar3D v4 trouvé :  = " + BaseInstallation[4], Color.DarkOliveGreen, 1);
#endif
                }
            }
        }

		private void OnSCReceiveOpen(object sender, SCManagerEventOpen e)
		{
			if (BaseInstallation[(uint)m_SC.GetVersion()] != "")
			{
				m_Table = m_SC.GetVersion().ToString();
				switch (m_SC.GetVersion())
				{
					case SIM_VERSION.FSX:
						m_AircraftPath = Properties.Settings.Default.AIScanFoldersFSX;
						break;
					case SIM_VERSION.FSX_STEAM:
						m_AircraftPath = Properties.Settings.Default.AIScanFoldersFSXSE;
						break;
					case SIM_VERSION.P3D_V2:
						m_AircraftPath = Properties.Settings.Default.AIScanFoldersP3DV2;
						break;
					case SIM_VERSION.P3D_V3:
						m_AircraftPath = Properties.Settings.Default.AIScanFoldersP3DV3;
                        break;
                    case SIM_VERSION.P3D_V4:
                        m_AircraftPath = Properties.Settings.Default.AIScanFoldersP3DV4;
                        break;
				}
#if DEBUG
				Log.LogMessage("AIMapping : Simulateur détecté: " + m_Table, Color.DarkOliveGreen, 1);
				Log.LogMessage("AIMapping : Répertoires des objets :", Color.DarkOliveGreen, 1);
				foreach (string folder in m_AircraftPath)
				{
					Log.LogMessage("AIMapping : " + folder, Color.DarkOliveGreen, 1);
				}
#endif
				InitSQLite();
				StartScanSimObject();
			}
		}


		/// <summary>
		/// Initialise la base de donnée des AI ou la met à jour
		/// </summary>
		private void InitSQLite()
		{
			try
			{
				m_DatabasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "\\AIDB.sqlite";
				m_DB = new SQLiteConnection("Data Source=" + m_DatabasePath + ";Version=3;");
				if (m_DB == null) return;
				m_DB.Open();
				SQLiteCommand cmd = m_DB.CreateCommand();
				// Recherche de colonnes manquante pour mise à jour
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "PRAGMA table_info(" + m_Table + ");";
				SQLiteDataReader r = cmd.ExecuteReader();
				List<string> Colonnes = new List<string>();
				if (r.StepCount > 0)
				{
					while (r.Read())
					{
						Colonnes.Add(r.GetString(1));
#if DEBUG
						Log.LogMessage("AIMapping : table_info :  " + r.GetString(1), Color.DarkOliveGreen, 2);
#endif
					}
					if (!Colonnes.Contains("Editeur"))
					{
#if DEBUG
						Log.LogMessage("AIMapping : Colonne Editeur manquante :  Création", Color.DarkOliveGreen, 2);
#endif
						cmd = m_DB.CreateCommand();
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = "DROP TABLE " + m_Table;
						cmd.ExecuteNonQuery();

					}
				}
				cmd = m_DB.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "create table if not exists " + m_Table + " ( checksum varchar(32), Titre varchar(256) collate nocase, Editeur varchar(100) collate nocase, Type varchar(30) collate nocase, Model varchar(10) collate nocase, Path varchar(256) collate nocase, Categorie varchar(20) collate nocase, NbMoteur int, TypeMoteur int, CGHeight real, Pitch real)";
				cmd.ExecuteNonQuery();
				cmd = m_DB.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "create table if not exists Remplacement (TitreDistant varchar(256) UNIQUE, TitreLocal varchar(256), Sim varchar(16))";
				cmd.ExecuteNonQuery();
				cmd = m_DB.CreateCommand();
				cmd.CommandType = CommandType.Text;
				m_Initialized = true;
				return;
			}
			catch (SQLiteException e)
			{
				Log.LogMessage("AIMapping : Erreur constructeur SQLite = " + e.Message, Color.DarkViolet);
				return;
			}
			catch (Exception e)
			{
				Log.LogMessage("AIMapping : Exception = " + e.Message, Color.DarkViolet);
				return;
			}
		}

		/// <summary>
		/// Thread de scan de mise à jours des objets AI dans la base de donnée
		/// </summary>
		public void StartScanSimObject()
		{
			if (m_AircraftPath.Count == 0) return;
			if (m_Thread != null)
			{
				if (m_Thread.IsAlive) m_Thread.Abort();
			}
			m_Thread = new Thread(ScanSimObject);
			m_Thread.Priority = ThreadPriority.Lowest;
			m_Thread.IsBackground = true;
			m_Thread.Start();
		}

		/// <summary>
		/// Scan des fichiers objets AI dans les répertoires renseignés
		/// </summary>
		private void ScanSimObject()
		{
			m_NbAdded = 0;
			m_NbUpdated = 0;
			Log.LogMessage("Début de la mise à jour BBD des appareils...");
			foreach (string Directory in m_AircraftPath)
			{
				DirectoryInfo di = new DirectoryInfo(Directory);
				MD5 md5 = MD5.Create();
				string Key;
				IEnumerable<FileInfo> Dir = null;
				try
				{
					Dir = di.GetFiles("*.cfg", SearchOption.AllDirectories).Where(s => s.Name.ToLower().StartsWith("aircraft") || s.Name.ToLower().StartsWith("sim"));
				}
				catch (Exception e)
				{
					Log.LogMessage("AIMapping : Exception lors de l'accès au répertoire SimObject :  " + e.Message, Color.DarkViolet);
					continue;
				}
				foreach (FileInfo d in Dir)
				{
					FileStream stream = File.OpenRead(d.FullName);
					byte[] checksum = md5.ComputeHash(stream);
					Key = BitConverter.ToString(checksum).Replace("-", "").ToLower();
					try
					{
						DBFileUpdate(Key, d.FullName);
					}
					catch (Exception e)
					{
						Log.LogMessage("AIMapping : Exception lors de l'ajout au dictonnaire du SimObject : " + e.Message + ", Fichier concerné :" + d.FullName, Color.DarkViolet);
					}
#if DEBUG
					Log.LogMessage("AIMapping : Scan du SimObject " + d.Directory + ", MD5=" + Key, Color.DarkOliveGreen, 2);
#endif
				}
			}
			Log.LogMessage("Fin de la mise à jour : " + m_NbAdded.ToString() + " Appareils ajoutés et " + m_NbUpdated.ToString() + " mis à jour.");
			DBFilePurge();
		}

		/// <summary>
		/// Mise à jour des enregistrement AI dans la base de donnée
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="Path"></param>
		private void DBFileUpdate(string Key, string Path)
		{
			if (!m_Initialized) return;
			try
			{
				SQLiteCommand cmd = m_DB.CreateCommand();
				SQLiteParameter sCheckSum = new SQLiteParameter("@checksum", Key);
				SQLiteParameter sPath = new SQLiteParameter("@path", Path);
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT checksum FROM " + m_Table + " WHERE checksum = @checksum";
				cmd.Parameters.Add(sCheckSum);
				SQLiteDataReader r = cmd.ExecuteReader();
				if (r.StepCount == 0)
				{
#if DEBUG
					Log.LogMessage("AIMapping : Aucun enregistrement trouvé pour " + Key, Color.DarkOliveGreen, 2);
#endif
					IniFile Parser = new IniFile(Path);
					string Editeur = Parser.IniReadValue("fltsim.0", "ui_createdby");
					string atc_model = Parser.IniReadValue("General", "atc_model");
					string atc_type = Parser.IniReadValue("General", "atc_type");
					string category = Parser.IniReadValue("General", "Category");
					string sEngine_Type = Parser.IniReadValue("GeneralEngineData", "engine_type");
					string sStatic_CG_Height = Parser.IniReadValue("contact_points", "static_cg_height");
					string sStatic_Pitch = Parser.IniReadValue("contact_points", "static_pitch");
					int Engine_Type = 0;
					double Static_CG_Height = 0;
					double Static_Pitch = 0;
					try
					{
						Engine_Type = Convert.ToInt32(sEngine_Type);
						if (sStatic_CG_Height.Length == 0) sStatic_CG_Height = "0.0";
						if (sStatic_Pitch.Length == 0) sStatic_Pitch = "0.0";
						Static_CG_Height = Convert.ToDouble(sStatic_CG_Height.Replace(',','.'), new CultureInfo("en-US"));
						Static_Pitch = Convert.ToDouble(sStatic_Pitch.Replace(',', '.'), new CultureInfo("en-US"));
					}
					catch (Exception e)
					{
						Log.LogMessage("AIMapping : DBFile Update exception = " + e.ToString(), Color.DarkViolet);
					}
#if DEBUG
					Log.LogMessage("AIMapping : Editeur = " + Editeur, Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : ATCModel = " + atc_model, Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : ATCType = " + atc_type, Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : Category = " + category, Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : Type de moteur = " + Engine_Type.ToString(), Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : Garde au sol = " + Static_CG_Height.ToString(), Color.DarkOliveGreen, 2);
					Log.LogMessage("AIMapping : Inclinaison au sol = " + Static_Pitch.ToString(), Color.DarkOliveGreen, 2);
#endif

					int nb_moteur = 0;
					while (true)
					{
						string Reponse = Parser.IniReadValue("GeneralEngineData", "Engine." + nb_moteur.ToString());
						if (Reponse != "")
						{
							nb_moteur++;
						}
						else break;
					}
#if DEBUG
					Log.LogMessage("AIMapping : Nombre de moteurs = " + nb_moteur.ToString(), Color.DarkOliveGreen, 2);
#endif
					int nb_title = 0;
					while (true)
					{
						string Title = Parser.IniReadValue("fltsim." + nb_title.ToString(), "title");
						if (Title != "")
						{
#if DEBUG
							Log.LogMessage("AIMapping : Variante trouvée Title = " + Title, Color.DarkOliveGreen, 2);
#endif
							nb_title++;
							SQLiteParameter sTitre = new SQLiteParameter("@titre", Title);
							SQLiteParameter sModel = new SQLiteParameter("@model", atc_model);
							SQLiteParameter sType = new SQLiteParameter("@type", atc_type);
							SQLiteParameter sEditeur = new SQLiteParameter("@editeur", Editeur);
							SQLiteParameter sCategorie = new SQLiteParameter("@categorie", category);
							SQLiteParameter sNbMoteur = new SQLiteParameter("@nbmoteur", nb_moteur);
							SQLiteParameter sTypeMoteur = new SQLiteParameter("@typemoteur", Engine_Type);
							SQLiteParameter sCGHeight = new SQLiteParameter("@cgheight", Static_CG_Height);
							SQLiteParameter sPitch = new SQLiteParameter("@pitch", Static_Pitch);
							// Recherche de Title identique pour mise à jour
							cmd = m_DB.CreateCommand();
							cmd.CommandType = CommandType.Text;
							cmd.CommandText = "SELECT * FROM " + m_Table + " WHERE Titre = @titre AND checksum = @checksum";
							cmd.Parameters.Add(sTitre);
							cmd.Parameters.Add(sCheckSum);
							r = cmd.ExecuteReader();
							if (r.StepCount == 0) // Si 0 enregistrement, alors il s'agit d'un nouvel appareil
							{
								cmd = m_DB.CreateCommand();
								cmd.CommandType = CommandType.Text;
								cmd.CommandText = "INSERT INTO " + m_Table + " VALUES (@checksum, @titre, @editeur, @type, @model, @path, @categorie, @nbmoteur, @typemoteur, @cgheight, @pitch)";
								cmd.Parameters.Add(sCheckSum);
								cmd.Parameters.Add(sTitre);
								cmd.Parameters.Add(sEditeur);
								cmd.Parameters.Add(sType);
								cmd.Parameters.Add(sModel);
								cmd.Parameters.Add(sPath);
								cmd.Parameters.Add(sCategorie);
								cmd.Parameters.Add(sNbMoteur);
								cmd.Parameters.Add(sTypeMoteur);
								cmd.Parameters.Add(sCGHeight);
								cmd.Parameters.Add(sPitch);
								cmd.ExecuteNonQuery();
								m_NbAdded++;
							}
							else //Mise à jour de l'enregistrement
							{
								cmd = m_DB.CreateCommand();
								cmd.CommandType = CommandType.Text;
								cmd.CommandText = "UPDATE " + m_Table + " SET checksum = @checksum, Editeur = @editeur, Type = @type, Model = @model , Path = @path, Categorie = @categorie, NbMoteur = @nbmoteur, TypeMoteur = @typemoteur, CGHeight = @cgheight, Pitch = @pitch WHERE Titre=@titre";
								cmd.Parameters.Add(sCheckSum);
								cmd.Parameters.Add(sTitre);
								cmd.Parameters.Add(sEditeur);
								cmd.Parameters.Add(sType);
								cmd.Parameters.Add(sModel);
								cmd.Parameters.Add(sPath);
								cmd.Parameters.Add(sCategorie);
								cmd.Parameters.Add(sNbMoteur);
								cmd.Parameters.Add(sTypeMoteur);
								cmd.Parameters.Add(sCGHeight);
								cmd.Parameters.Add(sPitch);
								cmd.ExecuteNonQuery();
								m_NbUpdated++;
							}
						}
						else break;
					}
				}
				else
				{
#if DEBUG
					Log.LogMessage("AIMapping : " + r.StepCount.ToString() + " enregistrement trouvés pour " + Key, Color.DarkOliveGreen, 2);
#endif
				}
			}
			catch (SQLiteException e)
			{
				Log.LogMessage("AIMapping : Erreur SQLite = " + e.Message, Color.DarkViolet);
				return;
			}
		}

		/// <summary>
		/// Résolution d'un AI avec la base de donnée
		/// </summary>
		/// <param name="Title"></param>
		/// <param name="Model"></param>
		/// <param name="Type"></param>
		/// <returns></returns>
		public AIResol SolveTitle(string Title, string Model, string Type)
		{
			//Avion de substitution
			AIResol Resol = new AIResol();
			Resol.Titre = "Mooney Bravo";
			Resol.CG_Height = 3.7;
			Resol.Pitch = 2.9;
			if (!m_Initialized) return Resol;
			// On vérifie les règles locales
			string Rule = GetRule(Title);
			if (Rule != "")
			{
#if DEBUG
				Log.LogMessage("AIMapping : Règle locale trouvée dans la DB : " + Title + " Remplacé par " + Rule, Color.DarkOliveGreen, 2);
#endif
				Title = Rule;
			}
			// On commence par vérifier si le Title existe dans la DB
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Titre,CGHeight,Pitch FROM " + m_Table + " WHERE Titre = @titre COLLATE NOCASE";
			cmd.Parameters.Add(new SQLiteParameter("@titre", Title));
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Resol.Titre = r.GetString(0);
					Resol.CG_Height = r.GetDouble(1);
					Resol.Pitch = r.GetDouble(2);
#if DEBUG
					Log.LogMessage("AIMapping : Occurance directe trouvée dans la DB : " + Resol.Titre, Color.DarkOliveGreen, 2);
#endif
				}
			}
			else
			{
				cmd = m_DB.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT Titre,CGHeight,Pitch FROM " + m_Table + " WHERE Model  like @model";
				cmd.Parameters.Add(new SQLiteParameter("@model", Model));
				r = cmd.ExecuteReader();
				if (r.StepCount > 0)
				{
					while (r.Read())
					{
						Resol.Titre = r.GetString(0);
						Resol.CG_Height = r.GetDouble(1);
						Resol.Pitch = r.GetDouble(2);
#if DEBUG
						Log.LogMessage("AIMapping : Occurance ATC Model trouvée dans la DB : " + Resol.Titre, Color.DarkOliveGreen, 2);
#endif
					}
				}
			}
			return Resol;
		}

		public List<string> GetAITitreDispo(string Categorie = "",string Editeur = "", string Type="", string Model="")
		{
			List<string> Liste = new List<string>();
			if (!m_Initialized) return Liste;
			bool FiltresEnable = false;
			int nbFiltre = 0;
			if (Categorie == "*") Categorie = "";
			if (Editeur == "*") Editeur = "";
			if (Type == "*") Type = "";
			if (Model == "*") Model = "";
			if (
				Categorie.Length > 0 ||
				Editeur.Length > 0 ||
				Type.Length > 0 ||
				Model.Length > 0)
				FiltresEnable = true;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Titre FROM " + m_Table;
			if (FiltresEnable)
			{
				cmd.CommandText += " WHERE ";
				if (Categorie.Length>0)
				{
					nbFiltre++;
					cmd.Parameters.Add(new SQLiteParameter("@categorie", Categorie));
					cmd.CommandText += " Categorie like @categorie";
				}
				if (Editeur.Length>0)
				{
					if (nbFiltre>0) cmd.CommandText += " and ";
					nbFiltre++;
					cmd.Parameters.Add(new SQLiteParameter("@editeur", Editeur));
					cmd.CommandText += " Editeur like @editeur";
				}
				if (Type.Length > 0)
				{
					if (nbFiltre > 0) cmd.CommandText += " and ";
					nbFiltre++;
					cmd.Parameters.Add(new SQLiteParameter("@type", Type));
					cmd.CommandText += " Type like @type";
				}
				if (Model.Length > 0)
				{
					if (nbFiltre > 0) cmd.CommandText += " and ";
					nbFiltre++;
					cmd.Parameters.Add(new SQLiteParameter("@model", Model));
					cmd.CommandText += " Model like @model";
				}
			}
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Liste.Add(r.GetString(0));
				}
			}
			return Liste;
		}

		/// <summary>
		/// Retourn la liste des catégories enregistrées
		/// </summary>
		/// <returns></returns>
		public List<string> GetCategoryList()
		{
			List<string> Liste = new List<string>();
			if (!m_Initialized) return Liste;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Distinct Categorie FROM " + m_Table;
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Liste.Add(r.GetString(0));
				}
			}
			return Liste;
		}

		public List<string> GetEditeurList()
		{
			List<string> Liste = new List<string>();
			if (!m_Initialized) return Liste;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Distinct Editeur FROM " + m_Table;
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Liste.Add(r.GetString(0));
				}
			}
			return Liste;
		}

		public List<string> GetATCTypeList()
		{
			List<string> Liste = new List<string>();
			if (!m_Initialized) return Liste;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Distinct Type FROM " + m_Table;
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Liste.Add(r.GetString(0));
				}
			}
			return Liste;
		}

		public List<string> GetATCModelList()
		{
			List<string> Liste = new List<string>();
			if (!m_Initialized) return Liste;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT Distinct Model FROM " + m_Table;
			SQLiteDataReader r = cmd.ExecuteReader();
			if (r.StepCount > 0)
			{
				while (r.Read())
				{
					Liste.Add(r.GetString(0));
				}
			}
			return Liste;
		}

		/// <summary>
		/// Retourne le titre de remplacement mémorisée pour un titre distant
		/// </summary>
		/// <param name="pTitre"></param>
		/// <returns></returns>
		public string GetRule(string pTitre)
		{
			if (!m_Initialized) return "";
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT TitreLocal FROM Remplacement WHERE (TitreDistant = @titre) and (Sim = @sim)";
			cmd.Parameters.Add(new SQLiteParameter("@titre", pTitre));
			cmd.Parameters.Add(new SQLiteParameter("@sim", m_Table));
			SQLiteDataReader r = cmd.ExecuteReader();
			while (r.Read())
			{
				return r.GetString(0);
			}
			return "";
		}

		public void AddRule(string pRemoteTitre, string pLocalTitre)
		{
			if (!m_Initialized) return;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "INSERT OR REPLACE INTO Remplacement (TitreDistant, TitreLocal, Sim) VALUES ( @titredistant, @titrelocal, @sim)";
			cmd.Parameters.Add(new SQLiteParameter("@titredistant", pRemoteTitre));
			cmd.Parameters.Add(new SQLiteParameter("@titrelocal", pLocalTitre));
			cmd.Parameters.Add(new SQLiteParameter("@sim", m_Table));
			cmd.ExecuteNonQuery();
		}

		public void DelRule(string pRemoteTitre)
		{
			if (!m_Initialized) return;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "DELETE FROM Remplacement WHERE (TitreDistant = @titredistant) and (Sim = @sim)";
			cmd.Parameters.Add(new SQLiteParameter("@titredistant", pRemoteTitre));
			cmd.Parameters.Add(new SQLiteParameter("@sim", m_Table));
			cmd.ExecuteNonQuery();
		}

		public void CleanAIDB()
		{
			if (!m_Initialized) return;
			SQLiteCommand cmd = m_DB.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "DELETE FROM " + m_Table;
			cmd.ExecuteNonQuery();
		}

		public Dictionary<string, string> GetList
		{
			get
			{
				Dictionary<string, string> md5mlist = new Dictionary<string, string>();
				if (m_Initialized)
				{
					SQLiteCommand cmd = m_DB.CreateCommand();
					cmd.CommandText = "SELECT DISTINCT checksum,Path FROM " + m_Table;
					SQLiteDataReader r = cmd.ExecuteReader();
					while (r.Read())
					{
						md5mlist.Add(r.GetString(0), r.GetString(1));
					}
				}
				return md5mlist;
			}
		}

		/// <summary>
		/// Renvoie le chemin du fichier AI avec son checksum
		/// </summary>
		/// <param name="CheckSum"></param>
		/// <returns></returns>
		public string GetPath(string CheckSum)
		{
			string rep = "";
			if (m_Initialized)
			{
				SQLiteCommand cmd = m_DB.CreateCommand();
				cmd.CommandText = "SELECT DISTINCT Path FROM " + m_Table + " WHERE checksum = @CheckSum";
				cmd.Parameters.Add(new SQLiteParameter("@CheckSum", CheckSum));
				SQLiteDataReader r = cmd.ExecuteReader();
				while (r.Read())
				{
					rep = r.GetString(0);
					break;
				}
			}
			return rep;
		}

		/// <summary>
		/// Récupère le contenu d'un fichier AI avec son identifiant DB
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetFichierData(string key)
		{
			if (!m_Initialized) return "";
			string Fichier = GetPath(key);
			if (Fichier != "")
			{
				try
				{
					FileStream stream = File.OpenRead(Fichier);
					MemoryStream output = new MemoryStream();
					int length = Convert.ToInt32(stream.Length);
					Byte[] Tampon = new Byte[length];
					stream.Read(Tampon, 0, length);
					DeflateStream Compress = new DeflateStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, true);
					Compress.Write(Tampon, 0, length);
					Compress.Close();
					return Convert.ToBase64String(output.ToArray());
				}
				catch (Exception e)
				{
					Log.LogMessage("AIMapping : Exception lors de l'ouverture du fichier  : " + e.Message, Color.DarkViolet);
				}

			}
			return "";
		}

		/// <summary>
		/// Purge les AI supprimés ou inéxistants 
		/// </summary>

		private void DBFilePurge()
		{
			if (!m_Initialized) return;
#if DEBUG
			Log.LogMessage("AIMapping : Début de la purge automatique...", Color.DarkOliveGreen, 1);
#endif
			try
			{
				SQLiteCommand cmd = m_DB.CreateCommand();
				cmd.CommandText = "SELECT DISTINCT Path FROM " + m_Table;
				SQLiteDataReader r = cmd.ExecuteReader();
				while (r.Read())
				{
					if (!File.Exists(r.GetString(0)))
					{
#if DEBUG
						Log.LogMessage("AIMapping : suprresion de la DB du fichier  : " + r.GetString(0), Color.DarkOliveGreen, 2);
#endif
						cmd = m_DB.CreateCommand();
						cmd.CommandText = "DELETE FROM " + m_Table + " WHERE Path = @path";
						cmd.Parameters.Add(new SQLiteParameter("@path", r.GetString(0)));
						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (SQLiteException e)
			{
				Log.LogMessage("AIMapping : Erreur SQLite = " + e.Message, Color.DarkViolet);
				return;
			}
#if DEBUG
			Log.LogMessage("AIMapping : Fin de la purge automatique.", Color.DarkOliveGreen, 1);
#endif
		}
	}
}
