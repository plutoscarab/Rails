
// ZField.cs

/*
 * This class generates a random height-field using random midpoint displacement.
 * It's implemented by the diamond-square recursive algorithm. It's not great for 
 * generating good 3D landscapes, but it's very fast and plenty good enough to
 * generate the coastlines for our game.
 * 
 */

using System;

namespace Rails
{
	public class ZField
	{
		int width, height;
		byte[,] field;

		public ZField(byte[,] field)
		{
			this.field = field;
			width = field.GetUpperBound(0) + 1;
			height = field.GetUpperBound(1) + 1;
		}

		public ZField(int width, int height, Random rand) : this(width, height, rand, 2)
		{
		}

		public ZField(int width, int height, Random rand, int magnification)
		{
			int size;		// the width/height of the 2D height field array, which must be a power of 2
			double[,] h;	// the height array values

			this.width = width;		// the actual width and height needed
			this.height = height;

			size = 1;
			while (size < width || size < height)
				size *= 2;

			// allocate the memory
			h = new double[size + 1, size + 1];

			// precalculate the square root of 2
			double sq2 = Math.Sqrt(2);

			// choose random altitudes for the corners
			h[0,  0] = rand.NextDouble();
			h[size, 0] = rand.NextDouble();
			h[0, size] = rand.NextDouble();
			h[size, size] = rand.NextDouble();

			int scale = size;
			double R = size / magnification;
			while (scale > 1)
			{
				R *= 0.6;

				// calculate the square centers
				for (int x=0; x<size; x+=scale)
					for (int y=0; y<size; y+=scale)
					{
						int xm = x + scale / 2;
						int ym = y + scale / 2;
						double dh = R * (rand.NextDouble() - 0.5) * sq2;
						h[xm, ym] = (h[x,y] + h[x+scale,y] + h[x,y+scale] + h[x+scale,y+scale]) / 4.0 + dh;
					}

				// Calculate the diamond centers. The edges need special treatment, which is
				// why we have all the =0 and =size tests.
				for (int x=0; x<=size; x+=scale)
					for (int y=0; y<=size; y+=scale)
					{
						int xm = x + scale / 2;
						int ym = y + scale / 2;
						double dh;
						if (y < size)
						{
							if (x == 0)
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x+scale/2,ym]) / 3.0 + dh;
							}
							else if (x == size)
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x-scale/2,ym]) / 3.0 + dh;
							}
							else
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x-scale/2,ym] + h[x+scale/2,ym]) / 4.0 + dh;
							}
						}
						if (x < size)
						{
							if (y == 0)
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y+scale/2]) / 3.0 + dh;
							}
							else if (y == size)
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y-scale/2]) / 3.0 + dh;
							}
							else
							{
								dh = R * (rand.NextDouble() - 0.5);
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y-scale/2] + h[xm,y+scale/2]) / 4.0 + dh;
							}
						}
					}

				scale /= 2;
			}

			// determine the range of altitudes we've ended up with
			double min = double.MaxValue;
			double max = double.MinValue;
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
				{
					double hh = h[x, y];
					if (hh < min)
						min = hh;
					if (hh > max)
						max = hh;
				}

			// normalize the altitudes into the 0 to 255 range
			double range = 255 / (max - min);
			field = new byte[width, height];
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
				{
					double hh = (h[x,y] - min) * range;
					field[x, y] = (byte) hh;
				}

			h = null; // discard the floating-point samples
		}

		public void Fold()
		{
			for (int y=0; y<height; y++)
				for (int x=0; x<width; x++)
					field[x, y] = (byte) (255 - (Math.Abs(field[x,y]-128) * 256) / 129);
		}

		// an indexer to allow the ZField to be treated just like an array
		public int this[int x, int y]
		{
			get
			{
				return field[x, y];
			}
		}

		// Calculate the altitudes for which given percentiles of the land lie below.
		// For example, GetPercentiles(30, 95) returns two values, 30% of the height field
		// values will be less than the first value returned, and 95% of the height field
		// values will be less than the second value returned.
		public int[] GetPercentiles(params int[] pct)
		{
			// count how many height field samples there are for each possible height value
			int[] hist = new int[256];
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
					hist[field[x, y]]++;

			// calculate the cumulative totals for each height value
			int[] totals = new int[256];
			int t = 0;
			for (int i=0; i<256; i++)
			{
				t += hist[i];
				totals[i] = t;
			}
			
			// look up each result
			int[] result = new int[pct.Length];
			for (int i=0; i<pct.Length; i++)
				result[i] = GetPercentile(totals, t, pct[i]);

			return result;
		}

		static private int GetPercentile(int[] totals, int total, int pct)
		{
			total = (int) (total * pct / 100.0);
			for (int i=0; i<256; i++)
				if (totals[i] > total)
					return i;
			return 256;
		}
	}
}
