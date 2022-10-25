
// Disaster.cs

/*
 * This is the abstract base class for all the random event types.
 *
 */

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Rails
{
	public abstract class Disaster : IDisposable
	{
		// the number of different types of disasters that can occur
		public const int DisasterCount = 20;

		// keep track of which player's turn it was when the disaster occurred
		// so we know when to end the disaster
		public int Player;

		// most disasters have a geographical area they affect, so we need access
		// to the map
		protected Map map;

		protected Disaster(int player, Map map)
		{
			Player = player;
			this.map = map;
		}

		public virtual void Save(BinaryWriter writer)
		{
			writer.Write((int) 0);	// version;
			writer.Write(this.Player);
		}

		public Disaster(BinaryReader reader, Map map)
		{
			int version = reader.ReadInt32();
			this.Player = reader.ReadInt32();
			this.map = map;
		}

		// override this method to affect the game state
		public virtual void AffectState(GameState state)
		{
		}

		// override this method to indicate that a player loses their turn
		public virtual bool PlayerLosesTurn(Player player)
		{
			return false;
		}

		// override this method to indicate the a player loses a load
		public virtual bool PlayerLosesLoad(Player player)
		{
			return false;
		}

		// override this method to provide a visual cue for the disaster
		public virtual void Draw(Graphics g)
		{
		}

		// Override this method for the newspaper headline.
		// Be sure the include the \r between the headline and the subhead.
		public override string ToString()
		{
			return "Widespread Disaster\rEmergency engulfs continent";
		}

		// a flood algorithm method for calculating the distance between mileposts as the crow flies
		public bool LinearDistanceMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 1;
			return true;
		}

		// override this method to affect the speed of movement or to stop movement
		//		x,y = milepost moving from
		//		d = direction of movement
		//		i,j = milepost moving to
		//		cost = number of minutes required to move
		//		return false if can't move
		public virtual bool AdjustMovementCost(int x, int y, int d, int i, int j, ref int cost)
		{
			return true;
		}

		// override this method to affect the cost of building track or to prevent building
		//		x,y = milepost building from
		//		d = direction of track
		//		i,j = milepost building to
		//		cost = cost of building track
		//		return false if can't build
		public virtual bool AdjustBuildingCost(int x, int y, int d, int i, int j, ref int cost)
		{
			return true;
		}

		// override this method to prevent loading/unloading cargo at certain cities
		public virtual bool IsLaborStrike(int city)
		{
			return false;
		}

		// create a random disaster appropriate to the current game state and map
		public static Disaster Create(GameState state, Map map)
		{
			Disaster disaster = null;
			while (disaster == null)
			{
				// choose the type of event
				switch (state.DisasterDeck.Draw())
				{
						// regional strike
					case 0: 
						disaster = new Strike(state.CurrentPlayer, map, true);
						break;
					case 1:
						disaster = new Strike(state.CurrentPlayer, map, false);
						break;

						// company strike
					case 2:
						disaster = new CompanyStrike(state.CurrentPlayer, map, state.PlayerInfo[state.DisasterPlayer.Draw()]);
						break;

						// tax
					case 3:
						if (state.DisableTax)	// no taxes after any player meets victory condition
							continue;
						disaster = new ExcessProfitTax(state.CurrentPlayer, map);
						break;

						// derailment
					case 4: case 5: case 6: case 7: case 8:
						disaster = new Derailment(state.CurrentPlayer, map);
						break;

						// snow
					case 9: 
						disaster = new Snow(state.CurrentPlayer, map, true);
						break;
					case 10: case 11: case 12:
						disaster = new Snow(state.CurrentPlayer, map, false);
						break;

						// fog
					case 13:
						disaster = new Fog(state.CurrentPlayer, map);
						break;

						// flood
					case 14: case 15: case 16:
						if (map.Rivers == null || map.Rivers.Count == 0)
							continue;
						int r = state.DisasterRivers.Draw();
						River river = (River) map.Rivers[r];
						disaster = new Flood(state.CurrentPlayer, map, river);
						break;

						// gale
					case 17: 
						if (map.Seas.Length == 0)
							continue;
						disaster = new Gale(state.CurrentPlayer, map, state.DisasterSeas.Draw(), true);
						break;
					case 18: case 19:
						if (map.Seas.Length == 0)
							continue;
						disaster = new Gale(state.CurrentPlayer, map, state.DisasterSeas.Draw(), false);
						break;
				}
			}
			return disaster;
		}

		public static Disaster CreateInstance(string typeName, BinaryReader reader, Map map)
		{
			Type type = Type.GetType(typeName);
			ConstructorInfo info = type.GetConstructor(new Type[] {typeof(BinaryReader), typeof(Map)});
			Disaster disaster = (Disaster) info.Invoke(new Object[] {reader, map});
//			Disaster disaster = (Disaster) Assembly.GetExecutingAssembly().CreateInstance(
//				typeName, 
//				false, 
//				BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.ExactBinding,
//				null, 
//				new object[] {reader, map}, 
//				System.Globalization.CultureInfo.CurrentCulture, 
//				null);
			return disaster;
		}

		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		~Disaster()
		{
			Dispose();
		}
	}
}