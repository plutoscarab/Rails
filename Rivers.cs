
// Rivers.cs

/*
 * Implements a class for loading river name linguistic data, generating random river
 * names, and a class for representing the path of a river on the map.
 * 
 */

using System;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Collections;

namespace Rails
{
	public class Rivers
	{
		// linguistic patterns derived from the names of the world's largest rivers (RiverNames.txt)
		static Trigram trigram;

		// load the river name data
		static Rivers()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("Rails.RiverNames.txt");
			StreamReader reader = new StreamReader(stream);
			trigram = new Trigram();
			string s;
			while ((s = reader.ReadLine()) != null)
			{
				trigram.Add(s);
			}
			reader.Close();
			stream.Close();
		}

		// Generate a random river name. Use the same random number generator from Map class
		// so that when a map is regenerated from a given seed, the river names come out the same.
		public static string GetSampleName(Random rand, Hashtable riverNames)
		{
			return trigram.GetSample(rand, riverNames);
		}
	}

	// A river, including the name and the path. The river names don't appear on the map but
	// the appear in notices of the Flood disaster.
	public class River
	{
		public int ID;
		public PointF[] Path;
		public string Name;

		public River(int id, PointF[] path, string name)
		{
			ID = id;
			Path = path;
			Name = name;
		}

		public River(int id, PointF[] path, Random rand, Hashtable riverNames)
		{
			ID = id;
			Path = path;
			Name = Rivers.GetSampleName(rand, riverNames);
		}
	}
}
