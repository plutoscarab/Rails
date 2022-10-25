using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace Network
{
	public class Host
	{
		IPEndPoint endPoint;
		Thread connectionListener;
		bool enabled;
		public ArrayList Clients;
		public int Backlog = 10;
		Socket server;
		Type channelType;
		object state;
		public int BufferSize;

		public Host(IPAddress address, int port) 
			: this(address, port, null)
		{
		}

		public Host(IPAddress address, int port, Type channelType)
			: this(address, port, channelType, null)
		{
		}

		public Host(IPAddress address, int port, Type channelType, object state)
		{
			endPoint = new IPEndPoint(address, port);
			this.channelType = channelType;
			this.state = state;
			Clients = new ArrayList();
		}

		public void Start()
		{
			connectionListener = new Thread(new ThreadStart(ConnectionListener));
			connectionListener.IsBackground = true;
			enabled = true;
			connectionListener.Start();
		}

		public void Stop()
		{
			enabled = false;
			Channel[] c = (Channel[]) Clients.ToArray(typeof(Channel));
			for (int i=0; i<c.Length; i++)
				c[i].Stop();
			connectionListener.Abort();
			if (server != null)
			{
				server.Close();
				server = null;
			}
		}

		void ConnectionListener()
		{
			server = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			server.Bind(endPoint);
			server.Listen(Backlog);
			int id = 0;
			while (enabled)
			{
				Socket socket = server.Accept();
				Channel channel;
				if (channelType == null)
					channel = new Channel();
				else
					channel = (Channel) channelType.GetConstructor(new Type[] {typeof(object)}).Invoke(new object[] {state});
				channel.BufferSize = this.BufferSize;
				channel.ConnectFrom(++id, socket);
				channel.OnDisconnected += new EventHandler(Disconnected);
				Clients.Add(channel);
				channel.Start();
				channel.Connected();
			}
			server.Close();
			server = null;
		}

		void Disconnected(object sender, EventArgs e)
		{
			Channel channel = sender as Channel;
			Clients.Remove(channel);
			channel.Disconnected();
		}
	}

}
