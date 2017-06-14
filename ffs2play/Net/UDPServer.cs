using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ffs2play
{
	public class UDPServer : IDisposable
	{
		private Logger Log;
		private int m_Port;
		private bool m_bRunning;
		private Socket m_listener;
		private byte[] m_Buffer;
		private EndPoint m_ClientEP;
		private object m_ClientLock;
		public event EventHandler<UDPReceiveEvent> OnReceiveMessage;

		/// <summary>
		/// Port d'écoute du serveur
		/// </summary>

		public int Port
		{
			get { return m_Port; }
			set { if (!m_bRunning) m_Port = value; }
		}

		/// <summary>
		/// Retourne l'état du serveur
		/// </summary>

		public bool Running
		{
			get { return m_bRunning; }
		}

		/// <summary>
		/// Constructeur
		/// </summary>

		public UDPServer()
		{
			m_bRunning = false;
			Log = Logger.Instance;
			m_Buffer = new byte[2048];
			m_ClientEP = new IPEndPoint(IPAddress.Any, 0);
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_ClientLock = new object();
		}


		/// <summary>
		/// Démarre le serveur
		/// </summary>

		public void Start()
		{
			if (!m_bRunning)
			{
				if (m_listener != null) Stop();
				try
				{
					m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					var sioUdpConnectionReset = -1744830452;
					var inValue = new byte[] { 0 };
					var outValue = new byte[] { 0 };
					m_listener.IOControl(sioUdpConnectionReset, inValue, outValue);
					m_listener.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoDelay, 1);
					m_listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
					m_listener.Bind(new IPEndPoint(IPAddress.Any, m_Port));
					m_listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 136);
					ListenForData();
#if DEBUG
					Log.LogMessage("UDPServer : En écoute sur le port " + m_Port.ToString(), Color.DarkBlue, 1);
#endif
					m_bRunning = true;
				}
				catch (SocketException Ex)
				{
					Log.LogMessage("UDPServer : L'initialisation a échouée : " + Ex.Message, Color.DarkBlue, 0);
					if (m_listener != null)
					{
						m_listener.Close();
						m_listener = null;
					}
				}
			}
		}

		private void ListenForData()
		{
			try
			{
				m_listener.BeginReceiveFrom(m_Buffer, 0, m_Buffer.Length, SocketFlags.None, ref m_ClientEP, DoReceiveFrom, m_listener);
			}
			catch (SocketException Ex)
			{
				Log.LogMessage("UDPServer : Problème Socket : " + Ex.Message, Color.DarkViolet, 0);
			}


		}
		/// <summary>
		/// Arrête le serveur
		/// </summary>

		public void Stop()
		{
			if (m_bRunning && (m_listener != null))
			{
				m_listener.Close();
				m_listener = null;
#if DEBUG
				Log.LogMessage("UDPServer : Arrêt du serveur", Color.DarkBlue, 1);
#endif
			}
			m_bRunning = false;
		}

		/// <summary>
		/// Reçois un Datagramme
		/// </summary>
		/// <param name="iar"></param>

		protected void DoReceiveFrom(IAsyncResult iar)
		{
			try
			{
				if (m_listener == null) return;
				//Get the received message.
				Socket recvSock = (Socket)iar.AsyncState;
				int msgLen = recvSock.EndReceiveFrom(iar, ref m_ClientEP);
				byte[] localMsg = new byte[msgLen];
				Array.Copy(m_Buffer, localMsg, msgLen);
				ThreadPool.QueueUserWorkItem(ProcessIncomingData, new UDPReceiveEvent((IPEndPoint)m_ClientEP, localMsg));
				ListenForData();
			}
			catch (ObjectDisposedException Ex)
			{
				Log.LogMessage("UDPServer : Problème réception de données : " + Ex.Message, Color.DarkBlue, 0);
			}
			catch (SocketException Ex)
			{
				Log.LogMessage("UDPServer : Problème Socket : " + Ex.Message, Color.DarkBlue, 0);
			}
		}

		private void ProcessIncomingData(object obj)
		{
			try
			{
				lock (m_ClientLock)
				{
					UDPReceiveEvent Packet = (UDPReceiveEvent)obj;
					FireReceiveMessage(ref Packet);
#if DEBUG
					Log.LogMessage("UDPServer : Requête entrante de " + (Packet.Client.Address.ToString() + " : " + Outils.HexToString(Packet.Data, Packet.Data.Length)), Color.Blue, 2);
#endif

				}
			}
			catch (SocketException Ex)
			{
				Log.LogMessage("UDPServer : Problème Socket : " + Ex.Message, Color.DarkBlue, 0);
			}
		}

		/// <summary>
		/// Envoi un datagramme
		/// </summary>
		/// <param name="remoteEP"></param>
		/// <param name="buffer"></param>

		public void Send(ref IPEndPoint remoteEP, byte[] buffer)
		{
			if ((m_listener != null) && (buffer != null))
			{
				try
				{
					m_listener.SendTo(buffer, remoteEP);
#if DEBUG
					Log.LogMessage("UDPServer : Données sortante vers " + remoteEP.Address.ToString() + " sur le Port = " + remoteEP.Port.ToString() + ": " + Outils.HexToString(buffer, buffer.Length), Color.Blue, 2);
#endif
				}
				catch (SocketException ex)
				{
#if DEBUG
					Log.LogMessage("Exception écriture socket  : " + ex.ErrorCode.ToString(),Color.Violet);
#endif
				}
				catch (Exception ex)
				{
#if DEBUG
					Log.LogMessage("Exception écriture socket  : " + ex.Message.ToString(), Color.Violet);
#endif
				}
			}
		}

		protected virtual void FireReceiveMessage(ref UDPReceiveEvent e)
		{
			OnReceiveMessage?.Invoke(this, e);
		}

		public void Dispose()
		{
			Stop();
			GC.SuppressFinalize(this);
		}
	}
	public class UDPReceiveEvent : EventArgs
	{
		public IPEndPoint Client;
		public byte[] Data;
		public DateTime Time;
		public UDPReceiveEvent(IPEndPoint p_Client, byte[] p_Data)
		{
			Client = p_Client;
			Data = p_Data;
			Time = DateTime.UtcNow;
		}
	}
}

