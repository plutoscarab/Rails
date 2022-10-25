using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;

namespace MapEditor
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class AppForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem chooseBackground;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem undo;
		private System.Windows.Forms.MenuItem save;
		private System.Windows.Forms.MenuItem open;
		private System.Windows.Forms.MenuItem menuItem20;
		private System.Windows.Forms.MenuItem fade;
		private System.Windows.Forms.MenuItem importCommodity;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.StatusBar statusBar;

		public AppForm(string[] args)
		{
			InitializeComponent();
			foreach (string arg in args)
			{
				MapForm form = new MapForm(arg);
				form.MdiParent = this;
				form.Show();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppForm));
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.open = new System.Windows.Forms.MenuItem();
			this.save = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.chooseBackground = new System.Windows.Forms.MenuItem();
			this.importCommodity = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.undo = new System.Windows.Forms.MenuItem();
			this.menuItem20 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem12 = new System.Windows.Forms.MenuItem();
			this.menuItem13 = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.fade = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem8,
																					  this.menuItem20,
																					  this.menuItem10,
																					  this.menuItem4});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem2,
																					  this.open,
																					  this.save,
																					  this.menuItem9,
																					  this.chooseBackground,
																					  this.importCommodity,
																					  this.menuItem5,
																					  this.menuItem6});
			this.menuItem1.Text = "&File";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "&New";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// open
			// 
			this.open.Index = 1;
			this.open.Text = "&Open";
			this.open.Click += new System.EventHandler(this.open_Click);
			// 
			// save
			// 
			this.save.Index = 2;
			this.save.Text = "&Save";
			this.save.Click += new System.EventHandler(this.save_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 3;
			this.menuItem9.Text = "-";
			// 
			// chooseBackground
			// 
			this.chooseBackground.Index = 4;
			this.chooseBackground.Text = "Load &background";
			this.chooseBackground.Click += new System.EventHandler(this.chooseBackground_Click);
			// 
			// importCommodity
			// 
			this.importCommodity.Index = 5;
			this.importCommodity.Text = "&Import commodity icon";
			this.importCommodity.Click += new System.EventHandler(this.importCommodity_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 6;
			this.menuItem5.Text = "-";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 7;
			this.menuItem6.Text = "E&xit";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 1;
			this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.undo});
			this.menuItem8.Text = "&Edit";
			// 
			// undo
			// 
			this.undo.Index = 0;
			this.undo.Text = "&Undo";
			this.undo.Click += new System.EventHandler(this.undo_Click);
			// 
			// menuItem20
			// 
			this.menuItem20.Index = 2;
			this.menuItem20.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem3,
																					   this.menuItem7,
																					   this.menuItem12,
																					   this.menuItem13,
																					   this.menuItem11,
																					   this.fade});
			this.menuItem20.Text = "&Tools";
			this.menuItem20.Popup += new System.EventHandler(this.menuItem20_Popup);
			// 
			// menuItem3
			// 
			this.menuItem3.Checked = true;
			this.menuItem3.Index = 0;
			this.menuItem3.Shortcut = System.Windows.Forms.Shortcut.F5;
			this.menuItem3.Text = "&Cities and sea markers";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Shortcut = System.Windows.Forms.Shortcut.F6;
			this.menuItem7.Text = "&Terrain";
			this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Index = 2;
			this.menuItem12.Shortcut = System.Windows.Forms.Shortcut.F7;
			this.menuItem12.Text = "&Rivers";
			this.menuItem12.Click += new System.EventHandler(this.menuItem12_Click);
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 3;
			this.menuItem13.Shortcut = System.Windows.Forms.Shortcut.F8;
			this.menuItem13.Text = "Causeways and &ferries";
			this.menuItem13.Click += new System.EventHandler(this.menuItem13_Click);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 4;
			this.menuItem11.Text = "-";
			// 
			// fade
			// 
			this.fade.Index = 5;
			this.fade.Text = "Fade &background";
			this.fade.Click += new System.EventHandler(this.fade_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 3;
			this.menuItem10.MdiList = true;
			this.menuItem10.Text = "&Window";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 4;
			this.menuItem4.Text = "&Help";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 819);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(1016, 22);
			this.statusBar.TabIndex = 1;
			// 
			// AppForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(1016, 841);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.statusBar});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu1;
			this.Name = "AppForm";
			this.Text = "Rails Map Editor";
			this.MdiChildActivate += new System.EventHandler(this.AppForm_MdiChildActivate);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			SetFileAssociations();
			Application.Run(new AppForm(args));
		}

		static void SetFileAssociations()
		{
			try
			{
				RegistryKey key = Registry.ClassesRoot.OpenSubKey("rlmfile");
				if (key != null)
				{
					key.Close();
					return;
				}

				key = Registry.ClassesRoot.CreateSubKey("rlmfile");
				key.SetValue(null, "Rails Map");

				RegistryKey key2 = key.CreateSubKey("DefaultIcon");
				key2.SetValue(null, Application.ExecutablePath + ",0");
				key2.Close();

				key2 = key.CreateSubKey("shell");
				RegistryKey key3 = key2.CreateSubKey("open");

				RegistryKey key4 = key3.CreateSubKey("command");
				key4.SetValue(null, "\"" + Application.ExecutablePath + "\" \"%1\"");
				key4.Close();

				key3.Close();
				key2.Close();

				key.Close();

				key = Registry.ClassesRoot.CreateSubKey(".rlm");
				key.SetValue(null, "rlmfile");
				key.Close();
			}
			catch
			{
			}
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			MapForm form = new MapForm();
			form.MdiParent = this;
			form.Show();
		}

		MapForm CurrentMap()
		{
			return this.ActiveMdiChild as MapForm;
		}

		private void chooseBackground_Click(object sender, System.EventArgs e)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			form.ChooseBackground();
		}

		private void undo_Click(object sender, System.EventArgs e)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			form.Undo();
		}

		private void save_Click(object sender, System.EventArgs e)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			form.Save();
		}

		private void open_Click(object sender, System.EventArgs e)
		{
			MapForm.Open(this);
		}

		private void fade_Click(object sender, System.EventArgs e)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			form.Fade();
		}

		private void importCommodity_Click(object sender, System.EventArgs e)
		{
			Import form = new Import();
			if (form.Open())
				form.ShowDialog();
			form.Close();
			form.Dispose();
		}

		private void AppForm_MdiChildActivate(object sender, System.EventArgs e)
		{
		}

		void SetEditMode(EditMode editMode, MenuItem menuItem)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			form.SetEditMode(editMode);
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			SetEditMode(EditMode.CityOrSea, menuItem3);
		}

		private void menuItem7_Click(object sender, System.EventArgs e)
		{
			SetEditMode(EditMode.Terrain, menuItem7);
		}

		private void menuItem12_Click(object sender, System.EventArgs e)
		{
			SetEditMode(EditMode.River, menuItem12);
		}

		private void menuItem13_Click(object sender, System.EventArgs e)
		{
			SetEditMode(EditMode.Causeway, menuItem13);
		}

		void CheckEditMode(MenuItem menuItem)
		{
			menuItem3.Checked = (menuItem == menuItem3);
			menuItem7.Checked = (menuItem == menuItem7);
			menuItem12.Checked = (menuItem == menuItem12);
			menuItem13.Checked = (menuItem == menuItem13);
		}

		private void menuItem20_Popup(object sender, System.EventArgs e)
		{
			MapForm form = CurrentMap();
			if (form == null) return;
			switch (form.EditMode)
			{
				case EditMode.Causeway:
					CheckEditMode(menuItem13);
					break;
				case EditMode.CityOrSea:
					CheckEditMode(menuItem3);
					break;
				case EditMode.River:
					CheckEditMode(menuItem12);
					break;
				case EditMode.Terrain:
					CheckEditMode(menuItem7);
					break;
			}
		}
	}
}