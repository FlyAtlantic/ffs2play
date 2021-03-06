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
 * Peer.Requetes.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.IO;
using System.Drawing;
using System.Net;
using ProtoBuf;

namespace ffs2play
{
	public partial class Peer
	{
		private void Send(ref MemoryStream Output)
		{
			if (m_EP != null) Server.Send(ref m_EP, Output.ToArray());
			else
			{
				if (m_bLocal && (m_InternalIP.Count>0))
				{
					if (m_sel_iplocal >= m_InternalIP.Count) m_sel_iplocal = 0;
					IPEndPoint EP = new IPEndPoint(m_InternalIP[m_sel_iplocal], m_Port);
					Server.Send(ref EP, Output.ToArray());
					m_sel_iplocal++;
				}
			}
		}

		/// <summary>
		/// Envoi un message texte au peer
		/// </summary>
		/// <param name="Message"></param>

		public void SendChat(string Message)
		{
			if (!m_OnLine) return;
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.CHAT);
			BOutput.Write(Message);
			Send(ref Output);
		}

		private void SendPing()
		{
			m_LastPing = DateTimeEx.UtcNowMilli;
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.PING);
			Send(ref Output);
			m_Counter++;
			if ((m_Counter > 2) && m_OnLine )
			{
#if DEBUG
                if (m_Counter >2)
				    Log.LogMessage("Peer [" + CallSign + "] Passage en état OffLine = Timeout", Color.DarkBlue, 1);
                else
                    Log.LogMessage("Peer [" + CallSign + "] Passage en état OffLine = Déconnexion", Color.DarkBlue, 1);
#endif
                m_OnLine = false;
				m_Version = 0;
				m_MiniPing = 1000;
				P2P.UpdateListItem(CallSign);
				Spawn_AI(false);
			}
		}

		private void SendPong(long Time)
		{
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.PONG);
			BOutput.Write(Time);
			Send(ref Output);
		}

		public void SendVersion()
		{
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.VERSION);
			BOutput.Write(PROTO_VERSION);
			BOutput.Write(m_SendData.Title);
			BOutput.Write(m_SendData.Type);
			BOutput.Write(m_SendData.Model);
			BOutput.Write(m_SendData.Category);
			Send(ref Output);
		}

		/// <summary>
		/// Réclame la version au PEER
		/// </summary>
		private void RequestVersion()
		{
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.REQ_VERSION);
			Send(ref Output);
		}

		/// <summary>
		/// Envois les données périodiques de position
		/// </summary>
		private void SendData()
		{
			//Envoi des données
			MemoryStream Output = new MemoryStream();
			BinaryWriter BOutput = new BinaryWriter(Output);
			BOutput.Write((byte)Protocol.DATA);
			// On boucle le  compteur de datagramme si il arrive à la limite sinon on l'incrémente
			if (m_Counter_Out == 255) m_Counter_Out = 0;
			else m_Counter_Out++;
			BOutput.Write(m_Counter_Out);
            MemoryStream Data = new MemoryStream();
            Serializer.Serialize(Data, m_SendData);
            BOutput.Write(Data.ToArray());
			Send(ref Output);
		}
	}
}
