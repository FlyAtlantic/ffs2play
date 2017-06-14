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