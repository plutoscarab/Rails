using System;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Collections;

namespace Rails
{
	public enum NetworkMode
	{
		Offline, Hosting, Joined,
	}

	public class HostChannel : Network.Channel
	{
		object sync = new object();

		public HostChannel(object state)
		{
		}

		public override void Disconnected()
		{
			NetworkState.LostClient(this);
		}

		[Network.ProxyMethod]
		public void Hello(StringCollection users)
		{
			lock(sync)
				NetworkState.RegisterUsers(users, this);
		}

		[Network.ProxyMethod]
		public void Bye()
		{
			this.Stop();
		}

		[Network.ProxyMethod]
		public void Say(string user, string message)
		{
			lock(sync)
				Chat.Current.Heard(user, message);
		}

		[Network.ProxyMethod]
		public void SyncGameDataFull(byte[] data)
		{
			NetworkState.TransmitExcept(this, "SyncGameDataFull", data);
			Game.SyncGameDataFull(data);
		}

		[Network.ProxyMethod]
		public void SyncGameData(ArrayList delta)
		{
			NetworkState.TransmitExcept(this, "SyncGameData", delta);
			Game.SyncGameData(delta);
		}

		[Network.ProxyMethod]
		public void NeedFullSync()
		{
			NetworkState.TransmitExcept(this, "NeedFullSync");
			Game.NeedFullSync();
		}

		[Network.ProxyMethod]
		public void PhantomMouse(int x, int y)
		{
			NetworkState.TransmitExcept(this, "PhantomMouse", x, y);
			GameForm.UpdatePhantomMouse(x, y);
		}

		[Network.ProxyMethod]
		public void Newspaper(string headline, string subhead)
		{
			NetworkState.TransmitExcept(this, "Newspaper", headline, subhead);
			Game.ShowNewspaper(headline, subhead);
		}

		[Network.ProxyMethod]
		public void Winner(string message)
		{
			NetworkState.TransmitExcept(this, "Winner", message);
			System.Windows.Forms.MessageBox.Show(message, Resource.GetString("Rails"));
		}
	}

	public delegate void UserAcceptanceEventHandler(bool accepted, StringCollection users);
	public delegate void GainedClientEventHandler(HostChannel channel);
	public delegate void UsersUpdatedDelegate(StringCollection users);

	public class ClientChannel : Network.Channel
	{
		public event UserAcceptanceEventHandler OnUserAcceptance;

		[Network.ProxyMethod]
		public void UsersAccepted(StringCollection users)
		{
			if (OnUserAcceptance != null)
				OnUserAcceptance(true, users);
		}

		[Network.ProxyMethod]
		public void UsersRejected(StringCollection rejects)
		{
			if (OnUserAcceptance != null)
				OnUserAcceptance(false, rejects);
		}

		[Network.ProxyMethod]
		public void Say(string user, string message)
		{
			Chat.Current.Heard(user, message);
		}

		[Network.ProxyMethod]
		public void UsersUpdated(StringCollection users)
		{
			NetworkState.InvokeUsersUpdated(users);
		}

		[Network.ProxyMethod]
		public void OpenNewGameDialog(NewGameInfo info)
		{
			NewGame.Start(info);
		}

		[Network.ProxyMethod]
		public void SetNewGameInfo(NewGameInfo info)
		{
			NewGame.SetInfo(info);
		}

		[Network.ProxyMethod]
		public void CancelNewGame()
		{
			NewGame.CancelNewGame();
		}

		[Network.ProxyMethod]
		public void StartGame(byte[] data)
		{
			NewGame.StartGame(data);
		}

		[Network.ProxyMethod]
		public void SyncGameDataFull(byte[] data)
		{
			Game.SyncGameDataFull(data);
		}

		[Network.ProxyMethod]
		public void SyncGameData(ArrayList delta)
		{
			Game.SyncGameData(delta);
		}

		[Network.ProxyMethod]
		public void NeedFullSync()
		{
			Game.NeedFullSync();
		}

		[Network.ProxyMethod]
		public void PhantomMouse(int x, int y)
		{
			GameForm.UpdatePhantomMouse(x, y);
		}

		[Network.ProxyMethod]
		public void Newspaper(string headline, string subhead)
		{
			Game.ShowNewspaper(headline, subhead);
		}

		[Network.ProxyMethod]
		public void Winner(string message)
		{
			System.Windows.Forms.MessageBox.Show(message, Resource.GetString("Rails"));
		}

		[Network.ProxyMethod]
		public void ShowComputerIntent(string s)
		{
			Game.Current.ShowComputerIntent(s);
		}

		[Network.ProxyMethod]
		public void HideComputerIntent(bool b)
		{
			Game.Current.HideComputerIntent(b);
		}

		[Network.ProxyMethod]
		public void NeedSync()
		{
			Game.Current.Sync();
		}
	}

	public delegate void UsersUpdatedEventHandler(StringCollection users);

	public class NetworkState
	{
		const int BufferSize = 20000;

		public static NetworkMode Mode = NetworkMode.Offline;

		public static Network.Host Host;
		public static ClientChannel Client;

		public static bool IsMaster
		{
			get { return Mode != NetworkMode.Joined; }
		}

		public static bool IsHost
		{
			get { return Mode == NetworkMode.Hosting; }
		}

		public static bool IsOffline
		{
			get { return Mode == NetworkMode.Offline; }
		}

		public static bool StartHosting(IPAddress address, int port)
		{
			if (Mode != NetworkMode.Offline)
				throw new InvalidOperationException();

			try
			{
				Host = new Network.Host(address, port, typeof(HostChannel));
				Host.BufferSize = NetworkState.BufferSize;
			}
			catch
			{
				Host = null;
				return false;
			}

			Host.Start();
			Mode = NetworkMode.Hosting;
			return true;
		}

		public static void StopHosting()
		{
			if (Mode != NetworkMode.Hosting)
				throw new InvalidOperationException();

			Host.Stop();
			Host = null;
			Mode = NetworkMode.Offline;
			userList.Clear();
			UserChannels.Clear();
			channelUsers.Clear();
		}

		public static bool JoinHost(IPAddress address, int port)
		{
			if (Mode != NetworkMode.Offline)
				throw new InvalidOperationException();

			Client = new ClientChannel();
			Client.BufferSize = NetworkState.BufferSize;
			if (!Client.ConnectTo(address, port))
				return false;

			Client.OnDisconnected += new EventHandler(ClientDisconnected);
			Client.Start();
			Mode = NetworkMode.Joined;
			return true;
		}

		public static void LeaveHost()
		{
			if (Mode != NetworkMode.Joined)
				throw new InvalidOperationException();

			Client.Stop();
			Mode = NetworkMode.Offline;
		}

		public static event EventHandler LostHost;

		private static void ClientDisconnected(object sender, EventArgs e)
		{
			Mode = NetworkMode.Offline;
			if (LostHost != null)
				LostHost(sender, e);
		}

		public static void Stop()
		{
			if (Mode == NetworkMode.Hosting)
				StopHosting();

			if (Mode == NetworkMode.Joined)
				LeaveHost();
		}

		public static void Transmit(string method, params object[] args)
		{
			if (IsHost)
			{
				foreach (HostChannel channel in Host.Clients)
					channel.Invoke(method, args);
			}
			else if (!IsMaster)
			{
				Client.Invoke(method, args);
			}
		}

		public static void TransmitExcept(HostChannel except, string method, params object[] args)
		{
			if (IsHost)
			{
				foreach (HostChannel channel in Host.Clients)
					if (channel != except)
						channel.Invoke(method, args);
			}
		}

		static StringCollection userList = new StringCollection();
		public static Hashtable UserChannels = new Hashtable();
		static Hashtable channelUsers = new Hashtable();

		public static event GainedClientEventHandler GainedClient;

		public static void RegisterUsers(StringCollection users, HostChannel channel)
		{
			StringCollection cu = null;

			if (channel != null)	// remote users
			{
				StringCollection rejects = new StringCollection();
				foreach (string user in users)
					if (userList.Contains(user))
						rejects.Add(user);
				if (rejects.Count == 0)
				{
					channel.Invoke("UsersAccepted", users);
					if (GainedClient != null)
						GainedClient(channel);
				}
				else
				{
					channel.Invoke("UsersRejected", rejects);
					return;
				}
			
				cu = (StringCollection) channelUsers[channel];
				if (cu == null)
					channelUsers[channel] = cu = new StringCollection();
			}

			foreach (string user in users)
			{
				userList.Add(user);
				UserChannels[user] = channel;
				if (cu != null)
					cu.Add(user);
			}

			InvokeUsersUpdated(userList);

			if (channel != null)
				Transmit("UsersUpdated", userList);
		}

		public static bool UserIsLocal(string user)
		{
			return UserChannels[user] == null;
		}

		public static event UsersUpdatedEventHandler UsersUpdated;

		public static void LostClient(HostChannel channel)
		{
			if (channelUsers[channel] == null)
				return;

			foreach (string user in (StringCollection) channelUsers[channel])
			{
				userList.Remove(user);
				UserChannels.Remove(user);
			}
			channelUsers.Remove(channel);
			InvokeUsersUpdated(userList);
			Transmit("UsersUpdated", userList);
		}

		public static void InvokeUsersUpdated(StringCollection users)
		{
			if (UsersUpdated != null)
				UsersUpdated(users);
		}

		public static StringCollection CurrentUsers
		{
			get { return userList; }
		}

		public static void KickUser(string user)
		{
			HostChannel channel = (HostChannel) UserChannels[user];
			if (channel != null)
				channel.Stop();
		}
	}
}