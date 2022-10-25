using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for Rankings.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class Rankings : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		Game game;
		Map map;

		int dy = 40;
		int[] quota;

		public Rankings(Form owner, Game game, Map map)
		{
			this.Owner = owner;
			this.game = game;
			this.map = map;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			Progress pr = new Progress("Computing major city quotas");
			quota = new int[game.state.NumPlayers];
			for (int i=0; i<quota.Length; i++)
			{
				quota[i] = game.CityQuota(i);
				pr.SetProgress((i+1)*100/quota.Length);
			}
			pr.Close();
			pr.Dispose();

			this.Size = new Size(this.Size.Width, 32 + 20 + game.state.NumPlayers * dy);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Rankings));
			// 
			// Rankings
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(459, 240);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Rankings";
			this.Text = "Player Rankings";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Rankings_Paint);

		}
		#endregion

		private void Rankings_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Font font = new Font("Tahoma", 10);
			int x1 = 4;
			int x2 = 40;
			int x3 = 110;
			int x4 = 178;
			int x5 = 340;
			int x6 = 400;
			g.FillRectangle(Brushes.Black, 0, 0, this.ClientRectangle.Width, 20);
			g.DrawString("Cash", font, Brushes.White, x2, 2);
			g.DrawString("Engine", font, Brushes.White, x3, 2);
			g.DrawString("Cars and Loads", font, Brushes.White, x4 + 26, 2);
			g.DrawString("Quota", font, Brushes.White, x5, 2);
			g.DrawString("Score", font, Brushes.White, x6, 2);
			for (int i=0; i<game.state.NumPlayers; i++)
			{
				int y = 20 + i * dy;
				Player pl = game.state.PlayerInfo[i];
				Brush brush = new SolidBrush(Game.TrackColor[pl.TrackColor]);
				g.FillRectangle(brush, 0, y, this.ClientRectangle.Width, dy);
				brush.Dispose();
				if (pl.Human)
					g.DrawImage(Images.HumanIcon.ToBitmap(), x1, y + 4, 32, 32);
				else
					g.DrawImage(Images.ComputerIcon.ToBitmap(), x1, y + 4, 32, 32);

				int score = pl.Funds;
				g.DrawString(Utility.CurrencyString(pl.Funds), font, Brushes.White, x2, y + 12);

				score += 20 * pl.EngineType;
				g.DrawString(Engine.Description[pl.EngineType], font, Brushes.White, x3, y + 12);

				score += 20 * (pl.Cars - 2);
				int cw = 36 * 4 + 2;
				g.FillRectangle(Brushes.Black, x4, y + 2, cw, 36);
				int sp = (cw - pl.Cars * 32) / (pl.Cars + 1);
				int[] cargoValues = pl.CargoValues;
				bool[] used = new bool[Player.NumContracts];
				for (int j=0; j<pl.Cars; j++)
				{
					int x = x4 + 2 + sp + j * (sp + 32);
					g.FillRectangle(Brushes.Gray, x, y + 4, 32, 32);
					int val = cargoValues[j];
					if (val > 0)
					{
						g.DrawImage(Products.Icon[pl.Cargo[j]], x, y + 4, 32, 32);
						score += val;
						string s = Utility.CurrencyString(val);
						SizeF size = g.MeasureString(s, font);
						Utility.DrawStringOutlined(g, s, font, Brushes.White, Brushes.Black, x + 16 - (int)(size.Width/2), y + 20 - (int)(size.Height/2));
					}
				}

				score -= quota[i];
				g.DrawString(Utility.CurrencyString(quota[i]), font, Brushes.White, x5, y + 12);

				g.DrawString(Utility.CurrencyString(score), font, Brushes.White, x6, y + 12);
			}
			font.Dispose();
		}
	}
}
