
// TrackRecord.cs

/*
 * This small class is used to store individual track segments while a player is building
 * track. The segments are stored in a stack that is used to allow the player to back-track
 * and erase newly-built track while building.
 * 
 */

using System;

namespace Rails
{
	public class TrackRecord
	{
		public int i;	// where from
		public int j;
		public int d;	// which direction
		public int c;	// how much was spent

		public TrackRecord(int i, int j, int d, int c)
		{
			this.i = i;
			this.j = j;
			this.d = d;
			this.c = c;
		}
	}
}
