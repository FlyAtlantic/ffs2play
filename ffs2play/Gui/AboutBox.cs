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
