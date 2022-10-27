using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections.Specialized;

namespace Rails
{
	/// <summary>
	/// Summary description for LocalUsers.
	/// </summary>
	public class LocalUsers : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckedListBox people;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.TextBox newUser;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LocalUsers()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey, false);
			if (key == null)
				return;
			try
			{
				string[] users = key.GetValueNames();
				bool[] here = new bool[users.Length];
				for (int i=0; i<users.Length; i++)
				{
					here[i] = 1 == (int) key.GetValue(users[i], 0);
					people.Items.Add(users[i], here[i]);
				}
				newUser.Focus();
			}
			finally
			{
				key.Close();
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.people = new System.Windows.Forms.CheckedListBox();
			this.addButton = new System.Windows.Forms.Button();
			this.newUser = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(152, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "People at this computer:";
			// 
			// people
			// 
			this.people.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.people.CheckOnClick = true;
			this.people.Location = new System.Drawing.Point(0, 16);
			this.people.Name = "people";
			this.people.Size = new System.Drawing.Size(224, 169);
			this.people.Sorted = true;
			this.people.TabIndex = 0;
			this.people.KeyDown += new System.Windows.Forms.KeyEventHandler(this.people_KeyDown);
			// 
			// addButton
			// 
			this.addButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(192, 192);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(32, 23);
			this.addButton.TabIndex = 2;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// newUser
			// 
			this.newUser.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.newUser.Location = new System.Drawing.Point(0, 192);
			this.newUser.Name = "newUser";
			this.newUser.Size = new System.Drawing.Size(184, 20);
			this.newUser.TabIndex = 1;
			this.newUser.Text = "";
			this.newUser.KeyDown += new System.Windows.Forms.KeyEventHandler(this.newUser_KeyDown);
			// 
			// LocalUsers
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.newUser,
																		  this.addButton,
																		  this.people,
																		  this.label1});
			this.Name = "LocalUsers";
			this.Size = new System.Drawing.Size(224, 216);
			this.Load += new System.EventHandler(this.LocalUsers_Load);
			this.Leave += new System.EventHandler(this.LocalUsers_Leave);
			this.ResumeLayout(false);

		}
		#endregion

		string subKey = "Software\\Pluto Scarab\\Rails\\Users";

		void PersistList()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey, true);
			if (key == null)
				key = Registry.CurrentUser.CreateSubKey(subKey);
			try
			{
				for (int i=0; i<people.Items.Count; i++)
					key.SetValue((string) people.Items[i], people.GetItemChecked(i) ? 1 : 0);
				string[] users = key.GetValueNames();
				foreach (string user in users)
					if (!people.Items.Contains(user))
						key.DeleteValue(user, false);
			}
			finally
			{
				key.Close();
			}
		}

		private void LocalUsers_Load(object sender, System.EventArgs e)
		{
		}

		void Add()
		{
			string name = newUser.Text.Trim();
			if (name.Length == 0)
				return;

			if (name.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(" bot"))
				name = name.Substring(0, name.Length - 4);

			foreach (string user in people.Items)
				if (user == name)
					return;

			people.Items.Add(name, true);
			newUser.Text = "";
			newUser.Focus();
		}

		private void addButton_Click(object sender, System.EventArgs e)
		{
			Add();
		}

		private void people_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				int i = people.SelectedIndex;
				if (i < 0 || i >= people.Items.Count)
					return;

				people.Items.Remove(people.Items[i]);
			}
		}

		private void LocalUsers_Leave(object sender, System.EventArgs e)
		{
			PersistList();
		}

		private void newUser_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				Add();
		}

		public TextBox NewUser
		{
			get { return this.newUser; }
		}

		public StringCollection Users
		{
			get
			{
				StringCollection users = new StringCollection();
				for (int i=0; i<people.Items.Count; i++)
					if (people.GetItemChecked(i))
						users.Add((string) people.Items[i]);
				return users;
			}
		}
	}
}
