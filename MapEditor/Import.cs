using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace MapEditor
{
	/// <summary>
	/// Summary description for Import.
	/// </summary>
	public class Import : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox preview;
		private System.Windows.Forms.CheckBox autoCorrect;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox name;
		private System.Windows.Forms.TrackBar threshhold;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Import()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Import));
			this.preview = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.autoCorrect = new System.Windows.Forms.CheckBox();
			this.OK = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.name = new System.Windows.Forms.TextBox();
			this.threshhold = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.threshhold)).BeginInit();
			this.SuspendLayout();
			// 
			// preview
			// 
			this.preview.Image = ((System.Drawing.Bitmap)(resources.GetObject("preview.Image")));
			this.preview.Location = new System.Drawing.Point(16, 80);
			this.preview.Name = "preview";
			this.preview.Size = new System.Drawing.Size(234, 57);
			this.preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.preview.TabIndex = 0;
			this.preview.TabStop = false;
			this.preview.Paint += new System.Windows.Forms.PaintEventHandler(this.preview_Paint);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(232, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Transparency preview";
			// 
			// autoCorrect
			// 
			this.autoCorrect.Location = new System.Drawing.Point(24, 152);
			this.autoCorrect.Name = "autoCorrect";
			this.autoCorrect.Size = new System.Drawing.Size(224, 24);
			this.autoCorrect.TabIndex = 2;
			this.autoCorrect.Text = "&Auto-correct transparency";
			this.autoCorrect.CheckedChanged += new System.EventHandler(this.autoCorrect_CheckedChanged);
			// 
			// OK
			// 
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(88, 232);
			this.OK.Name = "OK";
			this.OK.TabIndex = 3;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(176, 232);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 4;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Name";
			// 
			// name
			// 
			this.name.Location = new System.Drawing.Point(16, 32);
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(232, 20);
			this.name.TabIndex = 6;
			this.name.Text = "";
			this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
			// 
			// threshhold
			// 
			this.threshhold.Location = new System.Drawing.Point(16, 184);
			this.threshhold.Maximum = 17;
			this.threshhold.Name = "threshhold";
			this.threshhold.Size = new System.Drawing.Size(232, 45);
			this.threshhold.TabIndex = 7;
			this.threshhold.Value = 9;
			this.threshhold.Visible = false;
			this.threshhold.Scroll += new System.EventHandler(this.threshhold_Scroll);
			// 
			// Import
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(264, 272);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.threshhold,
																		  this.name,
																		  this.label2,
																		  this.cancel,
																		  this.OK,
																		  this.autoCorrect,
																		  this.label1,
																		  this.preview});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Import";
			this.Text = "Preview commodity icon";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Import_Closing);
			((System.ComponentModel.ISupportInitialize)(this.threshhold)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		const int D = 48;
		Bitmap orig = null;
		int size = 0;
		Bitmap thumb = null;
		object thumbLock = new object();

		public bool Open()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Picture files|*.bmp;*.png;*.gif;*.jpg|All files (*.*)|*.*";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					Bitmap bmp = (Bitmap) Bitmap.FromFile(dlg.FileName);
					int w = bmp.Width;
					int h = bmp.Height;
					size = w > h ? w : h;
					orig = new Bitmap(size, size);
					Graphics g = Graphics.FromImage(orig);
//					g.DrawImage(bmp, (size - w) / 2, (size - h) / 2, w * bmp.HorizontalResolution / orig.HorizontalResolution, h * bmp.VerticalResolution / orig.VerticalResolution);
					g.DrawImage(bmp, (size - w) / 2, (size - h) / 2, w, h);
					g.Dispose();
					bmp.Dispose();
					CreateThumbnail(false);
					name.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
					if (name.Text.Length > 1)
						name.Text = name.Text.Substring(0, 1).ToUpper() + name.Text.Substring(1);
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

		void CreateThumbnail(bool autoCorrect)
		{
			Bitmap temp = new Bitmap(orig.Width, orig.Height);
			Graphics g = Graphics.FromImage(temp);
			g.DrawImageUnscaled(orig, 0, 0);
			g.Dispose();

			if (autoCorrect)
			{
				Hashtable h = new Hashtable();
				for (int x=0; x<orig.Width; x++)
				{
					Color c = temp.GetPixel(x, 0);
					if (c.A == 255)
					{
						int k = c.ToArgb();
						if (h.ContainsKey(k))
							h[k] = (int)h[k] + 1;
						else
							h[k] = 1;
					}

					c = temp.GetPixel(x, orig.Height - 1);
					if (c.A == 255)
					{
						int k = c.ToArgb();
						if (h.ContainsKey(k))
							h[k] = (int)h[k] + 1;
						else
							h[k] = 1;
					}
				}
				for (int y=1; y<orig.Height-1; y++)
				{
					Color c = temp.GetPixel(0, y);
					if (c.A == 255)
					{
						int k = c.ToArgb();
						if (h.ContainsKey(k))
							h[k] = (int)h[k] + 1;
						else
							h[k] = 1;
					}

					c = temp.GetPixel(orig.Width - 1, y);
					if (c.A == 255)
					{
						int k = c.ToArgb();
						if (h.ContainsKey(k))
							h[k] = (int)h[k] + 1;
						else
							h[k] = 1;
					}
				}
				int max = int.MinValue;
				int best = -1;
				foreach (object key in h.Keys)
				{
					int n = (int) h[key];
					if (n > max)
					{
						max = n;
						best = (int) key;
					}
				}
				Color bg = Color.FromArgb(best);
				int limit = (int) (255.0 * Math.Pow(1.0 * threshhold.Value / threshhold.Maximum, 1.14));
				for (int x=0; x<temp.Width; x++)
					for (int y=0; y<temp.Height; y++)
					{
						Color c = temp.GetPixel(x, y);
						if (c.A == 255)
						{
							int dist = (int) Math.Sqrt((c.R-bg.R)*(c.R-bg.R)+(c.G-bg.G)*(c.G-bg.G)+(c.B-bg.B)*(c.B-bg.B));
							if (dist < limit)
							{
								double alpha = 1.0 * dist / limit;
								if (dist == 0)
									temp.SetPixel(x, y, Color.Transparent);
								else
								{
									double beta = 1.0 - alpha;
									double red = (c.R - bg.R * beta) / alpha;
									if (red < 0) red = 0; else if (red > 255) red = 255;
									double grn = (c.G - bg.G * beta) / alpha;
									if (grn < 0) grn = 0; else if (grn > 255) grn = 255;
									double blu = (c.B - bg.B * beta) / alpha;
									if (blu < 0) blu = 0; else if (blu > 255) blu = 255;
									c = Color.FromArgb((int)(255*alpha), (int)red, (int)grn, (int)blu);
									temp.SetPixel(x, y, c);
								}
							}
						}
					}
			}

			lock(thumbLock)
			{
				if (thumb != null)
					thumb.Dispose();

				thumb = new Bitmap(D, D);
				g = Graphics.FromImage(thumb);
				g.DrawImage(temp, new Rectangle(0, 0, D, D), 0, 0, temp.Width, temp.Height, GraphicsUnit.Pixel);
				g.Dispose();
			}

			temp.Dispose();
		}

		private void preview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			lock(thumbLock)
			{
				g.DrawImageUnscaled(thumb, 20, 4);
				g.DrawImageUnscaled(thumb, 90, 4);
			}
			Font font = new Font("Arial", 8.0f);
			g.DrawString(name.Text, font, Brushes.White, 144, 5);
			font.Dispose();
		}

		private void autoCorrect_CheckedChanged(object sender, System.EventArgs e)
		{
			threshhold.Visible = autoCorrect.Checked;
			CreateThumbnail(autoCorrect.Checked);
			preview.Refresh();
		}

		private void name_TextChanged(object sender, System.EventArgs e)
		{
			preview.Refresh();
		}

		private void threshhold_Scroll(object sender, System.EventArgs e)
		{
			CreateThumbnail(true);
			preview.Refresh();
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			
		}

		private void Import_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK)
				return;

			try
			{
				string exePath = Path.GetDirectoryName(Application.ExecutablePath);
				string icoPath = exePath + "\\commodities";
				if (!Directory.Exists(icoPath))
					Directory.CreateDirectory(icoPath);
				string icoFile = icoPath + "\\" + name.Text + ".bmp";
				if (File.Exists(icoFile))
				{
					if (MessageBox.Show("Do you want to overwrite the existing " + name.Text + " commodity?", "Commodity already exists", MessageBoxButtons.YesNo) == DialogResult.No)
					{
						e.Cancel = true;
						return;
					}
				}
				lock(thumbLock)
				{
					thumb.Save(icoFile);
				}
			}
			catch(System.IO.IOException ex)
			{
				MessageBox.Show("Unable to save " + name.Text + ". " + ex.Message);
				e.Cancel = true;
			}
		}
	}
}
