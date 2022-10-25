
// NewGame.cs

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;

namespace Rails
{
	/// <summary>
	/// Summary description for NewGame.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class NewGame : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.RadioButton seedOption;
		private System.Windows.Forms.RadioButton galleryOption;
		private System.Windows.Forms.RadioButton reuseOption;
		private System.Windows.Forms.RadioButton randomOption;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox fastStart;
		private System.Windows.Forms.CheckBox cityIncentives;
		private System.Windows.Forms.CheckBox limitedCommodities;
		private System.Windows.Forms.CheckBox automaticTrack;
		private System.Windows.Forms.CheckBox firstToCityBonus;
		private System.Windows.Forms.CheckBox groupedContracts;

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.TextBox textBox6;

		private System.Windows.Forms.CheckBox none5;
		private System.Windows.Forms.CheckBox computer5;
		private System.Windows.Forms.CheckBox human5;
		private System.Windows.Forms.CheckBox none4;
		private System.Windows.Forms.CheckBox computer4;
		private System.Windows.Forms.CheckBox human4;
		private System.Windows.Forms.CheckBox none3;
		private System.Windows.Forms.CheckBox computer3;
		private System.Windows.Forms.CheckBox human3;
		private System.Windows.Forms.CheckBox none2;
		private System.Windows.Forms.CheckBox computer2;
		private System.Windows.Forms.CheckBox human2;
		private System.Windows.Forms.CheckBox none1;
		private System.Windows.Forms.CheckBox computer1;
		private System.Windows.Forms.CheckBox human1;
		private System.Windows.Forms.CheckBox none0;
		private System.Windows.Forms.CheckBox computer0;
		private System.Windows.Forms.CheckBox human0;

		public bool SavedMap;
		public object Seed;
		public Type MapType;
		public Options Options;
		CheckBox[] humanButtons, computerButtons, noneButtons;
		public bool RandomMap = true;
		Random rand = new Random();
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox fundsGoal;
		private System.Windows.Forms.CheckBox newStyle;
		TextBox[] nameBoxes;

		public NewGame()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			humanButtons = new CheckBox[] {human0, human1, human2, human3, human4, human5};
			computerButtons = new CheckBox[] {computer0, computer1, computer2, computer3, computer4, computer5};
			noneButtons = new CheckBox[] {none0, none1, none2, none3, none4, none5};
			nameBoxes = new TextBox[] {textBox1, textBox2, textBox3, textBox4, textBox5, textBox6};
			for (int i=0; i<6; i++)
			{
				humanButtons[i].BackColor = computerButtons[i].BackColor = noneButtons[i].BackColor
					= Game.TrackColor[i];
			}
			Options.FundsGoal = GameState.DefaultFundsGoal;
			fundsGoal.Text = Options.FundsGoal.ToString();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewGame));
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.newStyle = new System.Windows.Forms.CheckBox();
			this.seedOption = new System.Windows.Forms.RadioButton();
			this.galleryOption = new System.Windows.Forms.RadioButton();
			this.reuseOption = new System.Windows.Forms.RadioButton();
			this.randomOption = new System.Windows.Forms.RadioButton();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.groupedContracts = new System.Windows.Forms.CheckBox();
			this.firstToCityBonus = new System.Windows.Forms.CheckBox();
			this.automaticTrack = new System.Windows.Forms.CheckBox();
			this.limitedCommodities = new System.Windows.Forms.CheckBox();
			this.cityIncentives = new System.Windows.Forms.CheckBox();
			this.fastStart = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.none5 = new System.Windows.Forms.CheckBox();
			this.computer5 = new System.Windows.Forms.CheckBox();
			this.human5 = new System.Windows.Forms.CheckBox();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.none4 = new System.Windows.Forms.CheckBox();
			this.computer4 = new System.Windows.Forms.CheckBox();
			this.human4 = new System.Windows.Forms.CheckBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.none3 = new System.Windows.Forms.CheckBox();
			this.computer3 = new System.Windows.Forms.CheckBox();
			this.human3 = new System.Windows.Forms.CheckBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.none2 = new System.Windows.Forms.CheckBox();
			this.computer2 = new System.Windows.Forms.CheckBox();
			this.human2 = new System.Windows.Forms.CheckBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.none1 = new System.Windows.Forms.CheckBox();
			this.computer1 = new System.Windows.Forms.CheckBox();
			this.human1 = new System.Windows.Forms.CheckBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.none0 = new System.Windows.Forms.CheckBox();
			this.computer0 = new System.Windows.Forms.CheckBox();
			this.human0 = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fundsGoal = new System.Windows.Forms.TextBox();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(20, 300);
			this.button1.Name = "button1";
			this.button1.TabIndex = 53;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(116, 300);
			this.button2.Name = "button2";
			this.button2.TabIndex = 54;
			this.button2.Text = "Cancel";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.newStyle,
																					this.seedOption,
																					this.galleryOption,
																					this.reuseOption,
																					this.randomOption});
			this.groupBox3.Location = new System.Drawing.Point(232, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(200, 116);
			this.groupBox3.TabIndex = 60;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Map";
			// 
			// newStyle
			// 
			this.newStyle.Checked = true;
			this.newStyle.CheckState = System.Windows.Forms.CheckState.Checked;
			this.newStyle.Location = new System.Drawing.Point(148, 16);
			this.newStyle.Name = "newStyle";
			this.newStyle.Size = new System.Drawing.Size(48, 32);
			this.newStyle.TabIndex = 64;
			this.newStyle.Text = "New style";
			this.newStyle.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// seedOption
			// 
			this.seedOption.Location = new System.Drawing.Point(12, 80);
			this.seedOption.Name = "seedOption";
			this.seedOption.Size = new System.Drawing.Size(164, 24);
			this.seedOption.TabIndex = 63;
			this.seedOption.Text = "Specify a map serial number";
			this.seedOption.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
			// 
			// galleryOption
			// 
			this.galleryOption.Location = new System.Drawing.Point(12, 60);
			this.galleryOption.Name = "galleryOption";
			this.galleryOption.Size = new System.Drawing.Size(164, 24);
			this.galleryOption.TabIndex = 62;
			this.galleryOption.Text = "Load a saved map";
			this.galleryOption.CheckedChanged += new System.EventHandler(this.galleryOption_CheckedChanged);
			// 
			// reuseOption
			// 
			this.reuseOption.Location = new System.Drawing.Point(12, 40);
			this.reuseOption.Name = "reuseOption";
			this.reuseOption.Size = new System.Drawing.Size(164, 24);
			this.reuseOption.TabIndex = 61;
			this.reuseOption.Text = "Re-use current map";
			this.reuseOption.CheckedChanged += new System.EventHandler(this.reuseOption_CheckedChanged);
			// 
			// randomOption
			// 
			this.randomOption.Checked = true;
			this.randomOption.Location = new System.Drawing.Point(12, 20);
			this.randomOption.Name = "randomOption";
			this.randomOption.Size = new System.Drawing.Size(136, 24);
			this.randomOption.TabIndex = 60;
			this.randomOption.TabStop = true;
			this.randomOption.Text = "Generate random map";
			this.randomOption.CheckedChanged += new System.EventHandler(this.randomOption_CheckedChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.groupedContracts,
																					this.firstToCityBonus,
																					this.automaticTrack,
																					this.limitedCommodities,
																					this.cityIncentives,
																					this.fastStart});
			this.groupBox4.Location = new System.Drawing.Point(232, 140);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(200, 176);
			this.groupBox4.TabIndex = 61;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Variations";
			// 
			// groupedContracts
			// 
			this.groupedContracts.Location = new System.Drawing.Point(12, 144);
			this.groupedContracts.Name = "groupedContracts";
			this.groupedContracts.Size = new System.Drawing.Size(152, 24);
			this.groupedContracts.TabIndex = 6;
			this.groupedContracts.Text = "Grouped contracts";
			// 
			// firstToCityBonus
			// 
			this.firstToCityBonus.Location = new System.Drawing.Point(12, 120);
			this.firstToCityBonus.Name = "firstToCityBonus";
			this.firstToCityBonus.Size = new System.Drawing.Size(152, 24);
			this.firstToCityBonus.TabIndex = 5;
			this.firstToCityBonus.Text = "First-to-city bonuses";
			// 
			// automaticTrack
			// 
			this.automaticTrack.Location = new System.Drawing.Point(12, 24);
			this.automaticTrack.Name = "automaticTrack";
			this.automaticTrack.Size = new System.Drawing.Size(156, 24);
			this.automaticTrack.TabIndex = 4;
			this.automaticTrack.Text = "Automatic track-building";
			this.automaticTrack.CheckedChanged += new System.EventHandler(this.automaticTrack_CheckedChanged);
			// 
			// limitedCommodities
			// 
			this.limitedCommodities.Location = new System.Drawing.Point(12, 96);
			this.limitedCommodities.Name = "limitedCommodities";
			this.limitedCommodities.Size = new System.Drawing.Size(152, 24);
			this.limitedCommodities.TabIndex = 3;
			this.limitedCommodities.Text = "Limited commodities";
			// 
			// cityIncentives
			// 
			this.cityIncentives.Location = new System.Drawing.Point(12, 72);
			this.cityIncentives.Name = "cityIncentives";
			this.cityIncentives.Size = new System.Drawing.Size(156, 24);
			this.cityIncentives.TabIndex = 2;
			this.cityIncentives.Text = "City incentives";
			// 
			// fastStart
			// 
			this.fastStart.Location = new System.Drawing.Point(12, 48);
			this.fastStart.Name = "fastStart";
			this.fastStart.Size = new System.Drawing.Size(152, 24);
			this.fastStart.TabIndex = 1;
			this.fastStart.Text = "Fast start";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.textBox6,
																					this.none5,
																					this.computer5,
																					this.human5,
																					this.textBox5,
																					this.none4,
																					this.computer4,
																					this.human4,
																					this.textBox4,
																					this.none3,
																					this.computer3,
																					this.human3,
																					this.textBox3,
																					this.none2,
																					this.computer2,
																					this.human2,
																					this.textBox2,
																					this.none1,
																					this.computer1,
																					this.human1,
																					this.textBox1,
																					this.none0,
																					this.computer0,
																					this.human0});
			this.groupBox1.Location = new System.Drawing.Point(16, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(204, 244);
			this.groupBox1.TabIndex = 64;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Players";
			// 
			// textBox6
			// 
			this.textBox6.Enabled = false;
			this.textBox6.Location = new System.Drawing.Point(100, 204);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(96, 21);
			this.textBox6.TabIndex = 86;
			this.textBox6.Text = "Not playing";
			this.textBox6.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none5
			// 
			this.none5.Appearance = System.Windows.Forms.Appearance.Button;
			this.none5.BackColor = System.Drawing.Color.Green;
			this.none5.Checked = true;
			this.none5.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none5.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none5.ForeColor = System.Drawing.Color.White;
			this.none5.Location = new System.Drawing.Point(68, 200);
			this.none5.Name = "none5";
			this.none5.Size = new System.Drawing.Size(28, 28);
			this.none5.TabIndex = 85;
			this.none5.Tag = "n5";
			this.none5.Text = "none";
			this.none5.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer5
			// 
			this.computer5.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer5.BackColor = System.Drawing.Color.Green;
			this.computer5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer5.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer5.Image")));
			this.computer5.Location = new System.Drawing.Point(40, 200);
			this.computer5.Name = "computer5";
			this.computer5.Size = new System.Drawing.Size(28, 28);
			this.computer5.TabIndex = 84;
			this.computer5.Tag = "c5";
			this.computer5.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human5
			// 
			this.human5.Appearance = System.Windows.Forms.Appearance.Button;
			this.human5.BackColor = System.Drawing.Color.Green;
			this.human5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human5.Image = ((System.Drawing.Bitmap)(resources.GetObject("human5.Image")));
			this.human5.Location = new System.Drawing.Point(12, 200);
			this.human5.Name = "human5";
			this.human5.Size = new System.Drawing.Size(28, 28);
			this.human5.TabIndex = 83;
			this.human5.Tag = "h5";
			this.human5.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// textBox5
			// 
			this.textBox5.Enabled = false;
			this.textBox5.Location = new System.Drawing.Point(100, 168);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(96, 21);
			this.textBox5.TabIndex = 82;
			this.textBox5.Text = "Not playing";
			this.textBox5.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none4
			// 
			this.none4.Appearance = System.Windows.Forms.Appearance.Button;
			this.none4.BackColor = System.Drawing.Color.Green;
			this.none4.Checked = true;
			this.none4.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none4.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none4.ForeColor = System.Drawing.Color.White;
			this.none4.Location = new System.Drawing.Point(68, 164);
			this.none4.Name = "none4";
			this.none4.Size = new System.Drawing.Size(28, 28);
			this.none4.TabIndex = 81;
			this.none4.Tag = "n4";
			this.none4.Text = "none";
			this.none4.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer4
			// 
			this.computer4.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer4.BackColor = System.Drawing.Color.Green;
			this.computer4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer4.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer4.Image")));
			this.computer4.Location = new System.Drawing.Point(40, 164);
			this.computer4.Name = "computer4";
			this.computer4.Size = new System.Drawing.Size(28, 28);
			this.computer4.TabIndex = 80;
			this.computer4.Tag = "c4";
			this.computer4.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human4
			// 
			this.human4.Appearance = System.Windows.Forms.Appearance.Button;
			this.human4.BackColor = System.Drawing.Color.Green;
			this.human4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human4.Image = ((System.Drawing.Bitmap)(resources.GetObject("human4.Image")));
			this.human4.Location = new System.Drawing.Point(12, 164);
			this.human4.Name = "human4";
			this.human4.Size = new System.Drawing.Size(28, 28);
			this.human4.TabIndex = 79;
			this.human4.Tag = "h4";
			this.human4.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// textBox4
			// 
			this.textBox4.Enabled = false;
			this.textBox4.Location = new System.Drawing.Point(100, 132);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(96, 21);
			this.textBox4.TabIndex = 78;
			this.textBox4.Text = "Not playing";
			this.textBox4.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none3
			// 
			this.none3.Appearance = System.Windows.Forms.Appearance.Button;
			this.none3.BackColor = System.Drawing.Color.Green;
			this.none3.Checked = true;
			this.none3.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none3.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none3.ForeColor = System.Drawing.Color.White;
			this.none3.Location = new System.Drawing.Point(68, 128);
			this.none3.Name = "none3";
			this.none3.Size = new System.Drawing.Size(28, 28);
			this.none3.TabIndex = 77;
			this.none3.Tag = "n3";
			this.none3.Text = "none";
			this.none3.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer3
			// 
			this.computer3.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer3.BackColor = System.Drawing.Color.Green;
			this.computer3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer3.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer3.Image")));
			this.computer3.Location = new System.Drawing.Point(40, 128);
			this.computer3.Name = "computer3";
			this.computer3.Size = new System.Drawing.Size(28, 28);
			this.computer3.TabIndex = 76;
			this.computer3.Tag = "c3";
			this.computer3.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human3
			// 
			this.human3.Appearance = System.Windows.Forms.Appearance.Button;
			this.human3.BackColor = System.Drawing.Color.Green;
			this.human3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human3.Image = ((System.Drawing.Bitmap)(resources.GetObject("human3.Image")));
			this.human3.Location = new System.Drawing.Point(12, 128);
			this.human3.Name = "human3";
			this.human3.Size = new System.Drawing.Size(28, 28);
			this.human3.TabIndex = 75;
			this.human3.Tag = "h3";
			this.human3.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// textBox3
			// 
			this.textBox3.Enabled = false;
			this.textBox3.Location = new System.Drawing.Point(100, 96);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(96, 21);
			this.textBox3.TabIndex = 74;
			this.textBox3.Text = "Not playing";
			this.textBox3.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none2
			// 
			this.none2.Appearance = System.Windows.Forms.Appearance.Button;
			this.none2.BackColor = System.Drawing.Color.Green;
			this.none2.Checked = true;
			this.none2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none2.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none2.ForeColor = System.Drawing.Color.White;
			this.none2.Location = new System.Drawing.Point(68, 92);
			this.none2.Name = "none2";
			this.none2.Size = new System.Drawing.Size(28, 28);
			this.none2.TabIndex = 73;
			this.none2.Tag = "n2";
			this.none2.Text = "none";
			this.none2.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer2
			// 
			this.computer2.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer2.BackColor = System.Drawing.Color.Green;
			this.computer2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer2.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer2.Image")));
			this.computer2.Location = new System.Drawing.Point(40, 92);
			this.computer2.Name = "computer2";
			this.computer2.Size = new System.Drawing.Size(28, 28);
			this.computer2.TabIndex = 72;
			this.computer2.Tag = "c2";
			this.computer2.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human2
			// 
			this.human2.Appearance = System.Windows.Forms.Appearance.Button;
			this.human2.BackColor = System.Drawing.Color.Green;
			this.human2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human2.Image = ((System.Drawing.Bitmap)(resources.GetObject("human2.Image")));
			this.human2.Location = new System.Drawing.Point(12, 92);
			this.human2.Name = "human2";
			this.human2.Size = new System.Drawing.Size(28, 28);
			this.human2.TabIndex = 71;
			this.human2.Tag = "h2";
			this.human2.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// textBox2
			// 
			this.textBox2.Enabled = false;
			this.textBox2.Location = new System.Drawing.Point(100, 60);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(96, 21);
			this.textBox2.TabIndex = 70;
			this.textBox2.Text = "Not playing";
			this.textBox2.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none1
			// 
			this.none1.Appearance = System.Windows.Forms.Appearance.Button;
			this.none1.BackColor = System.Drawing.Color.Green;
			this.none1.Checked = true;
			this.none1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none1.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none1.ForeColor = System.Drawing.Color.White;
			this.none1.Location = new System.Drawing.Point(68, 56);
			this.none1.Name = "none1";
			this.none1.Size = new System.Drawing.Size(28, 28);
			this.none1.TabIndex = 69;
			this.none1.Tag = "n1";
			this.none1.Text = "none";
			this.none1.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer1
			// 
			this.computer1.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer1.BackColor = System.Drawing.Color.Green;
			this.computer1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer1.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer1.Image")));
			this.computer1.Location = new System.Drawing.Point(40, 56);
			this.computer1.Name = "computer1";
			this.computer1.Size = new System.Drawing.Size(28, 28);
			this.computer1.TabIndex = 68;
			this.computer1.Tag = "c1";
			this.computer1.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human1
			// 
			this.human1.Appearance = System.Windows.Forms.Appearance.Button;
			this.human1.BackColor = System.Drawing.Color.Green;
			this.human1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human1.Image = ((System.Drawing.Bitmap)(resources.GetObject("human1.Image")));
			this.human1.Location = new System.Drawing.Point(12, 56);
			this.human1.Name = "human1";
			this.human1.Size = new System.Drawing.Size(28, 28);
			this.human1.TabIndex = 67;
			this.human1.Tag = "h1";
			this.human1.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// textBox1
			// 
			this.textBox1.Enabled = false;
			this.textBox1.Location = new System.Drawing.Point(100, 24);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(96, 21);
			this.textBox1.TabIndex = 66;
			this.textBox1.Text = "Not playing";
			this.textBox1.TextChanged += new System.EventHandler(this.Name_Changed);
			// 
			// none0
			// 
			this.none0.Appearance = System.Windows.Forms.Appearance.Button;
			this.none0.BackColor = System.Drawing.Color.Green;
			this.none0.Checked = true;
			this.none0.CheckState = System.Windows.Forms.CheckState.Checked;
			this.none0.Font = new System.Drawing.Font("Tahoma", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.none0.ForeColor = System.Drawing.Color.White;
			this.none0.Location = new System.Drawing.Point(68, 20);
			this.none0.Name = "none0";
			this.none0.Size = new System.Drawing.Size(28, 28);
			this.none0.TabIndex = 65;
			this.none0.Tag = "n0";
			this.none0.Text = "none";
			this.none0.CheckedChanged += new System.EventHandler(this.NoneButton_Click);
			// 
			// computer0
			// 
			this.computer0.Appearance = System.Windows.Forms.Appearance.Button;
			this.computer0.BackColor = System.Drawing.Color.Green;
			this.computer0.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.computer0.Image = ((System.Drawing.Bitmap)(resources.GetObject("computer0.Image")));
			this.computer0.Location = new System.Drawing.Point(40, 20);
			this.computer0.Name = "computer0";
			this.computer0.Size = new System.Drawing.Size(28, 28);
			this.computer0.TabIndex = 64;
			this.computer0.Tag = "c0";
			this.computer0.CheckedChanged += new System.EventHandler(this.ComputerButton_Click);
			// 
			// human0
			// 
			this.human0.Appearance = System.Windows.Forms.Appearance.Button;
			this.human0.BackColor = System.Drawing.Color.Green;
			this.human0.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.human0.Image = ((System.Drawing.Bitmap)(resources.GetObject("human0.Image")));
			this.human0.Location = new System.Drawing.Point(12, 20);
			this.human0.Name = "human0";
			this.human0.Size = new System.Drawing.Size(28, 28);
			this.human0.TabIndex = 63;
			this.human0.Tag = "h0";
			this.human0.CheckedChanged += new System.EventHandler(this.HumanButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(20, 268);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(108, 20);
			this.label1.TabIndex = 65;
			this.label1.Text = "Victory condition:   €";
			// 
			// fundsGoal
			// 
			this.fundsGoal.Location = new System.Drawing.Point(136, 264);
			this.fundsGoal.Name = "fundsGoal";
			this.fundsGoal.Size = new System.Drawing.Size(60, 21);
			this.fundsGoal.TabIndex = 66;
			this.fundsGoal.Text = "250";
			this.fundsGoal.Validating += new System.ComponentModel.CancelEventHandler(this.textBox7_Validating);
			// 
			// NewGame
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(442, 336);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.fundsGoal,
																		  this.label1,
																		  this.groupBox1,
																		  this.groupBox4,
																		  this.groupBox3,
																		  this.button2,
																		  this.button1});
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewGame";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "New Game";
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
		
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		Player[] players;

		private void ValidateOk()
		{
			button1.Enabled = false;
			int n = 0;
			for (int i=0; i<6; i++)
			{
				if (nameBoxes[i].Enabled && nameBoxes[i].Text.Length == 0)
					return;
				if (!noneButtons[i].Checked)
					n++;
			}
			if (n < 1)
				return;
			players = new Player[n];
			int j = 0;
			bool h = false;
			for (int i=0; i<6; i++)
				if (!noneButtons[i].Checked)
				{
					bool human = humanButtons[i].Checked;
					h |= human;
					players[j++] = new Player(i, human, human ? nameBoxes[i].Text : null);
				}
//			if (!h)
//				return;
			button1.Enabled = true;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			Options.AutomaticTrackBuilding = automaticTrack.Checked;
			Options.CityIncentives = cityIncentives.Checked;
			Options.FastStart = fastStart.Checked;
			Options.FirstToCityBonuses = this.firstToCityBonus.Checked;
			Options.LimitedCommodities = limitedCommodities.Checked;
			Options.GroupedContracts = groupedContracts.Checked;

			if (galleryOption.Checked)
			{
				using (MapGallery form = new MapGallery())
				{
					DialogResult dr = form.ShowDialog();
					if (dr != DialogResult.OK)
					{
						DialogResult = DialogResult.Cancel;
						return;
					}
					Seed = form.Seed;
					MapType = form.MapType;
				}
			} 
			else if (seedOption.Checked)
			{
				using (SerialNumber form = new SerialNumber())
				{
					DialogResult dr = form.ShowDialog();
					if (dr != DialogResult.OK)
					{
						DialogResult = DialogResult.Cancel;
						return;
					}
					Seed = form.Seed;
					MapType = form.MapType;
				}
			}

			string subkey = "Software\\Pluto Scarab\\Rails\\Names";
			RegistryKey key = Registry.LocalMachine.OpenSubKey(subkey, true);
			if (key == null)
				key = Registry.LocalMachine.CreateSubKey(subkey);
			for (int i=0; i<6; i++)
				if (humanButtons[i].Checked)
					key.SetValue(Game.ColorName[i], nameBoxes[i].Text);
			key.Close();

			int n = players.Length;
			for (int i=0; i<n-1; i++)
			{
				int j = i + rand.Next(n - i);
				Player temp = players[i];
				players[i] = players[j];
				players[j] = temp;
			}
		}

		private void randomOption_CheckedChanged(object sender, System.EventArgs e)
		{
			RandomMap = randomOption.Checked;
			SavedMap = galleryOption.Checked || seedOption.Checked;
			newStyle.Enabled = randomOption.Checked;
		}

		public bool NewStyle
		{
			get
			{
				return newStyle.Checked;
			}
		}

		private void reuseOption_CheckedChanged(object sender, System.EventArgs e)
		{
			RandomMap = randomOption.Checked;
			SavedMap = galleryOption.Checked || seedOption.Checked;
		}

		private void galleryOption_CheckedChanged(object sender, System.EventArgs e)
		{
			RandomMap = randomOption.Checked;
			SavedMap = galleryOption.Checked || seedOption.Checked;
		}

		private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
		{
			RandomMap = randomOption.Checked;
			SavedMap = galleryOption.Checked || seedOption.Checked;
		}

		private void automaticTrack_CheckedChanged(object sender, System.EventArgs e)
		{
		}

		bool processingClick = false;

		private void HumanButton_Click(object sender, System.EventArgs e)
		{
			if (processingClick) return;
			processingClick = true;
			CheckBox cb = (CheckBox) sender;
			int i = int.Parse(((string) cb.Tag).Substring(1), CultureInfo.InvariantCulture);
			if (cb.Checked)
			{
				computerButtons[i].Checked = false;
				noneButtons[i].Checked = false;
				nameBoxes[i].Enabled = true;
				nameBoxes[i].ReadOnly = false;
				string subkey = "Software\\Pluto Scarab\\Rails\\Names";
				RegistryKey key = Registry.LocalMachine.OpenSubKey(subkey);
				nameBoxes[i].Text = Game.ColorName[i] + " player";
				if (key != null)
				{
					nameBoxes[i].Text = (string) key.GetValue(Game.ColorName[i], nameBoxes[i].Text);
					key.Close();
				}
				nameBoxes[i].Focus();
				nameBoxes[i].SelectAll();
			}
			else
			{
				nameBoxes[i].Enabled = false;
				nameBoxes[i].Text = Resource.GetString("NewGame.NotPlaying");
				noneButtons[i].Checked = true;
			}
			ValidateOk();
			processingClick = false;
		}

		private void ComputerButton_Click(object sender, System.EventArgs e)
		{
			if (processingClick) return;
			processingClick = true;
			CheckBox cb = (CheckBox) sender;
			int i = int.Parse(((string) cb.Tag).Substring(1), CultureInfo.InvariantCulture);
			if (cb.Checked)
			{
				humanButtons[i].Checked = false;
				noneButtons[i].Checked = false;
				nameBoxes[i].Enabled = true;
				nameBoxes[i].ReadOnly = true;
				nameBoxes[i].Text = Game.ColorName[i] + " bot";
			}
			else
			{
				nameBoxes[i].Text = Resource.GetString("NewGame.NotPlaying");
				nameBoxes[i].Enabled = false;
				noneButtons[i].Checked = true;
			}
			ValidateOk();
			processingClick = false;
		}

		private void NoneButton_Click(object sender, System.EventArgs e)
		{
			if (processingClick) return;
			processingClick = true;
			CheckBox cb = (CheckBox) sender;
			int i = int.Parse(((string) cb.Tag).Substring(1), CultureInfo.InvariantCulture);
			nameBoxes[i].Text = Resource.GetString("NewGame.NotPlaying");
			nameBoxes[i].Enabled = false;
			if (cb.Checked)
			{
				humanButtons[i].Checked = false;
				computerButtons[i].Checked = false;
			}
			ValidateOk();
			processingClick = false;
		}

		private void Name_Changed(object sender, System.EventArgs e)
		{
			ValidateOk();
		}

		private void textBox7_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string s = ((TextBox) sender).Text;
			int v = -1;
			try
			{
				v = int.Parse(s);
			}
			catch
			{
				e.Cancel = true;
				return;
			}
			if (v < 100)
				e.Cancel = true;
			Options.FundsGoal = v;
		}

		public Player[] Players
		{
			get
			{
				return players;
			}
		}
	}
}
