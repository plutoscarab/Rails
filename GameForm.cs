
// GameForm.cs

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
	public class GameForm : System.Windows.Forms.Form
	{
//		System.Windows.Forms.Timer idleTimer;

		#region Winforms components
		private System.Windows.Forms.MainMenu mainMenu1;
		#endregion
		private System.ComponentModel.IContainer components;

		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem menuItem14;
		private System.Windows.Forms.MenuItem discardAll;

		private System.Windows.Forms.MenuItem fileMenu;
		private System.Windows.Forms.MenuItem saveMapMenuItem;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.MenuItem editMenu;
		private System.Windows.Forms.MenuItem undoMenuItem;
		private System.Windows.Forms.MenuItem commandsMenu;
		private System.Windows.Forms.MenuItem useOtherTrackMenuItem;
		private System.Windows.Forms.MenuItem statsMenu;
		private System.Windows.Forms.MenuItem topCommoditiesMenuItem;
		private System.Windows.Forms.MenuItem playerRankingsMenuItem;
		private System.Windows.Forms.MenuItem winLossMenuItem;
		private System.Windows.Forms.MenuItem helpMenu;
		private System.Windows.Forms.MenuItem returnMenuItem;
		private System.Windows.Forms.MenuItem quitMenuItem;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem disconnectMenuItem;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem replacePlayerMenuItem;
		private System.Windows.Forms.MenuItem concedeMenuItem;
		private System.Windows.Forms.Timer syncTimer;
		private System.Windows.Forms.MenuItem aboutRails;

		private GameForm()
		{
			InitializeComponent();

			Current = this;
			Application.Idle += new EventHandler(Idle);
		}

		public static GameForm Current;

		public Game Game { get { return game; } }

		public GameForm(Game game) : this()
		{
			game.form = this;
			this.game = game;
			if (NetworkState.IsHost)
			{
				NetworkState.Transmit("StartGame", game.GetData());
			}
		}

		public GameForm(NewGame dlg) : this()
		{
			Rectangle mr = new Rectangle(0, 0, 864, 712);
			Rectangle sr = new Rectangle(mr.Right, 0, statusWidth, mr.Height);
			game = new Game(dlg.Map, sr, dlg.Players, this, dlg.Options);
			this.Invalidate();
			if (NetworkState.IsHost)
			{
				// use the game's serialization instead of serialization of the
				// NewGame dialog parameter so that we get correct random number
				// generator state for contracts
				NetworkState.Transmit("StartGame", game.GetData());
			}
		}

		public GameForm(byte[] data) : this()
		{
			MemoryStream stream = new  MemoryStream(data);
			game = new Game(new BinaryReader(stream));
			stream.Close();
			game.form = this;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GameForm));
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.fileMenu = new System.Windows.Forms.MenuItem();
			this.saveMapMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem12 = new System.Windows.Forms.MenuItem();
			this.returnMenuItem = new System.Windows.Forms.MenuItem();
			this.quitMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.disconnectMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.editMenu = new System.Windows.Forms.MenuItem();
			this.undoMenuItem = new System.Windows.Forms.MenuItem();
			this.commandsMenu = new System.Windows.Forms.MenuItem();
			this.discardAll = new System.Windows.Forms.MenuItem();
			this.useOtherTrackMenuItem = new System.Windows.Forms.MenuItem();
			this.replacePlayerMenuItem = new System.Windows.Forms.MenuItem();
			this.concedeMenuItem = new System.Windows.Forms.MenuItem();
			this.statsMenu = new System.Windows.Forms.MenuItem();
			this.topCommoditiesMenuItem = new System.Windows.Forms.MenuItem();
			this.playerRankingsMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem14 = new System.Windows.Forms.MenuItem();
			this.winLossMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenu = new System.Windows.Forms.MenuItem();
			this.aboutRails = new System.Windows.Forms.MenuItem();
			this.syncTimer = new System.Windows.Forms.Timer(this.components);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.fileMenu,
																					  this.editMenu,
																					  this.commandsMenu,
																					  this.statsMenu,
																					  this.helpMenu});
			// 
			// fileMenu
			// 
			this.fileMenu.Index = 0;
			this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.saveMapMenuItem,
																					 this.menuItem12,
																					 this.returnMenuItem,
																					 this.quitMenuItem,
																					 this.menuItem3,
																					 this.disconnectMenuItem,
																					 this.menuItem1,
																					 this.exitMenuItem});
			this.fileMenu.Text = "&File";
			this.fileMenu.Popup += new System.EventHandler(this.fileMenu_Popup);
			// 
			// saveMapMenuItem
			// 
			this.saveMapMenuItem.Index = 0;
			this.saveMapMenuItem.Text = "&Save map";
			this.saveMapMenuItem.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Index = 1;
			this.menuItem12.Text = "-";
			// 
			// returnMenuItem
			// 
			this.returnMenuItem.Index = 2;
			this.returnMenuItem.Text = "Save &game and return to menu";
			this.returnMenuItem.Click += new System.EventHandler(this.returnMenuItem_Click);
			// 
			// quitMenuItem
			// 
			this.quitMenuItem.Index = 3;
			this.quitMenuItem.Text = "&Quit game and return to menu";
			this.quitMenuItem.Click += new System.EventHandler(this.quitMenuItem_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 4;
			this.menuItem3.Text = "-";
			// 
			// disconnectMenuItem
			// 
			this.disconnectMenuItem.Index = 5;
			this.disconnectMenuItem.Text = "&Disconnect and continue playing locally";
			this.disconnectMenuItem.Click += new System.EventHandler(this.disconnectMenuItem_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 6;
			this.menuItem1.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 7;
			this.exitMenuItem.Text = "E&xit Rails";
			this.exitMenuItem.Click += new System.EventHandler(this.menuItem13_Click);
			// 
			// editMenu
			// 
			this.editMenu.Index = 1;
			this.editMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.undoMenuItem});
			this.editMenu.Text = "&Edit";
			this.editMenu.Popup += new System.EventHandler(this.menuItem7_Popup);
			// 
			// undoMenuItem
			// 
			this.undoMenuItem.Index = 0;
			this.undoMenuItem.Text = "&Undo";
			this.undoMenuItem.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// commandsMenu
			// 
			this.commandsMenu.Index = 2;
			this.commandsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.discardAll,
																						 this.useOtherTrackMenuItem,
																						 this.replacePlayerMenuItem,
																						 this.concedeMenuItem});
			this.commandsMenu.Text = "&Commands";
			this.commandsMenu.Popup += new System.EventHandler(this.menuItem16_Popup);
			// 
			// discardAll
			// 
			this.discardAll.Index = 0;
			this.discardAll.Text = "&Discard all contracts";
			this.discardAll.Click += new System.EventHandler(this.discardAll_Click);
			// 
			// useOtherTrackMenuItem
			// 
			this.useOtherTrackMenuItem.Index = 1;
			this.useOtherTrackMenuItem.Text = "&Use another player\'s track...";
			this.useOtherTrackMenuItem.Click += new System.EventHandler(this.menuItem18_Click);
			// 
			// replacePlayerMenuItem
			// 
			this.replacePlayerMenuItem.Index = 2;
			this.replacePlayerMenuItem.Text = "&Replace player...";
			this.replacePlayerMenuItem.Click += new System.EventHandler(this.replacePlayerMenuItem_Click);
			// 
			// concedeMenuItem
			// 
			this.concedeMenuItem.Index = 3;
			this.concedeMenuItem.Text = "&Concede game...";
			this.concedeMenuItem.Click += new System.EventHandler(this.concedeMenuItem_Click);
			// 
			// statsMenu
			// 
			this.statsMenu.Index = 3;
			this.statsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.topCommoditiesMenuItem,
																					  this.playerRankingsMenuItem,
																					  this.menuItem14,
																					  this.winLossMenuItem});
			this.statsMenu.Text = "&Stats";
			this.statsMenu.Popup += new System.EventHandler(this.menuItem3_Popup);
			// 
			// topCommoditiesMenuItem
			// 
			this.topCommoditiesMenuItem.Index = 0;
			this.topCommoditiesMenuItem.Text = "&Top commodities";
			this.topCommoditiesMenuItem.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// playerRankingsMenuItem
			// 
			this.playerRankingsMenuItem.Index = 1;
			this.playerRankingsMenuItem.Text = "&Player rankings";
			this.playerRankingsMenuItem.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem14
			// 
			this.menuItem14.Index = 2;
			this.menuItem14.Text = "-";
			// 
			// winLossMenuItem
			// 
			this.winLossMenuItem.Index = 3;
			this.winLossMenuItem.Text = "&Win/loss record";
			this.winLossMenuItem.Click += new System.EventHandler(this.menuItem15_Click);
			// 
			// helpMenu
			// 
			this.helpMenu.Index = 4;
			this.helpMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.aboutRails});
			this.helpMenu.Text = "&Help";
			// 
			// aboutRails
			// 
			this.aboutRails.Index = 0;
			this.aboutRails.Text = "&About Rails";
			this.aboutRails.Click += new System.EventHandler(this.menuItem10_Click);
			// 
			// syncTimer
			// 
			this.syncTimer.Enabled = true;
			this.syncTimer.Interval = 1000;
			this.syncTimer.Tick += new System.EventHandler(this.syncTimer_Tick);
			// 
			// GameForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1014, 711);
			this.Cursor = System.Windows.Forms.Cursors.Cross;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Menu = this.mainMenu1;
			this.Name = "GameForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Rails";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
			this.Load += new System.EventHandler(this.GameForm_Load);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
			this.Closed += new System.EventHandler(this.Form1_Closed);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
			this.MouseLeave += new System.EventHandler(this.GameForm_MouseLeave);
			this.Deactivate += new System.EventHandler(this.Form1_Deactivate);

		}
		#endregion

		/*
		[STAThread]
		static void xMain() 
		{
			// check for a newer version in the background
			Thread t = new Thread(new ThreadStart(CheckForNewVersion));
			t.ApartmentState = ApartmentState.STA;
			t.Start();

			GameForm form = new GameForm();
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
			
//			// otherwise start a new game
//			if (form.game == null)
//				form.StartNewGame();

			// start the message loop a pumpin'
			Application.Run(form);
		}
		*/

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
		int statusWidth = 150;			// the width of the status area to the right of the map
		Rectangle tickler = new Rectangle(0, 0, 1, 1);

		// check for end-of-game and let the computer do it's thing
		void Idle(object sender, EventArgs e)
		{
			if (game == null)
				return;

			if (!game.IsOver)
			{
				if (NetworkState.Mode != NetworkMode.Joined)
					game.ComputerMove();
				this.Invalidate(tickler);
			}

#if TEST
			if (game != null && game.IsOver)
			{
				Map map;
				if (game.map.GetType() == typeof(RandomMap))
					map = new RandomMap(game.map.ImageSize);
				else
					map = new RandomMap2(game.map.ImageSize);
				Rectangle mr = new Rectangle(0, 0, 864, 712);
				Rectangle sr = new Rectangle(mr.Right, 0, statusWidth, mr.Height);
				game = new Game(map, sr, game.state.PlayerInfo, this, game.options);
				this.Invalidate();
			}
#endif
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
			if (phantomMouseLocation.X != -1 || phantomMouseLocation.Y != -1)
				e.Graphics.DrawImageUnscaled(Images.PhantomCursor, 
					phantomMouseLocation.X + phantomMouseOffset.X,
					phantomMouseLocation.Y + phantomMouseOffset.Y);
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
			PhantomMouse(e.X, e.Y);
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
			Startup.Open();
		}

		// invoke the top commodities dialog
		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			if (game == null)
				return;
			TopCommodities form = new TopCommodities(this, game, game.map);
			form.ShowDialog();
			form.Dispose();
		}

		// invoke the player rankings dialog
		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			if (game == null) return;
			Rankings form = new Rankings(this, game, game.map);
			form.ShowDialog();
			form.Dispose();
		}

		// save the current map to the map gallery
		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			if (game == null || game.map == null)
				return;
			game.map.Save();
		}

		private void menuItem3_Popup(object sender, System.EventArgs e)
		{
			// enable the top commodities menu item
			topCommoditiesMenuItem.Enabled = game != null && game.WaitingForHuman;	
			
			// enable the player rankings menu item
			playerRankingsMenuItem.Enabled = game != null && game.WaitingForHuman;	
		}

		// save the game if they close the form
		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (NetworkState.Mode == NetworkMode.Joined)
				Startup.LeaveHost();
			else
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
			undoMenuItem.Enabled = (game != null && game.CanUndo);
			if (undoMenuItem.Enabled)
				undoMenuItem.Text = game.UndoLabel;
			else
				undoMenuItem.Text = Resource.GetString("Form1.Undo");
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
			useOtherTrackMenuItem.Enabled = game != null && game.CanUseOtherTrack();
			replacePlayerMenuItem.Enabled = NetworkState.IsMaster;
			if (NetworkState.IsOffline)
				concedeMenuItem.Enabled = game.WaitingForHuman;
			else
				concedeMenuItem.Enabled = game.WaitingForHuman && Startup.LocalUsers.Contains(game.state.ThisPlayer.Name);
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

		private void GameForm_Load(object sender, System.EventArgs e)
		{
//			// we require a fixed-size window because I haven't done the work to make the graphics scalable
//			this.Location = new Point(0, 0);
//			this.Size = new Size(1024, 768);
//			this.Location = new Point(
//				(Screen.PrimaryScreen.Bounds.Width - this.Size.Width) / 2,
//				(Screen.PrimaryScreen.Bounds.Height - this.Size.Height) / 2);

//			idleTimer = new System.Windows.Forms.Timer();
//#if TEST
//			idleTimer.Interval = 100;
//#else
//			idleTimer.Interval = 1000;
//#endif
//			idleTimer.Tick += new EventHandler(Idle);
//			idleTimer.Start();
		}

		private void returnMenuItem_Click(object sender, System.EventArgs e)
		{
			if (NetworkState.Mode == NetworkMode.Joined)
				Startup.LeaveHost();
			this.Close();
		}

		private void quitMenuItem_Click(object sender, System.EventArgs e)
		{
			if (NetworkState.Mode == NetworkMode.Joined)
				Startup.LeaveHost();
			else
			{
				if (!game.Quit())
					return;
				game.RecordResult();
			}
			File.Delete("game.sav");
			game.Dispose();
			game = null;
			this.Close();
		}

		private void GameForm_MouseLeave(object sender, System.EventArgs e)
		{
			NetworkState.Transmit("PhantomMouse", -1, -1);
		}

		DateTime phantomMouseTime = DateTime.Now;

		void PhantomMouse(int x, int y)
		{
			if (game == null)
				return;

			if (DateTime.Now < phantomMouseTime)
				return;

			if (NetworkState.IsOffline)
				return;

			if (!Startup.LocalUsers.Contains(game.state.ThisPlayer.Name))
				return;

			phantomMouseTime = DateTime.Now.AddMilliseconds(50);
			NetworkState.Transmit("PhantomMouse", x, y);
			UpdatePhantomMouse(-1, -1);
		}

		static Point phantomMouseLocation = new Point(-1, -1);
		static Point newPhantomMouse = new Point(-1, -1);
		static Point phantomMouseOffset = new Point(-2, -2);

		public static void UpdatePhantomMouse(int x, int y)
		{
			if (Current == null)
				return;

			Point p = new Point(x, y);
			if (p == phantomMouseLocation)
				return;

			newPhantomMouse = p;
			Current.Invoke(new MethodInvoker(Current.RedrawPhantomMouse));
		}

		void RedrawPhantomMouse()
		{
			InvalidatePhantomMouse();
			phantomMouseLocation = newPhantomMouse;
			InvalidatePhantomMouse();
		}

		void InvalidatePhantomMouse()
		{
			if (phantomMouseLocation.X == -1 && phantomMouseLocation.Y == -1)
				return;

			Rectangle r = new Rectangle(
				phantomMouseLocation.X + phantomMouseOffset.X, 
				phantomMouseLocation.Y + phantomMouseOffset.Y, 
				Images.PhantomCursor.Width, 
				Images.PhantomCursor.Height);
			this.Invalidate(r);
		}

		private void disconnectMenuItem_Click(object sender, System.EventArgs e)
		{
			if (NetworkState.Mode != NetworkMode.Joined)
				return;

			Startup.LeaveHost();
		}

		private void fileMenu_Popup(object sender, System.EventArgs e)
		{
			disconnectMenuItem.Enabled = NetworkState.Mode == NetworkMode.Joined;
		}

		private void replacePlayerMenuItem_Click(object sender, System.EventArgs e)
		{
			using (ReplacePlayer form = new ReplacePlayer(game.state))
			{
				if (form.ShowDialog() == DialogResult.OK)
				{
					game.state.PlayerInfo[form.PlayerToReplace].Human = form.Human;
					game.state.PlayerInfo[form.PlayerToReplace].TemporaryBot = form.TemporaryBot;
					if (form.Human)
					{
						game.state.PlayerInfo[form.PlayerToReplace].Name = form.ReplacementPlayer;
					}
					this.Invalidate();
				}
			}
		}

		private void concedeMenuItem_Click(object sender, System.EventArgs e)
		{
			using (Concede concede = new Concede(game))
			{
				if (concede.ShowDialog() == DialogResult.OK)
				{
					game.state.ThisPlayer.ConcedeTo = concede.ConcedeTo;
					if (concede.ConcedeTo == null)
						return;

					// determine who the computer players concede to, if anybody
					string bestHuman = null;
					int max = int.MinValue;
					int[] score = new int[game.state.NumPlayers];
					string bot = String.Empty;
					bool botsPlaying = false;
					for (int i=0; i<game.state.NumPlayers; i++)
						if (!game.state.PlayerInfo[i].Human)
							botsPlaying = true;
					if (botsPlaying)
					{
						for (int i=0; i<game.state.NumPlayers; i++)
						{
							Player pl = game.state.PlayerInfo[i];
							score[i] = pl.Funds - game.CityQuota(i) + game.TotalCargoValue(pl);
							if (score[i] > max)
							{
								max = score[i];
								if (pl.Human)
									bestHuman = pl.Name;
								else
								{
									bestHuman = null;
									bot = pl.Name;
								}
							}
						}
					}

					if (botsPlaying && bestHuman == null)
						MessageBox.Show(bot + " thinks it will win and does not concede the game.", this.Text);
					else
					{
						int goal = game.options.FundsGoal;
						for (int i=0; i<game.state.NumPlayers; i++)
							if (!game.state.PlayerInfo[i].Human)
							{
								game.state.PlayerInfo[i].ConcedeTo = null;
								if (max >= score[i] + 50)
									if (max >= goal || (goal - max) * 3 < (goal - score[i]) * 2)
										game.state.PlayerInfo[i].ConcedeTo = bestHuman;
								if (game.state.PlayerInfo[i].ConcedeTo == null)
								{
									MessageBox.Show(game.state.PlayerInfo[i].Name + " thinks it still has a chance and does not concede the game.", this.Text);
									return;
								}
							}

						string champ = game.state.PlayerInfo[0].ConcedeTo;
						if (champ == null) champ = game.state.PlayerInfo[0].Name;
						for (int i=1; i<game.state.NumPlayers; i++)
						{
							Player pl = game.state.PlayerInfo[i];
							if (pl.Name != champ)
								if (pl.ConcedeTo != champ)
								{
									if (pl.ConcedeTo == null)
										MessageBox.Show(pl.Name + " has not yet conceded, so the game will continue.", this.Text);
									else
										MessageBox.Show(pl.Name + " would concede to " + pl.ConcedeTo + ", but not " + champ + ", so the game will continue.", this.Text);
									return;
								}
						}
						int winner = -1;
						for (int i=0; i<game.state.NumPlayers; i++)
							if (champ == game.state.PlayerInfo[i].Name)
								winner = i;
						game.state.Winners = new ArrayList();
						game.state.Winners.Add(winner);
						game.ReportEndOfGame();
						game = null;
						this.Close();
					}
				}
			}
		}

		private void syncTimer_Tick(object sender, System.EventArgs e)
		{
			if (!NetworkState.IsHost)
				return;

			if (game == null)
				return;

			if (game.HasControl || !game.state.ThisPlayer.Human)
			{
				game.Sync();
				return;
			}

			if (NetworkState.UserChannels == null)
				return;

			Network.Channel ch = (Network.Channel) NetworkState.UserChannels[game.state.ThisPlayer.Name];
			if (ch == null) return;
			ch.Invoke("NeedSync");
		}
	}
}
