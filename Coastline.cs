
// Coastline.cs

/*
 * This class calculates the distance from each land milepost to a given sea 
 * and locates all the sea mileposts for a given sea. The class itself is not
 * retained as data.
 * 
 */

using System;

namespace Rails
{
	public class Coastline
	{
		private Map map;

		public Coastline(int sea, Map map)
		{
			this.map = map;

			// start flood algorithm at known point for the sea
			map.ResetFloodMap();
			map.Flood(map.Seas[sea].X, map.Seas[sea].Y, new FloodMethod(GaleFloodMethod));

			// store results
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
				{
					// store the distance to each milepost from this sea
					map.SetDistanceFromSea(x, y, sea, (byte) map[x, y].Value);

					// store the index of this sea
					if (map[x, y].Value == 0)
						map.SetSeaIndex(x, y, sea);
				}
		}

		// Increment the flood algorithm value by one for each milepost across land
		// but keep it the same throughout a sea
		bool GaleFloodMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;
			if (map.IsLand(i, j))
				cost = 1;
			else 
				cost = 0;
			return true;
		}
	}
}
