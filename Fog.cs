
// Fog.cs

/*
 * This class represents the fog random event. It prevents building within
 * the affected area and causes trains to move at half speed.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Rails
{
	public class Fog : Disaster
	{
		private static Random rand = new Random();	// one random number generator shared by all Fog instances
		private City city;			// the affected city
		private bool[,] affected;	// which mileposts are affected

		public Fog(int player, Map map) : base(player, map)
		{
			// choose the city at random
			city = map.Cities[rand.Next(map.CityCount)];

			// calculate the distance from city to other mileposts
			map.ResetFloodMap();
			map.Flood(city.X, city.Y, new FloodMethod(LinearDistanceMethod));

			// determine which mileposts are within affected area
			affected = new bool[map.GridSize.Width, map.GridSize.Height];
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					affected[x, y] = map[x, y].Value < 5;
		}
		
		public override string ToString()
		{
			return "Fog in " + city.Name + "\rTrains slowed, building halted";
		}

		// draw hatch marks over the affected region
		public override void Draw(Graphics g)
		{
			int xx, yy;
			int R = 4 * 18 + 2;
			Brush brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.FromArgb(128, Color.Yellow), Color.Transparent);
			if (map.GetCoord(city.X, city.Y, out xx, out yy))
				g.FillEllipse(brush, xx - R, yy - R, 2*R, 2*R);
			brush.Dispose();					
		}

		// double the time required to move through the fog
		public override bool AdjustMovementCost(int x, int y, int d, int i, int j, ref int cost)
		{
			if (affected[x, y] || affected[i, j])
				cost *= 2;

			return true;
		}

		// do not allow track building in foggy region
		public override bool AdjustBuildingCost(int x, int y, int d, int i, int j, ref int cost)
		{
			if (affected[i, j] || affected[x, y])
				return false;

			return true;
		}
	}
}
