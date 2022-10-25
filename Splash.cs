
// Splash.cs

/*
 * Splash screen and selection of local vs networked game.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for Splash.
	/// </summary>
	public class Splash : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Splash()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

//			this.BackgroundImage = Images.Logo;
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
			// 
			// Splash
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(461, 700);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Splash";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Rails";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Splash_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseMove);

		}
		#endregion

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		Bitmap buffer = new Bitmap(461, 700);
		const int spacing = 75;
		const int left = 30;
		const int top = 350;

		private void Splash_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			using (Graphics g = Graphics.FromImage(buffer))
			{
				g.DrawImage(Images.Logo, 0, 0, 461, 700);
				if (!m1)
					g.DrawImageUnscaled(Images.GelButton2, left, top);
				else if (down)
					g.DrawImageUnscaled(Images.GelButton3, left, top);
				else
					g.DrawImageUnscaled(Images.GelButton1, left, top);
				if (!m2)
					g.DrawImageUnscaled(Images.GelButton2, left, top + spacing);
				else if (down)
					g.DrawImageUnscaled(Images.GelButton3, left, top + spacing);
				else
					g.DrawImageUnscaled(Images.GelButton1, left, top + spacing);
				if (!m3)
					g.DrawImageUnscaled(Images.GelButton2, left, top + 2*spacing);
				else if (down)
					g.DrawImageUnscaled(Images.GelButton3, left, top + 2*spacing);
				else
					g.DrawImageUnscaled(Images.GelButton1, left, top + 2*spacing);
				if (!m4)
					g.DrawImageUnscaled(Images.GelButton2, left, top + 3*spacing);
				else if (down)
					g.DrawImageUnscaled(Images.GelButton3, left, top + 3*spacing);
				else
					g.DrawImageUnscaled(Images.GelButton1, left, top + 3*spacing);
				using (StringFormat format = new StringFormat())
				{
					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;
					using (Font font = new Font("Engravers MT", 12.0f))
					{
						g.DrawString("Play on this computer only", font, Brushes.White, c1, format);
						g.DrawString("Host a networked game", font, Brushes.White, c2, format);
						g.DrawString("Join a networked game", font, Brushes.White, c3, format);
						g.DrawString("Exit", font, Brushes.White, c4, format);
					}
				}
			}

			e.Graphics.DrawImageUnscaled(buffer, 0, 0);
		}

		bool down = false;
		const float xc = left + 17 + 370/2;
		const float yc = top + 5 + 75/2;

		Rectangle r1 = new Rectangle(left + 17, top + 5, 370, 75);
		PointF c1 = new PointF(xc, yc);
		bool m1 = false;

		Rectangle r2 = new Rectangle(left + 17, top + 5 + spacing, 370, 75);
		PointF c2 = new PointF(xc, yc + spacing);
		bool m2 = false;

		Rectangle r3 = new Rectangle(left + 17, top + 5 + 2*spacing, 370, 75);
		PointF c3 = new PointF(xc, yc + 2 * spacing);
		bool m3 = false;

		Rectangle r4 = new Rectangle(left + 17, top + 5 + 3*spacing, 370, 75);
		PointF c4 = new PointF(xc, yc + 3 * spacing);
		bool m4 = false;


		private void Splash_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			bool b1 = r1.Contains(e.X, e.Y);
			if (b1 != m1)
			{
				m1 = b1;
				this.Invalidate(r1);
			}
			bool b2 = r2.Contains(e.X, e.Y);
			if (b2 != m2)
			{
				m2 = b2;
				this.Invalidate(r2);
			}
			bool b3 = r3.Contains(e.X, e.Y);
			if (b3 != m3)
			{
				m3 = b3;
				this.Invalidate(r3);
			}
			bool b4 = r4.Contains(e.X, e.Y);
			if (b4 != m4)
			{
				m4 = b4;
				this.Invalidate(r4);
			}
		}

		private void Splash_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!down && (m1 || m2 || m3 || m4))
			{
				down = true;
				this.Invalidate();
			}
		}

		private void Splash_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (down)
			{
				down = false;
				this.Invalidate();
				if (m4)
				{
					this.DialogResult = DialogResult.Cancel;
					this.Close();
				}
			}
		}
	}
}
