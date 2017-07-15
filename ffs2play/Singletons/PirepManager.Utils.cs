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
 * PirepManager.Utils.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

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
