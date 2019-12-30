using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HttpProxyServer
{
	class HttpListenerProxy
	{
		public static void Run(int localPort, WebProxy webProxy)
		{
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add("http://*:" + localPort + "/");
			listener.Start();
			Logger.Instance.WriteLine("Listening on port " + localPort);
			try
			{
				while (true)
				{
					HttpListenerContext context = listener.GetContext();
					string requestString = context.Request.RawUrl;

					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
					request.KeepAlive = context.Request.KeepAlive;
					request.Proxy = webProxy;
					request.Timeout = 200000;

					HttpListenerProxyConnection connection = new HttpListenerProxyConnection(request, context);
				}
			}
			catch (WebException wex)
			{
				Logger.Instance.WriteLine("\nMain WebException raised: \r\n" + wex);
			}
			catch (Exception ex)
			{
				Logger.Instance.WriteLine("\nMain Exception raised: \r\n" + ex);
			}

			listener.Stop();
		}
	}
}
