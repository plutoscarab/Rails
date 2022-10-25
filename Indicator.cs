using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for Indicator.
	/// </summary>
	[System.Runtime.InteropServices.ComVisible(false)]
	public class Indicator : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Indicator(string text)
		{
#if TEST
			return;
#endif
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			label1.Text = String.Empty;
			this.Show();
			SetText(text);
		}

		public Indicator(string text, Color foreground, Color background)
		{
#if TEST
			return;
#endif
			InitializeComponent();

			label1.Text = String.Empty;
			label1.ForeColor = foreground;
			label1.BackColor = background;
			this.Show();
			SetText(text);
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
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(192, 48);
			this.label1.TabIndex = 0;
			this.label1.Text = String.Empty;
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Indicator
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(192, 48);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1});
			this.Name = "Indicator";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);

		}
		#endregion

		public void SetText(string text)
		{
#if TEST
			return;
#endif
			label1.Text = text;
			this.Refresh();
		}
	}
}
