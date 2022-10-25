
// AuthoredMap.cs

/*
 * This class represents a hand-authored map created using the Rails Map Editor or
 * other software.
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Rails
{
	public class AuthoredMap : Map
	{
		public string Name;

		public AuthoredMap(string filename) : base(new Size(864, 712))
		{
			Name = filename;

			System.Globalization.NumberFormatInfo numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

			string path = Path.GetDirectoryName(Application.ExecutablePath);
			FileStream s = new FileStream(path + "\\maps\\" + filename + ".rlm", FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(s);

			int version = r.ReadInt32();
			if (version < 7)
			{
				r.Close();
				s.Close();
				throw new ApplicationException(Resource.GetString("AuthoredMap.TypeNotSupported"));
			}

			// skip thumbnail
			int length = r.ReadInt32();
			s.Seek(length, SeekOrigin.Current);

			// background
			length = r.ReadInt32();
			byte[] b = new Byte[length];
			r.Read(b, 0, length);
			MemoryStream ms = new MemoryStream(b);
			Background = (Bitmap) Bitmap.FromStream(ms);
			b = null;

			// mileposts
			milepost = new Milepost[gridW, gridH];
			for (int i=0; i<gridW; i++)
				for (int j=0; j<gridH; j++)
				{
					milepost[i, j].Terrain = (TerrainType) r.ReadByte();
					milepost[i, j].CityIndex = -1;
					milepost[i, j].CityType = CityType.None;
				}

			// fade level
			int fade = r.ReadInt32();
			if (fade > 0)
			{
				Graphics g = Graphics.FromImage(Background);
				Brush brush = new SolidBrush(Color.FromArgb(fade, Color.White));
				g.FillRectangle(brush, 0, 0, Background.Width, Background.Height);
				brush.Dispose();
				g.Dispose();
			}

			// cities, pass 1
			CityCount = r.ReadInt32();
			Cities = new City[CityCount];
			CityType[] cityType = new CityType[CityCount];
			int productCount;
			NumCapitals = NumCities = NumTowns = 0;
			for (int i=0; i<CityCount; i++)
			{
				/* int cityVersion = */ r.ReadInt32();
				cityType[i] = (CityType) r.ReadByte();
				switch(cityType[i])
				{
					case CityType.Capital:
						NumCapitals++;
						break;
					case CityType.City:
						NumCities++;
						break;
					case CityType.Town:
						NumTowns++;
						break;
				}
				string name = r.ReadString();
				int x = r.ReadInt32();
				int y = r.ReadInt32();
				productCount = r.ReadInt32();
				ArrayList products = new ArrayList(productCount);
				for (int j=0; j<productCount; j++)
					products.Add(r.ReadString());
				City city = new City(x, y, name, products);
				Cities[i] = city;
			}
			Array.Sort(cityType, Cities);

			// products
			productCount = r.ReadInt32();
			ProductSources = new ArrayList[productCount];
			string[] productNames = new String[productCount];
			Bitmap[] productIcons = new Bitmap[productCount];
			Hashtable productIds = new Hashtable();
			for (int i=0; i<productCount; i++)
			{
				ProductSources[i] = new ArrayList(3);
				productNames[i] = r.ReadString();
				productIds[productNames[i]] = i.ToString(numberFormat);
				length = r.ReadInt32();
				b = new byte[length];
				r.Read(b, 0, length);
				ms = new MemoryStream(b);
				productIcons[i] = (Bitmap) Bitmap.FromStream(ms);
				// ms.Close();
			}
			Products.SetProductList(productNames, productIcons);

			// cities, pass 2
			for (int i=0; i<Cities.Length; i++)
			{
				City city = Cities[i];
				milepost[city.X, city.Y].CityIndex = i;
				milepost[city.X, city.Y].CityType = cityType[i];
				if (i < NumCapitals)
				{
					int x, y;
					for (int d=0; d<6; d++)
						if (GetAdjacent(city.X, city.Y, d, out x, out y))
						{
							milepost[x, y].CityIndex = i;
							milepost[x, y].CityType = CityType.CapitalCorner;
						}
				}
				for (int j=0; j<city.Products.Count; j++)
				{
					int id = int.Parse((string) productIds[city.Products[j]], numberFormat);
					city.Products[j] = id;
					ProductSources[id].Add(i);
				}
			}

			// seas
			int count = r.ReadInt32();
			Seas = new Point[count];
			for (int i=0; i<count; i++)
			{
				int x = r.ReadInt32();
				int y = r.ReadInt32();
				Seas[i] = new Point(x, y);
			}

			// rivers
			this.Rivers = new ArrayList();
			if (version >= 8)
			{
				count = r.ReadInt32();
				for (int i=0; i<count; i++)
				{
					string name = r.ReadString();
					int npoints = r.ReadInt32();
					PointF[] rpath = new PointF[npoints];
					for (int j=0; j<npoints; j++)
					{
						float x = r.ReadSingle();
						float y = r.ReadSingle();
						rpath[j] = new PointF(x, y);
					}
					Rivers.Add(new River(i, rpath, name));
				}
			}

			// causeways
			if (version >= 9)
			{
				for (int i=0; i<gridW; i++)
					for (int j=0; j<gridH; j++)
					{
						int i2, j2;
						byte wm = r.ReadByte();
						if ((wm & 1) != 0)
							if (GetAdjacent(i, j, 0, out i2, out j2))
							{
								milepost[i, j].WaterMask |= WaterMasks.InletMask[0];
								milepost[i2, j2].WaterMask |= WaterMasks.InletMask[3];
							}
						if ((wm & 2) != 0)
							if (GetAdjacent(i, j, 1, out i2, out j2))
							{
								milepost[i, j].WaterMask |= WaterMasks.InletMask[1];
								milepost[i2, j2].WaterMask |= WaterMasks.InletMask[4];
							}
						if ((wm & 4) != 0)
							if (GetAdjacent(i, j, 1, out i2, out j2))
							{
								milepost[i, j].WaterMask |= WaterMasks.InletMask[2];
								milepost[i2, j2].WaterMask |= WaterMasks.InletMask[5];
							}
					}
			}

			r.Close();
			s.Close();

			base.LocateBridgeSites();
			base.DrawForeground();

			// temporarily fill inaccessible areas with water to calculate sea disaster areas
			TerrainType[,] save = new TerrainType[gridW, gridH];
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					save[x, y] = milepost[x, y].Terrain;
					if (save[x, y] == TerrainType.Inaccessible)
						milepost[x, y].Terrain = TerrainType.Sea;
				}
			base.InitializeSeaDisasters();
			for (int x=0; x<gridW; x++)
				for (int y=0; y<gridH; y++)
				{
					milepost[x, y].Terrain = save[x, y];
				}
		}
	}
}
