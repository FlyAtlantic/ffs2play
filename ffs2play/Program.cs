using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ffs2play
{
	static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			if ((!EnsureSingleInstance())&&(!Properties.Settings.Default.MultiInstance))
			{
				return;
			}
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ffs2play());
        }
		/// <summary>
		/// Méthode pour determiner si il existe une autre instance de ffs2play
		/// </summary>
		/// <returns></returns>
		static bool EnsureSingleInstance()
		{
			Process currentProcess = Process.GetCurrentProcess();

			var runningProcess = (from process in Process.GetProcesses()
								  where
									process.Id != currentProcess.Id &&
									process.ProcessName.Equals(
									  currentProcess.ProcessName,
									  StringComparison.Ordinal)
								  select process).FirstOrDefault();

			if (runningProcess != null)
			{
				ShowWindow(runningProcess.MainWindowHandle, SW_SHOW);
				ShowWindow(runningProcess.MainWindowHandle, SW_RESTORE);
				SetForegroundWindow(runningProcess.MainWindowHandle);

				return false;
			}

			return true;
		}
		[DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

		private const int SW_RESTORE = 9;
		private const int SW_SHOW = 5;
    }


}
