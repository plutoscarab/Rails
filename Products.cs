
// Products.cs

/*
 * Static class containing global data about the commodities.
 * 
 */

using System;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace Rails
{
	public sealed class Products
	{
		static int basicProductCount = 29;

		static StaticStringArray basicProductNames = new StaticStringArray(new string[]{
			"Bauxite", "Beer", "Cars", "Cattle", "Cheese", "China", "Chocolate", 
			"Coal", "Copper", "Cork", "Fish", "Flowers", "Ham", "Hops", "Drugs", 
			"Iron", "Machinery", "Marble", "Oil", "Oranges", "Potatoes", "Spam",
			"Sheep", "Steel", "Tobacco", "Furniture", "Wheat", "Wine", "Wood", 
		});

		static StaticBitmapArray basicProductIcons = GetBasicProductIcons();

		private Products()	// suppress the auto-generated public default constructor
		{
		}

		// load the product icons into memory
		static StaticBitmapArray GetBasicProductIcons()
		{
			Bitmap[] temp = new Bitmap[basicProductCount];
			Assembly a = Assembly.GetExecutingAssembly();
			for (int i=0; i<basicProductCount; i++)
			{
				string name = "Rails.Icons." + basicProductNames[i].ToLower(CultureInfo.InvariantCulture) + ".bmp";
				Stream s = a.GetManifestResourceStream(name);
				temp[i] = new Bitmap(s);
				s.Close();
			}
			return new StaticBitmapArray(temp);
		}

		static bool basicProducts = true;
		static int customProductCount = 0;
		static StaticStringArray customProductNames = null;
		static StaticBitmapArray customProductIcons = null;

		public static void UseStandardProducts()
		{
			basicProducts = true;
			customProductNames = null;
			if (customProductIcons != null)
			{
				customProductIcons.Dispose();
				customProductIcons = null;
			}
		}

		public static void SetProductList(string[] productNames, Bitmap[] productIcons)
		{
			basicProducts = false;
			customProductCount = productNames.Length;
			customProductNames = new StaticStringArray(productNames);
			customProductIcons = new StaticBitmapArray(productIcons);
		}

		// how many products are there?
		public static int Count
		{
			get { return basicProducts ? basicProductCount : customProductCount; }
		}

		// what are they called?
		public static StaticStringArray Name
		{
			get { return basicProducts ? basicProductNames : customProductNames; }
		}

		// the "icons", actually Bitmap not Icon
		public static StaticBitmapArray Icon
		{
			get { return basicProducts ? basicProductIcons : customProductIcons; }
		}
	}

	public class StaticStringArray
	{
		string[] array;

		public StaticStringArray(string[] array)
		{
			this.array = array;
		}

		public string this[int index]
		{
			get { return array[index]; }
		}

		public int Length
		{
			get { return array.Length; }
		}
	}

	public class StaticBitmapArray
	{
		Bitmap[] array;

		public StaticBitmapArray(Bitmap[] array)
		{
			this.array = array;
		}

		public Bitmap this[int index]
		{
			get { return array[index]; }
		}

		public void Dispose()
		{
			foreach (Bitmap bmp in array)
				bmp.Dispose();
		}
	}
}
