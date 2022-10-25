
// Gale.cs

/*
 * This class represents a storm at sea which affects coastal regions.
 * 
 */

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Rails
{
	public class Gale : Disaster, IDisposable
	{
		public int Sea;			// which sea is affected

		private bool major;		// one-third of gales are major gales
		private int dist;		// distance inland affected by the gale
		private Bitmap bitmap;	// the image of the gale

		public Gale(int player, Map map, int sea, bool major) : base(player, map)
		{
			Sea = sea;
			this.major = major;

			dist = major ? 6 : 4;		// major gales reach 6 dots inland, minor only 4
		}

		public override void Save(BinaryWriter writer)
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			writer.Write(Sea);
			writer.Write(major);
			writer.Write(dist);
		}

		public Gale(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			Sea = reader.ReadInt32();
			major = reader.ReadBoolean();
			dist = reader.ReadInt32();
		}

		public override bool PlayerLosesTurn(Player player)
		{
			return PlayerLosesLoad(player);
		}

		public override bool PlayerLosesLoad(Player player)
		{
			// only major gales cause players to lose a turn and a load
			if (!major)
				return false;

			// if the train hasn't been placed on the map, they're safe
			if (player.X == -1)
				return false;

			// if they're currently floating around on the water, they're dead
			if (map[player.X, player.Y].SeaIndex == Sea)
				return true;

			// if they're at a port on this sea, they're dead
			if (map.IsPort(player.X, player.Y) && map[player.X, player.Y].DistanceFromSea[Sea] == 1)
				return true;

			// otherwise they're OK
			return false;
		}

		public override string ToString()
		{
			if (major)
				return "Severe Coastal Storms\rRail movement/building affected";
			else
				return "Coastal Storms\rRail movement/building affected";
		}

		public override bool AdjustMovementCost(int x, int y, int d, int i, int j, ref int cost)
		{
			// if they're travelling on this sea, they can't move
			if (map[x, y].SeaIndex == Sea || map[i, j].SeaIndex == Sea)
				return false;

			// if they're travelling on the coast near this sea, they move at half speed
			if (map[x, y].DistanceFromSea[Sea] <= dist || map[i, j].DistanceFromSea[Sea] <= dist)
				cost *= 2;

			// otherwise they're OK
			return true;
		}

		public override bool AdjustBuildingCost(int x, int y, int d, int i, int j, ref int cost)
		{
			// can't build near the coastline during a gale
			if (map[x, y].DistanceFromSea[Sea] <= dist || map[i, j].DistanceFromSea[Sea] <= dist)
				return false;

			return true;
		}

		// draw the hatch marks we created earlier
		public override void Draw(Graphics g)
		{
			if (bitmap == null)
			{
				int R = 18 * dist - 13;

				// create the region to fill with hatch marks
				using (Region region = new Region())
				{
					region.MakeEmpty();
					for (int x=0; x<map.GridSize.Width; x++)
						for (int y=0; y<map.GridSize.Height; y++)
							if (map[x, y].DistanceFromSea[Sea] == 1) // find all mileposts on coastline
							{
								int xx, yy;
								map.GetCoord(x, y, out xx, out yy);

								// draw overlapping circles all along the coastline
								using (GraphicsPath path = new GraphicsPath())
								{
									path.AddEllipse(xx - R, yy - R, 2*R, 2*R);
									region.Union(path);
								}
							}

					// fill the region with gale pattern
					bitmap = new Bitmap(map.ImageSize.Width, map.ImageSize.Height);
					using (Graphics gr = Graphics.FromImage(bitmap))
					{
						using (Brush brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.FromArgb(128, Color.Yellow), Color.Transparent))
						{
							gr.FillRegion(brush, region);
						}
					}
				}
			}

			g.DrawImageUnscaled(bitmap, 0, 0);
		}

		// we have to dispose of the bitmap when we're done
		public override void Dispose()
		{
			base.Dispose();
			if (bitmap != null)
				bitmap.Dispose();
		}

		~Gale()
		{
			Dispose();
		}
	}
}
