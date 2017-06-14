using System;
using System.Windows.Forms;

namespace ffs2play
{
	public partial class dlgLogger : Form
	{
#if DEBUG
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbFiltreLog;
		private System.Windows.Forms.RadioButton rbDebugNormal;
		private System.Windows.Forms.RadioButton rbBavard;
		private System.Windows.Forms.RadioButton rbDebugDesactive;
#endif
		public dlgLogger(ffs2play pp)
		{
			InitializeComponent();
#if DEBUG
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbFiltreLog = new System.Windows.Forms.ComboBox();
			this.rbDebugNormal = new System.Windows.Forms.RadioButton();
			this.rbBavard = new System.Windows.Forms.RadioButton();
			this.rbDebugDesactive = new System.Windows.Forms.RadioButton();
			this.groupBox2.SuspendLayout();
			this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.cbFiltreLog);
			this.groupBox2.Controls.Add(this.rbDebugNormal);
			this.groupBox2.Controls.Add(this.rbBavard);
			this.groupBox2.Controls.Add(this.rbDebugDesactive);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.ForeColor = System.Drawing.Color.White;
			this.groupBox2.Location = new System.Drawing.Point(3, 118);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(278, 109);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Mode Debug";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(29, 61);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Filtre :";
			// 
			// cbFiltreLog
			// 
			this.cbFiltreLog.FormattingEnabled = true;
			this.cbFiltreLog.Items.AddRange(new object[] {
            "*",
            "^SCM*",
            "^P2P*",
            "^UDP*",
            "^PM*",
            "^ANA*",
            "^PEER*",
            "^AIMapping*",
			"^HTTPRequestThread*"});
			this.cbFiltreLog.Location = new System.Drawing.Point(81, 58);
			this.cbFiltreLog.Name = "cbFiltreLog";
			this.cbFiltreLog.Size = new System.Drawing.Size(121, 21);
			this.cbFiltreLog.TabIndex = 4;
			this.cbFiltreLog.TextChanged += new System.EventHandler(this.cbFiltreLog_TextChanged);
			// 
			// rbDebugNormal
			// 
			this.rbDebugNormal.AutoSize = true;
			this.rbDebugNormal.Location = new System.Drawing.Point(104, 19);
			this.rbDebugNormal.Name = "rbDebugNormal";
			this.rbDebugNormal.Size = new System.Drawing.Size(58, 17);
			this.rbDebugNormal.TabIndex = 1;
			this.rbDebugNormal.TabStop = true;
			this.rbDebugNormal.Text = "Normal";
			this.rbDebugNormal.UseVisualStyleBackColor = true;
			this.rbDebugNormal.CheckedChanged += new System.EventHandler(this.rbDebugNormal_CheckedChanged);
			// 
			// rbBavard
			// 
			this.rbBavard.AutoSize = true;
			this.rbBavard.Location = new System.Drawing.Point(203, 19);
			this.rbBavard.Name = "rbBavard";
			this.rbBavard.Size = new System.Drawing.Size(59, 17);
			this.rbBavard.TabIndex = 2;
			this.rbBavard.TabStop = true;
			this.rbBavard.Text = "Bavard";
			this.rbBavard.UseVisualStyleBackColor = true;
			this.rbBavard.CheckedChanged += new System.EventHandler(this.rbBavard_CheckedChanged);
			// 
			// rbDebugDesactive
			// 
			this.rbDebugDesactive.AutoSize = true;
			this.rbDebugDesactive.Location = new System.Drawing.Point(9, 19);
			this.rbDebugDesactive.Name = "rbDebugDesactive";
			this.rbDebugDesactive.Size = new System.Drawing.Size(73, 17);
			this.rbDebugDesactive.TabIndex = 3;
			this.rbDebugDesactive.TabStop = true;
			this.rbDebugDesactive.Text = "Désactivé";
			this.rbDebugDesactive.UseVisualStyleBackColor = true;
			this.rbDebugDesactive.CheckedChanged += new System.EventHandler(this.rbDebugDesactive_CheckedChanged);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
#endif
			p = pp;
			if (Properties.Settings.Default.LogVisible) cbLogVisible.Checked = true;
			else cbLogVisible.Checked = false;
			Log = Logger.Instance;
#if DEBUG
			cbFiltreLog.Text = Log.Filter;
			switch (Log.DebugLevel)
			{
				case 0:
					rbDebugDesactive.Checked = true;
					break;
				case 1:
					rbDebugNormal.Checked = true;
					break;
				case 2:
					rbBavard.Checked = true;
					break;
				default:
					break;
			}
#endif
		}
		private Logger Log;

		private ffs2play p;

		private void btnFermer_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cbLogVisible_CheckedChanged(object sender, EventArgs e)
		{
			p.LogVisible = cbLogVisible.Checked;
		}
#if DEBUG
		private void cbFiltreLog_TextChanged(object sender, EventArgs e)
		{
			Log.Filter = cbFiltreLog.Text;
		}

		private void rbDebugDesactive_CheckedChanged(object sender, EventArgs e)
		{
			if (rbDebugDesactive.Checked==true)
			{
				Log.DebugLevel = 0;
			}
		}

		private void rbDebugNormal_CheckedChanged(object sender, EventArgs e)
		{
			if (rbDebugNormal.Checked == true)
			{
				Log.DebugLevel = 1;
			}
		}

		private void rbBavard_CheckedChanged(object sender, EventArgs e)
		{
			if (rbBavard.Checked == true)
			{
				Log.DebugLevel = 2;
			}
		}
#endif
	}
}
