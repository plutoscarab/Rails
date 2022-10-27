
// ReversibleRandom.cs

/*
 * This class implements a random number generator identical to System.Random except
 * that you can save and restore the internal state of the generator so that you can
 * restart the random sequence from a particular point. This is used by the undo 
 * feature so that you can't undo cargo delivery and get a different sent of new contracts
 * if you don't like the ones you got.
 *
 */

using System;
using System.IO;

namespace Rails
{
	public struct RandomState
	{
		public int inext;
		public int inextp;
		public int[] SeedArray;
	}

	// Lagged Fibonacci pseudo-random number generator, presumably trying to implement
	// X[n] = (X[n-55] - X[n-24]) mod m. Most of this code comes from using Lutz Roeder's 
	// .NET Reflector on the System.Random class. It seems to have a bug, however. It should 
	// need to keep 55 seed values in order to have n-1 through n-55 available for the next
	// term, but it only uses 54 of the values. It initializes the array with 55 values
	// (strangely using a 1-based array, but the 55th value never gets updated. Still seems 
	// to do the job of a PRNG, however.
	public class ReversibleRandom
	{
		int inext;			// n - 55
		int inextp;			// n - 24
		int[] SeedArray;	// X's

		// use the system clock as the seed
		public ReversibleRandom()
		{
			Constructor(Environment.TickCount);
		}

		// use a specific seed
		public ReversibleRandom(int seed)
		{
			Constructor(seed);
		}

		// read the state of the PRNG from the game save file
		public ReversibleRandom(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			inext = reader.ReadInt32();
			inextp = reader.ReadInt32();
			SeedArray = new int[reader.ReadInt32()];
			for (int i=0; i<SeedArray.Length; i++)
				SeedArray[i] = reader.ReadInt32();
		}

		// write the state to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 0);
			writer.Write(inext);
			writer.Write(inextp);
			writer.Write(SeedArray.Length);
			foreach (int i in SeedArray)
				writer.Write(i);
		}

		// get a copy of the state for the Undo stack
		public RandomState GetState()
		{
			RandomState temp;
			temp.inext = inext;
			temp.inextp = inextp;
			temp.SeedArray = (int[]) SeedArray.Clone();
			return temp;
		}

		// restore the PRNG to a previous state
		public void SetState(RandomState state)
		{
			inext = state.inext;
			inextp = state.inextp;
			SeedArray = state.SeedArray;
		}

		// initialize the seed array
		void Constructor(int seed)
		{
			int num1;
			int num2;
			int num3;
			int num4;
			int num5;
			int num6;
			int[] array1;
			int ptr1;
			this.SeedArray = new int[56];
			num2 = (161803398 - Math.Abs(seed));
			this.SeedArray[55] = num2;
			num3 = 1;
			for (num4 = 1; (num4 < 55); num4 = (num4 + 1))
			{
				num1 = ((21 * num4) % 55);
				this.SeedArray[num1] = num3;
				num3 = (num2 - num3);
				if (num3 < 0)
				{
					num3 = (num3 + 2147483647);
 
				}
				num2 = this.SeedArray[num1];
			}
			for (num5 = 1; (num5 < 5); num5 = (num5 + 1))
			{
				for (num6 = 1; (num6 < 56); num6 = (num6 + 1))
				{
					array1 = this.SeedArray;
					ptr1 = num6;
					this.SeedArray[num6] = (array1[ptr1] - this.SeedArray[(1 + ((num6 + 30) % 55))]);
					if (this.SeedArray[num6] < 0)
					{
						array1 = this.SeedArray;
						ptr1 = num6;
						this.SeedArray[num6] = (array1[ptr1] + 2147483647);
					}
				}
			}
			this.inext = 0;
			this.inextp = 21;
		}

		double Sample()
		{
			int num1;
			int num2;
			int num3;
			num2 = this.inext;
			num3 = this.inextp;
			num2 = (num2 + 1);
			if ((num2 + 1) >= 56)
			{
				num2 = 1;
			}
			num3 = (num3 + 1);
			if ((num3 + 1) >= 56)
			{
				num3 = 1;
			}
			num1 = (this.SeedArray[num2] - this.SeedArray[num3]);
			if (num1 < 0)
			{
				num1 = (num1 + 2147483647);
			}
			this.SeedArray[num2] = num1;
			this.inext = num2;
			this.inextp = num3;
			return (((double) num1) * 4.6566128752457969E-10); 
		}

		public int Next(int maxValue)
		{
			if (maxValue < 0)
			{
				throw new ArgumentOutOfRangeException("maxValue");
			}
			return ((int) (this.Sample() * ((double) maxValue))); 
		}
	}
}
