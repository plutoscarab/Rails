
// WinLoss.cs

/*
 * This is the form that displays the game results history and the
 * win/loss record for each player.
 *
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Collections.Specialized;

namespace Rails
{
	/// <summary>
	/// Summary description for WinLoss.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class WinLoss : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ListView listView2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.CheckedListBox numPlayers;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckedListBox exclude;
		private System.Windows.Forms.CheckedListBox include;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckedListBox options;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.Button button3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WinLoss()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "Bret", System.Drawing.SystemColors.WindowText, System.Drawing.SystemColors.Window, new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)))),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "2 bots"),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "75"),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "2004-07-23"),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "None"),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "987439843"),
																																								new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "Yes")}, -1);
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WinLoss));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.listView2 = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.button2 = new System.Windows.Forms.Button();
			this.options = new System.Windows.Forms.CheckedListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.include = new System.Windows.Forms.CheckedListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.exclude = new System.Windows.Forms.CheckedListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numPlayers = new System.Windows.Forms.CheckedListBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.tabPage2,
																					  this.tabPage1,
																					  this.tabPage3});
			this.tabControl1.Location = new System.Drawing.Point(16, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(600, 288);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.listView2});
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(592, 259);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Player records";
			// 
			// listView2
			// 
			this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader3,
																						this.columnHeader4,
																						this.columnHeader8,
																						this.columnHeader9,
																						this.columnHeader14,
																						this.columnHeader10,
																						this.columnHeader11});
			this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView2.GridLines = true;
			this.listView2.Name = "listView2";
			this.listView2.Size = new System.Drawing.Size(592, 259);
			this.listView2.TabIndex = 0;
			this.listView2.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Player";
			this.columnHeader3.Width = 125;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Wins";
			this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader4.Width = 42;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Losses";
			this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader8.Width = 52;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "Ties";
			this.columnHeader9.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader9.Width = 38;
			// 
			// columnHeader14
			// 
			this.columnHeader14.Text = "Unofficial";
			this.columnHeader14.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader14.Width = 70;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Win%";
			this.columnHeader10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader10.Width = 47;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Turns";
			this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader11.Width = 46;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.listView1});
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(592, 259);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Game history";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2,
																						this.columnHeader5,
																						this.columnHeader6,
																						this.columnHeader12,
																						this.columnHeader7,
																						this.columnHeader13});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.GridLines = true;
			this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																					  listViewItem1});
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(592, 259);
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Winners";
			this.columnHeader1.Width = 109;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Losers";
			this.columnHeader2.Width = 109;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Turns";
			this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader5.Width = 46;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Date";
			this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader6.Width = 84;
			// 
			// columnHeader12
			// 
			this.columnHeader12.Text = "Options";
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Map number";
			this.columnHeader7.Width = 101;
			// 
			// columnHeader13
			// 
			this.columnHeader13.Text = "Official";
			this.columnHeader13.Width = 55;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.button2,
																				   this.options,
																				   this.label4,
																				   this.include,
																				   this.label3,
																				   this.exclude,
																				   this.label2,
																				   this.label1,
																				   this.numPlayers});
			this.tabPage3.Location = new System.Drawing.Point(4, 25);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(592, 259);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Filter";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(296, 216);
			this.button2.Name = "button2";
			this.button2.TabIndex = 8;
			this.button2.Text = "Reset";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// options
			// 
			this.options.CheckOnClick = true;
			this.options.Items.AddRange(new object[] {
														 "Automatic track building",
														 "Fast start",
														 "City incentives",
														 "Limited commodities",
														 "First-to-city bonuses"});
			this.options.Location = new System.Drawing.Point(296, 104);
			this.options.Name = "options";
			this.options.Size = new System.Drawing.Size(176, 94);
			this.options.TabIndex = 7;
			this.options.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.Filter_Changed);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(296, 88);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(160, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Exclude these variations";
			// 
			// include
			// 
			this.include.CheckOnClick = true;
			this.include.Location = new System.Drawing.Point(152, 24);
			this.include.Name = "include";
			this.include.Size = new System.Drawing.Size(128, 220);
			this.include.Sorted = true;
			this.include.TabIndex = 5;
			this.include.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.Filter_Changed);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(152, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "Require these players";
			// 
			// exclude
			// 
			this.exclude.CheckOnClick = true;
			this.exclude.Location = new System.Drawing.Point(8, 24);
			this.exclude.Name = "exclude";
			this.exclude.Size = new System.Drawing.Size(128, 220);
			this.exclude.Sorted = true;
			this.exclude.TabIndex = 3;
			this.exclude.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.Filter_Changed);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Exclude these players";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(296, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(168, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Number of players must be";
			// 
			// numPlayers
			// 
			this.numPlayers.CheckOnClick = true;
			this.numPlayers.ColumnWidth = 60;
			this.numPlayers.Items.AddRange(new object[] {
															"One",
															"Two",
															"Three",
															"Four",
															"Five",
															"Six"});
			this.numPlayers.Location = new System.Drawing.Point(296, 24);
			this.numPlayers.MultiColumn = true;
			this.numPlayers.Name = "numPlayers";
			this.numPlayers.Size = new System.Drawing.Size(128, 58);
			this.numPlayers.TabIndex = 0;
			this.numPlayers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.Filter_Changed);
			// 
			// button1
			// 
			this.button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(543, 320);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(16, 320);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(144, 23);
			this.button3.TabIndex = 2;
			this.button3.Text = "Delete all records...";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// WinLoss
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.ClientSize = new System.Drawing.Size(632, 358);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.button3,
																		  this.button1,
																		  this.tabControl1});
			this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(272, 192);
			this.Name = "WinLoss";
			this.Text = "Win/Loss Records";
			this.Load += new System.EventHandler(this.WinLoss_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		class PlayerSheet
		{
			public int Wins;
			public int Losses;
			public int Ties;
			public int Unofficial;
			public int Turns;
			public int TurnGames;
		}

		GameResultsCollection gr = null;

		void ApplyFilters()
		{
			filtersChanged = false;

			listView1.Items.Clear();
			Hashtable ps = new Hashtable();
			ArrayList names = new ArrayList();
			for (int i=gr.Count-1; i>=0; i--)
			{
				GameResult res = gr[i];

				if (!numPlayers.GetItemChecked(res.NumPlayers - 1))
					continue;

				if (res.GameOptions.AutomaticTrackBuilding && options.GetItemChecked(0))
					continue;
				if (res.GameOptions.FastStart && options.GetItemChecked(1))
					continue;
				if (res.GameOptions.CityIncentives && options.GetItemChecked(2))
					continue;
				if (res.GameOptions.LimitedCommodities && options.GetItemChecked(3))
					continue;
				if (res.GameOptions.FirstToCityBonuses && options.GetItemChecked(4))
					continue;

				bool excluded = false;
				int included = 0;
				foreach (PlayerResult pr in res.Players)
				{
					string name = null;
					if (pr.Name == null)
						name = "(Bots)";
					else if (pr.Name == String.Empty)
						name = "(Unnamed)";
					else
						name = pr.Name;
					if (exclude.CheckedItems.Contains(name))
						excluded = true;
					if (include.CheckedItems.Contains(name))
						included++;
				}
				if (excluded || (included < include.CheckedItems.Count))
					continue;

				int winners = 0;
				foreach (PlayerResult pr in res.Players)
					if (pr.Won == OfficialGame.Yes)
						winners++;

				foreach (PlayerResult pr in res.Players)
				{
					string name = null;
					if (pr.Name == null)
						name = "(Bots)";
					else if (pr.Name == String.Empty)
						name = "(Unnamed)";
					else
						name = pr.Name;

					PlayerSheet sheet = (PlayerSheet) ps[name];
					if (sheet == null)
					{
						ps[name] = sheet = new PlayerSheet();
						names.Add(name);
					}
					
					if (pr.Won == OfficialGame.Unknown)
						sheet.Unofficial++;
					else if (pr.Won == OfficialGame.No)
						sheet.Losses++;
					else if (winners > 1 || res.Players.Length == 1)
						sheet.Ties++;
					else
						sheet.Wins++;
					if (res.Turns != -1)
					{
						sheet.Turns += res.Turns;
						sheet.TurnGames++;
					}
				}

				string[] items = new String[7];

				StringBuilder s = new StringBuilder();
				int bots = 0;
				foreach (PlayerResult pl in res.Players)
					if (pl.Won == OfficialGame.Yes)
					{
						if (pl.Name == null)
							bots++;
						else
						{
							if (s.Length > 0)
								s.Append(", ");
							s.Append(pl.Name);
						}
					}
				if (bots > 0)
				{
					if (s.Length > 0)
						s.Append(", ");
					s.Append(bots);
					s.Append(" bot");
					if (bots > 1)
						s.Append("s");
				}
				items[0] = s.ToString();

				s = new StringBuilder();
				bots = 0;
				foreach (PlayerResult pl in res.Players)
					if (pl.Won != OfficialGame.Yes)
					{
						if (pl.Name == null)
							bots++;
						else
						{
							if (s.Length > 0)
								s.Append(", ");
							s.Append(pl.Name);
						}
					}
				if (bots > 0)
				{
					if (s.Length > 0)
						s.Append(", ");
					s.Append(bots);
					s.Append(" bot");
					if (bots > 1)
						s.Append("s");
				}
				items[1] = s.ToString();

				items[2] = res.Turns == -1 ? "?" : res.Turns.ToString();
				items[3] = res.Date == DateTime.MinValue ? "?" : res.Date.ToString("yyyy-MM-dd");
				items[4] = res.GameOptions.ToString(true);
				items[5] = res.Map.ToString();
				items[6] = res.Official == OfficialGame.No ? "No" : "Yes";

				listView1.Items.Add(new ListViewItem(items));
			}

			bool firstTime = exclude.Items.Count == 0;

			listView2.Items.Clear();
			string[] snames = (string[]) names.ToArray(typeof(string));
			Array.Sort(snames);
			foreach (string name in snames)
			{
				if (firstTime)
				{
					exclude.Items.Add(name);
					include.Items.Add(name);
				}

				string[] items = new string[7];
				items[0] = name;
				PlayerSheet sheet = (PlayerSheet) ps[name];
				items[1] = sheet.Wins.ToString();
				items[2] = sheet.Losses.ToString();
				items[3] = sheet.Ties.ToString();
				items[4] = sheet.Unofficial.ToString();
				if (sheet.Wins + sheet.Losses > 0)
					items[5] = ((int)(100 * sheet.Wins / (sheet.Wins + sheet.Losses))).ToString() + "%";
				else
					items[5] = "?";
				if (sheet.TurnGames > 0)
					items[6] = ((int)(sheet.Turns / sheet.TurnGames)).ToString();
				else
					items[6] = "?";
				listView2.Items.Add(new ListViewItem(items));
			}
		}

		private void WinLoss_Load(object sender, System.EventArgs e)
		{
			gr = new GameResultsCollection();
			ResetFilters();
			ApplyFilters();
		}

		private void listView2_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		bool filtersChanged = false;

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl1.SelectedIndex != 2 && filtersChanged)
				ApplyFilters();
		}

		private void Filter_Changed(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			filtersChanged = true;
		}

		void ResetFilters()
		{
			for (int i=0; i<exclude.Items.Count; i++)
			{
				exclude.SetItemChecked(i, false);
				include.SetItemChecked(i, false);
			}

			for (int i=0; i<6; i++)
				numPlayers.SetItemChecked(i, true);

			for (int i=0; i<5; i++)
				options.SetItemChecked(i, false);
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			ResetFilters();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			string warning = Resource.GetString("WinLoss.Warning");
			if (MessageBox.Show(this, warning, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
				return;
			try
			{
				string fn = "winloss.rec";
				string bak = "winloss" + DateTime.Now.Ticks.ToString() + ".rec.bak";
				System.IO.File.Move(fn, bak);
				this.Close();
			}
			catch(System.IO.IOException)
			{
				MessageBox.Show(this, Resource.GetString("WinLoss.DeleteError"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
