
// MapGallery.cs

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Rails
{
	/// <summary>
	/// Summary description for MapGallery.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class MapGallery : System.Windows.Forms.Form
	{
		private ClearPanel panel1;
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		ArrayList seeds;
		ArrayList thumbs;
		private System.Windows.Forms.Label serialNo;
		private System.Windows.Forms.Button deleteMapButton;
		ArrayList filenames;

		public MapGallery()
		{
			InitializeComponent();
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
				foreach (Image img in thumbs)
					img.Dispose();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MapGallery));
			this.panel1 = new ClearPanel();
			this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
			this.button1 = new System.Windows.Forms.Button();
			this.deleteMapButton = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.serialNo = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.panel1.BackColor = System.Drawing.Color.Transparent;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.hScrollBar1});
			this.panel1.Location = new System.Drawing.Point(16, 16);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(688, 144);
			this.panel1.TabIndex = 0;
			this.panel1.Click += new System.EventHandler(this.panel1_Click);
			this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
			this.panel1.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
			this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
			this.panel1.MouseLeave += new System.EventHandler(this.panel1_MouseLeave);
			// 
			// hScrollBar1
			// 
			this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.hScrollBar1.Location = new System.Drawing.Point(0, 123);
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(684, 17);
			this.hScrollBar1.TabIndex = 0;
			this.hScrollBar1.ValueChanged += new System.EventHandler(this.hScrollBar1_ValueChanged);
			// 
			// button1
			// 
			this.button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(536, 168);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = Resource.GetString("Forms.OK");
			// 
			// deleteMapButton
			// 
			this.deleteMapButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.deleteMapButton.Enabled = false;
			this.deleteMapButton.Location = new System.Drawing.Point(24, 168);
			this.deleteMapButton.Name = "deleteMapButton";
			this.deleteMapButton.TabIndex = 2;
			this.deleteMapButton.Text = Resource.GetString("MapGallery.DeleteMap");
			this.deleteMapButton.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button3.Location = new System.Drawing.Point(624, 168);
			this.button3.Name = "button3";
			this.button3.TabIndex = 3;
			this.button3.Text = Resource.GetString("Forms.Cancel");
			// 
			// serialNo
			// 
			this.serialNo.Location = new System.Drawing.Point(120, 173);
			this.serialNo.Name = "serialNo";
			this.serialNo.Size = new System.Drawing.Size(152, 16);
			this.serialNo.TabIndex = 4;
			// 
			// MapGallery
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(720, 206);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.serialNo,
																		  this.button3,
																		  this.deleteMapButton,
																		  this.button1,
																		  this.panel1});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(2048, 240);
			this.MinimumSize = new System.Drawing.Size(472, 240);
			this.Name = "MapGallery";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = Resource.GetString("MapGallery.Title");
			this.Resize += new System.EventHandler(this.MapGallery_Resize);
			this.Load += new System.EventHandler(this.MapGallery_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		int margin = 8;
		int maxWidth, maxHeight;
		int selected = -1;
		int highlighted = -1;
		object seed;

		private void MapGallery_Load(object sender, System.EventArgs e)
		{
			seeds = new ArrayList();
			thumbs = new ArrayList();
			filenames = new ArrayList();
			maxWidth = int.MinValue;
			maxHeight = int.MinValue;
			bool ok = false;
			try
			{
				foreach (string filename in Directory.GetFiles(".\\maps", "*.rlm"))
				{
					string s = Path.GetFileNameWithoutExtension(filename);
					seeds.Add(s);
					Image thumb = LoadThumb(filename);
					thumbs.Add(thumb);
					filenames.Add(filename);
					int width = 128;
					if (width > maxWidth) maxWidth = width;
					int height = 105;
					if (height > maxHeight) maxHeight = height;
				}
				foreach (string filename in Directory.GetFiles(".\\maps", "*.png"))
				{
					string s = Path.GetFileNameWithoutExtension(filename);
					int number = unchecked((int) uint.Parse(s, System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
					seeds.Add(number);
					Image thumb = Image.FromFile(filename);
					thumbs.Add(thumb);
					filenames.Add(filename);
					int width = thumb.Width;
					int height = thumb.Height;
					if (width > maxWidth) maxWidth = width;
					if (height > maxHeight) maxHeight = height;
				}
				if (thumbs.Count == 0)
				{
					string message = Resource.GetString("MapGallery.NoSavedMaps");
					MessageBox.Show(message, this.Text);
				}
				hScrollBar1.Maximum = margin + (maxWidth + margin) * thumbs.Count - 1;
				hScrollBar1.LargeChange = panel1.ClientRectangle.Width;
				hScrollBar1.SmallChange = 10;
				ok = true;
			}
			catch(IOException)
			{
			}
			if (!ok)
			{
				MessageBox.Show(Resource.GetString("MapGallery.LoadError"), this.Text);
			}
		}

		static Image LoadThumb(string filename)
		{
			FileStream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(s);
			/* int version = */ r.ReadInt32();
			int length = r.ReadInt32();
			byte[] b = new Byte[length];
			r.Read(b, 0, length);
			r.Close();
			s.Close();
			MemoryStream ms = new MemoryStream(b);
			return Image.FromStream(ms);
		}

		private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Bitmap bmp = new Bitmap(panel1.ClientRectangle.Width, panel1.ClientRectangle.Height);
			Graphics g = Graphics.FromImage(bmp);
			g.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
			Pen pen = new Pen(Brushes.Red, 3);
			Brush brush = new SolidBrush(Color.FromArgb(128, Color.DarkBlue));
			for (int i=0; i<thumbs.Count; i++)
			{
				int x = margin + (maxWidth + margin) * i - hScrollBar1.Value;
				int y = margin;
				Image image = (Image) thumbs[i];
				g.DrawImageUnscaled(image, x, y);
				if (i == selected)
					g.DrawRectangle(pen, x - 2, y - 2, image.Width + 3, image.Height + 3);
				if (i == highlighted)
					g.FillRectangle(brush, x, y, image.Width, image.Height);
			}
			brush.Dispose();
			pen.Dispose();
			g.Dispose();
			e.Graphics.DrawImageUnscaled(bmp, 0, 0);
			bmp.Dispose();
		}

		private void MapGallery_Resize(object sender, System.EventArgs e)
		{
			hScrollBar1.LargeChange = panel1.ClientRectangle.Width;
			panel1.Invalidate();
		}

		private void hScrollBar1_ValueChanged(object sender, System.EventArgs e)
		{
			panel1.Invalidate();
		}

		private void panel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int i = (e.X + hScrollBar1.Value - margin) / (maxWidth + margin);
			if (i < 0 || i >= thumbs.Count) 
				i = -1;
			else
			{
				int x = margin + (maxWidth + margin) * i - hScrollBar1.Value;
				int y = margin;
				if (e.X - x >= maxWidth || e.Y - y >= maxHeight || e.Y - y < 0)
					i = -1;
			}
			if (highlighted == i)
				return;
			highlighted = i;
			panel1.Invalidate();
		}

		private void panel1_MouseLeave(object sender, System.EventArgs e)
		{
			if (highlighted == -1)
				return;
			highlighted = -1;
			panel1.Invalidate();
		}

		private void panel1_Click(object sender, System.EventArgs e)
		{
			if (selected == highlighted)
				return;
			selected = highlighted;
			if (selected != -1)
			{
				seed = seeds[selected];
				if (seed is int)
				{
					System.Globalization.NumberFormatInfo nf = System.Globalization.CultureInfo.CurrentUICulture.NumberFormat;
					string format = Resource.GetString("MapGallery.SerialNumberFormat");
					serialNo.Text = string.Format(nf, format, unchecked((uint) (int) seed));
					deleteMapButton.Enabled = true;
				}
				else
				{
					string format = Resource.GetString("MapGallery.MapNameFormat");
					serialNo.Text = string.Format(System.Globalization.CultureInfo.CurrentUICulture, format, (string)seed);
					deleteMapButton.Enabled = false;
				}
				Clipboard.SetDataObject(serialNo.Text, true);
			}
			else
			{
				serialNo.Text = String.Empty;
				deleteMapButton.Enabled = false;
			}
			button1.Enabled = (selected != -1);
			panel1.Invalidate();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			string message = Resource.GetString("MapGallery.AreYouSure");
			if (DialogResult.Yes == MessageBox.Show(message, this.Text, MessageBoxButtons.YesNo))
			{
				try
				{
					Image thumb = (Image) thumbs[selected];
					thumbs.RemoveAt(selected);
					thumb.Dispose();
					thumb = null;
					File.Delete((string) filenames[selected]);
					filenames.RemoveAt(selected);
					seeds.RemoveAt(selected);
					hScrollBar1.Maximum = margin + (maxWidth + margin) * thumbs.Count - 1;
					selected = -1;
					button1.Enabled = deleteMapButton.Enabled = false;
					panel1.Invalidate();
				}
				catch(IOException)
				{
					message = Resource.GetString("MapGallery.DeleteError");
					MessageBox.Show(message, this.Text);
				}
			}
		}

		private void panel1_DoubleClick(object sender, System.EventArgs e)
		{
			if (selected != -1)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		public object Seed
		{
			get
			{
				return seed;
			}
		}
	}
}

class ClearPanel : Panel
{
	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}
}
