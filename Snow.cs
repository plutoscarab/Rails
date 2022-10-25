
// Snow.cs

/*
 * This class represents the Snow random event. Snow prevents movement through mountains and
 * slows movement elsewhere in the affected area, and prevents rail building.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Rails
{
	public class Snow : Disaster, IDisposable
	{
		static Random rand = new Random();	// shared by all instances of the Snow class

		private Bitmap bitmap;				// picture of the snowstorm
		private bool[,] mountains;			// which mountains are at the center of the storm
		private bool[,] affected;			// which mileposts are affected
		private TerrainType terrain;		// whether it's alpine, or both alpine and mountain

		public Snow(int player, Map map, bool alpine) : base(player, map)
		{
			// create flags indicating the terrain types that prevent movement
			terrain = TerrainType.Alpine;
			if (!alpine)
				terrain |= TerrainType.Mountain;

			// find a suitable location for the storm
			int x = 0, y = 0;
			while (true)
			{
				// pick a spot at random
				x = rand.Next(map.GridSize.Width);
				y = rand.Next(map.GridSize.Height);

				// it has to be the correct terrain type
				if ((map[x, y].Terrain & terrain) == 0)
					continue;

				// it has to be adjacent to at least three other mountains
				int n = 0;
				int i, j;
				for (int d=0; d<6; d++)
					if (map.GetAdjacent(x, y, d, out i, out j))
						if (map[i, j].Terrain == TerrainType.Mountain || map[i, j].Terrain == TerrainType.Alpine)
							n++;
				if (n < 3)
					continue;

				break;
			}

			// find the contiguous chain of mountains connected to our starting mountain,
			// but only go up to three mileposts away
			map.ResetFloodMap();
			map.Flood(x, y, new FloodMethod(AdjacentMountainsMethod));

			// create the shape of the snowstorm
			Region region = new Region();
			region.MakeEmpty();
			mountains = new bool[map.GridSize.Width, map.GridSize.Height];
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value != int.MaxValue)
					{
						mountains[x, y] = true;
						map.SetValue(x, y, 0);
						GraphicsPath path = new GraphicsPath();
						int xx, yy;
						map.GetCoord(x, y, out xx, out yy);
						path.AddEllipse(xx - 55, yy - 55, 110, 110);
						region.Union(path);
					}

			// draw the snowstorm
			bitmap = new Bitmap(map.ImageSize.Width, map.ImageSize.Height);
			Graphics g = Graphics.FromImage(bitmap);

			// semi-transparent gray background
			Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray));
			g.FillRegion(brush, region);
			brush.Dispose();

			// find all mileposts within three units of the affected mountain chain
			map.Flood(new FloodMethod(LinearDistanceMethod));
			affected = new bool[map.GridSize.Width, map.GridSize.Height];
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value < 4)
					{
						affected[x, y] = true;
						int xx, yy;
						map.GetCoord(x, y, out xx, out yy);

						// Draw random snowflakes. 
						// See Graphics.exe companion program for snowflake generator.
						g.DrawImageUnscaled(Images.Snowflake[rand.Next(10)], xx - 15 + rand.Next(10), yy - 15 + rand.Next(10));
					}
			g.Dispose();
		}

		public override void Save(BinaryWriter writer)
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			writer.Write((int) terrain);
			int m = 0, n = 0;
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
				{
					if (mountains[x, y])
						m++;
					if (affected[x, y])
						n++;
				}
			writer.Write(m);
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (mountains[x, y])
					{
						writer.Write(x);
						writer.Write(y);
					}
			writer.Write(n);
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (affected[x, y])
					{
						writer.Write(x);
						writer.Write(y);
					}
		}

		public Snow(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			terrain = (TerrainType) reader.ReadInt32();
			int m = reader.ReadInt32();
			mountains = new bool[map.GridSize.Width, map.GridSize.Height];
			for (int i=0; i<m; i++)
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				mountains[x, y] = true;
			}
			int n = reader.ReadInt32();
			affected = new bool[map.GridSize.Width, map.GridSize.Height];
			for (int i=0; i<n; i++)
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				affected[x, y] = true;
			}
		}

		// find adjacent mountain chain in path-finding algorithm
		public bool AdjacentMountainsMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 1;

			// don't go more than three dots (0, 1, 2)
			if (map[x, y].Value > 2)
				return false;

			// only go to mountain spots
			return (map[i, j].Terrain == TerrainType.Mountain || map[i, j].Terrain == TerrainType.Alpine);
		}

		public override void Draw(Graphics g)
		{
			if (bitmap == null)
			{
				// create the shape of the snowstorm
				using (Region region = new Region())
				{
					region.MakeEmpty();
					for (int x=0; x<map.GridSize.Width; x++)
						for (int y=0; y<map.GridSize.Height; y++)
							if (mountains[x, y])
							{
								int xx, yy;
								map.GetCoord(x, y, out xx, out yy);
								using (GraphicsPath path = new GraphicsPath())
								{
									path.AddEllipse(xx - 55, yy - 55, 110, 110);
									region.Union(path);
								}
							}

					// draw the snowstorm
					bitmap = new Bitmap(map.ImageSize.Width, map.ImageSize.Height);
					using (Graphics gr = Graphics.FromImage(bitmap))
					{
						// semi-transparent gray background
						using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
						{
							gr.FillRegion(brush, region);
						}

						// find all mileposts within three units of the affected mountain chain
						for (int x=0; x<map.GridSize.Width; x++)
							for (int y=0; y<map.GridSize.Height; y++)
								if (affected[x, y])
								{
									int xx, yy;
									map.GetCoord(x, y, out xx, out yy);

									// Draw random snowflakes. 
									// See Graphics.exe companion program for snowflake generator.
									gr.DrawImageUnscaled(Images.Snowflake[rand.Next(10)], xx - 15 + rand.Next(10), yy - 15 + rand.Next(10));
								}
					}
				}
			}

			g.DrawImageUnscaled(bitmap, 0, 0);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (bitmap != null)
				bitmap.Dispose();
		}

		~Snow()
		{
			Dispose();
		}

		public override string ToString()
		{
			if (terrain == TerrainType.Alpine)
				return "Severe Winter Storm\rAlpine passes closed";
			else
				return "Severe Winter Storm\rMountains passes closed";
		}

		public override bool AdjustMovementCost(int x, int y, int d, int i, int j, ref int cost)
		{
			// half movement speed
			if (affected[x, y] || affected[i, j])
				cost *= 2;

			// no movement in mountains/alpine, or just alpine
			if (affected[i, j] && (map[i, j].Terrain & terrain) != 0)
				return false;

			return true;
		}

		public override bool AdjustBuildingCost(int x, int y, int d, int i, int j, ref int cost)
		{
			// no building in storm area
			if (affected[i, j] && (map[i, j].Terrain & terrain) != 0)
				return false;

			return true;
		}
	}
}
