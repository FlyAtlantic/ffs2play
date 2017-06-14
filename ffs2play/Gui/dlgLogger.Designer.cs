namespace ffs2play
{
	partial class dlgLogger
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
			this.btnFermer = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbLogVisible = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.btnFermer, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(284, 261);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// btnFermer
			// 
			this.btnFermer.BackColor = System.Drawing.Color.DodgerBlue;
			this.btnFermer.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnFermer.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnFermer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnFermer.ForeColor = System.Drawing.Color.White;
			this.btnFermer.Location = new System.Drawing.Point(206, 233);
			this.btnFermer.Name = "btnFermer";
			this.btnFermer.Size = new System.Drawing.Size(75, 25);
			this.btnFermer.TabIndex = 3;
			this.btnFermer.Text = "Fermer";
			this.btnFermer.UseVisualStyleBackColor = false;
			this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbLogVisible);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.ForeColor = System.Drawing.Color.White;
			this.groupBox1.Location = new System.Drawing.Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(278, 109);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Fenêtre de Log";
			// 
			// cbLogVisible
			// 
			this.cbLogVisible.AutoSize = true;
			this.cbLogVisible.Location = new System.Drawing.Point(21, 48);
			this.cbLogVisible.Name = "cbLogVisible";
			this.cbLogVisible.Size = new System.Drawing.Size(131, 17);
			this.cbLogVisible.TabIndex = 0;
			this.cbLogVisible.Text = "Afficher la zone de log";
			this.cbLogVisible.UseVisualStyleBackColor = true;
			this.cbLogVisible.CheckedChanged += new System.EventHandler(this.cbLogVisible_CheckedChanged);
			// 
			// dlgLogger
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.DimGray;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "dlgLogger";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Option Log";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button btnFermer;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox cbLogVisible;
	}
}