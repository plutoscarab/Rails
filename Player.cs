
// Player.cs

/*
 * This class represents the state of a player during a game.
 * 
 */

using System;
using System.IO;

namespace Rails
{
	public class Player
	{
		public const int MaxCars = 4;		// the maximum number of freight cars
		public const int NumContracts = 9;	// the number of contracts

		public Guid ID;						// which player are we?
		public int TrackColor;				// what color is our track?
		public bool Human;					// human or bot?
		public int EngineType;				// type of locomotive
		public int Cars;					// number of freight cars
		public int[] Cargo;					// cargo carried
		public int X;						// location
		public int Y;
		public int D;						// most recent direction of travel
		public Contract[] Contracts;		// demand "cards"
		public int FirstUpgradePoint;		// when computer first upgrades train (funds level)
		public int UpgradePointIncrement;	// how long it waits until next upgrade
		public int NextUpgradePoint;		// when the computer is going to upgrade next
		public bool LoseTurn;				// did we lose a turn due to a disaster?
		public Strategy Strategy;			// the computer player's instructions
		public int DumpCount;				// the number of times the player dumped all contracts

		private int funds;					// how much money we have
		private string name;				// player's name, or null for bot

		public Player(Player player)
		{
			ID = player.ID;
			TrackColor = player.TrackColor;
			Human = true;
			EngineType = player.EngineType;
			Cars = player.Cars;
			Cargo = (int[]) player.Cargo.Clone();
			X = player.X;
			Y = player.Y;
			D = player.D;
			Contracts = (Contract[]) player.Contracts.Clone();
			LoseTurn = player.LoseTurn;
			funds = player.funds;
			name = player.name;
			DumpCount = player.DumpCount;
		}

		// read the player from the game save file
		public Player(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			ID = new Guid(reader.ReadString());
			TrackColor = reader.ReadInt32();
			Human = reader.ReadBoolean();
			EngineType = reader.ReadInt32();
			Cars = reader.ReadInt32();
			Cargo = new int[reader.ReadInt32()];
			for (int i=0; i<Cargo.Length; i++)
				Cargo[i] = reader.ReadInt32();
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			D = reader.ReadInt32();
			Contracts = new Contract[reader.ReadInt32()];
			for (int i=0; i<Contracts.Length; i++)
				Contracts[i] = new Contract(reader);
			FirstUpgradePoint = reader.ReadInt32();
			UpgradePointIncrement = reader.ReadInt32();
			NextUpgradePoint = reader.ReadInt32();
			LoseTurn = reader.ReadBoolean();
			bool b = reader.ReadBoolean();
			if (b)
				Strategy = new Strategy(reader);
			else
				Strategy = null;
			funds = reader.ReadInt32();
			if (version >= 1)
			{
				b = reader.ReadBoolean();
				if (b)
					name = reader.ReadString();
				else
					name = null;
			}
			if (version >= 2)
				DumpCount = reader.ReadInt32();
			else
				DumpCount = 0;
		}

		// write the player to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 2);	// version
			writer.Write(ID.ToString());
			writer.Write(TrackColor);
			writer.Write(Human);
			writer.Write(EngineType);
			writer.Write(Cars);
			writer.Write(Cargo.Length);
			foreach (int cargo in Cargo)
				writer.Write(cargo);
			writer.Write(X);
			writer.Write(Y);
			writer.Write(D);
			writer.Write(Contracts.Length);
			foreach (Contract contract in Contracts)
				contract.Save(writer);
			writer.Write(FirstUpgradePoint);
			writer.Write(UpgradePointIncrement);
			writer.Write(NextUpgradePoint);
			writer.Write(LoseTurn);
			if (Strategy == null)
				writer.Write(false);
			else
			{
				writer.Write(true);
				Strategy.Save(writer);
			}
			writer.Write(funds);
			writer.Write(name != null);
			if (name != null) writer.Write(name);
			writer.Write(DumpCount);
		}

		// create an initialized player
		public Player(int trackColor, bool human, string name)
		{
			ID = Guid.NewGuid();
			TrackColor = trackColor;
			Human = human;
			this.name = name;
		}

		public void Reset(Map map, bool fastStart)
		{
			if (fastStart)
			{
				funds = 70;		// start with 70mil
				EngineType = 1; // and an electric engine
				Cars = 2;		// with two freight cars
				FirstUpgradePoint = 71;
			}
			else
			{
				funds = 50;		// start with 50mil
				EngineType = 0; // and a steam engine
				Cars = 2;		// with two freight cars
				FirstUpgradePoint = 51;
			}

			// empty, of course
			Cargo = new int[MaxCars];
			Cargo[0] = Cargo[1] = Cargo[2] = Cargo[3] = -1;

			// not yet on the map
			X = Y = -1;	D = 1;

			// choose the starting contracts
			InitContracts(map);
			DumpCount = 0;

			// computers always upgrade trains if they have 70mil or more
			NextUpgradePoint = FirstUpgradePoint;
			UpgradePointIncrement = 0;

			LoseTurn = false;
			Strategy = null;
		}

		public void InitContracts(Map map)
		{
			Contracts = new Contract[NumContracts];

			// a bit-mapped field indicating which types of contracts we've already drawn
			ulong mask = 0; 

			// draw the contracts
			for (int j=0; j<NumContracts; j++)
			{
				Contract c = null;
				ulong m = 0;
				while (true)
				{
					// choose a random contract
					c = Contract.Random(map);

					// see if we already have one like this
					m = 1UL << (c.GetHashCode() & 0x3F);

					// if not, it's good
					if ((mask & m) == 0)
						break;
				}

				// mark down that we've draw this type of contract and save it
				mask |= m;
				Contracts[j] = c;
			}
		}

		// find an empty freight car
		public int EmptyCar
		{
			get
			{
				for (int i=0; i<Cars; i++)
					if (Cargo[i] == -1)
						return i;
				return -1;
			}
		}

		// delete a contract and replace it with a new one
		public void DeleteContract(Map map, int i)
		{
			// move all the preceding contracts down one slot
			for (int j=i; j>0; j--)
				Contracts[j] = Contracts[j - 1];

			// mark down which contract types we already have
			ulong mask = 0;
			for (int j=1; j<NumContracts; j++)
				mask |= (1UL << (Contracts[j].GetHashCode() & 0x3F));

			// choose a new contract of a type we don't already have
			while (true)
			{
				Contracts[0] = Contract.Random(map);
				ulong m = 1UL << (Contracts[0].GetHashCode() & 0x3F);
				if ((mask & m) == 0)
					break;
			}
		}

		public int Funds
		{
			get
			{
				return funds;
			}
		}

		// spend money
		public void Spend(int amount)
		{
			// pay back double if we have to borrow
			if (amount > funds)
				amount += amount - (funds > 0 ? funds : 0);
			funds -= amount;
		}

		// receive money
		public void Receive(int amount)
		{
			funds += amount;
		}

		// lose a load selected at random
		public bool LoseLoad(Random rand, GameState state)
		{
			// count how many loads we're carrying
			int n = 0;
			for (int i=0; i<Cars; i++)
				if (Cargo[i] != -1)
					n++;

			if (n > 0)
			{
				// choose one of them at random
				n = rand.Next(n);

				// figure out which one it is
				for (int i=0; i<Cars; i++)
					if (Cargo[i] != -1)
						if (n-- == 0)
						{
							state.ReleaseCommodity(ref Cargo[i]);	// lost it
							return true;
						}
			}
			return false;
		}

		// return the player's "name"
		public string Name
		{
			get
			{
				if (Human && name != null)
					return name;
				else
					return string.Format("{0} {1}", Game.ColorName[TrackColor], Human ? "player" : "bot");
			}
		}

		int[] cargoValues = null;

		private void ComputeCargoValues()
		{
			if (cargoValues == null)
				cargoValues = new int[MaxCars];
			bool[] used = new bool[NumContracts];
			for (int j=0; j<Cars; j++)
			{
				if (Cargo[j] == -1)
					cargoValues[j] = 0;
				else
				{
					int max = int.MinValue;
					int best = -1;
					for (int k=0; k<NumContracts; k++)
						if (!used[k])
							if (Contracts[k].Product == Cargo[j])
								if (Contracts[k].Payoff > max)
								{
									max = Contracts[k].Payoff;
									best = k;
								}
					if (best != -1)
					{
						used[best] = true;
						cargoValues[j] = max;
					}
				}
			}
		}

		public int[] CargoValues
		{
			get
			{
				ComputeCargoValues();
				return cargoValues;
			}
		}
	}
}