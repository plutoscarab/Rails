using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for UpgradeTrain.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class UpgradeTrain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button upgradeEngineButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button addFreightCarButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label checkmark1;
		private System.Windows.Forms.Label checkmark2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public UpgradeTrain(int engineType, int cars)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			switch(engineType)
			{
				case 0:
					checkmark1.Top = label1.Top;
					break;
				case 1:
					checkmark1.Top = label2.Top;
					break;
				case 2:
					checkmark1.Top = label3.Top;
					break;
				case 3:
					checkmark1.Top = label4.Top;
					upgradeEngineButton.Enabled = false;
					break;
			}

			switch(cars)
			{
				case 2:
					checkmark2.Top = label9.Top;
					break;
				case 3:
					checkmark2.Top = label8.Top;
					break;
				case 4:
					checkmark2.Top = label7.Top;
					addFreightCarButton.Enabled = false;
					break;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UpgradeTrain));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkmark1 = new System.Windows.Forms.Label();
			this.upgradeEngineButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkmark2 = new System.Windows.Forms.Label();
			this.addFreightCarButton = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.checkmark1,
																					this.upgradeEngineButton,
																					this.label4,
																					this.label3,
																					this.label2,
																					this.label1});
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(136, 152);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Upgrade Engine";
			// 
			// checkmark1
			// 
			this.checkmark1.Font = new System.Drawing.Font("Wingdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.checkmark1.Location = new System.Drawing.Point(8, 24);
			this.checkmark1.Name = "checkmark1";
			this.checkmark1.Size = new System.Drawing.Size(16, 16);
			this.checkmark1.TabIndex = 5;
			this.checkmark1.Text = "ü";
			// 
			// upgradeEngineButton
			// 
			this.upgradeEngineButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.upgradeEngineButton.Location = new System.Drawing.Point(16, 120);
			this.upgradeEngineButton.Name = "upgradeEngineButton";
			this.upgradeEngineButton.Size = new System.Drawing.Size(104, 23);
			this.upgradeEngineButton.TabIndex = 4;
			this.upgradeEngineButton.Text = "Upgrade for €20";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(32, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "Maglev: 1h 20m";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Diesel: 1h 36m";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Electric: 2h 00m";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Steam: 2h 40m";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.checkmark2,
																					this.addFreightCarButton,
																					this.label7,
																					this.label8,
																					this.label9});
			this.groupBox2.Location = new System.Drawing.Point(168, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(136, 152);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Upgrade Freight Cars";
			// 
			// checkmark2
			// 
			this.checkmark2.Font = new System.Drawing.Font("Wingdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.checkmark2.Location = new System.Drawing.Point(8, 24);
			this.checkmark2.Name = "checkmark2";
			this.checkmark2.Size = new System.Drawing.Size(16, 16);
			this.checkmark2.TabIndex = 5;
			this.checkmark2.Text = "ü";
			// 
			// addFreightCarButton
			// 
			this.addFreightCarButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.addFreightCarButton.Location = new System.Drawing.Point(16, 120);
			this.addFreightCarButton.Name = "addFreightCarButton";
			this.addFreightCarButton.Size = new System.Drawing.Size(104, 23);
			this.addFreightCarButton.TabIndex = 4;
			this.addFreightCarButton.Text = "Add car for €20";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(32, 72);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 16);
			this.label7.TabIndex = 2;
			this.label7.Text = "Four cars";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(32, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(88, 16);
			this.label8.TabIndex = 1;
			this.label8.Text = "Three cars";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(32, 24);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(96, 16);
			this.label9.TabIndex = 0;
			this.label9.Text = "Two cars";
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.Location = new System.Drawing.Point(120, 184);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "Cancel";
			// 
			// UpgradeTrain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(314, 224);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.button1,
																		  this.groupBox2,
																		  this.groupBox1});
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpgradeTrain";
			this.ShowInTaskbar = false;
			this.Text = "Rails - Upgrade Train";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
