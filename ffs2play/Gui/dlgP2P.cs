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
 * dlgP2P.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace ffs2play
{
    public partial class dlgP2P : Form
    {
		private bool init;
		private bool Changes;
		private bool NeedP2PRestart;
		private bool NeedDBReload;
		private AIMapping AIM;
		private StringCollection FolderList;
		private SIM_VERSION Sim;
#if DEBUG
		private CheckBox cbShadow;
#endif

		public dlgP2P()
        {
			SCManager sc = SCManager.Instance;
			init = true;
            InitializeComponent();
			AIM = AIMapping.Instance;
#if DEBUG
			cbShadow = new CheckBox();
			gbMultijoueur.Controls.Add(cbShadow);
			// 
			// cbShadow
			// 
			cbShadow.AutoSize = true;
			cbShadow.Location = new Point(9, 21);
			cbShadow.Name = "cbShadow";
			cbShadow.Size = new Size(141, 17);
			cbShadow.TabIndex = 1;
			cbShadow.Text = "Activer le mode Shadow";
			cbShadow.UseVisualStyleBackColor = true;
			cbShadow.CheckedChanged += new EventHandler(this.cbShadow_CheckedChanged);
			cbShadow.Checked = Properties.Settings.Default.ShadowEnable;
#endif
			Changes = false;
			NeedP2PRestart = false;
			NeedDBReload = false;
			cbUPNP.Checked = Properties.Settings.Default.UPNPEnable;
			cbInfoSim.Checked = Properties.Settings.Default.P2PInfoEnable;
			numPort.Value = Properties.Settings.Default.P2PPort;
			numP2PTx.Value = Properties.Settings.Default.P2PRate;
			numAIRayon.Value = Properties.Settings.Default.P2PRadius;
			numAILimite.Value = Properties.Settings.Default.P2PAILimite;
            cbBeta.Checked = Properties.Settings.Default.Beta;
			cbDropRateDisp.Checked = Properties.Settings.Default.EnaDropDisp;
			cbSendDropRate.Checked = Properties.Settings.Default.EnaDropSend;
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;
			UpdateEnable();
			if (PirepManager.Instance.IsConnected()) numPort.Enabled = false;
			init = false;
			Sim = sc.GetVersion();
			switch (Sim)
			{
				case SIM_VERSION.UNKNOWN:
					FolderList = null;
					lvItems.Enabled = false;
					btnAddItem.Enabled = false;
					btnDelItem.Enabled = false;
					break;
				case SIM_VERSION.FSX:
					gbFSX.Text += "FSX Microsoft";
					FolderList = Properties.Settings.Default.AIScanFoldersFSX;
					break;
				case SIM_VERSION.FSX_STEAM:
					gbFSX.Text += "FSX Steam Edition";
					FolderList = Properties.Settings.Default.AIScanFoldersFSXSE;
					break;
				case SIM_VERSION.P3D_V2:
					gbFSX.Text += "Prepar3D V2";
					FolderList = Properties.Settings.Default.AIScanFoldersP3DV2;
					break;
				case SIM_VERSION.P3D_V3:
					gbFSX.Text += "Prepar3D V3";
					FolderList = Properties.Settings.Default.AIScanFoldersP3DV3;
					break;
                case SIM_VERSION.P3D_V4:
                    gbFSX.Text += "Prepar3D V4";
                    FolderList = Properties.Settings.Default.AIScanFoldersP3DV4;
                    break;
            }
			if (FolderList != null)
			{
				foreach (string Path in FolderList)
				{
					lvItems.Items.Add(Path);
				}
			}
		}

		private void btnAnnuler_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void CheckChanges()
		{
			if (init) return;
			Changes = false;
			NeedP2PRestart = false;
			if (cbUPNP.Checked != Properties.Settings.Default.UPNPEnable)
			{
				Changes = true;
				NeedP2PRestart = true;
			}

			if (cbInfoSim.Checked != Properties.Settings.Default.P2PInfoEnable)
			{
				Changes = true;
			}
#if DEBUG
			if (cbShadow.Checked != Properties.Settings.Default.ShadowEnable)
			{
				Changes = true;
				NeedP2PRestart = true;
			}
#endif
			if (cbDropRateDisp.Checked != Properties.Settings.Default.EnaDropDisp)
			{
				Changes = true;
			}
			if (cbSendDropRate.Checked != Properties.Settings.Default.EnaDropSend)
			{
				Changes = true;
			}
			if (numPort.Value != Properties.Settings.Default.P2PPort)
			{
				Changes = true;
				NeedP2PRestart = true;
			}
			if (numP2PTx.Value != Properties.Settings.Default.P2PRate)
			{
				Changes = true;
			}
			if (numAIRayon.Value != Properties.Settings.Default.P2PRadius)
			{
				Changes = true;
			}
			if (numAILimite.Value != Properties.Settings.Default.P2PAILimite)
			{
				Changes = true;
			}
			if (FolderList!=null)
			{
				NeedDBReload = false;
				foreach(ListViewItem item in lvItems.Items)
				{
					if (!FolderList.Contains(item.Text))
					{
						Changes = true;
						NeedDBReload = true;
						break;
					}
				}
				if (lvItems.Items.Count != FolderList.Count)
				{
					NeedDBReload = true;
					Changes = true;
				}
			}
            if (cbBeta.Checked != Properties.Settings.Default.Beta)
            {
                Changes = true;
            }
			if (Changes)
			{
				btnAppliquer.Enabled = true;
				btnAppliquer.BackColor = Color.DodgerBlue;
				btnAppliquer.ForeColor = Color.White;
			}
			else
			{
				btnAppliquer.Enabled = false;
				btnAppliquer.BackColor = Color.LightGray;
				btnAppliquer.ForeColor = Color.Black;
			}
		}

		private void ApplyChanges()
		{
			if (!Changes) return;
			if (cbUPNP.Checked != Properties.Settings.Default.UPNPEnable)
			{
				Properties.Settings.Default.UPNPEnable = cbUPNP.Checked;
			}
			if (cbInfoSim.Checked != Properties.Settings.Default.P2PInfoEnable)
			{
				Properties.Settings.Default.P2PInfoEnable = cbInfoSim.Checked;
			}
#if DEBUG
			if (cbShadow.Checked != Properties.Settings.Default.ShadowEnable)
			{
				Properties.Settings.Default.ShadowEnable = cbShadow.Checked;
			}
#endif
			if (cbDropRateDisp.Checked != Properties.Settings.Default.EnaDropDisp)
			{
				Properties.Settings.Default.EnaDropDisp = cbDropRateDisp.Checked;
			}
			if (cbSendDropRate.Checked != Properties.Settings.Default.EnaDropSend)
			{
				Properties.Settings.Default.EnaDropSend = cbSendDropRate.Checked;
			}
			if (numPort.Value != Properties.Settings.Default.P2PPort)
			{
				Properties.Settings.Default.P2PPort = (ushort)numPort.Value;
			}
			if (numP2PTx.Value != Properties.Settings.Default.P2PRate)
			{
				Properties.Settings.Default.P2PRate = (ushort)numP2PTx.Value;
			}
			if (numAIRayon.Value != Properties.Settings.Default.P2PRadius)
			{
				Properties.Settings.Default.P2PRadius = (ushort)numAIRayon.Value;
			}
			if (numAILimite.Value != Properties.Settings.Default.P2PAILimite)
			{
				Properties.Settings.Default.P2PAILimite = (int)numAILimite.Value;
			}
			if (NeedP2PRestart)
			{
				P2PManager P2P = P2PManager.Instance;
				P2P.Init(false);
				if (Properties.Settings.Default.P2PEnable) P2P.Init(true);
				NeedP2PRestart = false;
			}
			if (NeedDBReload)
			{
				if (FolderList!= null)
				{
					FolderList.Clear();
					foreach (ListViewItem item in lvItems.Items)
					{
						FolderList.Add(item.Text);
					}
					//AIM.CleanAIDB();
					AIM.StartScanSimObject();
				}
			}
            if (cbBeta.Checked != Properties.Settings.Default.Beta)
            {
                Properties.Settings.Default.Beta = cbBeta.Checked;
            }
			Properties.Settings.Default.Save();
			Changes = false;
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;
		}

		private void UpdateEnable()
		{
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (Changes) ApplyChanges();
			Close();
		}

		private void btnAppliquer_Click(object sender, EventArgs e)
		{
			ApplyChanges();
		}

		private void cbEnableP2P_CheckedChanged(object sender, EventArgs e)
		{
			UpdateEnable();
			CheckChanges();
		}
#if DEBUG
        private void cbShadow_CheckedChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}
#endif
		private void cbUPNP_CheckedChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void numPort_ValueChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void numP2PTx_ValueChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void numAIRadius_ValueChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void btnAddItem_Click(object sender, EventArgs e)
		{
			var ofd = new FolderBrowserDialog();
			string oldPath;
			if (lvItems.Items.Count >0)
			{
				oldPath = lvItems.Items[lvItems.Items.Count - 1].Text;
			}
			else oldPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			ofd.SelectedPath = oldPath;
			if (ofd.ShowDialog(this) == DialogResult.OK)
			{
				lvItems.Items.Add(ofd.SelectedPath);
				CheckChanges();
			}
			else return;
		}

		private void btnDelItem_Click(object sender, EventArgs e)
		{
			if (lvItems.SelectedItems.Count>0)
			{
				foreach (ListViewItem item in lvItems.SelectedItems)
				{
					lvItems.Items.Remove(item);
				}
				CheckChanges();
			}
		}

		private void cbInfoSim_CheckedChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void numAILimite_ValueChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

        private void cbBeta_CheckedChanged(object sender, EventArgs e)
        {
            CheckChanges();
        }

		private void cbDropRateDisp_CheckedChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}

		private void cbSendDropRate_CheckedChanged(object sender, EventArgs e)
		{
			CheckChanges();
		}
	}
}
