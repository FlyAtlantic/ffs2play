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
 * TCPServer.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ffs2play
{
    class TCPServer : IDisposable
    {
        private Logger Log;
        private int m_Port;
        private bool m_bRunning;
        private Socket m_listener;
        private byte[] m_Buffer;
        private EndPoint m_ClientEP;
        private object m_ClientLock;
        public event EventHandler<TCPReceiveEvent> OnReceiveMessage;
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

        public TCPServer()
        {
            m_bRunning = false;
            Log = Logger.Instance;
            m_Buffer = new byte[2048];
            m_ClientEP = new IPEndPoint(IPAddress.Any, 0);
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                    if (m_listener != null) Stop();
                    m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_listener.Bind(new IPEndPoint(IPAddress.Any, m_Port));
                    m_listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 136);
                    m_listener.Listen(1);
                    ListenForNewConnection();
#if DEBUG
                    Log.LogMessage("TCPServer : En écoute sur le port " + m_Port.ToString(), Color.DarkBlue, 1);
#endif
                    m_bRunning = true;
                }
                catch (SocketException Ex)
                {
                    Log.LogMessage("TCPServer : L'initialisation a échouée : " + Ex.Message, Color.DarkBlue, 0);
                    if (m_listener != null)
                    {
                        m_listener.Close();
                        m_listener = null;
                    }
                }
            }
        }

        private void ListenForNewConnection ()
        {
            try
            {
                m_listener.BeginAccept(new AsyncCallback(AcceptCallback), m_listener);
            }
            catch (SocketException Ex)
            {
                Log.LogMessage("TCPServer : Problème Socket : " + Ex.Message, Color.DarkViolet, 0);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            state.Address = handler.RemoteEndPoint;
#if DEBUG
            Log.LogMessage("TCPServer : Connexion entrante IP =  " + ((IPEndPoint)state.Address).Address.ToString(), Color.DarkBlue, 1);
#endif

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(DoReceiveFrom), state);
            ListenForNewConnection();
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
                Log.LogMessage("TCPServer : Arrêt du serveur", Color.DarkBlue, 1);
#endif
            }
            m_bRunning = false;
        }

        /// <summary>
        /// Reçois un Datagramme
        /// </summary>
        /// <param name="iar"></param>

        protected void DoReceiveFrom(IAsyncResult ar)
        {
            try
            {
                if (m_listener == null) return;
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;
                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] localMsg = new byte[bytesRead];
                    Array.Copy(state.buffer, localMsg, bytesRead);
                    ThreadPool.QueueUserWorkItem(ProcessIncomingData, new TCPReceiveEvent((IPEndPoint)state.Address, localMsg));
                }
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(DoReceiveFrom), state);
            }
            catch (ObjectDisposedException Ex)
            {
                Log.LogMessage("TCPServer : Problème réception de données : " + Ex.Message, Color.DarkBlue, 0);
            }
            catch (SocketException Ex)
            {
                Log.LogMessage("TCPServer : Problème Socket : " + Ex.Message, Color.DarkBlue, 0);
            }
        }

        private void ProcessIncomingData(object obj)
        {
            try
            {
                lock (m_ClientLock)
                {
                    TCPReceiveEvent Packet = (TCPReceiveEvent)obj;
                    FireReceiveMessage(ref Packet);
#if DEBUG
                    Log.LogMessage("TCPServer : Requête entrante de " + (Packet.Client.Address.ToString() + " : " + Outils.HexToString(Packet.Data, Packet.Data.Length)), Color.Blue, 2);
#endif

                }
            }
            catch (SocketException Ex)
            {
                Log.LogMessage("TCPServer : Problème Socket : " + Ex.Message, Color.DarkBlue, 0);
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
                    Log.LogMessage("TCPServer : Données sortante vers " + remoteEP.Address.ToString() + " sur le Port = " + remoteEP.Port.ToString() + ": " + Outils.HexToString(buffer, buffer.Length), Color.Blue, 2);
#endif
                }
                catch (SocketException ex)
                {
#if DEBUG
                    Log.LogMessage("Exception écriture socket  : " + ex.ErrorCode.ToString(), Color.Violet);
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

        protected virtual void FireReceiveMessage(ref TCPReceiveEvent e)
        {
            OnReceiveMessage?.Invoke(this, e);
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

    }

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // EndPoint Address
        public EndPoint Address;
    }

    public class TCPReceiveEvent : EventArgs
    {
        public IPEndPoint Client;
        public byte[] Data;
        public DateTime Time;
        public TCPReceiveEvent(IPEndPoint p_Client, byte[] p_Data)
        {
            Client = p_Client;
            Data = p_Data;
            Time = DateTimeEx.UtcNow;
        }
    }
}
