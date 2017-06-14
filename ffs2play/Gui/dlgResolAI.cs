using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ffs2play
{
	public partial class dlgResolAI : Form
	{
		private string m_ExistingRule;
		private AIMapping m_Mapping;
		private List<string> ListeLocal;
		public dlgResolAI(string RemoteAI)
		{
			InitializeComponent();
			tbRemoteAI.Text = RemoteAI;
			m_Mapping = AIMapping.Instance;
			ListeLocal = m_Mapping.GetAITitreDispo();
			cbAIRemplacement.DataSource = ListeLocal;
			m_ExistingRule = m_Mapping.GetRule(RemoteAI);
			if (m_ExistingRule != "")
			{
				cbAIRemplacement.SelectedIndex= cbAIRemplacement.FindString(m_ExistingRule);
				btnSupprimer.Visible = true;
			}
			else
			{
				btnSupprimer.Visible = false;
			}
		}

		private void btnAnnuler_Click(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (m_ExistingRule != cbAIRemplacement.Text)
			{
				m_Mapping.AddRule(tbRemoteAI.Text, cbAIRemplacement.Text);
				DialogResult = DialogResult.OK;
			}
			else DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnSupprimer_Click(object sender, EventArgs e)
		{
			if (m_ExistingRule != "")
			{
				m_Mapping.DelRule(tbRemoteAI.Text);
				DialogResult = DialogResult.OK;
			}
			Close();
		}
	}
}
