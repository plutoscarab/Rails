
// About.cs

/*
 * The Help/About dialog box, which shows the build number and provides
 * a link to the web site to get an updated version.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Rails
{
	[ComVisible(false)]
	public class About : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public About()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(About));
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(176, 152);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(88, 152);
			this.button2.Name = "button2";
			this.button2.TabIndex = 2;
			this.button2.Text = "Web Site";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(16, 24);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(240, 112);
			this.textBox1.TabIndex = 5;
			this.textBox1.Text = "PC Rails version {0}\r\n\r\nCopyright ©{1} Bret Mulvey. All Rights Reserved.\r\n\r\nSpeci" +
				"al thanks to Sky Sternberg for endless playtesting and encouragement.";
			// 
			// About
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(274, 192);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.textBox1,
																		  this.button2,
																		  this.button1});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "About";
			this.Text = "About Rails";
			this.Load += new System.EventHandler(this.About_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void About_Load(object sender, System.EventArgs e)
		{
			this.BackgroundImage = Startup.Current.BackgroundImage;
			Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			DateTime buildDate = (new DateTime(2000, 1, 1)).AddDays(thisVersion.Build);
			textBox1.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, textBox1.Text, thisVersion, buildDate.Year);
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("http://bretm.home.comcast.net/rails.html");
		}
	}
}
