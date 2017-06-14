namespace ffs2play
{
	partial class dlgResolAI
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnAnnuler = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tbRemoteAI = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.cbAIRemplacement = new System.Windows.Forms.ComboBox();
			this.btnSupprimer = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.DimGray;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(465, 110);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panel1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
			this.panel1.Controls.Add(this.btnSupprimer);
			this.panel1.Controls.Add(this.btnOK);
			this.panel1.Controls.Add(this.btnAnnuler);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 85);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(465, 25);
			this.panel1.TabIndex = 4;
			// 
			// btnOK
			// 
			this.btnOK.BackColor = System.Drawing.Color.DodgerBlue;
			this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.DodgerBlue;
			this.btnOK.FlatAppearance.BorderSize = 0;
			this.btnOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Blue;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.ForeColor = System.Drawing.Color.White;
			this.btnOK.Location = new System.Drawing.Point(306, -1);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = false;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnAnnuler
			// 
			this.btnAnnuler.BackColor = System.Drawing.Color.DodgerBlue;
			this.btnAnnuler.FlatAppearance.BorderColor = System.Drawing.Color.DodgerBlue;
			this.btnAnnuler.FlatAppearance.BorderSize = 0;
			this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAnnuler.ForeColor = System.Drawing.Color.White;
			this.btnAnnuler.Location = new System.Drawing.Point(387, -1);
			this.btnAnnuler.Name = "btnAnnuler";
			this.btnAnnuler.Size = new System.Drawing.Size(75, 23);
			this.btnAnnuler.TabIndex = 4;
			this.btnAnnuler.Text = "Annuler";
			this.btnAnnuler.UseVisualStyleBackColor = false;
			this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tbRemoteAI);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(3, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(226, 79);
			this.panel2.TabIndex = 5;
			// 
			// tbRemoteAI
			// 
			this.tbRemoteAI.Location = new System.Drawing.Point(3, 31);
			this.tbRemoteAI.Name = "tbRemoteAI";
			this.tbRemoteAI.ReadOnly = true;
			this.tbRemoteAI.Size = new System.Drawing.Size(220, 20);
			this.tbRemoteAI.TabIndex = 8;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(64, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Titre de l\'AI Distant";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.label2);
			this.panel3.Controls.Add(this.cbAIRemplacement);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(235, 3);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(227, 79);
			this.panel3.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(66, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(89, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Titre de l\'AI Local";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// cbAIRemplacement
			// 
			this.cbAIRemplacement.FormattingEnabled = true;
			this.cbAIRemplacement.Location = new System.Drawing.Point(3, 30);
			this.cbAIRemplacement.Name = "cbAIRemplacement";
			this.cbAIRemplacement.Size = new System.Drawing.Size(224, 21);
			this.cbAIRemplacement.TabIndex = 7;
			// 
			// btnSupprimer
			// 
			this.btnSupprimer.BackColor = System.Drawing.Color.OrangeRed;
			this.btnSupprimer.FlatAppearance.BorderColor = System.Drawing.Color.DodgerBlue;
			this.btnSupprimer.FlatAppearance.BorderSize = 0;
			this.btnSupprimer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Blue;
			this.btnSupprimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSupprimer.ForeColor = System.Drawing.Color.White;
			this.btnSupprimer.Location = new System.Drawing.Point(12, 0);
			this.btnSupprimer.Name = "btnSupprimer";
			this.btnSupprimer.Size = new System.Drawing.Size(75, 23);
			this.btnSupprimer.TabIndex = 6;
			this.btnSupprimer.Text = "Supprimer";
			this.btnSupprimer.UseVisualStyleBackColor = false;
			this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
			// 
			// dlgResolAI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(465, 110);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "dlgResolAI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Résolution d\'AI";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnAnnuler;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbRemoteAI;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbAIRemplacement;
		private System.Windows.Forms.Button btnSupprimer;
	}
}