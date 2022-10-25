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
	/// Summary description for Chat.
	/// </summary>
	public class Chat : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private System.Windows.Forms.ComboBox user;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox message;
		private System.Windows.Forms.Button sayButton;
		private System.Windows.Forms.RichTextBox chatLog;
		private System.Windows.Forms.ListBox online;
		private System.Windows.Forms.Label label2;

		private Chat(StringCollection users)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (users.Count == 0)
				throw new ArgumentException();

			if (users.Count > 1)
				this.user.Items.Add(everybodyFlag);

			StringBuilder b = new StringBuilder();
			foreach (string user in users)
			{
				this.user.Items.Add(user);
				if (b.Length > 0)
					b.Append(" & ");
				b.Append(user);
			}
			everybody = b.ToString();

			this.user.SelectedIndex = 0;

			NetworkState.UsersUpdated += new UsersUpdatedEventHandler(UsersUpdated);
			UsersUpdated(NetworkState.CurrentUsers);
		}

		public static Chat Current;

		public static void Start(StringCollection users)
		{
			Stop();
			Current = new Chat(users);
			Current.Show();
		}

		public static void Stop()
		{
			if (Current != null)
			{
				Current.Dispose();
				Current = null;
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
				NetworkState.UsersUpdated -= new UsersUpdatedEventHandler(UsersUpdated);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Chat));
			this.user = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.message = new System.Windows.Forms.TextBox();
			this.sayButton = new System.Windows.Forms.Button();
			this.chatLog = new System.Windows.Forms.RichTextBox();
			this.online = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// user
			// 
			this.user.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.user.Location = new System.Drawing.Point(8, 8);
			this.user.Name = "user";
			this.user.Size = new System.Drawing.Size(120, 22);
			this.user.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(128, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "says";
			// 
			// message
			// 
			this.message.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.message.Location = new System.Drawing.Point(160, 8);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(192, 22);
			this.message.TabIndex = 2;
			this.message.Text = "";
			this.message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.message_KeyDown);
			// 
			// sayButton
			// 
			this.sayButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.sayButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sayButton.Location = new System.Drawing.Point(360, 8);
			this.sayButton.Name = "sayButton";
			this.sayButton.Size = new System.Drawing.Size(32, 23);
			this.sayButton.TabIndex = 3;
			this.sayButton.Text = "Say";
			this.sayButton.Click += new System.EventHandler(this.sayButton_Click);
			// 
			// chatLog
			// 
			this.chatLog.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.chatLog.Location = new System.Drawing.Point(8, 40);
			this.chatLog.Name = "chatLog";
			this.chatLog.ReadOnly = true;
			this.chatLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.chatLog.Size = new System.Drawing.Size(272, 176);
			this.chatLog.TabIndex = 4;
			this.chatLog.Text = "";
			// 
			// online
			// 
			this.online.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.online.ItemHeight = 14;
			this.online.Location = new System.Drawing.Point(288, 56);
			this.online.Name = "online";
			this.online.Size = new System.Drawing.Size(104, 158);
			this.online.TabIndex = 5;
			this.online.KeyDown += new System.Windows.Forms.KeyEventHandler(this.online_KeyDown);
			// 
			// label2
			// 
			this.label2.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.label2.Location = new System.Drawing.Point(288, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Who\'s online:";
			// 
			// Chat
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(400, 222);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.online,
																		  this.chatLog,
																		  this.sayButton,
																		  this.message,
																		  this.label1,
																		  this.user});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(280, 128);
			this.Name = "Chat";
			this.Text = "Chat";
			this.Activated += new System.EventHandler(this.Chat_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		private void Chat_Activated(object sender, System.EventArgs e)
		{
			message.Focus();
		}

		const string everybodyFlag = "Everybody";
		string everybody;

		string User()
		{
			string u = user.Text;
			if (u == everybodyFlag)
				u = everybody;
			return u;
		}

		StringBuilder log = new StringBuilder();

		void Log(string user, string message)
		{
			log.Append(@"\b {\cf2 ");	// bold on, green start
			log.Append(user);			// user name
			log.Append(@":} \b0 ");		// colon, green end, bold off
			log.Append(message);		// user text
			log.Append(@"\par ");		// new line

			chatLog.Rtf = 
				@"{\rtf1" +													// RTF version
				@"{\colortbl;\red0\green0\blue0;\red0\green128\blue0;}" +	// color table
				@"\cf1 " +													// select black
				log.ToString() +											// log text
				"}";

			chatLog.Focus();
			chatLog.Select(chatLog.TextLength, 0);
			chatLog.ScrollToCaret();
			this.message.Focus();
		}

		void Say()
		{
			message.Focus();
			string s = message.Text.Trim();
			if (s.Length == 0)
				return;

			message.Text = "";
			string u = User();

			if (NetworkState.IsHost)
				Log(u, s);
            
			NetworkState.Transmit("Say", u, s);
		}

		private void message_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				Say();
		}

		delegate void Handler(string string1, string string2);

		public void Heard(string user, string message)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Handler(Heard), new object[] {user, message});
				return;
			}

			Log(user, message);
			if (NetworkState.IsHost)
				NetworkState.Transmit("Say", user, message);
		}

		private void sayButton_Click(object sender, System.EventArgs e)
		{
			Say();
		}

		void UsersUpdated(StringCollection users)
		{
			if (InvokeRequired)
			{
				Invoke(new UsersUpdatedDelegate(UsersUpdated), new object[] {users});
				return;
			}

			online.Items.Clear();
			foreach (string user in users)
				online.Items.Add(user);
			this.Focus();
		}

		private void online_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
				if (NetworkState.IsHost)
					if (online.SelectedIndex >= 0 && online.SelectedIndex < online.Items.Count)
						NetworkState.KickUser((string) online.Items[online.SelectedIndex]);
		}
	}
}
