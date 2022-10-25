
// Options.cs

/*
 * This structure describes the game variation selections made in the new game dialog.
 * 
 */

using System;
using System.IO;
using System.Text;

namespace Rails
{
	public struct Options
	{
		public bool AutomaticTrackBuilding;
		public bool FastStart;
		public bool CityIncentives;
		public bool LimitedCommodities;
		public bool FirstToCityBonuses;

		public Options(BinaryReader reader)
		{
			/* int version = */ reader.ReadInt32();
			AutomaticTrackBuilding = reader.ReadBoolean();
			FastStart = reader.ReadBoolean();
			CityIncentives = reader.ReadBoolean();
			LimitedCommodities = reader.ReadBoolean();
			FirstToCityBonuses = reader.ReadBoolean();
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 0); // version
			writer.Write(AutomaticTrackBuilding);
			writer.Write(FastStart);
			writer.Write(CityIncentives);
			writer.Write(LimitedCommodities);
			writer.Write(FirstToCityBonuses);
		}

		public override string ToString()
		{
			return AutomaticTrackBuilding.ToString() + ":" + FastStart.ToString() + ":"
				+ CityIncentives.ToString() + ":" + LimitedCommodities.ToString() + ":"
				+ FirstToCityBonuses.ToString();
		}

		public string ToString(bool abbreviate)
		{
			if (!abbreviate)
				return ToString();

			StringBuilder b = new StringBuilder();
			if (AutomaticTrackBuilding)
				b.Append("At");
			if (FastStart)
				b.Append("Fs");
			if (CityIncentives)
				b.Append("Ci");
			if (LimitedCommodities)
				b.Append("Lc");
			if (FirstToCityBonuses)
				b.Append("Fc");
			if (b.Length == 0)
				b.Append("None");
			return b.ToString();
		}
	}
}
