
// Milepost.cs

/*
 * This class represents map data for a single milepost. Note that session-specific
 * data such as tracks connecting to the milepost are not included.
 * 
 */

using System;

namespace Rails
{
	[Flags]
	public enum TerrainType
	{
		Inaccessible = 1,	// mileposts removed from the map because they're not accessible
		Sea = 2,			
		Clear = 4,			
		Mountain = 8,
		Alpine = 16,
		Port = 32,
	}

	// the WaterMask field of the Milepost class is a bit-mapped field indicating which
	// of the six directions away from the milepost cross either a river or other water
	public class WaterMasks
	{
		public static readonly StaticIntArray RiverMask = new StaticIntArray(new int[]{1, 2, 4, 8, 16, 32});
		public static readonly StaticIntArray InletMask = new StaticIntArray(new int[]{64, 128, 256, 512, 1024, 2048});
	}

	public class StaticIntArray
	{
		int[] array;

		public StaticIntArray(int[] array)
		{
			this.array = array;
		}

		public int this[int index]
		{
			get
			{
				return array[index];
			}
		}
	}

	public struct Milepost
	{
		public TerrainType Terrain;		// which type of terrain
		public CityType CityType;		// which type of city
		public int CityIndex;			// which city
		public int Capital;				// which country
		public int WaterMask;			// bits 0-5: rivers, bits 6-11: inlets
		public int SeaIndex;			// which sea
		public long[] RiversCrossed;	// which rivers crossed in each direction
		public byte[] DistanceFromSea;	// how far from each sea

		// path-finding or distance-calculating data
		public int Value;
		public int Gradient;

		// determine how far inland the milepost is by seeing how far
		// it is to the nearest sea
		public int DistanceInland
		{
			get
			{
				if (DistanceFromSea == null) 
					return int.MaxValue;
				byte min = byte.MaxValue;
				foreach (byte d in DistanceFromSea)
					if (d < min)
						min = d;
				return min;
			}
		}
	}
}
