using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Microsoft.Win32;

namespace Magnifier
{
	public class main : System.Windows.Forms.Form
	{
		const int focusWidth = 864;
		const int focusHeight = 712;

		private System.ComponentModel.Container components = null;

		Bitmap background, buffer;
		int mouseX = 0, mouseY = 0;

		public main()
		{
			InitializeComponent();

			buffer = new Bitmap(Screen.PrimaryScreen.Bounds.Width, 
				Screen.PrimaryScreen.Bounds.Height);
			Bitmap myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
				Screen.PrimaryScreen.Bounds.Height);
			Graphics gr1 = Graphics.FromImage(myImage);
			IntPtr dc1 = gr1.GetHdc();
			IntPtr dc2 = GetWindowDC(GetDesktopWindow());
			BitBlt(dc1, 0, 0, Screen.PrimaryScreen.Bounds.Width,
				Screen.PrimaryScreen.Bounds.Height, dc2, 0, 0, 13369376);
			gr1.ReleaseHdc(dc1);
			background = myImage;
			this.BackgroundImage = myImage;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			Cursor.Show();

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(main));
			// 
			// main
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "main";
			this.Text = "Rails Map Frame";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.main_KeyDown);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.main_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.main_MouseMove);
			this.Click += new EventHandler(this.main_Click);
		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new main());
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr GetDesktopWindow();

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hwnd);

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern UInt64 BitBlt
			(IntPtr hDestDC,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hSrcDC,
			int xSrc,
			int ySrc,
			System.Int32 dwRop);

		private void main_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = Graphics.FromImage(buffer);
			g.DrawImageUnscaled(background, 0, 0, this.Width, this.Height);
			using (Brush br = new SolidBrush(Color.FromArgb(100, Color.White)))
			{
				g.FillRectangle(br, mouseX - focusWidth / 2 + 1, mouseY - focusHeight / 2 + 1, focusWidth - 1, focusHeight - 1);
			}
			using (Pen pe = new Pen(Color.FromArgb(200, Color.White)))
			{
				g.DrawRectangle(pe, mouseX - focusWidth / 2, mouseY - focusHeight / 2, focusWidth, focusHeight);
			}
			g.Dispose();
			e.Graphics.DrawImageUnscaled(buffer, 0, 0, this.Width, this.Height);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		private void main_Click(object sender, EventArgs e)
		{
			Bitmap temp = new Bitmap(focusWidth, focusHeight);
			Graphics g = Graphics.FromImage(temp);
			Rectangle destRect = new Rectangle(0, 0, focusWidth, focusHeight);
			g.DrawImage(background, destRect, mouseX - focusWidth / 2, mouseY - focusHeight / 2, focusWidth, focusHeight, GraphicsUnit.Pixel);
			g.Dispose();
			temp.Save("capture.bmp");
			temp.Dispose();
			Application.Exit();
		}

		private void main_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			mouseX = e.X;
			mouseY = e.Y;
			this.Refresh();
		}

		private void main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyValue)
			{
				default:
					Application.Exit();
					break;
			}
		}

	}
}
