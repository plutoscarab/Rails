using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Collections.Specialized;
using System.Text;

namespace Rails
{
	/// <summary>
	/// Summary description for Startup.
	/// </summary>
	public class Startup : System.Windows.Forms.Form
	{
		const string resumeGameFilename = "game.sav";

		private Rails.GelButton resumeGameButton;
		private Rails.GelButton loadGameButton;
		private Rails.GelButton exitButton;
		private Rails.GelButton viewWinLossButton;
		private Rails.GelButton hostButton;
		private Rails.GelButton joinButton;
//		private System.Windows.Forms.Button startGameButton2;
		private Rails.GelButton startGameButton;
		private Rails.GelButton webSiteButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Startup()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Current = this;
			NetworkState.LostHost += new EventHandler(LostHost);
			NetworkState.GainedClient += new GainedClientEventHandler(GainedClient);
			
			// Force the creation of the window handle so that the Invoke()
			// call works correctly the first time we try to call Show() on
			// the NewGame dialog if it's invoked from a network callback.
			IntPtr h = newGameDialog.Handle;
		}

		NewGame newGameDialog = new NewGame();
		public static Startup Current;
		public static StringCollection LocalUsers;

		public static void Open()
		{
			Current.Show();
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

				NetworkState.LostHost -= new EventHandler(LostHost);
				NetworkState.GainedClient -= new GainedClientEventHandler(GainedClient);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Startup));
			this.resumeGameButton = new Rails.GelButton();
			this.loadGameButton = new Rails.GelButton();
			this.exitButton = new Rails.GelButton();
			this.viewWinLossButton = new Rails.GelButton();
			this.webSiteButton = new Rails.GelButton();
			this.hostButton = new Rails.GelButton();
			this.joinButton = new Rails.GelButton();
			this.startGameButton = new Rails.GelButton();
			this.SuspendLayout();
			// 
			// resumeGameButton
			// 
			this.resumeGameButton.ButtonColor = Rails.GelButton.ButtonColors.Blue;
			this.resumeGameButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.resumeGameButton.ForeColor = System.Drawing.Color.White;
			this.resumeGameButton.Label = "Resume game";
			this.resumeGameButton.Location = new System.Drawing.Point(40, 80);
			this.resumeGameButton.Name = "resumeGameButton";
			this.resumeGameButton.Size = new System.Drawing.Size(120, 42);
			this.resumeGameButton.TabIndex = 1;
			this.resumeGameButton.Click += new System.EventHandler(this.resumeGameButton_Click);
			// 
			// loadGameButton
			// 
			this.loadGameButton.ButtonColor = Rails.GelButton.ButtonColors.Blue;
			this.loadGameButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.loadGameButton.ForeColor = System.Drawing.Color.White;
			this.loadGameButton.Label = "Load game";
			this.loadGameButton.Location = new System.Drawing.Point(40, 120);
			this.loadGameButton.Name = "loadGameButton";
			this.loadGameButton.Size = new System.Drawing.Size(120, 42);
			this.loadGameButton.TabIndex = 2;
			// 
			// exitButton
			// 
			this.exitButton.ButtonColor = Rails.GelButton.ButtonColors.Red;
			this.exitButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.exitButton.ForeColor = System.Drawing.Color.White;
			this.exitButton.Label = "Exit";
			this.exitButton.Location = new System.Drawing.Point(40, 184);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(120, 42);
			this.exitButton.TabIndex = 5;
			this.exitButton.Click += new System.EventHandler(this.button6_Click);
			// 
			// viewWinLossButton
			// 
			this.viewWinLossButton.ButtonColor = Rails.GelButton.ButtonColors.Yellow;
			this.viewWinLossButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.viewWinLossButton.ForeColor = System.Drawing.Color.Black;
			this.viewWinLossButton.Label = "Win/loss record";
			this.viewWinLossButton.Location = new System.Drawing.Point(192, 144);
			this.viewWinLossButton.Name = "viewWinLossButton";
			this.viewWinLossButton.Size = new System.Drawing.Size(120, 42);
			this.viewWinLossButton.TabIndex = 3;
			this.viewWinLossButton.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// webSiteButton
			// 
			this.webSiteButton.ButtonColor = Rails.GelButton.ButtonColors.Yellow;
			this.webSiteButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.webSiteButton.ForeColor = System.Drawing.Color.Black;
			this.webSiteButton.Label = "Web site";
			this.webSiteButton.Location = new System.Drawing.Point(192, 184);
			this.webSiteButton.Name = "webSiteButton";
			this.webSiteButton.Size = new System.Drawing.Size(120, 42);
			this.webSiteButton.TabIndex = 4;
			this.webSiteButton.Click += new System.EventHandler(this.button1_Click);
			// 
			// hostButton
			// 
			this.hostButton.ButtonColor = Rails.GelButton.ButtonColors.Green;
			this.hostButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.hostButton.ForeColor = System.Drawing.Color.White;
			this.hostButton.Label = "Start hosting";
			this.hostButton.Location = new System.Drawing.Point(192, 40);
			this.hostButton.Name = "hostButton";
			this.hostButton.Size = new System.Drawing.Size(120, 42);
			this.hostButton.TabIndex = 6;
			this.hostButton.Click += new System.EventHandler(this.hostButton_Click);
			// 
			// joinButton
			// 
			this.joinButton.ButtonColor = Rails.GelButton.ButtonColors.Green;
			this.joinButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.joinButton.ForeColor = System.Drawing.Color.White;
			this.joinButton.Label = "Connect to host";
			this.joinButton.Location = new System.Drawing.Point(192, 80);
			this.joinButton.Name = "joinButton";
			this.joinButton.Size = new System.Drawing.Size(120, 42);
			this.joinButton.TabIndex = 7;
			this.joinButton.Click += new System.EventHandler(this.joinButton_Click);
			// 
			// startGameButton
			// 
			this.startGameButton.ButtonColor = Rails.GelButton.ButtonColors.Blue;
			this.startGameButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.startGameButton.ForeColor = System.Drawing.Color.White;
			this.startGameButton.Label = "Start new game";
			this.startGameButton.Location = new System.Drawing.Point(40, 40);
			this.startGameButton.Name = "startGameButton";
			this.startGameButton.Size = new System.Drawing.Size(120, 42);
			this.startGameButton.TabIndex = 8;
			this.startGameButton.Click += new System.EventHandler(this.startGameButton_Click);
			// 
			// Startup
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.BackgroundImage = ((System.Drawing.Bitmap)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = new System.Drawing.Size(354, 272);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.startGameButton,
																		  this.joinButton,
																		  this.hostButton,
																		  this.webSiteButton,
																		  this.viewWinLossButton,
																		  this.exitButton,
																		  this.loadGameButton,
																		  this.resumeGameButton});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Startup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Rails";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Startup_Closing);
			this.Activated += new System.EventHandler(this.Startup_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main()
		{
			
			using (Startup form = new Startup())
			{
				Application.Run(form);
			}
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		bool IsGameInProgress
		{
			get
			{
				return File.Exists(resumeGameFilename);
			}
		}

		void DeleteGameInProgress()
		{
			try
			{
				File.Delete(resumeGameFilename);
			}
			catch(IOException)
			{
			}
		}

		bool OkToAbandon
		{
			get
			{
				return MessageBox.Show(this,
					Resource.GetString("Startup.Abandon"),
					Resource.GetString("Rails"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question)
									== DialogResult.Yes;
			}
		}

		Game GetGameInProgress()
		{
			this.Cursor = Cursors.WaitCursor;
			Stream s = new FileStream(resumeGameFilename, FileMode.Open);
			BinaryReader r = new BinaryReader(s);
			try
			{
				return new Game(r);
			}
			catch(Exception e)
			{
				return null;
			}
			finally
			{
				r.Close();
				s.Close();
				this.Cursor = Cursors.Default;
			}
		}

		bool OkToStartNewGame
		{
			get
			{
				if (!IsGameInProgress)
					return true;

				if (!OkToAbandon)
					return false;

				this.Refresh();

				using (Game game = GetGameInProgress())
				{
					if (game == null)
						return false;

					if (!game.Quit())
						return false;

					game.RecordResult();
				}

				DeleteGameInProgress();
				return true;
			}
		}

		private void startGameButton_Click(object sender, System.EventArgs e)
		{
			if (newGameDialog.Visible)
			{
				newGameDialog.Focus();
				return;
			}

			if (!OkToStartNewGame) return;

//			this.Hide();
			newGameDialog.Show();
		}

		private void Startup_Activated(object sender, System.EventArgs e)
		{
			resumeGameButton.Enabled = IsGameInProgress;
		}

		private void button1_Click_1(object sender, System.EventArgs e)
		{
			using (WinLoss form = new WinLoss())
			{
				form.ShowDialog();
			}
		}

		private void resumeGameButton_Click(object sender, System.EventArgs e)
		{
			if (!IsGameInProgress)
				return;

			Game game = GetGameInProgress();
			if (game == null)
				return;

//			if (NetworkState.IsHost)
//			{
//				foreach (Player player in game.state.PlayerInfo)
//					if (player.Human && !NetworkState.CurrentUsers.Contains(player.Name))
//					{
//						game = null;
//						MessageBox.Show("Online player names do not match those in the current game. This will be supported in a future version.");
//						return;
//					}
//			}

			GameForm form = new GameForm(game);
			this.Hide();
			form.Show();

		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("http://bretm.home.comcast.net/rails.html");
		}

		private void Startup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (newGameDialog != null)
			{
				newGameDialog.Dispose();
				newGameDialog = null;
			}

			NetworkState.Stop();
		}

		private void hostButton_Click(object sender, System.EventArgs e)
		{
			if (NetworkState.Mode == NetworkMode.Hosting)
			{
				NetworkState.StopHosting();
				hostButton.Text = "Start hosting";
				joinButton.Enabled = true;
				Chat.Stop();
				return;
			}

			using (HostForm form = new HostForm(true))
			{
				while (form.ShowDialog() == DialogResult.OK)
				{
					StringCollection users = form.Users;
					int count = users.Count;
					if (count == 0)
						return;
					if (count > Game.MaxPlayers)
					{
						MessageBox.Show(this, "The game only supports a maximum of " + Game.MaxPlayers.ToString() + " players.", Resource.GetString("Rails"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						continue;
					}
					if (NetworkState.StartHosting(form.Address, form.Port))
					{
						LocalUsers = users;
						hostButton.Text = "Stop Hosting";
						joinButton.Enabled = false;
						NetworkState.RegisterUsers(users, null);
						Chat.Start(users);
					}
					return;
				}
			}
		}

		private void joinButton_Click(object sender, System.EventArgs e)
		{
			if (NetworkState.Mode == NetworkMode.Joined)
			{
				LeaveHost();
				return;
			}

			using (HostForm form = new HostForm(false))
			{
				while (form.ShowDialog() == DialogResult.OK)
				{
					StringCollection users = form.Users;
					int count = users.Count;
					if (count == 0)
						return;
					if (count > Game.MaxPlayers - 1)
					{
						MessageBox.Show(this, "The game only supports a maximum of " + (Game.MaxPlayers-1).ToString() + " remote players.", Resource.GetString("Rails"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						continue;
					}
					this.Cursor = Cursors.WaitCursor;
					if (NetworkState.JoinHost(form.Address, form.Port))
					{
						this.Cursor = Cursors.Default;
						LocalUsers = users;
						joinButton.Text = "Disconnect";
						hostButton.Enabled = false;
						startGameButton.Enabled = false;
						loadGameButton.Enabled = false;
						NetworkState.Client.OnUserAcceptance += new UserAcceptanceEventHandler(UserAcceptance);
						NetworkState.Transmit("Hello", users);
					}
					else
					{
						this.Cursor = Cursors.Default;
						MessageBox.Show(this, "Unable to connect to host.", Resource.GetString("Rails"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						continue;
					}
					return;
				}
			}
		}

		void LostHost(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new EventHandler(LostHost));
				return;
			}

			Chat.Stop();
			joinButton.Text = "Connect to host";
			hostButton.Enabled = true;
			startGameButton.Enabled = true;
			loadGameButton.Enabled = true;
		}

		public static void LeaveHost()
		{
			NetworkState.Client.Invoke("Bye");
			NetworkState.LeaveHost();
		}

		delegate void UserAcceptanceDelegate(bool b, StringCollection c);

		void UserAcceptance(bool accepted, StringCollection users)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new UserAcceptanceDelegate(UserAcceptance), new object[] {accepted, users});
				return;
			}

			if (accepted)
			{
				Chat.Start(users);
				return;
			}

			LeaveHost();
			StringBuilder b = new StringBuilder();
			b.Append("One or more user names conflicts with users already online. Please use nicknames for the following user(s): ");
			int n = b.Length;
			foreach (string user in users)
			{
				if (b.Length > n)
					b.Append(", ");
				b.Append(user);
			}
			
			MessageBox.Show(this, b.ToString(), Resource.GetString("Rails"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		void GainedClient(HostChannel channel)
		{
			if (NewGame.Current != null && NewGame.Current.Visible)
				channel.Invoke("OpenNewGameDialog", NewGame.Current.GetNewGameInfo());
			else if (GameForm.Current != null && GameForm.Current.Visible)
				channel.Invoke("StartGame", GameForm.Current.Game.GetData());
		}
	}
}
