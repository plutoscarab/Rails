
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
	[Serializable]
	public struct Options
	{
		public bool AutomaticTrackBuilding;
		public bool FastStart;
		public bool CityIncentives;
		public bool LimitedCommodities;
		public bool FirstToCityBonuses;
		public bool GroupedContracts;
		public int FundsGoal;

		public Options(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			AutomaticTrackBuilding = reader.ReadBoolean();
			FastStart = reader.ReadBoolean();
			CityIncentives = reader.ReadBoolean();
			LimitedCommodities = reader.ReadBoolean();
			FirstToCityBonuses = reader.ReadBoolean();
			if (version >= 1)
				GroupedContracts = reader.ReadBoolean();
			else
				GroupedContracts = false;
			if (version >= 2)
				FundsGoal = reader.ReadInt32();
			else
				FundsGoal = GameState.DefaultFundsGoal;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 2); // version
			writer.Write(AutomaticTrackBuilding);
			writer.Write(FastStart);
			writer.Write(CityIncentives);
			writer.Write(LimitedCommodities);
			writer.Write(FirstToCityBonuses);
			writer.Write(GroupedContracts);
			writer.Write(FundsGoal);
		}

		public override string ToString()
		{
			return AutomaticTrackBuilding.ToString() + ":" + FastStart.ToString() + ":"
				+ CityIncentives.ToString() + ":" + LimitedCommodities.ToString() + ":"
				+ FirstToCityBonuses.ToString() + (GroupedContracts ? ":" + GroupedContracts.ToString() : "")
				+ (FundsGoal == GameState.DefaultFundsGoal ? "" : ":" + FundsGoal.ToString());
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
			if (GroupedContracts)
				b.Append("Gc");
			if (FundsGoal != GameState.DefaultFundsGoal)
				b.Append(FundsGoal.ToString());
			if (b.Length == 0)
				b.Append("None");
			return b.ToString();
		}
	}
}
