
// RandomMap.cs

/*
 * This class represents a randomly-generated map. The map can be recreated exactly by providing 
 * the same random number seed.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;

namespace Rails
{
	public class RandomMap : Map
	{
		public int Number;						// the seed used to initialize the random number generator

		Color waterColor = Color.LightBlue;		// color of river/seas
		Brush waterBrush = Brushes.LightBlue;

		Random rand;							// the much-used random-number generator

		// delegates for various path-finding algorithms
		FloodMethod terrainCostMethod;
		FloodMethod portsMethod;

		// create a new map with a random seed
		public RandomMap(Size imageSize) : base(imageSize)
		{
			Number = ChooseSeed();
			rand = new Random(Number);
			CreateMap();
		}

		// create a new map with a given seed
		public RandomMap(Size imageSize, int number) : base(imageSize)
		{
			Number = number;
			rand = new Random(number);
			CreateMap();
		}

		// choose a random seed
		int ChooseSeed()
		{
			// This works because this is an entropy-based random number generator
			// from System.Security.Cryptography.
			RandomNumberGenerator rng = new RNGCryptoServiceProvider();
			byte[] seed = new byte[4];
			rng.GetBytes(seed);
			return seed[0] + 256 * (seed[1] + 256 * (seed[2] + 256 * seed[3]));
		}

		// Save the map to disk. Since all we need is the seed to recreate the map, we
		// save a lot of space by just storing the seed and a thumbnail image. The seed
		// is stored in the image filename.
		public override void Save()
		{
			try
			{
				Directory.CreateDirectory("maps");
				uint n = unchecked((uint) Number);
				string filename = "maps\\" + n.ToString() + ".png";
				
				int tw = 128;
				int th = tw * ImageSize.Height / ImageSize.Width;
				using (Bitmap thumb = new Bitmap(tw, th))
				{
					using (Image bg = this.Background.GetThumbnailImage(tw, th, null, IntPtr.Zero))
					{
						using (Image fg = this.Foreground.GetThumbnailImage(tw, th, null, IntPtr.Zero))
						{
							using (Graphics g = Graphics.FromImage(thumb))
							{
								g.DrawImage(bg, 0, 0, tw, th);
								g.DrawImage(fg, 0, 0, tw, th);
								thumb.Save(filename);
							}
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
						else if (field[xx, yy] >= alpineLevel)
							t = TerrainType.Alpine;
						else if (field[xx, yy] >= hillLevel)
							t = rand.Next(3) == 0 ? TerrainType.Mountain : TerrainType.Clear;
						else
							t = rand.Next(8) == 0 ? TerrainType.Mountain : TerrainType.Clear;
						milepost[x, y].Terrain = t;
						milepost[x, y].CityIndex = -1;
						milepost[x, y].SeaIndex = -1;
					}
				}
		}

		void AnalyzeLandAndSeas()
		{
			int xx, yy, uu, vv;

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

			// use ports to connect each sufficiently large land mass to neighboring land masses
			for (int i=0; i<massCount-1; i++) 
				if ((int) masses[massIdx[i]] >= 13) // must be at least 13 mileposts in size
					for (int j=i+1; j<massCount; j++)
						if ((int) masses[massIdx[j]] >= 13)
						{
							int min = int.MaxValue;
							int bx, by, bu, bv;
							bx = by = bu = bv = -1;
							for (int x=0; x<gridW; x++)
								for (int y=0; y<gridH; y++)
									if (milepost[x, y].Value == massIdx[i])
										if (IsValidPortSite(x, y))
											if (GetCoord(x, y, out xx, out yy))
												for (int u=0; u<gridW; u++)
													for (int v=0; v<gridH; v++)
														if (milepost[u, v].Value == massIdx[j])
															if (IsValidPortSite(u, v))
																if (GetCoord(u, v, out uu, out vv))
																	if ((adjacent[i] & adjacent[j]) != 0)
																	{
																		int dist = (xx - uu) * (xx - uu) + (yy - vv) * (yy - vv);
																		if (dist < min)
																		{
																			min = dist;
																			bx = x; by = y; bu = u; bv = v;
																		}
																	}
							if (min < int.MaxValue)
							{
								milepost[bx, by].Terrain = TerrainType.Port;
								milepost[bu, bv].Terrain = TerrainType.Port;
							}
						}

			// record locations of large seas
			int nseas = 0;
			for (int i=0; i<massCount-1; i++)
				if ((int) masses[massIdx[i]] > 50)
					if (IsSea(massLocation[i].X, massLocation[i].Y))
						nseas++;
			Seas = new Point[nseas];
			nseas = 0;
			for (int i=0; i<massCount-1; i++)
				if ((int) masses[massIdx[i]] > 50)
					if (IsSea(massLocation[i].X, massLocation[i].Y))
					{
						Seas[nseas] = massLocation[i];
						nseas++;
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
					if (!isWater[x,y])
						if (isWater[x-1,y] || isWater[x+1,y] || isWater[x,y-1] || isWater[x,y+1])
							Background.SetPixel(x, y, Color.Gray);
			isWater = null;
		}

		// Create a new map. The random number generator has already been initialized appropriately.
		void CreateMap()
		{
			// create the delegates for the path-finding algorithm
			terrainCostMethod = new FloodMethod(TerrainCostMethod);		// for positioning cities
			portsMethod = new FloodMethod(PortsMethod);					// for finding sea-lanes

			Indicator ind = new Indicator("Generating random map");

			// generate the terrain height map
			ZField field = new ZField(width, height, rand);

			// determine altitudes for coastline and mountains
			int[] levels;
			levels = field.GetPercentiles(30, 60, 99); // 30% water, 30% lowlands, 40% highlands including 1% alpine
			int seaLevel = levels[0];
			int hillLevel = levels[1];
			int alpineLevel = levels[2];

			GenerateMilepostGrid(field, seaLevel, hillLevel, alpineLevel);
			AnalyzeLandAndSeas();
			GenerateCities();

			// remove inaccessible mileposts from map
			ResetFloodMap();
			Flood(Cities[0].X, Cities[0].Y, terrainCostMethod);
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					if (milepost[x, y].Value == int.MaxValue)
						milepost[x, y].Terrain = TerrainType.Inaccessible;

			GenerateSeaLanes();

			// draw the land and water
			bool[,] isWater = new Boolean[width, height];
			Background = new Bitmap(width, height);
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
					if (field[x,y] < seaLevel)
					{
						Background.SetPixel(x, y, waterColor);
						isWater[x, y] = true;
					}
					else
						Background.SetPixel(x, y, Color.White);

			// locate causeway sites (track over inlets or lakes)
			LocateWater(WaterMasks.InletMask);

			Graphics g = Graphics.FromImage(Background);

			GenerateRivers(field, seaLevel, g, isWater);

			// draw the country borders (cosmetic -- not actually used for the game)
			using (Pen pen = (Pen) Pens.Gray.Clone())
			{
				int x1, y1, x2, y2, i, j;
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if (milepost[x, y].Terrain != TerrainType.Sea && milepost[x, y].Terrain != TerrainType.Inaccessible)
							if (GetCoord(x, y, out x1, out y1))
								for (int d=0; d<6; d++)
									if (GetAdjacent(x, y, d, out i, out j))
										if (milepost[x, y].Capital != milepost[i, j].Capital)
											if (milepost[i, j].Terrain != TerrainType.Sea && milepost[i, j].Terrain != TerrainType.Inaccessible)
												if (GetCoord(i, j, out x2, out y2))
												{
													int dx = (int) ((x2 - x1) / 3.464);
													int dy = (int) ((y2 - y1) / 3.464);
													int x0 = (x1 + x2) / 2;
													int y0 = (y1 + y2) / 2;
													g.DrawLine(pen, x0 - dy, y0 + dx, x0 + dy, y0 - dx);
												}
			}

			g.Dispose();

			DrawForeground();

			ind.Close();
			ind.Dispose();
			field = null;

			Products.UseStandardProducts();
			InitProductSources();
		}

		// make sure the map allows up to six players to built to all the major cities
		int[,,] testTrack;
		int testPlayer = -1;

		public override bool IsViable()
		{
			testTrack = new int[gridW, gridH, 6];
			FloodMethod testTrackMethod = new FloodMethod(TestTrackMethod);
			for (testPlayer=1; testPlayer<=6; testPlayer++)
			{
				this.ResetFloodMap();
				milepost[Cities[0].X, Cities[0].Y].Value = 0;
				this.Flood(Cities[0].X, Cities[0].Y, testTrackMethod);
				bool[] connected = new bool[this.NumCapitals];
				connected[0] = true;
				for (int i=1; i<this.NumCapitals; i++)
				{
					int best = -1;
					int dist = int.MaxValue;
					for (int j=0; j<this.NumCapitals; j++)
						if (!connected[j])
						{
							int m = milepost[Cities[j].X, Cities[j].Y].Value;
							if (m < dist)
							{
								best = j;
								dist = m;
							}
						}
					if (best == -1)
						goto badMap;
					connected[best] = true;
					int x = Cities[best].X;
					int y = Cities[best].Y;
					int x2 = 0;
					int y2 = 0;
					while (milepost[x, y].Value > 0)
					{
						milepost[x, y].Value = 0;
						int d = milepost[x, y].Gradient;
						if (d == -1)
							goto badMap;
						testTrack[x, y, (d+3) % 6] = testPlayer;
						GetAdjacent(x, y, (d+3) % 6, out x2, out y2);
						x = x2; y = y2;
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

		// delegate for path-finding algorithm that determines where track builds are allowed, 
		// in order to find cheapest path for track building
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

			// it's free if we already have track at this segment
			if (testTrack[x, y, d] == testPlayer)
			{
				cost = 0;
				return true;
			}

			// prohibited if somebody else's track is here
			if (testTrack[x, y, d] != 0)
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
			// (the real game doesn't have this, but Sky wanted it)
			int i, j;
			for (int d=0; d<6; d++)
			{
				if (!GetAdjacent(x, y, d, out i, out j))
					return false;
				if (milepost[i, j].Terrain == TerrainType.Sea)
					return false;
				int i2, j2;
				if (!GetAdjacent(i, j, d, out i2, out j2))
					return false;
				if (milepost[i2, j2].Terrain == TerrainType.Sea)
					return false;
				if (!GetAdjacent(i, j, (d+1)%6, out i2, out j2))
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

			return (nw == 3);
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

		// determine if a line segment drawn on the background would overwrite a pixel of a specific color
		bool LineSegmentTouchesColor(int x1, int y1, int x2, int y2, int argb)
		{
			if (Math.Abs(x1 - x2) < 2 && Math.Abs(y1 - y2) < 2)
				return false;

			int x = (x1 + x2) / 2;
			int y = (y1 + y2) / 2;
			if (Background.GetPixel(x - 1, y).ToArgb() == argb)
				return true;
			if (Background.GetPixel(x + 1, y).ToArgb() == argb)
				return true;
			if (Background.GetPixel(x, y + 1).ToArgb() == argb)
				return true;
			if (Background.GetPixel(x, y - 1).ToArgb() == argb)
				return true;

			if (LineSegmentTouchesColor(x1, y1, x, y, argb))
				return true;
			if (LineSegmentTouchesColor(x, y, x2, y2, argb))
				return true;

			return false;
		}

		// find all the track segments that would cross water (rivers handled separately)
		void LocateWater(StaticIntArray masks)
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
											if (LineSegmentTouchesColor(xx, yy, uu, vv, waterColor.ToArgb()))
											{
												milepost[x, y].WaterMask |= masks[d];
												milepost[u, v].WaterMask |= masks[(d+3) % 6];
											}
		}

		// a failed attempt at drawing the river names on the map
		/*
		void DrawTextAlongPolyline(Graphics g, string text, Point[] path, Font font, Brush brush)
		{
			int p = path.Length;
			int n = text.Length;
			StringFormat fmt = new StringFormat();
			fmt.Alignment = StringAlignment.Center;
			int k = p / n;
			if (k < 1) k = 1;
			if (k > 3) k = 3;
			for (int i=0; i<n; i++)
			{
				int j = (p - k * n) / 2 + k * i + 1;
				int j1 = j - 3; if (j1 < 0) j1 = 0;
				int j2 = j + 3; if (j2 > p - 1) j2 = p - 1;
				int dx = path[j2].X - path[j1].X;
				int dy = path[j2].Y - path[j1].Y;
				double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
				g.TranslateTransform(path[j].X, path[j].Y);
				g.RotateTransform((float) angle);
				g.DrawString(text.Substring(i, 1), font, brush, 0, 0, fmt);
				g.ResetTransform();
			}
			fmt.Dispose();
		}
		*/
	}
}
