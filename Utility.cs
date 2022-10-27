
// Utility.cs

/*
 * This class contains a global method for drawing outlined text to give the text more contrast
 * with its background. It's here instead of somewhere else because it's used by multiple parts of the
 * program.
 * 
 */

using System;
using System.Drawing;

namespace Rails
{
	public sealed class Utility
	{
		static string currencyFormat = Resource.GetString("Utility.CurrencyFormat");

		private Utility()
		{
		}

		public static void DrawStringOutlined(Graphics g, string s, Font font, Brush brush, Brush outline, int x, int y)
		{
			for (int dx=-1; dx<=1; dx++)
				for (int dy=-1; dy<=1; dy++)
					if ((dx | dy) != 0)
						g.DrawString(s, font, outline, x + dx, y + dy);
			g.DrawString(s, font, brush, x, y);
		}

		public static Color BlendColors(Color color1, int weight1, Color color2, int weight2)
		{
			int totalWeight = weight1 + weight2;
			int a = (color1.A * weight1 + color2.A * weight2) / totalWeight;
			int r = (color1.R * weight1 + color2.R * weight2) / totalWeight;
			int g = (color1.G * weight1 + color2.G * weight2) / totalWeight;
			int b = (color1.B * weight1 + color2.B * weight2) / totalWeight;
			return Color.FromArgb(a, r, g, b);
		}

		public static string CurrencyString(double amount)
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, currencyFormat, Math.Round(amount));
		}
	}
}
