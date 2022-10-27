using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for TopCommodities.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class TopCommodities : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Game game;
		private Map map;
		private double[] score;
		private System.Windows.Forms.VScrollBar vScrollBar1;
		private int[] index;

		int dy = 56;

		public TopCommodities(Form owner, Game game, Map map)
		{
			this.game = game;
			this.map = map;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.Owner = owner;

			int n = Products.Count;
			score = map.AveragePayoff;
			index = new int[n];
			for (int i=0; i<n; i++)
			{
				index[i] = i;
			}
			Array.Sort(score, index);

			vScrollBar1.Maximum = dy * Products.Count - 1;
			vScrollBar1.LargeChange = this.ClientRectangle.Height;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TopCommodities));
			this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
			this.SuspendLayout();
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
			this.vScrollBar1.Location = new System.Drawing.Point(183, 0);
			this.vScrollBar1.Maximum = 28;
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.Size = new System.Drawing.Size(17, 448);
			this.vScrollBar1.SmallChange = 10;
			this.vScrollBar1.TabIndex = 0;
			this.vScrollBar1.Resize += new System.EventHandler(this.vScrollBar1_Resize);
			this.vScrollBar1.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
			// 
			// TopCommodities
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(200, 448);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.vScrollBar1});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(208, 2000);
			this.MinimumSize = new System.Drawing.Size(208, 0);
			this.Name = "TopCommodities";
			this.Text = "Average Payoffs";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.TopCommodities_Paint);
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		private void TopCommodities_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Bitmap bmp = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
			Graphics g = Graphics.FromImage(bmp);
			g.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
			int x0 = 8;
			int y0 = 0;
			Font font = new Font("Tahoma", 10);
			for (int i=0; i<Products.Count; i++)
			{
				int y = y0 + i * dy - vScrollBar1.Value;
				int j = Products.Count - 1 - i;
				if (j >= 0)
				{
					int p = index[j];
					g.DrawImageUnscaled(Products.Icon[p], x0 + 8, y + 4);
					g.DrawString(Products.Name[p], font, Brushes.White, x0 + 64, y + 8);
					g.DrawString(Utility.CurrencyString(score[j]), font, Brushes.White, x0 + 64, y + 24);
				}
			}
			font.Dispose();
			g.Dispose();
			e.Graphics.DrawImageUnscaled(bmp, 0, 0);
			bmp.Dispose();
		}

		private void vScrollBar1_Resize(object sender, System.EventArgs e)
		{
			vScrollBar1.LargeChange = this.ClientRectangle.Height;
		}

		private void vScrollBar1_ValueChanged(object sender, System.EventArgs e)
		{
			this.Invalidate();
		}
	}
}
