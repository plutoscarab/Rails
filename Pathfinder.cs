
using System;
using System.Collections;

namespace Rails
{
	public delegate bool PathfinderMethod(Pathfinder f, int x, int y, int d, int i, int j, out IComparable val);

	public class Pathfinder
	{
		Map map;
		int gridW, gridH;
		bool[,] b;
		IComparable[,] val;
		int[,] gradient;

		public Pathfinder(Map map, IComparable initialValue)
		{
			this.map = map;
			gridW = map.GridSize.Width;
			gridH = map.GridSize.Height;
			b = new Boolean[gridW, gridH];
			val = new IComparable[gridW, gridH];
			gradient = new int[gridW, gridH];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					val[x, y] = initialValue;
					gradient[x, y] = -1;
				}
		}

		public void SetValue(int x, int y, IComparable val)
		{
			this.val[x, y] = val;
			b[x, y] = true;
		}

		public IComparable GetValue(int x, int y)
		{
			return this.val[x, y];
		}

		public void Find(PathfinderMethod method, IsValidSite isValidSite)
		{
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
							IComparable cost;
							for (int d=0; d<6; d++)
								if (map.GetAdjacent(x, y, d, out i, out j))
									if (map[i, j].Terrain != TerrainType.Inaccessible)
										if (isValidSite == null || isValidSite(i, j))
											// call the pathfinder method delegate
											if (method(this, x, y, d, i, j, out cost))
											{
												// if the cost is at least as good as previously found, 
												// store the result and mark as interesting for the
												// next loop
												if (cost.CompareTo(val[i, j]) < 0)
												{
													val[i, j] = cost;
													gradient[i, j] = d;
													b2[i, j] = true;
													done = false;
												}
											}
						}

				// hand off interesting spots for next loop
				b = b2;
			}
			while (!done);
		}

		public Stack GetGradientStack(int x, int y)
		{
			Stack stack = new Stack();
			while (gradient[x, y] != -1)
			{
				stack.Push(gradient[x, y]);
				map.GetAdjacent(x, y, (gradient[x, y] + 3) % 6, out x, out y);
			}
			return stack;
		}
	}
}