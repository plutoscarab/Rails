using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using System.Text;

namespace Network
{
	public class DataReceivedEventArgs
	{
		public int ID;
		public byte[] Buffer;
		public int Count;

		public DataReceivedEventArgs(int id, byte[] buffer, int count)
		{
			ID = id;
			Buffer = buffer;
			Count = count;
		}
	}

	public delegate void DataReceivedEventHandler(Channel sender, DataReceivedEventArgs e);

	[AttributeUsage(AttributeTargets.Method)]
	public class ProxyMethodAttribute : Attribute
	{
		public ProxyMethodAttribute()
		{
		}
	}

	public class Channel
	{
		public int ID;
		public Socket Socket;

		Thread listenerProc;
		bool enabled;
		byte[] buffer = new byte[1000];

		public virtual void Connected()
		{
			Log("Connected");
		}

		public virtual void Disconnected()
		{
			Log("Disconnected");
		}

		public int BufferSize
		{
			get { return buffer.Length; }
			set { buffer = new byte[value]; }
		}

		public void ConnectFrom(int id, Socket socket)
		{
			ID = id;
			Socket = socket;
		}

		public bool ConnectTo(IPAddress address, int port) 
		{
			ID = -1;
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint endPoint = new IPEndPoint(address, port);
			try
			{
				Socket.Connect(endPoint);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public event EventHandler OnDisconnected;

		public void Start()
		{
			enabled = true;
			listenerProc = new Thread(new ThreadStart(ListenerProc));
			listenerProc.IsBackground = true;
			listenerProc.Start();
		}

		public void Stop()
		{
			if (!enabled)
				return;
			if (OnDisconnected != null)
				OnDisconnected(this, null);
			enabled = false;
			Socket.Close();
		}

		void Send(byte[] buffer, int count)
		{
			byte[] cb = BitConverter.GetBytes(count);
			Socket.BeginSend(cb, 0, cb.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
			Socket.BeginSend(buffer, 0, count, SocketFlags.None, new AsyncCallback(SendCallback), null);
		}

		void SendCallback(IAsyncResult asyncResult)
		{
			Socket.EndSend(asyncResult);
		}

		void ListenerProc()
		{
			while (enabled)
			{
				if (!Socket.Connected)
				{
					Stop();
					return;
				}
				try
				{
					int count = Socket.Receive(buffer);
					Log("Received " + count.ToString() + " bytes");
					if (count > 0)
					{
						DecodePackets(buffer, count);
					}
				}
				catch(SocketException e)
				{
					Log("Socket exception " + e.Message);
					Stop();
				}
			}
		}

		int packetIndex;

		public void Invoke(string methodName, params object[] args)
		{
			Type[] types = new Type[args.Length];
			for (int i=0; i<args.Length; i++)
				types[i] = args[i].GetType();

			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			packetIndex++;
			formatter.Serialize(stream, packetIndex);
			formatter.Serialize(stream, methodName);
			for (int i=0; i<args.Length; i++)
				formatter.Serialize(stream, args[i]);
			if (stream.Position >= this.BufferSize)
				throw new IOException();
			byte[] buffer = stream.GetBuffer();
			Send(buffer, (int) stream.Position);
			stream.Close();
		}

		object decodeSync = new Object();
		bool packetSizeNeeded = true;
		int bytesNeeded = 4;
		byte[] incoming = new byte[4];
		int bytesReceived = 0;

		void DecodePackets(byte[] buffer, int count)
		{
			lock(decodeSync)
			{
				int index = 0;
				while (index < count)
				{
					int n = Math.Min(bytesNeeded - bytesReceived, count - index);
					Array.Copy(buffer, index, incoming, bytesReceived, n);
					bytesReceived += n;
					index += n;
					if (bytesReceived == bytesNeeded)
					{
						if (packetSizeNeeded)
						{
							bytesNeeded = incoming[0] + 256 * (incoming[1] + 256 * (incoming[2] + 256 * incoming[3]));
							packetSizeNeeded = false;
						}
						else
						{
							InvokeProxyMember(incoming);
							bytesNeeded = 4;
							packetSizeNeeded = true;
						}
						incoming = new byte[bytesNeeded];
						bytesReceived = 0;
					}
				}
			}
		}

		void InvokeProxyMember(byte[] buffer)
		{
			try
			{
				MemoryStream stream = new MemoryStream(buffer);
//				while (stream.Position < stream.Length)
				{
					BinaryFormatter formatter = new BinaryFormatter();
			
					int packetIndex = (int) formatter.Deserialize(stream);
					string methodName = (string) formatter.Deserialize(stream);
					MethodInfo method = GetType().GetMethod(methodName);
					Log(packetIndex.ToString() + " method call to '" + methodName + "'");

					if (method == null)
						throw new InvalidOperationException("Method '" + methodName + "' not found");

					ParameterInfo[] info = method.GetParameters();
					object[] attr = method.GetCustomAttributes(typeof(ProxyMethodAttribute), false);
					if (attr == null || attr.Length == 0)
					{
						StringBuilder s = new StringBuilder();
						s.Append(this.GetType().FullName);
						s.Append(".");
						s.Append(methodName);
						s.Append("(");
						foreach (ParameterInfo param in info)
						{
							if (s[s.Length-1] != '(')
								s.Append(", ");
							s.Append(param.ParameterType.ToString());
						}
						s.Append(")");
						throw new InvalidOperationException("Method '" + s.ToString() + "' does not have ProxyMethod attribute");
					}

					object[] parameters = new object[info.Length];
					for (int i=0; i<info.Length; i++)
						parameters[i] = formatter.Deserialize(stream);

					method.Invoke(this, parameters);
				}
				stream.Close();
			}
			catch(Exception e)
			{
				Log("Exception in InvokeProxyMember: " + e.Message);
			}
		}

		public override int GetHashCode()
		{
			return this.ID;
		}

		public override bool Equals(object o)
		{
			Channel c = o as Channel;
			if (c == null) return false;
			return c.ID == this.ID;
		}

		object logSync = new object();

		void Log(string s)
		{
//			lock (logSync)
//			{
//				StreamWriter w = new StreamWriter("channel.log", true);
//				w.WriteLine(s);
//				w.Close();
//			}
		}
	}
}
