
// Derailment.cs

/*
 * This class represents a derailment random event. Affected players
 * lose a turn and a load.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Rails
{
	public class Derailment : Disaster
	{
		const int n = 5;	// the number of cities randomly selected

		private City[] affectedCities;				// the affected cities
		private static Random rand = new Random();	// a random number generator, shared by all instances of this class
		private bool[,] affected;					// indicates which mileposts are near the affected cities

		// create a new derailment
		public Derailment(int player, Map map) : base(player, map)
		{
			affectedCities = new City[n];
			map.ResetFloodMap();

			// choose five unique cities
			int[] m = new int[map.CityCount];
			for (int i=0; i<map.CityCount; i++)
				m[i] = i;
			for (int i=0; i<n; i++)
			{
				int j = rand.Next(map.CityCount - i);
				int temp = m[i];
				affectedCities[i] = map.Cities[m[j]];
				m[j] = temp;
				map.SetValue(affectedCities[i].X, affectedCities[i].Y, 0);
			}

			// calculate the distance to each of the other mileposts
			map.Flood(new FloodMethod(LinearDistanceMethod));

			// determine which mileposts are affected
			affected = new bool[map.GridSize.Width, map.GridSize.Height];
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					affected[x, y] = map[x, y].Value < 4;
		}

		public override void Save(BinaryWriter writer) 
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			for (int i=0; i<affectedCities.Length; i++)
				writer.Write(map[affectedCities[i].X, affectedCities[i].Y].CityIndex);
			int m = 0;
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (affected[x, y])
						m++;
			writer.Write(m);
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (affected[x, y])
					{
						writer.Write(x);
						writer.Write(y);
					}
		}

		public Derailment(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			affectedCities = new City[n];
			for (int i=0; i<n; i++)
				affectedCities[i] = map.Cities[reader.ReadInt32()];
			int m = reader.ReadInt32();
			affected = new bool[map.GridSize.Width, map.GridSize.Height];
			for (int i=0; i<m; i++)
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				affected[x, y] = true;
			}
		}

		public override bool PlayerLosesTurn(Player player)
		{
			return PlayerLosesLoad(player);
		}

		public override bool PlayerLosesLoad(Player player)
		{
			if (player.X == -1)
				return false;

			return affected[player.X, player.Y];
		}

		public override string ToString()
		{
			return "Freak Multiple Wrecks\rDerailments near several cities";
		}

		// display the affected areas in case players forget why they're losing their turn
		public override void Draw(Graphics g)
		{
			int xx, yy;
			int R = 3 * 18 + 5;
			Brush brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.FromArgb(128, Color.Yellow), Color.Transparent);
			foreach (City city in affectedCities)
				if (map.GetCoord(city.X, city.Y, out xx, out yy))
					g.FillEllipse(brush, xx - R, yy - R, 2*R, 2*R);
			brush.Dispose();					
		}
	}
}
