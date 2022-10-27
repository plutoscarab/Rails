
// Strategy.cs

/*
 * This class stores the sequence of instructs for a computer player to carry out. 
 * The actual strategy logic is in Game.cs--this class just stores the result.
 * 
 */

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

namespace Rails
{
	// one step in the series of instructions
	[Serializable]
	public class Step
	{
		public bool PickUp;		// or deliver
		public int Contract;	// which contract
		public int Source;		// and where to get the commodity (-1 = already on-board)

		// read the step from the game save file
		public Step(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			PickUp = reader.ReadBoolean();
			Contract = reader.ReadInt32();
			Source = reader.ReadInt32();
		}

		// write the step to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 0);
			writer.Write(PickUp);
			writer.Write(Contract);
			writer.Write(Source);
		}

		// create a "pick up" step
		public Step(int contract, int source)
		{
			PickUp = true;
			Contract = contract;
			Source = source;
		}

		// create a "deliver" step
		public Step(int contract)
		{
			PickUp = false;
			Contract = contract;
			Source = int.MinValue;
		}
	}

	[Serializable]
	public class Strategy
	{
		public ArrayList Steps;		// the list of steps in the instructions
		public double PayoffRate;	// the rate of income, which is (payoff - cost) / time
		public bool Reconsider;

		// read the steps from the game save file
		public Strategy(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			int count = reader.ReadInt32();
			Steps = new ArrayList(count);
			for (int i=0; i<count; i++)
				Steps.Add(new Step(reader));
			PayoffRate = reader.ReadDouble();
			if (version >= 1)
				Reconsider = reader.ReadBoolean();
			else
				Reconsider = true;
		}

		// write the steps to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 1);
			writer.Write(Steps.Count);
			foreach (Step step in Steps)
				step.Save(writer);
			writer.Write(PayoffRate);
			writer.Write(Reconsider);
		}

		// create a new, blank list
		public Strategy()
		{
			Steps = new ArrayList();
		}

		// add a step to the list
		public void AddStep(Step step)
		{
			Steps.Add(step);
		}

		// create and add a pickup step
		public void AddStep(int contract, int source)
		{
			Steps.Add(new Step(contract, source));
		}

		// create and add a delivery step
		public void AddStep(int contract)
		{
			Steps.Add(new Step(contract));
		}

		// given two contracts and two commodity sources, generate all the different orders that
		// the steps could be performed, with pickups coming before deliveries
		public static Strategy[] CreateCombos(Options options, int contract1, int source1, int contract2, int source2)
		{
			ArrayList combos = new ArrayList();
			Strategy s;

			// evaluate earliest-delivery combos first in case of ties

			if (options.GroupedContracts || (contract2 < Player.NumContracts - 2))
			{
				s = new Strategy();
				s.AddStep(contract1, source1);
				s.AddStep(contract1);
				s.AddStep(contract2, source2);
				s.AddStep(contract2);
				combos.Add(s);
			}

			if (options.GroupedContracts || (contract1 < Player.NumContracts - 2))
			{
				s = new Strategy();
				s.AddStep(contract2, source2);
				s.AddStep(contract2);
				s.AddStep(contract1, source1);
				s.AddStep(contract1);
				combos.Add(s);
			}

			// and then evaluate the earliest-picked combos

			if (options.GroupedContracts || (contract2 < Player.NumContracts - 2))
			{
				s = new Strategy();
				s.AddStep(contract1, source1);
				s.AddStep(contract2, source2);
				s.AddStep(contract1);
				s.AddStep(contract2);
				combos.Add(s);

				s = new Strategy();
				s.AddStep(contract2, source2);
				s.AddStep(contract1, source1);
				s.AddStep(contract1);
				s.AddStep(contract2);
				combos.Add(s);
			}

			if (options.GroupedContracts || (contract1 < Player.NumContracts - 2))
			{
				s = new Strategy();
				s.AddStep(contract1, source1);
				s.AddStep(contract2, source2);
				s.AddStep(contract2);
				s.AddStep(contract1);
				combos.Add(s);

				s = new Strategy();
				s.AddStep(contract2, source2);
				s.AddStep(contract1, source1);
				s.AddStep(contract2);
				s.AddStep(contract1);
				combos.Add(s);
			}

			return (Strategy[]) combos.ToArray(typeof(Strategy));
		}
	}
}
