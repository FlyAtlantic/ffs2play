using System;
using System.Reflection;
using System.Windows.Forms;

namespace ffs2play
{
	partial class AboutBox : Form
	{
		public AboutBox()
		{
			InitializeComponent();
			AssemblyInfo entryAssemblyInfo = new AssemblyInfo(Assembly.GetEntryAssembly());
			this.Text = String.Format("À propos de {0}", entryAssemblyInfo.ProductTitle);
			this.labelProductName.Text = entryAssemblyInfo.Product;
			this.labelVersion.Text = String.Format("Version {0}", entryAssemblyInfo.Version);
			this.labelCopyright.Text = entryAssemblyInfo.Copyright;
			this.labelCompanyName.Text = "http://www.ffsimulateur2.fr";
			this.textBoxDescription.Text = entryAssemblyInfo.Description;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void labelCompanyName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.ffsimulateur2.fr");
			labelCompanyName.LinkVisited = true;
		}
	}
}
