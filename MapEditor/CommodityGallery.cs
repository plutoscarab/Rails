using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace MapEditor
{
	/// <summary>
	/// Summary description for CommodityGallery.
	/// </summary>
	public class CommodityGallery : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox list;
		private System.Windows.Forms.VScrollBar scroller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		ArrayList files;
		Hashtable icons;
		int across = 1, down = 1;
		int highlighted = -1;

		public CommodityGallery()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			files = new ArrayList();
			icons = new Hashtable();

			files.Add(string.Empty);
			icons.Add(string.Empty, null);

			string path = Path.GetDirectoryName(Application.ExecutablePath);
			foreach (string file in Directory.GetFiles(path + "\\commodities", "*.bmp"))
			{
				files.Add(file);
				icons.Add(file, Image.FromFile(file));
			}

			ResizeList();
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
				foreach (Image image in icons.Values)
					if (image != null)
						image.Dispose();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CommodityGallery));
			this.list = new System.Windows.Forms.PictureBox();
			this.scroller = new System.Windows.Forms.VScrollBar();
			this.SuspendLayout();
			// 
			// list
			// 
			this.list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(288, 266);
			this.list.TabIndex = 0;
			this.list.TabStop = false;
			this.list.Click += new System.EventHandler(this.list_Click);
			this.list.Paint += new System.Windows.Forms.PaintEventHandler(this.list_Paint);
			this.list.MouseMove += new System.Windows.Forms.MouseEventHandler(this.list_MouseMove);
			this.list.MouseLeave += new System.EventHandler(this.list_MouseLeave);
			// 
			// scroller
			// 
			this.scroller.Dock = System.Windows.Forms.DockStyle.Right;
			this.scroller.Location = new System.Drawing.Point(271, 0);
			this.scroller.Name = "scroller";
			this.scroller.Size = new System.Drawing.Size(17, 266);
			this.scroller.TabIndex = 2;
			this.scroller.ValueChanged += new System.EventHandler(this.scroller_ValueChanged);
			this.scroller.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scroller_Scroll);
			// 
			// CommodityGallery
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(288, 266);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.scroller,
																		  this.list});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CommodityGallery";
			this.Text = "Commodity Gallery";
			this.Resize += new System.EventHandler(this.CommodityGallery_Resize);
			this.ResumeLayout(false);

		}
		#endregion

		private void scroller_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
		}

		void ResizeList()
		{
			across = (list.Width - scroller.Width) / 52;
			if (across < 1) across = 1;

			down = list.Height / 52;
			if (down < 1) down = 1;

			scroller.Maximum = (files.Count + across - 1) / across - 1;
			scroller.LargeChange = down;
		}

		private void CommodityGallery_Resize(object sender, System.EventArgs e)
		{
			ResizeList();
		}

		private void list_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.FillRectangle(Brushes.Black, 0, 0, list.Width, list.Height);
			for (int y=0; y<down; y++)
				for (int x=0; x<across; x++)
				{
					int x0 = 52 * x + 4;
					int y0 = 52 * y + 4;
					if (x + across * y == highlighted)
						g.FillRectangle(Brushes.DarkBlue, x0, y0, 52, 52);
					int index = across * (scroller.Value + y) + x;
					if (index == 0)
					{
						Font font = new Font("Arial", 7.0f);
						g.DrawString("None", font, Brushes.White, x0 + 12, y0 + 20);
						font.Dispose();
					}
					else if (index < files.Count)
					{
						g.DrawImageUnscaled((Image) icons[(string) files[index]], x0, y0);
					}

				}
		}

		private void scroller_ValueChanged(object sender, System.EventArgs e)
		{
			list.Refresh();
		}

		private void list_MouseLeave(object sender, System.EventArgs e)
		{
			highlighted = -1;
		}

		private void list_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int x = e.X / 52;
			int y = e.Y / 52;
			if (x < 0 || x >= across)
				highlighted = -1;
			else if (y < 0 || y >= down)
				highlighted = -1;
			else
			{
				highlighted = x + across * y;
				if (highlighted >= files.Count)
					highlighted = -1;
			}
			list.Refresh();
		}

		string name = null;

		private void list_Click(object sender, System.EventArgs e)
		{
			if (highlighted != -1)
			{
				highlighted += scroller.Value * across;
				if (highlighted == 0)
					name = null;
				else
					name = Path.GetFileNameWithoutExtension((string) files[highlighted]);
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		public string Selected
		{
			get
			{
				return name;
			}
		}
	}
}
