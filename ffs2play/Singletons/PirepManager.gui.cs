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
 * PirepManager.gui.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using System.Xml;

namespace ffs2play
{
    public partial class PirepManager
    {
        private Button m_btnConnect;
        private RichTextBox m_rtbDecryptedMetar;
        private TextBox m_tbMetar;
        /// <summary>
        /// Event Click sur le bouton de connexion au serveur VMS
        /// En fonction de l'état de connexion, on lance la connexion
        /// Ou on déconnecte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConPirep_Click(object sender, EventArgs e)
        {
            m_btnConnect.Enabled = false;
            if (!m_bConnected)
            {
                dlgConServeur ConPirep = new dlgConServeur(true);
                if (ConPirep.DialogResult != DialogResult.OK) ConPirep.ShowDialog();
            }
            else
            {
                Disconnect();
            }
        }
    }
}
