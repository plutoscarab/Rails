using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace Rails
{
	/// <summary>
	/// Summary description for GelButton.
	/// </summary>
	public class GelButton : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		Image[] buttonNormal, buttonDown, buttonDark, buttonGlow;
		bool isDown, isGlowing;
		Bitmap background;

		public GelButton()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			buttonNormal = LoadImages("");
			buttonDown = LoadImages("Down");
			buttonDark = LoadImages("Dark");
			buttonGlow = LoadImages("Glow");
		}

		private string label = "";

		public string Label
		{
			get { return label; }
			set { label = value; }
		}

		public override string Text
		{
			get { return label; }
			set { label = value; }
		}

		Image[] LoadImages(string suffix)
		{
			Image[] temp = new Image[4];
			for (int i=0; i<4; i++)
			{
				string filename = "Rails.Buttons." + ((ButtonColors) i).ToString() + "Button" + suffix + ".png";
				Stream stream;
				stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
				temp[i] = Image.FromStream(stream);
			}
			return temp;
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
				if (background != null)
					background.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// GelButton
			// 
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "GelButton";
			this.EnabledChanged += new System.EventHandler(this.GelButton_EnabledChanged);
			this.Load += new System.EventHandler(this.GelButton_Load);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GelButton_MouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.GelButton_Paint);
			this.MouseEnter += new System.EventHandler(this.GelButton_MouseEnter);
			this.MouseLeave += new System.EventHandler(this.GelButton_MouseLeave);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GelButton_MouseDown);

		}
		#endregion

		protected override CreateParams CreateParams 
		{ 
			get 
			{ 
				CreateParams cp = base.CreateParams; 
				cp.ExStyle|=0x00000020; //WS_EX_TRANSPARENT 
				return cp; 
			} 
		} 

		protected virtual void InvalidateEx() 
		{ 
			this.Invalidate();
			return;

			if (Parent == null) 
				return; 

			Rectangle rc = new Rectangle(this.Location, this.Size);
			Parent.Invalidate(rc, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		public enum ButtonColors
		{
			Blue = 0, Green = 1, Red = 2, Yellow = 3,
		}

		ButtonColors color = ButtonColors.Blue;

		public ButtonColors ButtonColor
		{
			get { return color; }
			set { color = value; }
		}

		private void GelButton_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			int dy = 0;
			Color fore = this.ForeColor;

			using (Bitmap buffer = new Bitmap(this.Width, this.Height))
			{
				using (Graphics g = Graphics.FromImage(buffer))
				{
					if (background != null)
						g.DrawImageUnscaled(background, 0, 0);

					if (this.Enabled)
						if (isDown)
						{
							g.DrawImageUnscaled(buttonDown[(int) color], 0, 2);
							dy = 2;
						}
						else if (isGlowing)
							g.DrawImageUnscaled(buttonGlow[(int) color], 0, 0);
						else
							g.DrawImageUnscaled(buttonNormal[(int) color], 0, 0);
					else
					{
						g.DrawImageUnscaled(buttonDark[(int) color], 0, 0);
						fore = Color.FromArgb(fore.A / 2, fore.R, fore.G, fore.B);
					}

					using (Brush brush = new SolidBrush(fore))
					using (StringFormat format = new StringFormat())
					{
						format.LineAlignment = StringAlignment.Center;
						format.Alignment = StringAlignment.Center;
						Rectangle r = this.ClientRectangle;
						r.Offset(0, dy);
						g.DrawString(label, this.Font, brush, r, format);
					}
				}

				e.Graphics.DrawImageUnscaled(buffer, 0, 0);
			}
		}

		private void GelButton_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!isDown)
			{
				isDown = true;
				InvalidateEx();
			}
		}

		private void GelButton_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (isDown)
			{
				isDown = false;
				InvalidateEx();
			}
		}

		private void GelButton_MouseLeave(object sender, System.EventArgs e)
		{
			if (isGlowing)
			{
				isGlowing = false;
				InvalidateEx();
			}
		}

		private void GelButton_EnabledChanged(object sender, System.EventArgs e)
		{
			InvalidateEx();
		}

		private void GelButton_MouseEnter(object sender, System.EventArgs e)
		{
			if (!isGlowing)
			{
				isGlowing = true;
				InvalidateEx();
			}
		}

		private void GelButton_Load(object sender, System.EventArgs e)
		{
			if (Parent == null)
				return;

			background = new Bitmap(this.Width, this.Height);
			using (Graphics g = Graphics.FromImage(background))
				g.DrawImageUnscaled(Parent.BackgroundImage, -this.Location.X, - this.Location.Y);
		}
	}
}
