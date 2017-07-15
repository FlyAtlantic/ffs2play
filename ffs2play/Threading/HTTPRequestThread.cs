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
 * HTTPRequestThread.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


using System;
using System.Threading;
using System.Xml;
using System.Net;
using System.IO;
using System.Drawing;
using System.Text;

namespace ffs2play
{
	class HTTPRequestThread
	{
		private Logger Log;
		private Thread m_Thread;
		private HttpWebRequest m_Request;
		private delegate void m_Callback(HttpWebResponse Response);
		private m_Callback Callback;
		private HttpWebResponse m_Response;
		private Stream requestStream;
		private XmlDocument m_xmldoc;
		public HTTPRequestThread (XmlDocument Doc,string URL, Action<HttpWebResponse> pCallBack)
		{
			InitRequest(URL, pCallBack);
			m_Request.Method = "POST";
			m_xmldoc = Doc;
		}

		public HTTPRequestThread(string URL, Action<HttpWebResponse> pCallBack)
		{
			InitRequest(URL, pCallBack);
			m_Request.Method = "GET";
		}

		private void InitRequest(string URL, Action<HttpWebResponse> pCallBack)
		{
			Log = Logger.Instance;
			Callback = new m_Callback(pCallBack);
			m_Request = (HttpWebRequest)WebRequest.Create(URL);
            m_Request.ServicePoint.BindIPEndPointDelegate = (ServicePoint, remoteEndPoint, retryCount) =>
            {
                if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(IPAddress.Any, 0);
                }

                throw new InvalidOperationException("no IPv4 address");
            };
			m_Thread = new Thread(Main);
			m_Thread.IsBackground = true;
			m_Request.Timeout = 10000;
			m_Request.KeepAlive = true;
		}

		public void Start()
		{
			m_Thread.Start();
		}

		public void Join()
		{
			m_Thread.Join();
		}

		public void Main()
		{
			try
			{
				if (m_Request.Method == "POST")
				{
					byte[] bytes;
					bytes = Encoding.UTF8.GetBytes(m_xmldoc.OuterXml);
					m_Request.ContentType = "text/xml; encoding='utf-8'";
					m_Request.ContentLength = bytes.Length;
					m_Request.UserAgent = "ffs2play";
					m_Request.AllowAutoRedirect = false;
					requestStream = m_Request.GetRequestStream();
					requestStream.Write(bytes, 0, bytes.Length);
					requestStream.Close();
				}
				m_Response = (HttpWebResponse)m_Request.GetResponse();
				Callback(m_Response);
			}
			catch (WebException e)
			{
#if DEBUG
				Log.LogMessage("HTTPRequestThreadErreur WebException: " + e.Message + " " + e.Status.ToString(), Color.Blue,1);
#endif
				Callback(m_Response);
			}
			catch (Exception e)
			{
				Log.LogMessage("HTTPRequestThread Erreur: " + e.Message, Color.Violet);
				Callback(m_Response);
			}
		}
	}
}