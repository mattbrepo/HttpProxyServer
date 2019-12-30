using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace HttpProxyServer
{
	class HttpProxyServer
	{
		public static void Run(int localPort, WebProxy webProxy)
		{
			//--------------------------------------------- start tcp listener
			TcpListener tcplistener = new TcpListener(IPAddress.Any, localPort);
			Logger.Instance.WriteLine("Listening on port " + localPort);
			tcplistener.Start();

			//--------------------------------------------- main loop
			while (true)
			{
				Socket socket = tcplistener.AcceptSocket();
				HttpProxyServerConnection bridgeProxy = new HttpProxyServerConnection(socket, webProxy);
				Thread thread = new Thread(new ThreadStart(bridgeProxy.Run));
				thread.Start();
			}
		}
	}
}
