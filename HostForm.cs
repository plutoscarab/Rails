using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Collections.Specialized;

namespace Rails
{
	/// <summary>
	/// Summary description for HostForm.
	/// </summary>
	public class HostForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox tcpAddress;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tcpPort;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.GroupBox groupBox2;
		private Rails.LocalUsers localUsers;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public HostForm(bool isHost)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (isHost)
			{
				foreach (IPAddress ip in Dns.Resolve(Dns.GetHostName()).AddressList)
					tcpAddress.Items.Add(ip.ToString());
			
				if (tcpAddress.Items.Count > 0)
					tcpAddress.SelectedIndex = 0;
			}
			else
			{
				tcpAddress.DropDownStyle = ComboBoxStyle.Simple;
#if DEBUG
				tcpAddress.Text = "192.168.2.10";
#endif
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HostForm));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tcpPort = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tcpAddress = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.localUsers = new Rails.LocalUsers();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.tcpPort,
																					this.label2,
																					this.tcpAddress,
																					this.label1});
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(288, 64);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Network settings";
			// 
			// tcpPort
			// 
			this.tcpPort.Location = new System.Drawing.Point(224, 24);
			this.tcpPort.Name = "tcpPort";
			this.tcpPort.Size = new System.Drawing.Size(48, 22);
			this.tcpPort.TabIndex = 1;
			this.tcpPort.Text = "18407";
			this.tcpPort.Validating += new System.ComponentModel.CancelEventHandler(this.tcpPort_Validating);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(192, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(28, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Port";
			// 
			// tcpAddress
			// 
			this.tcpAddress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.tcpAddress.Location = new System.Drawing.Point(64, 24);
			this.tcpAddress.Name = "tcpAddress";
			this.tcpAddress.Size = new System.Drawing.Size(120, 22);
			this.tcpAddress.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Address";
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(136, 272);
			this.button1.Name = "button1";
			this.button1.TabIndex = 3;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.CausesValidation = false;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(224, 272);
			this.button2.Name = "button2";
			this.button2.TabIndex = 4;
			this.button2.Text = "Cancel";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.localUsers});
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 88);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(288, 168);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Users";
			// 
			// localUsers
			// 
			this.localUsers.Location = new System.Drawing.Point(16, 24);
			this.localUsers.Name = "localUsers";
			this.localUsers.Size = new System.Drawing.Size(256, 136);
			this.localUsers.TabIndex = 2;
			// 
			// HostForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(322, 312);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox2,
																		  this.button2,
																		  this.button1,
																		  this.groupBox1});
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "HostForm";
			this.Text = "Connection Settings";
			this.Activated += new System.EventHandler(this.HostForm_Activated);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void tcpPort_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				int i = int.Parse(tcpPort.Text);
				if (i < ushort.MinValue || i > ushort.MaxValue)
					e.Cancel = true;
			}
			catch
			{
				e.Cancel = true;
			}
		}

		private void HostForm_Activated(object sender, System.EventArgs e)
		{
			localUsers.NewUser.Focus();
		}

		public IPAddress Address
		{
			get { return IPAddress.Parse(tcpAddress.Text); }
		}

		public int Port
		{
			get { return int.Parse(tcpPort.Text); }
		}

		public StringCollection Users
		{
			get { return localUsers.Users; }
		}
	}
}
