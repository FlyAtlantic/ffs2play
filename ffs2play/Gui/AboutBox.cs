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
 * AboutBox.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


using System;
using System.Reflection;
using System.Windows.Forms;

namespace ffs2play
{
	partial class AboutBox : Form
	{
#if PLATFORM_X86
        private const string Platform = "X86";
#elif PLATFORM_X64
        private const string Platform = "X64";
#endif
        public AboutBox()
		{
            InitializeComponent();
			AssemblyInfo entryAssemblyInfo = new AssemblyInfo(Assembly.GetEntryAssembly());
			this.Text = String.Format("À propos de {0}", entryAssemblyInfo.ProductTitle);
			this.labelProductName.Text = entryAssemblyInfo.Product;
			this.labelVersion.Text = String.Format("Version {0}", entryAssemblyInfo.Version) + " " + Platform;
			this.labelCopyright.Text = entryAssemblyInfo.Copyright;
			this.labelCompanyName.Text = "https://www.ffs2play.fr";
			this.textBoxDescription.Text = entryAssemblyInfo.Description;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void labelCompanyName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://www.ffs2play.fr");
			labelCompanyName.LinkVisited = true;
		}
	}
}
