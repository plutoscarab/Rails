using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MapEditor
{
	/// <summary>
	/// Summary description for EditCityForm.
	/// </summary>
	public class EditCityForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox cityName;
		private System.Windows.Forms.PictureBox product1;
		private System.Windows.Forms.PictureBox product2;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RadioButton townType;
		private System.Windows.Forms.RadioButton cityType;
		private System.Windows.Forms.RadioButton capitalType;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button deleteCity;

		City city;

		public EditCityForm(City city)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.city = city;
			cityName.Text = city.Name;
			cityName.SelectAll();
			switch(city.Type)
			{
				case CityType.Town:
					townType.Checked = true;
					break;
				case CityType.City:
					cityType.Checked = true;
					break;
				case CityType.Capital:
					capitalType.Checked = true;
					break;
			}
			if (city.Products.Count >= 1)
			{
				product2.Tag = (string) city.Products[0];
				product2.Image = Images.CommodityImage((string) city.Products[0]);
			}
			if (city.Products.Count >= 2)
			{
				product1.Tag = (string) city.Products[1];
				product1.Image = Images.CommodityImage((string) city.Products[1]);
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EditCityForm));
			this.label1 = new System.Windows.Forms.Label();
			this.cityName = new System.Windows.Forms.TextBox();
			this.product1 = new System.Windows.Forms.PictureBox();
			this.product2 = new System.Windows.Forms.PictureBox();
			this.OK = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.townType = new System.Windows.Forms.RadioButton();
			this.cityType = new System.Windows.Forms.RadioButton();
			this.capitalType = new System.Windows.Forms.RadioButton();
			this.deleteCity = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			// 
			// cityName
			// 
			this.cityName.Location = new System.Drawing.Point(16, 32);
			this.cityName.Name = "cityName";
			this.cityName.Size = new System.Drawing.Size(176, 20);
			this.cityName.TabIndex = 1;
			this.cityName.Text = "";
			// 
			// product1
			// 
			this.product1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.product1.Location = new System.Drawing.Point(88, 80);
			this.product1.Name = "product1";
			this.product1.Size = new System.Drawing.Size(52, 52);
			this.product1.TabIndex = 2;
			this.product1.TabStop = false;
			this.product1.Click += new System.EventHandler(this.product1_Click);
			// 
			// product2
			// 
			this.product2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.product2.Location = new System.Drawing.Point(144, 80);
			this.product2.Name = "product2";
			this.product2.Size = new System.Drawing.Size(52, 52);
			this.product2.TabIndex = 3;
			this.product2.TabStop = false;
			this.product2.Click += new System.EventHandler(this.product2_Click);
			// 
			// OK
			// 
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(32, 184);
			this.OK.Name = "OK";
			this.OK.TabIndex = 4;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(120, 184);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 5;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(88, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Commodities";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "Type";
			// 
			// townType
			// 
			this.townType.Location = new System.Drawing.Point(16, 80);
			this.townType.Name = "townType";
			this.townType.Size = new System.Drawing.Size(64, 24);
			this.townType.TabIndex = 8;
			this.townType.Text = "Town";
			// 
			// cityType
			// 
			this.cityType.Location = new System.Drawing.Point(16, 104);
			this.cityType.Name = "cityType";
			this.cityType.Size = new System.Drawing.Size(64, 24);
			this.cityType.TabIndex = 9;
			this.cityType.Text = "City";
			// 
			// capitalType
			// 
			this.capitalType.Location = new System.Drawing.Point(16, 128);
			this.capitalType.Name = "capitalType";
			this.capitalType.Size = new System.Drawing.Size(64, 24);
			this.capitalType.TabIndex = 10;
			this.capitalType.Text = "Capital";
			// 
			// deleteCity
			// 
			this.deleteCity.Location = new System.Drawing.Point(120, 152);
			this.deleteCity.Name = "deleteCity";
			this.deleteCity.TabIndex = 11;
			this.deleteCity.Text = "Delete city";
			this.deleteCity.Click += new System.EventHandler(this.deleteCity_Click);
			// 
			// EditCityForm
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(210, 216);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.deleteCity,
																		  this.capitalType,
																		  this.cityType,
																		  this.townType,
																		  this.label3,
																		  this.label2,
																		  this.cancel,
																		  this.OK,
																		  this.product2,
																		  this.product1,
																		  this.cityName,
																		  this.label1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EditCityForm";
			this.Text = "Edit city";
			this.ResumeLayout(false);

		}
		#endregion

		private void OK_Click(object sender, System.EventArgs e)
		{
			city.Name = cityName.Text;
			if (townType.Checked)
				city.Type = CityType.Town;
			if (cityType.Checked)
				city.Type = CityType.City;
			if (capitalType.Checked)
				city.Type = CityType.Capital;
			city.Products = new ArrayList();
			if (product2.Tag != null)
				city.Products.Add((string) product2.Tag);
			if (product1.Tag != null)
				city.Products.Add((string) product1.Tag);
		}

		private void product1_Click(object sender, System.EventArgs e)
		{
			CommodityGallery form = new CommodityGallery();
			try
			{
				if (form.ShowDialog() == DialogResult.OK)
				{
					product1.Tag = form.Selected;
					if (form.Selected == null)
						product1.Image = null;
					else
						product1.Image = Images.CommodityImage(form.Selected);
				}
				form.Dispose();
			}
			catch
			{
			}
		}

		private void product2_Click(object sender, System.EventArgs e)
		{
			CommodityGallery form = new CommodityGallery();
			if (form.ShowDialog() == DialogResult.OK)
			{
				product2.Tag = form.Selected;
				if (form.Selected == null)
					product2.Image = null;
				else
					product2.Image = Images.CommodityImage(form.Selected);
			}
			form.Dispose();
		}

		private void deleteCity_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to delete " + cityName.Text + "?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				this.DialogResult = DialogResult.No;
				this.Close();
			}
		}
	}
}
