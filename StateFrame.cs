
// StateFrame.cs

/*
 * This class stores the game state information for the current player only,
 * so that the game can be restored to a previous state using the Undo feature.
 * 
 */

using System;

namespace Rails
{
	public class StateFrame
	{
		public string Message;	// reason for the frame (title of Undo menu item)
		Player PlayerInfo;		// current player's info
		int[,,] Track;			// track
		int AccumCost;			// money spent this turn
		int AccumTime;			// time spent this turn
		RandomState ContractRandomState;	// state of the random number generator for new contracts
		bool[] CityIncentive;	// which are incentive cities
		bool[] CityWasVisited;	// for first-to-city bonuses
		int[] Availability;		// for limited commodity option
		bool[] UseTrack;		// which track can be ridden

		// make a copy of the current game state
		public StateFrame(GameState state, string message)
		{
			Message = message;
			PlayerInfo = new Player(state.ThisPlayer);
			Track = (int[,,]) state.Track.Clone();
			AccumCost = state.AccumCost;
			AccumTime = state.AccumTime;
			ContractRandomState = Contract.RandomState;
			CityIncentive = (bool[]) state.CityIncentive.Clone();
			CityWasVisited = (bool[]) state.CityWasVisited.Clone();
			Availability = (int[]) state.Availability.Clone();
			UseTrack = (bool[]) state.UseTrack.Clone();
		}

		// restore the game state from the copy
		public void Restore(GameState state)
		{
			state.PlayerInfo[state.CurrentPlayer] = PlayerInfo;
			state.Track = Track;
			state.AccumCost = AccumCost;
			state.AccumTime = AccumTime;
			Contract.RandomState = ContractRandomState;
			state.CityIncentive = CityIncentive;
			state.CityWasVisited = CityWasVisited;
			state.Availability = Availability;
			state.UseTrack = UseTrack;
		}
	}
}
