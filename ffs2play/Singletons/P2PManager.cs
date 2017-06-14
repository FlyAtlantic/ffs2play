using System;
using System.Drawing;
using System.Net;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Forms;
using Open.Nat;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using ProtoBuf;
using System.IO;

namespace ffs2play
{
    class P2PManager
    {
		private ListViewItem m_Item;
		private List<Peer> Peers;
        private Logger Log;
		private AIMapping m_Mapper;
        private bool m_bUPNP;
        private int m_Port;
		private IPAddress m_ExternalIP;
		private List<IPAddress> m_InternalIP;
		private UDPServer Server;
		private RichTextBox m_rtbLogChat;
		private ListViewEx m_lvUsers;
		private ListViewEx m_lvPeerDetail;
		private NatDiscoverer Discoverer;
		private NatDevice Device;
		private System.Timers.Timer CheckAI;

		/// <summary>
		/// Retourne l'instance du singleton
		/// </summary>
		public static P2PManager Instance
        {
            get
            {
                return Nested.instance;
            }
        }

		public int Port
		{
			get
			{
				return m_Port;
			}
		}

		public bool Shadow
		{
			get
			{
				return Properties.Settings.Default.ShadowEnable;
			}
		}

		public string LocalIPSerialized
		{
			get
			{
                List<string> sInternalIP = new List<string>();
                foreach (IPAddress ip in m_InternalIP)
                {
                    sInternalIP.Add(ip.ToString());
                }
                MemoryStream Data = new MemoryStream();
                Serializer.Serialize(Data,sInternalIP);
                return Convert.ToBase64String(Data.ToArray());

            }
		}

		public IPAddress ExternalIP
		{
			get { return m_ExternalIP; }
			set { m_ExternalIP = value; }
		}

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

            internal static readonly P2PManager instance = new P2PManager();
        }

		/// <summary>
		/// Constructeur du Singleton
		/// </summary>
        private P2PManager()
        {
            Log = Logger.Instance;
#if DEBUG
            Log.LogMessage("P2P Manager : Démarré", Color.Blue,1 );
#endif
			m_Mapper = AIMapping.Instance;
			Server = new UDPServer();
			Peers = new List<Peer>();
			Discoverer = new NatDiscoverer();
			m_InternalIP = new List<IPAddress>();
			CheckAI = new System.Timers.Timer(5000);
			CheckAI.Elapsed += CheckAI_Elapsed;
            CheckPropertiesData();
            try
			{
				// récupération du pointeur sur le contrôle liste des utilisateurs P2P
				if ((m_lvUsers = ffs2play.getControl("lvUsers") as ListViewEx) == null)
					throw new InvalidOperationException("La liste lvUsers n'existe pas");
				// récupération du pointeur sur le contrôle liste des détails Peer
				if ((m_lvPeerDetail = ffs2play.getControl("lvPeerDetail") as ListViewEx) == null)
					throw new InvalidOperationException("La liste lvPeerDetail n'existe pas");
				// récupération du pointeur sur le contrôle liste des utilisateurs P2P
				if ((m_rtbLogChat = ffs2play.getControl("rtbLogChat") as RichTextBox) == null)
					throw new InvalidOperationException("La zone de tchat rtbLogChat n'existe pas");
			}
			catch (Exception e)
			{
				Log.LogMessage("P2P Manager : Erreur Constructeur = " + e.Message, Color.DarkViolet);
			}
			try
			{
				// Récupératgion de l'adresse IP locale
				IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (IPAddress ip in host.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						m_InternalIP.Add(ip);
#if DEBUG
						Log.LogMessage("P2PManager: IP locale trouvée  = " + ip.ToString(), Color.DarkBlue, 1);
#endif
					}
				}
				if (m_InternalIP.Count == 0) throw new Exception("IP Locale introuvable");
			}
			catch (Exception e)
			{
				Log.LogMessage("P2P Manager : Erreur Constructeur = " + e.Message, Color.DarkViolet);
			}
			m_lvUsers.SelectedIndexChanged += M_lvUsers_SelectedIndexChanged;
			m_lvUsers.MouseClick += M_lvUsers_MouseClick;
			m_lvUsers.ItemSelectionChanged += M_lvUsers_ItemSelectionChanged;
		}

		/// <summary>
		/// Vérification périodique de la limitation d'AI
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckAI_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			int NbInvisible = 0;
			int NbVisible = 0;
			int Limite = Properties.Settings.Default.P2PAILimite;
			int PosLointain = 0;
			double ScoreLointain = -1;
			int PosPret = 0;
			double ScorePret=double.MaxValue;
			for (int i=0; i<Peers.Count; i++)
			{
				if (Peers[i].Spawnable)
				{
					if (Peers[i].Visible)
					{
						NbVisible++;
						if (Peers[i].Distance > ScoreLointain)
						{
							ScoreLointain = Peers[i].Distance;
							PosLointain = i;
						}
					}
					else
					{
						NbInvisible++;

						if (Peers[i].Distance < ScorePret)
						{
							ScorePret = Peers[i].Distance;
							PosPret = i;
						}
					}
				}
			}
			// Si on en a plus de visible que la limite alors on en choisi un pour le rendre invisible (les plus lointains)
			if (NbVisible > Limite)
			{
				//On rend le plus lointain invisible
				Peers[PosLointain].Visible = false;
#if DEBUG
				Log.LogMessage("P2PManager: Peer " + Peers[PosLointain].CallSign + " rendu invisible", Color.DarkBlue, 1);
#endif
			}
			//Sinon , si on a des invisibles et que le nombre de visibles est inférieur à la limite
			//cela signifie qu'on a des invisibles qu'on peut afficher (les plus prêt)
			else if ((NbInvisible > 0) && (NbVisible < Limite))
			{
				Peers[PosPret].Visible = true;
#if DEBUG
				Log.LogMessage("P2PManager: Peer " + Peers[PosLointain].CallSign + " rendu visible", Color.DarkBlue, 1);
#endif
			}

		}

		private void M_lvUsers_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			foreach (Peer Pair in Peers)
			{
				if (Pair.CallSign == e.Item.Text)
				{
					Pair.Selected = e.IsSelected;
				}
			}
		}

		private void M_lvUsers_MouseClick(object sender, MouseEventArgs e)
		{
			if (m_lvUsers.FocusedItem.Bounds.Contains(e.Location))
			{
				m_Item = m_lvUsers.GetItemAt(e.X, e.Y);
				if (m_Item != null)
				{
					Peer Pair = Peers.Find(x => x.CallSign == m_Item.Text);
					if ((e.Button == MouseButtons.Right) && (Pair != null))
					{
						ContextMenu contextMenu = new ContextMenu();
						if (Pair.Spawned>=3)
						{
							MenuItem ResolMenuItem = new MenuItem("Remplacement AI");
							ResolMenuItem.Click += ResolMenuItem_Click;
							contextMenu.MenuItems.Add(ResolMenuItem);
						}
						MenuItem UserMenuItem = new MenuItem("Masquer");
						if (Properties.Settings.Default.P2PBlackList.Contains(m_Item.Text)) UserMenuItem.Checked = true;
						UserMenuItem.Click += UserMenuItem_Click;
						contextMenu.MenuItems.Add(UserMenuItem);
						m_lvUsers.ContextMenu = contextMenu;
					}
				}
			}
		}

		/// <summary>
		/// Ouvre la boite de dialogue de remplacement d'AI
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ResolMenuItem_Click(object sender, EventArgs e)
		{
			Peer Pair = Peers.Find(x => x.CallSign == m_Item.Text);
			dlgResolAI ResolAI = new dlgResolAI(Pair.Titre);
			if (ResolAI.ShowDialog() == DialogResult.OK)
			{
				Pair.ResetAI();
			}
		}

		/// <summary>
		/// Créer le menu contextuel du paire
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserMenuItem_Click(object sender, EventArgs e)
		{
			ListViewItem User = m_lvUsers.SelectedItems[0];
			Peer Pair = Peers.Find(x => x.CallSign == User.Text);
			if (IsDisabled(User.Text))
			{
				Properties.Settings.Default.P2PBlackList.Remove(User.Text);
				Pair.Disabled = false;
			}
			else
			{
				Properties.Settings.Default.P2PBlackList.Add(User.Text);
				Pair.Disabled = true;
			}
			Properties.Settings.Default.Save();
		}

		private void M_lvUsers_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePeerDetail();
		}

		/// <summary>
		/// Initialisation ou arrêt du manager
		/// </summary>
		/// <param name="Flag"></param>
		public void Init (bool Flag)
		{
			if (Flag)
			{
				m_Port = Properties.Settings.Default.P2PPort;
				Server.Port = m_Port;
				m_bUPNP = false;
				try
				{
					Server.Start();
				}
				catch (Exception e)
				{
					Log.LogMessage("Impossible d'initialiser le serveur UDP : " + e.Message, Color.DarkViolet);
				}
				//On test la fonctionnalité UPNP
				if (Properties.Settings.Default.UPNPEnable)
				{
					InitUPNP(true);
				}
				CheckAI.Start();
			}
			else
			{
				CheckAI.Stop();
				Server.Stop();
				InitUPNP(false);
				Clear();
			}
		}

		/// <summary>
		/// Gestion de l'ouverture automatique des ports UDP pour le P2P
		/// </summary>
		/// <param name="flag"></param>
		private void InitUPNP(bool flag)
        {
			if (flag)
			{
				if (m_bUPNP) return;
#if NET45
				var t = Task.Run(async () =>
				{
#endif
				var nat = new NatDiscoverer();
				var cts = new CancellationTokenSource();
#if NET45
                    try
                    {
					Device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
#else
				cts.CancelAfter(3000);
				Device = null;
				Task<NatDevice> Discoverer = nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
				try
				{
					Discoverer.Wait();
#endif

#if DEBUG
					Log.LogMessage("P2PManager: Routeur UPNP détecté", Color.DarkBlue, 1);
#endif
#if NET45
                        await Device.CreatePortMapAsync(new Mapping(Open.Nat.Protocol.Udp, m_Port, m_Port, 0, "ffs2playP3D"));
#else
					Device = Discoverer.Result;
					Task OpenPort = Device.CreatePortMapAsync(new Mapping(Open.Nat.Protocol.Udp, m_Port, m_Port, 0, "ffs2play"));
					OpenPort.Wait();
#endif
#if DEBUG
					Log.LogMessage("P2PManager: Ouverture du port UPNP ok", Color.DarkBlue, 1);
#endif
					}
					catch (AggregateException ae)
					{
						ae.Handle((x) =>
						{
							if (x is NatDeviceNotFoundException)
							{
								Log.LogMessage("P2PManager: Routeur UPNP introuvable", Color.DarkViolet);
								return true;
							}
							if (x is MappingException)
							{
								Log.LogMessage("P2PManager: Erreur lors de l'ouverture du port " + m_Port.ToString() + " : " + x.Message, Color.DarkViolet);
								return true;
							}
							return false;
						});
					}
					m_bUPNP = true;
#if NET45
                });
#endif
			}
			else
			{
				if ((!m_bUPNP) || (Device==null)) return;
#if NET45
                var t = Task.Run(async () =>
				{
#endif
				try
				{
#if NET45
                        await Device.DeletePortMapAsync(new Mapping(Open.Nat.Protocol.Udp, m_Port, m_Port));
#else
					Task ClosePort = Device.DeletePortMapAsync(new Mapping(Open.Nat.Protocol.Udp, m_Port, m_Port));
					ClosePort.Wait();
#endif
#if DEBUG
					Log.LogMessage("P2PManager: Fermeture du port UPNP ok", Color.DarkBlue, 1);
#endif
				}
				catch (AggregateException ae)
				{
					ae.Handle((x) =>
					{
						if (x is MappingException)
						{
							Log.LogMessage("P2PManager: Erreur lors de la fermetur du port", Color.DarkViolet);
							return true;
						}
						return false;
					});
				}
				m_bUPNP = false;
#if NET45
                });
#endif
			}
		}

		/// <summary>
		/// Test si un utilisateur est blacklisté
		/// </summary>
		/// <param name="pCallSign"></param>
		/// <returns></returns>
		private bool IsDisabled (string pCallSign)
		{
			if (Properties.Settings.Default.P2PBlackList.Count > 0)
			{
				if (Properties.Settings.Default.P2PBlackList.Contains(pCallSign)) return true;
			}
			return false;
		}


		/// <summary>
		/// Envoi un message teste sur le P2P
		/// </summary>
		/// <param name="Message"></param>
		public void Send_Tchat (string Message)
		{
			foreach (Peer p in Peers)
			{
				p.SendChat(Message);
			}
		}

		/// <summary>
		/// Mise à jour de l'annuaire des peers connecté sur le serveur web
		/// </summary>
		/// <param name="Liste"></param>
		public void Wazzup_Update(XmlNode Liste, string pAESKey)
		{
			//Détection Shadow
			string UserName = PirepManager.Instance.Get_UserName();
			UserName = UserName.Replace('[', '_');
			UserName = UserName.Replace(']', '_');
			UserName = UserName.ToLower();
			foreach (XmlNode Child in Liste.ChildNodes)
			{
				bool Shadow = false;
				bool Disabled = IsDisabled(Child.Name);
				//Détection Shadow
				if (UserName==Child.Name.ToLower())
				{
#if DEBUG
                    if (Properties.Settings.Default.ShadowEnable) Shadow = true;
                    else
#endif
                        continue;
				}
				IPAddress IP = IPAddress.Parse("0.0.0.0");
				int Port = 0;
				List<IPAddress> Local_IP=new List<IPAddress>();
				//Le partenaire est il déjà présent dans la liste?
				Peer test = Peers.Find(x => x.CallSign == Child.Name);
				//Si non, on le crée
				if (test == null)
				{
					foreach (XmlAttribute Item in Child.Attributes)
					{
						switch (Item.Name)
						{
							case ("ip"):
								{
                                    if (Shadow) IP = IPAddress.Parse("127.0.0.1");
                                    else
                                    {
                                        string sIP = Outils.DecryptMessage(Item.Value, pAESKey);
                                        if (!IPAddress.TryParse(sIP,out IP))
                                        {
                                            Log.LogMessage("P2P Manager : Adresse IP invalide", Color.DarkViolet);
                                            IP = IPAddress.Parse("0.0.0.0");
                                        }
                                    }
									break;
								}
							case ("port"):
								{
									Port = int.Parse(Item.Value);
									break;
								}
							case ("local_ip"):
								{
									try
									{
										if (Item.Value != "na")
										{
                                            MemoryStream Data = new MemoryStream(Convert.FromBase64String(Item.Value));
                                            List<string> sLocal_IP = Serializer.Deserialize<List<string>>(Data);
                                            foreach (string item in sLocal_IP)
                                            {
                                                IPAddress ip;
                                                if (IPAddress.TryParse(item,out ip))
                                                {
                                                    Local_IP.Add(ip);
                                                }
                                            }
										}
									}
									catch (Exception e)
									{
										Log.LogMessage("P2P Manager : Erreur Whazzup Update = " + e.Message, Color.DarkViolet);
									}
									break;
								}
						}
					}
					if ((!IP.Equals(IPAddress.Parse("0.0.0.0"))) && (Port > 0))
					{
						Peers.Add(new Peer(ref Server, IP, Port, Child.Name, Disabled, m_ExternalIP.Equals(IP), Local_IP));
						UpdateList();
#if DEBUG
						Log.LogMessage("P2PManager: Ajout d'un utilisateur P2P Nom = " + Child.Name + ", IP=" + IP + ", Port=" + Port.ToString(), Color.Blue, 1);
						if (Local_IP != null)
						{
							foreach (IPAddress sip in Local_IP) Log.LogMessage("P2PManager: Liste ip locale = " + sip.ToString(), Color.Blue, 1);
						}
#endif
					}
					else
					{
						Log.LogMessage("P2PManager: Utilisateur " + Child.Name + " ignoré", Color.Violet);

					}
				}
				//Si oui , on le met à jour
				else
				{
					foreach (XmlAttribute Item in Child.Attributes)
					{
						switch (Item.Name)
						{
							case ("ip"):
								{
									if (Shadow) IP = IPAddress.Parse( "127.0.0.1");
                                    else
                                    {
                                        string sIP = Outils.DecryptMessage(Item.Value, pAESKey);
                                        if (!IPAddress.TryParse(sIP, out IP))
                                        {
                                            Log.LogMessage("P2P Manager : Adresse IP invalide", Color.DarkViolet);
                                            IP = IPAddress.Parse("0.0.0.0");
                                        }
                                    }
									if (!test.ExternalIP.Equals(IP))
									{
#if DEBUG
										Log.LogMessage("P2PManager: Modification IP de = " + test.CallSign + ", IP=" + test.ExternalIP.ToString(), Color.Blue, 1);
#endif
										DeleteUser(test.CallSign);
									}
									break;
								}
							case ("port"):
								{
									if (test.Port != int.Parse(Item.Value))
									{
#if DEBUG
										Log.LogMessage("P2PManager: Modification Port de = " + test.CallSign + ", Port=" + test.Port.ToString(), Color.Blue, 1);
#endif
										DeleteUser(test.CallSign);
									}
									break;
								}
						}
					}
				}
			}
			// Recherche Inverse pour détecter un joueur déconnecté
			for (int i=Peers.Count - 1; i>=0; i--)
			{
				bool find = false;
				foreach (XmlNode Child in Liste.ChildNodes)
				{
					if (Peers[i].CallSign==Child.Name)
					{
						if ((!Properties.Settings.Default.ShadowEnable) && (Child.Name == UserName))
						{
							find = false;
						}
						else find = true;
					}
				}
				if (!find)
				{
#if DEBUG
					Log.LogMessage("P2PManager: Supression du joueur " + Peers[i].CallSign, Color.Blue, 1);
#endif
					Peers[i].Dispose();
					Peers.RemoveAt(i);
				}
			}
			UpdateList();
		}

		private void DeleteUser(string Callsign)
		{
			// Recherche Inverse pour détecter un joueur déconnecté
			for (int i = Peers.Count - 1; i >= 0; i--)
			{
				if (Peers[i].CallSign == Callsign)
				{
#if DEBUG
					Log.LogMessage("P2PManager: Supression du joueur " + Peers[i].CallSign, Color.Blue, 1);
#endif
					Peers[i].Dispose();
					Peers.RemoveAt(i);
					UpdateList();
				}
			}
		}
		public void Clear()
		{
			foreach (Peer Pair in Peers)
			{
				Pair.Dispose();
			}
			Peers.Clear();
			if (m_lvUsers != null) m_lvUsers.Items.Clear();
			UpdatePeerDetail();
		}

		/// <summary>
		/// Méthode d'appel asynchrone thread-safe pour l'ajout d'une ligne de tchat
		/// Dans la zone de Tchat P2P
		/// </summary>
		/// <param name="CallSign"></param>
		/// <param name="Message"></param>
		/// <param name="c"></param>
		delegate void AddLineChatCallback(string CallSign, string Message, Color? c);

		/// <summary>
		/// Méthode d'ajout d'une ligne de tchat
		/// Dans la zone de Tchat P2P
		/// </summary>
		/// <param name="CallSign"></param>
		/// <param name="Message"></param>
		/// <param name="c"></param>
		public void AddLineChat(string CallSign, string Message, Color? c = null)
		{
			if (m_rtbLogChat.InvokeRequired)
			{
				AddLineChatCallback d = new AddLineChatCallback(AddLineChat);
				m_rtbLogChat.Invoke(d, new object[] { CallSign, Message, c });
			}
			else
			{
				if (m_rtbLogChat != null)
				{
					m_rtbLogChat.SelectionStart = m_rtbLogChat.TextLength;
					m_rtbLogChat.SelectionLength = 0;
					m_rtbLogChat.SelectionColor = c ?? Color.Black;
					m_rtbLogChat.AppendText(CallSign + " : " + Message + Environment.NewLine);
					m_rtbLogChat.SelectionColor = m_rtbLogChat.ForeColor;
					m_rtbLogChat.ScrollToCaret();
					
				}
			}
		}

		delegate void UpdateListCallback();

		public void UpdateList()
		{
			if (m_lvUsers.InvokeRequired)
			{
				UpdateListCallback d = new UpdateListCallback(UpdateList);
				m_lvUsers.Invoke(d);
			}
			else
			{
				foreach (Peer item in Peers)
				{
					ListViewItem test = m_lvUsers.FindItemWithText(item.CallSign);
					if (test==null)
					{
						int Status = 0;
						if (item.Disabled) Status = 4;
						ListViewItem P = new ListViewItem(item.CallSign, Status);
						P.SubItems.Add(new ListViewItem.ListViewSubItem(P,""));
						m_lvUsers.Items.Add(P);
						m_lvUsers.Update();
					}

				}
				for (int i= m_lvUsers.Items.Count-1; i >= 0; i--)
				{
					Peer test = Peers.Find(x => x.CallSign == m_lvUsers.Items[i].Text);
					if (test == null)
					{
						m_lvUsers.Items.RemoveAt(i);
						m_lvUsers.Update();
					}
				}
			}
		}

		delegate void UpdateListItemCallback(string CallSign);

		public void UpdateListItem(string CallSign)
		{
			if (m_lvUsers.InvokeRequired)
			{
				UpdateListItemCallback d = new UpdateListItemCallback(UpdateListItem);
				m_lvUsers.Invoke(d, new object[] { CallSign });
			}
			else
			{
				Peer Pair = Peers.Find(x => x.CallSign == CallSign);
				ListViewItem Item = m_lvUsers.FindItemWithText(CallSign);
				if ((Item != null) && (Pair != null))
				{
					if (Pair.Disabled)
					{
						Item.ImageIndex = 4;
						Item.SubItems[1].Text = "";
					}
					else if (!Pair.IsOnline)
					{
						Item.ImageIndex = 0;
						Item.SubItems[1].Text = "";
					}
					else
					{
						if (Pair.Latence < 200) Item.ImageIndex = 3;
						else if ((Pair.Latence >= 200) && (Pair.Latence < 500)) Item.ImageIndex = 2;
						else Item.ImageIndex = 1;
						Item.SubItems[1].Text =Pair.AIResolution.Titre;
					}
					m_lvUsers.Update();
					UpdatePeerDetail();
				}
#if DEBUG
				else Log.LogMessage("P2PManager: Pas de correspondance trouvée pour " + CallSign, Color.DarkViolet,1);
#endif
			}
		}

		public void UpdatePeerDetail()
		{
			if (m_lvUsers.SelectedItems.Count == 0)
			{
				ClearPeerDetail();
				return;
			}
			ListViewItem User = m_lvUsers.SelectedItems[0];
			Peer Pair = Peers.Find(x => x.CallSign == User.Text);
			if (Pair != null)
			{
				// On rempli la ligne correspondante dans la listview
				foreach (ListViewItem Item in m_lvPeerDetail.Items)
				{
					if (Item.SubItems.Count < 1) continue;
					ListViewItem.ListViewSubItem Sub = Item.SubItems[1];
					switch (Item.Text)
					{
						case "Statut":
							if (Pair.IsOnline) Sub.Text = "Connecté";
							else Sub.Text = "Déconnecté";
							break;
						case "Refresh":
							Sub.Text = string.Format("{0:0000} mSec", Pair.RefreshRate);
							break;
						case "Latence":
							Sub.Text = string.Format("{0:0.} mSec", Pair.Latence);
							break;
						case "Distance":
							Sub.Text = string.Format("{0:0.0} Nm", Pair.Distance);
							break;
						case "Altitude":
							Sub.Text = string.Format("{0:0.} ft", Pair.Altitude);
							break;
						case "Longitude":
							Sub.Text = string.Format("{0:0.000}°", Pair.Longitude);
							break;
						case "Latitude":
							Sub.Text = string.Format("{0:0.000}°", Pair.Latitude);
							break;
						case "Bank Angle":
							Sub.Text = string.Format("{0:0.000}°", Pair.BankAngle);
							break;
						case "Pitch Angle":
							Sub.Text = string.Format("{0:0.000}°", Pair.PitchAngle);
							break;
						case "Vitesse":
							Sub.Text = string.Format("{0:0.} KIAS", Pair.Vitesse);
							break;
						case "Direction":
							Sub.Text = string.Format("{0:0.} °", Pair.Direction);
							break;
						case "OnGround":
							Sub.Text = string.Format("{0:0.}", Pair.OnGround);
							break;
						case "Version":
							Sub.Text = string.Format("{0:0.}", Pair.Version);
							break;
						case "Squawk":
							Sub.Text = string.Format("{0:0.}", Outils.ConvertToBinaryCodedDecimal(Pair.Squawk));
							break;
						case "Titre":
							Sub.Text = Pair.Titre;
							break;
						case "Model":
							Sub.Text = Pair.Model;
							break;
						case "Type":
							Sub.Text = Pair.Type;
							break;
						case "Refresh Distant":
							Sub.Text = string.Format("{0:0000} mSec", Pair.RemoteRefreshRate);
							break;
						case "Décalage":
							Sub.Text = string.Format("{0:000000} mSec", Pair.Decalage);
							break;
						case "Spawned":
							Sub.Text = Pair.Spawned.ToString();
							break;
						case "Object_ID":
							Sub.Text = Pair.ObjectId.ToString();
							break;
						case "Frame Count":
							Sub.Text = Pair.FrameCount.ToString();
							break;
						case "Ecart":
							Sub.Text = string.Format("{0:0.0} Nm", Pair.Ecart);
							break;
						case "Last Update":
							Sub.Text = string.Format(Pair.LastAIUpdate.ToLongTimeString());
							break;
					}
				}
			}
			else
			{
				ClearPeerDetail();
			}
		}
		private void ClearPeerDetail ()
		{
			// On vide la liste des données
			foreach (ListViewItem Item in m_lvPeerDetail.Items)
			{
				if (Item.SubItems.Count < 1) continue;
				Item.SubItems[1].Text = "";
			}
		}

        private void CheckPropertiesData ()
        {
            if ((Properties.Settings.Default.P2PPort<49152) || (Properties.Settings.Default.P2PPort > 65535))
            {
                Properties.Settings.Default.P2PPort = 54856;
                Properties.Settings.Default.Save();
            }
            if ((Properties.Settings.Default.P2PRate < 200) || (Properties.Settings.Default.P2PRate > 5000))

            {
                Properties.Settings.Default.P2PRate = 200;
                Properties.Settings.Default.Save();
            }
            
        }
	}
}
