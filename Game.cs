
// Game.cs

/*
 * The monster class. This is where it all happens.
 * 
 * I. DATA
 * II. CONSTRUCTORS AND SERIALIZERS
 * III. DRAWING ROUTINES
 * IV. EVENT HANDLERS
 * V. TRAIN MOVEMENT
 * VI. BUILDING TRACK
 * VII. USER INTERFACE BOUNDARY RECTANGLES
 * VIII. TURN MANAGEMENT
 * IX. MULTI-LEVEL UNDO FUNCTIONALITY
 * X. RANDOM EVENTS
 * XI. CONTRACT MANAGEMENT
 * XII. COMPUTER INTELLIGENCE
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace Rails
{
	public class Game : IDisposable
	{

/* CONSTANTS ---------------------------------------------------------------------------------------- */

		const int seaMovementCost = 360;	// takes six hours to move between sea mileposts
		const int loadingTime = 120;		// takes two hours to load/unload cargo

		const int useTrackCost = 4;			// costs 4 to use another player's track

		// player colors and color names
		public static readonly StaticColorArray TrackColor = new StaticColorArray(new Color[] {Color.Green, Color.Crimson, Color.DodgerBlue, /*Color.MediumOrchid*/ Color.DarkMagenta, Color.FromArgb(255, 177, 0), Color.DarkGray, Color.Black});
		public static readonly StaticStringArray ColorName = new StaticStringArray(new string[] {"Green", "Red", "Blue", "Purple", "Gold", "Silver"});
		public const int MaxPlayers = 6;
		public const int NationalizedTrackColor = 6;
		
/* PUBLIC FIELDS ------------------------------------------------------------------------------------ */

		public Map map;						// the map
		public GameState state;				// the game state (player info, tracks)

/* INTERNAL DATA ------------------------------------------------------------------------------------ */

		// USER INTERFACE AND DRAWING

		public GameForm form;				// the main form
		public static Game Current;			// the current game
		Rectangle statusRect;				// the bounds of the game status area (on the right)
		bool suppressUI;					// prevents mouse clicks while auto-build/auto-move is happening
		static Pen[] trackPen = CreatePens(); // Pen objects of each train color
		int ptrI = -1, ptrJ = -1;			// which milepost the mouse is on
		int ptrX = -1, ptrY = -1;			// the screen coordinate of the milepost the mouse is on
		int mouseCity = -1;					// which city the mouse is on
		bool mouseCityShow;					// whether the mouseover city's commodities are displayed
		int mouseContract = -1;				// which contract we're mousing over
		int mouseCommodity = -1;			// which source commodity we're mousing over
		int mouseFreight = -1;				// which freight car we're mousing over
		bool inNextPlayerRect;				// if we're in the next-player rectangle
		bool inUpgradeRect;					// if we're in the upgrade-train rectangle
		int contractTop = 214;				// the top (y-coord) of the contract display area
		int contractHeight = 56;			// the height (y-coord) of each contract display rectangle
		bool blinkingTrain;					// whether or not the train should blink off and on
		bool blinkOn;						// whether the train is blunked or not (that's a new word)
		Timer blinkTimer;					// timer to toggle the blinking

		// PATH-FINDING FUNCTION POINTERS

		FloodMethod trainMovementMethod;	// algorithm for finding shortest route for train movement
		FloodMethod buildTrackMethod;		// algorithm for finding cheapest route for building track

		// COMPUTER INTELLIGENCE

		bool suspendAI;						// causes computer players to pause
		Indicator ind;						// a display form showing the computer's action
		int cilen;							// the length of the string describing the action
		bool suspendUI;						// suppresses display of trains and tracks while computer is experimenting

		// MULTI-LEVEL UNDO

		Stack UndoStack;					// keeps track of game states for Undo feature

		// BUILDING TRACK

		bool building;						// are we currently building track (the left button is down)?
		int buildI = -1, buildJ = -1;		// the endpoint of the new track
		Stack buildStack;					// keep track of new track segments so we can abort
		int buildCost;						// keep track of build cost

		// TRAIN MOVEMENT

		bool placingLocomotive;				// is the user selecting the starting position for their train?
		Stack moveStack;					// keeps track of train movements for train move automation
		Timer moveTimer;					// timer for movement animation
		bool[,] canReachInTime;				// which mileposts could be reached in remaining time

		// CONTRACT ANIMATION
		
		DateTime animationStart;			// what time we starting animating the contract deletion
		TimeSpan animationLength;			// how long the contract deletion animation should last
		int deadContract = -1;				// which contract is being deleted

		// RANDOM EVENTS

		Random rand;						// random number generator for random events
		Queue disasters = new Queue();		// the list of currently active random events

		// GAME OPTIONS

		public Options options;				// game variations as selected in the new game dialog







/* CONSTRUCTORS AND SERIALIZERS ---------------------------------------------------------------------- */

		// initialize the Pen objects used for drawing the little circle where the mouse is
		private static Pen[] CreatePens()
		{
			Pen[] temp = new Pen[TrackColor.Length];
			for (int i=0; i<TrackColor.Length; i++)
				temp[i] = new Pen(new SolidBrush(TrackColor[i]), 2.0f);
			return temp;
		}

		void DeserializeGame(BinaryReader reader, bool initialize)
		{
			int version = reader.ReadInt32();
			int mapType;
			if (version < 1)
				mapType = 0;
			else
				mapType = reader.ReadInt32();
			if (mapType == 0)
			{
				int seed = reader.ReadInt32();
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				if (initialize) 
					map = new RandomMap(new Size(width, height), seed);
			}
			else if (mapType == 1)
			{
				string name = reader.ReadString();
				/* int width = */ reader.ReadInt32();
				/* int height = */ reader.ReadInt32();
				if (initialize)
					map = new AuthoredMap(name);
			}
			else if (mapType == 2)
			{
				string name = reader.ReadString();
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				if (initialize)
					map = new RandomMap2(new Size(width, height), name);
			}
			else if (initialize)
				throw new InvalidOperationException();
			statusRect.X = reader.ReadInt32();
			statusRect.Y = reader.ReadInt32();
			statusRect.Width = reader.ReadInt32();
			statusRect.Height = reader.ReadInt32();
			state = new GameState(reader, map);
			Contract.ReadRandom(reader);
			options = new Options(reader);
			disasters = new Queue();
			if (version >= 3)
			{
				int disasterCount = reader.ReadInt32();
				for (int i=0; i<disasterCount; i++)
				{
					string typeName = reader.ReadString();
					Disaster disaster = Disaster.CreateInstance(typeName, reader, map);
					if (disaster != null)
						disasters.Enqueue(disaster);
				}
			}
		}

		private Game()
		{
			Application.Idle += new EventHandler(Idle);
		}

		// restore the game from the game save file
		public Game(BinaryReader reader) : this()
		{
//			this.form = form;
			Current = this;

			DeserializeGame(reader, true);

			trainMovementMethod = new FloodMethod(TrainMovementMethod);
			buildTrackMethod = new FloodMethod(BuildTrackMethod);
			rand = new Random();
			UndoStack = new Stack();
		}

		// write the game to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 3); // version
			if (map is RandomMap)
			{
				writer.Write((int) 0);
				writer.Write(((RandomMap) map).Number);
			}
			else if (map is RandomMap2)
			{
				writer.Write((int) 2);
				writer.Write(((RandomMap2) map).Name);
			}
			else 
			{
				writer.Write((int) 1);
				writer.Write(((AuthoredMap) map).Name);
			}
			writer.Write(map.ImageSize.Width);
			writer.Write(map.ImageSize.Height);
			writer.Write(statusRect.X);
			writer.Write(statusRect.Y);
			writer.Write(statusRect.Width);
			writer.Write(statusRect.Height);
			state.Save(writer, map);
			Contract.SaveRandom(writer);
			options.Save(writer);
			writer.Write(disasters.Count);
			foreach (Disaster disaster in disasters)
			{
				writer.Write(disaster.GetType().FullName);
				disaster.Save(writer);
			}

//			trainMovementMethod = new FloodMethod(TrainMovementMethod);
//			buildTrackMethod = new FloodMethod(BuildTrackMethod);
//			rand = new Random();
//			UndoStack = new Stack();
		}

		// start a new game with a specified map and list of players
		public Game(Map map, Rectangle statusRect, Player[] playerList, GameForm form, Options options) : this()
		{
			Current = this;
			this.map = map;
			this.statusRect = statusRect;
			this.form = form;
			this.options = options;

			rand = new Random();
			state = new GameState(playerList, map, options, rand);
			trainMovementMethod = new FloodMethod(TrainMovementMethod);
			buildTrackMethod = new FloodMethod(BuildTrackMethod);
			UndoStack = new Stack();
		}

		public void Dispose()
		{
			Application.Idle -= new EventHandler(Idle);
			Current = null;
			GC.SuppressFinalize(this);
			if (moveTimer != null)
				moveTimer.Dispose();
			if (ind != null)
				ind.Dispose();
			if (blinkTimer != null)
				blinkTimer.Dispose();
		}

		public byte[] GetData()
		{
			MemoryStream stream = new MemoryStream();
			this.Save(new BinaryWriter(stream));
			byte[] buffer = stream.GetBuffer();
			byte[] data = new byte[stream.Position];
			Array.Copy(buffer, data, data.Length);
			stream.Close();
			return data;
		}





/* DRAWING ROUTINES ---------------------------------------------------------------------------------- */

		// draw the game map and status area
		public void Paint(PaintEventArgs e)
		{
			// nothing to do if clip rectangle is empty
			if (e.ClipRectangle.Width <= 0 || e.ClipRectangle.Height <= 0)
				return;

			// only update status window if necessary
			if (e.ClipRectangle.Right > map.ImageSize.Width)
			{
				DrawStatus(e.Graphics);
			}

			// only update map window if necessary
			if (e.ClipRectangle.Left < map.ImageSize.Width)
			{
				// use image buffering to eliminate redraw flicker
				Bitmap bmp = new Bitmap(e.ClipRectangle.Width, e.ClipRectangle.Height);

				Graphics g = Graphics.FromImage(bmp);

				// make sure the hatch brushes don't jiggle
				g.RenderingOrigin = new Point(16 - e.ClipRectangle.X % 16, 16 - e.ClipRectangle.Y % 16);

				g.TranslateTransform(-e.ClipRectangle.X, -e.ClipRectangle.Y);
				Rectangle destRect = e.ClipRectangle;
				Rectangle srcRect = e.ClipRectangle;

				// draw the background (water and land)
				g.DrawImage(map.Background, destRect, srcRect, GraphicsUnit.Pixel);

				// then the train range
				DrawTrainRange(g);

				// then the tracks
				if (!suspendUI) DrawTrack(g);

				// then the random events
				DrawDisasters(g);

				// then the incentive cities
				DrawIncentives(g);

				// then the foreground (cities, mileposts)
				g.DrawImage(map.Foreground, destRect, srcRect, GraphicsUnit.Pixel);

				// then the little circle where the mouse is
				DrawPointer(g);

				// then the highlighted commodity supply locations
				DrawProductIcons(g);

				// then the train indicators
				if (!suspendUI) DrawTrains(g);

				// the the highlighted city commodities
				DrawMouseCity(g);

				// the player comparison plaque
				DrawPlaque(g);

				g.Dispose();
				e.Graphics.DrawImageUnscaled(bmp, e.ClipRectangle.X, e.ClipRectangle.Y);
				bmp.Dispose();
			}
		}

		// Draw the little circle where the mouse is, or if we're placing the train, draw the train
		// where the mouse is. If we're building track, also draw the cumulative cost so far.
		void DrawPointer(Graphics g)
		{
			if (ptrI == -1)
				return;

			if (placingLocomotive)
			{
				DrawTrain(g, ptrX, ptrY, 1, state.CurrentPlayer);
				return;
			}

			g.DrawEllipse(trackPen[state.PlayerInfo[state.CurrentPlayer].TrackColor], ptrX - 5, ptrY - 5, 11, 11);

			if (building && state.AccumCost + buildCost > 0)
			{
				Font f = new Font("Tahoma", 13.0f, FontStyle.Bold);
				string s = (state.AccumCost + buildCost).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				int px = ptrX + 8;
				int py = ptrY + 8;
				Utility.DrawStringOutlined(g, s, f, Brushes.Black, Brushes.White, px, py);
				f.Dispose();
			}
		}

		// determine whether to hide or display the commodities available at the city where the mouse is
		void ShowMouseCity(bool show)
		{
			City city = map.Cities[mouseCity];
			int x, y;
			if (!map.GetCoord(city.X, city.Y, out x, out y))
				return;
			Rectangle r;
			if (city.Products.Count == 2)
				r = new Rectangle(x - 24, y - 58, 83, 67);
			else
				r = new Rectangle(x + 10, y - 40, 48, 48);
			mouseCityShow = show;
			form.Invalidate();
		}

		// hide or display the commodities available at the city where the mouse is
		void DrawMouseCity(Graphics g)
		{
			if (mouseCity == -1 || !mouseCityShow || building)
				return;

			ArrayList products = map.Cities[mouseCity].Products;
			if (products.Count == 0)
				return;

			if (products.Count == 1)
				DrawCityCommodities(g, (int) products[0]);
			else
				DrawCityCommodities(g, (int) products[0], (int) products[1]);
		}

		// draw all the track segments
		void DrawTrack(Graphics g)
		{
			int xmin, xmax, ymin, ymax;
			int i, j, xx, yy, ii, jj;

			// determine which mileposts fall within the clip rectangle and only draw those

			RectangleF rect = g.VisibleClipBounds;

			map.GetNearest((int) rect.X, (int) rect.Y, out xmin, out ymin);
			xmin--;
			ymin--;

			map.GetNearest((int) rect.Right, (int) rect.Bottom, out xmax, out ymax);
			xmax++;
			ymax++;

			for (int x=xmin; x<=xmax; x++)
				for (int y=ymin; y<=ymax; y++)
					if (map.GetCoord(x, y, out xx, out yy))
					{
						for (int d=0; d<3; d++)
							if (state.Track[x,y,d] != -1)
								if (map.GetAdjacent(x, y, d, out i, out j))
									if (map.GetCoord(i, j, out ii, out jj))
									{
										int pl = state.Track[x, y, d];
										int tc;
										if (pl == state.NumPlayers)
											tc = NationalizedTrackColor;
										else
											tc = state.PlayerInfo[pl].TrackColor;

										// draw either a regular track segment or a bridge segment
										if ((map[x, y].WaterMask & (WaterMasks.RiverMask[d] | WaterMasks.InletMask[d])) == 0)
											g.DrawImageUnscaled(Images.TrackBitmap[tc, d], (xx+ii)/2-11, (yy+jj)/2-11);
										else
											g.DrawImageUnscaled(Images.BridgeBitmap[tc, d], (xx+ii)/2-11, (yy+jj)/2-11);
									}
					}
		}

		// display the status area to the right of the map
		void DrawStatus(Graphics gr)
		{
			int x = 8;
			int lh = 14;

			// initialize graphics objects
			Bitmap bmp = new Bitmap(statusRect.Width, statusRect.Height); // buffer to eliminate flicker
			Graphics g = Graphics.FromImage(bmp);
			Font font = new Font("Tahoma", 8.5f);
			Font bold = new Font("Tahoma", 8.5f, FontStyle.Bold);
			Font small = new Font("Tahoma", 6.0f);

			// erase the background
			g.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);

			// display the turn and player stats
			Player pl = state.PlayerInfo[state.CurrentPlayer];

			// background color
			int colorIndex = pl.TrackColor;
			using (Brush brush = new SolidBrush(TrackColor[colorIndex]))
			{
				g.FillRectangle(brush, 0, 0, statusRect.Width, 64);
			}
			if (inNextPlayerRect)
			{
				using (Brush brush = new SolidBrush(Color.FromArgb(64, Color.Black)))
				{
					g.FillRectangle(brush, NextPlayerRect);
				}
			}

			// turn number
			System.Globalization.NumberFormatInfo numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
			g.DrawString("Turn " + state.Turn.ToString(numberFormat), bold, Brushes.White, x, 8);

			// species and icon
			bool human = pl.Human;
			g.DrawString(state.ThisPlayer.Name, font, Brushes.White, x, 8 + lh);
			Icon icon = human ? Images.HumanIcon : Images.ComputerIcon;
			g.DrawImage(icon.ToBitmap(), statusRect.Width - 40, 8, 32, 32);

			// funds
			int funds = pl.Funds;
			string fundsStr = Utility.CurrencyString(Math.Abs(funds)) + " in " + (funds < 0 ? "debt" : "the bank");
			g.DrawString(fundsStr, font, Brushes.White, 8, 8 + 2*lh);

			// loco or placement button
			Rectangle rect = TrainUpgradeRect;
			if (pl.X == -1)
			{
				if (inUpgradeRect)
					g.FillRectangle(Brushes.DarkBlue, rect);
				else
					g.FillRectangle(Brushes.DimGray, rect);
				g.DrawRectangle(Pens.Black, rect);
				g.DrawLine(Pens.White, rect.Left+1, rect.Bottom-1, rect.Left+1, rect.Top+1);
				g.DrawLine(Pens.White, rect.Left+1, rect.Top+1, rect.Right-1, rect.Top+1);
				g.DrawLine(Pens.DarkSlateGray, rect.Right-1, rect.Top+1, rect.Right-1, rect.Bottom-1);
				g.DrawLine(Pens.DarkSlateGray, rect.Right-1, rect.Bottom-1, rect.Left+1, rect.Bottom-1);
				StringFormat fmt = new StringFormat();
				fmt.Alignment = StringAlignment.Center;
				fmt.LineAlignment = StringAlignment.Center;
				RectangleF r = RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
				string s = placingLocomotive ? "Now click a map location" : "Click here to place train";
				g.DrawString(s, font, Brushes.White, r, fmt);
				fmt.Dispose();
			}
			else
			{
				if (inUpgradeRect)
					g.FillRectangle(Brushes.DarkBlue, rect);
				int tux = (rect.Left + rect.Right) / 2;
				int tuy = (rect.Top + rect.Bottom) / 2;
				g.DrawImageUnscaled(Images.LocoBitmap, tux - Images.LocoBitmap.Width/2, tuy - Images.LocoBitmap.Height/2);
				string locoStr = Engine.Description[pl.EngineType] + ": " + Engine.SpeedString[pl.EngineType] + " / dot";
				SizeF size = g.MeasureString(locoStr, font);
				int lsw = (int) size.Width/2;
				int lsh = (int) size.Height/2;
				Utility.DrawStringOutlined(g, locoStr, font, Brushes.White, Brushes.Black, tux - lsw, tuy - lsh);
			}

			// cargo
			for (int i=0; i<pl.Cars; i++)
			{
				rect = FreightRect(i);
				if (i == mouseFreight)
					g.FillRectangle(Brushes.DarkBlue, rect);
				else
					g.FillRectangle(Brushes.DimGray, rect);
				int product = pl.Cargo[i];
				if (product == -1)
					g.DrawString("empty", small, Brushes.White, rect.X + 4, rect.Y + 10);
				else
					g.DrawImage(Products.Icon[product], rect);
			}

			// map position
			int posY = 142;
			if (pl.X == -1)
			{
				g.DrawString("Train not yet positioned", font, Brushes.White, x, posY);
			}
			else if (map.IsSea(pl.X, pl.Y))
			{
				g.DrawString("Cargo is aboard ship", font, Brushes.White, x, posY);
			}
			else if (!map.IsCity(pl.X, pl.Y))
			{
				g.DrawString("Train is en route", font, Brushes.White, x, posY);
			}
			else
			{
				// commodities available at current city
				int ci = map[pl.X, pl.Y].CityIndex;
				City city = map.Cities[ci];
				g.DrawString("At " + city.Name, font, Brushes.White, x, posY);
				if (city.Products.Count == 0)
					g.DrawString("No commodities", font, Brushes.White, x, posY + lh);
				else
				{
					for (int i=0; i < city.Products.Count; i++)
					{
						rect = CommodityRect(i);
						if (i == mouseCommodity)
							g.FillRectangle(Brushes.DarkBlue, rect);
						g.DrawImage(Products.Icon[(int) city.Products[i]], rect);
					}
				}
			}
			
			// time remaining
			string timeStr = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Time: {0:00}:{1:00}   Spent: {2}", state.AccumTime / 60, state.AccumTime % 60, Utility.CurrencyString(state.AccumCost));
			g.DrawString(timeStr, font, Brushes.White, 8, contractTop - lh - 8);

			// list the contracts
			int y = statusRect.Y + contractTop;
			int dx = 52;
			int dy = contractHeight;
			string to = Resource.GetString("Game.To");
			float wto = gr.MeasureString(to, font).Width;

			// calculate dead-contract animation variables
			double clock;
			int voffset = 0;
			int fade = 0;
			int hole = 0;
			if (deadContract != -1)
			{
				clock = 1.0 * (DateTime.Now - animationStart).TotalMilliseconds / animationLength.TotalMilliseconds;
				if (clock > 1.0) clock = 1.0;
				clock = clock * clock * (3 - 2 * clock);
				voffset = (int) (clock * dy);
				fade = (int) (255 * clock);
				if (options.GroupedContracts)
					hole = 3 * (deadContract / 3);
			}

			if (deadContract == -1)
			{
				// highlight the mouseover contract unless we're animating a contract deletion
				if (mouseContract != -1)
				{
					g.FillRectangle(Brushes.DarkBlue, 0, y + dy * mouseContract - 4, statusRect.Width, dy);
				}
			}

			// display each contract
			Font af = new Font("Tahoma", 14);
			for (int i=Player.NumContracts-1; i>=0; i--)
			{
				// calculate the vertical position
				int cy = y + dy * i;

				// slide it down if we're animating a contract deletion
				if (deadContract != -1 && i < deadContract && i >= hole)
					cy += voffset;

				// get the contract info
				Contract contract = state.ThisPlayer.Contracts[i];
				int product = contract.Product;

				// draw a green background if we're carrying that product
				bool has = false;
				for (int j=0; j<state.ThisPlayer.Cars; j++)
					if (product == state.ThisPlayer.Cargo[j])
						has = true;
				if (has)
					g.FillRectangle(Brushes.DarkGreen, 0, cy - 4, statusRect.Width, dy);
				else if (state.Availability[product] == 0)
					// draw a red background if the product is unavailable
					g.FillRectangle(Brushes.DarkRed, 0, cy - 4, statusRect.Width, dy);

				// get the destination city info
				City destination = map.Cities[contract.Destination];

				// draw the product icon
				g.DrawImageUnscaled(Products.Icon[product], x, cy);

				// draw the availability
				if (options.LimitedCommodities)
					Utility.DrawStringOutlined(g, state.Availability[product].ToString(numberFormat), af, Brushes.Red, Brushes.Black, x, cy);

				// draw the name of the product
				g.DrawString(Products.Name[product], font, Brushes.White, x + dx, cy);

				// draw the name of the destination, with special treatment for major cities
				if (map.IsCapital(destination.X, destination.Y))
				{
					g.DrawString(to, font, Brushes.White, x + dx, cy + lh);
					g.DrawString(destination.Name, bold, Brushes.Gold, x + dx + wto , cy + lh);
				}
				else
					g.DrawString(to + destination.Name, font, Brushes.White, x + dx, cy + lh);

				// draw the payoff amount
				g.DrawString("for " + Utility.CurrencyString(contract.Payoff), font, Brushes.White, x + dx, cy + 2*lh);

				// fade out the contract being deleted
				if (i == deadContract)
				{
					Brush dim = new SolidBrush(Color.FromArgb(fade, Color.Black));
					g.FillRectangle(dim, 0, cy - 4, statusRect.Width, dy);
					dim.Dispose();
				}
			}

			// draw contract group borders
			if (this.options.GroupedContracts)
			{
				using (Pen pen = new Pen(Color.Gray, 1))
				{
					pen.DashStyle = DashStyle.Dash;
					for (int i=3; i<Player.NumContracts; i+=3)
					{
						int cy = y + dy * i - 4;
						g.DrawLine(pen, 0, cy, statusRect.Width, cy);
					}
				}
			}

			// clean up
			af.Dispose();
			font.Dispose();
			bold.Dispose();
			g.Dispose();
			gr.DrawImageUnscaled(bmp, statusRect.X, statusRect.Y);
			bmp.Dispose();
		}

		void DrawCityCommodities(Graphics g, params int[] products)
		{
			const int radius = 30;

			int x, y;

			foreach (City city in map.Cities)
				if (city.Products.Count > 0)
				{
					int n = 0;
					int whichOne = 0;
					for (int p=0; p<city.Products.Count; p++)
						foreach (int product in products)
							if ((int) city.Products[p] == product)
							{
								n++;
								whichOne = p;
							}

					if (n == 0)
						continue;

					if (!map.GetCoord(city.X, city.Y, out x, out y))
						continue;
					
					int dx = map.Background.Width / 2 - x;
					int dy = map.Background.Height / 2 - y;
					int dr = (int) Math.Sqrt(dx * dx + dy * dy);
					dx = dx * radius / dr;
					dy = dy * radius / dr;

					if (n == 1) // draw one icon
					{
						Bitmap icon = Products.Icon[(int) city.Products[whichOne]];
						g.DrawImageUnscaled(icon, x + dx - icon.Width / 2, y + dy - icon.Height / 2, icon.Width, icon.Height);
					}
					else // draw both icons
					{
						Bitmap icon = Products.Icon[(int) city.Products[0]];
						g.DrawImageUnscaled(icon, x + dx + dy - icon.Width / 2, y + dy - dx - icon.Height / 2, icon.Width, icon.Height);
						icon = Products.Icon[(int) city.Products[1]];
						g.DrawImageUnscaled(icon, x + dx - dy - icon.Width / 2, y + dy + dx - icon.Height / 2, icon.Width, icon.Height);
					}
				}
		}

		// if the mouse is hovering over a contract, indicate the sources and the destinations
		void DrawProductIcons(Graphics g)
		{
			if (mouseContract == -1)
				return; // we're not doing that

			Contract contract = state.ThisPlayer.Contracts[mouseContract];
			int product = contract.Product;
			DrawCityCommodities(g, product);

			// highlight the destination city with a blue glow
			int x, y;
			if (map.GetCoord(map.Cities[contract.Destination].X, map.Cities[contract.Destination].Y, out x, out y))
			{
				g.DrawImageUnscaled(Images.Glow, x - Images.Glow.Width / 2, y - Images.Glow.Height / 2, Images.Glow.Width, Images.Glow.Height);
			}
		}

		// draw all the current disasters
		void DrawDisasters(Graphics g)
		{
			if (disasters.Count == 0)
				return;

			foreach (Disaster disaster in disasters)
				disaster.Draw(g);
		}

		// show the remaining range of movement for the current player's train
		void DrawTrainRange(Graphics g)
		{
			if (canReachInTime == null)
				return;

			int xx, yy, R = 8;

			Brush brush = new SolidBrush(Color.FromArgb(128, TrackColor[state.ThisPlayer.TrackColor]));
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (canReachInTime[x, y])
						if (map.GetCoord(x, y, out xx, out yy))
							g.FillEllipse(brush, xx - R, yy - R, 2 * R, 2 * R);
			brush.Dispose();
		}

		// draw the train indicator
		void DrawTrain(Graphics g, int x, int y, int d, int p)
		{
			g.DrawImageUnscaled(Images.TrainPointer[d], x - 10, y - 10);
			g.DrawImageUnscaled(Images.TrainDot[state.PlayerInfo[p].TrackColor], x - 3, y - 3);
		}

		// draw all the train indicators
		void DrawTrains(Graphics g)
		{
			for (int i=0; i<state.NumPlayers; i++)
			{
				int j = (state.CurrentPlayer + 1 + i) % state.NumPlayers;
				Player pl = state.PlayerInfo[j];
				if (pl.X == -1)
					continue;
				if (j == state.CurrentPlayer && !TrainIsVisible)
					continue;
				int x, y;
				if (map.GetCoord(pl.X, pl.Y, out x, out y))
					DrawTrain(g, x, y, pl.D, j);
			}
		}

		// set whether or not the current player's train should be blinking
		void BlinkTrain(bool blink)
		{
			blinkingTrain = blink;
			if (blink)
			{
				if (blinkTimer == null)
				{
					blinkTimer = new Timer();
					blinkTimer.Tick += new EventHandler(BlinkEventHandler);
					blinkTimer.Interval = 250;
				}
				blinkTimer.Start();
			}
			else 
			{
				if (blinkTimer != null)
					blinkTimer.Stop();
			}
		}

		// toggle the train blink state and update the display
		void BlinkEventHandler(object sender, EventArgs e)
		{
			blinkOn = !blinkOn;
			int xx, yy;
			if (map.GetCoord(state.ThisPlayer.X, state.ThisPlayer.Y, out xx, out yy))
				form.Invalidate(new Rectangle(xx - 10, yy - 10, 20, 20));
		}

		// determine if the train should be displayed
		bool TrainIsVisible
		{
			get
			{
				return (!blinkingTrain || blinkOn);
			}
		}

		void DrawIncentives(Graphics g)
		{
			int R = 25;

			Brush brush;

			if (options.CityIncentives)
			{
				brush = new HatchBrush(HatchStyle.DiagonalCross, Color.YellowGreen, Color.Transparent);			
				for (int i=0; i<map.CityCount; i++)
					if (state.CityIncentive[i])
					{
						City city = map.Cities[i];
						int xx, yy;
						if (map.GetCoord(city.X, city.Y, out xx, out yy))
							g.FillEllipse(brush, xx - R, yy - R, 2 * R, 2 * R);
					}
				brush.Dispose();
			}

			if (options.FirstToCityBonuses)
			{
				brush = new HatchBrush(HatchStyle.DottedGrid, Color.Purple, Color.Transparent);
				for (int i=0; i<map.CityCount; i++)
					if (!state.CityWasVisited[i])
					{
						City city = map.Cities[i];
						int xx, yy;
						if (map.GetCoord(city.X, city.Y, out xx, out yy))
							g.FillEllipse(brush, xx - R, yy - R, 2 * R, 2 * R);
					}
				brush.Dispose();
			}
		}

		void InvalidateCity(int cityIndex)
		{
			if (cityIndex == -1) return;
			City city = map.Cities[cityIndex];
			int xx, yy;
			if (map.GetCoord(city.X, city.Y, out xx, out yy))
				form.Invalidate(new Rectangle(xx - 30, yy - 30, 60, 60));
		}

		bool plaqueEnabled = true;
		Point plaqueLocation = Point.Empty;

		void DeterminePlaqueLocation()
		{
			if (plaqueLocation == Point.Empty)
			{
				int[,] f = new int[map.GridSize.Width, map.GridSize.Height];
				for (int x=0; x<map.GridSize.Width; x++)
					for (int y=0; y<map.GridSize.Height; y++)
						if (x==0 || x==map.GridSize.Width-1 || y == 0 || y == map.GridSize.Height - 1 || map[x, y].Terrain != TerrainType.Inaccessible)
							f[x, y] = 0;
						else
							f[x, y] = int.MaxValue;
				int c = 0;
				bool ok = true;
				int bx = -1, by = -1;
				while (ok)
				{
					ok = false;
					int i, j;
					for (int x=0; x<map.GridSize.Width; x++)
						for (int y=0; y<map.GridSize.Height; y++)
							if (f[x, y] == c)
								for (int d=0; d<6; d++)
									if (map.GetAdjacent(x, y, d, out i, out j))
										if (f[i, j] == int.MaxValue)
										{
											f[i, j] = c + 1;
											ok = true;
											bx = i; by = j;
										}
					c++;
				}

				if (f[bx, by] < 5)
				{
					plaqueEnabled = false;
					return;
				}

				map.GetCoord(bx, by, out bx, out by);
				plaqueLocation = new Point(bx, by);
			}
		}

		void UpdatePlaque()
		{
			DeterminePlaqueLocation();
			int x0 = plaqueLocation.X - Images.Plaque.Width / 2;
			int y0 = plaqueLocation.Y - Images.Plaque.Height / 2;
			form.Invalidate(new Rectangle(new Point(x0, y0), Images.Plaque.Size));
		}

		void DrawPlaque(Graphics g)
		{
			if (!plaqueEnabled)
				return;

			DeterminePlaqueLocation();
			int x0 = plaqueLocation.X - Images.Plaque.Width / 2;
			int y0 = plaqueLocation.Y - Images.Plaque.Height / 2;
			g.DrawImageUnscaled(Images.Plaque, x0, y0);
			char curr = Utility.CurrencyString(0)[0];
			string cars = "01²³4";
			using (Font font = new Font("Tahoma", 9))
			using (StringFormat format = new StringFormat())
			{
				format.Alignment = StringAlignment.Center;
				for (int i=0; i<state.NumPlayers; i++)
				{
					int x = plaqueLocation.X;
					int y = plaqueLocation.Y - (15*state.NumPlayers)/2 + 15 * i;
					string s = string.Format("{0}{1} {3}{4} {2}C", curr, state.PlayerInfo[i].Funds, citiesReached[i], Engine.Description[state.PlayerInfo[i].EngineType][0], cars[state.PlayerInfo[i].Cars]);
					using (Brush brush = new SolidBrush(Game.TrackColor[state.PlayerInfo[i].TrackColor]))
					{
						g.DrawString(s, font, brush, x, y, format);
					}
				}
			}
		}






/* EVENT HANDLERS ------------------------------------------------------------------------------------ */

		// When the mouse moves, update the location of the pointer. If we're building
		// track, update the build state. If we move over a city, show the source commodities.
		void UpdatePointer(int i, int j)
		{
			int x, y;

			if (i == ptrI && j == ptrJ)
				return;

			int ci = (i == -1 ? -1 : map[i, j].CityIndex);
			if (ci != mouseCity)
			{
				if (mouseCity != -1)
					ShowMouseCity(false);
				mouseCity = ci;
				if (mouseCity != -1 && map.Cities[mouseCity].Products.Count == 0)
					mouseCity = -1;
				if (mouseCity != -1)
					ShowMouseCity(true);
			}

			int size = building ? 50 : 10;

			if (ptrI != -1)
				if (map.GetCoord(ptrI, ptrJ, out x, out y))
					form.Invalidate(new Rectangle(x - size, y - size, 2*size, 2*size));

			if (building)
				BuildTrack(buildI, buildJ, i, j);

			if (!map.GetCoord(i, j, out x, out y))
			{
				ptrI = ptrJ = -1;
				return;
			}

			ptrI = i;
			ptrJ = j;
			ptrX = x;
			ptrY = y;

			form.Invalidate(new Rectangle(x - size, y - size, 2*size, 2*size));
		}

		// figure out what to do if the mouse is moved
		public void MouseMove(MouseEventArgs e)
		{
			int x, y, xx, yy;

			// if mouse isn't near a milepost, hide the pointer
			if (!map.GetNearest(e.X, e.Y, out x, out y))
				UpdatePointer(-1, -1);

			// if the milepost isn't at a valid screen location, hide the pointer
			else if (!map.GetCoord(x, y, out xx, out yy))
				UpdatePointer(-1, -1);

			// if mouse isn't close enough to a milepost, hide the pointer
			else if ((e.X-xx)*(e.X-xx)+(e.Y-yy)*(e.Y-yy)>26)
				UpdatePointer(-1, -1);

			// hide the pointer if it's not a land milepost
			else if (!map.IsLand(x, y))
				UpdatePointer(-1, -1);

			// hide the pointer at the center dot in major cities--no reason to end movement there or build from there
			else if (map[x, y].CityType == CityType.Capital)
				UpdatePointer(-1, -1);

			// if we're building track and the new milepost isn't next to the last one, hide the pointer
			else if (building && !map.IsAdjacentOrEqual(x, y, buildI, buildJ))
				UpdatePointer(-1, -1);

			// show the pointer at the new location, build track, highlight source commodities, etc.
			else
				UpdatePointer(x, y);

			// determine which contract the mouse is positioned over and update the display if this has changed
			int newContract = -1;
			if (e.X >= map.ImageSize.Width)
				if (e.Y >= contractTop - 4)
				{
					int c = (e.Y - contractTop + 4) / contractHeight;
					if (c >= 0 && c < Player.NumContracts)
					{
						newContract = c;
					}
				}

			if (newContract != mouseContract)
			{
				mouseContract = newContract;
				form.Invalidate(statusRect);
				((GameForm) form).InitRefreshTimer();
			}

			Point pt = new Point(e.X - statusRect.X, e.Y - statusRect.Y);

			// determine which source commodity the mouse is positioned over
			int newCommodity = -1;
			for (int i=0; i<2; i++)
				if (CommodityRect(i).Contains(pt))
					newCommodity = i;

			// and update the display if this has changed
			if (newCommodity != mouseCommodity)
			{
				mouseCommodity = newCommodity;
				form.Invalidate(statusRect);
			}

			// determine which freight car the mouse is positioned over
			int newFreight = -1;
			for (int i=0; i<state.ThisPlayer.Cars; i++)
				if (FreightRect(i).Contains(pt))
					newFreight = i;

			// and update the display if this has changed
			if (newFreight != mouseFreight)
			{
				mouseFreight = newFreight;
				form.Invalidate(statusRect);
			}

			// determine if the mouse is over the next-player rectangle
			bool b = NextPlayerRect.Contains(pt);
			// and update the display if this has changed
			if (b != inNextPlayerRect)
			{
				inNextPlayerRect = b;
				form.Invalidate(statusRect);
			}

			// determine if the mouse is over the upgrade-train rectangle
			b = TrainUpgradeRect.Contains(pt);
			// and update the display if this has changed
			if (b != inUpgradeRect)
			{
				inUpgradeRect = b;
				form.Invalidate(statusRect);
			}
		}

		// we go here if they right-click
		public void RightClick()
		{
			// cancel locomotive placement
			if (placingLocomotive)
			{
				if (ptrI != -1)
					form.Invalidate(new Rectangle(ptrX - 10, ptrY - 10, 20, 20));
				DonePlacingLocomotive();
				return;
			}

			// abort track building
			if (building)
			{
				AbortTrackBuilding();
				form.Refresh();
				return;
			}

			// not building or placing train, so move the train to the specified milepost
			if (ptrI != -1 && state.PlayerInfo[state.CurrentPlayer].X != -1)
			{
				// calculate fastest route
				map.ResetFloodMap();
				map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

				if (map[ptrI, ptrJ].Value == 0)
				{
					// we're already there
				}
				else if (map[ptrI, ptrJ].Value != int.MaxValue)
				{
					PushState(Resource.GetString("Game.TrainMovement"));
					BeginMove(ptrI, ptrJ, true);
				}
				else
					MessageBox.Show(Resource.GetString("Game.CannotReach"));
			}
		}

		public bool HasControl
		{
			get
			{
				if (!state.ThisPlayer.Human)
					return false;

				if (NetworkState.IsOffline)
					return true;

				if (Startup.LocalUsers.Contains(state.ThisPlayer.Name))
					return true;

				return false;
			}
		}

		// figure out what to do if they click the a mouse button
		public void MouseDown(MouseEventArgs e)
		{
			// don't do anything if it's the computer's turn
			if (!HasControl)
				return;

			// don't do anything if we're busy auto-building track
			if (suppressUI)
				return;

			// next player
			if (inNextPlayerRect)
			{
				if (state.AccumCost == 0 && state.AccumTime == 0 && !state.ThisPlayer.LoseTurn)
					if (MessageBox.Show(Resource.GetString("Game.NotSpentAny"), form.Text, MessageBoxButtons.YesNo) == DialogResult.No)
						return;
				NextPlayer();
				return;
			}

			// don't do anything if we've lost our turn
			if (state.ThisPlayer.LoseTurn)
				return;

			// they right-clicked, so cancel track building, or move the train
			if (e.Button == MouseButtons.Right)
			{
				RightClick();
				return;
			}

			// pick up commodity
			if (mouseCommodity != -1)
			{
				int ci = CurrentCity;
				if (ci == -1)
					return; // not at a city

				City city = map.Cities[ci];
				if (mouseCommodity > city.Products.Count)
					return; // can't click on commodities the city doesn't have

				// check if the product is available
				int product = (int) city.Products[mouseCommodity];
				if (state.Availability[product] == 0)
				{
					MessageBox.Show(Products.Name[product] + " is unavailable until another player unloads it.", form.Text);
					return;
				}

				// check if there's an empty freight car
				Player pl = state.ThisPlayer;
				int car = pl.EmptyCar;
				if (car == -1)
				{
					MessageBox.Show(Resource.GetString("Game.AllYourBase"), form.Text);
					return;
				}

				// check if there's a labor strike
				if (CityIsClosed(ci))
				{
					string message = string.Format(System.Globalization.CultureInfo.InvariantCulture, Resource.GetString("Game.CannotLoad"), city.Name);
					MessageBox.Show(message, form.Text);
					return;
				}

				// check if they're enough time to load cargo
				if (state.NoMoreTime)
				{
					MessageBox.Show(Resource.GetString("Game.NoTimeToLoad"), form.Text);
					return;
				}

				// all is well
				PushState(Resource.GetString("Game.CargoPickup"));
				pl.Cargo[car] = state.ReserveCommodity(product);
				SpendTime(loadingTime);
				form.Invalidate(statusRect);
				return;
			}

			// drop off cargo
			if (mouseFreight != -1)
			{
				int mf = mouseFreight;
				Player pl = state.ThisPlayer;
				int cargo = pl.Cargo[mf];
				if (cargo == -1)
					return; // freight car is empty

				// check if there's enough time to unload cargo
				if (state.NoMoreTime)
				{
					MessageBox.Show(Resource.GetString("Game.NoTimeToUnload"), form.Text);
					return;
				}

				PushState(Resource.GetString("Game.CargoDelivery"));
				bool earned = false;

				// are we at a city?
				if (CurrentCity != -1)
				{
					City city = map.Cities[CurrentCity];

					// check if there's a labor strike
					if (CityIsClosed(CurrentCity))
					{
						string message = string.Format(System.Globalization.CultureInfo.InvariantCulture, Resource.GetString("Game.CannotUnload"), city.Name);
						MessageBox.Show(message, form.Text);
						PopState();
						return;
					}

					// check if we earn any money for this delivery
					earned = FulfillContract(ref pl.Cargo[mf], CurrentCity);
				}

				if (!earned)
				{
					DialogResult dr = MessageBox.Show(Resource.GetString("Game.NoProfit"), form.Text, MessageBoxButtons.YesNo);
					if (dr == DialogResult.No)
					{
						PopState();
						return;
					}
					state.ReleaseCommodity(ref pl.Cargo[mf]);
				}

				// all is well
				SpendTime(loadingTime);
				form.Invalidate(statusRect);
				return;
			}

			// update train or place locomotive
			if (inUpgradeRect)
			{
				if (state.ThisPlayer.X == -1)
				{
					if (placingLocomotive)
						DonePlacingLocomotive();
					else
						PlaceLocomotive();
					return;
				}

				// make sure we haven't reached our spending limit for the turn
				if (state.AccumCost > 0)
				{
					string message = string.Format(System.Globalization.CultureInfo.CurrentUICulture, Resource.GetString("Game.NoMoneyToUpgrade"), Utility.CurrencyString(Engine.UpgradeCost));
					MessageBox.Show(message, form.Text);
					return;
				}

				// make sure we haven't reached our time limit for the turn
				if (state.NoMoreTime)
				{
					MessageBox.Show(Resource.GetString("Game.NoTimeToUpgrade"), form.Text);
					return;
				}

				// show the train upgrade dialog
				Form uf = new UpgradeTrain(state.ThisPlayer.EngineType, state.ThisPlayer.Cars);
				DialogResult dr = uf.ShowDialog(form);
				uf.Dispose();

				switch (dr)
				{
					case DialogResult.Cancel:
						return;

					case DialogResult.OK:
						PushState(Resource.GetString("Game.EngineUpgrade"));
						state.PlayerInfo[state.CurrentPlayer].EngineType++;
						break;

					case DialogResult.Yes:
						PushState(Resource.GetString("Game.TrainUpgrade"));
						state.PlayerInfo[state.CurrentPlayer].Cars++;
						break;
				}

				// all is well
				SpendMoney(Engine.UpgradeCost);
				SpendTime(Engine.UpgradeTime);
				form.Invalidate(statusRect);
				return;
			}

			// place train
			if (placingLocomotive)
			{
				if (ptrI != -1)
				{
					PushState(Resource.GetString("Game.TrainPlacement"));
					state.MovePlayerTo(ptrI, ptrJ);
				}
				form.Invalidate(new Rectangle(e.X - 20, e.Y - 10, 40, 20));
				DonePlacingLocomotive();
				ShowTrainRange();
				return;
			}

			// automatic build
			if ((e.Button & MouseButtons.Middle) != 0)
			{
				AutomaticBuild(ptrI, ptrJ);
				return;
			}

			// they didn't want to do anything else, so they must want to build track

			if (ptrI == -1)
				return;	// mouse not on accessible milepost

			// can build off existing track
			bool ok = PlayerTracksAt(ptrI, ptrJ, state.CurrentPlayer);

			// can build anywhere if train is not on map
			if (state.ThisPlayer.X == -1)
				ok = true;

			// can build from a port
			if (map.IsPort(ptrI, ptrJ))
				ok = true;

			// can build from anywhere train can get to
			if (!ok)
				if (CouldMoveTo(ptrI, ptrJ))
					ok = true;
			
			if (!ok)
				return;

			// start building
			PushState(Resource.GetString("Game.TrackBuilding"));
			building = true;
			buildI = ptrI;
			buildJ = ptrJ;
			buildStack = new Stack();
			buildCost = 0;
		}

		// they released the mouse button
		public void MouseUp(MouseEventArgs e)
		{
			if (!HasControl)
				return;

			if (state.ThisPlayer.LoseTurn)
				return;

			// finished building track?
			if (building)
			{
				// spent the time and the money
				SpendMoney(buildCost);
				form.Invalidate(statusRect);
				form.Invalidate(new Rectangle(e.X - 50, e.Y - 50, 100, 100));
				building = false;
				buildStack = null;
				ShowTrainRange();
				EndOfGameNotification();
			}
		}







/* TRAIN MOVEMENT ------------------------------------------------------------------------------------ */

		bool BeginMove(int x, int y, bool firstMove)
		{
			// create the list of individual track segment movement instructions
			moveStack = new Stack();
			GetMoveStack(x, y, moveStack);

			// set up a timer for the train movement animation
			moveTimer = new Timer();
			moveTimer.Tick += new EventHandler(MoveTimerHandler);
			Application.DoEvents();

			// do the first move
			return MoveAnimation(firstMove);
		}

		// calculate time taken to move between mileposts
		bool TrainMovementMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;

			bool sea = map.IsSea(x, y);
			bool port = map.IsPort(x, y);
			bool toSea = map.IsSea(i, j);
			bool toPort = map.IsPort(i, j);

			// check if this is an illegal move
			if (!(port && toSea) && !(sea && (toPort || toSea))) // land-to-land (sea movement is OK)
				if (state.Track[x, y, d] == -1 || !state.UseTrack[state.Track[x, y, d]]) // but no allowed track (otherwise OK)
					if (!map.IsCapital(x, y) || !map.IsCapital(i, j) || map[x, y].CityIndex != map[i, j].CityIndex) // and not within major city limits
						return false;

			// basic cost of movement
			if (sea || toSea)
				cost = seaMovementCost;
			else
				cost = Engine.SpeedInMinutes[state.ThisPlayer.EngineType];

			// adjust for random events such as snow, gale, etc.
			foreach (Disaster disaster in disasters)
				if (!disaster.AdjustMovementCost(x, y, d, i, j, ref cost))
					return false;

			return true;
		}

		// clean up movement animation info when movement is complete
		void StopMoveAnimation()
		{
			moveStack = null;
			if (moveTimer != null)
			{
				moveTimer.Dispose();
				moveTimer = null;
			}
			form.Invalidate(statusRect);
			ShowTrainRange();
		}

		// move one track segment at a time in the train movement animation
		bool MoveAnimation(bool firstMove)
		{
			bool moved = false;
#if TEST
fastLoop:
#endif
			// are we done moving?
			if (moveStack == null || moveStack.Count == 0)
			{
				StopMoveAnimation();
				return moved;
			}

			// don't move while computer prompt is displayed
			if (ind != null)
				return moved;

			Player pl = state.ThisPlayer;
			int x, y;
			int d = (int) moveStack.Pop();

			// if new location isn't adjacent to current location, something went wrong
			if (!map.GetAdjacent(pl.X, pl.Y, d, out x, out y))
			{
				System.Diagnostics.Debugger.Break(); // haven't hit this yet
			}

			// determine the time it takes to move one segment
			int time = 0;
			if (map.IsSea(pl.X, pl.Y) || map.IsSea(x, y))
				time = seaMovementCost;
			else
				time = Engine.SpeedInMinutes[pl.EngineType];

			// adjust for random events
			foreach (Disaster disaster in disasters)
				if (!disaster.AdjustMovementCost(pl.X, pl.Y, d, x, y, ref time))
					time = GameState.MaxTime;

			// stop if we don't have enough time left
			if (state.NoMoreTime)
			{
				StopMoveAnimation();
				if (firstMove)
					MessageBox.Show(Resource.GetString("Game.NoTimeToMove"), form.Text);
				return moved;
			}

			// redraw the train indicator
			int xx, yy;
			map.GetCoord(pl.X, pl.Y, out xx, out yy);
			form.Invalidate(new Rectangle(xx - 10, yy - 10, 20, 20));
			pl.X = x;
			pl.Y = y;
			pl.D = d;
			map.GetCoord(pl.X, pl.Y, out xx, out yy);

			// keep track of time spent
			SpendTime(time);
			moved = true;
			form.Invalidate(new Rectangle(xx - 10, yy - 10, 20, 20));

#if TEST
			Application.DoEvents();
			firstMove = false;
			goto fastLoop;
#else
			// animation frame rate is proportional to the train speed
			int ms = 2000 * time / GameState.MaxTime;
			moveTimer.Interval = ms;
			moveTimer.Start();
			return moved;
#endif			
		}

		void MoveTimerHandler(object sender, EventArgs e)
		{
			if (moveTimer == null)
				return;

			moveTimer.Stop();
			MoveAnimation(false);
		}

		// get the list of track segment moves, ignoring starting location
		void GetMoveStack(int i, int j, Stack stack)
		{
			int x, y;
			GetMoveStack(i, j, stack, out x, out y);
		}

		// get the list of track segment moves, keeping track of the starting location
		void GetMoveStack(int i, int j, Stack stack, out int x, out int y)
		{
			int limit = 1000; 
			while (map[i, j].Value > 0 && --limit >= 0)
			{
				// store the direction of the move
				int d = map[i, j].Gradient;
				if (d == byte.MaxValue)
					break;
				if (d < 0 || d > 5)
					throw new InvalidOperationException();
				if (stack != null)
					stack.Push(d);

				// get the next milepost
				if (!map.GetAdjacent(i, j, (d + 3) % 6, out i, out j))
					break;
			}
			x = i; y = j;
		}

		public void PlaceLocomotive()
		{
			placingLocomotive = true;
			form.Invalidate(statusRect);
		}

		void DonePlacingLocomotive()
		{
			placingLocomotive = false;
			form.Invalidate(statusRect);
			Sync();
		}

		// Move the computer's train to a specified city and calculate the time it takes.
		// The move isn't permanent--it's just used to evaluate contracts.
		int MoveToCity(int city)
		{
			// if the train isn't on the map, just place it there
			if (state.ThisPlayer.X == -1)
			{
				state.MovePlayerTo(map.Cities[city].X, map.Cities[city].Y);
				return 0;
			}

			// determine the time it takes to get to any point in the current system of track
			map.ResetFloodMap();
			int x = state.ThisPlayer.X;
			int y = state.ThisPlayer.Y;
			map.Flood(x, y, trainMovementMethod);

			// find the cheapest path to return to the starting point from the destination point
			Stack stack = new Stack();
			GetMoveStack(map.Cities[city].X, map.Cities[city].Y, stack);

			// calculate the movement speed for each segment in the path
			int time = 0;
			while (stack.Count > 0)
			{
				int d = (int) stack.Pop();
				int i, j;
				if (!map.GetAdjacent(x, y, d, out i, out j))
					break;
				if (map.IsSea(x, y) || map.IsSea(i, j))
					time += seaMovementCost;
				else
					time += Engine.SpeedInMinutes[state.ThisPlayer.EngineType];

				// note that we don't adjust the time for current disasters since disasters
				// are temporary and we don't know which disasters will still be in effect by
				// the time the train gets to this location

				x = i; y = j;
			}
			state.MovePlayerTo(x, y);
			return time;
		}

		// what city is the train at?
		int CurrentCity
		{
			get
			{
				Player pl = state.ThisPlayer;

				// locomotive not on map
				if (pl.X == -1)
					return -1;

				// not at a city
				if (!map.IsCity(pl.X, pl.Y))
					return -1;

				return map[pl.X, pl.Y].CityIndex;
			}
		}

		// Move the computer's train toward a city (for real). The track may not reach
		// all the way there yet, so get as close as possible.
		bool MoveTowardCity(int city)
		{
			TopOffCars();

			int x, y;
			City c = map.Cities[city];

			// determine all the map locations the train can reach
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			// if able to move all the way to city, determine exact destination
			if (map[c.X, c.Y].Value != int.MaxValue)
			{
				if (map[c.X, c.Y].CityIndex >= map.NumCapitals)
					return BeginMove(c.X, c.Y, false);
				
				// determine which corner of capital is best
				int bestX = -1, bestY = -1;
				int bestDist = int.MaxValue;
				for (int d=0; d<6; d++)
					if (map.GetAdjacent(c.X, c.Y, d, out x, out y))
						if (map[x, y].Value < bestDist)
						{
							bestDist = map[x, y].Value;
							bestX = x; bestY = y;
						}
				if (bestX != -1)
					return BeginMove(bestX, bestY, false);
			}

			// determine cost of building to all other map locations
			map.ZeroFloodMap();
			map.Flood(buildTrackMethod);

			// check if tracks already go there (could happen during a disaster)
			if (map[c.X, c.Y].Value == 0)
			{
				// find where train *can* currently move with disasters in place
				map.ResetFloodMap();
				map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

				// mark currently accessible locations as possible starting point
				// once disasters are gone
				map.ZeroFloodMap();

				// temporarily supress disasters and figure out how to get
				// the rest of the way
				Queue save = disasters;
				disasters = new Queue();
				map.Flood(trainMovementMethod);

				// determine the route to find the closest accessible milepost
				GetMoveStack(c.X, c.Y, null, out x, out y);

				// restore disasters
				disasters = save;

				// do the move
				map.ResetFloodMap();
				map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);
				return BeginMove(x, y, false);
			}

			// make sure it's possible to build there
			if (map[c.X, c.Y].Value == int.MaxValue)
				return false; // can't build to there

			// find spot where new track would intersect current track
			GetMoveStack(c.X, c.Y, null, out x, out y);

			// check if we're already as close as we can get
			if (x == state.ThisPlayer.X && y == state.ThisPlayer.Y)
				return false;

			// prepare for BeginMove
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			if (map[x, y].Value == int.MaxValue)
			{
				// how is this possible?
#if TEST
				this.ScreenShot();
#endif
				return false;
			}

			return BeginMove(x, y, false);
		}

		// determine the remaining range of movement for the current player's train
		void ShowTrainRange()
		{
			BlinkTrain(false);

			if (!state.ThisPlayer.Human || moveTimer != null)
				return;

			// determine which mileposts can be reached in remaining time
			if (canReachInTime == null)
				canReachInTime = new bool[map.GridSize.Width, map.GridSize.Height];

			// calculate how far the train can reach
			map.ResetFloodMap();
			if (state.ThisPlayer.X != -1)
			{
				map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);
			}

			int speed = Engine.SpeedInMinutes[state.ThisPlayer.EngineType];
			int n = 0;
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					n += (canReachInTime[x, y] = (map[x, y].Value < GameState.MaxTime + speed - state.AccumTime)) ? 1 : 0;
        
			BlinkTrain(n > 1);
			form.Invalidate();
		}

		// could we move to a specific location if we wanted to?
		bool CouldMoveTo(int cx, int cy)
		{
			return CouldMoveTo(cx, cy, state.ThisPlayer);
		}

		// could a specific player move to a specific location if they wanted to?
		bool CouldMoveTo(int cx, int cy, Player pl)
		{
			if (pl.X == -1)
				return false;

			if (pl.X == cx && pl.Y == cy)
				return true;

			map.ResetFloodMap();
			map.Flood(pl.X, pl.Y, trainMovementMethod);
			return map[cx, cy].Value != int.MaxValue;
		}

		public void UseOtherTrack()
		{
			using (ChooseTrack chooseTrack = new ChooseTrack(state))
			{
				if (chooseTrack.ShowDialog(form) == DialogResult.OK)
				{
					int cost = 0;
					for (int i=0; i<state.NumPlayers; i++)
						if (chooseTrack.UseTrack[i] && !state.UseTrack[i])
							cost += Game.useTrackCost;

					if (state.AccumCost + cost > GameState.MaxSpend)
					{
						string message = string.Format(System.Globalization.CultureInfo.InvariantCulture, Resource.GetString("Game.OtherTrackCost"), Utility.CurrencyString(Game.useTrackCost));
						MessageBox.Show(message, form.Text);
						return;
					}

					this.PushState(Resource.GetString("Game.UseOfOther"));
					lockSync = true;
					SpendMoney(cost);
					for (int i=0; i<state.NumPlayers; i++)
						if (chooseTrack.UseTrack[i] && !state.UseTrack[i])
							state.PlayerInfo[i].Receive(Game.useTrackCost);
					Array.Copy(chooseTrack.UseTrack, state.UseTrack, state.UseTrack.Length);
					ShowTrainRange();
					lockSync = false;
					Sync();
				}
			}
		}






/* BUILDING TRACK ------------------------------------------------------------------------------------ */

		// build cheapest track automatically
		void AutomaticBuild(int x, int y)
		{
			if (!options.AutomaticTrackBuilding)
				return;

			if (x == -1)
				return; // train isn't on the map yet

			// wait for train to stop moving
			while (moveTimer != null)
			{
				Application.DoEvents();
			}

			PushState("track building");
			suppressUI = true;
			int cost = BuildTrackToLocation(x, y, true);
			suppressUI = false;

			// indicate error if we can't build there
			if (cost == -1)
			{
				PopState();
				int ci = map[x, y].CityIndex;
				if (ci == -1)
					MessageBox.Show("There is no way to connect your track to that location.", form.Text);
				else
					MessageBox.Show("There is no way to connect your track to " + map.Cities[ci].Name + ".", form.Text);
				return;
			}

			// spend the money
			SpendMoney(cost);
			ShowTrainRange();
			EndOfGameNotification();
		}

		// determine if track capacity of city or town allows new track
		bool TownBuildAllowed(int x, int y)
		{
			int ci = map[x, y].CityIndex;
			if (ci == -1)
				return true; // not a town or city

			if (ci < map.NumCapitals)
				return true; // major city doesn't count

			// keep track of which players' tracks are here
			bool[] pb = new Boolean[state.NumPlayers];
			pb[state.CurrentPlayer] = true;

			int np = 1;	// number of players with track to this town
			int pt = 1;	// pieces of current player's track to this town (including new track)

			int i, j;
			for (int d=0; d<6; d++)
				if (map.GetAdjacent(x, y, d, out i, out j))
				{
					int t = state.Track[x, y, d];
					if (t != -1 && t != state.NumPlayers)
					{
						if (!pb[t]) // count the number of players with track
						{
							pb[t] = true;
							np++;
						}
						if (t == state.CurrentPlayer) // count this player's track
							pt++;
					}
				}

			// check maximum number of players to this town
			int mnp = 2 + (ci < map.NumCapitals + map.NumCities ? 1 : 0);
			if (np > mnp)
				return false;

			// maximum number of track segments per player
			if (pt > 3)
				return false;

			return true;
		}

		bool avoidNationalizedTrack = false;

		// determine how much a track segment will cost to build
		//		i1,j1 = from	i2,j2 = to
		int CostOfBuildingTrack(int i1, int j1, int i2, int j2)
		{
			int cost = 0;

			// can't build within a major city hexagon
			if (map.IsCapital(i1, j1) && map.IsCapital(i2, j2) && map[i1, j1].CityIndex == map[i2, j2].CityIndex)
				return 0;

			// check if we're allowed to build out of the city
			if (!TownBuildAllowed(i1, j1))
				return -1;

			// check if we're allowed to build into the city
			if (!TownBuildAllowed(i2, j2))
				return -1;

			// cost of terrain
			switch(map[i2, j2].Terrain)
			{
				case TerrainType.Inaccessible:
					cost = 100;
					break;
				case TerrainType.Sea:
					cost = 0;
					break;
				case TerrainType.Clear:
					cost = 1;
					break;
				case TerrainType.Mountain:
					cost = 2;
					break;
				case TerrainType.Alpine:
					cost = 5;
					break;
				case TerrainType.Port:
					cost = state.Turn > 2 ? 4 : 1;	// port surcharge omitted during first 2 turns
					break;
			}

			// cost of city entry
			switch(map[i2, j2].CityType)
			{
				case CityType.None:
					break;
				case CityType.Town:
				case CityType.City:
					cost += 2;
					break;
				case CityType.Capital:			// shouldn't get here
				case CityType.CapitalCorner:	// should get here
					cost += state.Turn > 2 ? 4 : 0;		// it's free during the first two turns
					break;
			}

			// cost of starting at a new port
			if (state.Turn > 2)	// free first two turns
				if (map.IsPort(i1, j1))
					if (!PlayerTracksAt(i1, j1, state.CurrentPlayer))
						cost += 3;

			// determine direction
			int i3, j3, dir = -1;
			for (int d=0; d<6; d++)
				if (map.GetAdjacent(i1, j1, d, out i3, out j3))
					if (i2 == i3 && j2 == j3)
					{
						dir = d;
						break;
					}

			if (dir != -1)
			{
				// cost of crossing water
				int wm = map[i1, j1].WaterMask;
				if ((wm & WaterMasks.InletMask[dir]) != 0)
					cost += 3;
				else if ((wm & WaterMasks.RiverMask[dir]) != 0)
					cost += 2;

				// adjust cost based on random events
				foreach (Disaster disaster in disasters)
					if (!disaster.AdjustBuildingCost(i1, j1, dir, i2, j2, ref cost))
						return -1;

				// avoid nationalized track by artifically inflating the cost
				if (avoidNationalizedTrack)
					if (map.IsNationalized(i1, j1, dir))
						cost += 1;
			}

			return cost;
		}

		// build a segment of track between i1,j1 and i2,j2
		void BuildTrack(int i1, int j1, int i2, int j2)
		{
			int dd = -1, i3, j3;

			// determine which direction that is
			for (int d=0; d<6; d++)
				if (map.GetAdjacent(i1, j1, d, out i3, out j3))
					if (i3 == i2 && j3 == j2)
						dd = d;
			if (dd == -1)
				return;

			// can't build inside a major city hexagon
			if (map.IsCapitalCorner(i1, j1) && map.IsCapitalCorner(i2, j2) && map[i1, j1].CityIndex == map[i2, j2].CityIndex)
				return;

			// get the opposite direction, too
			int d2 = (dd + 3) % 6;

			// are we back-tracking?
			if (buildStack.Count > 0)
			{
				TrackRecord rec = (TrackRecord) buildStack.Peek();
				if (rec != null)
					if (i1 == rec.i && j1 == rec.j && dd == rec.d)
					{
						// yes, so erase the last segment of track
						state.Track[i1, j1, dd] = -1;
						state.Track[i2, j2, d2] = -1;
//						NetworkState.Transmit("BuildTrack", i1, j1, dd, -1);
						Sync();
						buildStack.Pop();
						buildI = i2;
						buildJ = j2;
						buildCost = rec.c;
						return;
					}
			}

			// can't build on top of existing track segment
			if (state.Track[i1, j1, dd] != -1)
				return;

			// determine the cost, and whether or not the build is permitted
			int cost = CostOfBuildingTrack(i1, j1, i2, j2);
			if (cost == -1)
				return;	// build not allowed

			// check if we've reached our spending limit for the turn
			if (state.AccumCost + buildCost + cost > GameState.MaxSpend)
				return;

			// all is well
			if (map.IsNationalized(i1, j1, dd))
			{
				state.Track[i1, j1, dd] = state.NumPlayers;
				state.Track[i2, j2, d2] = state.NumPlayers;
			}
			else
			{
				state.Track[i1, j1, dd] = state.CurrentPlayer;
				state.Track[i2, j2, d2] = state.CurrentPlayer;
			}
//			NetworkState.Transmit("BuildTrack", i1, j1, dd, state.Track[i1, j1, dd]);
			Sync();
			buildStack.Push(new TrackRecord(i2, j2, d2, buildCost));
			buildI = i2;
			buildJ = j2;
			buildCost += cost;
		}

		// Is a segment of a specific player's track located at a specific location?
		bool PlayerTracksAt(int i, int j, int p)
		{
			if (i == state.PlayerInfo[p].X && j == state.PlayerInfo[p].Y)
				return true; // if that's where their train is, act like there's track there, too

			// check all six directions
			for (int d=0; d<6; d++)
				if (state.Track[i, j, d] == p)
					return true;

			// no track segments found
			return false;
		}

		// check if the train would be able to move to a specific location via track and/or sea
		bool TracksGoTo(int i, int j)
		{
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);
			return map[i, j].Value != int.MaxValue;
		}

		// delegate for path-finding algorithm that determines where track builds are allowed
		// and how much they cost, in order to find cheapest path for track building
		bool BuildTrackMethod(int x, int y, int d, int i, int j, out int cost)
		{
			cost = 0;

			bool fromSea = map.IsSea(x, y);
			bool fromPort = map.IsPort(x, y);
			bool toSea = map.IsSea(i, j);
			bool toPort = map.IsPort(i, j);

			// don't need to build track on sea--can move there at will
			if (fromSea && (toSea || toPort))
			{
				cost = 0;
				return true;
			}

			// can't move from sea to land, except at a port
			if (fromSea)
				return false;

			// don't need to build track to embark from a port 
			if (toSea && fromPort)
			{
				cost = 0;
				return true;
			}

			// can't move to sea from land, except at a port
			if (toSea)
				return false;

			// it's free if we already have track at this segment
			if (state.Track[x, y, d] == state.CurrentPlayer)
			{
				cost = 0;
				return true;
			}

			// it's free if there is nationalized track at this segment
			if (state.Track[x, y, d] == state.NumPlayers)
			{
				cost = 0;
				return true;
			}

			// prohibited if somebody else's track is here
			if (state.Track[x, y, d] != -1)
				return false;

			// free to move within major city hexagon
			if (map.IsCapital(x, y) && map.IsCapital(i, j) && map[x, y].CityIndex == map[i, j].CityIndex)
			{
				cost = 0;
				return true;
			}

			// otherwise the cost is terrain-specific and disaster-specific
			cost = CostOfBuildingTrack(x, y, i, j);
			return cost != -1;
		}

		// Build the cheapest track possible to connect to a specific city.
		// Don't honor the per-turn spending limit. This is only used by computer
		// players in order to evaluate contracts.
		int BuildTrackToCity(int city)
		{
			return BuildTrackToCity(city, false);
		}

		delegate void FourInt32Delegate(int a, int b, int c, int d);

		// draw one new track segment and pause for a moment
		void NewTrackAnimation(int x, int y, int i, int j)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new FourInt32Delegate(NewTrackAnimation), new object[] {x, y, i, j});
				return;
			}
#if TEST
#else
			int xx, yy, ii, jj;
			if (!map.GetCoord(x, y, out xx, out yy))
				return;
			if (!map.GetCoord(i, j, out ii, out jj))
				return;

			// draw the new track segment
			Rectangle r = new Rectangle(Math.Min(xx, ii) - 5, Math.Min(yy, jj) - 5, Math.Abs(xx - ii) + 10, Math.Abs(yy - jj) + 10);
			form.Invalidate(r);
			Application.DoEvents();

			// wait 1/20th of a second
			System.Threading.Thread.Sleep(50);
#endif
		}

		// build cheapest track to connect to a specific city, and indicate whether
		// to honor the per-turn spending limit
		int BuildTrackToCity(int city, bool useSpendingLimit)
		{
			City c = map.Cities[city];
			return BuildTrackToLocation(c.X, c.Y, useSpendingLimit);
		}

		// build cheapest track to connect to a specific location, and indicate whether
		// to honor the per-turn spending limit
		int BuildTrackToLocation(int cx, int cy, bool useSpendingLimit)
		{
			// train isn't on the map, so we don't need to build to get there, just place train there
			if (state.ThisPlayer.X == -1)
				return 0;

			// find all the spots where the train can currently reach
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			// are we already at the target location?
			if (map[cx, cy].Value != int.MaxValue)
				return 0;

			// find the cheapest way to build to each map location
			map.ZeroFloodMap();	// don't ResetFloodMap!
			avoidNationalizedTrack = !state.ThisPlayer.Human;
			map.Flood(buildTrackMethod);

			// can we get there with track?
			if (map[cx, cy].Value == int.MaxValue)
				return -1; // no

			// determine the route
			int x, y;
			Stack stack = new Stack();
			GetMoveStack(cx, cy, stack, out x, out y);

			// calculate the real cost if avoided nationalized track
			if (avoidNationalizedTrack)
			{
				avoidNationalizedTrack = false;
				map.Flood(buildTrackMethod);
			}

			// lay the track
			int cost = 0;
			bool blinkSave = this.blinkingTrain;
			BlinkTrain(false);
			while (stack.Count > 0)
			{
				int d = (int) stack.Pop();
				int i, j;
				if (!map.GetAdjacent(x, y, d, out i, out j))
					break;
				if (!useSpendingLimit || (map[i, j].Value <= GameState.MaxSpend - state.AccumCost))
				{
					if (map.IsLand(x, y) && map.IsLand(i, j))
						if (!map.IsCapital(x, y) || !map.IsCapital(i, j) || map[x, y].CityIndex != map[i, j].CityIndex)
						{
							if (map.IsNationalized(x, y, d))
							{
								state.Track[x, y, d] = state.NumPlayers;
								state.Track[i, j, (d+3) % 6] = state.NumPlayers;
							}
							else
							{
								state.Track[x, y, d] = state.CurrentPlayer;
								state.Track[i, j, (d+3) % 6] = state.CurrentPlayer;
							}
							//							NetworkState.Transmit("BuildTrack", x, y, d, state.Track[x, y, d]);
							Sync();
							if (useSpendingLimit)
								NewTrackAnimation(x, y, i, j);
						}
					cost = map[i, j].Value;
				}
				x = i; y = j;
			}
			BlinkTrain(blinkSave);

			return cost;
		}

		/*
		// build best track to connect to a contract destination
		int BuildBestTrackToDestination(Contract c)
		{
			// train isn't on the map, so we don't need to build to get there, just place train there
			if (state.ThisPlayer.X == -1)
				return 0;

			Stack stack = this.FindBestTrack(c);

			// lay the track
			int x = state.ThisPlayer.X;
			int y = state.ThisPlayer.Y;
			int cost = 0;
			bool blinkSave = this.blinkingTrain;
			BlinkTrain(false);
			while (stack.Count > 0)
			{
				int d = (int) stack.Pop();
				int i, j;
				if (!map.GetAdjacent(x, y, d, out i, out j))
					break;
				if (state.Track[x, y, d] == -1)
					if (map.IsLand(x, y) && map.IsLand(i, j))
						if (!map.IsCapital(x, y) || !map.IsCapital(i, j) || map[x, y].CityIndex != map[i, j].CityIndex)
						{
							int cost2 = this.CostOfBuildingTrack(x, y, i, j);
							if (cost + cost2 <= GameState.MaxSpend - state.AccumCost)
							{
								cost += cost2;
								if (map.IsNationalized(x, y, d))
								{
									state.Track[x, y, d] = state.NumPlayers;
									state.Track[i, j, (d+3) % 6] = state.NumPlayers;
								}
								else
								{
									state.Track[x, y, d] = state.CurrentPlayer;
									state.Track[i, j, (d+3) % 6] = state.CurrentPlayer;
								}
								// NetworkState.Transmit("BuildTrack", x, y, d, state.Track[x, y, d]);
								Sync();
								NewTrackAnimation(x, y, i, j);
							}
						}
				x = i; y = j;
			}
			BlinkTrain(blinkSave);

			return cost;
		}
		*/

		public static void BuildTrackRemote(int x, int y, int d, int c)
		{
			if (Current != null)
				Current.BuildTrackInternal(x, y, d, c);
		}

		void BuildTrackInternal(int x, int y, int d, int c)
		{
			state.Track[x, y, d] = c;
			int i, j;
			if (map.GetAdjacent(x, y, d, out i, out j))
			{
				state.Track[i, j, (d+3)%6] = c;
				NewTrackAnimation(x, y, i, j);
			}
		}

		// Build toward a new major city in order to meet the end-of-game requirements.
		// Be sure to spend as little as possible.
		bool BuildToCheapestCapital()
		{
			// determine map locations where our track already reaches
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			// determine cost of building to all other map locations
			map.ZeroFloodMap();
			map.Flood(buildTrackMethod);

			// determine which major city corner would be cheapest to build to
			int min = int.MaxValue;
			int bx = -1, by = -1;
			int x, y;
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x,y].CityType == CityType.CapitalCorner)
						if (map[x,y].Value > 0 && map[x,y].Value < min)
						{
							min = map[x,y].Value;
							bx = x; by = y;
						}

			// we can't get to any more major cities
			if (bx == -1)
			{
				return false;
			}

			// build track toward the city
			ShowComputerIntent("Building track toward " + map.Cities[map[bx, by].CityIndex].Name + " for major city quota");
			int cost = BuildTrackToCity(map[bx, by].CityIndex, true);
			if (cost == 0) // at spending limit
			{
				HideComputerIntent();
				return false;
			}

			// check for switch to end-game strategy even if we no longer meeting the money requirement
			if (PlayerMeetsCityRequirements(state.CurrentPlayer))
				state.ThisPlayer.IsInEndGame = true;

			// spend the time and the money and update the display
			SpendMoney(cost);
			form.Invalidate();
			HideComputerIntent();
			return true;
		}

		// could we build track to a specific location if we wanted to?
		bool CouldBuildTo(int cx, int cy)
		{
			return CouldBuildTo(cx, cy, state.ThisPlayer);
		}

		// could a specific player build track to a specific location if they wanted to?
		bool CouldBuildTo(int cx, int cy, Player pl)
		{
			if (pl.X == -1)
				return true;

			map.ResetFloodMap();
			map.Flood(pl.X, pl.Y, trainMovementMethod);
			if (map[cx, cy].Value != int.MaxValue)
				return true;

			map.ZeroFloodMap();
			map.Flood(buildTrackMethod);

			return (map[cx, cy].Value != int.MaxValue);
		}

		// How much would it cost for a given player to build to all the major cities
		// they would need to win? This is used by the player rankings display.
		public int CityQuota(int player)
		{
			lockSync = true;
			Player pl = state.PlayerInfo[player];

			// save the track state and player state because we're not *really* going to build
			int[,,] saveTrack = (int[,,]) state.Track.Clone();
			int current = state.CurrentPlayer;
			bool[] useTrack = (bool[]) state.UseTrack.Clone();
			state.CurrentPlayer = player;
			for (int i=0; i<state.NumPlayers; i++)
				state.UseTrack[i] = (i == player);
			Point location = new Point(pl.X, pl.Y);

			// stick the player at one of the major cities if the game is just starting
			if (pl.X == -1)
				state.MovePlayerTo(map.Cities[0].X, map.Cities[0].Y);

			int quota = 0;
			int last = 0;
			while (true)
			{
				// find where our tracks already go
				map.ResetFloodMap();
				if (pl.X == -1)
					map.SetValue(map.Cities[0].X, map.Cities[0].Y, 0);
				else
					map.SetValue(pl.X, pl.Y, 0);
				map.Flood(trainMovementMethod);

				// find out costs to build to places we don't go
				map.ZeroFloodMap();
				map.Flood(buildTrackMethod);

				// find cheapest capital to connect
				int min = int.MaxValue;
				int best = -1;
				for (int i=0; i<map.NumCapitals; i++)
				{
					City city = map.Cities[i];
					int val = map[city.X, city.Y].Value;
					if (val > 0 && val != int.MaxValue)
						if (val < min)
						{
							min = val;
							best = i;
						}
				}

				// quit if there are no more cities to connect to
				if (min == int.MaxValue)
				{
					quota -= last;	// we don't have to build to the last city
					break;
				}

				quota += min;
				last = min;

				// build to cheapest city
				BuildTrackToCity(best, false);
			}

			// restore the game state
			state.Track = saveTrack;
			state.MovePlayerTo(location.X, location.Y);
			state.CurrentPlayer = current;
			state.UseTrack = useTrack;
			lockSync = false;
			return quota;
		}

		private void AbortTrackBuilding()
		{
			if (!building) return;
			PopState();
			building = false;
			buildStack = null;
		}

		public void Deactivate()
		{
			AbortTrackBuilding();
		}







/* USER INTERFACE BOUNDARY RECTANGLES ---------------------------------------------------------------- */

		// calculate a freight car boundary rectangle
		Rectangle FreightRect(int i)
		{
			int freightTop = 102;
			int freightSize = 32;

			int cars = state.ThisPlayer.Cars;
			int sp = (statusRect.Width - freightSize * cars) / (cars + 1);
			int h = sp + i * (freightSize + sp);
			return new Rectangle(h, freightTop, freightSize, freightSize);
		}

		// calculate a city commodity boundary rectangle
		Rectangle CommodityRect(int i)
		{
			int commodityTop = 156;
			int commoditySize = 32;

			Rectangle rect = Rectangle.Empty;
			int ci = CurrentCity;
			if (ci == -1) return rect;

			int n = map.Cities[ci].Products.Count;
			if (i >= n) return rect;

			int sp = (statusRect.Width - commoditySize * n) / (n + 1);
			int h = sp + i * (commoditySize + sp);
			return new Rectangle(h, commodityTop, commoditySize, commoditySize);
		}

		// get the next-player boundary rectangle
		Rectangle NextPlayerRect
		{
			get 
			{
				return new Rectangle(0, 0, statusRect.Width, 64);
			}
		}

		// get the upgrade-train boundary rectangle
		Rectangle TrainUpgradeRect
		{
			get
			{
				return new Rectangle(0, 64, statusRect.Width, 32);
			}
		}







/* TURN MANAGEMENT ----------------------------------------------------------------------------------- */

		// go to the next player's turn
		public void NextPlayer()
		{
			// wait until the train movement animation is finished
			while (moveStack != null)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(0);
			}

			// count how many cities are reached, for plaque display
			PlayerMeetsCityRequirements(state.CurrentPlayer);

			// turn back into a pumpkin
			if (state.ThisPlayer.TemporaryBot)
			{
				state.ThisPlayer.Human = true;
				state.ThisPlayer.TemporaryBot = false;
			}

			// advance the game state
			state.NextPlayer();

			// redraw the status area
			DonePlacingLocomotive();

			// check if there's a new random event, or if a random event has elapsed
			HandleDisasters();

			// save the game state to the game save file
			Save();

			// start over with a new Undo stack
			UndoStack = new Stack();

			// show the new player's train range
			canReachInTime = null;
			ShowTrainRange();

			// check if the game is over
			CheckEndOfGameCondition();

			// synchronize other players in multiplayer game
			Sync();
		}

		// save the game state to the game save file
		public void Save()
		{
			try
			{
				Stream s = new FileStream("game.sav", FileMode.Create);
				BinaryWriter w = new BinaryWriter(s);
				Save(w);
				w.Close();
				s.Close();
			}
			catch(IOException)
			{
			}
		}

		// increase the amount of time elapsed and update train range display
		void SpendTime(int time)
		{
			state.AccumTime += time;
			ShowTrainRange();
			Sync();
		}

		// increase the amount of money spent
		void SpendMoney(int amount)
		{
			state.AccumCost += amount;
			state.ThisPlayer.Spend(amount);
			Sync();
			UpdatePlaque();
		}

		void EndOfGameNotification()
		{
			if (!PlayerMeetsRequirements(state.CurrentPlayer))
				return;

#if TEST
#else
			if (state.ThisPlayer.Human)
				MessageBox.Show(Resource.GetString("Game.EndOfGame"), form.Text);
#endif
		}

		int[] citiesReached = new int[Game.MaxPlayers];

		bool PlayerMeetsCityRequirements(int i)
		{
			if (state.ThisPlayer.X == -1)
				return false;

			// save current player and track permissions
			int cp = state.CurrentPlayer;
			bool[] useTrack = (bool[]) state.UseTrack.Clone();
			state.CurrentPlayer = i;
			for (int j=0; j<state.NumPlayers; j++)
				state.UseTrack[j] = (j == i);

			// find all the map locations where their train can reach
			// assuming current disasters have ended
			Queue save = disasters;
			disasters = new Queue();
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);
			disasters = save;

			// count the number of major cities we can reach
			int cities = 0;
			for (int j=0; j<map.NumCapitals; j++)
				if (CouldMoveTo(map.Cities[j].X, map.Cities[j].Y))
					cities++;
				else if (!CouldBuildTo(map.Cities[j].X, map.Cities[j].Y))
					cities++; // it counts if it's impossible to build to it

			citiesReached[i] = cities;

			// restore current player and track permissions
			state.CurrentPlayer = cp;
			state.UseTrack = useTrack;

			// do we have enough major cities?
			int okToMiss = map.NumCapitals / 7;
			return (cities >= map.NumCapitals - okToMiss);
		}

		// check if a specific player meets the end-of-game requirements
		bool PlayerMeetsRequirements(int i)
		{
			// can't win if they're not on the map
			if (state.PlayerInfo[i].X == -1)	
				return false;

			// do they have enough money?
			if (state.PlayerInfo[i].Funds < options.FundsGoal)
				return false;

			// do they reach enough cities?
			if (!PlayerMeetsCityRequirements(i))
				return false;

			return true;
		}

		// check if somebody has won
		public void CheckEndOfGameCondition()
		{
			if (state.Winners != null)		// yes, we already knew that
				return;

			ArrayList winners = new ArrayList();
			for (int i=0; i<state.NumPlayers; i++)
				if (PlayerMeetsRequirements(i))
					winners.Add(i);

			if (winners.Count == 0)
				return;

			state.DisableTax = true;

			if (state.CurrentPlayer != 0)
			{
#if TEST
#else
				MessageBox.Show("One or more players has met the requirements for winning, but each remaining player is allowed to complete their final turn.", form.Text);
#endif
				return;
			}

			// resolve ties
			int max = int.MinValue;
			int nwinners = 0;
			int ultimateWinner = -1;
			foreach (int winner in winners)
				if (state.PlayerInfo[winner].Funds > max)
				{
					max = state.PlayerInfo[winner].Funds;
					nwinners = 1;
					ultimateWinner = winner;
				}
				else if (state.PlayerInfo[winner].Funds == max)
					nwinners++;

			if (nwinners == 1)
			{
				winners = new ArrayList();
				winners.Add(ultimateWinner);
			}

			// at least one person won
			state.Winners = winners;
			ReportEndOfGame();
		}

		public void ReportEndOfGame()
		{
			
			// game is over, so delete the game save file
			try
			{
				File.Delete("game.sav");
			}
			catch(IOException)
			{
			}
			
#if TEST
			// take a snapshot of the map for our scrap book
			// (will fail if form is minimized)
//			ScreenShot();
			LogPayoff(-999);
#else
			RecordResult();

			Sync();
			if (state.Winners.Count == 1)
			{
				string s = state.PlayerInfo[(int) state.Winners[0]].Name + " is the winner!";
				NetworkState.Transmit("Winner", s);
				MessageBox.Show(s, form.Text);
			}
			else
			{
				string s = "Multiple winners:";
				foreach (int winner in state.Winners)
					s += "\r\n\t" + state.PlayerInfo[winner].Name;
				NetworkState.Transmit("Winner", s);
				MessageBox.Show(s, form.Text);
			}
#endif
		}

#if TEST
		void ScreenShot()
		{
			Bitmap capture = new Bitmap(form.ClientRectangle.Width, form.ClientRectangle.Height);
			Graphics gcapture = Graphics.FromImage(capture);
			this.Paint(new PaintEventArgs(gcapture, form.ClientRectangle));
			gcapture.Dispose();
			string filename = DateTime.Now.ToShortTimeString().Replace(":", "");
			capture.Save(filename + ".png");
			capture.Dispose();
		}
#endif

		// Is the game over? (The form's idle processor wants to know)
		public bool IsOver
		{
			get
			{
				return state.Winners != null;
			}
		}

		// Is the game past the point where quitting would be counted as losing?
		public bool IsOfficial
		{
			get
			{
//				bool official = false;
//				foreach (Player p in state.PlayerInfo)
//					if (p.Funds + Engine.UpgradeCost * p.EngineType >= 100)
//						official = true;
//				return official;
				return state.Turn > 5;
			}
		}

		public void RecordResult()
		{
			if (state.Winners == null)
				state.Winners = new ArrayList();
			GameResultsCollection.Record(new GameResult(this));
		}

		// Quit the game, recording the loss if the game is already official. User has the option
		// of cancelling without quitting.
		public bool Quit()
		{
			if (!IsOfficial)
				return true;

			if (state.DisableTax)
				return true;

			DialogResult dr = MessageBox.Show(Resource.GetString("Game.AlreadyOfficial"), Resource.GetString("Rails"), MessageBoxButtons.YesNo);
			return dr == DialogResult.Yes;
		}

		// Is the UI waiting for a human player to make a choice?
		public bool WaitingForHuman
		{
			get
			{
				if (!state.ThisPlayer.Human)
					return false;
				if (moveTimer != null)
					return false;
				return true;
			}
		}







/* MULTI-LEVEL UNDO FUNCTIONALITY -------------------------------------------------------------------- */

		// is there anything in the undo stack?
		public bool CanUndo
		{
			get
			{
				return UndoStack.Count > 0;
			}
		}

		// what should we call the undo action?
		public string UndoLabel
		{
			get
			{
				return "Undo " + ((StateFrame) UndoStack.Peek()).Message;
			}
		}

		// save the current game state to the undo stack
		void PushState(string message)
		{
			UndoStack.Push(new StateFrame(state, message));
		}

		// restore the game state from the undo stack
		void PopState()
		{
			if (UndoStack.Count == 0)
				return;
			StateFrame frame = (StateFrame) UndoStack.Pop();
			frame.Restore(state);
			Sync();
		}

		// restore the game state from the undo stack and redraw the form
		public void Undo()
		{
			PopState();
			ShowTrainRange();
		}

		// synchronize game state with remote computer
		public static void SyncGameDataFull(byte[] data)
		{
			if (Current != null)
				Current.SyncGameDataFullInternal(data);
		}

		public static void SyncGameData(ArrayList delta)
		{
			if (Current != null)
				Current.SyncGameDataInternal(delta);
		}

		delegate void ByteArrayDelegate(byte[] data);

		void SyncGameDataFullInternal(byte[] data)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new ByteArrayDelegate(SyncGameDataFullInternal), new object[] {data});
				return;
			}
			MemoryStream stream = new MemoryStream(data);
			DeserializeGame(new BinaryReader(stream), false);
			stream.Close();
			ShowTrainRange();
			form.Invalidate();
		}

		delegate void ArrayListDelegate(ArrayList delta);

		void SyncGameDataInternal(ArrayList delta)
		{
			byte[] data = this.GetData();
			for (int i=0; i<delta.Count-1; i += 2)
			{
				int pos = (int) delta[i];
				byte[] chunk = (byte[]) delta[i+1];
				Array.Copy(chunk, 0, data, pos, chunk.Length);
			}
			byte[] hash1 = (byte[]) delta[delta.Count-1];
			byte[] hash2 = sha1.ComputeHash(data);
			for (int i=0; i<hash1.Length; i++)
				if (hash1[i] != hash2[i])
				{
					NetworkState.Transmit("NeedFullSync");
					return;
				}
			SyncGameDataFullInternal(data);
		}

		void Idle(object sender, EventArgs e)
		{
		}

		byte[] data1;
		bool lockSync;

		public void Sync()
		{
			if (!lockSync && !NetworkState.IsOffline)
			{
				byte[] data2 = this.GetData();
				if (data1 == null)
				{
					NetworkState.Transmit("SyncGameDataFull", data2);
					System.Diagnostics.Debug.WriteLine("Full sync");
				}
				else
				{
					if (data1.Length != data2.Length)
					{
						NetworkState.Transmit("SyncGameDataFull", data2);
						System.Diagnostics.Debug.WriteLine("Full sync");
					}
					else
					{
						ArrayList delta = new ArrayList();
						for (int i=0; i<data1.Length; i++)
							if (data1[i] != data2[i])
							{
								int j = i;
								while (j < data1.Length && data1[j] != data2[j])
									j++;
								if (j > data1.Length)
									j = data1.Length;
								byte[] chunk = new byte[j - i];
								Array.Copy(data2, i, chunk, 0, j - i);
								delta.Add(i);
								delta.Add(chunk);
								i = j;
							}
						delta.Add(sha1.ComputeHash(data2));
						NetworkState.Transmit("SyncGameData", delta);
						System.Diagnostics.Debug.WriteLine("Delta sync");
					}
				}
				data1 = data2;
			}
		}

		static SHA1 sha1 = SHA1.Create();

		public static void NeedFullSync()
		{
			if (Current == null)
				return;

			if (!Startup.LocalUsers.Contains(Current.state.ThisPlayer.Name))
				return;

			NetworkState.Transmit("SyncGameDataFull", Current.GetData());
		}






/* RANDOM EVENTS ------------------------------------------------------------------------------------- */

		// generate random events and cancel elapsed events
		void HandleDisasters()
		{
			// check if any events have elapsed
			int n = disasters.Count;
			while (disasters.Count > 0 && ((Disaster) disasters.Peek()).Player == state.CurrentPlayer)
			{
				// remove the event from the disaster queue
				Disaster d = (Disaster) disasters.Dequeue();

				// dispose of the event if necessary
				d.Dispose();
			}

			// if any events lapsed, redraw the map
			bool sync = false;
			if (disasters.Count < n)
			{
				form.Invalidate();
				sync = true;
			}

			// check if there's a new event
			state.DisasterProbability += 0.003801658305; // averages out to one disaster every 20 player turns

			if (rand.NextDouble() > state.DisasterProbability)
			{
				if (sync) 
					Sync();
				return;
			}
			state.DisasterProbability = 0.0;	// reset for next disaster

			// create the event
			Disaster disaster = Disaster.Create(state, map);

			// adjust the game state
			disaster.AffectState(state);

			// add the event to the disaster queue
			disasters.Enqueue(disaster);

			// check which players lose a turn and/or a load
			System.Text.StringBuilder messages = new System.Text.StringBuilder();
			for (int i=0; i<state.NumPlayers; i++)
			{
				if (disaster.PlayerLosesTurn(state.PlayerInfo[i]))
				{
					state.PlayerInfo[i].LoseTurn = true;
					if (state.PlayerInfo[i].Human)
						messages.Append(state.PlayerInfo[i].Name + " loses a turn. ");
				}
				if (disaster.PlayerLosesLoad(state.PlayerInfo[i]))
					if (state.PlayerInfo[i].LoseLoad(rand, state))
					{
						if (state.PlayerInfo[i].Human)
							messages.Append(state.PlayerInfo[i].Name + " loses a load of cargo. ");
					}
			}

			// get the headline and subhead
			string s = disaster.ToString();
			int cr = s.IndexOf('\r');
			string headline = s.Substring(0, cr);
			string subhead = s.Substring(cr + 1);

			// notify other players
			if (!NetworkState.IsOffline)
			{
				Sync();
				NetworkState.Transmit("Newspaper", headline, subhead);
			}

			// redraw the map
			form.Invalidate();

#if TEST
#else
			// display the newpaper if there are any human players
			bool humans = false;
			foreach (Player pl in state.PlayerInfo)
				if (pl.Human)
					humans = true;

			if (humans)
			{
				ShowNewspaperInternal(headline, subhead);
				if (messages.Length > 0)
					MessageBox.Show(messages.ToString(), form.Text);
			}
#endif
		}

		void ShowNewspaperInternal(string headline, string subhead)
		{
			using (Newspaper news = new Newspaper(headline, subhead))
			{
				suspendAI = true;
				news.ShowDialog(form);
				suspendAI = false;
			}
		}

		delegate void TwoStringDelegate(string a, string b);

		public static void ShowNewspaper(string headline, string subhead)
		{
			if (Current == null)
				return;

			if (Current.form == null)
				return;

			Current.form.Invoke(new TwoStringDelegate(Current.ShowNewspaperInternal), new object[] {headline, subhead});
		}

		// determine if there's a labor strike going on at a city
		bool CityIsClosed(int city)
		{
			foreach (Disaster disaster in disasters)
				if (disaster.IsLaborStrike(city))
					return true;

			return false;
		}







/* CONTRACT MANAGEMENT ------------------------------------------------------------------------------- */

		// delete a contract, display the deletion animation, and draw a new contract
		void DeleteContract(int i)
		{
#if TEST
#else
			bool blinkSave = this.blinkingTrain;
			deadContract = i;
			BlinkTrain(false);
			animationStart = DateTime.Now;
			animationLength = TimeSpan.FromSeconds(0.5);
			DateTime animationEnd = animationStart + animationLength;
			while (DateTime.Now < animationEnd)
			{
				form.Invalidate(statusRect);
				Application.DoEvents();
				System.Threading.Thread.Sleep(0);
			}
			deadContract = -1;
			BlinkTrain(blinkSave);
#endif
			state.ThisPlayer.DeleteContract(map, i, rand, options);
		}

		int AdjustPayment(int amt, int cityIndex)
		{
			double bonus = 0.0;

			if (state.CityIncentive[cityIndex])
				bonus += 0.5;

			if (options.FirstToCityBonuses && !state.CityWasVisited[cityIndex])
				bonus += 0.5;

			return amt + (int) (bonus * amt);
		}

		// move one of the incentive cities to a different location
		void FindNewIncentiveCity(int oldCity)
		{
			map.ResetFloodMap();

			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
				{
					bool track = state.Track[x,y,0]!=-1 || state.Track[x,y,1]!=-1 || state.Track[x,y,2]!=-1 || state.Track[x,y,3]!=-1 || state.Track[x,y,4]!=-1 || state.Track[x,y,5]!=-1;
					map.SetValue(x, y, track ? 0 : int.MaxValue);
				}

			for (int i=0; i<map.CityCount; i++)
				if (state.CityIncentive[i])
					map.SetValue(map.Cities[i].X, map.Cities[i].Y, 0);

			int maxX, maxY;
			map.Flood(buildTrackMethod, new IsValidSite(IsValidIncentiveCity), -1, out maxX, out maxY);
			int ci = map[maxX, maxY].CityIndex;
			if (ci != -1)
			{
				state.CityIncentive[oldCity] = false;
				state.CityIncentive[ci] = true;
				InvalidateCity(oldCity);
				InvalidateCity(ci);
			}
		}

		// determine if a milepost is a valid incentive city location
		bool IsValidIncentiveCity(int x, int y)
		{
			return map[x, y].CityIndex != -1;
		}

		// determine if unloading cargo results in a contract fulfillment
		bool FulfillContract(ref int product, int cityIndex)
		{
			for (int i=0; i<Player.NumContracts; i++)
			{
				Contract contract = state.ThisPlayer.Contracts[i];
				if (contract.Product == product && contract.Destination == cityIndex)
				{
					// get paid
					int amt = AdjustPayment(contract.Payoff, cityIndex);
					state.ThisPlayer.Receive(amt);
					UpdatePlaque();

					// update incentives
					if (state.CityIncentive[cityIndex])
						FindNewIncentiveCity(cityIndex);

					if (options.FirstToCityBonuses && !state.CityWasVisited[cityIndex])
					{
						state.CityWasVisited[cityIndex] = true;
						InvalidateCity(cityIndex);
					}

					// delete this contract and the bottom two contracts
					state.ReleaseCommodity(ref product);
					DeleteContract(i);
					if (options.GroupedContracts)
					{
						i += 2 - (i % 3);
						DeleteContract(i);
						DeleteContract(i);
					}
					else
					{
						DeleteContract(Player.NumContracts - 2);
						DeleteContract(Player.NumContracts - 1);
					}

					EndOfGameNotification();

					return true;
				}
			}
			return false;
		}

		// determine how many loads of a given product we're carrying
		int AmountCarried(int product)
		{
			Player pl = state.ThisPlayer;
			int n = 0;
			for (int i=0; i<pl.Cars; i++)
				if (pl.Cargo[i] == product)
					n++;
			return n;
		}

		// determine if we're carrying at least one load of a given product
		bool HasProduct(int product)
		{
			return AmountCarried(product) > 0;
		}

		// determine if we're carrying a product, and in which freight car
		bool HasProduct(int product, out int car)
		{
			Player pl = state.ThisPlayer;
			for (int i=0; i<pl.Cars; i++)
				if (pl.Cargo[i] == product)
				{
					car = i;
					return true;
				}
			car = -1;
			return false;
		}

		// check if the player is allowed to build on other player's track
		public bool CanUseOtherTrack()
		{
			return true;
		}

		// check if the player is allowed to discard all of their contracts
		public bool CanDiscardAll(bool allowComputer)
		{
			if (!allowComputer && !state.ThisPlayer.Human)
				return false;
			return state.AccumCost == 0 && state.AccumTime <= state.StartingAccumTime && !state.ThisPlayer.LoseTurn;
		}

		// discard all contracts and replace with new ones
		public void DiscardAll()
		{
			DiscardAll(false);
		}

		public void DiscardAll(bool allowComputer)
		{
			if (!CanDiscardAll(allowComputer))
				return;

			PushState(Resource.GetString("Game.ContractDump"));
			for (int i=0; i<Player.NumContracts; i++)
				DeleteContract(i);

			state.ThisPlayer.LoseTurn = true;
			state.ThisPlayer.DumpCount++;
			state.ThisPlayer.LastDumpTurn = state.Turn;
			form.Invalidate(statusRect);
			Sync();
		}







/* COMPUTER INTELLIGENCE ----------------------------------------------------------------------------- */

		// find the most efficient source to fulfill a given contract, and calculate the payoff rate
		// which is (payoff - cost) / time
		double PayoffPerHour(Contract contract, out int bestSource, out string description, out int cost, out int time)
		{
			bestSource = -1;
			double maxPayoff = double.MinValue;
			int bestCost = -1;
			int bestTime = -1;
			description = String.Empty;

			// get the list of possible sources
			ArrayList sources = (ArrayList) map.ProductSources[contract.Product].Clone();

			// if we're already carrying that product, add our train to the list
			if (HasProduct(contract.Product))
				sources.Add(-1);

			// save the track state and train position because we're not *really* going to build and move
			int[,,] saveTrack = (int[,,]) state.Track.Clone();
			int x = state.ThisPlayer.X;
			int y = state.ThisPlayer.Y;

			// suppress updates to the UI to avoid showing trial configurations
			suspendUI = true;

			// evaluate each source
			foreach (int source in map.ProductSources[contract.Product])
			{
				Application.DoEvents();

				// restore the track state and train position
				state.Track = (int[,,]) saveTrack.Clone();
				state.MovePlayerTo(x, y);

				// start calculating the cost and time
				cost = time = 0;
				string desc = String.Empty;

				// reward contracts that connect major cities
				int majorCityDiscount = 0;

				// if we're not carrying the product, calculate cost/time to go get it
				if (source != -1)
				{
					cost = BuildTrackToCity(source);
					if (cost == -1)
						continue;
					if (source < map.NumCapitals && cost > 0)
						majorCityDiscount += cost;
					desc += "Build to " + map.Cities[source].Name + " for " + cost + ". ";
					time += MoveToCity(source);
					time += loadingTime;
				}

				// calculate cost/time to deliver it
				int cost2 = BuildTrackToCity(contract.Destination);
				if (cost2 == -1)
					continue;
				if (contract.Destination < map.NumCapitals && cost2 > 0)
					majorCityDiscount += cost2;
				desc += "Build to " + map.Cities[contract.Destination].Name + " for " + cost2 + ". ";
				cost += cost2;
				time += MoveToCity(contract.Destination);
				time += loadingTime;

				// calculate the interest paid if we have to borrow
				if (cost > state.ThisPlayer.Funds)
					cost += cost - (state.ThisPlayer.Funds > 0 ? state.ThisPlayer.Funds : 0);
				else
					cost -= majorCityDiscount;

				// calculate profit
				int profit = AdjustPayment(contract.Payoff, contract.Destination) - cost;

				double payoff;
				if (profit > 0)
				{
					// time *= time;
					payoff = profit * 60.0 / time;
				}
				else
				{
					payoff = (double)profit * time; // money-losing routes penalized by time (minimize loss*time)
				}

				// special strategy for end-game
				if (state.ThisPlayer.IsInEndGame)
					if (state.ThisPlayer.Funds + profit >= options.FundsGoal)
						payoff = 1e6 / time;	// if funds are sufficient to win, only time matters

				// test if this is the best source so far
				if (payoff > maxPayoff)
				{
					maxPayoff = payoff;
					bestSource = source;
					description = desc;
					bestCost = cost;
					bestTime = time;
				}
			}

			// restore the track state and train position
			state.Track = (int[,,]) saveTrack.Clone();
			state.MovePlayerTo(x, y);
			suspendUI = false;

			// return the info
			cost = bestCost;
			time = bestTime;
			return maxPayoff;
		}

		delegate void StringDelegate(string s);

		// display the computer's action
		public void ShowComputerIntent(string s)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new StringDelegate(ShowComputerIntent), new object[] {s});
				return;
			}

			if (ind != null)
				HideComputerIntent();
			cilen = s.Length;
			ind = new Indicator(s, Color.White, TrackColor[state.ThisPlayer.TrackColor]);
			Application.DoEvents();
			if (NetworkState.IsHost)
				NetworkState.Transmit("ShowComputerIntent", s);
		}

		// remove the display of the computer's action, leaving it up long enough to read
		// based on the length of the string (for a fast reader)
		void HideComputerIntent()
		{
			HideComputerIntent(true);
		}

		delegate void BooleanDelegate(bool b);

		// remove the display of the computer's action, maybe or maybe not leaving it
		// up long enough to read
		public void HideComputerIntent(bool waitToRead)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new BooleanDelegate(HideComputerIntent), new object[] {waitToRead});
				return;
			}
				
#if TEST
#else
			if (waitToRead)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(250 + 30 * cilen);
			}
#endif
			if (ind != null)
			{
				ind.Close();
				ind.Dispose();
				ind = null;
			}
			Application.DoEvents();

			if (NetworkState.IsHost)
				NetworkState.Transmit("HideComputerIntent", waitToRead);
		}

		// determine the payoff rate for a given computer strategy under consideration
		double EvaluateStrategy(Strategy strategy)
		{
			// save the track state and train position because we're not *really* going to build and move
			int[,,] saveTrack = (int[,,]) state.Track.Clone();
			int x = state.ThisPlayer.X;
			int y = state.ThisPlayer.Y;
			suspendUI = true;

			// begin adding up the cost and time
			int cost = 0;
			int time = 0;
			int payoff = 0;
			int majorCityDiscount = 0;
			double pph = double.MinValue;

			// if this is a four-step strategy, switch 2nd and 3rd step if the first delivery location
			// is the same as the second pickup location
			if (strategy.Steps.Count == 4)
			{
				Step step1 = (Step) strategy.Steps[1];
				Step step2 = (Step) strategy.Steps[2];
				Contract c2 = state.ThisPlayer.Contracts[step2.Contract];
				if (step1.PickUp && !step2.PickUp && step1.Source == c2.Destination)
				{
					Step temp = step1;			// keep reference to step1 alive
					strategy.Steps[1] = step2;	// because this statement would orphan it
					strategy.Steps[2] = temp;	// and it could be GC'd before here
				}
			}

			// add up the cost and time for each step in the strategy
			foreach (Step step in strategy.Steps)
			{
				if (step.PickUp)
				{
					if (step.Source != -1)
					{
						int cost1 = BuildTrackToCity(step.Source, false);
						if (cost1 == -1)
							goto cleanup;
						cost += cost1;
						if (step.Source < map.NumCapitals)
							majorCityDiscount += cost1;
						time += MoveToCity(step.Source) + loadingTime;
					}
				}
				else
				{
					Contract c = state.ThisPlayer.Contracts[step.Contract];
					int cost2 = BuildTrackToCity(c.Destination, false);
					if (cost2 == -1)
						goto cleanup;
					cost += cost2;
					if (c.Destination < map.NumCapitals)
						majorCityDiscount += cost2;
					time += MoveToCity(c.Destination) + loadingTime;
					payoff += AdjustPayment(c.Payoff, c.Destination);
				}
			}

			// add interest if we need to borrow
			if (cost > state.ThisPlayer.Funds)
				cost += cost - (state.ThisPlayer.Funds > 0 ? state.ThisPlayer.Funds : 0);
			else 
				cost -= majorCityDiscount;

			int profit = payoff - cost;
			if (profit > 0)
			{
				// time *= time;
				pph = profit * 60.0 / time;
			}
			else
			{
				pph = (double)profit * time; // money-losing routes penalized by time (minimize loss*time)
			}

cleanup:
			// restore the track state and train position
			state.Track = (int[,,]) saveTrack.Clone();
			state.MovePlayerTo(x, y);
			suspendUI = false;
			return pph;
		}

		// Create each possible two-contract combination that includes a certain contract
		// and source already known to be the best single-contract strategy. Evaluate each
		// one and return the best combination.
		double ExamineCombos(int contract1, int source1, out Strategy best)
		{
			Contract c1 = state.ThisPlayer.Contracts[contract1];
			best = null;
			double maxPayoff = double.MinValue;

			// determine which contracts are invalid as the 2nd contract
			bool[] willExpire = new Boolean[Player.NumContracts];
			willExpire[contract1] = true; // can't deliver same contract twice
			if (options.GroupedContracts)
			{
				// can't deliver more than one in same group
				int j = 3 * (contract1 / 3);
				for (int i=j; i<j+3; i++)
					willExpire[i] = true;
			}
			else
			{
				// can't deliver more than one in bottom three
				if (contract1 >= Player.NumContracts - 3)
				{
					for (int i=1; i<=3; i++)
						willExpire[Player.NumContracts - i] = true;
				}
			}

			// check each second contract
			for (int contract2 = 0; contract2 < Player.NumContracts; contract2++)
				if (!willExpire[contract2])
				{
					Contract c2 = state.ThisPlayer.Contracts[contract2];

					// get the possible sources for the contract commodity
					ArrayList sources = (ArrayList) map.ProductSources[c2.Product].Clone();

					// if we're carrying enough of the commodity to fulfill the two contracts,
					// add the train as another source
					int n = AmountCarried(c2.Product);
					if (n > 0 && c2.Product != c1.Product) // carrying at least one, and only one contract needs it
						sources.Add(-1);
					else if (n >= 2 && c2.Product == c1.Product) // carrying at least two, and both contracts need it
						sources.Add(-1);

					// evaluate each source for the second contract
					foreach (int source2 in sources)
					{
						// generate all the different orders that the pickup and delivery steps can occur
						Strategy[] strategies = Strategy.CreateCombos(options, contract1, source1, contract2, source2);

						// test each strategy
						foreach (Strategy strategy in strategies)
						{
							Application.DoEvents();
							double payoffPerHour = EvaluateStrategy(strategy);

							// save the best one
							if (payoffPerHour > maxPayoff)
							{
								maxPayoff = payoffPerHour;
								best = strategy;
							}
						}
					}
				}

			// store the payoff rate in the strategy object
			if (best != null)
				best.PayoffRate = maxPayoff;

			return maxPayoff;
		}

#if TEST
		void LogPayoff(double p)
		{
			StreamWriter writer = new StreamWriter("payoff.log", true);
			writer.WriteLine("{0}\t{1}", state.Turn, p);
			writer.Close();
		}
#endif

		// calculate the total value of all currently-deliverable cargo on-board
		public int TotalCargoValue(Player pl)
		{
			int[] cv = pl.CargoValues;
			int total = 0;
			for (int i=0; i<cv.Length; i++)
				if (cv[i] > 0)
				{
					bool ok = false;
					for (int j=0; j<Player.NumContracts; j++)
					{
						City city = map.Cities[pl.Contracts[j].Destination];
						if (CouldMoveTo(city.X, city.Y, pl))
						{
							ok = true;
							break;
						}
					}
					if (ok)
						total += cv[i];
				}
			return total;
		}

		// fill any empty cars with goods before leaving town
		void TopOffCars()
		{
			Player pl = state.ThisPlayer;

			// can't fill cars if not on map
			if (pl.X == -1)
				return;

			// do we have any empty cars?
			if (pl.EmptyCar == -1)
				return;

			// can't load anything if we're not at a city
			if (map[pl.X, pl.Y].CityType == CityType.None)
				return;

			// haven't formulated a strategy so nothing at risk and 
			// we will be needing unknown amount of room soon
			if (pl.Strategy == null)
				return;

			/*
			// see if we're carrying anything valuable
			bool atRisk = false;
			for (int c=0; c<pl.Cars; c++)
				foreach (Step step in pl.Strategy.Steps)
					if (pl.Contracts[step.Contract].Product == pl.Cargo[c])
						atRisk = true;
			*/

			// see if we'll have extra room
			int load = 0, maxLoad = 0;
			for (int c=0; c<pl.Cars; c++)
				if (pl.Cargo[c] != -1)
					load++;
			foreach (Step step in pl.Strategy.Steps)
				if (step.PickUp)
				{
					load++;
					if (load > maxLoad)
						maxLoad = load;
				}
				else
					load--;

			// are we going to be full?
			int excessCapacity = pl.Cars - maxLoad;
			if (excessCapacity <= 0)
				return;

			City city = map.Cities[map[pl.X, pl.Y].CityIndex];

			while (excessCapacity-- > 0 && pl.EmptyCar != -1 && !state.NoMoreTime)
			{
				int stuff = -1;
				foreach (int product in city.Products)
					if (state.Availability[product] > 0)
						if (stuff == -1 || map.AveragePayoff[product] > map.AveragePayoff[stuff])
							stuff = product;

				if (stuff == -1)
					break;

				ShowComputerIntent("Loading extra " + Products.Name[stuff]);
				pl.Cargo[pl.EmptyCar] = state.ReserveCommodity(stuff);
				SpendTime(loadingTime);
				form.Invalidate(statusRect);
				HideComputerIntent();
			}
		}

		// pause the computer players while the new game form is displayed
		public void Pause(bool pause)
		{
			suspendAI = pause;
		}

		// check if it's time to upgrade the train
		void CheckForUpgrade(int deliveryAmount)
		{
			Player pl = state.ThisPlayer;

			if (pl.EngineType == Engine.MaxEngineType)
				return;	// upgraded all the way already

			if (state.NoMoreTime)
				return;	// no time to upgrade now

			if (pl.Funds < Engine.UpgradeCost)
				return;	// can't afford to upgrade at the moment

			if (state.AccumCost + Engine.UpgradeCost > GameState.MaxSpend)
				return;	// not enough spending limit left this turn

			if (pl.Funds + deliveryAmount - Engine.UpgradeCost < 25)
				return;	// not enough money buffer left after delivering commodity

			pl.EngineType++;										
			ShowComputerIntent("Upgrading train to " + Engine.Description[pl.EngineType]);
			SpendMoney(Engine.UpgradeCost);
			SpendTime(Engine.UpgradeTime);
			form.Invalidate(statusRect);
			HideComputerIntent();
		}

		// it's the computers turn, so when the form is otherwise idle, do computer stuff
		public void ComputerMove()
		{
			Contract contract;
			City city;
			int car;
			Player pl;

			if (suspendAI)	// don't do anything if we're showing the newspaper
				return;

			// don't do anything if we're not a computer
			pl = state.ThisPlayer;
			if (pl.Human)
			{
				if (canReachInTime == null)
					ShowTrainRange();
				return;
			}

			// don't do anything else until the train stops moving
			if (moveStack != null)
			{
				if (!moveTimer.Enabled)
				{
					MoveAnimation(false); // weird bug, shouldn't need to do this
				}
				return;
			}

			// don't do anything if we have lost our turn due to a disaster
			if (pl.LoseTurn)
			{
				ShowComputerIntent(state.ThisPlayer.Name + " loses a turn");
				HideComputerIntent();
				goto DoneForNow;
			}

			// check if we need to build to major cities
			if (state.ThisPlayer.X != -1)
			{
				if (PlayerMeetsRequirements(state.CurrentPlayer))
					goto DoneForNow;

				if (pl.Funds >= options.FundsGoal)			// we have enough money to win
					if (state.AccumCost < GameState.MaxSpend)	// and can spend more this turn
						if (BuildToCheapestCapital())			// but we need to build to another city
							return;
			}

			// get our current strategy
			Strategy strategy = pl.Strategy;

			// if we don't have a current strategy, formulate one
			lockSync = true;
			if (strategy == null || strategy.Steps.Count == 0)
			{
				ShowComputerIntent(state.ThisPlayer.Name + " is thinking");

				// find the best single-contract run
				double maxPayoff = double.MinValue;
				int bestSource = -1;
				int bestContract = -1;

				// assume disasters won't still be around
				Queue save = disasters;
				disasters = new Queue();

				// evaluate each contract
				for (int i=0; i<Player.NumContracts; i++)
				{
					Application.DoEvents();
					contract = pl.Contracts[i];
					int source;
					string description;
					int cost, time;
					double payoffPerHour = PayoffPerHour(contract, out source, out description, out cost, out time);
					if (payoffPerHour > maxPayoff)
					{
						maxPayoff = payoffPerHour;
						bestSource = source;
						bestContract = i;
					}
				}

				// restore disasters
				disasters = save;

				// create a single pickup, single delivery strategy
				if (bestContract != -1)
				{
					strategy = new Strategy();
					strategy.AddStep(bestContract, bestSource);
					strategy.AddStep(bestContract);
					strategy.PayoffRate = maxPayoff;
					strategy.Reconsider = true;
				}

				// store this as our current strategy
				pl.Strategy = strategy;
				HideComputerIntent(false);

				// there are no deliveries possible with the current contracts
				// so dump all of them
				if (strategy == null || strategy.Steps.Count == 0 || (strategy.PayoffRate < 0 && state.Turn > 2 + pl.LastDumpTurn))
				{
					pl.Strategy = null;
					if (!CanDiscardAll(true))
					{
						ShowComputerIntent("No feasible deliveries found. Waiting until next turn to discard contracts.");
						HideComputerIntent();
						goto DoneForNow;
					}
					ShowComputerIntent("Throwing away all contracts.");
					DiscardAll(true);
					HideComputerIntent(false);
					goto DoneForNow;
				}

//				return;
			}
			lockSync = false;

			// get the first step in our strategy
			Step step = (Step) strategy.Steps[0];
			
			// if we lost our contract, abandon this strategy
			if (step.Contract >= Player.NumContracts)
				goto AbandonStrategy;

			// if this is a single-contract strategy and we haven't evaluated the two-contract
			// strategies, do so now
			if (strategy.Reconsider)	// haven't looked yet
				if (!pl.IsInEndGame)	// don't worry about doing two loads at once if racing for final money
					if ((strategy.Steps.Count == 2 && step.PickUp && !((Step) strategy.Steps[1]).PickUp) // single-contract and haven't picked up yet
						|| (strategy.Steps.Count == 1 && !step.PickUp && map[pl.X, pl.Y].CityIndex != pl.Contracts[step.Contract].Destination))	// single-contract strategy and not at destination
					{
						// evaluate the two-contract strategies
						lockSync = true;
						Strategy best;
						ShowComputerIntent(state.ThisPlayer.Name + " is thinking");
						Queue save = disasters;
						disasters = new Queue();
						int src = step.PickUp ? step.Source : -1;
						double cp = ExamineCombos(step.Contract, src, out best);
						disasters = save;
						HideComputerIntent(false);
						lockSync = false;

						/* Can't do this part because it's map-dependent now that we support AuthoredMap
						* 
						// make sure it's worth it, otherwise dump all the contracts
						double minAcceptible = 0.003883 * (state.Turn - 5 * state.ThisPlayer.DumpCount) - 0.10145;
						if (cp < minAcceptible && strategy.PayoffRate < minAcceptible)
						{
							if (!CanDiscardAll(true))
							{
								ShowComputerIntent("No run found with acceptible return. Waiting until next turn to dump contracts.");
								HideComputerIntent();
								goto DoneForNow;
							}
							ShowComputerIntent("Throwing away all contracts.");
							DiscardAll(true);
							HideComputerIntent(false);
							goto DoneForNow;
						}
						*
						*/

						if (cp > strategy.PayoffRate)
						{
							pl.Strategy = strategy = best;	// we found a better one
							strategy.Reconsider = false;
	#if TEST
							LogPayoff(cp);
	#endif
						}
						else
						{
	#if TEST
							LogPayoff(strategy.PayoffRate);
	#endif
							strategy.Reconsider = false;	// don't recalculate this one again
						}
					}

			// get the first step again in case we just changed our strategy
			step = (Step) strategy.Steps[0];
			if (step.Contract >= Player.NumContracts)
				goto AbandonStrategy;

			contract = pl.Contracts[step.Contract];

			if (step.PickUp)	// we need to pick up a commodity
			{
				// we already have it
				if (step.Source == -1)
					goto NextStep;

				// determine how many of them we'll need
				int amt = 0;
				foreach (Step st in strategy.Steps)
					if (!st.PickUp)
						if (pl.Contracts[st.Contract].Product == contract.Product)
							amt++;

				// if we already have enough, go to the next step
				if (AmountCarried(contract.Product) >= amt)
					goto NextStep;

				city = map.Cities[step.Source];

				// if our train isn't on the map yet, just put it at our first pickup point
				if (pl.X == -1)
				{
					state.MovePlayerTo(city.X, city.Y);
					ShowComputerIntent("Placing train at " + city.Name);
					HideComputerIntent();
					return;
				}
	
				// if we're already at the source location, load the cargo
				if (map[pl.X, pl.Y].CityIndex == step.Source)
				{
					if (pl.EmptyCar == -1)
					{
						// we don't have any empty freight car, so we need to unload something

						// do we have time?
						if (state.NoMoreTime)
							goto DoneForNow;

						// don't unload anything we may want later
						car = -1;
						for (int c=0; c<pl.Cars; c++)
						{
							bool needed = false;
							foreach (Step s in strategy.Steps)
							{
								Contract ct = pl.Contracts[s.Contract];
								if (pl.Cargo[c] == ct.Product)
								{
									needed = true;
									break;
								}
							}
							if (!needed)
							{
								// unload least valuable (or only) unneeded cargo
								if (car == -1 || map.AveragePayoff[pl.Cargo[c]] < map.AveragePayoff[pl.Cargo[car]])
									car = c;
							}
						}

						// if everything we're carrying is something we may want later, just drop the last car
						if (car == -1)
							car = pl.Cars - 1;

						// unload it
						ShowComputerIntent("Unloading " + Products.Name[pl.Cargo[car]] + " to make room");
						state.ReleaseCommodity(ref pl.Cargo[car]);
						SpendTime(loadingTime);
						form.Invalidate(statusRect);
						HideComputerIntent();
					}

					// do we have time to load the cargo?
					if (state.NoMoreTime)
						goto DoneForNow;

					// is the cargo available?
					if (state.Availability[contract.Product] == 0)
						goto DoneForNow;

					// is there a labor strike?
					if (CityIsClosed(step.Source))
					{
						ShowComputerIntent("Waiting for current emergency in " + city.Name + " to end in order to pick up " + Products.Name[contract.Product]);
						HideComputerIntent();
						goto DoneForNow;
					}

					// all is well--load it
					ShowComputerIntent("Loading " + Products.Name[contract.Product] + " for delivery to " + map.Cities[contract.Destination].Name);
					pl.Cargo[pl.EmptyCar] = state.ReserveCommodity(contract.Product);
					SpendTime(loadingTime);

//					// load extra if no more pick-ups scheduled
//					int pickups = 0; 
//					foreach (Step substep in strategy.Steps)
//						if (substep.PickUp)
//							pickups++;
//					if (pickups < 2) // just did one of them
//					{
//						// fill up cars with extra loads to protect against disasters
//						// and to improve odds of serendipitous contracts
//						while (true)
//						{ 
//							if (pl.EmptyCar == -1)
//								break;
//							if (state.NoMoreTime)
//								break;
//							if (state.Availability[contract.Product] == 0)
//								break;
//							ShowComputerIntent("Loading extra " + Products.Name[contract.Product]);
//							pl.Cargo[pl.EmptyCar] = state.ReserveCommodity(contract.Product);
//						}
//					}

					HideComputerIntent();
					form.Invalidate(statusRect);
					goto NextStep;
				}

				// we're not at the source city yet, so check if we need to build to it
				if (!TracksGoTo(city.X, city.Y))
				{
					ShowComputerIntent("Building track toward " + city.Name + " to pick up " + Products.Name[contract.Product]);
					int cost = BuildTrackToCity(step.Source, true);
					HideComputerIntent(false);
//					if (cost == 0) // at spending limit
//						goto DoneForNow;

					if (cost == -1)
					{
						// we can't build there
						if (disasters.Count > 0)
							goto DoneForNow;		// perhaps due to disasters
						else
							goto AbandonStrategy;	// or may be a track monopoly
					}
					if (cost > 0)
					{
						SpendMoney(cost);
						form.Invalidate();
					}
				}

				// move toward the source city whether or not our track goes all the way there yet
				if (!MoveTowardCity(step.Source))
					goto DoneForNow;
				ShowComputerIntent("Moving toward " + city.Name + " to pick up " + Products.Name[contract.Product]);
				HideComputerIntent();
				return;
			}
			
			// this is a delivery step
			city = map.Cities[contract.Destination];

			// are we carrying the product we need?
			if (HasProduct(contract.Product, out car))
			{
				// are we already at the destination?
				if (map[pl.X, pl.Y].CityIndex == contract.Destination)
				{
					// do we have time to unload cargo?
					if (state.NoMoreTime)
						goto DoneForNow;

					// is there a labor strike here?
					if (CityIsClosed(contract.Destination))
					{
						ShowComputerIntent("Waiting for current emergency in " + city.Name + " to end in order to deliver " + Products.Name[contract.Product]);
						HideComputerIntent();
						goto DoneForNow;
					}

					// all is well--deliver it
					int payment = AdjustPayment(contract.Payoff, contract.Destination);
					ShowComputerIntent("Delivering " + Products.Name[contract.Product] + " for " + Utility.CurrencyString(payment));
					FulfillContract(ref pl.Cargo[car], contract.Destination);
					SpendTime(loadingTime);
					form.Invalidate(statusRect);
					HideComputerIntent();
					goto AdjustContracts;
				}

				// check if we need to build track to get to the destination
				bool checkForUpgrade = true;
				if (!TracksGoTo(city.X, city.Y))
				{
					checkForUpgrade = false;
					ShowComputerIntent("Building track toward " + city.Name + " to deliver " + Products.Name[contract.Product]);
					int cost = BuildTrackToCity(contract.Destination, true);
//					int cost = BuildBestTrackToDestination(contract);
					HideComputerIntent(false);
//					if (cost == 0) // at spending limit
//						goto DoneForNow;

					if (cost == -1)
					{
						// we can't build there
						if (disasters.Count > 0)
							goto DoneForNow;		// perhaps due to disasters
						else
							goto AbandonStrategy;	// or may be a track monopoly
					}
					if (cost > 0)
					{
						SpendMoney(cost);
						form.Invalidate();
					}
				}

				if (checkForUpgrade || TracksGoTo(city.X, city.Y))
				{
					// we are about to move toward connected destination so it would be a good
					// time to upgrade the train
					CheckForUpgrade(AdjustPayment(contract.Payoff, contract.Destination));
				}

				// move toward the destination city whether or not our tracks go all the way there yet
				if (!MoveTowardCity(contract.Destination))
					goto DoneForNow;
				ShowComputerIntent("Moving toward " + city.Name + " to deliver " + Products.Name[contract.Product]);
				HideComputerIntent();
				return;
			}

AbandonStrategy:
			// we lost our cargo or our contract or we can't build to required city
			pl.Strategy = null;
			form.Invalidate(statusRect);
			lockSync = false;
			return;
			
AdjustContracts:
			// contract was fulfilled causing other contracts to slide down
			int sc = step.Contract;
			if (!options.GroupedContracts)
			{
				foreach (Step s in strategy.Steps)
					if (s.Contract > sc)
						s.Contract += 2;
					else
						s.Contract += 3;
			}

NextStep:
			bool madeDelivery = !((Step) strategy.Steps[0]).PickUp;
			strategy.Steps.RemoveAt(0);		// remove the current step from the list
			if (madeDelivery && !strategy.Reconsider)
			{
				strategy.Reconsider = true;	// allow reconsideration of multi-step strategies
				strategy.PayoffRate = 0.0;	// don't influence new decision based on past payoff
			}
			form.Invalidate(statusRect);
			lockSync = false;
			return;

DoneForNow:
			lockSync = false;
			NextPlayer();
			return;
		}

		/*
		Stack FindBestTrack(Contract c)
		{
			Player pl = state.ThisPlayer;
			Pathfinder f = new Pathfinder(map, new PayoffTerms(int.MinValue, int.MaxValue, 0));
			f.SetValue(pl.X, pl.Y, new PayoffTerms(c.Payoff, 0, 0));
			f.Find(new PathfinderMethod(FindBestTrackMethod), null);
			City city = map.Cities[c.Destination];
			return f.GetGradientStack(city.X, city.Y);
		}

		bool FindBestTrackMethod(Pathfinder f, int x, int y, int d, int i, int j, out IComparable val)
		{
			PayoffTerms t = (PayoffTerms) f.GetValue(x, y);
			val = null;
			
			bool fromSea = map.IsSea(x, y);
			bool fromPort = map.IsPort(x, y);
			bool toSea = map.IsSea(i, j);
			bool toPort = map.IsPort(i, j);
			
			int landSpeed = Engine.SpeedInMinutes[state.ThisPlayer.EngineType];

			// don't need to build track on sea--can move there at will
			if (fromSea && (toSea || toPort))
			{
				val = new PayoffTerms(t.Payoff, t.Cost, Game.seaMovementCost);
				return true;
			}

			// can't move from sea to land, except at a port
			if (fromSea)
				return false;

			// don't need to build track to embark from a port 
			if (toSea && fromPort)
			{
				val = new PayoffTerms(t.Payoff, t.Cost, t.Time + Game.seaMovementCost);
				return true;
			}

			// can't move to sea from land, except at a port
			if (toSea)
				return false;

			// it's free if we already have track at this segment
			if (state.Track[x, y, d] == state.CurrentPlayer)
			{
				val = new PayoffTerms(t.Payoff, t.Cost, t.Time + landSpeed);
				return true;
			}

			// it's free if there is nationalized track at this segment
			if (state.Track[x, y, d] == state.NumPlayers)
			{
				val = new PayoffTerms(t.Payoff, t.Cost, t.Time + landSpeed);
				return true;
			}

			// prohibited if somebody else's track is here
			if (state.Track[x, y, d] != -1)
				return false;

			// free to move within major city hexagon
			if (map.IsCapital(x, y) && map.IsCapital(i, j) && map[x, y].CityIndex == map[i, j].CityIndex)
			{
				val = new PayoffTerms(t.Payoff, t.Cost, t.Time + landSpeed);
				return true;
			}

			// otherwise the cost is terrain-specific and disaster-specific
			int cost = CostOfBuildingTrack(x, y, i, j);
			if (cost == -1)
				return false;

			val = new PayoffTerms(t.Payoff, t.Cost + cost, t.Time + landSpeed);
			return true;
		}
		*/
	}

	public class StaticColorArray
	{
		Color[] array;

		public StaticColorArray(Color[] array)
		{
			this.array = array;
		}

		public Color this[int index]
		{
			get { return array[index]; }
		}

		public int Length
		{
			get { return array.Length; }
		}
	}

	/*
	public class PayoffTerms : IComparable
	{
		public int Payoff;
		public int Cost;
		public int Time;

		public double Rate
		{
			get
			{
				if (Payoff >= Cost)
					return (1.0 * Payoff - Cost) / Time;

				return (1.0 * Payoff - Cost) * (Time + 1);
			}
		}

		public PayoffTerms(int payoff, int cost, int time)
		{
			Payoff = payoff;
			Cost = cost;
			Time = time;
		}

		public int CompareTo(object obj)
		{
			PayoffTerms t = obj as PayoffTerms;
			return t.Rate.CompareTo(this.Rate);
		}
	}
	*/
}