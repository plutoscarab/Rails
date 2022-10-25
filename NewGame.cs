
// NewGame.cs

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Specialized;
using System.IO;

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

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox fundsGoal;

		private System.Windows.Forms.CheckBox classicStyle;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button localPlayerIcon;
		private System.Windows.Forms.Button botPlayerIcon;
		private System.Windows.Forms.Button remotePlayerIcon;
		private System.Windows.Forms.PictureBox icon0;
		private System.Windows.Forms.PictureBox color0;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox color1;
		private System.Windows.Forms.PictureBox icon1;
		private System.Windows.Forms.PictureBox color2;
		private System.Windows.Forms.PictureBox icon2;
		private System.Windows.Forms.PictureBox color3;
		private System.Windows.Forms.PictureBox icon3;
		private System.Windows.Forms.PictureBox color4;
		private System.Windows.Forms.PictureBox icon4;
		private System.Windows.Forms.PictureBox color5;
		private System.Windows.Forms.PictureBox icon5;
		private System.Windows.Forms.Button addBotButton;
		private System.Windows.Forms.Button remove5;
		private System.Windows.Forms.Button remove4;
		private System.Windows.Forms.Button remove3;
		private System.Windows.Forms.Button remove2;
		private System.Windows.Forms.Button remove1;
		private System.Windows.Forms.Button remove0;
		private System.Windows.Forms.Button addPersonButton;
		private System.Windows.Forms.ComboBox name0;
		private System.Windows.Forms.ComboBox name1;
		private System.Windows.Forms.ComboBox name2;
		private System.Windows.Forms.ComboBox name3;
		private System.Windows.Forms.ComboBox name4;
		private System.Windows.Forms.ComboBox name5;

		public NewGame()
		{
			if (Current != null)
				throw new InvalidOperationException();

			Current = this;

			InitializeComponent();

			fundsGoal.Text = GameState.DefaultFundsGoal.ToString();

			names = new ComboBox[] {name0, name1, name2, name3, name4, name5};
			colors = new PictureBox[] {color0, color1, color2, color3, color4, color5};
			icons = new PictureBox[] {icon0, icon1, icon2, icon3, icon4, icon5};
//			ready = new CheckBox[] {ready0, ready1, ready2, ready3, ready4, ready5};
			removes = new Button[] {remove0, remove1, remove2, remove3, remove4, remove5};
			hostOwned = new Control[] {fundsGoal, automaticTrack, cityIncentives, fastStart, limitedCommodities,
				groupedContracts, firstToCityBonus, randomOption, classicStyle, galleryOption, seedOption,
				addPersonButton, addBotButton, button1, button2};
			hostOwnedGroups = new Control[][] {names};
			isBot = new bool[6];
			channelIDs = new int[6];

			NetworkState.UsersUpdated += new UsersUpdatedEventHandler(UsersUpdated);
			Application.Idle += new EventHandler(Idle);
		}

		public bool SavedMap;
		public Map Map;
		public Options Options;
		public bool RandomMap = true;
		public int ChannelID;
		public int NeedToAddPerson;
		public int NameChangedIndex;
		public string NameChangedText;
		public bool NameChangedNeeded;
		public int ColorChangedIndex;
		public int ColorChangedColor;
		public bool ColorChangedNeeded;
		public int ReadyChangedIndex;
		public bool ReadyChangedReady;
		public bool ReadyChangedNeeded;

		Random rand = new Random();
		Player[] players;
		Rectangle mr = new Rectangle(0, 0, 864, 712);	// map rectangle
		ComboBox[] names;
		PictureBox[] colors, icons;
//		CheckBox[] ready;
		Button[] removes;
		Control[] hostOwned;
		Control[][] hostOwnedGroups;
		bool[] isBot;
		int[] channelIDs;
		bool updateOnIdle;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
//				if (host != null)
//					host.Stop();
//
//				if (channel != null)
//					channel.Stop();
//
//				if (hostStatus != null)
//					hostStatus.Dispose();

				NetworkState.UsersUpdated -= new UsersUpdatedEventHandler(UsersUpdated);
				Application.Idle -= new EventHandler(Idle);
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
			this.classicStyle = new System.Windows.Forms.CheckBox();
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
			this.name5 = new System.Windows.Forms.ComboBox();
			this.name4 = new System.Windows.Forms.ComboBox();
			this.name3 = new System.Windows.Forms.ComboBox();
			this.name2 = new System.Windows.Forms.ComboBox();
			this.name1 = new System.Windows.Forms.ComboBox();
			this.name0 = new System.Windows.Forms.ComboBox();
			this.addPersonButton = new System.Windows.Forms.Button();
			this.addBotButton = new System.Windows.Forms.Button();
			this.remove5 = new System.Windows.Forms.Button();
			this.remove4 = new System.Windows.Forms.Button();
			this.remove3 = new System.Windows.Forms.Button();
			this.remove2 = new System.Windows.Forms.Button();
			this.remove1 = new System.Windows.Forms.Button();
			this.remove0 = new System.Windows.Forms.Button();
			this.color5 = new System.Windows.Forms.PictureBox();
			this.icon5 = new System.Windows.Forms.PictureBox();
			this.color4 = new System.Windows.Forms.PictureBox();
			this.icon4 = new System.Windows.Forms.PictureBox();
			this.color3 = new System.Windows.Forms.PictureBox();
			this.icon3 = new System.Windows.Forms.PictureBox();
			this.color2 = new System.Windows.Forms.PictureBox();
			this.icon2 = new System.Windows.Forms.PictureBox();
			this.color1 = new System.Windows.Forms.PictureBox();
			this.icon1 = new System.Windows.Forms.PictureBox();
			this.label3 = new System.Windows.Forms.Label();
			this.color0 = new System.Windows.Forms.PictureBox();
			this.icon0 = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.fundsGoal = new System.Windows.Forms.TextBox();
			this.localPlayerIcon = new System.Windows.Forms.Button();
			this.botPlayerIcon = new System.Windows.Forms.Button();
			this.remotePlayerIcon = new System.Windows.Forms.Button();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(60, 300);
			this.button1.Name = "button1";
			this.button1.TabIndex = 53;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.CausesValidation = false;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(144, 300);
			this.button2.Name = "button2";
			this.button2.TabIndex = 54;
			this.button2.Text = "Cancel";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.classicStyle,
																					this.seedOption,
																					this.galleryOption,
																					this.reuseOption,
																					this.randomOption});
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(268, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(180, 136);
			this.groupBox3.TabIndex = 60;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Map";
			// 
			// classicStyle
			// 
			this.classicStyle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.classicStyle.Location = new System.Drawing.Point(32, 44);
			this.classicStyle.Name = "classicStyle";
			this.classicStyle.Size = new System.Drawing.Size(144, 20);
			this.classicStyle.TabIndex = 64;
			this.classicStyle.Text = "Use classic map style";
			this.classicStyle.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.classicStyle.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// seedOption
			// 
			this.seedOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.seedOption.Location = new System.Drawing.Point(12, 100);
			this.seedOption.Name = "seedOption";
			this.seedOption.Size = new System.Drawing.Size(164, 24);
			this.seedOption.TabIndex = 63;
			this.seedOption.Text = "Specify a map serial number";
			this.seedOption.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// galleryOption
			// 
			this.galleryOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.galleryOption.Location = new System.Drawing.Point(12, 80);
			this.galleryOption.Name = "galleryOption";
			this.galleryOption.Size = new System.Drawing.Size(164, 24);
			this.galleryOption.TabIndex = 62;
			this.galleryOption.Text = "Load a saved map";
			this.galleryOption.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// reuseOption
			// 
			this.reuseOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.reuseOption.Location = new System.Drawing.Point(12, 60);
			this.reuseOption.Name = "reuseOption";
			this.reuseOption.Size = new System.Drawing.Size(164, 24);
			this.reuseOption.TabIndex = 61;
			this.reuseOption.Text = "Re-use current map";
			this.reuseOption.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// randomOption
			// 
			this.randomOption.Checked = true;
			this.randomOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.randomOption.Location = new System.Drawing.Point(12, 20);
			this.randomOption.Name = "randomOption";
			this.randomOption.Size = new System.Drawing.Size(164, 24);
			this.randomOption.TabIndex = 60;
			this.randomOption.TabStop = true;
			this.randomOption.Text = "Generate random map";
			this.randomOption.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
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
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(268, 160);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(180, 176);
			this.groupBox4.TabIndex = 61;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Variations";
			// 
			// groupedContracts
			// 
			this.groupedContracts.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupedContracts.Location = new System.Drawing.Point(12, 144);
			this.groupedContracts.Name = "groupedContracts";
			this.groupedContracts.Size = new System.Drawing.Size(152, 24);
			this.groupedContracts.TabIndex = 6;
			this.groupedContracts.Text = "Grouped contracts";
			this.groupedContracts.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// firstToCityBonus
			// 
			this.firstToCityBonus.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.firstToCityBonus.Location = new System.Drawing.Point(12, 120);
			this.firstToCityBonus.Name = "firstToCityBonus";
			this.firstToCityBonus.Size = new System.Drawing.Size(152, 24);
			this.firstToCityBonus.TabIndex = 5;
			this.firstToCityBonus.Text = "First-to-city bonuses";
			this.firstToCityBonus.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// automaticTrack
			// 
			this.automaticTrack.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.automaticTrack.Location = new System.Drawing.Point(12, 24);
			this.automaticTrack.Name = "automaticTrack";
			this.automaticTrack.Size = new System.Drawing.Size(152, 24);
			this.automaticTrack.TabIndex = 4;
			this.automaticTrack.Text = "Automatic track-building";
			this.automaticTrack.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// limitedCommodities
			// 
			this.limitedCommodities.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.limitedCommodities.Location = new System.Drawing.Point(12, 96);
			this.limitedCommodities.Name = "limitedCommodities";
			this.limitedCommodities.Size = new System.Drawing.Size(152, 24);
			this.limitedCommodities.TabIndex = 3;
			this.limitedCommodities.Text = "Limited commodities";
			this.limitedCommodities.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// cityIncentives
			// 
			this.cityIncentives.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cityIncentives.Location = new System.Drawing.Point(12, 72);
			this.cityIncentives.Name = "cityIncentives";
			this.cityIncentives.Size = new System.Drawing.Size(152, 24);
			this.cityIncentives.TabIndex = 2;
			this.cityIncentives.Text = "City incentives";
			this.cityIncentives.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// fastStart
			// 
			this.fastStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fastStart.Location = new System.Drawing.Point(12, 48);
			this.fastStart.Name = "fastStart";
			this.fastStart.Size = new System.Drawing.Size(152, 24);
			this.fastStart.TabIndex = 1;
			this.fastStart.Text = "Fast start";
			this.fastStart.CheckedChanged += new System.EventHandler(this.optionCheckboxes_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.name5,
																					this.name4,
																					this.name3,
																					this.name2,
																					this.name1,
																					this.name0,
																					this.addPersonButton,
																					this.addBotButton,
																					this.remove5,
																					this.remove4,
																					this.remove3,
																					this.remove2,
																					this.remove1,
																					this.remove0,
																					this.color5,
																					this.icon5,
																					this.color4,
																					this.icon4,
																					this.color3,
																					this.icon3,
																					this.color2,
																					this.icon2,
																					this.color1,
																					this.icon1,
																					this.label3,
																					this.color0,
																					this.icon0,
																					this.label2});
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(240, 244);
			this.groupBox1.TabIndex = 64;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Players";
			// 
			// name5
			// 
			this.name5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name5.Location = new System.Drawing.Point(8, 184);
			this.name5.Name = "name5";
			this.name5.Size = new System.Drawing.Size(104, 21);
			this.name5.TabIndex = 40;
			this.name5.Tag = "5";
			this.name5.Visible = false;
			this.name5.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name5.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name5.Enter += new System.EventHandler(this.name_Enter);
			// 
			// name4
			// 
			this.name4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name4.Location = new System.Drawing.Point(8, 156);
			this.name4.Name = "name4";
			this.name4.Size = new System.Drawing.Size(104, 21);
			this.name4.TabIndex = 39;
			this.name4.Tag = "4";
			this.name4.Visible = false;
			this.name4.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name4.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name4.Enter += new System.EventHandler(this.name_Enter);
			// 
			// name3
			// 
			this.name3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name3.Location = new System.Drawing.Point(8, 128);
			this.name3.Name = "name3";
			this.name3.Size = new System.Drawing.Size(104, 21);
			this.name3.TabIndex = 38;
			this.name3.Tag = "3";
			this.name3.Visible = false;
			this.name3.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name3.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name3.Enter += new System.EventHandler(this.name_Enter);
			// 
			// name2
			// 
			this.name2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name2.Location = new System.Drawing.Point(8, 100);
			this.name2.Name = "name2";
			this.name2.Size = new System.Drawing.Size(104, 21);
			this.name2.TabIndex = 37;
			this.name2.Tag = "2";
			this.name2.Visible = false;
			this.name2.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name2.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name2.Enter += new System.EventHandler(this.name_Enter);
			// 
			// name1
			// 
			this.name1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name1.Location = new System.Drawing.Point(8, 72);
			this.name1.Name = "name1";
			this.name1.Size = new System.Drawing.Size(104, 21);
			this.name1.TabIndex = 36;
			this.name1.Tag = "1";
			this.name1.Visible = false;
			this.name1.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name1.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name1.Enter += new System.EventHandler(this.name_Enter);
			// 
			// name0
			// 
			this.name0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.name0.Location = new System.Drawing.Point(8, 44);
			this.name0.Name = "name0";
			this.name0.Size = new System.Drawing.Size(104, 21);
			this.name0.TabIndex = 35;
			this.name0.Tag = "0";
			this.name0.Visible = false;
			this.name0.TextChanged += new System.EventHandler(this.name0_TextChanged);
			this.name0.SelectedIndexChanged += new System.EventHandler(this.name0_TextChanged);
			this.name0.Enter += new System.EventHandler(this.name_Enter);
			// 
			// addPersonButton
			// 
			this.addPersonButton.Image = ((System.Drawing.Bitmap)(resources.GetObject("addPersonButton.Image")));
			this.addPersonButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.addPersonButton.Location = new System.Drawing.Point(24, 212);
			this.addPersonButton.Name = "addPersonButton";
			this.addPersonButton.Size = new System.Drawing.Size(96, 23);
			this.addPersonButton.TabIndex = 34;
			this.addPersonButton.Text = "Add a person";
			this.addPersonButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.addPersonButton.Click += new System.EventHandler(this.addPersonButton_Click);
			// 
			// addBotButton
			// 
			this.addBotButton.Image = ((System.Drawing.Bitmap)(resources.GetObject("addBotButton.Image")));
			this.addBotButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.addBotButton.Location = new System.Drawing.Point(128, 212);
			this.addBotButton.Name = "addBotButton";
			this.addBotButton.Size = new System.Drawing.Size(80, 23);
			this.addBotButton.TabIndex = 33;
			this.addBotButton.Text = "Add a bot";
			this.addBotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.addBotButton.Click += new System.EventHandler(this.addBotButton_Click);
			// 
			// remove5
			// 
			this.remove5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove5.Location = new System.Drawing.Point(176, 184);
			this.remove5.Name = "remove5";
			this.remove5.Size = new System.Drawing.Size(52, 23);
			this.remove5.TabIndex = 32;
			this.remove5.Tag = "5";
			this.remove5.Text = "Remove";
			this.remove5.Visible = false;
			this.remove5.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// remove4
			// 
			this.remove4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove4.Location = new System.Drawing.Point(176, 156);
			this.remove4.Name = "remove4";
			this.remove4.Size = new System.Drawing.Size(52, 23);
			this.remove4.TabIndex = 31;
			this.remove4.Tag = "4";
			this.remove4.Text = "Remove";
			this.remove4.Visible = false;
			this.remove4.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// remove3
			// 
			this.remove3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove3.Location = new System.Drawing.Point(176, 128);
			this.remove3.Name = "remove3";
			this.remove3.Size = new System.Drawing.Size(52, 23);
			this.remove3.TabIndex = 30;
			this.remove3.Tag = "3";
			this.remove3.Text = "Remove";
			this.remove3.Visible = false;
			this.remove3.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// remove2
			// 
			this.remove2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove2.Location = new System.Drawing.Point(176, 100);
			this.remove2.Name = "remove2";
			this.remove2.Size = new System.Drawing.Size(52, 23);
			this.remove2.TabIndex = 29;
			this.remove2.Tag = "2";
			this.remove2.Text = "Remove";
			this.remove2.Visible = false;
			this.remove2.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// remove1
			// 
			this.remove1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove1.Location = new System.Drawing.Point(176, 72);
			this.remove1.Name = "remove1";
			this.remove1.Size = new System.Drawing.Size(52, 23);
			this.remove1.TabIndex = 28;
			this.remove1.Tag = "1";
			this.remove1.Text = "Remove";
			this.remove1.Visible = false;
			this.remove1.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// remove0
			// 
			this.remove0.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove0.Location = new System.Drawing.Point(176, 44);
			this.remove0.Name = "remove0";
			this.remove0.Size = new System.Drawing.Size(52, 23);
			this.remove0.TabIndex = 27;
			this.remove0.Tag = "0";
			this.remove0.Text = "Remove";
			this.remove0.Visible = false;
			this.remove0.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// color5
			// 
			this.color5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color5.Location = new System.Drawing.Point(140, 184);
			this.color5.Name = "color5";
			this.color5.Size = new System.Drawing.Size(24, 24);
			this.color5.TabIndex = 25;
			this.color5.TabStop = false;
			this.color5.Visible = false;
			this.color5.Click += new System.EventHandler(this.color_Click);
			this.color5.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color5.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon5
			// 
			this.icon5.Location = new System.Drawing.Point(116, 188);
			this.icon5.Name = "icon5";
			this.icon5.Size = new System.Drawing.Size(16, 16);
			this.icon5.TabIndex = 24;
			this.icon5.TabStop = false;
			this.icon5.Visible = false;
			// 
			// color4
			// 
			this.color4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color4.Location = new System.Drawing.Point(140, 156);
			this.color4.Name = "color4";
			this.color4.Size = new System.Drawing.Size(24, 24);
			this.color4.TabIndex = 21;
			this.color4.TabStop = false;
			this.color4.Visible = false;
			this.color4.Click += new System.EventHandler(this.color_Click);
			this.color4.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color4.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon4
			// 
			this.icon4.Location = new System.Drawing.Point(116, 160);
			this.icon4.Name = "icon4";
			this.icon4.Size = new System.Drawing.Size(16, 16);
			this.icon4.TabIndex = 20;
			this.icon4.TabStop = false;
			this.icon4.Visible = false;
			// 
			// color3
			// 
			this.color3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color3.Location = new System.Drawing.Point(140, 128);
			this.color3.Name = "color3";
			this.color3.Size = new System.Drawing.Size(24, 24);
			this.color3.TabIndex = 17;
			this.color3.TabStop = false;
			this.color3.Visible = false;
			this.color3.Click += new System.EventHandler(this.color_Click);
			this.color3.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color3.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon3
			// 
			this.icon3.Location = new System.Drawing.Point(116, 132);
			this.icon3.Name = "icon3";
			this.icon3.Size = new System.Drawing.Size(16, 16);
			this.icon3.TabIndex = 16;
			this.icon3.TabStop = false;
			this.icon3.Visible = false;
			// 
			// color2
			// 
			this.color2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color2.Location = new System.Drawing.Point(140, 100);
			this.color2.Name = "color2";
			this.color2.Size = new System.Drawing.Size(24, 24);
			this.color2.TabIndex = 13;
			this.color2.TabStop = false;
			this.color2.Visible = false;
			this.color2.Click += new System.EventHandler(this.color_Click);
			this.color2.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color2.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon2
			// 
			this.icon2.Image = ((System.Drawing.Bitmap)(resources.GetObject("icon2.Image")));
			this.icon2.Location = new System.Drawing.Point(116, 104);
			this.icon2.Name = "icon2";
			this.icon2.Size = new System.Drawing.Size(16, 16);
			this.icon2.TabIndex = 12;
			this.icon2.TabStop = false;
			this.icon2.Visible = false;
			// 
			// color1
			// 
			this.color1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color1.Location = new System.Drawing.Point(140, 72);
			this.color1.Name = "color1";
			this.color1.Size = new System.Drawing.Size(24, 24);
			this.color1.TabIndex = 9;
			this.color1.TabStop = false;
			this.color1.Visible = false;
			this.color1.Click += new System.EventHandler(this.color_Click);
			this.color1.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color1.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon1
			// 
			this.icon1.Image = ((System.Drawing.Bitmap)(resources.GetObject("icon1.Image")));
			this.icon1.Location = new System.Drawing.Point(116, 76);
			this.icon1.Name = "icon1";
			this.icon1.Size = new System.Drawing.Size(16, 16);
			this.icon1.TabIndex = 8;
			this.icon1.TabStop = false;
			this.icon1.Visible = false;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(136, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(38, 20);
			this.label3.TabIndex = 6;
			this.label3.Text = "Color";
			// 
			// color0
			// 
			this.color0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.color0.Location = new System.Drawing.Point(140, 44);
			this.color0.Name = "color0";
			this.color0.Size = new System.Drawing.Size(24, 24);
			this.color0.TabIndex = 3;
			this.color0.TabStop = false;
			this.color0.Visible = false;
			this.color0.Click += new System.EventHandler(this.color_Click);
			this.color0.BackColorChanged += new System.EventHandler(this.color_Changed);
			this.color0.DoubleClick += new System.EventHandler(this.color_Click);
			// 
			// icon0
			// 
			this.icon0.Image = ((System.Drawing.Bitmap)(resources.GetObject("icon0.Image")));
			this.icon0.Location = new System.Drawing.Point(116, 48);
			this.icon0.Name = "icon0";
			this.icon0.Size = new System.Drawing.Size(16, 16);
			this.icon0.TabIndex = 2;
			this.icon0.TabStop = false;
			this.icon0.Visible = false;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 20);
			this.label2.TabIndex = 1;
			this.label2.Text = "Name";
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
			this.fundsGoal.Validated += new System.EventHandler(this.fundsGoal_Validated);
			// 
			// localPlayerIcon
			// 
			this.localPlayerIcon.Image = ((System.Drawing.Bitmap)(resources.GetObject("localPlayerIcon.Image")));
			this.localPlayerIcon.Location = new System.Drawing.Point(192, 272);
			this.localPlayerIcon.Name = "localPlayerIcon";
			this.localPlayerIcon.Size = new System.Drawing.Size(24, 23);
			this.localPlayerIcon.TabIndex = 69;
			this.localPlayerIcon.Text = "button3";
			this.localPlayerIcon.Visible = false;
			// 
			// botPlayerIcon
			// 
			this.botPlayerIcon.Image = ((System.Drawing.Bitmap)(resources.GetObject("botPlayerIcon.Image")));
			this.botPlayerIcon.Location = new System.Drawing.Point(216, 272);
			this.botPlayerIcon.Name = "botPlayerIcon";
			this.botPlayerIcon.Size = new System.Drawing.Size(20, 23);
			this.botPlayerIcon.TabIndex = 70;
			this.botPlayerIcon.Text = "button4";
			this.botPlayerIcon.Visible = false;
			// 
			// remotePlayerIcon
			// 
			this.remotePlayerIcon.Image = ((System.Drawing.Bitmap)(resources.GetObject("remotePlayerIcon.Image")));
			this.remotePlayerIcon.Location = new System.Drawing.Point(240, 272);
			this.remotePlayerIcon.Name = "remotePlayerIcon";
			this.remotePlayerIcon.Size = new System.Drawing.Size(24, 20);
			this.remotePlayerIcon.TabIndex = 71;
			this.remotePlayerIcon.Text = "button5";
			this.remotePlayerIcon.Visible = false;
			// 
			// NewGame
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(462, 348);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.remotePlayerIcon,
																		  this.botPlayerIcon,
																		  this.localPlayerIcon,
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
			this.Closing += new System.ComponentModel.CancelEventHandler(this.NewGame_Closing);
			this.VisibleChanged += new System.EventHandler(this.NewGame_VisibleChanged);
			this.Activated += new System.EventHandler(this.NewGame_Activated);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		private void ValidateOk()
		{
//			UpdateClients();
			button1.Enabled = false;

			// joining players cannot dismiss dialog
			if (NetworkState.Mode == NetworkMode.Joined)
				return;

			// colors must all be different
			int used = 0;
			for (int i=0; i<6; i++)
				if (colors[i].Visible)
				{
					int c = 1 << (int) colors[i].Tag;
					if ((used & c) != 0) return;
					used |= c;
				}

			// names must all be specified
			for (int i=0; i<6; i++)
				if (names[i].Enabled && names[i].Visible)
					if (names[i].Text.Trim().Length == 0)
						return;

			button1.Enabled = true;
		}

		void ExportOptions()
		{
			Options.AutomaticTrackBuilding = automaticTrack.Checked;
			Options.CityIncentives = cityIncentives.Checked;
			Options.FastStart = fastStart.Checked;
			Options.FirstToCityBonuses = this.firstToCityBonus.Checked;
			Options.FundsGoal = int.Parse(this.fundsGoal.Text);
			Options.LimitedCommodities = limitedCommodities.Checked;
			Options.GroupedContracts = groupedContracts.Checked;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			int n = 0;
			for (int i=0; i<6; i++)
				if (names[i].Visible)
					n++;

			players = new Player[n];
			for (int i=0; i<n; i++)
				players[i] = new Player((int) colors[i].Tag, !isBot[i], names[i].Text);

			for (int i=0; i<n-1; i++)
			{
				int j = i + rand.Next(n - i);
				Player temp = players[i];
				players[i] = players[j];
				players[j] = temp;
			}

			if (galleryOption.Checked)
			{
				using (MapGallery form = new MapGallery())
				{
					if (form.ShowDialog() != DialogResult.OK)
						return;

					StartGame(form.MapType, form.Seed, players);
				}
			} 
			else if (seedOption.Checked)
			{
				using (SerialNumber form = new SerialNumber())
				{
					if (form.ShowDialog() != DialogResult.OK)
						return;

					StartGame(form.MapType, form.Seed, players);
				}
			}
			else if (randomOption.Checked)
			{
				do
				{
					if (!classicStyle.Checked)
						Map = new RandomMap2(mr.Size);
					else
						Map = new RandomMap(mr.Size);
				}
				while (!Map.IsViable());
				StartGame(Map.GetType(), null, players);
			}
			else if (Map == null)
			{
				StartGame(typeof(RandomMap2), null, players);
			}
			else
				StartGame(typeof(Map), null, players);
		}

		void StartGame(Type mapType, object seed, Player[] players)
		{
			this.Hide();

			ExportOptions();

			if (seed != null)
			{
				if (Map != null)
					Map.Dispose();
				if (mapType == typeof(AuthoredMap))
					Map = new AuthoredMap((string) seed);
				else if (mapType == typeof(RandomMap2))
					Map = new RandomMap2(mr.Size, (string) seed);
				else
					Map = new RandomMap(mr.Size, (int) seed);
			}

			Startup.Current.Hide();
			GameForm game = new GameForm(this);
			game.Show();
		}

		public static void StartGame(byte[] data)
		{
			Current.StartGameInternal(data);
		}

		delegate void ByteArrayDelegate(byte[] data);

		public void StartGameInternal(byte[] data)
		{
			if (InvokeRequired)
			{
				Invoke(new ByteArrayDelegate(StartGameInternal), new object[] {data});
				return;
			}

			this.Hide();
			Startup.Current.Hide();
			GameForm game = new GameForm(data);
			game.Show();
		}

		private void randomOption_CheckedChanged(object sender, System.EventArgs e)
		{
			RandomMap = randomOption.Checked;
			SavedMap = galleryOption.Checked || seedOption.Checked;
			classicStyle.Enabled = randomOption.Checked;
		}

		public bool NewStyle
		{
			get
			{
				return !classicStyle.Checked;
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
		}

		private void NewGame_Activated(object sender, System.EventArgs e)
		{
			reuseOption.Enabled = this.Map != null;
		}

		int NextFreeSlot()
		{
			for (int i=0; i<6; i++)
				if (!names[i].Visible)
					return i;

			return -1;
		}

		int UsedColors()
		{
			int used = 0;
			for (int i=0; i<6; i++)
				if (colors[i].Visible)
					used |= 1 << (int) colors[i].Tag;
			return used;
		}

		int NextFreeColor()
		{
			int used = UsedColors();
			for (int i=0; i<6; i++)
				if ((used & (1 << i)) == 0)
					return i;

			return -1;	// should never happen
		}

		private void addPersonButton_Click(object sender, System.EventArgs e)
		{
			AddPerson();
			UpdateClients();
		}

		public void AddPerson()
		{
			int i = NextFreeSlot();
			if (i == -1) return;

			int c = NextFreeColor();
			if (c == -1) return;	// should never happen

			isBot[i] = false;

			if (NetworkState.IsHost)
			{
				names[i].DropDownStyle = ComboBoxStyle.DropDownList;
				names[i].Items.Clear();
				foreach (string user in NetworkState.CurrentUsers)
					names[i].Items.Add(user);
				names[i].SelectedIndex = 0;
			}
			else if (NetworkState.IsOffline)
			{
				names[i].DropDownStyle = ComboBoxStyle.Simple;
				names[i].Items.Clear();
				names[i].Text = "";
			}

			names[i].Enabled = true;
			names[i].Visible = true;
			names[i].Focus();

			icons[i].Image = localPlayerIcon.Image;
			icons[i].Visible = true;

			colors[i].Tag = c;
			colors[i].BackColor = Game.TrackColor[c];
			colors[i].Visible = true;
			colors[i].Enabled = true;

//			ready[i].Checked = false;
//			ready[i].Enabled = true;
//			ready[i].Visible = true;

			removes[i].Visible = true;

			UpdateEnabledState();
//			UpdateClients();
		}

		private void addBotButton_Click(object sender, System.EventArgs e)
		{
			int i = NextFreeSlot();
			if (i == -1) return;

			int c = NextFreeColor();
			if (c == -1) return;	// should never happen

			isBot[i] = true;

			names[i].DropDownStyle = ComboBoxStyle.Simple;
			names[i].Text = Game.ColorName[c] + " bot";
//			names[i].ReadOnly = true;
			names[i].Enabled = false;
			names[i].Visible = true;

			icons[i].Image = botPlayerIcon.Image;
			icons[i].Visible = true;

			colors[i].Tag = c;
			colors[i].BackColor = Game.TrackColor[c];
			colors[i].Visible = true;

//			ready[i].Checked = true;
//			ready[i].Enabled = false;
//			ready[i].Visible = true;

			removes[i].Visible = true;

			UpdateEnabledState();
			UpdateClients();
		}

		void UpdateEnabledState()
		{
			addPersonButton.Enabled = NextFreeSlot() != -1;
//			if (channel == null)
//				addBotButton.Enabled = addPersonButton.Enabled;
		}

		private void removeButton_Click(object sender, System.EventArgs e)
		{
			Button r = sender as Button;
			int i = int.Parse((string) r.Tag);
			this.SuspendLayout();
			for (int j=i; j<5; j++)
			{
				isBot[j] = isBot[j+1];
				channelIDs[j] = channelIDs[j+1];
				names[j].Text = names[j+1].Text;
//				names[j].ReadOnly = names[j+1].ReadOnly;
				names[j].Visible = names[j+1].Visible;
				names[j].DropDownStyle = names[j+1].DropDownStyle;
				names[j].Enabled = names[j+1].Enabled;
				icons[j].Image = icons[j+1].Image;
				icons[j].Visible = icons[j+1].Visible;
				colors[j].Tag = colors[j+1].Tag;
				colors[j].BackColor = colors[j+1].BackColor;
				colors[j].Visible = colors[j+1].Visible;
//				ready[j].Checked = ready[j+1].Checked;
//				ready[j].Enabled = ready[j+1].Enabled;
//				ready[j].Visible = ready[j+1].Visible;
				removes[j].Visible = removes[j+1].Visible;
			}
			names[5].Visible = false;
			icons[5].Visible = false;
			colors[5].Visible = false;
//			ready[5].Visible = false;
			removes[5].Visible = false;
			this.ResumeLayout();
			UpdateEnabledState();
			ValidateOk();
			UpdateClients();
		}

		private void color_Changed(object sender, System.EventArgs e)
		{
//			UpdateClients();
		}

		private void color_Click(object sender, EventArgs e)
		{
			PictureBox color = null;
			int i = 0;
			for (i=0; i<6; i++)
				if (colors[i] == sender)
				{
					color = (PictureBox) sender;
					break;
				}
			if (color == null)
				return;

			int c = (1 + (int) color.Tag) % 6;
			color.Tag = c;
			color.BackColor = Game.TrackColor[c];
			if (isBot[i])
				names[i].Text = Game.ColorName[c] + " bot";

//			UpdateClients();
//			if (channel != null)
//				channel.Invoke("ColorChanged", i, c);

			ValidateOk();
			UpdateClients();
		}

//		Network.Host host;
//		ClientChannel channel;
//		HostStatus hostStatus;

//		public HostStatus HostStatus
//		{
//			get { return hostStatus; }
//		}

		private void connectButton_Click(object sender, System.EventArgs e)
		{
			/*
			if (hostGame.Checked)
			{
				if (host == null)
				{
					connectButton.Text = "Stop hosting";
					localGame.Enabled = hostGame.Enabled = joinGame.Enabled = false;
					tcpAddress.Enabled = tcpPort.Enabled = false;
					hostStatus = new HostStatus();
					hostStatus.Host = host = new Network.Host(IPAddress.Parse(tcpAddress.Text), int.Parse(tcpPort.Text), typeof(HostChannel), this);
					host.Start();
					hostStatus.Show();
					this.Focus();
				}
				else
				{
					hostStatus.Dispose();
					host.Stop();
					host = null;
					localGame.Enabled = hostGame.Enabled = joinGame.Enabled = true;
					tcpAddress.Enabled = tcpPort.Enabled = true;
					connectButton.Text = "Start hosting";
				}
			}
			else if (joinGame.Checked)
			{
				if (channel == null)
				{
					channel = new ClientChannel(this);
					this.Cursor = Cursors.WaitCursor;
					if (!channel.ConnectTo(IPAddress.Parse(tcpAddress.Text), int.Parse(tcpPort.Text)))
					{
						this.Cursor = Cursors.Default;
						channel = null;
						MessageBox.Show(this, "Unable to connect to host.", Resource.GetString("Rails"),  MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}
					this.Cursor = Cursors.Default;
					connectButton.Text = "Disconnect";
					localGame.Enabled = hostGame.Enabled = joinGame.Enabled = false;
					tcpAddress.Enabled = tcpPort.Enabled = false;
					channel.OnDisconnected += new EventHandler(DisconnectedFromHost);
					channel.Start();
					channel.Invoke("Hello", GetSeatID());
					EnableControls(false, hostOwned, hostOwnedGroups);
				}
				else
				{
					channel.Stop();
					CloseConnectionToHost();
				}
			}
			*/
		}

		string GetSeatID()
		{
			string subkey = "Software\\Pluto Scarab\\Rails";
			RegistryKey key = Registry.LocalMachine.OpenSubKey(subkey, true);
			if (key == null)
				key = Registry.LocalMachine.CreateSubKey(subkey);
			string id = (string) key.GetValue("ID");
			if (id == null)
			{
				id = Guid.NewGuid().ToString();
				key.SetValue("ID", id);
			}
			key.Close();
			return id;
		}

//		void CloseConnectionToHost()
//		{
//			EnableControls(true, hostOwned, hostOwnedGroups);
//			for (int i=0; i<6; i++)
//				if (ready[i].Visible)
//					ready[i].Enabled = !isBot[i];
//			reuseOption.Enabled = this.Map != null;
//			channel = null;
//			ChannelID = 0;
//		}

		void EnableControls(bool enabled, Control[] controls)
		{
			foreach (Control control in controls)
				control.Enabled = enabled;
		}

		void EnableControls(bool enabled, params object[] controls)
		{
			foreach (object obj in controls)
				if (obj.GetType() == typeof(Control[]))
					EnableControls(enabled, (Control[]) obj);
				else if (obj.GetType() == typeof(Control[][]))
					foreach (Control[] array in (Control[][]) obj)
						EnableControls(enabled, array);
		}

//		void DisconnectedFromHost(object sender, EventArgs e)
//		{
//			disconnectedFromHost = true;
//		}

		private void NewGame_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			if (NetworkState.Mode == NetworkMode.Joined)
				return;
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

		public Player[] Players
		{
			get
			{
				return players;
			}
		}

		public void UpdateClients()
		{
			if (!NetworkState.IsHost)
				return;

			updateOnIdle = true;
		}

		private void fundsGoal_Validated(object sender, System.EventArgs e)
		{
			UpdateClients();
		}

		private void optionCheckboxes_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateClients();
		}

		public void ApplyInfo(NewGameInfo info)
		{
			if (InvokeRequired)
			{
				Invoke(new NewInfoDelegate(ApplyInfo), new object[] {info});
				return;
			}

			this.SuspendLayout();

			this.Options = info.Options;
			this.automaticTrack.Checked = info.Options.AutomaticTrackBuilding;
			this.cityIncentives.Checked = info.Options.CityIncentives;
			this.fastStart.Checked = info.Options.FastStart;
			this.firstToCityBonus.Checked = info.Options.FirstToCityBonuses;
			this.fundsGoal.Text = info.Options.FundsGoal.ToString();
			this.groupedContracts.Checked = info.Options.GroupedContracts;
			this.limitedCommodities.Checked = info.Options.LimitedCommodities;

			this.classicStyle.Checked = info.Classic;
			switch(info.MapOption)
			{
				case MapOption.Generate:
					this.randomOption.Checked = true;
					break;
				case MapOption.Reuse:
					this.reuseOption.Checked = true;
					break;
				case MapOption.Load:
					this.galleryOption.Checked = true;
					break;
				case MapOption.Specify:
					this.seedOption.Checked = true;
					break;
			}

			bool playerDeleted = info.PlayerCount < 6 && names[info.PlayerCount].Visible;
			for (int i=0; i<6; i++)
				if (i >= info.PlayerCount)
				{
					names[i].Visible = false;
					icons[i].Visible = false;
					colors[i].Visible = false;
//						ready[i].Visible = false;
					removes[i].Visible = false;
				}
				else
				{
					PlayerInfo player = info.Players[i];
					isBot[i] = player.Bot;
					channelIDs[i] = player.ChannelID;
					if (player.Bot)
					{
						names[i].DropDownStyle = ComboBoxStyle.Simple;
//						names[i].Enabled = false;
					}
					else
					{
						names[i].DropDownStyle = ComboBoxStyle.DropDownList;
						names[i].Items.Clear();
						foreach (string user in info.Users)
							names[i].Items.Add(user);
//						names[i].Enabled = true;
					}
					names[i].Text = player.Name;
					if (player.Bot)
						icons[i].Image = botPlayerIcon.Image;
					else
						icons[i].Image = localPlayerIcon.Image;
					colors[i].Tag = player.Color;
					colors[i].BackColor = Game.TrackColor[player.Color];
					colors[i].Enabled = true;
					names[i].Visible = true;
					icons[i].Visible = true;
					colors[i].Visible = true;
					removes[i].Visible = false;
				}
//			UpdateEnabledState();

			this.ResumeLayout();
		}

		public NewGameInfo GetNewGameInfo()
		{
			ExportOptions();
			NewGameInfo info = new NewGameInfo();

			info.Users = userList;
			info.Options = Options;

			info.Classic = this.classicStyle.Checked;
			if (this.randomOption.Checked)
				info.MapOption = MapOption.Generate;
			else if (this.reuseOption.Checked)
				info.MapOption = MapOption.Reuse;
			else if (this.galleryOption.Checked)
				info.MapOption = MapOption.Load;
			else 
				info.MapOption = MapOption.Specify;

			info.PlayerCount = NextFreeSlot();
			if (info.PlayerCount == -1) 
				info.PlayerCount = 6;
			info.Players = new PlayerInfo[info.PlayerCount];
			for (int i=0; i<info.PlayerCount; i++)
			{
				PlayerInfo player = new PlayerInfo();
				player.Bot = isBot[i];
				player.ChannelID = channelIDs[i];
				player.Name = names[i].Text;
				player.Color = (int) colors[i].Tag;
//				player.Ready = ready[i].Checked;
				info.Players[i] = player;
			}

			return info;
		}

		public static NewGame Current;

		delegate void NewInfoDelegate(NewGameInfo info);

		public static void Start(NewGameInfo info)
		{
			Current.Open(info);
		}

		public void Open(NewGameInfo info)
		{
			if (InvokeRequired)
			{
				Invoke(new NewInfoDelegate(Open), new object[] {info});
				return;
			}

			ApplyInfo(info);
			this.Show();
		}

		private void NewGame_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				if (NetworkState.IsHost)
				{
					EnableControls(true, hostOwned, hostOwnedGroups);
					for (int i=0; i<6; i++)
						colors[i].Visible = false;
					for (int i=0; i<6; i++)
						if (i < NetworkState.CurrentUsers.Count)
						{
							names[i].Enabled = true;
							names[i].DropDownStyle = ComboBoxStyle.DropDownList;
							names[i].Items.Clear();
							foreach (string user in NetworkState.CurrentUsers)
								names[i].Items.Add(user);
							string u = names[i].Text = NetworkState.CurrentUsers[i];
							icons[i].Image = localPlayerIcon.Image;
							int c = NextFreeColor();
							colors[i].Tag = c;
							colors[i].BackColor = Game.TrackColor[c];
							names[i].Visible = true;
							icons[i].Visible = true;
							colors[i].Visible = true;
							//							ready[i].Visible = true;
							removes[i].Visible = true;
						}
						else
						{
							names[i].Visible = false;
							icons[i].Visible = false;
							colors[i].Visible = false;
							//							ready[i].Visible = false;
							removes[i].Visible = false;
						}
					NetworkState.Transmit("OpenNewGameDialog", this.GetNewGameInfo());
				}
				else if (NetworkState.Mode == NetworkMode.Joined)
				{
					EnableControls(false, hostOwned, hostOwnedGroups);
				}
				else
				{
					EnableControls(true, hostOwned, hostOwnedGroups);
				}
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.Hide();
			if (NetworkState.IsHost)
				NetworkState.Transmit("CancelNewGame");
		}

		public static void CancelNewGame()
		{
			if (Current != null)
				Current.Invoke(new MethodInvoker(Current.Hide));
		}

		private void name0_TextChanged(object sender, System.EventArgs e)
		{
			ValidateOk();
			UpdateClients();
		}

		private void name_Enter(object sender, System.EventArgs e)
		{
			ComboBox c = sender as ComboBox;
			if (c.DropDownStyle == ComboBoxStyle.Simple)
				c.SelectAll();
		}

		public static void SetInfo(NewGameInfo info)
		{
			if (Current != null)
				Current.ApplyInfo(info);
		}

		StringCollection userList;

		void UsersUpdated(StringCollection users)
		{
			if (InvokeRequired)
			{
				Invoke(new UsersUpdatedDelegate(UsersUpdated), new object[] {users});
				return;
			}

			userList = users;
			for (int i=0; i<6; i++)
				if (names[i].Visible && names[i].DropDownStyle == ComboBoxStyle.DropDownList)
				{
					string temp = names[i].Text;
					names[i].Items.Clear();
					foreach (string user in users)
						names[i].Items.Add(user);
					names[i].Text = temp;	// keep same name, maybe change SelectedIndex
				}
			UpdateClients();
		}

		void Idle(object sender, EventArgs e)
		{
			if (!this.Visible)
				return;
			if (!updateOnIdle) 
				return;
			updateOnIdle = false;
			NewGameInfo info = GetNewGameInfo();
			NetworkState.Transmit("SetNewGameInfo", info);
		}
	}

	[Serializable]
	public struct NewGameInfo
	{
		public StringCollection Users;
		public Options Options;
		public MapOption MapOption;
		public bool Classic;
		public int PlayerCount;
		public PlayerInfo[] Players;
	}

	[Serializable]
	public struct PlayerInfo
	{
		public bool Bot;
		public int ChannelID;
		public string Name;
		public int Color;
		public bool Ready;
	}

	[Serializable]
	public enum MapOption
	{
		Generate, Reuse, Load, Specify,
	}

	/*
	public class HostChannel : Network.Channel
	{
		object sync = new Object();
		NewGame form;
		string seatID;

		public HostChannel(object state)
		{
			form = state as NewGame;
		}

		public override void Connected()
		{
			form.HostStatus.Connected(this);
		}

		public override void Disconnected()
		{
			form.HostStatus.Disconnected(this);
		}

		[Network.ProxyMethod]
		public void Hello(string seatID)
		{
			lock(sync)
			{
				System.Diagnostics.Debug.WriteLine(seatID);
				this.seatID = seatID;
				Invoke("HelloBack", form.GetNewGameInfo(), this.ID);
			}
		}

		[Network.ProxyMethod]
		public void AddPerson()
		{
			if (seatID == null) return;

			lock(sync)
			{
				form.NeedToAddPerson = this.ID;
				form.WaitForUpdate();
			}
		}

		[Network.ProxyMethod]
		public void NameChanged(int i, string name)
		{
			if (seatID == null) return;

			lock(sync)
			{
				form.NameChangedIndex = i;
				form.NameChangedText = name;
				form.NameChangedNeeded = true;
				form.WaitForUpdate();
			}
		}

		[Network.ProxyMethod]
		public void ColorChanged(int i, int c)
		{
			if (seatID == null) return;

			lock(sync)
			{
				form.ColorChangedIndex = i;
				form.ColorChangedColor = c;
				form.ColorChangedNeeded = true;
				form.WaitForUpdate();
			}
		}

		[Network.ProxyMethod]
		public void ReadyChanged(int i, bool ready)
		{
			if (seatID == null) return;

			lock(sync)
			{
				form.ReadyChangedIndex = i;
				form.ReadyChangedReady = ready;
				form.ReadyChangedNeeded = true;
				form.WaitForUpdate();
			}
		}
	}

	public class ClientChannel : Network.Channel
	{
		NewGame form;

		public ClientChannel(NewGame form)
		{
			this.form = form;
		}

		[Network.ProxyMethod]
		public void HelloBack(NewGameInfo info, int ID)
		{
			form.SetNewGameInfo(info);
			form.ChannelID = ID;
		}

		[Network.ProxyMethod]
		public void SetNewGameInfo(NewGameInfo info)
		{
			form.SetNewGameInfo(info); // doesn't touch UI
		}
	}
	*/
}