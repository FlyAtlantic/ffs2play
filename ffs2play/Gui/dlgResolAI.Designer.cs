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
			this.btnSupprimer = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnAnnuler = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tbRemoteAI = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.cbAIRemplacement = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbEditeur = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.cbModel = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.cbType = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cbTypeMoteur = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbNbMoteurs = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbCategorie = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.DimGray;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(466, 232);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panel1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
			this.panel1.Controls.Add(this.btnSupprimer);
			this.panel1.Controls.Add(this.btnOK);
			this.panel1.Controls.Add(this.btnAnnuler);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 207);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(466, 25);
			this.panel1.TabIndex = 4;
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
			this.panel2.Size = new System.Drawing.Size(227, 54);
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
			this.panel3.Location = new System.Drawing.Point(236, 3);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(227, 54);
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
			// groupBox1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
			this.groupBox1.Controls.Add(this.cbEditeur);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.cbModel);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.cbType);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.cbTypeMoteur);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.cbNbMoteurs);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.cbCategorie);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.ForeColor = System.Drawing.Color.White;
			this.groupBox1.Location = new System.Drawing.Point(3, 63);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(460, 141);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Filtres";
			// 
			// cbEditeur
			// 
			this.cbEditeur.FormattingEnabled = true;
			this.cbEditeur.Items.AddRange(new object[] {
            "*"});
			this.cbEditeur.Location = new System.Drawing.Point(302, 97);
			this.cbEditeur.Name = "cbEditeur";
			this.cbEditeur.Size = new System.Drawing.Size(103, 21);
			this.cbEditeur.TabIndex = 11;
			this.cbEditeur.SelectedIndexChanged += new System.EventHandler(this.cbEditeur_SelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(254, 100);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(46, 13);
			this.label8.TabIndex = 10;
			this.label8.Text = "Editeur :";
			// 
			// cbModel
			// 
			this.cbModel.FormattingEnabled = true;
			this.cbModel.Items.AddRange(new object[] {
            "*"});
			this.cbModel.Location = new System.Drawing.Point(303, 57);
			this.cbModel.Name = "cbModel";
			this.cbModel.Size = new System.Drawing.Size(103, 21);
			this.cbModel.TabIndex = 9;
			this.cbModel.SelectedIndexChanged += new System.EventHandler(this.cbModel_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(254, 60);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(42, 13);
			this.label7.TabIndex = 8;
			this.label7.Text = "Model :";
			// 
			// cbType
			// 
			this.cbType.FormattingEnabled = true;
			this.cbType.Items.AddRange(new object[] {
            "*"});
			this.cbType.Location = new System.Drawing.Point(303, 20);
			this.cbType.Name = "cbType";
			this.cbType.Size = new System.Drawing.Size(103, 21);
			this.cbType.TabIndex = 7;
			this.cbType.SelectedIndexChanged += new System.EventHandler(this.cbType_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(254, 23);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(37, 13);
			this.label6.TabIndex = 6;
			this.label6.Text = "Type :";
			// 
			// cbTypeMoteur
			// 
			this.cbTypeMoteur.Enabled = false;
			this.cbTypeMoteur.FormattingEnabled = true;
			this.cbTypeMoteur.Location = new System.Drawing.Point(124, 97);
			this.cbTypeMoteur.Name = "cbTypeMoteur";
			this.cbTypeMoteur.Size = new System.Drawing.Size(103, 21);
			this.cbTypeMoteur.TabIndex = 5;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 97);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(84, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Type de moteur:";
			// 
			// cbNbMoteurs
			// 
			this.cbNbMoteurs.Enabled = false;
			this.cbNbMoteurs.FormattingEnabled = true;
			this.cbNbMoteurs.Items.AddRange(new object[] {
            "*",
            "0",
            "1",
            "2",
            "3",
            "4"});
			this.cbNbMoteurs.Location = new System.Drawing.Point(124, 57);
			this.cbNbMoteurs.Name = "cbNbMoteurs";
			this.cbNbMoteurs.Size = new System.Drawing.Size(103, 21);
			this.cbNbMoteurs.TabIndex = 3;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 60);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(105, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Nombre de moteurs :";
			// 
			// cbCategorie
			// 
			this.cbCategorie.FormattingEnabled = true;
			this.cbCategorie.Items.AddRange(new object[] {
            "*"});
			this.cbCategorie.Location = new System.Drawing.Point(124, 20);
			this.cbCategorie.Name = "cbCategorie";
			this.cbCategorie.Size = new System.Drawing.Size(103, 21);
			this.cbCategorie.TabIndex = 1;
			this.cbCategorie.SelectedIndexChanged += new System.EventHandler(this.cbCategorie_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(58, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Catégorie :";
			// 
			// dlgResolAI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(466, 232);
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
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbTypeMoteur;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbNbMoteurs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbCategorie;
        private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbModel;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox cbType;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cbEditeur;
		private System.Windows.Forms.Label label8;
	}
}