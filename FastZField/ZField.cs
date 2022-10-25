using System;
using System.Drawing;

namespace FastZField
{
	class Class1
	{
		[STAThread]
		static void Main(string[] args)
		{
			ZField zfield = new ZField(1024, 768);
		}
	}

	public class ZField
	{
		byte[,] field;

		public ZField(int width, int height)
		{
			int size;
			double[,] h;
			Random rand;

			size = 1;
			while (size < width || size < height)
				size *= 2;

			h = new double[size + 1, size + 1];
			rand = new Random();
			double sq2 = Math.Sqrt(2);

			h[0,  0] = rand.NextDouble();
			h[size, 0] = rand.NextDouble();
			h[0, size] = rand.NextDouble();
			h[size, size] = rand.NextDouble();

			int scale = size;
			while (scale > 1)
			{
				for (int x=0; x<size; x+=scale)
					for (int y=0; y<size; y+=scale)
					{
						int xm = x + scale / 2;
						int ym = y + scale / 2;
						double dh = (rand.NextDouble() - 0.5) * scale / sq2 / size;
						h[xm, ym] = (h[x,y] + h[x+scale,y] + h[x,y+scale] + h[x+scale,y+scale]) / 4.0 + dh;
					}

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
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x+scale/2,ym]) / 3.0 + dh;
							}
							else if (x == size)
							{
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x-scale/2,ym]) / 3.0 + dh;
							}
							else
							{
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[x, ym] = (h[x,y] + h[x,y+scale] + h[x-scale/2,ym] + h[x+scale/2,ym]) / 4.0 + dh;
							}
						}
						if (x < size)
						{
							if (y == 0)
							{
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y+scale/2]) / 3.0 + dh;
							}
							else if (y == size)
							{
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y-scale/2]) / 3.0 + dh;
							}
							else
							{
								dh = (rand.NextDouble() - 0.5) * scale / 2 / size;
								h[xm, y] = (h[x,y] + h[x+scale,y] + h[xm,y-scale/2] + h[xm,y+scale/2]) / 4.0 + dh;
							}
						}
					}

				scale /= 2;
			}

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

			double range = 255 / (max - min);
			field = new byte[width, height];
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
				{
					double hh = (h[x,y] - min) * range;
					field[x, y] = (byte) hh;
				}

			h = null;
		}

		public int this[int x, int y]
		{
			get
			{
				return field[x, y];
			}
		}
	}
}
