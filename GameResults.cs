
// GameResults.cs

/*
 * Thes classes handle keeping track of the win/loss record and making sure they are
 * not tampered with.
 * 
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Rails
{
	public class PlayerResult
	{
		public string Name;	// null for bots
		public OfficialGame Won;
		public int Funds;

		public PlayerResult(string name, OfficialGame won, int funds)
		{
			Name = name;
			Won = won;
			Funds = funds;
		}

		public PlayerResult(BinaryReader r)
		{
			int version = r.ReadInt32();
			bool b = r.ReadBoolean();
			Name = b ? r.ReadString() : null;
			if (version >= 1)
			{
				Won = (OfficialGame) r.ReadInt32();
			}
			else
			{
				b = r.ReadBoolean();
				Won = b ? OfficialGame.Yes : OfficialGame.No;
			}
			Funds = r.ReadInt32();
		}

		public void Save(BinaryWriter w)
		{
			w.Write((int) 1);	// version
			w.Write(Name != null);
			if (Name != null) w.Write(Name);
			w.Write((int) Won);
			w.Write(Funds);
		}

		public override String ToString()
		{
			bool won = (Won == OfficialGame.Yes);
			return (Name == null ? String.Empty : Name) + ":" + won.ToString() + Funds.ToString();
		}
	}

	public enum OfficialGame
	{
		Yes, No, Unknown
	}

	public class GameResult
	{
		public object Map;
		public int NumPlayers;
		public PlayerResult[] Players;
		public Options GameOptions;
		public int Turns;
		public byte[] Checksum;
		public long FileOffset;
		public DateTime Date;
		public OfficialGame Official;

		public GameResult(Game game)
		{
			if (game.map is RandomMap)
				Map = unchecked((uint) ((RandomMap) game.map).Number);
			else if (game.map is RandomMap2)
				Map = ((RandomMap2) game.map).Name;
			else
				Map = ((AuthoredMap) game.map).Name;
			NumPlayers = game.state.NumPlayers;
			Players = new PlayerResult[NumPlayers];
			for (int i=0; i<NumPlayers; i++)
			{
				string name = null;
				if (game.state.PlayerInfo[i].Human)
					name = game.state.PlayerInfo[i].Name;
				OfficialGame won = OfficialGame.Unknown;
				if (game.IsOfficial)
					won = game.state.Winners.Contains(i) ? OfficialGame.Yes : OfficialGame.No;
				Players[i] = new PlayerResult(name, won, game.state.PlayerInfo[i].Funds);
			}
			GameOptions = game.options;
			Turns = game.state.Turn - 1;
			FileOffset = -1;
			Date = DateTime.Now;
			Official = game.IsOfficial ? OfficialGame.Yes : OfficialGame.No;
		}

		public GameResult(BinaryReader r)
		{
			FileOffset = r.BaseStream.Position;
			int version = r.ReadInt32();
			int mapType;
			if (version >= 2)
				mapType = r.ReadInt32();
			else
				mapType = 0;
			if (mapType == 0)
				Map = r.ReadUInt32();
			else
				Map = r.ReadString();
			NumPlayers = r.ReadInt32();
			Players = new PlayerResult[NumPlayers];
			for (int i=0; i<NumPlayers; i++)
				Players[i] = new PlayerResult(r);
			GameOptions = new Options(r);
			if (version >= 1)
			{
				Turns = r.ReadInt32();
				Date = new DateTime(r.ReadInt64());
			}
			else 
			{
				Turns = -1;
				Date = DateTime.MinValue;
			}
			if (version >= 3)
			{
				Official = (OfficialGame) r.ReadInt32();
			}
			else
			{
				Official = OfficialGame.Unknown;
			}
			Checksum = new byte[20];
			for (int i=0; i<20; i++)
				Checksum[i] = r.ReadByte();
			byte[] hash = ComputeHash();
			for (int i=0; i<20; i++)
				if (Checksum[i] != hash[i])
				{
					MessageBox.Show(Resource.GetString("Game.WinLossError"), Resource.GetString("Rails"));
					Application.Exit();
				}
		}

		public void Save(BinaryWriter w)
		{
			FileOffset = w.BaseStream.Position;
			w.Write((int) 3); // version
			if (Map is uint)
			{
				w.Write((int) 0);
				w.Write((uint) Map);
			}
			else
			{
				w.Write((int) 1);
				w.Write((string) Map);
			}
			w.Write(NumPlayers);
			foreach (PlayerResult r in Players)
				r.Save(w);
			GameOptions.Save(w);
			w.Write(Turns);
			w.Write(Date.Ticks);
			w.Write((int) Official);
			Checksum = ComputeHash();
			for (int i=0; i<20; i++)
				w.Write(Checksum[i]);
		}

		string GetDescription()
		{
			System.Globalization.NumberFormatInfo numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
			StringBuilder b = new StringBuilder(NumPlayers.ToString(numberFormat) + ":" + GameOptions.ToString()
				+ ":" + FileOffset.ToString(numberFormat) + ":" + Map.ToString() + (Turns == -1 ? "" : ":" + Turns.ToString(numberFormat)) + (Date == DateTime.MinValue ? "" : ":" + Date.Ticks));
			foreach (PlayerResult r in Players)
				b.Append(":" + r.ToString());
			if (Official != OfficialGame.Unknown)
				b.Append(":" + Official);
			return b.ToString();
		}

		internal byte[] ComputeHash()
		{
			string s = GetDescription();
			byte[] data = Encoding.UTF8.GetBytes(s + ":bc74baaa18c16480a545263ef434ee61");
			SHA1 sha = SHA1.Create();
			byte[] hash = sha.ComputeHash(data);
			return hash;
		}
	}

	public class GameResultsCollection : IEnumerable
	{
		public ArrayList Results;

		public GameResultsCollection()
		{
			Stream s = null;
			BinaryReader r = null;
			try
			{
				s = new FileStream("winloss.rec", FileMode.Open);
				r = new BinaryReader(s);
				Results = new ArrayList();
				while (s.Position < s.Length)
					Results.Add(new GameResult(r));
			}
			catch(System.IO.IOException)
			{
				return;
			}
			finally
			{
				if (r != null)
					r.Close();
				if (s != null)
					s.Close();
			}
		}

		public static void Record(GameResult result)
		{
			Stream s = null;
			BinaryWriter w = null;
			try
			{
				s = new FileStream("winloss.rec", FileMode.OpenOrCreate);
				s.Seek(0, SeekOrigin.End);
				w = new BinaryWriter(s);
				result.Save(w);
			}
			finally
			{
				if (w != null)
					w.Close();
				if (s != null)
					s.Close();
			}
		}

		public IEnumerator GetEnumerator()
		{
			return Results.GetEnumerator();
		}

		public GameResult this[int index]
		{
			get
			{
				return (GameResult) Results[index];
			}
		}

		public int Count
		{
			get
			{
				return Results.Count;
			}
		}
	}
}
