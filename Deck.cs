
// Deck.cs

/*
 * This class represents a deck of cards, shuffled and then drawn
 * without replacement.
 * 
 */

using System;
using System.IO;

namespace Rails
{
	public class Deck
	{
		int count;
		Random rand;
		int[] cards;
		int remaining;

		public Deck(int count)
		{
			this.count = count;
			rand = new Random();
			cards = new int[count];
			// remaining = 0;	// already done by the runtime
		}

		public void Shuffle()
		{
			for (int i=0; i<count; i++)
				cards[i] = i;
			remaining = count;
		}

		public int Draw()
		{
			if (remaining <= 0)
				Shuffle();
			int i = rand.Next(remaining);
			int temp = cards[i];
			cards[i] = cards[--remaining];
			return temp;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 0);	// version
			writer.Write(count);
			writer.Write(remaining);
			for (int i=0; i<remaining; i++)
				writer.Write(cards[i]);
		}

		public Deck(BinaryReader reader)
		{
			reader.ReadInt32();	// version
			count = reader.ReadInt32();
			cards = new int[count];
			remaining = reader.ReadInt32();
			for (int i=0; i<remaining; i++)
				cards[i] = reader.ReadInt32();
			rand = new Random();
		}
	}
}
