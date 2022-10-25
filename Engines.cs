
// Engines.cs

/*
 * Some global data describing locomotive types.
 * 
 */

using System;

namespace Rails
{
	public sealed class Engine
	{
		private static string[] description = 
			{"Steam", "Electric", "Diesel", "Maglev"};

		private static TimeSpan[] speed = 
			{
				new TimeSpan(2, 40, 0),	// steam
				new TimeSpan(2, 0, 0),	// electric
				new TimeSpan(1, 36, 0),	// diesel
				new TimeSpan(1, 20, 0),	// maglev
			};

		private static string[] speedString = 
			{"2h 40m", "2h 00m", "1h 36m", "1h 20m"};

		private static int[] speedInMinutes = 
			{160, 120, 96, 80};

		private Engine()
		{
		}

		public static string[] Description
		{
			get { return description; }
		}

		public static TimeSpan[] Speed
		{
			get { return speed; }
		}

		public static string[] SpeedString
		{
			get { return speedString; }
		}

		public static int[] SpeedInMinutes
		{
			get { return speedInMinutes; }
		}
	}
}