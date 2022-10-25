using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace GraphicsGen
{
	public struct LineSeg
	{
		public PointF P;
		public PointF Q;

		public LineSeg(PointF p, PointF q)
		{
			P = p;
			Q = q;
		}
	}

	class Class1
	{
		static Color[] trackColor = {Color.Green, Color.Crimson, Color.DodgerBlue, Color.MediumOrchid, Color.FromArgb(255, 177, 0), Color.DarkGray};
		static Random rand = new Random();

		[STAThread]
		static void Main(string[] args)
		{
//			GenBlackDot();
//			GenBlueDot();
//			GenHill();
//			GenAlpine();
//			GenMajorCity();
//			GenCity();
//			GenTown();
//			GenPort();
////			GenIcons();
//			GenGlow();
//			GenTrains();
//			GenAppIcons();
			GenTrack();
//			GenSnow();
		}

		static void GenSnow()
		{
			int s = 320;
			for (int k=0; k<10; k++)
			using (Bitmap b = new Bitmap(s, s))
			using (Graphics g = Graphics.FromImage(b))
			using (Pen pen = new Pen(Brushes.White, 1))
			{
//				g.FillRectangle(Brushes.Black, 0, 0, s, s);
				ArrayList points = new ArrayList();
				double r = s/2*(Math.Sqrt(rand.NextDouble()));
				int ss = (int) (r / 20);
				for (int i=0; i<100; i++)
				{
					int x = (int) (r * rand.NextDouble());
					int y = (int) (Math.Pow(rand.NextDouble(), 3) * (r/2 - Math.Abs(r/2 - x)));
					points.Add(new Point(x, y));
				}
				g.TranslateTransform(s/2, s/2);
				g.RotateTransform(rand.Next(60));
				for (int i=0; i<6; i++)
				{
					g.RotateTransform(60);
					foreach (Point p in points)
					{
						g.FillEllipse(Brushes.White, p.X - ss/2, p.Y - ss/2, ss, ss);
						g.FillEllipse(Brushes.White, p.X - ss/2, -p.Y - ss/2, ss, ss);
					}
				}
				using (Bitmap c = Reduce3(b))
				{
					c.Save("Snowflake" + k + ".bmp");
				}
			}
		}

		static void GenBranches(ArrayList lines, double x, double y, double t, double r, Stack stack)
		{
			double x2 = x + r * Math.Cos(t);
			double y2 = y + r * Math.Sin(t);
			lines.Add(new LineSeg(new PointF((float) x, (float) y), new PointF((float) x2, (float) y2)));
			if (stack.Count == 0)
				return;
			r *= (double) stack.Pop();
			double dt = Math.PI / 3;
			GenBranches(lines, x2, y2, t - dt, r, (Stack) stack.Clone());
			GenBranches(lines, x2, y2, t, r, (Stack) stack.Clone());
			GenBranches(lines, x2, y2, t + dt, r, (Stack) stack.Clone());
		}

		static void ChooseColor()
		{
			int max = int.MinValue;
			Color best = Color.Transparent;
			ArrayList colors = new ArrayList(trackColor);
			colors.Add(Color.FromArgb(120,255,120));
			colors.Add(Color.White);
			colors.Add(Color.FromArgb(30,0,135));
			for (int red=0; red<18; red++)
				for (int grn=0; grn<18; grn++)
					for (int blu=0; blu<18; blu++)
					{
						int min = int.MaxValue;
						foreach (Color co in colors)
							{
								int dred = red*15 - co.R;
								int dgrn = grn*15 - co.G;
								int dblu = blu*15 - co.B;
								int dist = dred*dred + dgrn*dgrn + dblu*dblu;
								if (dist < min)
									min = dist;
							}
						if (min > max)
						{
							max = min;
							best = Color.FromArgb(red*15, grn*15, blu*15);
						}
					}
			MessageBox.Show(best.ToString());
			
		}

		static void Brighten(ref Color c)
		{
			c = Color.FromArgb((c.R+255)/2, (c.G+255)/2, (c.B+255)/2);
		}

		static void Darken(ref Color c)
		{
			c = Color.FromArgb(c.R*2/3, c.G*2/3, c.B*2/3);
		}

		static void GenTrack()
		{
			for (int p=0; p<6; p++)
			{
				for (int d=0; d<3; d++)
				{
					double t = Math.PI * (1.5 + d) / 3;
					Bitmap b = new Bitmap(22 * 16, 22 * 16);
					Graphics g = Graphics.FromImage(b);
					double ct = 16 * Math.Cos(t);
					double st = 16 * Math.Sin(t);
					for (int i=0; i<2; i++)
					{
						Color co = trackColor[p];
						int dx = 11 * 16;
						int dy = 11 * 16;
						if (i == 0)
						{
							co = Color.Black;
							dx += 16; dy += 16;
						}
						Pen pen = new Pen(co, 16.0f);
						double r1 = 9, r2 = 2.5, r3 = 3.5;
						g.DrawLine(pen, 
							(float) (dx + r1 * ct + r2 * st), 
							(float) (dy + r1 * st - r2 * ct), 
							(float) (dx - r1 * ct + r2 * st), 
							(float) (dy - r1 * st - r2 * ct));
						g.DrawLine(pen, 
							(float) (dx + r1 * ct - r2 * st), 
							(float) (dy + r1 * st + r2 * ct), 
							(float) (dx - r1 * ct - r2 * st), 
							(float) (dy - r1 * st + r2 * ct));
						double nties = 2.75;
						for (int ties = -2; ties <= +2; ties++)
						{
							g.DrawLine(pen, 
								(float) (dx + r1 * ct * ties/nties - r3 * st), 
								(float) (dy + r1 * st * ties/nties + r3 * ct), 
								(float) (dx + r1 * ct * ties/nties + r3 * st), 
								(float) (dy + r1 * st * ties/nties - r3 * ct));
						}
						pen.Dispose();
					}
					Bitmap c = Reduce3(b);
					c.Save(string.Format("Track{0}-{1}.bmp", p, d));
					c.Dispose();
					for (int i=0; i<2; i++)
					{
						Color co = trackColor[p];
						Brighten(ref co);
						int dx = 11 * 16;
						int dy = 11 * 16;
						if (i == 0)
						{
							co = Color.Black;
							dx += 16; dy += 16;
						}
						Pen pen = new Pen(co, 16.0f);
						double r1 = 9, r2 = 2.5, r3 = 4.0;
						g.DrawLine(pen, 
							(float) (dx + r1 * ct), 
							(float) (dy + r1 * st), 
							(float) (dx - r1 * ct), 
							(float) (dy - r1 * st));
						g.DrawLine(pen, 
							(float) (dx + r1 * ct + r3 * st), 
							(float) (dy + r1 * st - r3 * ct), 
							(float) (dx - r1 * ct + r3 * st), 
							(float) (dy - r1 * st - r3 * ct));
						g.DrawLine(pen, 
							(float) (dx + r1 * ct - r3 * st), 
							(float) (dy + r1 * st + r3 * ct), 
							(float) (dx - r1 * ct - r3 * st), 
							(float) (dy - r1 * st + r3 * ct));
						g.DrawLine(pen, 
							(float) (dx + r1 * ct + r2 * st), 
							(float) (dy + r1 * st - r2 * ct), 
							(float) (dx - r1 * ct + r2 * st), 
							(float) (dy - r1 * st - r2 * ct));
						g.DrawLine(pen, 
							(float) (dx + r1 * ct - r2 * st), 
							(float) (dy + r1 * st + r2 * ct), 
							(float) (dx - r1 * ct - r2 * st), 
							(float) (dy - r1 * st + r2 * ct));
						double nties = 2.75;
						for (int ties = -2; ties <= +2; ties++)
						{
							g.DrawLine(pen, 
								(float) (dx + r1 * ct * ties/nties - r3 * st), 
								(float) (dy + r1 * st * ties/nties + r3 * ct), 
								(float) (dx + r1 * ct * ties/nties + r3 * st), 
								(float) (dy + r1 * st * ties/nties - r3 * ct));
						}
						pen.Dispose();
					}
					c = Reduce3(b);
					c.Save(string.Format("Bridge{0}-{1}.bmp", p, d));
					c.Dispose();
					g.Dispose();
					b.Dispose();
				}
			}
		}

		static void GenAppIcons()
		{
			GenAppIcon(16);
			GenAppIcon(32);
			GenAppIcon(48);
			GenAppIcon(64);
			GenAppIcon(96);
		}

		static void GenAppIcon(int size)
		{
			int h = size / 2;
			int r = (int) (size / Math.Sqrt(2));
			int q = 16;
			PointF[] p = new PointF[q];
			Bitmap b = new Bitmap(h * 32, h * 32);
			double a = Math.PI / 4;
			double R = 0;
			for (int i=0; i<q;  i++)
			{
				double t = Math.PI * 2 * i / q + a;
				double ct = Math.Cos(t);
				double st = Math.Sin(t);
				R = i == 0 ? r : r / 2;
				p[i] = new PointF(16 * (float) (h + R * ct), 16 * (float) (h - R * st));
			}
			Graphics g = Graphics.FromImage(b);
			g.FillPolygon(Brushes.Black, p);
			R = r * 0.3;
			g.FillEllipse(Brushes.Green, 16 * (float) (h - R), 16 * (float) (h - R), 16 * (float) (2*R), 16 * (float)(2*R));
			g.Dispose();
			Bitmap c = Reduce3(b);
			b.Dispose();
			c.Save("AppIcon" + size.ToString() + ".bmp");
		}

		static void GenTrains()
		{
			int r = 10;
			int q = 16;
			PointF[] p = new PointF[q];
			for (int d=0; d<6; d++)
			{
				Bitmap b = new Bitmap(r * 32, r * 32);
				double a = Math.PI / 2 - Math.PI / 3 * d;
				for (int i=0; i<q;  i++)
				{
					double t = Math.PI * 2 * i / q + a;
					double ct = Math.Cos(t);
					double st = Math.Sin(t);
					double R = i == 0 ? r : r / 2;
					p[i] = new PointF(16 * (float) (r + R * ct), 16 * (float) (r - R * st));
				}
				Graphics g = Graphics.FromImage(b);
				g.FillPolygon(Brushes.Black, p);
				g.Dispose();
				Bitmap c = Reduce(b, Color.Black);
				b.Dispose();
				c.Save("Train" + d.ToString() + ".bmp");

			}
			for (int i=0; i<6; i++)
			{
				Bitmap b = new Bitmap(6 * 16, 6 * 16);
				Graphics g = Graphics.FromImage(b);
				g.FillRectangle(Brushes.Black, 0, 0, b.Width, b.Height);
				Brush br = new SolidBrush(trackColor[i]);
				g.FillEllipse(br, 0, 0, b.Width, b.Height);
				br.Dispose();
				g.Dispose();
				Bitmap c = Reduce2(b, Color.Black);
				c.Save("PlayerDot" + i.ToString() + ".bmp");
				b.Dispose();
			}
		}

		static void GenGlow()
		{
			int size = 100;
			double h = (size - 1) / 2.0;
			Bitmap bmp = new Bitmap(size, size);
			for (int x=0; x<size; x++)
				for (int y=0; y<size; y++)
				{
					double dx = x - h;
					double dy = y - h;
					double r = Math.Sqrt(dx * dx + dy * dy) / h;
					if (r > 1) r = 1;
					r = Math.Pow(r, 0.2);
					if (r < 0.5) r = 0.5;
					int alpha = (int) (255 * (1 - r));
					bmp.SetPixel(x, y, Color.FromArgb(alpha, Color.Blue));
				}
			bmp.Save("Glow.bmp");
			bmp.Dispose();
		}

#if OLD_ICONS
		static void GenIcons()
		{
			string icons = "..\\..\\..\\icons\\";
			string[] names = Directory.GetFiles(icons + "opaque", "*.bmp");
			foreach (string name in names)
			{
				Bitmap bmp32 = new Bitmap(48, 48, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Bitmap bmp = new Bitmap(name);
				bool[,] fg = new Boolean[48, 48];
				int t = 230;
				for (int x=0; x<48; x++)
					for (int y=0; y<48; y++)
					{
						Color co = bmp.GetPixel(x, y);
						fg[x,y] = !(co.R >= t && co.G >= t && co.B >= t);
					}
				for (int x=0; x<48; x++)
					for (int y=0; y<48; y++)
						if (fg[x, y])
							bmp32.SetPixel(x, y, bmp.GetPixel(x, y));
						else
						{
							double min = double.MaxValue;
							for (int i=0; i<48; i++)
								for (int j=0; j<48; j++)
									if (fg[i, j])
									{
										int dx = i - x;
										int dy = j - y;
										double dist = Math.Sqrt(dx*dx + dy*dy);
										if (dist<min)
											min = dist;
									}
							int alpha = 255 - (int) (60 * (min - 1));
							if (alpha < 0) alpha = 0;
							bmp32.SetPixel(x, y, Color.FromArgb(alpha, Color.White));
						}
				bmp32.Save(icons + Path.GetFileName(name));
				bmp32.Dispose();
				bmp.Dispose();
			}
		}
#endif

		static void GenBlackDot()
		{
			int s = 3;
			Bitmap b = new Bitmap(s * 16, s * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			g.FillEllipse(Brushes.Black, 8, 8, b.Width - 16, b.Height - 16);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Black);
			c.Save("BlackDot.bmp");
		}

		static void GenBlueDot()
		{
			int s = 3;
			Bitmap b = new Bitmap(s * 16, s * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.LightBlue, 0, 0, b.Width, b.Height);
			g.FillEllipse(Brushes.Blue, 8, 8, b.Width - 16, b.Height - 16);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Blue);
			c.Save("BlueDot.bmp");
		}

		static void GenHill()
		{
			Bitmap b = new Bitmap(6 * 16, 7 * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			Point[] points = new Point[3];
			points[0] = new Point(0, 7 * 16);
			points[1] = new Point(6 * 16, 7 * 16);
			points[2] = new Point(3 * 16, 0);
			g.FillPolygon(Brushes.Black, points);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Black);
			c.Save("Hill.bmp");
		}

		static void GenAlpine()
		{
			Bitmap b = new Bitmap(8 * 16, 9 * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			Point[] points = new Point[3];
			points[0] = new Point(0, 9 * 16);
			points[1] = new Point(8 * 16, 9 * 16);
			points[2] = new Point(4 * 16, 0);
			g.FillPolygon(Brushes.Black, points);
			points[0] = new Point(2 * 16, 7 * 16 + 8);
			points[1] = new Point(6 * 16, 7 * 16 + 8);
			points[2] = new Point(4 * 16, 2 * 16);
			g.FillPolygon(Brushes.White, points);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Black);
			c.Save("Alpine.bmp");
		}

		static void GenMajorCity()
		{
			Bitmap b = new Bitmap(31 * 16, 35 * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			Point[] points = new Point[6];
			points[0] = new Point(16 * 16, 0);
			points[1] = new Point(31 * 16, 8 * 16);
			points[2] = new Point(31 * 16, 26 * 16);
			points[3] = new Point(16 * 16, 34 * 16);
			points[4] = new Point(0, 26 * 16);
			points[5] = new Point(0, 8 * 16);
			g.FillPolygon(Brushes.Red, points);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Red);
			c.Save("Capital.bmp");
		}

		static void GenCity()
		{
			Bitmap b = new Bitmap(15, 15);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.Red, 0, 0, b.Width, b.Height);
			g.Dispose();

			b.Save("City.bmp");
		}

		static void GenTown()
		{
			Bitmap b = new Bitmap(15 * 16, 15 * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			g.FillEllipse(Brushes.Red, 8, 8, b.Width - 16, b.Height - 16);
			g.Dispose();

			Bitmap c = Reduce(b, Color.Red);
			c.Save("Town.bmp");
		}

		static void GenPort()
		{
			Bitmap b = new Bitmap(11 * 16, 11 * 16);
			Graphics g = Graphics.FromImage(b);
			g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
			g.FillEllipse(Brushes.LightGray, 8, 8, b.Width - 16, b.Height - 16);
			g.DrawEllipse(new Pen(Brushes.Black, 16.0f), 8, 8, b.Width - 16, b.Height - 16);
			Font font = new Font("Arial", 90.0f);
			g.DrawString("P", font, Brushes.Black, 2.0f * 16, 1.5f * 16);
			g.Dispose();

			Bitmap c = Reduce2(b, Color.White);
			c.Save("Port.bmp");
		}

		static Bitmap Reduce(Bitmap b, Color fg)
		{
			Bitmap c = new Bitmap(b.Width / 16, b.Height / 16);
			for (int x = 0; x < c.Width; x++)
				for (int y = 0; y < c.Height; y++)
				{
					int alp = 0;
					for (int i = 0; i < 16; i++)
						for (int j = 0; j < 16; j++)
						{
							Color co = b.GetPixel(x * 16 + i, y * 16 + j);
							if (co.R == fg.R && co.G == fg.G && co.B == fg.B && co.A == fg.A)
								alp++;
						}
					if (alp == 256)
						alp = 255;
					c.SetPixel(x, y, Color.FromArgb(alp, fg));
				}
			return c;
		}

		static Bitmap Reduce2(Bitmap b, Color bg)
		{
			Bitmap c = new Bitmap(b.Width / 16, b.Height / 16);
			for (int x = 0; x < c.Width; x++)
				for (int y = 0; y < c.Height; y++)
				{
					int alp = 0, red = 0, grn = 0, blu = 0;
					for (int i = 0; i < 16; i++)
						for (int j = 0; j < 16; j++)
						{
							Color co = b.GetPixel(x * 16 + i, y * 16 + j);
							red += co.R;
							grn += co.G;
							blu += co.B;
							if (co.R != bg.R || co.G != bg.G || co.B != bg.B || co.A != bg.A)
								alp++;
						}
					red /= 256;
					grn /= 256;
					blu /= 256;
					if (alp == 256)
						alp = 255;
					c.SetPixel(x, y, Color.FromArgb(alp, red, grn, blu));
				}
			return c;
		}

		static Bitmap Reduce3(Bitmap b)
		{
			Bitmap c = new Bitmap(b.Width / 16, b.Height / 16);
			int transparent = Color.Transparent.ToArgb();
			for (int x = 0; x < c.Width; x++)
				for (int y = 0; y < c.Height; y++)
				{
					int weight = 0, red = 0, grn = 0, blu = 0;
					for (int i = 0; i < 16; i++)
						for (int j = 0; j < 16; j++)
						{
							Color co = b.GetPixel(x * 16 + i, y * 16 + j);
							weight += co.A;
							red += co.R * co.A;
							grn += co.G * co.A;
							blu += co.B * co.A;
						}
					if (weight > 0)
					{
						red /= weight;
						grn /= weight;
						blu /= weight;
						int alp = weight / 256;
						c.SetPixel(x, y, Color.FromArgb(alp, red, grn, blu));
					}
				}
			return c;
		}
	}
}
