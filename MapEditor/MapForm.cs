using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MapEditor
{
	[Flags]
	public enum TerrainType
	{
		Inaccessible = 1,	// mileposts removed from the map because they're not accessible
		Sea = 2,			
		Clear = 4,			
		Mountain = 8,
		Alpine = 16,
		Port = 32,
	}

	public enum EditMode
	{
		CityOrSea,	// edit cities on land, edit sea markers on water
		Terrain,	// set terrain type
		River,		// add rivers
		Causeway,	// edit causeways on land, edit ports/ferry routes on water
	}

	/// <summary>
	/// Summary description for MapForm.
	/// </summary>
	public class MapForm : System.Windows.Forms.Form
	{

		private System.Windows.Forms.PictureBox mapPicture;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		const int width = 864;
		const int height = 712;
		const int gridSpacing = 15;				// horizontal spacing of mileposts
		const int gridSpacingY = (int) (gridSpacing * 2 / 1.732);
		const int gridW = (width - gridSpacing / 2) / gridSpacing;
		const int gridH = height / gridSpacingY;

		TerrainType[,] terrain = new TerrainType[gridW, gridH];
		int fade;
		bool dirty = false;
		public ArrayList Cities;
		ArrayList seas = new ArrayList();
		EditMode editMode = EditMode.CityOrSea;
		bool[,,] causeway = new bool[gridW, gridH, 6];

		public MapForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			InitializeTerrain();
			fade = 0;
			Cities = new ArrayList();
			ChooseBackground();
			Cursor = Cursors.Cross;
		}

		public MapForm(string filename)
		{
			InitializeComponent();

			Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(s);
			int version = r.ReadInt32();

			// skip over the thumbnail image
			if (version >= 3)
			{
				SkipBlock(r);
			}

			// read the background image
			int length = r.ReadInt32();
			byte[] bfc = new Byte[length];
			r.Read(bfc, 0, length);
			mapPicture.BackgroundImage = ImageFromBytes(bfc);
			backgroundFilename = null;
			backgroundFileContents = bfc;

			// read the mileposts
			for (int i=0; i<gridW; i++)
				for (int j=0; j<gridH; j++)
					terrain[i, j] = (TerrainType) r.ReadByte();

			// read the background fade level
			if (version >= 2)
				fade = r.ReadInt32();
			else
				fade = 0;

			// read the cities
			Cities = new ArrayList();
			if (version >= 5)
			{
				int count = r.ReadInt32();
				for (int i=0; i<count; i++)
					Cities.Add(new City(r));
			}
			else if (version == 4)
			{
				for (int i=0; i<gridW; i++)
					for (int j=0; j<gridH; j++)
					{
						CityType c = (CityType) r.ReadByte();
						if (c == CityType.Town || c == CityType.City || c == CityType.Capital)
						{
							City city = new City(c, i, j);
							Cities.Add(city);
						}
					}
			}
			else
			{
			}

			// read the commodity icons
			if (version >= 7)
			{
				int count = r.ReadInt32();
				for (int i=0; i<count; i++)
				{
					string product = r.ReadString();
					length = r.ReadInt32();
					byte[] b = new byte[length];
					r.Read(b, 0, length);
					Image image = ImageFromBytes(b);
					Images.SubmitStoredImage(product, image);
				}
			}

			// read the sea locations
			if (version >= 6)
			{
				int count = r.ReadInt32();
				seas = new ArrayList();
				for (int i=0; i<count; i++)
				{
					int x = r.ReadInt32();
					int y = r.ReadInt32();
					seas.Add(new Point(x, y));
				}
			}

			// read the rivers
			rivers = new ArrayList();
			if (version >= 8)
			{
				int count = r.ReadInt32();
				for (int i=0; i<count; i++)
				{
					string name = r.ReadString();
					int npoints = r.ReadInt32();
					ArrayList points = new ArrayList();
					for (int j=0; j<npoints; j++)
					{
						float x = r.ReadSingle();
						float y = r.ReadSingle();
						if (Math.Abs(x - Math.Round(x, 0)) < 0.0001)
							x += Jiggle();
						if (Math.Abs(y - Math.Round(y, 0)) < 0.0001)
							y += Jiggle();
						points.Add(new PointF(x, y));
					}
					rivers.Add(new River(name, points));
				}
			}

			// read the causeways
			if (version >= 9)
			{
				for (int i=0; i<gridW; i++)
					for (int j=0; j<gridH; j++)
					{
						byte b = r.ReadByte();
						causeway[i, j, 0] = (b & 1) != 0;
						causeway[i, j, 1] = (b & 2) != 0;
						causeway[i, j, 2] = (b & 4) != 0;
						int x, y;
						for (int d=0; d<3; d++)
							if (GetAdjacent(i, j, d, out x, out y))
								causeway[x, y, (d+3)%6] = causeway[i, j, d];
					}
			}

			r.Close();
			s.Close();
			this.Text = Path.GetFileNameWithoutExtension(filename);
			this.Refresh();
		}

		void SkipBlock(BinaryReader r)
		{
			int length = r.ReadInt32();
			r.BaseStream.Seek(length, SeekOrigin.Current);
		}

		public bool Save()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.AddExtension = true;
			dlg.DefaultExt = ".rlm";
			dlg.Filter = "Rails Map (*.rlm)|*.rlm|All Files (*.*)|*.*";
			dlg.OverwritePrompt = true;
			dlg.ValidateNames = true;
			dlg.Title = "Save Map As";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if (File.Exists(dlg.FileName))
					{
						string bak = Path.GetDirectoryName(dlg.FileName) + "\\" + Path.GetFileNameWithoutExtension(dlg.FileName) + ".bak";
						if (File.Exists(bak))
							File.Delete(bak);
						File.Move(dlg.FileName, bak);
					}

					FileStream s = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
					BinaryWriter w = new BinaryWriter(s);

					int version = 9;
					w.Write(version);

					byte[] thumbnail = CreateThumbnail();
					w.Write(thumbnail.Length);
					w.Write(thumbnail);

					int L = backgroundFileContents.Length;
					w.Write(L);
					w.Write(backgroundFileContents);
					for (int i=0; i<gridW; i++)
						for (int j=0; j<gridH; j++)
							w.Write((byte) terrain[i, j]);

					w.Write(fade);

					w.Write(Cities.Count);
					Hashtable products = new Hashtable();
					foreach (City city in Cities)
					{
						city.Write(w);
						foreach (string product in city.Products)
							if (!products.ContainsKey(product))
								products[product] = products.Count;
					}

					w.Write(products.Count);
					foreach (string product in products.Keys)
					{
						w.Write(product);
						byte[] icon = ImageBytes(Images.CommodityImage(product), System.Drawing.Imaging.ImageFormat.Png);
						w.Write(icon.Length);
						w.Write(icon);
					}

					w.Write(seas.Count);
					foreach (Point p in seas)
					{
						w.Write(p.X);
						w.Write(p.Y);
					}

					w.Write(rivers.Count);
					foreach (River river in rivers)
					{
						w.Write(river.Name);
						ArrayList p = river.Points;
						w.Write(p.Count);
						foreach (PointF pf in p)
						{
							w.Write(pf.X);
							w.Write(pf.Y);
						}
					}

					for (int i=0; i<gridW; i++)
						for (int j=0; j<gridH; j++)
						{
							byte b = 0;
							if (causeway[i, j, 0]) b |= 1;
							if (causeway[i, j, 1]) b |= 2;
							if (causeway[i, j, 2]) b |= 4;
							w.Write(b);
						}

					w.Close();
					s.Close();
					dirty = false;
					return true;
				}
				catch(System.IO.IOException e)
				{
					MessageBox.Show("Unable to save. " + e.Message);
				}
			}
			return false;
		}

		byte[] CreateThumbnail()
		{
			int tw = 128;
			int th = 105;
			Bitmap bmp = new Bitmap(tw, th);
			Graphics g = Graphics.FromImage(bmp);
			g.DrawImage(mapPicture.BackgroundImage, new Rectangle(0, 0, tw, th), 0, 0, width, height, GraphicsUnit.Pixel);
			g.Dispose();
			byte[] temp = ImageBytes(bmp, System.Drawing.Imaging.ImageFormat.Jpeg);
			bmp.Dispose();
			return temp;
		}

		byte[] ImageBytes(Image bmp, System.Drawing.Imaging.ImageFormat imageFormat)
		{
			MemoryStream ms = new MemoryStream();
			bmp.Save(ms, imageFormat);
			byte[] temp = ms.ToArray();
			ms.Close();
			return temp;
		}

		Image ImageFromBytes(byte[] b)
		{
			MemoryStream ms = new MemoryStream(b);
			return Image.FromStream(ms);
		}

		void InitializeTerrain()
		{
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					terrain[x, y] = TerrainType.Clear;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MapForm));
			this.mapPicture = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// mapPicture
			// 
			this.mapPicture.Name = "mapPicture";
			this.mapPicture.Size = new System.Drawing.Size(864, 712);
			this.mapPicture.TabIndex = 0;
			this.mapPicture.TabStop = false;
			this.mapPicture.Click += new System.EventHandler(this.mapPicture_Click);
			this.mapPicture.Paint += new System.Windows.Forms.PaintEventHandler(this.mapPicture_Paint);
			this.mapPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapPicture_MouseMove);
			this.mapPicture.MouseLeave += new System.EventHandler(this.mapPicture_MouseLeave);
			this.mapPicture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mapPicture_MouseDown);
			// 
			// MapForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(864, 710);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.mapPicture});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MapForm";
			this.Text = "New map";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MapForm_KeyDown);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MapForm_Closing);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MapForm_KeyPress);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MapForm_KeyUp);
			this.ResumeLayout(false);

		}
		#endregion

		string backgroundFilename = null;
		byte[] backgroundFileContents = null;

		public void ChooseBackground()
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "Image files|*.bmp;*.jpg;*.png;*.gif|All files (*.*)|*.*" ;
			openFileDialog1.Title = "Choose Map Background Image";
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					string filename = openFileDialog1.FileName;
					byte[] bfc = FileContents(filename);
					Stream s = new MemoryStream(bfc);
					mapPicture.BackgroundImage = Image.FromStream(s);
					dirty = true;
					this.Text = Path.GetFileNameWithoutExtension(filename);
					backgroundFilename = filename;
					backgroundFileContents = bfc;
				}
				catch(OutOfMemoryException)
				{
					MessageBox.Show("The format of the selected image is not supported.");
				}
			}
		}

		bool ValidCoord(int i, int j)
		{
			int h = gridH;
			return i>=0 && i<gridW && j>=0 && j<h;
		}

		// get the screen coordinates of a milepost
		public bool GetCoord(int i, int j, out int x, out int y)
		{
			x = 0; y = 0;
			if (!ValidCoord(i, j))
				return false;

			int xm = (width - (gridW - 1) * gridSpacing) / 2;
			x = i * gridSpacing + xm;
			y = j * gridSpacingY + gridSpacingY / 2;
			if (i % 2 == 1)
				y += gridSpacingY / 2;
			return true;
		}

		// get the milepost that is closest to the location of the mouse
		public bool GetNearest(int x, int y, out int i, out int j)
		{
			int xm = (width - (gridW - 1) * gridSpacing) / 2;
			i = (x - xm + gridSpacing / 2) / gridSpacing;
			if (i % 2 == 1)
				y -= gridSpacingY / 2;
			j = y / gridSpacingY;
			return ValidCoord(i, j);
		}

		// get the grid location of the adjacent milepost in the specified direction
		public bool GetAdjacent(int i, int j, int d, out int x, out int y)
		{
			x = y = 0;
			switch(d)
			{
				case 0:	// north
					x = i;
					y = j - 1;
					break;
				case 1: // northeast
					x = i + 1;
					y = i % 2 == 0 ? j - 1 : j;
					break;
				case 2: // southeast
					x = i + 1;
					y = i % 2 == 0 ? j : j + 1;
					break;
				case 3: // south
					x = i;
					y = j + 1;
					break;
				case 4: // southwest
					x = i - 1;
					y = i % 2 == 0 ? j : j + 1;
					break;
				case 5: // northwest
					x = i - 1;
					y = i % 2 == 0 ? j - 1 : j;
					break;
			}
			return ValidCoord(x, y);
		}

		void DrawCities(Graphics g)
		{
			int x, y;
			Font font = new Font("Arial", 8.0f);
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			foreach (City city in Cities)
				if (GetCoord(city.X, city.Y, out x, out y))
				{
					SizeF size = g.MeasureString(city.Name, font);
					switch(city.Type)
					{
						case CityType.None:
							break;
						case CityType.Town:
							g.DrawImageUnscaled(Images.Town, x - 7, y - 7);
							DrawCityName(g, city.Name, font, x - size.Width * x / width, y + 7);
							break;
						case CityType.City:
							g.DrawImageUnscaled(Images.City, x - 7, y - 7);
							DrawCityName(g, city.Name, font, x - size.Width * x / width, y + 7);
							break;
						case CityType.Capital:
							g.DrawImageUnscaled(Images.Capital, x - 15, y - 17);
							DrawCityName(g, city.Name, font, x - size.Width / 2, y - size.Height / 2);
							break;
					}
				}
			font.Dispose();
		}

		void DrawCityName(Graphics g, string name, Font font, float x, float y)
		{
			int i = (int) x;
			int j = (int) y;
			for (int dx=-1; dx<=1; dx++)
				for (int dy=-1; dy<=1; dy++)
					if (dx != 0 || dy != 0)
						g.DrawString(name, font, Brushes.White, i+dx, j+dy);
			g.DrawString(name, font, Brushes.Black, i, j);
		}

		void DrawTerrain(Graphics g)
		{
			int x, y;
			for (int i=0; i<gridW; i++)
				for (int j=0; j<gridH; j++)
					if (GetCoord(i, j, out x, out y))
					{
						switch(terrain[i, j])
						{
							case TerrainType.Inaccessible:
								break;
							case TerrainType.Sea:
								g.DrawImageUnscaled(Images.BlueDot, x - 1, y - 1);
								break;
							case TerrainType.Clear:
								g.DrawImageUnscaled(Images.BlackDot, x - 1, y - 1);
								break;
							case TerrainType.Mountain:
								g.DrawImageUnscaled(Images.Mountain, x - 3, y - 4);
								break;
							case TerrainType.Alpine:
								g.DrawImageUnscaled(Images.Alpine, x - 4, y - 5);
								break;
							case TerrainType.Port:
								g.DrawImageUnscaled(Images.Port, x - 5, y - 5);
								break;
						}
					}
		}

		void DrawMouse(Graphics g)
		{
//			if (seaMode)
//			{
//				if (roseVisible)
//					g.DrawImageUnscaled(Images.Rose, mouseEventX - 24, mouseEventY - 24);
//				return;
//			}
//			if (mouseX == -1) 
//				return;
//			g.DrawEllipse(Pens.Red, mouseX-5, mouseY-5, 10, 10);
		}

		void DrawSeas(Graphics g)
		{
			int x, y ;
			foreach (Point p in seas)
				if (GetCoord(p.X, p.Y, out x, out y))
					g.DrawImageUnscaled(Images.Rose, x - 24, y - 24, 48, 48);
		}

		void DrawRiver(Graphics g, ArrayList river)
		{
			if (river == null || river.Count < 2)
				return;
			PointF[] points = (PointF[]) river.ToArray(typeof(PointF));
			g.DrawLines(Pens.Blue, points);
		}

		void DrawRivers(Graphics g)
		{
			if (currentRiver.Count > 0)
			{
				DrawRiver(g, currentRiver);
				g.DrawLine(Pens.Red, riverHead, new Point(mouseEventX, mouseEventY));
			}
			foreach (River river in rivers)
				DrawRiver(g, river.Points);
		}

		void DrawTooltip(Graphics g)
		{
			if (tooltipText == null || tooltipText.Length == 0)
				return;

			using(Font font = new Font("Arial", 8.0f))
			using(Brush brush = new SolidBrush(Color.FromArgb(100, Color.White)))
			{
				g.FillRectangle(brush, tooltipRect);
				g.DrawString(tooltipText, font, Brushes.Black, tooltipRect.Left, tooltipRect.Top);
			}
		}

		void DrawMask(Graphics g)
		{
			if (!maskVisible)
				return;

			int x, y;
			using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.Green)))
			{
				for (int i=0; i<gridW; i++)
					for (int j=0; j<gridH; j++)
						if (mask[i, j])
							if (GetCoord(i, j, out x, out y))
								g.FillEllipse(brush, x - 6, y - 6, 13, 13);
			}
		}

		void DrawCauseways(Graphics g)
		{
			if (causewayVisible)
			{
				using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.Red)))
				{
					g.FillEllipse(brush, causeway1.X - 3, causeway1.Y - 3, 7, 7);
					g.FillEllipse(brush, causeway2.X - 3, causeway2.Y - 3, 7, 7);
					using (Pen pen = new Pen(brush, 3.0f))
					{
						g.DrawLine(pen, causeway1, causeway2);
					}
				}
			}

			int i, j, x1, y1, x2, y2;
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					for (int d=0; d<3; d++)
						if (GetAdjacent(x, y, d, out i, out j))
							if (causeway[x, y, d])
								if (GetCoord(x, y, out x1, out y1))
									if (GetCoord(i, j, out x2, out y2))
									{
										using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.Purple)))
										{
											g.FillEllipse(brush, x1 - 3, y1 - 3, 7, 7);
											g.FillEllipse(brush, x2 - 3, y2 - 3, 7, 7);
											using (Pen pen = new Pen(brush, 3.0f))
											{
												g.DrawLine(pen, x1, y1, x2, y2);
											}
										}
									}
		}

		private void mapPicture_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			using (Brush brush = new SolidBrush(Color.FromArgb(fade, Color.White)))
			{
				g.FillRectangle(brush, 0, 0, width, height);
			}
			DrawCities(g);
			DrawTerrain(g);
			DrawSeas(g);
			DrawMouse(g);
			DrawRivers(g);
			DrawMask(g);
			DrawCauseways(g);
			DrawTooltip(g);
		}

		public void SetFade(int fade)
		{
			if (fade < 0) fade = 0;
			if (fade > 255) fade = 255;
			this.fade = fade;
			this.Refresh();
			dirty = true;
		}

		public void Fade()
		{
			Push("fade");
			Fader fader = new Fader(this, fade);
			fader.ShowDialog();
			fader.Dispose();
		}

		int mouseX = -1;
		int mouseY = -1;
		int mouseEventX = -1;
		int mouseEventY = -1;
		int mouseGridX = -1;
		int mouseGridY = -1;

		void InvalidateMouse()
		{
			if (mouseX == -1) return;
			if (seaMode)
				mapPicture.Invalidate(new Rectangle(mouseEventX - 24, mouseEventY - 24, 48, 48));
			else
				mapPicture.Invalidate(new Rectangle(mouseX-5, mouseY-5, 11, 11));
		}

		Rectangle riverHeadRect = Rectangle.Empty;

		void InvalidateRiverHead()
		{
			mapPicture.Invalidate(riverHeadRect);
		}

		Rectangle tooltipRect = Rectangle.Empty;
		string tooltipText = string.Empty;

		void ClearTooltip()
		{
			mapPicture.Invalidate(tooltipRect);
			tooltipRect = Rectangle.Empty;
			tooltipText = string.Empty;
			this.Cursor = Cursors.Default;
		}

		void SetTooltip(string text)
		{
			Size size;
			mapPicture.Invalidate(tooltipRect);
			if (mapPicture.BackgroundImage == null)
				return;

			using(Graphics g = Graphics.FromImage(mapPicture.BackgroundImage))
			using(Font font = new Font("Arial", 8.0f))
			{
				size = Size.Round(g.MeasureString(text, font));
			}

			int x, y;
			int d = 8;
			if (mouseEventX > mapPicture.Width / 2)
				x = mouseEventX - d - size.Width;
			else
				x = mouseEventX + d;
			if (mouseEventY > mapPicture.Height / 2)
				y = mouseEventY - d - size.Height;
			else
				y = mouseEventY + d;

			tooltipRect = new Rectangle(new Point(x, y), size);
			tooltipText = text;
			mapPicture.Invalidate(tooltipRect);
			this.Cursor = Cursors.Cross;
		}

		bool MouseAtCity()
		{
			City city;
			return MouseAtCity(out city);
		}

		bool MouseAtCity(out City city)
		{
			city = null;
			if (mouseGridX == -1)
				return false;

			foreach (City c in this.Cities)
				if (mouseGridX == c.X && mouseGridY == c.Y)
				{
					city = c;
					return true;
				}

			return false;
		}

		bool MouseAtSea()
		{
			Point sea;
			return MouseAtSea(out sea);
		}

		bool MouseAtSea(out Point sea)
		{
			sea = Point.Empty;
			if (mouseGridX == -1)
				return false;

			foreach (Point s in this.seas)
				if (mouseGridX == s.X && mouseGridY == s.Y)
				{
					sea = s;
					return true;
				}

			return false;
		}

		bool MouseOnLand()
		{
			if (mouseGridX == -1)
				return false;

			TerrainType t = terrain[mouseGridX, mouseGridY];
			return t == TerrainType.Port || t == TerrainType.Clear || t == TerrainType.Mountain || t == TerrainType.Alpine;
		}

		void EditCity(City city)
		{
			EditCityForm form = new EditCityForm(city);
			if (form.ShowDialog() == DialogResult.No)
				if (Cities.Contains(city))
					Cities.Remove(city);
			form.Dispose();
			mapPicture.Invalidate();
		}

		void AddCity()
		{
			City city = new City(CityType.Town, mouseGridX, mouseGridY);
			EditCityForm form = new EditCityForm(city);
			if (form.ShowDialog() == DialogResult.OK)
			{
				Cities.Add(city);
				mapPicture.Invalidate();
			}
			form.Dispose();
		}

		void EditSea(Point sea)
		{
			if (seas.Contains(sea))
			{
				seas.Remove(sea);
				mapPicture.Invalidate();
			}
		}

		void AddSea()
		{
			seas.Add(new Point(mouseGridX, mouseGridY));
			mapPicture.Invalidate();
		}

		bool maskVisible = false;
		bool[,] mask = null;

		void DisableMask()
		{
			if (!maskVisible)
				return;

			maskVisible = false;
			mapPicture.Invalidate();
		}

		void EnableMask()
		{
			if (maskVisible)
				return;

			if (!Ctrl)
				return;

			if (activeTerrain == TerrainType.Port || activeTerrain == TerrainType.Sea)
				return;

			mask = new Boolean[gridW, gridH];
			Color water = ((Bitmap) mapPicture.BackgroundImage).GetPixel(mouseEventX, mouseEventY);
			int x, y;
			for (int i=0; i<gridW; i++)
				for (int j=0; j<gridH; j++)
					if (GetCoord(i, j, out x, out y))
					{
						Color pixel = ((Bitmap) mapPicture.BackgroundImage).GetPixel(x, y);
						int dr = water.R - pixel.R;
						int dg = water.G - pixel.G;
						int db = water.B - pixel.B;
						int dist = dr*dr + dg*dg + db*db;
						if (dist < 300)
						{
							mask[i, j] = true;
						}
					}

			maskVisible = true;
			mapPicture.Invalidate();
		}

		void DetermineTerrainTooltip()
		{
			DisableMask();
			if (Ctrl && activeTerrain != TerrainType.Sea && activeTerrain != TerrainType.Port)
			{
				switch(activeTerrain)
				{
					case TerrainType.Alpine:
						SetTooltip("Fill with alpine");
						break;
					case TerrainType.Clear:
						SetTooltip("Fill with plains");
						break;
					case TerrainType.Inaccessible:
						SetTooltip("Fill with inaccessible/water");
						break;
					case TerrainType.Mountain:
						SetTooltip("Fill with mountains");
						break;
				}
				EnableMask();
			}
			else if (mouseX == -1)
				ClearTooltip();
			else
			{
				switch(activeTerrain)
				{
					case TerrainType.Alpine:
						SetTooltip("Set to alpine");
						break;
					case TerrainType.Clear:
						SetTooltip("Set to plains");
						break;
					case TerrainType.Inaccessible:
						SetTooltip("Set to inaccessible/water");
						break;
					case TerrainType.Mountain:
						SetTooltip("Set to mountain");
						break;
					case TerrainType.Port:
						SetTooltip("Set to port");
						break;
					case TerrainType.Sea:
						SetTooltip("Set to ferry route");
						break;
				}
			}
		}

		Rectangle causewayRect = Rectangle.Empty;
		bool causewayVisible = false;
		Point causeway1, causeway2;

		void InvalidateCausewayRect()
		{
			mapPicture.Invalidate(causewayRect);
		}

		bool DetermineCausewayPlacement(out int d, out int i, out int j, out TerrainType t1, out TerrainType t2, out bool water1, out bool water2)
		{
			d = i = j = 0;
			t1 = t2 = TerrainType.Inaccessible;
			water1 = water2 = false;

			if (mouseX == -1)
				return false;

			int x, y;

			int min = int.MaxValue;
			int best = -1;
			for (d=0; d<6; d++)
				if (GetAdjacent(mouseGridX, mouseGridY, d, out i, out j))
					if (GetCoord(i, j, out x, out y))
					{
						int dist = (x - mouseEventX) * (x - mouseEventX) + (y - mouseEventY) * (y - mouseEventY);
						if (dist < min)
						{
							min = dist;
							best = d;
						}
					}

			d = best;
			if (best == -1)
				return false;

			GetAdjacent(mouseGridX, mouseGridY, best, out i, out j);

			t1 = terrain[mouseGridX, mouseGridY];
			t2 = terrain[i, j];

			water1 = t1 == TerrainType.Inaccessible || t1 == TerrainType.Sea;
			water2 = t2 == TerrainType.Inaccessible || t2 == TerrainType.Sea;

			return true;
		}

		void DetermineCausewayTooltip()
		{
			InvalidateCausewayRect();
			causewayRect = Rectangle.Empty;
			causewayVisible = false;

			int d, i, j;
			TerrainType t1, t2;
			bool water1, water2;

			if (!DetermineCausewayPlacement(out d, out i, out j, out t1, out t2, out water1, out water2))
			{
				ClearTooltip();
				return;
			}

			if (water1 || water2)
			{
				if (t1 == TerrainType.Sea || t1 == TerrainType.Port)
					SetTooltip("Remove ferry route");
				else if (t2 == TerrainType.Sea || t2 == TerrainType.Port)
					SetTooltip("Remove ferry route");
				else
					SetTooltip("Add ferry route");
			}
			else
			{
				if (causeway[mouseGridX, mouseGridY, d])
					SetTooltip("Remove causeway");
				else
					SetTooltip("Add causeway");
			}

			int x, y;
			GetCoord(mouseGridX, mouseGridY, out x, out y);
			causeway1 = new Point(x, y);
			GetCoord(i, j, out x, out y);
			causeway2 = new Point(x, y);
			causewayRect = Rectangle.FromLTRB(
				Math.Min(causeway1.X, causeway2.X), 
				Math.Min(causeway1.Y, causeway2.Y), 
				Math.Max(causeway1.X, causeway2.X), 
				Math.Max(causeway1.Y, causeway2.Y));
			causewayRect.Inflate(10, 10);
			causewayVisible = true;
			InvalidateCausewayRect();
		}

		void DetermineTooltip()
		{
			switch(editMode)
			{
				case EditMode.CityOrSea:
					if (mouseX == -1)
						ClearTooltip();
					else if (MouseAtCity())
						SetTooltip("Edit city");
					else if (MouseAtSea())
						SetTooltip("Remove sea marker");
					else if (MouseOnLand())
						SetTooltip("Add city");
					else 
						SetTooltip("Add sea marker");
					break;
				case EditMode.River:
					InvalidateRiverHead();
					if (currentRiver.Count == 0)
						SetTooltip("Add river");
					else
						SetTooltip("Continue (Enter=done, Esc=cancel)");
					float rx = Math.Min(mouseEventX, riverHead.X);
					float ry = Math.Min(mouseEventY, riverHead.Y);
					float rw = Math.Abs(mouseEventX - riverHead.X);
					float rh = Math.Abs(mouseEventY - riverHead.Y);
					RectangleF rr = new RectangleF(rx, ry, rw, rh);
					rr.Inflate(10, 10);
					riverHeadRect = Rectangle.Round(rr);
					InvalidateRiverHead();
					break;
				case EditMode.Terrain:
					DetermineTerrainTooltip();
					break;
				case EditMode.Causeway:
					DetermineCausewayTooltip();
					break;
			}
		}

		private void mapPicture_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int i, j, x=-1, y=-1;

			mouseEventX = e.X;
			mouseEventY = e.Y;
			mouseX = mouseY = mouseGridX = mouseGridY = -1;
			if (GetNearest(e.X, e.Y, out i, out j))
				if (GetCoord(i, j, out x, out y))
				{
					mouseX = x;
					mouseY = y;
					mouseGridX = i;
					mouseGridY = j;
				}

			DetermineTooltip();
		}

		private void mapPicture_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			City city;
			Point sea;
			int d, i, j;
			TerrainType t1, t2;
			bool water1, water2;

			if (e.Button == MouseButtons.Right)
			{
				if (editMode == EditMode.Terrain)
				{
					switch (activeTerrain)
					{
						case TerrainType.Alpine:
							activeTerrain = TerrainType.Port;
							break;
						case TerrainType.Mountain:
							activeTerrain = TerrainType.Alpine;
							break;
						case TerrainType.Clear:
							activeTerrain = TerrainType.Mountain;
							break;
						case TerrainType.Inaccessible:
							activeTerrain = TerrainType.Clear;
							break;
						case TerrainType.Sea:
							activeTerrain = TerrainType.Inaccessible;
							break;
						case TerrainType.Port:
							activeTerrain = TerrainType.Sea;
							break;
					}
					DetermineTerrainTooltip();
				}
			}
			if (e.Button == MouseButtons.Left)
			{
				switch(editMode)
				{
					case EditMode.CityOrSea:
						if (mouseX == -1)
							return;
						if (MouseAtCity(out city))
							EditCity(city);
						else if (MouseAtSea(out sea))
							EditSea(sea);
						else if (MouseOnLand())
							AddCity();
						else
							AddSea();
						dirty = true;
						break;
					case EditMode.River:
						InvalidateRiverHead();
						riverHead = new PointF(mouseEventX + Jiggle(), mouseEventY + Jiggle());
						currentRiver.Add(riverHead);
						riverHeadRect = new Rectangle(Point.Round(riverHead), Size.Empty);
						break;
					case EditMode.Terrain:
						if (Ctrl && activeTerrain != TerrainType.Port && activeTerrain != TerrainType.Sea)
						{
							for (int x=0; x<gridW; x++)
								for (int y=0; y<gridH; y++)
									if (mask[x, y])
										if (activeTerrain != TerrainType.Inaccessible || terrain[x, y] != TerrainType.Sea)
											terrain[x, y] = activeTerrain;
							mapPicture.Invalidate();
							dirty = true;
						}
						else if (mouseX != -1)
						{
							terrain[mouseGridX, mouseGridY] = activeTerrain;
							mapPicture.Invalidate(new Rectangle(mouseX - 5, mouseY - 5, 11, 11));
							dirty = true;
						}
						break;
					case EditMode.Causeway:
						if (DetermineCausewayPlacement(out d, out i, out j, out t1, out t2, out water1, out water2))
						{
							if (water1 || water2)
							{
								if (t1 == TerrainType.Sea || t1 == TerrainType.Port || t2 == TerrainType.Sea || t2 == TerrainType.Port)
								{
									terrain[mouseGridX, mouseGridY] = water1 ? TerrainType.Inaccessible : TerrainType.Clear;
									terrain[i, j] = water2 ? TerrainType.Inaccessible : TerrainType.Clear;
								}
								else
								{
									terrain[mouseGridX, mouseGridY] = water1 ? TerrainType.Sea : TerrainType.Port;
									terrain[i, j] = water2 ? TerrainType.Sea : TerrainType.Port;
								}
							}
							else
							{
								bool cw = !causeway[mouseGridX, mouseGridY, d];
								causeway[mouseGridX, mouseGridY, d] = cw;
								causeway[i, j, (d+3) % 6] = cw;
							}
							DetermineCausewayTooltip();
							dirty = true;
						}
						break;
				}
			}
		}

		private void mapPicture_Click(object sender, System.EventArgs e)
		{

			return;
/*
			if (riverMode != RiverMode.None)
			{
				switch(riverMode)
				{
					case RiverMode.Ready:
						currentRiver = new ArrayList();
						riverMode = RiverMode.Adding;
						goto case RiverMode.Adding;
					case RiverMode.Adding:
						InvalidateRiverHead();
						riverHead = new PointF(mouseEventX + Jiggle(), mouseEventY + Jiggle());
						currentRiver.Add(riverHead);
						break;
				}
				return;
			}

			if (samplingTerrain)
			{
				this.Cursor = Cursors.Default;
				samplingTerrain = false;
				Push("sample terrain");
				Color water = ((Bitmap) mapPicture.BackgroundImage).GetPixel(mouseEventX, mouseEventY);
				int x=-1, y=-1;
				for (int i=0; i<gridW; i++)
					for (int j=0; j<gridH; j++)
						if (GetCoord(i, j, out x, out y))
							if (terrain[i, j] != sampleTerrainType && (sampleTerrainType != TerrainType.Inaccessible || terrain[i, j] != TerrainType.Sea))
							{
								Color pixel = ((Bitmap) mapPicture.BackgroundImage).GetPixel(x, y);
								int dr = water.R - pixel.R;
								int dg = water.G - pixel.G;
								int db = water.B - pixel.B;
								int dist = dr*dr + dg*dg + db*db;
								if (dist < 300)
								{
									terrain[i, j] = sampleTerrainType;
								}
							}
				mapPicture.Refresh();
				dirty = true;
			}

			if (seaMode)
			{
				roseVisible = false;
				InvalidateMouse();
				seaMode = false;
				this.Cursor = Cursors.Default;
				int x, y;
				if (GetNearest(mouseEventX, mouseEventY, out x, out y))
				{
					Push("add sea");
					seas.Add(new Point(x, y));
					mapPicture.Refresh();
					dirty = true;
				}
				return;
			}
			
			if (mouseGridX != -1)
			{
				foreach (Point sea in seas)
				{
					if (mouseGridX == sea.X && mouseGridY == sea.Y)
					{
						Push("remove sea");
						seas.Remove(sea);
						dirty = true;
						mapPicture.Invalidate(new Rectangle(mouseX - 24, mouseY - 24, 48, 48));
						return;
					}
				}

				if (cityMode)
				{
					foreach (City city in Cities)
						if (mouseGridX == city.X && mouseGridY == city.Y)
						{
							EditCityForm form = new EditCityForm(city);
							switch(form.ShowDialog())
							{
								case DialogResult.OK:
									Push("edit city");
									mapPicture.Refresh();
									dirty = true;
									break;
								case DialogResult.No:
									Push("remove city");
									Cities.Remove(city);
									mapPicture.Refresh();
									dirty = true;
									break;
							}
							form.Dispose();
							return;
						}
					if (activeCityType != CityType.None)
					{
						City city = new City(activeCityType, mouseGridX, mouseGridY);
						EditCityForm form = new EditCityForm(city);
						if (form.ShowDialog() == DialogResult.OK)
						{
							Push("add city");
							Cities.Add(city);
							mapPicture.Refresh();
							dirty = true;
						}
						form.Dispose();
					}
				}
				else
				{
					if (terrain[mouseGridX, mouseGridY] != activeTerrain)
					{
						Push("terrain change");
						terrain[mouseGridX, mouseGridY] = activeTerrain;
					}
				}
				dirty = true;
				mapPicture.Invalidate(new Rectangle(mouseX - 15, mouseY - 17, 31, 35));
			}
			*/
		}

		void SetMousePosition(int x, int y, int i, int j)
		{
			if (x != mouseX || y != mouseY)
			{
				InvalidateMouse();
				mouseX = x;
				mouseY = y;
				mouseGridX = i;
				mouseGridY = j;
				InvalidateMouse();
			}
		}

		bool samplingTerrain = false;
		TerrainType sampleTerrainType = TerrainType.Inaccessible;

		void SampleTerrain(TerrainType t)
		{
			this.Cursor = Cursors.Cross;
			samplingTerrain = true;
			SetMousePosition(-1, -1, -1, -1);
			sampleTerrainType = t;
		}

		public void SampleWater()
		{
			SampleTerrain(TerrainType.Inaccessible);
		}

		public void SampleMountains()
		{
			SampleTerrain(TerrainType.Mountain);
		}

		public void SamplePlains()
		{
			SampleTerrain(TerrainType.Clear);
		}

		Random rand = new Random();

		float Jiggle()
		{
			return (float) (rand.NextDouble() * 0.2 - 0.1);
		}

		Stack undoStack = new Stack();

		void Push(string title)
		{
			StackFrame frame = new StackFrame();
			frame.Title = title;
			frame.Terrain = (TerrainType[,]) terrain.Clone();
			frame.Cities = new ArrayList();
			frame.Cities.AddRange(Cities);
			frame.Fade = fade;
			frame.Seas = new ArrayList();
			frame.Seas.AddRange(seas);
			undoStack.Push(frame);
		}

		public void Undo()
		{
			if (undoStack.Count == 0)
				return;
			StackFrame frame = (StackFrame) undoStack.Pop();
			terrain = (TerrainType[,]) frame.Terrain.Clone();
			Cities = frame.Cities;
			fade = frame.Fade;
			seas = frame.Seas;
			dirty = true;
			this.Refresh();
		}

		public static void Open(Form parent)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Rails map files (*.rlm)|*.rlm|All files (*.*)|*.*" ;
			dlg.Title = "Open Map";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					MapForm form = new MapForm(dlg.FileName);
					form.MdiParent = parent;
					form.Show();
				}
				catch(System.IO.IOException e)
				{
					MessageBox.Show("Unable to open. " + e.Message);
				}
			}
		}

		byte[] FileContents(string filename)
		{
			FileStream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			int L = (int) s.Length;
			byte[] temp = new Byte[L];
			s.Read(temp, 0, L);
			s.Close();
			return temp;
		}

		private void MapForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!dirty) return;
			switch(MessageBox.Show("Would you like to save changes to " + this.Text + "?", "Save Changes", MessageBoxButtons.YesNoCancel))
			{
				case DialogResult.Yes:
					e.Cancel = !Save();
					break;
				case DialogResult.No:
					break;
				case DialogResult.Cancel:
					e.Cancel = true;
					break;
			}
		}

		TerrainType activeTerrain = TerrainType.Clear;
		CityType activeCityType = CityType.None;
		bool cityMode = false;

		public void SetTerrain(TerrainType t)
		{
			activeTerrain = t;
			cityMode = false;
		}

		public void SetCityType(CityType c)
		{
			activeCityType = c;
			cityMode = true;
		}

		public void Dirty()
		{
			dirty = true;
		}

		bool seaMode = false;
		bool roseVisible = false;

		public void SetSea()
		{
			seaMode = true;
			this.Cursor = null;
			roseVisible = true;
			InvalidateMouse();
		}

		ArrayList currentRiver = new ArrayList();
		PointF riverHead = PointF.Empty;
		ArrayList rivers = new ArrayList();

		private void MapForm_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' && editMode == EditMode.River)
			{
				if (currentRiver.Count >= 2)
				{
					InvalidateRiverHead();
					string name = RiverForm.GetRiverName();
					if (name != null)
					{
						rivers.Add(new River(name, currentRiver));
						dirty = true;
					}
				}
				currentRiver = new ArrayList();
				SetTooltip("Add river");
			}
			if (e.KeyChar == '\x1B' && editMode == EditMode.River)
			{
				currentRiver = new ArrayList();
				SetTooltip("Add river");
				mapPicture.Invalidate();
			}
		}

		bool Ctrl = false;

		private void MapForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Ctrl = e.Control;
			if (editMode == EditMode.Terrain)
				DetermineTerrainTooltip();
		}

		private void MapForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Ctrl = e.Control;
			if (editMode == EditMode.Terrain)
				DetermineTerrainTooltip();
		}

		private void mapPicture_MouseLeave(object sender, System.EventArgs e)
		{
			ClearTooltip();
			causewayVisible = false;
			InvalidateCausewayRect();
		}

		public void SetEditMode(EditMode editMode)
		{
			this.editMode = editMode;
			DetermineTooltip();
		}

		public EditMode EditMode
		{
			get
			{
				return editMode;
			}
		}
	}

	class StackFrame
	{
		public string Title;
		public TerrainType[,] Terrain;
		public ArrayList Cities;
		public int Fade;
		public ArrayList Seas;
	}

	class River
	{
		public string Name;
		public ArrayList Points;

		public River(string name, ArrayList points)
		{
			Name = name;
			Points = points;
		}
	}
}
