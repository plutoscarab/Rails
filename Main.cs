
// Main.cs

using System;
using System.Threading;
using System.Net;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace Rails
{
	public class App
	{
		[STAThread]
		static void Main() 
		{
			using (Splash sp = new Splash())
			{
				sp.ShowDialog();
			}

			return;

			// check for a newer version in the background
			Thread t = new Thread(new ThreadStart(CheckForNewVersion));
			t.ApartmentState = ApartmentState.STA;
			t.Start();

			Form1 form = new Form1();
			form.Show();

			// if we were in the middle of a game, restore the game state
			if (File.Exists("game.sav"))
			{
				try
				{
					Stream s = new FileStream("game.sav", FileMode.Open);
					BinaryReader r = new BinaryReader(s);
					try
					{
						form.game = new Game(r, form);
						form.map = form.game.map;
					}
					catch
					{
						if (form.game != null) form.game.Dispose();
						form.game = null;
						if (form.map != null) form.map.Dispose();
					}
					r.Close();
					s.Close();
				}
				catch
				{
					if (form.game != null) form.game.Dispose();
					form.game = null;
					if (form.map != null) form.map.Dispose();
				}
			}
			
			// otherwise start a new game
			if (form.game == null)
				form.StartNewGame();

			// start the message loop a pumpin'
			Application.Run(form);
		}

		// Check online to see if a newer version is available. I'll get rid of
		// this once the game is no longer under development (maybe).
		static void CheckForNewVersion()
		{
			Stream webStream = null;
			StreamReader reader = null;
			RegistryKey key = null;
			try
			{
				Version thisVersion = Assembly.GetExecutingAssembly().GetName().Version;

				string subkey = "Software\\Pluto Scarab\\Rails";
				key = Registry.LocalMachine.OpenSubKey(subkey, true);
				if (key == null)
					key = Registry.LocalMachine.CreateSubKey(subkey);

				string ver = (string) key.GetValue("Version", "1.0.0.0");
				Version lastVersion = new Version(ver);

				// is this the first time we've run this new version?
				if (thisVersion > lastVersion)
				{
					key.SetValue("Version", thisVersion.ToString());
					key.SetValue("VersionCheck", 1);
					return;
				}

				// did we already bug the user about checking for a newer version than this?
				int check = (int) key.GetValue("VersionCheck", 1);
				if (check == 0)
					return;

				// download the versions file to read the version available online
				Version version = null;
				using (WebClient web = new WebClient())
				{								 
					string address = "http://bretm.home.comcast.net/versions.txt";
					webStream = web.OpenRead(address);
					reader = new StreamReader(web.OpenRead(address));
					string line;
					char[] tab = {'\t'};
					while ((line = reader.ReadLine()) != null)
					{
						string[] appVersion = line.Split(tab);
						if (appVersion.Length == 2)
							if (appVersion[0] == "Rails")
							{
								version = new Version(appVersion[1]);
								break;
							}
					}
				}

				if (version == null)
					return;

				// tell them about it, but only once
				if (version > thisVersion)
				{
					key.SetValue("VersionCheck", 0);
					string message = Resource.GetString("Form1.NewVersionAvailable");
					if (DialogResult.Yes == MessageBox.Show(message, Resource.GetString("Rails"), MessageBoxButtons.YesNo))
					{
						System.Diagnostics.Process.Start("http://bretm.home.comcast.net/rails.html");
					}
				}
				else
				{
					key.SetValue("VersionCheck", 1);
				}
			}
			catch(IOException)
			{
			}
			catch(WebException)
			{
			}
			catch(System.Security.SecurityException)
			{
			}
			finally
			{
				if (reader != null)
					reader.Close();
				if (webStream != null)
					webStream.Close();
				if (key != null)
					key.Close();
			}
		}
	}
}
