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
			cbShadow.Location = new Point(146, 21);
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
			cbEnableP2P.Checked = Properties.Settings.Default.P2PEnable;
			cbInfoSim.Checked = Properties.Settings.Default.P2PInfoEnable;
			numPort.Value = Properties.Settings.Default.P2PPort;
			numP2PTx.Value = Properties.Settings.Default.P2PRate;
			numAIRayon.Value = Properties.Settings.Default.P2PRadius;
			numAILimite.Value = Properties.Settings.Default.P2PAILimite;
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
			if (cbEnableP2P.Checked != Properties.Settings.Default.P2PEnable)
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
			if (cbEnableP2P.Checked != Properties.Settings.Default.P2PEnable)
			{
				Properties.Settings.Default.P2PEnable = cbEnableP2P.Checked;
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
				Properties.Settings.Default.P2PAILimite = (ushort)numAILimite.Value;
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
			Properties.Settings.Default.Save();
			Changes = false;
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;
		}

		private void UpdateEnable()
		{
			if (cbEnableP2P.Checked)
			{
				cbUPNP.Enabled = true;
#if DEBUG
                cbShadow.Enabled = true;
#endif
			}
			else
			{
				cbUPNP.Enabled = false;
#if DEBUG
                cbShadow.Enabled = false;
#endif
			}
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
	}
}
