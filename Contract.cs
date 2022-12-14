
// Contract.cs

/*
 * This class represents a demand for a specific commodity from a specific city.
 * The payoff for the contract is calculated as the linear travelling distance between
 * the destination city and the closest source city for the product. Terrain type
 * is ignored.
 * 
 */

using System;
using System.IO;
using System.Runtime.Serialization;

namespace Rails
{
	[Serializable]
	public class Contract
	{
		public int Product;			// the commodity
		public int Destination;		// the demanding city
		public int Payoff;			// the amount paid upon delivery

		// We need to use a reversible random number generator so that if
		// the player uses "undo" to undo a delivery we ensure that we generate
		// the same new contracts if they redo the delivery.
		private static ReversibleRandom rand = new ReversibleRandom();

		// load the contract from the game save file
		public Contract(BinaryReader reader)
		{
			/* int version = */ reader.ReadInt32();
			Product = reader.ReadInt32();
			Destination = reader.ReadInt32();
			Payoff = reader.ReadInt32();
		}

		// write the contract to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 0);	// version
			writer.Write(Product);
			writer.Write(Destination);
			writer.Write(Payoff);
		}

		// create a new contract and calculate the payoff amount
		public Contract(int product, int destination, Map map)
		{
			Product = product;
			Destination = destination;

			// find the closest source for the product
			Payoff = int.MaxValue;
			foreach (int source in map.ProductSources[product])
			{
				int dist = map.CityDistance(source, destination);
				if (dist < Payoff)
					Payoff = dist;
			}
		}

		public static void ReadRandom(BinaryReader reader)
		{
			rand = new ReversibleRandom(reader);
		}

		public static void SaveRandom(BinaryWriter writer)
		{
			rand.Save(writer);
		}

		public static RandomState RandomState
		{
			get { return rand.GetState(); }
			set { rand.SetState(value); }
		}

		// randly generate a new contract
		public static Contract Random(Map map)
		{
			return Random(map, -1);
		}

		// randomly generate a new contract with a specific destination city size
		public static Contract Random(Map map, int citySize)
		{
			int product = -1;
			int destination = -1;

			if (citySize == -1)
				citySize = rand.Next(3);

			if (citySize < 0 || citySize > 2)
				throw new ArgumentOutOfRangeException("citySize");

			while (true)
			{
				// pick a product at random
				product = rand.Next(Products.Count);

				// one third of demands are major cities, one third are minor cities,
				// and one third are towns
				switch (citySize)
				{
					case 0:
						destination = rand.Next(map.NumCapitals);
						break;
					case 1:
						destination = map.NumCapitals + rand.Next(map.NumCities);
						break;
					case 2:
						destination = map.NumCapitals + map.NumCities + rand.Next(map.NumTowns);
						break;
				}

				// make sure the destination is not also a supplier of the product
				if (!map.ProductSources[product].Contains(destination))
					break;
			}
			return new Contract(product, destination, map);
		}

		// provide hashtable related methods
		public override int GetHashCode()
		{
			return Product + Products.Count * Destination;
		}

		public override bool Equals(object obj)
		{
			Contract c = obj as Contract;
			if (c == null) return false;
			return Product == c.Product && Destination == c.Destination;
		}
	}
}
