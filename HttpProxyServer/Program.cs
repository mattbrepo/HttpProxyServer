using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace HttpProxyServer
{
	class Program
	{
		private const int LOCAL_PORT = 8080;
		private const string PROXY_SERVER = @"http://10.1.1.50:8080/";
		private const string USER = "myuser";
		private const string DOMAIN = "mygroup";
		private const string PWD = "mypwd";

		static void Main(string[] args)
		{
			//--------------------------------------------- init file di log
			Logger.CreateInstance(@"c:\temp\HttpProxyServer.log", false);
			
			//--------------------------------------------- init web proxy "reale"
			WebProxy webProxy = new WebProxy();
			Uri uri = new Uri(PROXY_SERVER);
			NetworkCredential cred = new NetworkCredential();

			cred.Domain = DOMAIN;
			cred.UserName = USER;
			cred.Password = PWD;

			webProxy.Credentials = cred;
			webProxy.Address = uri;

			//--------------------------------------------- start proxy server
			if (false)
			{
				//non utilizzando HttpListener sembra molto più veloce...
				HttpProxyServer.Run(LOCAL_PORT, webProxy);
			}
			else
			{
				//...ma HttpListenerProxy non gestisce solo le GET!
				HttpListenerProxy.Run(LOCAL_PORT, webProxy);
			}
		} 
		
	}
}
