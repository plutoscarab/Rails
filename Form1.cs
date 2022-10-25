
// Form1.cs

/*
 * This is the main game form.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Net;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Rails
{
	[ComVisible(false)]
	public class Form1 : System.Windows.Forms.Form
	{
		#region Winforms components
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.ComponentModel.Container components = null;
		#endregion

		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.MenuItem menuItem14;
		private System.Windows.Forms.MenuItem menuItem15;
		private System.Windows.Forms.MenuItem menuItem16;
		private System.Windows.Forms.MenuItem menuItem18;
		private System.Windows.Forms.MenuItem discardAll;

		System.Windows.Forms.Timer idleTimer;

		public Form1()
		{
			InitializeComponent();

			// we require a fixed-size window because I haven't done the work to make the graphics scalable
			this.Size = new Size(1024, 768 - 20);
			this.StartPosition = FormStartPosition.CenterScreen;

			idleTimer = new System.Windows.Forms.Timer();
#if TEST
			idleTimer.Interval = 100;
#else
			idleTimer.Interval = 1000;
#endif
			idleTimer.Tick += new EventHandler(Idle);
			idleTimer.Start();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem12 = new System.Windows.Forms.MenuItem();
			this.menuItem13 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem16 = new System.Windows.Forms.MenuItem();
			this.discardAll = new System.Windows.Forms.MenuItem();
			this.menuItem18 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem14 = new System.Windows.Forms.MenuItem();
			this.menuItem15 = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem7,
																					  this.menuItem16,
																					  this.menuItem3,
																					  this.menuItem9});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem2,
																					  this.menuItem6,
																					  this.menuItem12,
																					  this.menuItem13});
			this.menuItem1.Text = "&File";
			this.menuItem1.Popup += new System.EventHandler(this.menuItem1_Popup);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "&New game";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 1;
			this.menuItem6.Text = "&Save map";
			this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Index = 2;
			this.menuItem12.Text = "-";
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 3;
			this.menuItem13.Text = "E&xit";
			this.menuItem13.Click += new System.EventHandler(this.menuItem13_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem8});
			this.menuItem7.Text = "&Edit";
			this.menuItem7.Popup += new System.EventHandler(this.menuItem7_Popup);
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 0;
			this.menuItem8.Text = "&Undo";
			this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// menuItem16
			// 
			this.menuItem16.Index = 2;
			this.menuItem16.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.discardAll,
																					   this.menuItem18});
			this.menuItem16.Text = "&Commands";
			this.menuItem16.Popup += new System.EventHandler(this.menuItem16_Popup);
			// 
			// discardAll
			// 
			this.discardAll.Index = 0;
			this.discardAll.Text = "&Discard all contracts";
			this.discardAll.Click += new System.EventHandler(this.discardAll_Click);
			// 
			// menuItem18
			// 
			this.menuItem18.Index = 1;
			this.menuItem18.Text = "&Use another player\'s track";
			this.menuItem18.Click += new System.EventHandler(this.menuItem18_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 3;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem4,
																					  this.menuItem5,
																					  this.menuItem14,
																					  this.menuItem15});
			this.menuItem3.Text = "&Stats";
			this.menuItem3.Popup += new System.EventHandler(this.menuItem3_Popup);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.Text = "&Top commodities";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 1;
			this.menuItem5.Text = "&Player rankings";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem14
			// 
			this.menuItem14.Index = 2;
			this.menuItem14.Text = "-";
			// 
			// menuItem15
			// 
			this.menuItem15.Index = 3;
			this.menuItem15.Text = "&Win/loss record";
			this.menuItem15.Click += new System.EventHandler(this.menuItem15_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 4;
			this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem10});
			this.menuItem9.Text = "&Help";
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 0;
			this.menuItem10.Text = "&About Rails";
			this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(300, 160);
			this.Cursor = System.Windows.Forms.Cursors.Cross;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Rails";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
			this.Closed += new System.EventHandler(this.Form1_Closed);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
			this.Deactivate += new System.EventHandler(this.Form1_Deactivate);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			// check for a newer version in the background
			Thread t = new Thread(new ThreadStart(CheckForNewVersion));
			t.ApartmentState = ApartmentState.STA;
			t.Start();

			Form1 form = new Form1();
			form.Show();

			// if we were in the middle of a game, restore the game state
			if (File.Exists("game.sav"))
			{
				try
				{
					Stream s = new FileStream("game.sav", FileMode.Open);
					BinaryReader r = new BinaryReader(s);
					try
					{
						form.game = new Game(r, form);
						form.map = form.game.map;
					}
					catch
					{
						if (form.game != null) form.game.Dispose();
						form.game = null;
					}
					r.Close();
					s.Close();
				}
				catch
				{
					if (form.game != null) form.game.Dispose();
					form.game = null;
				}
			}
			
			// otherwise start a new game
			if (form.game == null)
				form.StartNewGame();

			// start the message loop a pumpin'
			Application.Run(form);
		}

		// Check online to see if a newer version is available. I'll get rid of
		// this once the game is no longer under development (maybe).
		static void CheckForNewVersion()
		{
			Stream webStream = null;
			StreamReader reader = null;
			RegistryKey key = null;
			try
			{
				Version thisVersion = Assembly.GetExecutingAssembly().GetName().Version;

				string subkey = "Software\\Pluto Scarab\\Rails";
				key = Registry.LocalMachine.OpenSubKey(subkey, true);
				if (key == null)
					key = Registry.LocalMachine.CreateSubKey(subkey);

				string ver = (string) key.GetValue("Version", "1.0.0.0");
				Version lastVersion = new Version(ver);

				// is this the first time we've run this new version?
				if (thisVersion > lastVersion)
				{
					key.SetValue("Version", thisVersion.ToString());
					key.SetValue("VersionCheck", 1);
					return;
				}

				// did we already bug the user about checking for a newer version than this?
				int check = (int) key.GetValue("VersionCheck", 1);
				if (check == 0)
					return;

				// download the versions file to read the version available online
				Version version = null;
				using (WebClient web = new WebClient())
				{								 
					string address = "http://bretm.home.comcast.net/versions.txt";
					webStream = web.OpenRead(address);
					reader = new StreamReader(web.OpenRead(address));
					string line;
					char[] tab = {'\t'};
					while ((line = reader.ReadLine()) != null)
					{
						string[] appVersion = line.Split(tab);
						if (appVersion.Length == 2)
							if (appVersion[0] == "Rails")
							{
								version = new Version(appVersion[1]);
								break;
							}
					}
				}

				if (version == null)
					return;

				// tell them about it, but only once
				if (version > thisVersion)
				{
					key.SetValue("VersionCheck", 0);
					string message = Resource.GetString("Form1.NewVersionAvailable");
					if (DialogResult.Yes == MessageBox.Show(message, Resource.GetString("Rails"), MessageBoxButtons.YesNo))
					{
						System.Diagnostics.Process.Start("http://bretm.home.comcast.net/rails.html");
					}
				}
				else
				{
					key.SetValue("VersionCheck", 1);
				}
			}
			catch(IOException)
			{
			}
			catch(WebException)
			{
			}
			catch(System.Security.SecurityException)
			{
			}
			finally
			{
				if (reader != null)
					reader.Close();
				if (webStream != null)
					webStream.Close();
				if (key != null)
					key.Close();
			}
		}

		Game game;						// the game engine
		Map map;						// the map
		NewGame dlg = new NewGame();	// the new game dialog
		int statusWidth = 150;			// the width of the status area to the right of the map

		// let them choose the number of players, the map, etc., and start a new game
		public bool StartNewGame()
		{
			if (game != null)
				game.Pause(true);
			DialogResult result = dlg.ShowDialog();
			if (game != null)
				game.Pause(false);
			this.Refresh();
			if (result == DialogResult.Cancel)
				return false;
			if (dlg.Players == null)
				return false;

			StartGame();
			return true;
		}

		// generate the appropriate map and start the game
		void StartGame()
		{
			if (this.WindowState == FormWindowState.Minimized)
				return;

			Rectangle cr = this.ClientRectangle;
			Rectangle mr = new Rectangle(0, 0, 864, 712);
//			Rectangle mr = cr;
//			mr.Width -= statusWidth;
			Rectangle sr = cr;
			sr.X = mr.Right;
			sr.Width = statusWidth;

			if (dlg.RandomMap)
			{
				do
				{
					if (map != null) map.Dispose();
					if (dlg.NewStyle)
						map = new RandomMap2(mr.Size);
					else
						map = new RandomMap(mr.Size);
				}
				while (!map.IsViable());
			}
			else if (dlg.SavedMap)
			{
				if (map != null) map.Dispose();
				if (dlg.MapType == typeof(AuthoredMap))
					map = new AuthoredMap((string) dlg.Seed);
				else if (dlg.MapType == typeof(RandomMap))
					map = new RandomMap(mr.Size, (int) dlg.Seed);
				else
					map = new RandomMap2(mr.Size, (string) dlg.Seed);
			}
			else if (map == null)
			{
				// reuse current map, but there is no current map
				do
				{
					if (map != null) map.Dispose();
					map = new RandomMap2(mr.Size);
				}
				while (!map.IsViable());
			}

			if (game != null)
//				if (game.IsOfficial)
					game.RecordResult();

			if (game != null)
				game.Pause(true);
			game = new Game(map, sr, dlg.Players, this, dlg.Options);
			this.Invalidate();
		}

		// check for end-of-game and let the computer do it's thing
		void Idle(object sender, EventArgs e)
		{
			if (game == null) 
				return;
			idleTimer.Stop();
			if (!game.IsOver)
				game.ComputerMove();
#if TEST
			if (game != null && game.IsOver)
			{
				StartGame();
				this.Invalidate();
			}
#endif
			idleTimer.Start();
		}

		// File/New Game
		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			if (game != null)
				if (!game.Quit())
					return;

			if (StartNewGame())
			{
				try
				{
					File.Delete("game.sav");
				}
				catch(System.IO.IOException)
				{
				}
				this.Refresh();
			}
		}

		// don't draw the background color because that would cause terrible flickering
		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		// draw the game graphics
		private void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if (game == null)
			{
				e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);
				return;
			}

			game.Paint(e);
		}

		// display either the arrow or the cross cursor depending on which part of the form we're on
		private void Form1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.X > this.ClientRectangle.Width - statusWidth)
				this.Cursor = Cursors.Arrow;
			else
				this.Cursor = Cursors.Cross;
			if (game == null) return;
			game.MouseMove(e);
		}

		// transmit mouse clicks through to the game engine
		private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (game == null) return;
			game.MouseDown(e);
		}

		private void Form1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (game == null) return;
			game.MouseUp(e);
		}

		private void Form1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
		}

		private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				if (game != null)
					game.RightClick();
		}

		// What's all this refresh timer stuff about? When the player moves the mouse over a contract,
		// we want to show them all the sources and the destination. That requires redrawing the map,
		// which can take a moment, so we don't want to do it if they accidentally moused over the item
		// or if they mouse is straying across multiple contracts. So we wait until they PAUSE the
		// mouse over an item for 400 milliseconds before we redraw the map. This provides a much 
		// nicer experience.

		System.Threading.Timer timer;

		public void InitRefreshTimer()
		{
			CancelRefreshTimer();
			if (timer == null)
				timer = new System.Threading.Timer(new System.Threading.TimerCallback(RefreshTimer), null, 400, System.Threading.Timeout.Infinite);
			else
				timer.Change(400, System.Threading.Timeout.Infinite);
		}

		public void CancelRefreshTimer()
		{
			if (timer == null)
				return;
			timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
		}

		public void RefreshTimer(object state)
		{
			CancelRefreshTimer();
			this.Refresh();
		}

		private void Form1_Closed(object sender, System.EventArgs e)
		{
			if (game != null)
			{
				game.Dispose();
				game = null;
			}
		}

		// invoke the top commodities dialog
		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			if (game == null)
				return;
			TopCommodities form = new TopCommodities(this, game, map);
			form.ShowDialog();
			form.Dispose();
		}

		// invoke the player rankings dialog
		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			if (game == null) return;
			Rankings form = new Rankings(this, game, map);
			form.ShowDialog();
			form.Dispose();
		}

		// save the current map to the map gallery
		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			if (map == null)
				return;
			map.Save();
		}

		// determine if the map save option should be enabled
		private void menuItem1_Popup(object sender, System.EventArgs e)
		{
			menuItem6.Enabled = (map != null);
		}

		private void menuItem3_Popup(object sender, System.EventArgs e)
		{
			// enable the top commodities menu item
			menuItem4.Enabled = (map != null) && (game == null || game.WaitingForHuman);	
			
			// enable the player rankings menu item
			menuItem5.Enabled = (game != null) && game.WaitingForHuman;	
		}

		// save the game if they close the form
		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (game != null)
				if (!game.IsOver)
					game.Save();
		}

		// Edit->Undo
		private void menuItem8_Click(object sender, System.EventArgs e)
		{
			if (game == null)
				return;
			game.Undo();
		}

		// determine appearance of Undo menu item
		private void menuItem7_Popup(object sender, System.EventArgs e)
		{
			menuItem8.Enabled = (game != null && game.CanUndo);
			if (menuItem8.Enabled)
				menuItem8.Text = game.UndoLabel;
			else
				menuItem8.Text = Resource.GetString("Form1.Undo");
		}

		// Help->About
		private void menuItem10_Click(object sender, System.EventArgs e)
		{
			About form = new About();
			form.ShowDialog();
			form.Dispose();
		}

		private void discardAll_Click(object sender, System.EventArgs e)
		{
			if (game != null)
				game.DiscardAll();
		}

		private void menuItem13_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void menuItem15_Click(object sender, System.EventArgs e)
		{
			WinLoss form = new WinLoss();
			form.ShowDialog();
			form.Dispose();
		}

		private void menuItem16_Popup(object sender, System.EventArgs e)
		{
			discardAll.Enabled = game != null && game.CanDiscardAll(false);
			menuItem18.Enabled = game != null && game.CanUseOtherTrack();
		}

		private void menuItem18_Click(object sender, System.EventArgs e)
		{
			if (game != null)
				game.UseOtherTrack();
		}

		private void Form1_Deactivate(object sender, System.EventArgs e)
		{
			if (game != null)
				game.Deactivate();
		}
	}
}
