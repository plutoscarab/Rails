using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MapEditor
{
	/// <summary>
	/// Summary description for CitiesForm.
	/// </summary>
	public class CitiesForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.VScrollBar scroller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.PictureBox list;

		MapForm mapForm = null;
		int citiesPerPage = 10;
		int rowHeight = 56;

		public CitiesForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CitiesForm));
			this.scroller = new System.Windows.Forms.VScrollBar();
			this.list = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// scroller
			// 
			this.scroller.Dock = System.Windows.Forms.DockStyle.Right;
			this.scroller.Location = new System.Drawing.Point(335, 0);
			this.scroller.Name = "scroller";
			this.scroller.Size = new System.Drawing.Size(17, 278);
			this.scroller.TabIndex = 0;
			this.scroller.ValueChanged += new System.EventHandler(this.scroller_ValueChanged);
			// 
			// list
			// 
			this.list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(335, 278);
			this.list.TabIndex = 1;
			this.list.TabStop = false;
			this.list.Click += new System.EventHandler(this.list_Click);
			this.list.Paint += new System.Windows.Forms.PaintEventHandler(this.list_Paint);
			this.list.MouseMove += new System.Windows.Forms.MouseEventHandler(this.list_MouseMove);
			this.list.MouseLeave += new System.EventHandler(this.list_MouseLeave);
			// 
			// CitiesForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(352, 278);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.list,
																		  this.scroller});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CitiesForm";
			this.Text = "Cities";
			this.Resize += new System.EventHandler(this.CitiesForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CitiesForm_Closing);
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.list_MouseWheel);
			this.ResumeLayout(false);

		}
		#endregion

		private void CitiesForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		public void SetMap(MapForm mapForm)
		{
			this.mapForm = mapForm;
			if (mapForm.Cities.Count == 0)
				scroller.Maximum = 0;
			else
				scroller.Maximum = mapForm.Cities.Count - 1;
			list.Refresh();
		}

		int highlighted = -1;

		private void list_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Font font = new Font("Arial Bold", 10.0f);
			for (int i=0; i<citiesPerPage; i++)
			{
				int index = scroller.Value + i;
				if (index < mapForm.Cities.Count)
				{
					int y = rowHeight * i;
					City city = (City) mapForm.Cities[index];
					if (i == highlighted)
					{
						g.FillRectangle(Brushes.DarkBlue, 0, y, list.Width, rowHeight);
					}
					else
					{
						g.FillRectangle(Brushes.Black, 0, y, list.Width, rowHeight);
					}
					if (city.Products.Count >= 1)
					{
						g.DrawImageUnscaled(Images.CommodityImage((string) city.Products[0]), 56, y + 4);
					}
					if (city.Products.Count >= 2)
					{
						g.DrawImageUnscaled(Images.CommodityImage((string) city.Products[1]), 4, y + 4);
					}
					switch(city.Type)
					{
						case CityType.Capital:
							g.DrawImageUnscaled(Images.Capital, 108, y + 11);
							g.DrawImageUnscaled(Images.BlackDot, 122, y + 27);
							break;
						case CityType.City:
							g.DrawImageUnscaled(Images.City, 116, y + 21);
							g.DrawImageUnscaled(Images.BlackDot, 122, y + 27);
							break;
						case CityType.Town:
							g.DrawImageUnscaled(Images.Town, 116, y + 21);
							g.DrawImageUnscaled(Images.BlackDot, 122, y + 27);
							break;
					}
					g.DrawString(city.Name, font, Brushes.White, 144, y + 20);
				}
			}
			font.Dispose();
		}

		private void CitiesForm_Resize(object sender, System.EventArgs e)
		{
			citiesPerPage = (list.Height + rowHeight - 1) / rowHeight;
			if (citiesPerPage < 1)
				citiesPerPage = 1;
			scroller.LargeChange = citiesPerPage;
		}

		private void scroller_ValueChanged(object sender, System.EventArgs e)
		{
			list.Refresh();
		}

		private void list_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			highlighted = e.Y / rowHeight;
			list.Refresh();
		}

		int delta = 0;

		private void list_MouseWheel(object sender, MouseEventArgs e)
		{
			delta += e.Delta;
			while (delta >= 120)
			{
				delta -= 120;
				if (scroller.Value > 0)
					scroller.Value--;
			}
			while (delta <= -120)
			{
				delta += 120;
				if (scroller.Value + scroller.LargeChange <= scroller.Maximum)
					scroller.Value++;
			}
		}

		private void list_MouseLeave(object sender, System.EventArgs e)
		{
			highlighted = -1;
			list.Refresh();
		}

		private void list_Click(object sender, System.EventArgs e)
		{
			if (highlighted == -1)
				return;

			int index = highlighted + scroller.Value;
			if (index < 0 || index >= mapForm.Cities.Count)
				return;

			EditCity((City) mapForm.Cities[index]);
			list.Refresh();
			mapForm.Refresh();
			mapForm.Dirty();
		}

		void EditCity(City city)
		{
			EditCityForm form = new EditCityForm(city);
			form.ShowDialog();
			form.Dispose();
		}
	}
}
