
// Trigram.cs

/*
 * This class analyzes a list of words or phrases and calculates the probability distribution for 
 * a third letter following two previous letters. It is used to generate realistic-sounding yet 
 * random names for cities and rivers.
 * 
 * Originally I had it keep track of which words it generated so that it wouldn't generate the
 * same word more than once, but that made it so that maps couldn't be recreated with the same
 * city names just by initializing the map random number generator to a known seed. The client of
 * this class can still take care of name collisions, though.
 * 
 */

using System;
using System.Collections;
using System.Text;

namespace Rails
{
	public class Trigram
	{
		Hashtable trigram;	// keys are two-letter strings, values are strings where the 
							// characters have the same probability distribution as in the
							// original data

		int min, max;		// shortest and longest strings in the source data

		public Trigram()
		{
			trigram = new Hashtable();
			min = int.MaxValue;
			max = int.MinValue;
		}

		public void Add(string s)
		{
			// updated the shortest/longest length
			if (s.Length < min)
				min = s.Length;
			if (s.Length > max)
				max = s.Length;

			// start with the beginning-of-word pair, which is \0\0
			char c1 = '\0';
			char c2 = '\0';
			for (int i=0; i<=s.Length; i++)
			{
				// get the next character, or \0 if we're at the end of the string
				char c = i < s.Length ? s[i] : '\0';

				// look up the entry
				string prefix = c1.ToString() + c2.ToString();
				StringBuilder b = (StringBuilder) trigram[prefix];

				// if it doesn't exist, create a new entry
				if (b == null)
					trigram[prefix] = b = new StringBuilder();

				b.Append(c);
				c1 = c2;
				c2 = c;
			}
		}

		// generate a new random string based on the probability distributions
		public string GetSample(Random rand, Hashtable seen)
		{
			// keep trying until we get a string that isn't too long or too short
			while (true)
			{
				StringBuilder s = new StringBuilder();
				char c1 = '\0';
				char c2 = '\0';
				while(true)
				{
					StringBuilder b = (StringBuilder) trigram[c1.ToString() + c2.ToString()];
					char c = b[rand.Next(b.Length)];
					if (c == '\0')
						break;
					s.Append(c);
					c1 = c2;
					c2 = c;
				}
				string t = s.ToString();
				if (seen == null)
					return t;
				if (t.Length >= min && t.Length <= max && !seen.ContainsKey(t))
				{
					seen[t] = null;
					return t;
				}
			}
		}
	}
}
