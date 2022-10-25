
// RandomMap2.cs

/*
 * This class represents a randomly-generated map. The map can be recreated exactly by providing 
 * the same seed string. Only certain strings are allowed because the map algorithm version number
 * is embedded in the seed, and only specific version numbers are recognized.
 * 
 * This class differs from the RandomMap class in that it generates more realistic-looking
 * terrain and attempts to simulate a satellite photo of the map. It also generates better
 * mountain ranges.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Rails
{
	struct RGB
	{
		public byte Red, Grn, Blu;

		public RGB(int red, int grn, int blu)
		{
			Red = (byte) red;
			Grn = (byte) grn;
			Blu = (byte) blu;
		}

		public Color Color
		{
			get
			{
				return Color.FromArgb(Red, Grn, Blu);
			}
		}
	}

	public class RandomMap2 : Map
	{
		const int maxVersion = 2;
		/*
		 * Version history:
		 * 
		 * 0	Initial version
		 * 
		 * 1	Valid port sites require only one or more adjacent seas
		 *		instead of exactly 4
		 * 
		 * 2	Added nationalized track to ensure three players can reach
		 *		each non-capital city and two players can reach town
		 * 
		 */

		public string Name;
		int version;
		Random rand;

		Color waterColor;
		Brush waterBrush;

		// delegates for various path-finding algorithms
		FloodMethod terrainCostMethod;
		FloodMethod portsMethod;

		// create a new map with a random seed
		public RandomMap2(Size imageSize) : base(imageSize)
		{
			CreateRandomMap();
		}

		void CreateRandomMap()
		{
			int seed;

			RandomNumberGenerator rng = new RNGCryptoServiceProvider();
			byte[] s = new byte[4];
			rng.GetBytes(s);
			Random r = new Random(s[0] + 256 * (s[1] + 256 * (s[2] + 256 * s[3])));

#if DEBUG
			foreach (string filename in Directory.GetFiles(".", "rejects*.bmp"))
				File.Delete(filename);
			int rejects = 0;
#endif 

			while(true)
			{
				Name = ChooseName(r);
				GetVersionAndSeed(Name, out version, out seed);
				if (version == maxVersion)
				{
					rand = new Random(seed);
					CreateMap();
					if (IsViableInternal())
						break;
#if DEBUG
					using (Graphics g = Graphics.FromImage(Background))
					{
						g.DrawImageUnscaled(base.Foreground, 0, 0);
					}
					Background.Save("rejects" + (rejects++).ToString() + ".bmp");
#endif
					this.Dispose();
				}
			}
		}

		// create a new map with a given seed
		public RandomMap2(Size imageSize, string name) : base(imageSize)
		{
			int seed;

			Name = name;
			GetVersionAndSeed(name, out version, out seed);
			if (version > maxVersion)
			{
				string message = Resource.GetString("RandomMap2.UnsupportedMapName");
				MessageBox.Show(message, Resource.GetString("Rails"), MessageBoxButtons.OK);
				CreateRandomMap();
				return;
			}

			rand = new Random(seed);
			CreateMap();
			IsViableInternal();	// we already know it's viable, but we need to nationalize tracks
		}

		public new void Dispose()
		{
			if (waterBrush != null)
				waterBrush.Dispose();
			base.Dispose();
		}

		void GetVersionAndSeed(string name, out int version, out int seed)
		{
			HashAlgorithm hashAlg = MD5.Create();
			Encoding enc = Encoding.UTF8;
			byte[] hash = hashAlg.ComputeHash(enc.GetBytes(name.ToLower(System.Globalization.CultureInfo.InvariantCulture)));
			version = hash[0];
			seed = hash[1] + 256 * (hash[2] + 256 * (hash[3] + 256 * hash[4]));
		}

		// choose a random name
		string ChooseName(Random r)
		{
			return NamePart(r) + "-" + NamePart(r);
		}

		Hashtable seenPart = new Hashtable();

		string NamePart(Random r)
		{
			while(true)
			{
				string part = City.RandomName(2, r, seenPart);
				if (part.Length >= 12)
					return part;
			}
		}

		// Save the map to disk. Since all we need is the seed to recreate the map, we
		// save a lot of space by just storing the seed and a thumbnail image. The seed
		// is stored in the image filename.
		public override void Save()
		{
			try
			{
				Directory.CreateDirectory("maps");
				string filename = "maps\\" + Name + ".ter";
				
				int tw = 128;
				int th = tw * ImageSize.Height / ImageSize.Width;
				using (Bitmap thumb = new Bitmap(tw, th))
				{
					using (Image bg = this.Background.GetThumbnailImage(tw, th, null, IntPtr.Zero))
					{
						using (Graphics g = Graphics.FromImage(thumb))
						{
							g.DrawImage(bg, 0, 0, tw, th);
							thumb.Save(filename);
						}
					}
				}
			}
			catch(IOException)
			{
				System.Windows.Forms.MessageBox.Show("An error occurred while trying to save the map.");
			}
		}

		void GenerateMilepostGrid(ZField field, int seaLevel, int hillLevel, int alpineLevel)
		{
			int xx, yy;
			milepost = new Milepost[gridW, gridH];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH;  y++)
				{
					if (GetCoord(x, y, out xx, out yy))
					{
						TerrainType t = TerrainType.Inaccessible;
						if (field[xx, yy] < seaLevel)
							t = TerrainType.Sea;
						else if (xx > 0 && field[xx-1, yy] < seaLevel)
							t = TerrainType.Sea;
						else if (xx < width - 1 && field[xx+1, yy] < seaLevel)
							t = TerrainType.Sea;
						else if (yy > 0 && field[xx, yy-1] < seaLevel)
							t = TerrainType.Sea;
						else if (yy < height - 1 & field[xx, yy+1] < seaLevel)
							t = TerrainType.Sea;
						else if (field[xx, yy] >= hillLevel)
							t = rand.Next(3) == 0 ? TerrainType.Mountain : TerrainType.Clear;
						else
							t = TerrainType.Clear;
						milepost[x, y].Terrain = t;
						milepost[x, y].CityIndex = -1;
						milepost[x, y].SeaIndex = -1;
					}
				}
			int i, j;
			for (int y=0; y<height; y++)
				for (int x=0; x<width; x++)
					if (field[x, y] >= alpineLevel)
						if (base.GetNearest(x, y, out i, out j))
							milepost[i, j].Terrain = TerrainType.Alpine;
			for (int y=0; y<gridH; y++)
				for (int x=0; x<gridW; x++)
					if (milepost[x, y].Terrain == TerrainType.Alpine)
						if (rand.Next(3) > 0)
							milepost[x, y].Terrain = TerrainType.Mountain;
		}

		void AnalyzeLandAndSeas()
		{
			// identify land masses separated by water
			ResetFloodMap();
			massIndex = 0;
			Flood(0, 0, new FloodMethod(LandMassFloodMethod));

			// count the land masses and seas and measure their size
			Hashtable masses = new Hashtable(); // maps land/sea ID's to their size
			Hashtable massId = new Hashtable(); // maps land/sea ID's to a contiguous value 0..n
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					massIndex = milepost[x, y].Value;
					if (!masses.ContainsKey(massIndex))
					{
						massId[massIndex] = masses.Count;
						masses[massIndex] = 1;
					}
					else
						masses[massIndex] = (int) masses[massIndex] + 1;
				}

			// determine which landmasses are adjacent to which seas
			int massCount = masses.Count;
			ulong[] adjacent = new ulong[massCount];
			int[] massIdx = new int[massCount]; // maps contiguous value 0..n to land/sea ID
			Point[] massLocation = new Point[massCount];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					int i, j;
					for (int d=0; d<6; d++)
						if (GetAdjacent(x, y, d, out i, out j))
							if (milepost[x, y].Value != milepost[i, j].Value)
							{
								int v1 = milepost[x, y].Value;
								int v2 = milepost[i, j].Value;
								int m1 = (int) massId[v1];
								int m2 = (int) massId[v2];
								adjacent[m1] |= 1UL << m2;
								massIdx[m1] = v1;
								massIdx[m2] = v2;
							}
					massLocation[(int) massId[milepost[x, y].Value]] = new Point(x, y);
				}

			// keep track of which port and sea mileposts we have found to be useful
			bool[,] useful = new bool[gridW, gridH];

			// count the number of large land masses to see if ports are necessary
			const int criticalMass = 13;	// minimum size of each land mass
			int ncontinents = 0;
			for (int i=0; i<massCount; i++)
				if (!IsSea(massLocation[i].X, massLocation[i].Y))
					if ((int) masses[milepost[massLocation[i].X, massLocation[i].Y].Value] >= criticalMass)
						ncontinents++;

			// create ports if more than one large land mass
			if (ncontinents > 1)
			{
				// create mapping of mass ID's to continent ID's
				int[] continent = new int[massCount];
				ncontinents = 0;
				for (int i=0; i<massCount; i++)
				{
					int mx = massLocation[i].X;
					int my = massLocation[i].Y;
					if (!IsSea(mx, my))
					{
						massIndex = milepost[mx, my].Value;
						if ((int) masses[massIndex] >= criticalMass)
							continent[(int) massId[massIndex]] = ncontinents++;
					}
				}

				// catalog all of the viable port locations
				int nports = 0;
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if ((int) masses[milepost[x, y].Value] >= criticalMass)
							if (this.IsValidPortSite(x, y))
								nports++;

				Point[] portLocation = new Point[nports];
				int[] portIndex = new int[nports];

				nports = 0;
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if ((int) masses[milepost[x, y].Value] >= criticalMass)
							if (this.IsValidPortSite(x, y))
							{
								milepost[x, y].Terrain = TerrainType.Port;
								portLocation[nports] = new Point(x, y);
								portIndex[nports] = continent[(int) massId[milepost[x, y].Value]];
								nports++;
							}

				// use ports to connect each continent to nearest neighboring continent
				bool[] connected = new bool[ncontinents];
				connected[0] = true;

				for (int connectedCount=1; connectedCount<ncontinents; connectedCount++)
				{
					// calculate sea travel times from currently connected ports
					this.ResetFloodMap();
					for (int port=0; port<nports; port++)
						if (connected[portIndex[port]])
							milepost[portLocation[port].X, portLocation[port].Y].Value = 0;
					this.Flood(portsMethod);

					// find closest non-connected port
					int min = int.MaxValue;
					int closest = -1;
					int px, py;
					for (int port=0; port<nports; port++)
						if (!connected[portIndex[port]])
						{
							px = portLocation[port].X;
							py = portLocation[port].Y;
							if (milepost[px, py].Value < min)
							{
								min = milepost[px, py].Value;
								closest = port;
							}
						}

					if (closest == -1)
						break;

					// mark the closest port's continent as connected
					connected[portIndex[closest]] = true;

					// make note of useful port connection
					Point[] pts = new Point[min + 2];
					int pt = 0;
					px = portLocation[closest].X;
					py = portLocation[closest].Y;
					while (true)
					{
						// get dot location for drawing later
						int ptx, pty;
						base.GetCoord(px, py, out ptx, out pty);
						pts[pt++] = new Point(ptx, pty);

						useful[px, py] = true;
						if (milepost[px, py].Value == 0)
							break;

						// trace path back to starting port
						if (!base.GetAdjacent(px, py, (milepost[px, py].Gradient + 3) % 6, out px, out py))
							break;
					}

					if (pt > 1)	// should always happen
					{

						// create bezier end points and control points
						Point[] bez = new Point[3 * pt - 2];
						int bi = 0;
						bez[bi++] = pts[0];
						Point prev = new Point(2*pts[0].X - pts[1].X, 2*pts[0].Y - pts[1].Y);
						Point next;
						for (int i=1; i<pt; i++)
						{
							if (i < pt - 1)
								next = pts[i + 1];
							else
								next = new Point(2*pts[pt-1].X - pts[pt-2].X, 2*pts[pt-1].Y - pts[pt-2].Y);
							bez[bi++] = new Point(pts[i-1].X + (pts[i-1].X - prev.X)/3, pts[i-1].Y + (pts[i-1].Y - prev.Y)/3);
							bez[bi++] = new Point(pts[i].X + (pts[i].X - next.X)/3, pts[i].Y + (pts[i].Y - next.Y)/3);
							bez[bi++] = pts[i];
							prev = pts[i-1];
						}

						// draw beziers
						using (Graphics g = Graphics.FromImage(Background))
						{
							using (Pen p = new Pen(Color.FromArgb(75, Color.Blue), 5))
							{
								g.DrawBeziers(p, bez);
							}
						}
					}
				}
			}

			// record locations of large seas
			int nseas = 0;
			for (int i=0; i<massCount; i++)
				if ((int) masses[massIdx[i]] > 50)
					if (IsSea(massLocation[i].X, massLocation[i].Y))
						nseas++;
			Seas = new Point[nseas];
			nseas = 0;
			for (int i=0; i<massCount; i++)
				if ((int) masses[massIdx[i]] > 50)
					if (IsSea(massLocation[i].X, massLocation[i].Y))
					{
						Seas[nseas] = massLocation[i];
						nseas++;
					}

			// initialize sea disasters before removing unviable sea travel mileposts
			this.InitializeSeaDisasters();

			// remove unusable port and sea mileposts
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (!useful[x, y])
					{
						if (milepost[x, y].Terrain == TerrainType.Port)
							milepost[x, y].Terrain = TerrainType.Clear;
						else if (milepost[x, y].Terrain == TerrainType.Sea)
							milepost[x, y].Terrain = TerrainType.Inaccessible;
					}
		}

		void GenerateCities()
		{
			// loop until good city arrangement is found, usually only once
			while(true)
			{
				// reset city arrays and travel-distance data
				Cities = new City[this.CityCount];

				// initial test function is for major cities
				IsValidSite isValidSite = new IsValidSite(IsValidCapitalSite);

				// choose random spot for initial major city
				int cx, cy;
				while (true)
				{
					cx = rand.Next(gridW);
					cy = rand.Next(gridH);
					if (isValidSite(cx, cy))
						break;
				}

				// clear the travel-distance data
				ResetFloodMap();

				// place all the cities and towns
				Hashtable usedNames = new Hashtable();
				for (int i=0; i<this.CityCount; i++)
				{
					// store the city location
					milepost[cx, cy].CityIndex = i;

					string name;
					if (i < this.NumCapitals)
					{
						name = City.RandomName(i, rand, usedNames);
						milepost[cx, cy].CityType = CityType.Capital;

						int dx, dy;
						for (int d=0; d<6; d++)
							if (GetAdjacent(cx, cy, d, out dx, out dy))
							{
								milepost[dx, dy].CityType = CityType.CapitalCorner;
								milepost[dx, dy].CityIndex = i;
							}
					}
					else if (i < this.NumCapitals + this.NumCities)
					{
						name = City.RandomName(milepost[cx, cy].Capital, rand, usedNames);
						milepost[cx, cy].CityType = CityType.City;
					}
					else
					{
						name = City.RandomName(milepost[cx, cy].Capital, rand, usedNames);
						milepost[cx, cy].CityType = CityType.Town;
					}

					// after placing the major cities, switch to the city/town test function
					if (i == this.NumCapitals)
						isValidSite = new IsValidSite(IsValidCitySite);
					else if (i == this.NumCapitals + this.NumCities)
						isValidSite = new IsValidSite(IsValidTownSite);

					Cities[i] = new City(cx, cy, name, new ArrayList(2));

					// update travel-distance map to find furthest-distance site for next city
					Flood(cx, cy, terrainCostMethod, isValidSite, i < this.NumCapitals ? i : -1, out cx, out cy);
				}
				usedNames = null;

				// make sure all cities are accessible
				bool allAccessible = true;
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if (milepost[x, y].Value == int.MaxValue)
							if (milepost[x, y].CityType != CityType.None)
								allAccessible = false;

				if (allAccessible)
					break;
			}
		}

		/*
		void GenerateSeaLanes()
		{
			// for each remaining port, map out the sea lanes to other ports
			bool[,] usefulSea = new bool[gridW, gridH];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (milepost[x, y].Terrain == TerrainType.Port)
					{
						// get the physical location of the port
						int x1, y1;
						GetCoord(x, y, out x1, out y1);

						// calculate distances to all other ports on same body of water
						int mx, my;
						ResetFloodMap();
						Flood(x, y, portsMethod, null, out mx, out my);

						// trace out the route back to this port from each other accessible port
						for (int rx=0; rx<gridW; rx++)
							for (int ry=0; ry<gridH; ry++)
								if (milepost[rx, ry].Terrain == TerrainType.Port)
									if (milepost[rx, ry].Value != int.MaxValue)
									{
										// follow distance map back to distance 0 (origin)
										int px = rx, py = ry;
										while (milepost[px, py].Value > 0)
										{
											// find the best direction to take
											int bestDir = -1;
											int qx, qy;
											int min = int.MaxValue;
											int minDist = int.MaxValue;
											for (int d=0; d<6; d++)
												if (GetAdjacent(px, py, d, out qx, out qy))
												{
													// get the physical location of the 2nd port
													int x2, y2;
													GetCoord(qx, qy, out x2, out y2);

													// calculate the straight-line distance
													int dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);

													// check if this is the best direction tried so far
													int qvalue = milepost[qx, qy].Value;
													if (qvalue < min || (qvalue == min && dist < minDist))
													{
														min = qvalue;
														minDist = dist;
														bestDir = d;
													}
												}

											// move toward origin port and mark the sea-lane
											GetAdjacent(px, py, bestDir, out px, out py);
											usefulSea[px, py] = true;
										}
									}
					}

			// before we remove useless sea mileposts, calculate sea disaster areas
			base.InitializeSeaDisasters();

			// remove sea mileposts that aren't in sea lanes
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (milepost[x, y].Terrain == TerrainType.Sea && !usefulSea[x, y])
						milepost[x, y].Terrain = TerrainType.Inaccessible;
		}
		*/

		void GenerateRivers(ZField field, int seaLevel, Graphics g, bool[,] isWater)
		{
			// create rivers
			int rivers = 0;
			this.Rivers = new ArrayList();
			Hashtable riverNames = new Hashtable();
			while ((rivers < 15 || this.Rivers.Count < 3) && this.Rivers.Count < 7)
			{
				// pick a starting point near a city
				int x, y;
				City c = Cities[this.NumCapitals + (rivers % (this.CityCount - this.NumCapitals))];
				while (true)
				{
					GetCoord(c.X, c.Y, out x, out y);
					double a = Math.PI * 2 * rand.NextDouble();
					x += (int) (50 * Math.Cos(a));
					y += (int) (50 * Math.Sin(a));
					if (x >= 0 && x < width && y >= 0 && y < height)
						break;
				}

				// find the approximate nearest water
				int x2 = -1, y2 = -1;
				int min = int.MaxValue;
				for (int p=0; p<200; p++)
				{
					int i = rand.Next(width);
					int j = rand.Next(height);
					if (field[i, j] < seaLevel)
					{
						int dist = (i-x)*(i-x) + (j-y)*(j-y);
						if (dist < min)
						{
							min = dist;
							x2 = i;
							y2 = j;
						}
					}
				}

				// calculate a fractal line between the two points
				ArrayList riverPoints = DrawFractalLine(g, waterBrush, x, y, x2, y2, 0.0, 1.0, isWater, field, seaLevel);
				if (riverPoints.Count > 1)
				{
					// simplify the line into a manageable number of line segments
					ArrayList riverLine = SimplifyPolyLine(riverPoints);

					// throw out rivers that are too short
					if (riverLine.Count > 30)
					{
						PointF[] points = (PointF[]) riverLine.ToArray(typeof(PointF));

						// make sure we don't intersect other rivers
						bool riversCross = false;
						foreach (River r in this.Rivers)
							if (Geometry.PolylinesIntersect(points, r.Path))
							{
								riversCross = true;
								break;
							}
						if (riversCross)
							continue;

						// store the river data
						River rv = new Rails.River(this.Rivers.Count, points, rand, riverNames);
						this.Rivers.Add(rv);

						// draw the river, thicker near the outlet
						int np = riverPoints.Count * 2 / 3;
						foreach (PointF p in riverPoints)
						{
							Background.SetPixel((int)p.X, (int)p.Y, waterColor);
							isWater[(int)p.X, (int)p.Y] = true;
							if (--np < 0)
							{
								if ((int)p.X<width-1)
								{
									Background.SetPixel((int)p.X+1, (int)p.Y, waterColor);
									isWater[(int)p.X+1, (int)p.Y] = true;
									if ((int)p.Y<height-1)
									{
										Background.SetPixel((int)p.X+1, (int)p.Y+1, waterColor);
										isWater[(int)p.X+1, (int)p.Y+1] = true;
									}
								}
								if ((int)p.Y<height-1)
								{
									Background.SetPixel((int)p.X, (int)p.Y+1, waterColor);
									isWater[(int)p.X, (int)p.Y+1] = true;
								}
							}
						}
					}
				}

				rivers++;
			}
			riverNames = null;

			LocateBridgeSites();

			// outline the water
			for (int x=1; x<width-1; x++)
				for (int y=1; y<height-1; y++)
					if (!isWater[x,y] && field[x,y]>=seaLevel)
						if (isWater[x-1,y] || isWater[x+1,y] || isWater[x,y-1] || isWater[x,y+1])
							Background.SetPixel(x, y, Color.Black);
			isWater = null;
		}

		ZField GenerateTerrain(Progress progress)
		{
			int sc = 1;
			int w = width;
			int h = height;

			int ss = sc * sc;
			int sw = w * sc;
			int sh = h * sc;

			Rails.ZField z1 = new Rails.ZField(sw, sh, rand, 4);
			progress.SetProgress(15);
			Rails.ZField z2 = new Rails.ZField(sw, sh, rand, 8);
			progress.SetProgress(30);
			z2.Fold();

			int[] pct = z1.GetPercentiles(30);
			int sea1 = pct[0];
			pct = z2.GetPercentiles(30);
			int sea2 = pct[0];
			double scaleLand = (256.0 - sea1) / (256.0 - sea2);
			double scaleSea = 1.0 * sea1 / sea2;
			byte[,] zz = new Byte[sw, sh];

			for (int y=0; y<sh; y++)
				for (int x=0; x<sw; x++)
				{
					if (z1[x, y] < sea1)
						zz[x, y] = (byte) z1[x, y];
					else
					{
						double mult = (z1[x, y] - sea1) / 10.0;
						if (mult > 1.0) mult = 1.0;
						zz[x, y] = (byte) (sea1 + (z2[x, y] * mult * (256 - sea1)) / 256);
					}
				}
			Rails.ZField z = new Rails.ZField(zz);

			z1 = null;
			z2 = null;

			int sea = sea1;
			pct = z.GetPercentiles(90);
			int hill = pct[0];

			int[] red = {125, 143, 154, 164, 189, 212, 130, 152, 184, 208, 234, 230, 224, 220};
			int[] grn = {153, 170, 182, 190, 208, 227, 162, 174, 190, 206, 225, 213, 199, 187};
			int[] blu = {192, 207, 216, 213, 229, 238, 116, 129, 144, 160, 179, 160, 140, 122};
			int colors = 13;
			int landIdx = 6;
			int hillIdx = 9;

			double hillExp = Math.Log((hillIdx - landIdx) / (colors - landIdx - 1.0), (hill - sea) / (256.0 - sea));

			int[] adj = new int[256];
			for (int i=0; i<sea; i++)
				adj[i] = i;
			for (int i=sea; i<256; i++)
			{
				double alt = (i - sea) / (256.0 - sea);
				alt = Math.Pow(alt, hillExp);
				adj[i] =  (int) (sea + alt * (256 - sea));
			}

			zz = new byte[w, h];

			RGB[,] co = new RGB[w, h];
			for (int y=0; y<h; y++)
			{
				for (int x=0; x<w; x++)
				{
					int tred = 0;
					int tgrn = 0;
					int tblu = 0;
					int talt = 0;
					int yy = y * sc;
					for (int dy=0; dy<sc; dy++, yy++)
					{
						int xx = x * sc;
						for (int dx=0; dx<sc; dx++, xx++)
						{
							int alt = z[xx, yy];
							talt += adj[alt];
							int rr, gg, bb;
							if (alt >= sea && xx>0 && xx<sw-1 && yy>0 && yy<sh-1 && (z[xx-1,yy]<sea || z[xx+1,yy]<sea || z[xx,yy-1]<sea || z[xx,yy+1]<sea))
							{
								rr = gg = bb = 0; // black
							}
							else
							{
								float idx;
								if (alt < sea)
								{
									idx = (alt * (landIdx - 1.0f)) / sea;
								}
								else
								{
									float a = (adj[alt] - sea) / (256.0f - sea);
									//									a *= a;
									idx = a * (colors - landIdx - 1) + landIdx;
								}
								int i = (int) idx;
								int j = i + 1;
								float q = idx - i;
								//								q = 0;
								float p = 1 - q;
								float fr = (red[i] * p + red[j] * q);
								float fg = (grn[i] * p + grn[j] * q);
								float fb = (blu[i] * p + blu[j] * q);
								if (alt < sea)
								{
									rr = (int) fr;
									gg = (int) fg;
									bb = (int) fb;
								}
								else
								{
									int ds = 3;
									int sdh = 0;
									int sdd = 0;
									for (int dl = -ds; dl<=+ds; dl++)
									{
										int xd = xx + dl;
										if (xd >= 0 && xd < sw)
										{
											sdd += dl * dl;
											sdh += dl * adj[z[xd, yy]];
										}
									}
									double m = 1.0 * sdh / sdd;
									double nx = -m / Math.Sqrt(1 + m * m);
									sdh = sdd = 0;
									for (int dl = -ds; dl<=+ds; dl++)
									{
										int yd = yy + dl;
										if (yd >= 0 && yd < sh)
										{
											sdd += dl * dl;
											sdh += dl * adj[z[xx, yd]];
										}
									}
									m = 1.0 * sdh / sdd;
									double ny = -m / Math.Sqrt(1 + m * m);
									double nz = 5.0;
									double nr = Math.Sqrt(3.0 * (nx * nx + ny * ny + nz * nz)) / 1.4;
									double dot = (- nx - ny + nz) / nr;
									if (dot < 0)
										rr = gg = bb = 0;
									else
									{
										rr = (int) (dot * fr); if (rr > 255) rr = 255;
										gg = (int) (dot * fg); if (gg > 255) gg = 255;
										bb = (int) (dot * fb); if (bb > 255) bb = 255;
									}
								}
							}
							tred += rr;
							tgrn += gg;
							tblu += bb;
						}
					}
					tred /= ss; tgrn /= ss; tblu /= ss;
					co[x, y] = new RGB(tred, tgrn, tblu);
					zz[x, y] = (byte) (talt / ss);
				}
				progress.SetProgress(30 + 70 * y / (h - 1));
			}

			for (int y=0; y<h; y++)
				for (int x=0; x<w; x++)
					Background.SetPixel(x, y, co[x, y].Color);

			return new ZField(zz);
		}

		// Create a new map. The random number generator has already been initialized appropriately.
		void CreateMap()
		{
			waterColor = Color.FromArgb(212, 227, 239);		// color of river/seas
			waterBrush = new SolidBrush(waterColor);

			// create the delegates for the path-finding algorithm
			terrainCostMethod = new FloodMethod(TerrainCostMethod);		// for positioning cities
			portsMethod = new FloodMethod(PortsMethod);					// for finding sea-lanes

			Progress progress = new Progress("Generating random map");

			Background = new Bitmap(width, height);
			ZField field = GenerateTerrain(progress);
			int[] pct = field.GetPercentiles(30, 60, 99);
			if (pct[1] == pct[0])
				pct[1]++;
			GenerateMilepostGrid(field, pct[0], pct[1], pct[2]);
			AnalyzeLandAndSeas();
			GenerateCities();

			// remove inaccessible mileposts from map
			ResetFloodMap();
			Flood(Cities[0].X, Cities[0].Y, terrainCostMethod);
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (milepost[x, y].Value == int.MaxValue)
						milepost[x, y].Terrain = TerrainType.Inaccessible;

//			GenerateSeaLanes();

			int seaLevel = pct[0];
			bool[,] isWater = new Boolean[width, height];

			using (Graphics g = Graphics.FromImage(Background))
			{
				GenerateRivers(field, seaLevel, g, isWater);
			}

			for (int y=0; y<height; y++)
				for (int x=0; x<width; x++)
					isWater[x, y] = field[x, y] < seaLevel;

			// locate causeway sites (track over inlets or lakes)
			LocateWater(WaterMasks.InletMask, isWater);

			DrawForeground();

			progress.Close();
			progress.Dispose();
			field = null;

			Products.UseStandardProducts();
			InitProductSources();
		}

		int massIndex;

		// for area-finding algorithm, identify all mileposts in the same sea or landmass with
		// the same value, and apply a new value at any land/sea transition
		bool LandMassFloodMethod(int x, int y, int d, int i, int j, out int cost)
		{
			bool fromSea = IsSea(x, y);
			bool toSea = IsSea(i, j);
			if (fromSea == toSea)
				cost = 0;
			else
			{
				massIndex++;
				cost = massIndex - milepost[x, y].Value;
			}
			return true;
		}

		// Generate a fractal line using a 2-D recursive midpoint displacement algorithm.
		// Don't draw it yet--just store the points in a list.
		ArrayList DrawFractalLine(Graphics g, Brush brush, float x1, float y1, float x2, float y2, double t1, double t2, bool[,] isWater, ZField field, int seaLevel)
		{
			ArrayList temp;

			// don't subdivide further if endpoints closer than one pixel
			if (Math.Abs(x1 - x2) <= 1 && Math.Abs(y1 - y2) <= 1)
			{
				return new ArrayList();
			}
			
			// find the midpoint
			float xm = (x1 + x2) / 2;
			float ym = (y1 + y2) / 2;

			// offset the midpoint at right angles by a random amount proportional to the 
			// distance between the endpoints
			float dx = x2 - x1;
			float dy = y2 - y1;
			double offset = (rand.NextDouble() - 0.5) / 1.5;
			float x = xm + (float) (dy * offset);
			float y = ym - (float) (dx * offset);

			// do this recursively
			double t = (t1 + t2) / 2.0;
			temp = DrawFractalLine(g, brush, x1, y1, x, y, t1, t, isWater, field, seaLevel);

			// only store the points that are on land
			if (!((int)x < 0 || (int)x >= width || (int)y < 0 || (int)y >= height))
				if (field[(int)x, (int)y] >= seaLevel)
					temp.Add(new PointF(x, y));

			temp.AddRange(DrawFractalLine(g, brush, x, y, x2, y2, t, t2, isWater, field, seaLevel));

			return temp;
		}

		// Simplify the river outline by throwing out points that are closer than 5 pixels than
		// the previous point, but keep both endpoints. Simplifying the outline speeds drawing
		// but more importantly reduces the total cost of the O(m*n) river-intersection algorithm.
		static ArrayList SimplifyPolyLine(ArrayList points)
		{
			ArrayList temp = new ArrayList(points.Count / 5);
			PointF last = (PointF) points[0];
			temp.Add(last);
			foreach (PointF p in points)
			{
				float dist = (p.X - last.X)*(p.X - last.X) + (p.Y - last.Y)*(p.Y - last.Y);
				if (dist >= 25)
				{
					temp.Add(p);
					last = p;
				}
			}
			temp.Add((PointF) points[points.Count - 1]);
			return temp;
		}

		// This cost-finding algorithm delegate is used for identifying good city locations.
		// It makes the cities as far apart as possible, where the distance is determined by
		// how "hard" it would be to travel between the cities.
		bool TerrainCostMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;

			bool sea = IsSea(x, y);
			bool port = IsPort(x, y);
			bool toSea = IsSea(i, j);
			bool toPort = IsPort(i, j);

			// can't travel from sea directly onto land, except at a port
			if (sea && !(toSea || toPort))
				return false;

			// can't travel from land directly onto sea, except at a port
			if (!sea && !port && toSea)
				return false;

			switch(milepost[i, j].Terrain)
			{
				case TerrainType.Inaccessible:
					cost = 100;
					break;
				case TerrainType.Sea:
				case TerrainType.Mountain:
					cost = 2;
					break;
				case TerrainType.Clear:
				case TerrainType.Port:
					cost = 1;
					break;
				case TerrainType.Alpine:
					cost = 5;
					break;
			}

			cost += rand.Next(5);
			return true;
		}

		// This cost-finding algorithm delegate finds the shortest path between two ports.
		bool PortsMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;

			bool sea = IsSea(x, y);
			bool port = IsPort(x, y);
			bool toSea = IsSea(i, j);
			bool toPort = IsPort(i, j);

			// can't go on land at all, except at a port
			if (sea && !(toSea || toPort))
				return false;
			if (!toSea && !toPort)
				return false;

			// can't move from one port to an adjacent port without first going to sea
			if (port && toPort)
				return false;

			cost = 1;
			return true;
		}

		// check if site is valid for a major city
		bool IsValidCapitalSite(int x, int y)
		{
			// must be clear terrain
			if (milepost[x, y].Terrain != TerrainType.Clear)
				return false;

			// must be accessible from previously-known cities
			if (milepost[x, y].Value == int.MaxValue)
				return false;

			// cannot be at edge of map
			if (x < 2 || x > gridW - 3 || y < 2 || y > gridH - 3)
				return false;

			// Must be adjacent to land in all six directions up to two dots away
			// and cannot be in alpine mountains
			int i, j;
			for (int d=0; d<6; d++)
			{
				if (!GetAdjacent(x, y, d, out i, out j))
					return false;
				if (milepost[i, j].Terrain == TerrainType.Inaccessible)
					return false;
				if (milepost[i, j].Terrain == TerrainType.Sea)
					return false;
				if (milepost[i, j].Terrain == TerrainType.Alpine)
					return false;

				int i2, j2;
				if (!GetAdjacent(i, j, d, out i2, out j2))
					return false;
				if (milepost[i2, j2].Terrain == TerrainType.Inaccessible)
					return false;
				if (milepost[i2, j2].Terrain == TerrainType.Sea)
					return false;
				if (!GetAdjacent(i, j, (d+1)%6, out i2, out j2))
					return false;
				if (milepost[i2, j2].Terrain == TerrainType.Inaccessible)
					return false;
				if (milepost[i2, j2].Terrain == TerrainType.Sea)
					return false;
			}

			return true;
		}

		// check if the site is valid for a city
		bool IsValidCitySite(int x, int y)
		{
			return IsValidCityOrTown(x, y, 3);
		}

		// check if the site is valid for a town
		bool IsValidTownSite(int x, int y)
		{
			return IsValidCityOrTown(x, y, 2);
		}

		// check if the site is valid for a city or town
		bool IsValidCityOrTown(int x, int y, int min)
		{
			// must be accessible from previously-known cities
			if (milepost[x, y].Value == int.MaxValue)
				return false;

			// must be clear terrain or port
			if (milepost[x, y].Terrain != TerrainType.Clear && milepost[x, y].Terrain != TerrainType.Port)
				return false;

			// cannot be at edge of map
			if (x <= 0 || x >= gridW - 1 || y <= 0 || y >= gridH - 1)
				return false;

			// must be adjacent to at least 'min' land mileposts
			int n = 0;
			int i, j;
			for (int d=0; d<6; d++)
				if (GetAdjacent(x, y, d, out i, out j))
					if (IsLand(i, j))
						n++;
			if (n < min)
				return false;

			return true;
		}

		// check if the site is valid for a port
		bool IsValidPortSite(int x, int y)
		{
			// must be clear terrain
			if (milepost[x, y].Terrain != TerrainType.Clear)
				return false;

			// must be adjacent to exactly three sea mileposts and three land
			int nw = 0;
			for (int d=0; d<6; d++)
			{
				int i, j;
				if (!GetAdjacent(x, y, d, out i, out j))
					return false;
				if (milepost[i, j].Terrain == TerrainType.Sea)
					nw++;
			}

			if (version >= 1)
				return (nw > 0);
			else
				return (nw == 4);
		}

		// randomly distribute the commodities to the cities on the map
		void InitProductSources()
		{
			// allocate memory
			ProductSources = new ArrayList[Products.Count];
			for (int i=0; i<this.CityCount; i++)
				Cities[i].Products.Clear();

			for (int i=0; i<Products.Count; i++)
			{
				ProductSources[i] = new ArrayList(3);
				int n = rand.Next(3) + 1; // one, two, or three sources, chosen randomly
				for (int j=0; j<n; j++)
				{
					int c;

					// loop until we find a suitable source city
					while (true)
					{
						// choose a city at random
						c = rand.Next(this.CityCount);

						// make sure we didn't already choose it for this product
						if (Cities[c].Products.Contains(i))
							continue;

						int k = Cities[c].Products.Count;

						// OK if the city hasn't been chosen for any products yet
						if (k == 0)
							break;

						// not OK if the city already has two products
						if (k == 2)
							continue;

						// maybe OK if it already has one, but discourage it to try to
						// minimize the number if cities with zero products
						if (rand.Next(2) == 0)
							break;
					}
					Cities[c].Products.Add(i);
					ProductSources[i].Add(c);
				}
			}
		}

		// determine if a line segment drawn on the background would overwrite a water pixel
		bool LineSegmentTouchesWater(int x1, int y1, int x2, int y2, bool[,] isWater)
		{
			if (Math.Abs(x1 - x2) < 2 && Math.Abs(y1 - y2) < 2)
				return false;

			int x = (x1 + x2) / 2;
			int y = (y1 + y2) / 2;
			if (isWater[x - 1, y])
				return true;
			if (isWater[x + 1, y])
				return true;
			if (isWater[x, y + 1])
				return true;
			if (isWater[x, y - 1])
				return true;

			if (LineSegmentTouchesWater(x1, y1, x, y, isWater))
				return true;
			if (LineSegmentTouchesWater(x, y, x2, y2, isWater))
				return true;

			return false;
		}

		// find all the track segments that would cross water (rivers handled separately)
		void LocateWater(StaticIntArray masks, bool[,] isWater)
		{
			int xx, yy, u, v, uu, vv;
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (IsLand(x, y))
						if (GetCoord(x, y, out xx, out yy))
							for (int d=0; d<3; d++)
								if (GetAdjacent(x, y, d, out u, out v))
									if (IsLand(u, v))
										if (GetCoord(u, v, out uu, out vv))
											if (LineSegmentTouchesWater(xx, yy, uu, vv, isWater))
											{
												milepost[x, y].WaterMask |= masks[d];
												milepost[u, v].WaterMask |= masks[(d+3) % 6];
											}
		}

		// make sure the map allows up to six players to built to all the major cities
		int[,,] testTrack;
		int testPlayer = -1;
		bool suppressOtherTrack = false;

		bool IsViableInternal()
		{
			int x, y, x2, y2;

			testTrack = new int[gridW, gridH, 6];
			suppressOtherTrack = false;
			FloodMethod testTrackMethod = new FloodMethod(TestTrackMethod);
			for (testPlayer=1; testPlayer<=6; testPlayer++)
			{
			reevaluatePlayer:
				int maxCities;
				if (version < 2)
					maxCities = NumCapitals;
				else if (testPlayer <= 2)
					maxCities = CityCount;
				else if (testPlayer <= 3)
					maxCities = NumCapitals + NumCities;
				else
					maxCities = NumCapitals;
				this.ResetFloodMap();
				this.Flood(Cities[0].X, Cities[0].Y, testTrackMethod);
				bool[] connected = new bool[maxCities];
				connected[0] = true;
				for (int i=1; i<maxCities; i++)
				{
					int best = -1;
					int dist = int.MaxValue;
					for (int j=0; j<maxCities; j++)
						if (!connected[j])
						{
							int m = milepost[Cities[j].X, Cities[j].Y].Value;
							if (m < dist)
							{
								best = j;
								dist = m;
							}
						}

					// must nationalize some of the map
					if (best == -1)
					{
						// make a note of mileposts that we are currently able to build to
						bool[,] accessible = new bool[gridW, gridH];
						for (x=0; x<gridW; x++)
							for (y=0; y<gridH; y++)
								if (milepost[x, y].Value != int.MaxValue)
									accessible[x, y] = true;

						// find flood basin for each disconnected capital
						this.ResetFloodMap();
						for (int j=0; j<maxCities; j++)
							if (!connected[j])
								milepost[Cities[j].X, Cities[j].Y].Value = 0;
						this.Flood(testTrackMethod);
						bool[,] basin = new bool[gridW, gridH];
						for (x=0; x<gridW; x++)
							for (y=0; y<gridH; y++)
								basin[x, y] = milepost[x, y].Value != int.MaxValue;

						// suppress other players' track and try building again
						suppressOtherTrack = true;
						this.ResetFloodMap();
						for (x=0; x<gridW; x++)
							for (y=0; y<gridH; y++)
								if (accessible[x, y])
									milepost[x, y].Value = 0;
						this.Flood(testTrackMethod);
						suppressOtherTrack = false;

						// find closest accessible spot in disconnected basins
						int bx = -1, by = -1;
						best = int.MaxValue;
						for (x=0; x<gridW; x++)
							for (y=0; y<gridH; y++)
								if (basin[x, y])
									if (milepost[x, y].Value < best)
									{
										best = milepost[x, y].Value;
										bx = x; by = y;
									}
						if (best == 0 /*bug*/ || best == -1)
							goto badMap;

						// nationalize track from there back to current player's network
						if (nationalized == null)
							nationalized = new bool[gridW, gridH, 6];
						using (Graphics g = Graphics.FromImage(Background))
						{
							using (Pen pen = new Pen(Color.FromArgb(100, Color.Red), 3))
							{
								pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
								int x1, y1, cx, cy;
								if (!GetCoord(bx, by, out x1, out y1))
									goto badMap;
								while (milepost[bx, by].Value > 0)
								{
									int d = (milepost[bx, by].Gradient + 3) % 6;
									if (!GetAdjacent(bx, by, d, out cx, out cy))
										goto badMap;
									bool isLand = milepost[bx, by].Terrain != TerrainType.Sea && milepost[cx, cy].Terrain != TerrainType.Sea;
									bool already = false;
									if (isLand)
									{
										already |= nationalized[bx, by, d];
										nationalized[bx, by, d] = true;
										testTrack[bx, by, d] = 7;
									}
									d = (d + 3) % 6;
									if (isLand)
									{
										already |= nationalized[cx, cy, d];
										nationalized[cx, cy, d] = true;
										testTrack[cx, cy, d] = 7;
									}
									bx = cx; by = cy;
									if (!GetCoord(bx, by, out x2, out y2))
										goto badMap;
									if (isLand && !already)
										g.DrawLine(pen, x1, y1, x2, y2);
									x1 = x2; y1 = y2;
								}
							}
						}

						// try it again with new nationalized track in place
						goto reevaluatePlayer;
					}

					connected[best] = true;
					x = Cities[best].X;
					y = Cities[best].Y;
					x2 = 0;
					y2 = 0;
					while (milepost[x, y].Value > 0)
					{
						milepost[x, y].Value = 0;
						int d = milepost[x, y].Gradient;
						if (d == -1)
							goto badMap;
						int d2 = (d+3) % 6;
						if (testTrack[x, y, d2] != 7)
							testTrack[x, y, d2] = testPlayer;
						GetAdjacent(x, y, d2, out x2, out y2);
						x = x2; y = y2;
						if (testTrack[x, y, d] != 7)
							testTrack[x, y, d] = testPlayer;
					}
					for (x=0; x<gridW; x++)
						for (y=0; y<gridH; y++)
							if (milepost[x, y].Value != 0)
								milepost[x, y].Value = int.MaxValue;
					this.Flood(testTrackMethod);
				}
			}
			testTrack = null;
			return true;
			badMap:
				testTrack = null;
			return false;
		}

		// delegate for path-finding algorithm that determines where track builds are allowed 
		bool TestTrackMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;

			bool fromSea = IsSea(x, y);
			bool fromPort = IsPort(x, y);
			bool toSea = IsSea(i, j);
			bool toPort = IsPort(i, j);

			// don't need to build track on sea--can move there at will
			if (fromSea && (toSea || toPort))
			{
				cost = 0;
				return true;
			}

			// can't move from sea to land, except at a port
			if (fromSea)
				return false;

			// don't need to build track to embark from a port 
			if (toSea && fromPort)
			{
				cost = 0;
				return true;
			}

			// can't move to sea from land, except at a port
			if (toSea)
				return false;

			// it's free if we already have useable track at this segment
			if (testTrack[x, y, d] == testPlayer)
			{
				cost = 0;
				return true;
			}

			// it's free if there is nationalized track at this segment
			if (testTrack[x, y, d] == 7)
			{
				cost = 0;
				return true;
			}

			// prohibited if somebody else's track is here
			if (testTrack[x, y, d] != 0 && !suppressOtherTrack)
				return false;

			// free to move within major city hexagon
			if (IsCapital(x, y) && IsCapital(i, j))
			{
				cost = 0;
				return true;
			}

			// otherwise track can be built
			cost = 1;
			return true;
		}
	}
}
