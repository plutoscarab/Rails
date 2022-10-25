
// Tax.cs

/*
 * This class represents the Excess Profit Tax random event, which taxes everyone except
 * the last-place player (by cash-on-hand).
 * 
 */

using System;
using System.IO;

namespace Rails
{
	public class ExcessProfitTax : Disaster
	{
		private int minFunds;	// the amount of money that the poorest player has (but not less than zero)

		public ExcessProfitTax(int player, Map map) : base(player, map)
		{
			minFunds = int.MaxValue;
		}

		public override void Save(BinaryWriter writer)
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			writer.Write(minFunds);
		}

		public ExcessProfitTax(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			minFunds = reader.ReadInt32();
		}

		// calculate tax and reduce players' funds
		public override void AffectState(GameState state)
		{
			// find poorest player's cash
			minFunds = int.MaxValue;
			for (int i=0; i<state.NumPlayers; i++)
				if (state.PlayerInfo[i].Funds < minFunds)
					minFunds = state.PlayerInfo[i].Funds;
			if (minFunds < 0)
				minFunds = 0;

			// calculate excess funds and pay 20% tax
			for (int i=0; i<state.NumPlayers; i++)
			{
				int excessFunds = state.PlayerInfo[i].Funds - minFunds;
				if (excessFunds > 0)
					state.PlayerInfo[i].Spend(excessFunds / 5);
			}
		}

		public override string ToString()
		{
			return "Excess Profit Tax\rPay 20% of funds over " + Utility.CurrencyString(minFunds);
		}
	}
}
