using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rails
{
	/// <summary>
	/// Summary description for HostStatus.
	/// </summary>
	public class HostStatus : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Timer updateTimer;
		private System.ComponentModel.IContainer components;

		public HostStatus()
		{
			InitializeComponent();
		}

		public Network.Host Host;
		HostChannel[] clients;
		bool refreshClientList;
		Hashtable names = new Hashtable();

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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HostStatus));
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button1 = new System.Windows.Forms.Button();
			this.updateTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listBox1.ItemHeight = 14;
			this.listBox1.Location = new System.Drawing.Point(16, 16);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(184, 88);
			this.listBox1.TabIndex = 0;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// button1
			// 
			this.button1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(16, 112);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(184, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Disconnect selected computer";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// updateTimer
			// 
			this.updateTimer.Enabled = true;
			this.updateTimer.Interval = 200;
			this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
			// 
			// HostStatus
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(216, 150);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.button1,
																		  this.listBox1});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(99999, 176);
			this.MinimumSize = new System.Drawing.Size(224, 176);
			this.Name = "HostStatus";
			this.Text = "Connected Computers";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.HostStatus_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		public void Connected(HostChannel client)
		{
			refreshClientList = true;
		}

		public void Disconnected(HostChannel client)
		{
			refreshClientList = true;
		}

		public void RefreshClientList()
		{
			if (!refreshClientList)
				return;
			refreshClientList = false;
			listBox1.Items.Clear();
			foreach (HostChannel channel in Host.Clients)
			{
				string name = (string) names[channel.ID];
				if (name == null || name.Trim().Length == 0)
					name = "Computer #" + channel.ID;
				listBox1.Items.Add(name);
			}
			clients = (HostChannel[]) Host.Clients.ToArray(typeof(HostChannel));
			EnableButton();
		}

		public void SetName(int channelID, string name)
		{
			names[channelID] = name;
			refreshClientList = true;
		}

		private void HostStatus_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			EnableButton();
		}

		void EnableButton()
		{
			button1.Enabled = listBox1.SelectedIndex != -1 && listBox1.Items.Count > 0;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			HostChannel channel = clients[listBox1.SelectedIndex];
			channel.Stop();
		}

		private void updateTimer_Tick(object sender, System.EventArgs e)
		{
			RefreshClientList();
		}
	}
}
