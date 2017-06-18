using System;
using System.Windows;
using System.Xml;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace ffs2play
{
	public partial class PirepManager
	{
        private void HelloCB(HttpWebResponse Response)
        {
            XmlDocument xmlDoc = null;
            if (Response != null)
            {
                if (Response.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogMessage("Erreur de connexion : " + Response.StatusDescription, Color.DarkViolet);
                }
                xmlDoc = GetResultXml(ref Response);
                Response.Close();
            }
            if (xmlDoc == null)
            {
                if (m_btnConnect != null) m_btnConnect.Invoke(new Action(() =>
                {
                    m_btnConnect.Enabled = true; ;
                }));
                Log.LogMessage("Erreur de connexion", Color.DarkViolet);
                return;
            }
            //Test de la licence
            XmlNode Node = GetFirstElement(ref xmlDoc, "certificat");
            if (Node != null)
            {
                RSAtoPHPCryptography rsa = new RSAtoPHPCryptography();
                rsa.LoadCertificateFromString(Node.InnerText);
                Crypted_AESKey = Convert.ToBase64String(rsa.Encrypt(Convert.FromBase64String(m_sAESKey)));
                XmlBuild(ref SendVerify, XmlCode.verify);
#if DEBUG
                Log.LogMessage("PManager: AES Key encrypted = " + Crypted_AESKey,Color.DarkBlue,1);
                Log.LogMessage("PManager: Demande de connexion =\n " + Beautify(SendVerify), Color.DarkRed, 2);
#endif
                HTTPRequestThread Requete = new HTTPRequestThread(SendVerify, m_sURL, ConnectCB);
                Requete.Start();

            }
        }
        /// <summary>
        /// Callback Demande de connexion
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConnectCB(HttpWebResponse Response)
		{
			if (m_btnConnect != null) m_btnConnect.Invoke(new Action(() =>
			{
				m_btnConnect.Enabled = true; ;
			}));
			XmlDocument xmlDoc = null;
			if (Response != null)
			{
				if (Response.StatusCode != HttpStatusCode.OK)
				{
					Log.LogMessage("Erreur de connexion : " + Response.StatusDescription, Color.DarkViolet);
				}
				xmlDoc = GetResultXml(ref Response);
				Response.Close();
			}
			if (xmlDoc == null)
			{
				Log.LogMessage("Erreur de connexion", Color.DarkViolet);
				return;
			}
			bool bError = CheckError(xmlDoc);
            XmlNode Node = GetFirstElement(ref xmlDoc, "loginStatus");
			if (Node != null)
			{
				if (Node.InnerText == "1")
				{
					m_bConnected = true;
				}
				else m_bConnected = false;
				if (m_bConnected && (!bError))
				{
					Log.LogMessage("ffs2play est connecté au serveur");
					XmlNode xmlExternalIP = GetFirstElement(ref xmlDoc, "your_ip");
					if (xmlExternalIP != null)
					{
                        IPAddress ExternalIP = IPAddress.Parse("0.0.0.0");
                        if (IPAddress.TryParse(xmlExternalIP.InnerText, out ExternalIP))
                        {
#if DEBUG
                            Log.LogMessage("PManager: Reçu IP Exterieure = " + ExternalIP.ToString(), Color.DarkBlue, 2);
#endif
                        }
                        else
                        {
#if DEBUG
                            Log.LogMessage("PManager: Reçu IP Exterieure invalide", Color.DarkViolet, 2);
#endif
                        }
                        P2P.ExternalIP = ExternalIP;
                    }
					else
					{
						Log.LogMessage("Le serveur n'a pas retourné votre IP extérieure");
						m_bConnected = false;
						return;
					}

					XmlNode xmlKey = GetFirstElement(ref xmlDoc, "key");
					if (xmlKey != null)
					{
						Key = xmlKey.InnerText;
						OnLiveUpdate(null, null);
					}
					else
					{
						Log.LogMessage("Le serveur n'a pas retourné de clé d'authentification");
					}

					XmlNode xmlAtc = GetFirstElement(ref xmlDoc, "atc");
					if (xmlAtc != null)
					{
						if (xmlAtc.InnerText != "")
						{
							if (Convert.ToBoolean(xmlAtc.InnerText)) ATCTimer.Start();
						}
					}

					XmlNode AIManagement = GetFirstElement(ref xmlDoc, "AI_Management");
					if (AIManagement != null)
					{
						if (AIManagement.InnerText !="")
							m_AIManagement = Convert.ToBoolean(AIManagement.InnerText);
					}

					// Passage du fond du bouton en vert
					m_btnConnect.Invoke(new Action(() =>
					{
						m_btnConnect.ImageIndex = 1;
						ButtonsTips.SetToolTip(m_btnConnect, "Déconnexion du serveur");
					}));
					// On charge la liste des joueurs depuis le wazzup
					XmlNode Wazzup = GetFirstElement(ref xmlDoc, "whazzup");
					if (Wazzup != null)
					{
						P2P.Wazzup_Update(Wazzup,m_sAESKey);
					}
				}
				else
				{
					Log.LogMessage("Le serveur a refusé la connexion");
					return;
				}
			}
		}

		/// <summary>
		/// Callback Requete de mise à jour
		/// </summary>
		/// <param name="asyncResult"></param>
		private void LiveUpdateCB(HttpWebResponse Response)
		{
            // Si pas connecté au serveur on ignore le CB
			if (!m_bConnected) return;
			XmlDocument xmlDoc=null;
            // On récupère le contenu XML
			if (Response != null)
			{
				xmlDoc = GetResultXml(ref Response);
				Response.Close();
			}
            // Si le contenu est vide on ignore le CB
			if (xmlDoc == null) return;

			LastGoodUpdate = Outils.Now;
            //On vérifie les erreurs retournées par le serveur
			if (CheckError(xmlDoc))
            {
                Disconnect();
                return;
            }
			// On charge la liste des joueurs depuis le wazzup
			XmlNode Wazzup = GetFirstElement(ref xmlDoc, "whazzup");
			if (Wazzup != null)
			{
				P2P.Wazzup_Update(Wazzup,m_sAESKey);
			}
            //On synchronise le serveur avec les AI disponibles
			if ((!m_SyncAIDone) && m_AIManagement)
			{
				if (!Mapping.IsInit) return;
				XmlBuild(ref SendSyncAI, XmlCode.syncai);
				XmlNodeList Nodes = SendSyncAI.GetElementsByTagName("syncai");
				foreach (XmlNode node in Nodes)
				{
					foreach (XmlNode child in node.ChildNodes)
					{
						switch (child.Name)
						{
							case "md5list":
								child.InnerText = Outils.PhpSerialize(Mapping.GetList.Keys);
								break;
						}
					}
				}
				HTTPRequestThread Requete = new HTTPRequestThread(SendSyncAI, m_sURL, SyncAICB);
				Requete.Start();
#if DEBUG
				Log.LogMessage("PManager: Synchronisation des AI : \n" + Beautify(SendSyncAI), Color.DarkRed, 2);
#endif
			}
            // On check la méteo
            XmlNode Metar = GetFirstElement(ref xmlDoc, "metar");
            if (Metar != null)
            {
                MetarUpdate(ref Metar);
            }
		}

		/// <summary>
		/// Callback Requete de mise à jour ATC
		/// </summary>
		/// <param name="asyncResult"></param>
		private void ATCCB(HttpWebResponse Response)
		{
			XmlDocument xmlDoc = null;
			if (Response != null)
			{
				xmlDoc = GetResultXml(ref Response);
				Response.Close();
			}
		}

		/// <summary>
		/// Gestion d'un eventuel message d'erreur
		/// </summary>
		/// <param name="asyncResult"></param>
		/// <summary>

		private bool CheckError(XmlDocument xmlDoc)
		{
			if (xmlDoc != null)
			{
				XmlNode Node = GetFirstElement(ref xmlDoc, "erreur");
				if (Node != null)
				{
					int Level = Convert.ToInt32(Node.Attributes["error"].Value);
					Color Couleur = Color.Black;
					switch (Level)
					{
						case 0:
							Couleur = Color.Black;
							break;
						case 1:
							Couleur = Color.DarkRed;
							System.Media.SystemSounds.Beep.Play();
							break;
						case 2:
							Couleur = Color.DarkRed;
							Disconnect();
							break;
					}
					Log.LogMessage("Serveur : " + Node.InnerText, Couleur);
					if (Level >= 2) return true;
				}
			}
			return false;
		}


		/// Callback Déconnexion du serveur
		/// </summary>
		/// <param name="asyncResult"></param>
		private void DisconnectCB(HttpWebResponse Response)
		{
			if (m_btnConnect != null) m_btnConnect.Invoke(new Action(() =>
			{
				m_btnConnect.Enabled = true; ;
			}));
			XmlDocument xmlDoc = null;
			if (Response != null)
			{
				xmlDoc = GetResultXml(ref Response);
				Response.Close();
			}
			m_bConnected = false;
			m_SyncAIDone = false;
			m_AIManagement = false;
			Key = "";
			Log.LogMessage("Déconnecté du serveur");
			m_btnConnect.Invoke(new Action(() =>
			{
				m_btnConnect.ImageIndex = 0;
				ButtonsTips.SetToolTip(m_btnConnect, "Connexion au serveur");
				P2P.Clear();
				if (DelayedExit)
				{
					System.Windows.Forms.Application.Exit();
					DelayedExit = false;
				}
				else if (DelayReco)
				{
					m_btnConnect.Invoke(new Action(() => { m_btnConnect.PerformClick(); }));
					DelayReco = false;
				}
			}));
		}
		/// <summary>
		/// Callback Requete de mise à jour
		/// </summary>
		/// <param name="asyncResult"></param>
		private void SyncAICB(HttpWebResponse Response)
		{
			if (!m_bConnected) return;
			Dictionary<string, string> Fichiers = new Dictionary<string, string>();
			XmlDocument xmlDoc = null;
			if (Response != null)
			{
				xmlDoc = GetResultXml(ref Response);
				Response.Close();
			}
			if (xmlDoc != null)
			{
				CheckError(xmlDoc);
				XmlNode Node = GetFirstElement(ref xmlDoc, "no_found");
				if (Node != null)
				{
					List<string> Manquants = Outils.PhpDeSerialize(Node.InnerText);
					foreach (string item in Manquants)
					{
#if DEBUG
						Log.LogMessage("PManager: Fichier manquant :" + item + " Fichier :" + Mapping.GetPath(item), Color.DarkBlue, 1);
#endif
						Fichiers.Add(item, Mapping.GetFichierData(item));
						break;
					}
					if (Fichiers.Count == 0) return;
					XmlBuild(ref SendAI, XmlCode.sendai);
					XmlNode DNode = GetFirstElement(ref SendAI, "sendai");
					foreach (KeyValuePair<string, string> item in Fichiers)
					{
						XmlNode Fichier = SendAI.CreateElement(string.Empty, "MD5_" + item.Key, string.Empty);
						Fichier.InnerText = item.Value;
						DNode.AppendChild(Fichier);
					}
					HTTPRequestThread Requete = new HTTPRequestThread(SendAI, m_sURL, SyncAICB);
					Requete.Start();
#if DEBUG
					Log.LogMessage("PManager: Envois des AI : \n" + Beautify(SendAI), Color.DarkRed, 2);
#endif
				}
			}
			m_SyncAIDone = true;
		}
	}
}
