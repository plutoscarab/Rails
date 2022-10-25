﻿
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

namespace Rails
{
	public class Game : IDisposable
	{

/* CONSTANTS ---------------------------------------------------------------------------------------- */

		const int seaMovementCost = 360;	// takes six hours to move between sea mileposts
		const int loadingTime = 120;		// takes two hours to load/unload cargo
		const int upgradeCost = 20;			// costs 20 to upgrade train
		const int upgradeTime = 180;		// takes three hours to upgrade train

		const int useTrackCost = 4;			// costs 4 to use another player's track

		// player colors and color names
		public static readonly StaticColorArray TrackColor = new StaticColorArray(new Color[] {Color.Green, Color.Crimson, Color.DodgerBlue, Color.MediumOrchid, Color.FromArgb(255, 177, 0), Color.DarkGray});
		public static readonly StaticStringArray ColorName = new StaticStringArray(new string[] {"Green", "Red", "Blue", "Purple", "Gold", "Silver"});
		
/* PUBLIC FIELDS ------------------------------------------------------------------------------------ */

		public Map map;						// the map
		public GameState state;				// the game state (player info, tracks)

/* INTERNAL DATA ------------------------------------------------------------------------------------ */

		// USER INTERFACE AND DRAWING

		Form form;							// the main form
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

		// restore the game from the game save file
		public Game(BinaryReader reader, Form form)
		{
			this.form = form;

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
				map = new RandomMap(new Size(width, height), seed);
			}
			else
			{
				string name = reader.ReadString();
				/* int width = */ reader.ReadInt32();
				/* int height = */ reader.ReadInt32();
				map = new AuthoredMap(name);
			}
			statusRect.X = reader.ReadInt32();
			statusRect.Y = reader.ReadInt32();
			statusRect.Width = reader.ReadInt32();
			statusRect.Height = reader.ReadInt32();
			state = new GameState(reader, map);
			Contract.ReadRandom(reader);
			options = new Options(reader);

			trainMovementMethod = new FloodMethod(TrainMovementMethod);
			buildTrackMethod = new FloodMethod(BuildTrackMethod);
			rand = new Random();
			UndoStack = new Stack();
		}

		// write the game to the game save file
		public void Save(BinaryWriter writer)
		{
			writer.Write((int) 1); // version
			if (map is RandomMap)
			{
				writer.Write((int) 0);
				writer.Write(((RandomMap) map).Number);
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

			trainMovementMethod = new FloodMethod(TrainMovementMethod);
			buildTrackMethod = new FloodMethod(BuildTrackMethod);
			rand = new Random();
			UndoStack = new Stack();
		}

		// start a new game with a specified map and list of players
		public Game(Map map, Rectangle statusRect, Player[] playerList, Form form, Options options)
		{
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
			GC.SuppressFinalize(this);
			if (moveTimer != null)
				moveTimer.Dispose();
			if (ind != null)
				ind.Dispose();
			if (blinkTimer != null)
				blinkTimer.Dispose();
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
			form.Invalidate(r);
		}

		// hide or display the commodities available at the city where the mouse is
		void DrawMouseCity(Graphics g)
		{
			if (mouseCity == -1 || !mouseCityShow || building)
				return;
			City city = map.Cities[mouseCity];
			int x, y;
			if (!map.GetCoord(city.X, city.Y, out x, out y))
				return;
			int[] products = (int[]) city.Products.ToArray(typeof(int));
			g.DrawImageUnscaled(Products.Icon[products[0]], x + 10, y - 40, 48, 48);
			if (products.Length > 1)
				g.DrawImageUnscaled(Products.Icon[products[1]], x - 24, y - 58, 48, 48);
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
										int tc = state.PlayerInfo[pl].TrackColor;

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
			Brush brush = new SolidBrush(TrackColor[colorIndex]);
			g.FillRectangle(brush, 0, 0, statusRect.Width, 64);
			brush.Dispose();
			if (inNextPlayerRect)
			{
				brush = new SolidBrush(Color.FromArgb(64, Color.Black));
				g.FillRectangle(brush, NextPlayerRect);
				brush.Dispose();
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

			// calculte dead-contract animation variables
			double clock;
			int voffset = 0;
			int fade = 0;
			if (deadContract != -1)
			{
				clock = 1.0 * (DateTime.Now - animationStart).TotalMilliseconds / animationLength.TotalMilliseconds;
				voffset = (int) (clock * dy);
				fade = (int) (255 * clock);
				if (fade > 255) fade = 255;
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
				if (deadContract != -1 && i < deadContract)
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

			// clean up
			af.Dispose();
			font.Dispose();
			bold.Dispose();
			g.Dispose();
			gr.DrawImageUnscaled(bmp, statusRect.X, statusRect.Y);
			bmp.Dispose();
		}

		// if the mouse is hovering over a contract, indicate the sources and the destinations
		void DrawProductIcons(Graphics g)
		{
			if (mouseContract == -1)
				return; // we're not doing that

			Contract contract = state.ThisPlayer.Contracts[mouseContract];
			int product = contract.Product;
			Image icon = Products.Icon[product];
			int x, y;

			// display each source
			foreach (int c in map.ProductSources[product])
				if (map.GetCoord(map.Cities[c].X, map.Cities[c].Y, out x, out y))
				{
					// draw to the left or right depending on which half of the map the city is on
					if (x < map.ImageSize.Width / 2)
						g.DrawImageUnscaled(icon, x + 10, y - 40, 48, 48);
					else
						g.DrawImageUnscaled(icon, x - 58, y - 40, 48, 48);
				}

			// highlight the destination city with a blue glow
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
				((Form1) form).InitRefreshTimer();
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
				// rewind each new track segment
				PopState();
//				while (buildStack.Count > 0)
//				{
//					TrackRecord rec = buildStack.Pop() as TrackRecord;
//					state.Track[rec.i, rec.j, rec.d] = -1;
//					int i2, j2;
//					map.GetAdjacent(rec.i, rec.j, rec.d, out i2, out j2);
//					state.Track[i2, j2, (rec.d + 3) % 6] = -1;
//				}
				building = false;
				buildStack = null;
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

		// figure out what to do if they click the a mouse button
		public void MouseDown(MouseEventArgs e)
		{
			// don't do anything if it's the computer's turn
			if (!state.ThisPlayer.Human)
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
				if (state.AccumTime + loadingTime > GameState.MaxTime)
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
				if (state.AccumTime + loadingTime > GameState.MaxTime)
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
					string message = string.Format(System.Globalization.CultureInfo.CurrentUICulture, Resource.GetString("Game.NoMoneyToUpgrade"), Utility.CurrencyString(upgradeCost));
					MessageBox.Show(message, form.Text);
					return;
				}

				// make sure we haven't reached our time limit for the turn
				if (state.AccumTime + upgradeTime > GameState.MaxTime)
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
				state.ThisPlayer.Spend(upgradeCost);
				state.AccumCost += upgradeCost;
				SpendTime(upgradeTime);
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
			if (!state.ThisPlayer.Human)
				return;

			if (state.ThisPlayer.LoseTurn)
				return;

			// finished building track?
			if (building)
			{
				// spent the time and the money
				state.ThisPlayer.Spend(buildCost);
				state.AccumCost += buildCost;
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
			if (state.AccumTime + time > GameState.MaxTime)
			{
				StopMoveAnimation();
				if (firstMove)
					MessageBox.Show(Resource.GetString("Game.NoTimeToMove"), form.Text);
				return moved;
			}

			// keep track of time spent
			SpendTime(time);
			moved = true;

			// redraw the train indicator
			int xx, yy;
			map.GetCoord(pl.X, pl.Y, out xx, out yy);
			form.Invalidate(new Rectangle(xx - 10, yy - 10, 20, 20));
			pl.X = x;
			pl.Y = y;
			pl.D = d;
			map.GetCoord(pl.X, pl.Y, out xx, out yy);
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
			// get the screen coordinates of the city
			City c = map.Cities[city];
			int cx, cy;
			if (!map.GetCoord(c.X, c.Y, out cx, out cy))
				return false;

			// get the screen coordinates of the train
			int tx, ty;
			if (!map.GetCoord(state.ThisPlayer.X, state.ThisPlayer.Y, out tx, out ty))
				return false;

			// determine all the map locations the train can reach
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			// find the location closest to the destination
			int min = int.MaxValue;
			int min2 = int.MaxValue;
			int bx, by;
			bx = by = -1;
			int p = state.CurrentPlayer;
			int px, py;
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value != int.MaxValue)
					{
						if (!map.GetCoord(x, y, out px, out py))
							continue;

						// if moving to a major city, only allow movement to the corners that have our track
						if (map[x, y].CityIndex == city)
							if (!(map.IsPort(x,y) || state.Track[x,y,0]==p || state.Track[x,y,1]==p || state.Track[x,y,2]==p || state.Track[x,y,3]==p || state.Track[x,y,4]==p || state.Track[x,y,5]==p))
								continue; 

						// calculate the physical distance and check if it's the closest point so far
						int dist = (cx-px)*(cx-px)+(cy-py)*(cy-py);
						int dist2 = (tx-px)*(tx-px)+(ty-py)*(ty-py);
						if (dist < min)
						{
							min = dist;
							min2 = int.MaxValue;
							bx = x; by = y;
						}
						else if (dist == min && dist2 < min2)
						{
							min2 = dist2;
							bx = x; by = y;
						}
					}

			// if we're already the closest we can get, don't do anything
			if (state.ThisPlayer.X == bx && state.ThisPlayer.Y == by)
				return false;

			// start the movement animation
			return BeginMove(bx, by, false);
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

			int n = 0;
			for (int x=0; x<map.GridSize.Width; x++)
				for (int y=0; y<map.GridSize.Height; y++)
					n += (canReachInTime[x, y] = (map[x, y].Value <= GameState.MaxTime - state.AccumTime)) ? 1 : 0;
        
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
					state.ThisPlayer.Spend(cost);
					state.AccumCost += cost;
					for (int i=0; i<state.NumPlayers; i++)
						if (chooseTrack.UseTrack[i] && !state.UseTrack[i])
							state.PlayerInfo[i].Receive(Game.useTrackCost);
					Array.Copy(chooseTrack.UseTrack, state.UseTrack, state.UseTrack.Length);
					ShowTrainRange();
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
			state.ThisPlayer.Spend(cost);
			state.AccumCost += cost;
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
					if (t != -1)
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
				case TerrainType.Sea:
					cost = 0;
					break; // shouldn't get here
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
					cost = 4;
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

			// cost of crossing water
			int i3, j3, dir = -1;
			for (int d=0; d<6; d++)
				if (map.GetAdjacent(i1, j1, d, out i3, out j3))
					if (i2 == i3 && j2 == j3)
					{
						dir = d;
						int wm = map[i1, j1].WaterMask;
						if ((wm & WaterMasks.InletMask[d]) != 0)
							cost += 3;
						else if ((wm & WaterMasks.RiverMask[d]) != 0)
							cost += 2;
						break;
					}

			// adjust cost based on random events
			if (dir != -1)
				foreach (Disaster disaster in disasters)
					if (!disaster.AdjustBuildingCost(i1, j1, dir, i2, j2, ref cost))
						return -1;

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
			state.Track[i1, j1, dd] = state.CurrentPlayer;
			state.Track[i2, j2, d2] = state.CurrentPlayer;
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

		// draw one new track segment and pause for a moment
		void NewTrackAnimation(int x, int y, int i, int j)
		{
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

			// set track as valid starting points for the build
			int x, y;
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value != int.MaxValue)
						map.SetValue(x, y, 0);

			// find the cheapest way to build to each map location
			map.Flood(buildTrackMethod);

			// can we get there with track?
			if (map[cx, cy].Value == int.MaxValue)
				return -1; // no

			// lay the track
			Stack stack = new Stack();
			GetMoveStack(cx, cy, stack, out x, out y);
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
							state.Track[x, y, d] = state.CurrentPlayer;
							state.Track[i, j, (d+3) % 6] = state.CurrentPlayer;
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

		// Build toward a new major city in order to meet the end-of-game requirements.
		// Be sure to spend as little as possible.
		bool BuildToCheapestCapital()
		{
			// determine map locations where our track already reaches
			map.ResetFloodMap();
			map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

			// determine cost of building to all other map locations
			int x, y;
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value != int.MaxValue)
						map.SetValue(x, y, 0);
			map.Flood(buildTrackMethod);

			// determine which major city corner would be cheapest to build to
			int min = int.MaxValue;
			int bx = -1, by = -1;
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
				return false;

			ShowComputerIntent("Building track toward " + map.Cities[map[bx, by].CityIndex].Name + " for major city quota");
			int cost = BuildTrackToCity(map[bx, by].CityIndex, true);
			if (cost == 0)
			{
				HideComputerIntent();
				return false;
			}

			// spend the time and the money and update the display
			state.ThisPlayer.Spend(cost);
			state.AccumCost += cost;
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

			int x, y;
			for (x=0; x<map.GridSize.Width; x++)
				for (y=0; y<map.GridSize.Height; y++)
					if (map[x, y].Value != int.MaxValue)
						map.SetValue(x, y, 0);

			map.Flood(buildTrackMethod);

			return (map[cx, cy].Value != int.MaxValue);
		}

		// How much would it cost for a given player to build to all the major cities
		// they would need to win? This is used by the player rankings display.
		public int CityQuota(int player)
		{
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
				for (int x=0; x<map.GridSize.Width; x++)
					for (int y=0; y<map.GridSize.Height; y++)
						if (map[x, y].Value != int.MaxValue)
							map.SetValue(x, y, 0);
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
			return quota;
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

			// advance the game state
			state.NextPlayer();

			// redraw the status area
			DonePlacingLocomotive();

			// save the game state to the game save file
			Save();

			// check if there's a new random event, or if a random event has elapsed
			HandleDisasters();

			// start over with a new Undo stack
			UndoStack = new Stack();

			// show the new player's train range
			canReachInTime = null;
			ShowTrainRange();

			// check if the game is over
			CheckEndOfGameCondition();
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

		// check if a specific player meets the end-of-game requirements
		bool PlayerMeetsRequirements(int i)
		{
			bool result = false;
			int cp = state.CurrentPlayer;
			bool[] useTrack = (bool[]) state.UseTrack.Clone();
			state.CurrentPlayer = i;
			for (int j=0; j<state.NumPlayers; j++)
				state.UseTrack[j] = (j == i);

			while (true)
			{
				if (state.ThisPlayer.X == -1)	// can't win if they're not on the map
					break;

				// do they have enough money?
				if (state.ThisPlayer.Funds < GameState.FundsGoal)
					break;

				// find all the map locations where their train can reach
				map.ResetFloodMap();
				map.Flood(state.ThisPlayer.X, state.ThisPlayer.Y, trainMovementMethod);

				// count the number of major cities we can reach
				int cities = 0;
				for (int j=0; j<map.NumCapitals; j++)
					if (CouldMoveTo(map.Cities[j].X, map.Cities[j].Y))
						cities++;
					else if (!CouldBuildTo(map.Cities[j].X, map.Cities[j].Y))
						cities++; // it counts if it's impossible to build to it

				// do we have enough major cities?
				int okToMiss = map.NumCapitals / 7;
				if (cities < map.NumCapitals - okToMiss)
					break;

				result = true;
				break;
			}

			state.CurrentPlayer = cp;
			state.UseTrack = useTrack;
			return result;
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
			ScreenShot();
#else
			RecordResult();

			if (winners.Count == 1)
				MessageBox.Show(form, state.PlayerInfo[(int) winners[0]].Name + " is the winner!", form.Text);
			else
			{
				string s = "Multiple winners:";
				foreach (int winner in winners)
					s += "\r\n\t" + state.PlayerInfo[winner].Name;
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
				bool official = false;
				foreach (Player p in state.PlayerInfo)
					if (p.Funds + upgradeCost * p.EngineType >= 100)
						official = true;
				return official;
			}
		}

		public void RecordResult()
		{
			if (state.Winners == null)
				state.Winners = new ArrayList();
			GameResultsCollection.Record(new GameResult(this));
		}

		// Quit the game, recording the loss if the game is already official. Use has the option
		// of cancelling without quitting.
		public bool Quit()
		{
			if (!IsOfficial)
				return true;

			if (state.DisableTax)
				return true;

			DialogResult dr = MessageBox.Show(Resource.GetString("Game.AlreadyOfficial"), form.Text, MessageBoxButtons.YesNo);
			return dr == DialogResult.Yes;
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
		}

		// restore the game state from the undo stack and redraw the form
		public void Undo()
		{
			PopState();
			ShowTrainRange();
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

			// if any events elapsed, redraw the map
			if (disasters.Count < n)
			{
				form.Invalidate();
			}

			// check if there's a new event
			state.DisasterProbability += 0.00315; // averages out to one disaster every 22 player turns
			if (rand.NextDouble() > state.DisasterProbability)
				return;
			state.DisasterProbability = 0.0;	// reset for next disaster

			// create the event
			Disaster disaster = Disaster.Create(state, map);

			// adjust the game state
			disaster.AffectState(state);

			// add the event to the disaster queue
			disasters.Enqueue(disaster);
			
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
				string s = disaster.ToString();
				int cr = s.IndexOf('\r');
				Newspaper news = new Newspaper(s.Substring(0, cr), s.Substring(cr + 1));
				suspendAI = true;
				news.ShowDialog(form);
				suspendAI = false;
				news.Close();
				news.Dispose();
			}
#endif

			// check which players lose a turn and/or a load
			for (int i=0; i<state.NumPlayers; i++)
			{
				if (disaster.PlayerLosesTurn(state.PlayerInfo[i]))
				{
					state.PlayerInfo[i].LoseTurn = true;
					if (state.PlayerInfo[i].Human)
						MessageBox.Show(state.PlayerInfo[i].Name + " loses a turn.",  form.Text);
				}
				if (disaster.PlayerLosesLoad(state.PlayerInfo[i]))
					if (state.PlayerInfo[i].LoseLoad(rand, state))
					{
						if (state.PlayerInfo[i].Human)
							MessageBox.Show(state.PlayerInfo[i].Name + " loses a load of cargo.",  form.Text);
					}
			}
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
			state.ThisPlayer.DeleteContract(map, i);
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
					DeleteContract(Player.NumContracts - 2);
					DeleteContract(Player.NumContracts - 1);

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
			return state.AccumCost == 0 && state.AccumTime == 0 && !state.ThisPlayer.LoseTurn;
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
			form.Invalidate(statusRect);
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
//					if (source < map.NumCapitals && cost > 0)
//						majorCityDiscount += 5;
					desc += "Build to " + map.Cities[source].Name + " for " + cost + ". ";
					time += MoveToCity(source);
					time += loadingTime;
				}

				// calculate cost/time to deliver it
				int cost2 = BuildTrackToCity(contract.Destination);
				if (cost2 == -1)
					continue;
//				if (contract.Destination < map.NumCapitals && cost2 > 0)
//					majorCityDiscount += 5;
				desc += "Build to " + map.Cities[contract.Destination].Name + " for " + cost2 + ". ";
				cost += cost2;
				time += MoveToCity(contract.Destination);
				time += loadingTime;

				// calculate the interest paid if we have to borrow
				if (cost > state.ThisPlayer.Funds)
					cost += cost - (state.ThisPlayer.Funds > 0 ? state.ThisPlayer.Funds : 0);

				// discount if we're connecting new major cities
				cost -= majorCityDiscount;

				// calculate profit
				int profit = AdjustPayment(contract.Payoff, contract.Destination) - cost;

				// penalize slow routes, e.g. ferry runs
				if (profit > 0)
					time *= time;

				// calculate the payoff rate (net income per hour)
				double payoff =  profit * 60.0 / time;

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

		// display the computer's action
		void ShowComputerIntent(string s)
		{
			if (ind != null)
				HideComputerIntent();
			cilen = s.Length;
			ind = new Indicator(s, Color.White, TrackColor[state.ThisPlayer.TrackColor]);
			Application.DoEvents();
		}

		// remove the display of the computer's action, leaving it up long enough to read
		// based on the length of the string (for a fast reader)
		void HideComputerIntent()
		{
			HideComputerIntent(true);
		}

		// remove the display of the computer's action, maybe or maybe not leaving it
		// up long enough to read
		void HideComputerIntent(bool waitToRead)
		{
#if TEST
#else
			if (waitToRead)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(250 + 30 * cilen);
			}
#endif
			ind.Close();
			ind.Dispose();
			ind = null;
			Application.DoEvents();
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
						cost += BuildTrackToCity(step.Source, false);
						if (cost == -1)
							goto cleanup;
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
					time += MoveToCity(c.Destination) + loadingTime;
					payoff += AdjustPayment(c.Payoff, c.Destination);
				}
			}

			// add interest if we need to borrow
			if (cost > state.ThisPlayer.Funds)
				cost += cost - (state.ThisPlayer.Funds > 0 ? state.ThisPlayer.Funds : 0);

			// calculate the payoff rate, in next profit per hour
			pph = (payoff - cost) * 60.0 / time;

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

			// check each second contract
			int terminalContracts = contract1 < Player.NumContracts - 3 ? 0 : 2;
			for (int contract2 = 0; contract2 < Player.NumContracts - terminalContracts; contract2++)
				if (contract2 != contract1)	// can't fulfill the same contract twice
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
						Strategy[] strategies = Strategy.CreateCombos(contract1, source1, contract2, source2);

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
		private int TotalCargoValue(Player pl)
		{
			int[] cv = pl.CargoValues;
			int total = 0;
			for (int i=0; i<pl.Cars; i++)
				if (cv[i] > 0)
				{
					City city = map.Cities[pl.Contracts[i].Destination];
					if (CouldMoveTo(city.X, city.Y, pl))
						total += cv[i];
				}
			return total;
		}

		// it's the computers turn, so when the form is otherwise idle, do computer stuff
		public void ComputerMove()
		{
			Contract contract;
			City city;
			int car;

			if (suspendAI)	// don't do anything if we're showing the newspaper
				return;

			// don't do anything if we're not a computer
			Player pl = state.ThisPlayer;
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

				if (pl.Funds >= GameState.FundsGoal)			// we have enough money to win
					if (state.AccumCost < GameState.MaxSpend)	// and can spend more this turn
						if (BuildToCheapestCapital())			// but we need to build to another city
							return;
			}

			// check if it's time to upgrade our train
			int tcv = TotalCargoValue(pl);
			if (((pl.Funds + tcv >= 51									// it's time
				&& tcv > 0												// and we have other stuff to do
				&& pl.Funds >= 10 + upgradeCost)						// and there's enough buffer money
				|| pl.Funds >= 51)										// or else we're just stinking rich
				&& state.AccumCost + upgradeCost <= GameState.MaxSpend	// and we haven't reached our spending limit
				&& state.AccumTime + upgradeTime <= GameState.MaxTime	// or our time limit
				&& pl.EngineType + 1 < Engine.Description.Length)		// and we haven't reach Maglev yet
			{															// so let's do it
				pl.EngineType++;										
				ShowComputerIntent("Upgrading train to " + Engine.Description[pl.EngineType]);
				pl.Spend(upgradeCost);
				state.AccumCost += upgradeCost;
				SpendTime(upgradeTime);
				form.Invalidate(statusRect);
				pl.NextUpgradePoint += pl.UpgradePointIncrement;
				HideComputerIntent();
			}

			// get our current strategy
			Strategy strategy = pl.Strategy;
			if (strategy != null && strategy.Steps.Count == 0)
				strategy = null;

			// if we don't have a current strategy, formulate one
			if (strategy == null)
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
				}

				// store this as our current strategy
				pl.Strategy = strategy;
				HideComputerIntent(false);

				// there are no deliveries possible with the current contracts
				// so dump all of them
				if (strategy == null)
				{
					if (!CanDiscardAll(true))
					{
						ShowComputerIntent("No feasible deliveries found. Waiting until next turn to rethink.");
						HideComputerIntent();
						goto DoneForNow;
					}
					ShowComputerIntent("Throwing away all contracts.");
					DiscardAll(true);
					HideComputerIntent(false);
					goto DoneForNow;
				}

				return;
			}

			// get the first step in our strategy
			Step step = (Step) strategy.Steps[0];
			
			// if we lost our contract, abandon this strategy
			if (step.Contract >= Player.NumContracts)
				goto AbandonStrategy;

			// if this is a single-contract strategy and we haven't evaluated the two-contract
			// strategies, do so now
			if (strategy.PayoffRate != double.MaxValue)	// haven't looked yet
				if (strategy.Steps.Count == 2 && step.PickUp && !((Step) strategy.Steps[1]).PickUp) // single-contract strategy?
				{
					// evaluate the two-contract strategies
					Strategy best;
					ShowComputerIntent(state.ThisPlayer.Name + " is thinking");
					Queue save = disasters;
					disasters = new Queue();
					double cp = ExamineCombos(step.Contract, step.Source, out best);
					disasters = save;
					HideComputerIntent(false);

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
						pl.Strategy = strategy = best;			// we found a better one
#if TEST
						LogPayoff(cp);
#endif
					}
					else
					{
#if TEST
						LogPayoff(strategy.PayoffRate);
#endif
						strategy.PayoffRate = double.MaxValue;	// don't recalculate this one again
					}
				}

			// get the first step again in case we just changed our strategy
			step = (Step) strategy.Steps[0];
			if (step.Contract >= Player.NumContracts)
				goto AbandonStrategy;

			contract = pl.Contracts[step.Contract];

			if (step.PickUp)	// we need to pick up a commodity
			{
				// determine how many of them we'll need
				int amt = 0;
				foreach (Step st in strategy.Steps)
					if (!st.PickUp)
						if (pl.Contracts[st.Contract].Product == contract.Product)
							amt++;

				// if we already have enough, go to the next step
				if (AmountCarried(contract.Product) >= amt)
					goto NextStep;

				// if we don't already have enough, but our evaluation said that we did,
				// we must have lost a load of cargo
				if (step.Source == -1)
					goto AbandonStrategy;

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
						if (state.AccumTime + loadingTime > GameState.MaxTime)
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
									needed = true;
							}
							if (!needed)
							{
								car = c;
								break;
							}
						}

						// if everything we're carrying is something we may want later, just drop the last car
						if (car == -1)
							car = pl.Cars - 1;

						// unload it
						ShowComputerIntent("Unloading " + Products.Name[pl.Cargo[car]] + " to make room");
						HideComputerIntent();
						state.ReleaseCommodity(ref pl.Cargo[car]);
						SpendTime(loadingTime);
					}

					// do we have time to load the cargo?
					if (state.AccumTime + loadingTime > GameState.MaxTime)
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

					// is this only a single-contract strategy?
					if (strategy.Steps.Count < 4)
					{
						// fill up cars with extra loads to protect against disasters
						// and to improve odds of serendipitous contracts
						while (true)
						{ 
							if (pl.EmptyCar == -1)
								break;
							if (state.AccumTime + loadingTime > GameState.MaxTime)
								break;
							if (state.Availability[contract.Product] == 0)
								break;
							ShowComputerIntent("Loading extra " + Products.Name[contract.Product]);
							pl.Cargo[pl.EmptyCar] = state.ReserveCommodity(contract.Product);
						}
					}

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
						pl.Spend(cost);
						state.AccumCost += cost;
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
					if (state.AccumTime + loadingTime > GameState.MaxTime)
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
				if (!TracksGoTo(city.X, city.Y))
				{
					ShowComputerIntent("Building track toward " + city.Name + " to deliver " + Products.Name[contract.Product]);
					int cost = BuildTrackToCity(contract.Destination, true);
					HideComputerIntent(false);
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
						state.ThisPlayer.Spend(cost);
						state.AccumCost += cost;
						form.Invalidate();
					}
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
			return;
			
AdjustContracts:
			// contract was fulfilled causing all other contracts to slide down either 2 or 3 notches
			int sc = step.Contract;
			foreach (Step s in strategy.Steps)
				if (s.Contract > sc)
					s.Contract += 2;
				else
					s.Contract += 3;

NextStep:
			strategy.Steps.RemoveAt(0);		// remove the current step from the list
			form.Invalidate(statusRect);
			return;

DoneForNow:
			NextPlayer();
			return;
		}
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
}