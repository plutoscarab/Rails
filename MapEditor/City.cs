using System;
using System.Collections;
using System.IO;

namespace MapEditor
{
	public enum CityType
	{
		None = 0,		// a milepost not on a city
		Capital,		// the milepost at the center of a major city
		City,			// minor city
		Town,			// town
		CapitalCorner,	// a corner of a major city
	}

	public class City
	{
		static int cityCount = 0;

		public CityType Type;
		public string Name;
		public int X;
		public int Y;
		public ArrayList Products;

		public City(CityType type, int x, int y)
		{
			Type = type;
			X = x;
			Y = y;
			Name = "City " + (++cityCount).ToString();
			Products = new ArrayList();
		}

		public City(CityType type, string name, int x, int y, ArrayList products)
		{
			Type = type;
			Name = name;
			X = x;
			Y = y;
			Products = products;
		}

		public City(BinaryReader r)
		{
			int version = r.ReadInt32();
			Type = (CityType) r.ReadByte();
			Name = r.ReadString();
			X = r.ReadInt32();
			Y = r.ReadInt32();
			int count = r.ReadInt32();
			Products = new ArrayList();
			for (int i=0; i<count; i++)
				Products.Add(r.ReadString());
		}

		public void Write(BinaryWriter w)
		{
			int version = 2;
			w.Write(version);
			w.Write((byte) Type);
			w.Write(Name);
			w.Write(X);
			w.Write(Y);
			w.Write(Products.Count);
			foreach (string product in Products)
			{
				w.Write(product);
			}
		}
	}
}
