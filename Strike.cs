
// Strike.cs

/*
 * This class represents a coastal or inland labor strike random event.
 * 
 */

using System;
using System.Drawing;

namespace Rails
{
	public class Strike : Disaster
	{
		private bool coastal;			// or inland
		private bool[] affectedCities;	// which cites are in affected area

		public Strike(int player, Map map, bool coastal) : base(player, map)
		{
			this.coastal = coastal;

			// store them in an array for later
			affectedCities = new bool[map.CityCount];
			for (int i=0; i<map.CityCount; i++)
			{
				City city = map.Cities[i];
				if ((map[city.X, city.Y].DistanceInland < 4) == coastal)
					affectedCities[i] = true;
			}
		}

		public override bool IsLaborStrike(int city)
		{
			return affectedCities[city];
		}

		public override void Draw(Graphics g)
		{
			Brush brush = new SolidBrush(Color.FromArgb(128, Color.Yellow));
			int r = 40;
			int x, y;
			for (int i=0; i<map.CityCount; i++)
				if (affectedCities[i])
				{
					City city = map.Cities[i];
					if (map.GetCoord(city.X, city.Y, out x, out y))
						g.FillEllipse(brush, x - r, y - r, 2 * r, 2 * r);
				}
			brush.Dispose();
		}

		public override string ToString()
		{
			if (coastal)
				return "Dock Workers Strike\rUnrest paralyzes coastal cities";
			else
				return "General Strike\rUnrest paralyzes inland cities";
		}
	}
}
