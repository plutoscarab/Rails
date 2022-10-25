using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for ReplacePlayer.
	/// </summary>
	public class ReplacePlayer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox playerToReplace;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton botForOne;
		private System.Windows.Forms.RadioButton botForever;
		private System.Windows.Forms.RadioButton person;
		private System.Windows.Forms.ComboBox replacementPlayer;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ReplacePlayer(GameState state)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			for (int i=0; i<state.NumPlayers; i++)
			{
				Player pl = state.PlayerInfo[i];
				if (pl.Human)
				{
					playerToReplace.Items.Add(pl.Name + " (" + Game.ColorName[pl.TrackColor] + ")");
				}
				else
				{
					playerToReplace.Items.Add(pl.Name);
				}
			}
			
			foreach (string user in NetworkState.CurrentUsers)
			{
				replacementPlayer.Items.Add(user);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ReplacePlayer));
			this.label1 = new System.Windows.Forms.Label();
			this.playerToReplace = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.botForOne = new System.Windows.Forms.RadioButton();
			this.botForever = new System.Windows.Forms.RadioButton();
			this.person = new System.Windows.Forms.RadioButton();
			this.replacementPlayer = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Replace this player:";
			// 
			// playerToReplace
			// 
			this.playerToReplace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.playerToReplace.Location = new System.Drawing.Point(128, 16);
			this.playerToReplace.Name = "playerToReplace";
			this.playerToReplace.TabIndex = 1;
			this.playerToReplace.SelectedIndexChanged += new System.EventHandler(this.list_SelectionChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Replace them with:";
			// 
			// botForOne
			// 
			this.botForOne.Checked = true;
			this.botForOne.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.botForOne.Location = new System.Drawing.Point(16, 72);
			this.botForOne.Name = "botForOne";
			this.botForOne.Size = new System.Drawing.Size(144, 24);
			this.botForOne.TabIndex = 3;
			this.botForOne.TabStop = true;
			this.botForOne.Text = "Bot for one turn only";
			// 
			// botForever
			// 
			this.botForever.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.botForever.Location = new System.Drawing.Point(16, 96);
			this.botForever.Name = "botForever";
			this.botForever.Size = new System.Drawing.Size(160, 24);
			this.botForever.TabIndex = 4;
			this.botForever.Text = "Bot until further notice";
			// 
			// person
			// 
			this.person.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.person.Location = new System.Drawing.Point(16, 120);
			this.person.Name = "person";
			this.person.Size = new System.Drawing.Size(96, 24);
			this.person.TabIndex = 5;
			this.person.Text = "This person:";
			this.person.CheckedChanged += new System.EventHandler(this.person_CheckedChanged);
			// 
			// replacementPlayer
			// 
			this.replacementPlayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.replacementPlayer.Enabled = false;
			this.replacementPlayer.Location = new System.Drawing.Point(128, 120);
			this.replacementPlayer.Name = "replacementPlayer";
			this.replacementPlayer.TabIndex = 6;
			this.replacementPlayer.SelectedIndexChanged += new System.EventHandler(this.list_SelectionChanged);
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(48, 160);
			this.button1.Name = "button1";
			this.button1.TabIndex = 7;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(144, 160);
			this.button2.Name = "button2";
			this.button2.TabIndex = 8;
			this.button2.Text = "Cancel";
			// 
			// ReplacePlayer
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(266, 200);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.button2,
																		  this.button1,
																		  this.replacementPlayer,
																		  this.person,
																		  this.botForever,
																		  this.botForOne,
																		  this.label2,
																		  this.playerToReplace,
																		  this.label1});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ReplacePlayer";
			this.Text = "Replace player";
			this.Load += new System.EventHandler(this.ReplacePlayer_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void ReplacePlayer_Load(object sender, System.EventArgs e)
		{
		
		}

		private void person_CheckedChanged(object sender, System.EventArgs e)
		{
			replacementPlayer.Enabled = person.Checked;
			list_SelectionChanged(sender, e);
		}

		private void list_SelectionChanged(object sender, System.EventArgs e)
		{
			if (playerToReplace.Text.EndsWith(" bot"))
			{
				botForOne.Enabled = botForever.Enabled = false;
				if (!person.Checked)
					person.Checked = true;
			}
			else
			{
				botForOne.Enabled = botForever.Enabled = true;
			}
			button1.Enabled = playerToReplace.SelectedIndex >= 0 
				&& playerToReplace.SelectedIndex < playerToReplace.Items.Count
				&& (!person.Checked ||
				(replacementPlayer.SelectedIndex >= 0
				&& replacementPlayer.SelectedIndex < replacementPlayer.Items.Count));
			if (person.Checked && playerToReplace.Text.StartsWith(replacementPlayer.Text + " ("))
				button1.Enabled = false;
		}

		public int PlayerToReplace
		{
			get { return playerToReplace.SelectedIndex;	}
		}

		public string ReplacementPlayer
		{
			get { return person.Checked ? replacementPlayer.Text : null; }
		}

		public bool Human
		{
			get { return person.Checked; }
		}

		public bool TemporaryBot
		{
			get { return botForOne.Checked; }
		}
	}
}
