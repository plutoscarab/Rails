
// CompanyStrike.cs

/*
 * This class represents a labor strike against a specific player.
 * Since this game doesn't support riding other players' track, the
 * only effect is that the player loses a turn.
 * 
 */

using System;

namespace Rails
{
	public class CompanyStrike : Disaster
	{
		Player affected;

		public CompanyStrike(int currentPlayer, Map map, Player affectedPlayer) : base(currentPlayer, map)
		{
			affected = affectedPlayer;
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