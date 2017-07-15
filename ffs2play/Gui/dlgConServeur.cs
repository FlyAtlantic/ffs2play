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
 * dlgConServeur.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;

namespace ffs2play
{
	public partial class dlgConServeur : Form
	{
		private List<PirepUser> Users;
		private int position;
		private PirepManager pm;
		public dlgConServeur(bool Direct=false)
		{
			InitializeComponent();
			pm = PirepManager.Instance;
			Users = new List<PirepUser>();
			LoadUsers();
			foreach (PirepUser user in Users)
			{
				ListUsers.SelectedNode = ListUsers.Nodes[0];
				ListUsers.SelectedNode = ListUsers.SelectedNode.Nodes.Add(user.Profil);
			}
			if (ListUsers.Nodes[0].GetNodeCount(false) > 0)
			{
				string DernierUser = Properties.Settings.Default.DernierUser;
				foreach (TreeNode node in ListUsers.Nodes[0].Nodes)
				{
					if (DernierUser ==  node.Text )
					{
						ListUsers.SelectedNode = node;
						position = node.Index;
						if (Direct)
						{
							pm.Connect(Users[position]);
							Close();
							DialogResult = DialogResult.OK;
						}
						return;
					}
				}
				ListUsers.SelectedNode = ListUsers.Nodes[0].Nodes[0];
				position = 0;
			}
			else
			{
				position = -1;
				ListUsers.SelectedNode = ListUsers.Nodes[0];
			}
		}

		private void btnAnnuler_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void OnNewUser_Click(object sender, EventArgs e)
		{
			int num = GetNumNouveau ();
			string Profil = "Nouveau";
			if (num > 0)
			{
				Profil += " " + num;
			}
			ListUsers.SelectedNode= ListUsers.Nodes[0];
			PirepUser User = new PirepUser();
			User.Profil = Profil;
			User.Login = "Login";
			User.Password = Outils.Encrypt("");
			User.URL = "ffs2play.fr";
			Users.Add(User);
			ListUsers.SelectedNode = ListUsers.SelectedNode.Nodes.Add(Profil);
			tbProfil.Text = User.Profil;
			tbLogin.Text = User.Login;
			tbPassword.Text = "";
			tbURL.Text = User.URL;
			ListUsers.ExpandAll();
			btnSupprimer.Enabled = true;
			btnSupprimer.BackColor = Color.LightGray;
			btnSupprimer.ForeColor = Color.Black;
			SaveUsers();
		}

		private int GetNumNouveau()
		{
			int result = 0;
			TreeNodeCollection nodes = ListUsers.Nodes[0].Nodes;
			foreach (TreeNode n in nodes)
			{
				if (n.Text.StartsWith("Nouveau"))
				{
					result++;
				}
			}
			return result;
		}

		private void LoadUsers()
		{
			using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Users )))
			{
				if (ms.Length != 0)
				{
					BinaryFormatter bf = new BinaryFormatter();
					Users = (List<PirepUser>)bf.Deserialize(ms);
				}
			}
		}

		private void UpdateUser()
		{
			if (position < 0) return;
			PirepUser User = Users[position];
			User.Profil = tbProfil.Text;
			User.Login = tbLogin.Text;
			User.Password = Outils.Encrypt(tbPassword.Text);
			tbURL.Text = tbURL.Text.ToLower();
			if (tbURL.Text.StartsWith("http://"))
			{
				tbURL.Text = tbURL.Text.Substring(7);
			}
			User.URL = tbURL.Text;
			ListUsers.SelectedNode.Text = User.Profil;
			Users[position] = User;
		}

		private void SaveUsers()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(ms, Users);
				ms.Position = 0;
				byte[] buffer = new byte[(int)ms.Length];
				ms.Read(buffer, 0, buffer.Length);
				Properties.Settings.Default.Users = Convert.ToBase64String(buffer);
				// Sauvegarde de la configuration
				Properties.Settings.Default.Save();
			}
		}

		private void btnAppliquer_Click(object sender, EventArgs e)
		{
			UpdateUser();
			SaveUsers();
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;
		}

		private void ListUsers_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Parent != null)
			{
				string Profil = e.Node.Text;
				position = e.Node.Index;
				tbProfil.Text = Users[position].Profil;
				tbLogin.Text = Users[position].Login;
				tbPassword.Text = Outils.Decrypt(Users[position].Password);
				tbURL.Text = Users[position].URL;
				btnConnexion.Enabled = true;
				btnConnexion.BackColor = Color.DodgerBlue;
				btnConnexion.ForeColor = Color.White;
				btnAppliquer.Enabled = false;
				btnAppliquer.BackColor = Color.LightGray;
				btnAppliquer.ForeColor = Color.Black;
				btnSupprimer.Enabled = true;
				btnSupprimer.BackColor = Color.DodgerBlue;
				btnSupprimer.ForeColor = Color.White;
				tbProfil.Enabled = true;
				tbLogin.Enabled = true;
				tbPassword.Enabled = true;
				tbURL.Enabled = true;
			}
			else
			{
				position = -1;
				btnConnexion.Enabled = false;
				btnConnexion.BackColor = Color.LightGray;
				btnConnexion.ForeColor = Color.Black;
				btnAppliquer.Enabled = false;
				btnAppliquer.BackColor = Color.LightGray;
				btnAppliquer.ForeColor = Color.Black;
				btnSupprimer.Enabled = false;
				btnSupprimer.BackColor = Color.LightGray;
				btnSupprimer.ForeColor = Color.Black;
				tbProfil.Enabled = false;
				tbLogin.Enabled = false;
				tbProfil.Text = "";
				tbLogin.Text = "";
				tbPassword.Text = "";
				tbPassword.Enabled= false;
				tbURL.Text = "";
				tbURL.Enabled = false;
			}
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;

		}

		private void btnSupprimer_Click(object sender, EventArgs e)
		{
			if (position != -1)
			{
				Users.RemoveAt(position);
				ListUsers.Nodes[0].Nodes[position].Remove();
				if (Users.Count == 0)
				{
					ListUsers.SelectedNode = ListUsers.Nodes[0];
				}
				else
				{
					if (position > (Users.Count - 1)) ListUsers.SelectedNode = ListUsers.Nodes[0].Nodes[Users.Count - 1];
				}

			}
			SaveUsers();
			btnAppliquer.Enabled = false;
			btnAppliquer.BackColor = Color.LightGray;
			btnAppliquer.ForeColor = Color.Black;
		}

		private void tbLogin_TextChanged(object sender, EventArgs e)
		{
			if (position < 0) return;
			if (tbLogin.Text != Users[position].Login)
			{
				btnAppliquer.Enabled = true;
				btnAppliquer.BackColor = Color.DodgerBlue;
				btnAppliquer.ForeColor = Color.White;
			}
		}

		private void tbPassword_TextChanged(object sender, EventArgs e)
		{
			if (position < 0) return;
			if (tbLogin.Text != Outils.Decrypt(Users[position].Password))
			{
				btnAppliquer.Enabled = true;
				btnAppliquer.BackColor = Color.DodgerBlue;
				btnAppliquer.ForeColor = Color.White;
			}
		}

		private DialogResult MessageModifications()
		{
			DialogResult dialogResult = MessageBox.Show("Vouler vous enregistrer les modifications?", "Enregistrement des modifications", MessageBoxButtons.YesNoCancel);
			if (dialogResult == DialogResult.Yes)
			{
				UpdateUser();
				SaveUsers();
			}
			return dialogResult;
		}
		private void ListUsers_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (btnAppliquer.Enabled)
			{
				DialogResult dialogResult = MessageModifications();
				if (dialogResult == DialogResult.Cancel)
				{
					e.Cancel = true;
				}
			}
		}

		private void btnConnexion_Click(object sender, EventArgs e)
		{
			if (btnAppliquer.Enabled)
			{
				UpdateUser();
				SaveUsers();
			}
			Properties.Settings.Default.DernierUser = tbProfil.Text;
			Properties.Settings.Default.Save();
			if (pm.IsConnected()) pm.Disconnect(PirepManager.Disco_mode.Reco);
			else pm.Connect(Users[position]);
			Close();
			DialogResult = DialogResult.OK;
		}

		private void ListUsers_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Parent!=null)
			{
				ListUsers.SelectedNode = e.Node;
				btnConnexion_Click(new object(), new EventArgs());
			}
		}

		private void tbURL_TextChanged(object sender, EventArgs e)
		{
			if (position < 0) return;
			if (tbURL.Text != Users[position].URL)
			{
				btnAppliquer.Enabled = true;
				btnAppliquer.BackColor = Color.DodgerBlue;
				btnAppliquer.ForeColor = Color.White;
			}
		}

		private void tbProfil_TextChanged(object sender, EventArgs e)
		{
			if (position < 0) return;
			if (tbProfil.Text != Users[position].Profil)
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

		private void cbCompat_CheckedChanged(object sender, EventArgs e)
		{
			if (position < 0) return;
			else
			{
				btnAppliquer.Enabled = false;
				btnAppliquer.BackColor = Color.LightGray;
				btnAppliquer.ForeColor = Color.Black;
			}
		}
	}
}
