
// City.cs

/*
 * Represents a major city, minor city, or town
 * 
 */

using System;
using System.Reflection;
using System.IO;
using System.Collections;

namespace Rails
{
	public enum CityType	// used by Milepost class
	{
		None = 0,		// a milepost not on a city
		Capital,		// the milepost at the center of a major city
		City,			// minor city
		Town,			// town
		CapitalCorner,	// a corner of a major city
	}

	public class City
	{
		private int x;				// the grid location of the city
		private int y;
		private string name;		// the name of the city
		private ArrayList products;		// the commodities available at the city

		public City(int x, int y, string name, ArrayList products)
		{
			this.x = x;
			this.y = y;
			this.name = name;
			this.products = products;
		}

		public int X
		{
			get { return x; }
		}

		public int Y
		{
			get { return y; }
		}

		public string Name
		{
			get { return name; }
		}

		public ArrayList Products
		{
			get { return products; }
		}

		/* Linguistic data for eight countries in Europe. Countries selected were those with the
		 * greatest number of cities with populations of 100 000 or more. See CityNames.txt for
		 * source data derived from http://www.mongabay.com/igapo/European_cities.htm
		 */
		private static Trigram[] trigram = GetTrigrams();

		// Initialization routine to load CityNames.txt
		static Trigram[] GetTrigrams()
		{
			// get the stream of resource text
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("Rails.CityNames.txt");
			StreamReader reader = new StreamReader(stream);

			// initialize the trigram structures
			Trigram[] temp = new Trigram[8];
			for (int i=0; i<8; i++)
			{
				// skip the country name
				/* string country = */ reader.ReadLine();

				// read the city names until a blank line is found
				temp[i] = new Trigram();
				while (true)
				{
					string name = reader.ReadLine();
					if (name == null)
						break;
					if (name.Length == 0)
						break;
					temp[i].Add(name.Trim());
				}
			}
			reader.Close();
			return temp;
		}

		// Choose a random city name for a given country. Use the random number generator
		// from the Map class so that we generate the same names for a given map seed.
		public static string RandomName(int capital, Random rand, Hashtable seen)
		{
			return trigram[capital].GetSample(rand, seen);
		}
	}
}
