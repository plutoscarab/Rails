using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for Newspaper.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class Newspaper : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		string headline, subhead;

		public Newspaper(string headline, string subhead)
		{
			this.headline = headline;
			this.subhead = subhead;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Newspaper));
			// 
			// Newspaper
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackgroundImage = ((System.Drawing.Bitmap)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = new System.Drawing.Size(296, 214);
			this.ControlBox = false;
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Newspaper";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Click += new System.EventHandler(this.Newspaper_Click);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Newspaper_Paint);

		}
		#endregion

		private void Newspaper_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			StringFormat format = new StringFormat();
			format.Alignment = StringAlignment.Center;
			Rectangle hr = new Rectangle(4, 43, 294, 53);
			Font font = new Font("Times New Roman", 24.0f);
			e.Graphics.DrawString(headline, font, Brushes.Black, hr.Left + hr.Width / 2, hr.Top - 4, format);
			font.Dispose();
			font = new Font("Times New Roman", 16.0f);
			format.LineAlignment = StringAlignment.Far;
			e.Graphics.DrawString(subhead, font, Brushes.Black, hr.Left + hr.Width / 2, hr.Bottom + 2, format);
			font.Dispose();
			format.Dispose();
		}

		private void Newspaper_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
