using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	[System.Runtime.InteropServices.ComVisible(false)]
	public class ChooseTrack : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox checkBox6;
		private System.Windows.Forms.CheckBox checkBox5;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox0;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBox7;
		private System.Windows.Forms.CheckBox checkBox8;
		private System.Windows.Forms.CheckBox checkBox9;
		private System.Windows.Forms.CheckBox checkBox10;
		private System.Windows.Forms.CheckBox checkBox11;
		private CheckBox[] checkBoxes;
		private System.Windows.Forms.CheckBox botImage;
		public bool[] UseTrack;

		public ChooseTrack(GameState state)
		{
			InitializeComponent();

			checkBoxes = new CheckBox[12] {checkBox0, checkBox1, checkBox2, checkBox3,
											  checkBox4, checkBox5, checkBox6, checkBox7, 
											  checkBox8, checkBox9, checkBox10, checkBox11};

			UseTrack = (bool[]) state.UseTrack.Clone();

			for (int i=0; i<state.NumPlayers; i++)
			{
				int tc = state.PlayerInfo[i].TrackColor;
				checkBoxes[i].Enabled = true;
				checkBoxes[i + 6].Enabled = true;
				checkBoxes[i].BackColor = Game.TrackColor[tc];
				if (i == state.CurrentPlayer)
				{
					checkBoxes[i].Enabled = false;
					checkBoxes[i + 6].Enabled = false;
				}
				checkBoxes[i].Checked = UseTrack[i];
				checkBoxes[i + 6].Checked = UseTrack[i];
				if (!state.PlayerInfo[i].Human)
					checkBoxes[i].Image = botImage.Image;
			}

			for (int i=state.NumPlayers; i<6; i++)
			{
				checkBoxes[i].Visible = false;
				checkBoxes[i + 6].Visible = false;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseTrack));
			this.button7 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.checkBox6 = new System.Windows.Forms.CheckBox();
			this.checkBox5 = new System.Windows.Forms.CheckBox();
			this.checkBox4 = new System.Windows.Forms.CheckBox();
			this.checkBox3 = new System.Windows.Forms.CheckBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.checkBox0 = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBox7 = new System.Windows.Forms.CheckBox();
			this.checkBox8 = new System.Windows.Forms.CheckBox();
			this.checkBox9 = new System.Windows.Forms.CheckBox();
			this.checkBox10 = new System.Windows.Forms.CheckBox();
			this.checkBox11 = new System.Windows.Forms.CheckBox();
			this.botImage = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// button7
			// 
			this.button7.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button7.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button7.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.button7.Location = new System.Drawing.Point(168, 80);
			this.button7.Name = "button7";
			this.button7.TabIndex = 6;
			this.button7.Text = Resource.GetString("Forms.Cancel");
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.button1.Location = new System.Drawing.Point(168, 48);
			this.button1.Name = "button1";
			this.button1.TabIndex = 13;
			this.button1.Text = Resource.GetString("Forms.OK");
			// 
			// checkBox6
			// 
			this.checkBox6.Location = new System.Drawing.Point(64, 48);
			this.checkBox6.Name = "checkBox6";
			this.checkBox6.Size = new System.Drawing.Size(16, 24);
			this.checkBox6.TabIndex = 77;
			this.checkBox6.Tag = "0";
			this.checkBox6.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// checkBox5
			// 
			this.checkBox5.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox5.BackColor = System.Drawing.Color.Green;
			this.checkBox5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox5.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox5.Image")));
			this.checkBox5.Location = new System.Drawing.Point(96, 112);
			this.checkBox5.Name = "checkBox5";
			this.checkBox5.Size = new System.Drawing.Size(28, 28);
			this.checkBox5.TabIndex = 76;
			this.checkBox5.Tag = "5";
			this.checkBox5.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// checkBox4
			// 
			this.checkBox4.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox4.BackColor = System.Drawing.Color.Green;
			this.checkBox4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox4.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox4.Image")));
			this.checkBox4.Location = new System.Drawing.Point(96, 80);
			this.checkBox4.Name = "checkBox4";
			this.checkBox4.Size = new System.Drawing.Size(28, 28);
			this.checkBox4.TabIndex = 75;
			this.checkBox4.Tag = "4";
			this.checkBox4.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// checkBox3
			// 
			this.checkBox3.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox3.BackColor = System.Drawing.Color.Green;
			this.checkBox3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox3.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox3.Image")));
			this.checkBox3.Location = new System.Drawing.Point(96, 48);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new System.Drawing.Size(28, 28);
			this.checkBox3.TabIndex = 74;
			this.checkBox3.Tag = "3";
			this.checkBox3.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// checkBox2
			// 
			this.checkBox2.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox2.BackColor = System.Drawing.Color.Green;
			this.checkBox2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox2.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox2.Image")));
			this.checkBox2.Location = new System.Drawing.Point(24, 112);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(28, 28);
			this.checkBox2.TabIndex = 73;
			this.checkBox2.Tag = "2";
			this.checkBox2.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// checkBox1
			// 
			this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox1.BackColor = System.Drawing.Color.Green;
			this.checkBox1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox1.Image")));
			this.checkBox1.Location = new System.Drawing.Point(24, 80);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(28, 28);
			this.checkBox1.TabIndex = 72;
			this.checkBox1.Tag = "1";
			this.checkBox1.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// checkBox0
			// 
			this.checkBox0.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox0.BackColor = System.Drawing.Color.Green;
			this.checkBox0.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.checkBox0.Image = ((System.Drawing.Bitmap)(resources.GetObject("checkBox0.Image")));
			this.checkBox0.Location = new System.Drawing.Point(24, 48);
			this.checkBox0.Name = "checkBox0";
			this.checkBox0.Size = new System.Drawing.Size(28, 28);
			this.checkBox0.TabIndex = 71;
			this.checkBox0.Tag = "0";
			this.checkBox0.CheckedChanged += new System.EventHandler(this.icon_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(264, 23);
			this.label1.TabIndex = 78;
			this.label1.Text = Resource.GetString("ChooseTrack.IndicatePlayers");
			// 
			// checkBox7
			// 
			this.checkBox7.Location = new System.Drawing.Point(64, 80);
			this.checkBox7.Name = "checkBox7";
			this.checkBox7.Size = new System.Drawing.Size(16, 24);
			this.checkBox7.TabIndex = 79;
			this.checkBox7.Tag = "1";
			this.checkBox7.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// checkBox8
			// 
			this.checkBox8.Location = new System.Drawing.Point(64, 112);
			this.checkBox8.Name = "checkBox8";
			this.checkBox8.Size = new System.Drawing.Size(16, 24);
			this.checkBox8.TabIndex = 80;
			this.checkBox8.Tag = "2";
			this.checkBox8.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// checkBox9
			// 
			this.checkBox9.Location = new System.Drawing.Point(136, 48);
			this.checkBox9.Name = "checkBox9";
			this.checkBox9.Size = new System.Drawing.Size(16, 24);
			this.checkBox9.TabIndex = 81;
			this.checkBox9.Tag = "3";
			this.checkBox9.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// checkBox10
			// 
			this.checkBox10.Location = new System.Drawing.Point(136, 80);
			this.checkBox10.Name = "checkBox10";
			this.checkBox10.Size = new System.Drawing.Size(16, 24);
			this.checkBox10.TabIndex = 82;
			this.checkBox10.Tag = "4";
			this.checkBox10.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// checkBox11
			// 
			this.checkBox11.Location = new System.Drawing.Point(136, 112);
			this.checkBox11.Name = "checkBox11";
			this.checkBox11.Size = new System.Drawing.Size(16, 24);
			this.checkBox11.TabIndex = 83;
			this.checkBox11.Tag = "5";
			this.checkBox11.CheckedChanged += new System.EventHandler(this.box_CheckedChanged);
			// 
			// botImage
			// 
			this.botImage.Appearance = System.Windows.Forms.Appearance.Button;
			this.botImage.BackColor = System.Drawing.Color.Green;
			this.botImage.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.botImage.Image = ((System.Drawing.Bitmap)(resources.GetObject("botImage.Image")));
			this.botImage.Location = new System.Drawing.Point(192, 112);
			this.botImage.Name = "botImage";
			this.botImage.Size = new System.Drawing.Size(28, 28);
			this.botImage.TabIndex = 84;
			this.botImage.Tag = "5";
			this.botImage.Visible = false;
			// 
			// ChooseTrack
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.button7;
			this.ClientSize = new System.Drawing.Size(258, 152);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.botImage,
																		  this.checkBox11,
																		  this.checkBox10,
																		  this.checkBox9,
																		  this.checkBox8,
																		  this.checkBox7,
																		  this.label1,
																		  this.checkBox6,
																		  this.checkBox5,
																		  this.checkBox4,
																		  this.checkBox3,
																		  this.checkBox2,
																		  this.checkBox1,
																		  this.checkBox0,
																		  this.button1,
																		  this.button7});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChooseTrack";
			this.Text = Resource.GetString("ChooseTrack.Title");
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void icon_CheckedChanged(object sender, System.EventArgs e)
		{
			CheckBox c = sender as CheckBox;
			int tag = int.Parse((string) c.Tag, System.Globalization.CultureInfo.InvariantCulture);
			checkBoxes[tag + 6].Checked = c.Checked;
			UseTrack[tag] = c.Checked;
		}

		private void box_CheckedChanged(object sender, System.EventArgs e)
		{
			CheckBox c = sender as CheckBox;
			int tag = int.Parse((string) c.Tag, System.Globalization.CultureInfo.InvariantCulture);
			checkBoxes[tag].Checked = c.Checked;
			UseTrack[tag] = c.Checked;
		}
	}
}
