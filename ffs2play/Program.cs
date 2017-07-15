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
 * Program.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


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
