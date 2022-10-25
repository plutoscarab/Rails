
// GameState.cs

/*
 * This class stores the entire state of the current game session.
 * Everything is kept in one place to make it easy to save/restore
 * the game and to implement the Undo feature.
 * 
 */

using System;
using System.IO;
using System.Collections;

namespace Rails
{
	public class GameState
	{
		public const int MaxTime = 1440;	// minutes per day
		public const int MaxSpend = 20;		// maximum spend per day
		public const int DefaultFundsGoal = 250;	// money needed to win

		public int NumPlayers;				// how many players are there
		public int Turn;					// what turn is it
		public int CurrentPlayer;			// who's turn is it
		public Player[] PlayerInfo;			// about the players
		public int[,,] Track;				// crayon markings
		public int AccumCost;				// how much money has been spent this turn
		public int AccumTime;				// how much time has been spent this turn
		public int StartingAccumTime;		// how much excess time was passed on from previous day
		public ArrayList Winners;			// who won? null = nobody yet
		public bool[] CityIncentive;		// which cities are incentive cities
		public bool[] CityWasVisited;		// for first-to-city bonuses
		public int[] Availability;			// how many of each type of commodity are available
		public bool[] UseTrack;				// which players' tracks can be ridden
		public double DisasterProbability;	// the probability of a disaster per player turn
		public Deck DisasterDeck;			// disaster events in random order
		public Deck DisasterPlayer;			// players affected by disasters, in random order
		public Deck DisasterSeas;			// seas affected by disasters, in random order
		public Deck DisasterRivers;			// rivers affected by disasters, in random order
		public bool DisableTax;				// disable excess profit tax after someone reaches 250

		// read the game state from the game save file
		public GameState(BinaryReader reader, Map map)
		{
			int version = reader.ReadInt32();
			Turn = reader.ReadInt32();
			NumPlayers = reader.ReadInt32();
			CurrentPlayer = reader.ReadInt32();
			PlayerInfo = new Player[NumPlayers];
			for(int i=0; i<NumPlayers; i++)
				PlayerInfo[i] = new Player(reader);
			AccumCost = reader.ReadInt32();
			AccumTime = reader.ReadInt32();
			int nwinners = reader.ReadInt32();
			if (nwinners != 0)
			{
				Winners = new ArrayList();
				for (int i=0; i<nwinners; i++)
					Winners.Add(reader.ReadInt32());
			}
			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			Track = new int[width, height, 6];
			for (int x=0; x<width; x++)
				for (int y=0; y<height; y++)
				{
					int t = reader.ReadInt32();
					for (int d=5; d>=0; d--)
					{
						Track[x, y, d] = (t & 7) - 1;
						t >>= 3;
					}
				}
			
			int cityCount;
			if (version < 2)
				cityCount = 48;
			else
				cityCount = reader.ReadInt32();
			CityIncentive = new Boolean[cityCount];
			CityWasVisited = new Boolean[cityCount];
			for (int i=0; i<cityCount; i++)
			{
				CityIncentive[i] = reader.ReadBoolean();
				CityWasVisited[i] = reader.ReadBoolean();
			}
			Availability = new int[Products.Count];
			for (int i=0; i<Products.Count; i++)
				Availability[i] = reader.ReadInt32();

			UseTrack = new bool[NumPlayers + 1];
			UseTrack[NumPlayers] = true;	// nationalized track
			if (version >= 1)
			{
				for (int i=0; i<this.NumPlayers; i++)
					UseTrack[i] = reader.ReadBoolean();
			}
			else
				UseTrack[CurrentPlayer] = true;

			if (version >= 3)
			{
				DisasterProbability = reader.ReadDouble();
				DisasterDeck = new Deck(reader);
				DisasterPlayer = new Deck(reader);
				DisasterSeas = new Deck(reader);
				DisasterRivers = new Deck(reader);
				DisableTax = reader.ReadBoolean();
			}
			else
			{
				DisasterProbability = 0.0567;
				DisasterDeck = new Deck(Disaster.DisasterCount);
				DisasterPlayer = new Deck(NumPlayers);
				DisasterSeas = new Deck(map.Seas.Length);
				DisasterRivers = new Deck(map.Rivers.Count);
				// DisableTax = false;
			}

			if (version == 4 || version == 5)
			{
				// Journal data
				throw new ApplicationException(Resource.GetString("GameState.VersionNotSupported"));
			}

			if (version >= 5)
				StartingAccumTime = reader.ReadInt32();
			else
				StartingAccumTime = 0;
		}

		// write the game state to the game save file
		public void Save(BinaryWriter writer, Map map)
		{
			writer.Write((int) 6);	// version
			writer.Write(Turn);
			writer.Write(NumPlayers);
			writer.Write(CurrentPlayer);
			foreach (Player player in PlayerInfo)
				player.Save(writer);
			writer.Write(AccumCost);
			writer.Write(AccumTime);
			if (Winners == null)
				writer.Write((int) 0);
			else
			{
				writer.Write(Winners.Count);
				foreach (object winner in Winners)
					writer.Write((int) winner);
			}
			writer.Write(Track.GetLength(0));
			writer.Write(Track.GetLength(1));
			for (int x=0; x<Track.GetLength(0); x++)
				for (int y=0; y<Track.GetLength(1); y++)
				{
					int t = 0;
					for (int d=0; d<6; d++)
						t = (t << 3) | (Track[x, y, d] + 1);
					writer.Write(t);
				}
			writer.Write(map.CityCount);
			for (int i=0; i<map.CityCount; i++)
			{
				writer.Write(CityIncentive[i]);
				writer.Write(CityWasVisited[i]);
			}
			for (int i=0; i<Products.Count; i++)
				writer.Write(Availability[i]);
			for (int i=0; i<this.NumPlayers; i++)
				writer.Write(UseTrack[i]);
			writer.Write(DisasterProbability);
			DisasterDeck.Save(writer);
			DisasterPlayer.Save(writer);
			DisasterSeas.Save(writer);
			DisasterRivers.Save(writer);
			writer.Write(DisableTax);
			writer.Write(StartingAccumTime);
		}

		// initialize the game state
		public GameState(Player[] playerList, Map map, Options options, Random rand)
		{
#if TEST
			if (playerList == null)
			{
				playerList = new Player[3];
				playerList[0] = new Player(0, false, null);
				playerList[1] = new Player(1, false, null);
				playerList[2] = new Player(2, false, null);
			}
#endif
			NumPlayers = playerList.Length;
			// CurrentPlayer = 0;
			PlayerInfo = playerList;

			foreach (Player player in PlayerInfo)
				player.Reset(map, options.FastStart);

			Track = new int[map.GridSize.Width, map.GridSize.Height, 6];
			for (int x=0; x<Track.GetLength(0); x++)
				for (int y=0; y<Track.GetLength(1); y++)
					for (int d=0; d<Track.GetLength(2); d++)
						Track[x, y, d] = -1;

			// AccumCost = 0;
			// AccumTime = 0;
			// Winners = null;

			CityIncentive = new bool[map.CityCount];
			CityWasVisited = new bool[map.CityCount];
			if (options.CityIncentives)
			{
				int n = 0;
				while (n < 8)
				{
					int i = rand.Next(map.NumCapitals, map.CityCount - 1);
					if (!CityIncentive[i]) n++;
					CityIncentive[i] = true;
				}
			}

			Availability = new int[Products.Count];
			int navail;
			if (options.LimitedCommodities)
				navail = (NumPlayers + 1) / 2;
			else
				navail = 1000;
			for (int i=0; i<Products.Count; i++)
				Availability[i] = navail;

			UseTrack = new bool[NumPlayers + 1];
			UseTrack[CurrentPlayer] = true;
			UseTrack[NumPlayers] = true;	// nationalized track

			// DisasterProbability = 0.0;
			DisasterDeck = new Deck(Disaster.DisasterCount);
			DisasterPlayer = new Deck(NumPlayers);
			DisasterSeas = new Deck(map.Seas.Length);
			DisasterRivers = new Deck(map.Rivers.Count);

			Turn = 1;
		}

		public Player ThisPlayer
		{
			get { return PlayerInfo[CurrentPlayer]; }
		}

		public void MovePlayerTo(int x, int y)
		{
			PlayerInfo[CurrentPlayer].X = x;
			PlayerInfo[CurrentPlayer].Y = y;
		}

		public void NextPlayer()
		{
			if (!ThisPlayer.LoseTurn)
			{
				ThisPlayer.ExcessTime = AccumTime - MaxTime;
				if (ThisPlayer.ExcessTime < 0)
					ThisPlayer.ExcessTime = 0;
			}
			ThisPlayer.LoseTurn = false;
			CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
			if (CurrentPlayer == 0)
			{
				Turn++;
			}
			AccumCost = 0;
			StartingAccumTime = AccumTime = ThisPlayer.ExcessTime;
			for (int i=0; i<NumPlayers; i++)
				UseTrack[i] = (i == CurrentPlayer);
		}

		public int ReserveCommodity(int i)
		{
			Availability[i]--;
			return i;
		}

		public void ReleaseCommodity(ref int i)
		{
			Availability[i]++;
			i = -1;
		}

		public bool NoMoreTime
		{
			get
			{
				return this.AccumTime >= MaxTime;
			}
		}
	}
}
