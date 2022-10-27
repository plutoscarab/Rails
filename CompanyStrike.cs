
// CompanyStrike.cs

/*
 * This class represents a labor strike against a specific player.
 * The only effect is that the player loses a turn.
 * 
 */

using System;
using System.IO;

namespace Rails
{
	public class CompanyStrike : Disaster
	{
		Player affected;

		public CompanyStrike(int currentPlayer, Map map, Player affectedPlayer) : base(currentPlayer, map)
		{
			affected = affectedPlayer;
		}

		public override void Save(BinaryWriter writer)
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			affected.Save(writer);
		}

		public CompanyStrike(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			affected = new Player(reader);
		}

		public override bool PlayerLosesTurn(Player player)
		{
			return player.ID == affected.ID;
		}

		public override string ToString()
		{
			return Game.ColorName[affected.TrackColor] + " Workers Strike\rNo movement or building";
		}
	}
}