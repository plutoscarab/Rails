using System;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace MapEditor
{
	public class Images
	{
		public static Image Town;
		public static Image City;
		public static Image Capital;
		public static Image Alpine;
		public static Image BlackDot;
		public static Image BlueDot;
		public static Image Mountain;
		public static Image Port;
		public static Image Rose;

		static Assembly assembly;

		static Image LoadImage(string name)
		{
			return Image.FromStream(assembly.GetManifestResourceStream("MapEditor." + name + ".bmp"));
		}

		static Images()
		{
			assembly = Assembly.GetExecutingAssembly();
			Town = LoadImage("Town");
			City = LoadImage("City");
			Capital = LoadImage("Capital");
			Alpine = LoadImage("Alpine");
			BlackDot = LoadImage("BlackDot");
			BlueDot = LoadImage("BlueDot");
			Mountain = LoadImage("Hill");
			Port = LoadImage("Port");
			Rose = LoadImage("Rose");
			assembly = null;
		}

		static Hashtable commodityImages = new Hashtable();

		public static Image CommodityImage(string name)
		{
			if (commodityImages.ContainsKey(name))
				return (Image) commodityImages[name];

			string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
			Image image = new Bitmap(path + "\\commodities\\" + name + ".bmp");
			commodityImages[name] = image;
			return image;
		}

		public static void SubmitStoredImage(string name, Image image)
		{
			commodityImages[name] = image;
			string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
			string filename = path + "\\commodities\\" + name + ".bmp";
			if (File.Exists(filename))
				return;
			image.Save(filename);
		}
	}
}
