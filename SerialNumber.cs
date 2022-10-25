using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for SerialNumber.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class SerialNumber : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox serialNo;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Type mapType;

		public SerialNumber()
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

		public object Seed
		{
			get
			{
				try
				{
					uint u = uint.Parse(serialNo.Text);
					return unchecked((int) u);
				}
				catch(FormatException)
				{
					return serialNo.Text;
				}
				catch(ArgumentException)
				{
				}
				catch(ArithmeticException)
				{
				}
				return -1;
			}
		}

		public Type MapType
		{
			get
			{
				return mapType;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SerialNumber));
			this.label1 = new System.Windows.Forms.Label();
			this.serialNo = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(200, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "Specify the serial number of the map you wish to use:";
			// 
			// serialNo
			// 
			this.serialNo.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.serialNo.Location = new System.Drawing.Point(16, 56);
			this.serialNo.Name = "serialNo";
			this.serialNo.Size = new System.Drawing.Size(184, 23);
			this.serialNo.TabIndex = 1;
			this.serialNo.Text = "";
			this.serialNo.TextChanged += new System.EventHandler(this.serialNo_TextChanged);
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(40, 96);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(128, 96);
			this.button2.Name = "button2";
			this.button2.TabIndex = 3;
			this.button2.Text = "Cancel";
			// 
			// SerialNumber
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.ClientSize = new System.Drawing.Size(218, 136);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.button2,
																		  this.button1,
																		  this.serialNo,
																		  this.label1});
			this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SerialNumber";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Map Serial Number";
			this.ResumeLayout(false);

		}
		#endregion

		private void serialNo_TextChanged(object sender, System.EventArgs e)
		{
			button1.Enabled = false;
			try
			{
				uint u = uint.Parse(serialNo.Text);
				mapType = typeof(RandomMap);
				button1.Enabled = true;
			}
			catch(FormatException)
			{
				mapType = typeof(RandomMap2);
				button1.Enabled = true;
			}
			catch(ArgumentException)
			{
			}
			catch(ArithmeticException)
			{
			}
		}
	}
}
