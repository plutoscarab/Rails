using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for Concede.
	/// </summary>
	public class Concede : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Concede(Game game)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.game = game;
		}

		Game game;

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Concede));
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(264, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "I, {0}, hereby concede the game to";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.Location = new System.Drawing.Point(16, 40);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(128, 22);
			this.comboBox1.TabIndex = 1;
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(248, 40);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(160, 40);
			this.button2.Name = "button2";
			this.button2.TabIndex = 3;
			this.button2.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(304, 32);
			this.label2.TabIndex = 4;
			this.label2.Text = "The game will end once all players concede the game to the same person on their t" +
				"urn.";
			// 
			// Concede
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(338, 112);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.button2,
																		  this.button1,
																		  this.comboBox1,
																		  this.label1});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Concede";
			this.Text = "Concede";
			this.Load += new System.EventHandler(this.Concede_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void Concede_Load(object sender, System.EventArgs e)
		{
			label1.Text = string.Format(label1.Text, game.state.ThisPlayer.Name);
			comboBox1.Items.Clear();
			comboBox1.Items.Add("(don't concede)");
			if (NetworkState.IsOffline)
			{
				for (int i=0; i<game.state.NumPlayers; i++)
					if (game.state.PlayerInfo[i].Human)
						if (game.state.PlayerInfo[i].Name != game.state.ThisPlayer.Name)
							if (!comboBox1.Items.Contains(game.state.PlayerInfo[i].Name))
								comboBox1.Items.Add(game.state.PlayerInfo[i].Name);
			}
			else
			{
				foreach (string user in NetworkState.CurrentUsers)
					if (user != game.state.ThisPlayer.Name)
						comboBox1.Items.Add(user);
			}
			for (int i=0; i<game.state.NumPlayers; i++)
				if (!game.state.PlayerInfo[i].Human)
					comboBox1.Items.Add(game.state.PlayerInfo[i].Name);
			comboBox1.SelectedIndex = 0;			
		}

		public string ConcedeTo
		{
			get 
			{
				if (comboBox1.SelectedIndex < 1)
					return null;
				return comboBox1.Text;
			}
		}
	}
}
