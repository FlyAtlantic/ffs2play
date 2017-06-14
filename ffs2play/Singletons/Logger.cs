using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ffs2play
{
	public class Logger
	{
		private RichTextBox m_rtbLogZone;
#if DEBUG
		public string Filter
		{
			get { return m_FiltreLog; }
			set
			{
				m_FiltreLog = value;
				Pattern = m_FiltreLog;
				Pattern = Pattern.Replace(".", @"\.");
				Pattern = Pattern.Replace("?", ".");
				Pattern = Pattern.Replace("*", ".*?");
				Pattern = Pattern.Replace(@"\", @"\\");
				Pattern = Pattern.Replace(" ", @"\s");
				Properties.Settings.Default.FiltreLog = m_FiltreLog;
				Properties.Settings.Default.Save();
			}
		}
		private string m_FiltreLog;
		private string Pattern;
        private byte Debug_Level;
#endif
        private Logger ()
		{
			m_rtbLogZone = null;
			if ((m_rtbLogZone = ffs2play.getControl("rtbLogZone") as RichTextBox) != null)
			{
				m_rtbLogZone.Font = new Font("Microsoft San Serif", 10);
			}
#if DEBUG
			m_FiltreLog = Properties.Settings.Default.FiltreLog;
			Pattern = m_FiltreLog;
			Pattern = Pattern.Replace(".", @"\.");
			Pattern = Pattern.Replace("?", ".");
			Pattern = Pattern.Replace("*", ".*?");
			Pattern = Pattern.Replace(@"\", @"\\");
			Pattern = Pattern.Replace(" ", @"\s");
            Debug_Level = Properties.Settings.Default.DebugMode;
#endif
        }
        public static Logger Instance
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

			internal static readonly Logger instance = new Logger();
		}

#if DEBUG
		public byte DebugLevel
		{
			get
			{
				return Debug_Level;
			}
			set
			{
				if (value < 0) Debug_Level = 0;
				else if (value > 2) Debug_Level = 2;
				else Debug_Level = value;
				Properties.Settings.Default.DebugMode = Debug_Level;
				// Sauvegarde de la configuration
				Properties.Settings.Default.Save();
			}
		}

		public bool IsLike(string text, bool caseSensitive = false)
		{
			return new Regex(Pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase).IsMatch(text,0);
		}
#endif
		// This delegate enables asynchronous calls for setting
		// the text property on a TextBox control.
		delegate void LogMessageCallback(string text, Color? c, byte Level);

		public void LogMessage(string Message, Color? c = null, byte Level = 0)
		{
#if DEBUG
			if (Level > Debug_Level) return;
			if ((Level > 0) && (!IsLike(Message))) return;
#endif
			if (m_rtbLogZone != null)
			{
				if (m_rtbLogZone.InvokeRequired)
				{
					LogMessageCallback d = new LogMessageCallback(LogMessage);
					m_rtbLogZone.Invoke(d, new object[] { Message, c, Level });
				}
				else
				{
					if ((m_rtbLogZone != null)&&(!m_rtbLogZone.IsDisposed))
					{
						m_rtbLogZone.SelectionStart = m_rtbLogZone.TextLength;
						m_rtbLogZone.SelectionLength = 0;
						m_rtbLogZone.SelectionColor = c ?? Color.Black;
						m_rtbLogZone.AppendText(Message + Environment.NewLine);
						m_rtbLogZone.SelectionColor = m_rtbLogZone.ForeColor;
						m_rtbLogZone.ScrollToCaret();
					}
				}
			}
		}
	}
}
