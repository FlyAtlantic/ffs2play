using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Drawing;

namespace ffs2play
{
	public partial class PirepManager
	{
		public bool IsConnected()
		{
			return m_bConnected;
		}

		public XmlNode GetFirstElement(ref XmlDocument XmlDoc, String Name)
		{
			if (XmlDoc == null) return null;
			XmlNodeList element = XmlDoc.GetElementsByTagName(Name);
			if (element.Count > 0) return element[0];
			else return null;

		}

#if DEBUG
		public string Beautify(XmlDocument doc)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace
			};
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				doc.Save(writer);
			}
			return sb.ToString();
		}
#endif

		public XmlDocument GetResultXml(ref HttpWebResponse Reponse)
		{
			XmlDocument xmlDoc = new XmlDocument();
			Stream responseStream;
			string strContent = "";
			try
			{
				responseStream = Reponse.GetResponseStream();
				using (StreamReader sr = new StreamReader(responseStream))
				{
					//Need to return this response 
					strContent = sr.ReadToEnd();
					xmlDoc.LoadXml(strContent);
#if DEBUG
					Log.LogMessage("PManager: Réponse XML = \n" + Beautify(xmlDoc), Color.DarkGreen,2);
#endif
					return xmlDoc;
				}
			}
			catch (Exception e)
			{
				// Nous le signalons par un message
				Log.LogMessage("L'analyse de la réponse du serveur a échouée : " + e.Message, Color.DarkViolet);
#if DEBUG
				Log.LogMessage("PManager - Contenu de la trame :" + Environment.NewLine + strContent, Color.DarkViolet,1);
#endif
			}
			return null;
		}
	}
}
