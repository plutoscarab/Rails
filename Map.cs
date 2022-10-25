
// Map.cs

/*
 * This class represents the map. It contains no information about a specific game session
 * that uses the map. This is the base class for RandomMap and AuthoredMap.
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
	// The Map class contains an implemention of a region-filling and/or path-finding algorithm
	// which needs to be provided with the specific details of how to seek out and identify
	// the required mileposts and how to label them. This delegate is the interface that is
	// used for that.  x,y=old position, d=direction, i,j=new position, cost is cost. See
	// the various Flood() implementations for more details. Do not confuse the Flood() method
	// with the Flood class which implements a river flood random disaster.
	public delegate bool FloodMethod(int x, int y, int d, int i, int j, out int cost);

	// function pointer for Flood() method to determine which mileposts are valid for given purpose
	public delegate bool IsValidSite(int x, int y);

	public class Map : IDisposable
	{
		public Size ImageSize;				// the size, in pixels, of the map image
		public Size GridSize;				// the size, in mileposts, of the map
		public Bitmap Background;			// the background image (land and water only)
		public Bitmap Foreground;			// the foreground image (cities, mileposts, and ports)
		public City[] Cities;				// list of cities
		public ArrayList[] ProductSources;	// the sources for each commodity
		public ArrayList Rivers;			// list of rivers
		public Point[] Seas;				// list of sea locations
		public int NumCapitals = 8;			// the number of major cities
		public int NumCities = 16;			// the number of minor cities
		public int NumTowns = 24;			// the number of towns
		public int CityCount = 48;

		const int gridSpacing = 15;			// horizontal spacing of mileposts

		protected int gridSpacingY;			// vertical spacing of mileposts
		protected Milepost[,] milepost;		// the mileposts themselves
		protected int width, height;		// subfields of the ImageSize field
		protected int gridW, gridH;			// subfields of the GridSize field

		// delegates for various path-finding algorithms
		public FloodMethod cityDistanceMethod;

		// create a new map
		public Map(Size imageSize)
		{
			CreateMap(imageSize);
		}

		// Save the map to disk.
		public virtual void Save()
		{
		}

		// Create a new map.
		void CreateMap(Size imageSize)
		{
			ImageSize = imageSize;

			// create the delegates for the path-finding algorithm
			cityDistanceMethod = new FloodMethod(CityDistanceMethod);	// for computing contract payoffs

			// calculate map grid size
			width = imageSize.Width;
			height = imageSize.Height;
			gridSpacingY = (int) (gridSpacing * 2 / Math.Sqrt(3));
			gridW = (width - gridSpacing / 2) / gridSpacing;
			gridH = height / gridSpacingY;
			GridSize = new Size(gridW, gridH);
		}

		// Draw the foreground, including the cities and mileposts
		protected void DrawForeground()
		{
			Foreground = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(Foreground);

			// draw the cities and towns
			int xx, yy;
			for (int i=0; i<this.NumCapitals; i++)
				if (GetCoord(Cities[i].X, Cities[i].Y, out xx, out yy))
					g.DrawImageUnscaled(Images.Capital, xx - 15, yy - 17);
			for (int i=this.NumCapitals; i<this.NumCapitals + this.NumCities; i++)
				if (GetCoord(Cities[i].X, Cities[i].Y, out xx, out yy))
					g.DrawImageUnscaled(Images.City, xx - 7, yy - 7);
			for (int i=this.NumCapitals + this.NumCities; i<this.CityCount; i++)
				if (GetCoord(Cities[i].X, Cities[i].Y, out xx, out yy))
					g.DrawImageUnscaled(Images.Town, xx - 7, yy - 7);

			// draw the mileposts
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH;  y++)
					if (GetCoord(x, y, out xx, out yy))
					{
						switch(milepost[x, y].Terrain)
						{
							case TerrainType.Inaccessible:
								break;
							case TerrainType.Sea:
								g.DrawImageUnscaled(Images.BlueDot, xx - 1, yy - 1);
								break;
							case TerrainType.Clear:
								g.DrawImageUnscaled(Images.BlackDot, xx - 1, yy - 1);
								break;
							case TerrainType.Mountain:
								g.DrawImageUnscaled(Images.Hill, xx - 3, yy - 4);
								break;
							case TerrainType.Alpine:
								g.DrawImageUnscaled(Images.Alpine, xx - 4, yy - 5);
								break;
							case TerrainType.Port:
								g.DrawImageUnscaled(Images.Port, xx - 5, yy - 5);
								break;
						}
					}

			// draw the city names
			Font font = new Font("Arial", 8.0f);
			Brush brush = Brushes.Black;
			Brush erase = Brushes.White;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			for (int i=0; i<this.CityCount; i++)
				if (GetCoord(Cities[i].X, Cities[i].Y, out xx, out yy))
				{
					string s = Cities[i].Name;
					SizeF size = g.MeasureString(s, font);
					if (i < this.NumCapitals)
					{
						xx -= (int) (size.Width / 2);
						yy -= (int) (size.Height / 2);
					}
					else
					{
						xx -= (int) (size.Width * xx / width);
						yy += 7;
					}
					Utility.DrawStringOutlined(g, s, font, brush, erase, xx, yy);
				}
			font.Dispose();
			g.Dispose();
		}

		// calculate gale and strike affect areas
		public void InitializeSeaDisasters()
		{
			for (int i=0; i<Seas.Length; i++)
				new Coastline(i, this);
		}

		public void LocateBridgeSites()
		{
			int xx, yy, uu, vv;
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
					milepost[x, y].RiversCrossed = new long[6];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					if (GetCoord(x, y, out xx, out yy))
						for (int d=0; d<3; d++)
						{
							int u, v;
							if (GetAdjacent(x, y, d, out u, out v))
								if (GetCoord(u, v, out uu, out vv))
								{
									long mask = 1;
									foreach (River r in this.Rivers)
									{
										if (Geometry.LineSegmentIntersectsPolyline(xx, yy, uu, vv, r.Path))
										{
											milepost[x, y].WaterMask |= WaterMasks.RiverMask[d];
											milepost[u, v].WaterMask |= WaterMasks.RiverMask[d+3];
											milepost[x, y].RiversCrossed[d] |= mask;
											milepost[u, v].RiversCrossed[d+3] |= mask;
										}
										mask <<= 1;
									}
								}
						}
				}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Background.Dispose();
			Foreground.Dispose();
		}

		public Milepost this[int x, int y]
		{
			get { return milepost[x, y]; }
		}

		// initialize the path-finding and/or distance-finding array
		public void ResetFloodMap()
		{
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					milepost[x, y].Value = int.MaxValue;
					milepost[x, y].Gradient = -1;
				}
		}

		// This cost-finding algorith delegate is used for determining contract payoffs.
		// It counts the number of mileposts between map locations, only allowing valid
		// sea travel. All terrain types are otherwise treated equally.
		bool CityDistanceMethod(int x, int y, int d, int i, int j, out int cost)
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

			cost = 1;
			return true;
		}

		// flood the map starting at a certain location and using a certain method
		public void Flood(int startX, int startY, FloodMethod method)
		{
			int x, y;
			Flood(startX, startY, method, null, -1, out x, out y);
		}

		// Flood the map starting at a certain location and using a certain method.
		// Look for the highest-cost milepost that meets certain criteria.
		protected void Flood(int startX, int startY, FloodMethod method, IsValidSite isValidSite, out int maxX, out int maxY)
		{
			Flood(startX, startY, method, isValidSite, -1, out maxX, out maxY);
		}

		// Flood the map starting at a certain location and using a certain method.
		// Look for the highest-cost milepost that meets certain criteria.
		// Mark each milepost with a specific country identifier.
		protected void Flood(int startX, int startY, FloodMethod method, IsValidSite isValidSite, int capital, out int maxX, out int maxY)
		{
			milepost[startX, startY].Value = 0;
			if (capital != -1)
				milepost[startX, startY].Capital = capital;
			Flood(method, isValidSite, capital, out maxX, out maxY);
		}

		// Flood the map starting at mileposts that are currently zero-cost, using
		// the specified flood method.
		public void Flood(FloodMethod method)
		{
			int x, y;
			Flood(method, null, -1, out x, out y);
		}

		// Flood the map starting at mileposts that are currently zero-cost, using
		// the specified flood method. If isValidSite is not null, look for the highest-cost 
		// milepost that meets certain criteria. If capital != -1, mark each milepost found 
		// with a specific country identifier.
		public void Flood(FloodMethod method, IsValidSite isValidSite, int capital, out int maxX, out int maxY)
		{
			// clear the outputs
			maxX = maxY = 0;

			// set the starting location for the flood
			bool[,] b = new Boolean[gridW, gridH];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH;  y++)
					if (milepost[x, y].Value == 0)
						b[x, y] = true;

			// loop until we run out of terrain
			bool done;
			do
			{
				// clear the map of interesting spots for the next loop
				done = true;
				bool[,] b2 = new bool[gridW, gridH];

				// scan all the interesting spots found in the previous loop
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if (b[x, y])
						{
							int i, j;

							// examine adjacent mileposts
							int cost;
							for (int d=0; d<6; d++)
								if (GetAdjacent(x, y, d, out i, out j))
									if (milepost[i, j].Terrain != TerrainType.Inaccessible)
										// call the flood method delegate
										if (method(x, y, d, i, j, out cost))
										{
											cost += milepost[x, y].Value;

											// if the cost is at least as good as previously found, 
											// store the result and mark as interesting for the
											// next loop
											if (cost < milepost[i, j].Value)
											{
												milepost[i, j].Value = cost;
												milepost[i, j].Gradient = d;
												if (capital != -1)
													milepost[i, j].Capital = capital;
												b2[i, j] = true;
												done = false;
											}
										}
						}

				// hand off interesting spots for next loop
				b = b2;
			}
			while (!done);

			// calculate milepost "furthest" from start according to specified cost algorithm
			if (isValidSite != null)
			{
				int max = int.MinValue;
				for (int x=0; x<gridW; x++)
					for (int y=0; y<gridH; y++)
						if (milepost[x, y].Value > max)
							if (isValidSite(x, y))
							{
								max = milepost[x, y].Value;
								maxX = x;
								maxY = y;
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

		// determine if two mileposts are adjacent
		public bool IsAdjacent(int x, int y, int i, int j)
		{
			int xx, yy;
			for (int d=0; d<6; d++)
				if (GetAdjacent(x, y, d, out xx, out yy))
					if (i == xx && j == yy)
						return true;
			return false;
		}

		// determine if two mileposts are the same or are adjacent
		public bool IsAdjacentOrEqual(int x, int y, int i, int j)
		{
			return (x == i && y == j) || IsAdjacent(x, y, i, j);
		}

		// determine if a milepost is a land terrain type, including a port
		public bool IsLand(int x, int y)
		{
			TerrainType t = milepost[x, y].Terrain;
			return t != TerrainType.Inaccessible && t != TerrainType.Sea;
		}

		public bool IsSea(int x, int y)
		{
			return milepost[x, y].Terrain == TerrainType.Sea;
		}

		public bool IsPort(int x, int y)
		{
			return milepost[x, y].Terrain == TerrainType.Port;
		}

		public bool IsCity(int x, int y)
		{
			return milepost[x, y].CityType != CityType.None;
		}

		public bool IsCapitalCorner(int x, int y)
		{
			return milepost[x, y].CityType == CityType.CapitalCorner;
		}

		// determine if a milepost is a major city center or hex corner
		public bool IsCapital(int x, int y)
		{
			return milepost[x, y].CityType == CityType.Capital || IsCapitalCorner(x, y);
		}

		// calculate the distance between two cities
		public int CityDistance(int city1, int city2)
		{
			int x, y;

			City c1 = Cities[city1];
			City c2 = Cities[city2];
			ResetFloodMap();
			Flood(c1.X, c1.Y, cityDistanceMethod, null, out x, out y);

			return milepost[c2.X, c2.Y].Value;
		}

		// intialize the map values for the Flood() algorithm, usually to zero or int.MaxValue
		public void SetValue(int x, int y, int v)
		{
			milepost[x, y].Value = v;
		}

		// used by the Coastline class to indicate which sea each sea milepost belongs to
		public void SetSeaIndex(int x, int y, int sea)
		{
			milepost[x, y].SeaIndex = sea;
		}

		// used by the Coastline class to keep track of how far each land milepost is from each sea
		public void SetDistanceFromSea(int x, int y, int sea, byte distance)
		{
			if (milepost[x, y].DistanceFromSea == null)
				milepost[x, y].DistanceFromSea = new byte[Seas.Length];
			milepost[x, y].DistanceFromSea[sea] = distance;
		}

		public int DistanceInland(int x, int y)
		{
			return milepost[x, y].DistanceInland;
		}

		public virtual bool IsViable()
		{
			return true;
		}
	}
}
