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
 * dlgResolAI.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


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
		private List<string> CategoryList;
		private List<string> ATCTypeList;
		private List<string> ATCModelList;
		private List<string> EditeurList;
		private bool Initialised;

		public dlgResolAI(string RemoteAI)
		{
			Initialised = false;
			InitializeComponent();
			tbRemoteAI.Text = RemoteAI;
			m_Mapping = AIMapping.Instance;
			CategoryList = m_Mapping.GetCategoryList();
			CategoryList.Insert(0, "*");
			cbCategorie.DataSource = CategoryList;
			EditeurList = m_Mapping.GetEditeurList();
			EditeurList.Insert(0, "*");
			cbEditeur.DataSource = EditeurList;
			ATCTypeList = m_Mapping.GetATCTypeList();
			ATCTypeList.Insert(0, "*");
			cbType.DataSource = ATCTypeList;
			ATCModelList = m_Mapping.GetATCModelList();
			ATCModelList.Insert(0, "*");
			cbModel.DataSource = ATCModelList;
			UpdateAITitreDispo();
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
			Initialised = true;
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

		private void UpdateAITitreDispo()
		{
			ListeLocal = m_Mapping.GetAITitreDispo(cbCategorie.Text, cbEditeur.Text, cbType.Text, cbModel.Text);
			cbAIRemplacement.DataSource = ListeLocal;
		}

		private void cbCategorie_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Initialised)
			{
				UpdateAITitreDispo();
			}
		}

		private void cbEditeur_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Initialised)
			{
				UpdateAITitreDispo();
			}
		}

		private void cbType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Initialised)
			{
				UpdateAITitreDispo();
			}
		}

		private void cbModel_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Initialised)
			{
				UpdateAITitreDispo();
			}
		}
	}
}
