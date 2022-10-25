
// Flood.cs

/*
 * This class represents a river flood event. This event destroys any track
 * that crosses the river and prevents movement over the river if the track
 * is rebuilt.
 * 
 */

using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace Rails
{
	public class Flood : Disaster
	{
		public River River;

		public Flood(int player, Map map, River river) : base(player, map)
		{
			this.River = river;
		}

		public override void Save(BinaryWriter writer)
		{
			base.Save(writer);
			writer.Write((int) 0);	// version
			writer.Write(River.ID);
		}

		public Flood(BinaryReader reader, Map map) : base(reader, map)
		{
			int version = reader.ReadInt32();
			int id = reader.ReadInt32();
			River = (River) map.Rivers[id];
		}

		// destroy the affect track segments
		public override void AffectState(GameState state)
		{
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					for (int d=0; d<6; d++)
						if (state.Track[x, y, d] != -1)
							if ((map[x, y].RiversCrossed[d] & (1L << River.ID)) != 0)
								state.Track[x, y, d] = -1;
		}

		// highlight the river with a yellow highlighter pen
		public override void Draw(Graphics g)
		{
			Pen pen = new Pen(Color.FromArgb(128, Color.Yellow), 10.0f);
			g.DrawLines(pen, River.Path);
			pen.Dispose();
		}

		public override string ToString()
		{
			return River.Name + " Floods\rBridges unsafe, closed indefinitely";
		}

		// prevent movement across the river
		public override bool AdjustMovementCost(int x, int y, int d, int i, int j, ref int cost)
		{
			if (x < 0 || x >= map.GridSize.Width)
				throw new ArgumentOutOfRangeException("x");

			if (y < 0 || y >= map.GridSize.Height)
				throw new ArgumentOutOfRangeException("y");

			if (d < 0 || d > 5)
				throw new ArgumentOutOfRangeException("d");

			if (map[x, y].RiversCrossed == null)
				throw new NullReferenceException("RiversCrossed");

			if (map[x, y].RiversCrossed.Length < 6)
				throw new InvalidOperationException("RiversCrossed.Length");

			if (this.River.ID < 0 || this.River.ID > 31)
				throw new InvalidOperationException("River.ID");

			if ((map[x, y].RiversCrossed[d] & (1U << this.River.ID)) != 0)
				return false;

			return true;
		}
	}
}
