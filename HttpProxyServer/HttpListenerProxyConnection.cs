using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace HttpProxyServer
{
	class HttpListenerProxyConnection
	{
		#region field
		private HttpWebRequest _request;
		private HttpListenerContext _context;
		#endregion

		public HttpListenerProxyConnection(HttpWebRequest request, HttpListenerContext context)
		{
			_request = request;
			_context = context;

			//Thread thread = new Thread(new ThreadStart(ManageConnection));
			//thread.Start();

			IAsyncResult result = (IAsyncResult)request.BeginGetResponse(new AsyncCallback(Run), null);
		}

		private void Run(IAsyncResult ar)
		{
			string message;
			ManageCommunication(ar, out message);
			Logger.Instance.WriteLine(message);

			try
			{
				_context.Response.OutputStream.Close();
			}
			catch (Exception) { }
		}

		private bool ManageCommunication(IAsyncResult ar, out string message)
		{
			message = "From " + _context.Request.RemoteEndPoint + " to " + _request.Address.AbsoluteUri;

			try
			{
				HttpListenerResponse responseOut = _context.Response;

				using (HttpWebResponse response = (HttpWebResponse)_request.EndGetResponse(ar))
				using (Stream receiveStream = response.GetResponseStream())
				{
					// Need to get the length of the response before it can be forwarded on
					//responseOut.ContentLength64 = response.ContentLength;

					if (!responseOut.OutputStream.CanWrite)
					{
						message += ": ERROR: !CanWrite";
						return false;
					}
					
					int bytesCopied = CopyStream(receiveStream, responseOut.OutputStream);
					//responseOut.OutputStream.Close();
					//Logger.Instance.WriteLine("Copied " + bytesCopied + " bytes");
				}
			}
			catch (WebException wex)
			{
				if (wex.Message.Contains("404"))
				{
					//%%da indagare
					message += ": ERROR: 404";
					return false;
				}

				message += ": ERROR: WebException: \r\n" + wex.ToString();
			}
			catch (Exception ex)
			{
				message += ": ERROR: DownloadData: \r\n" + ex.ToString();
				return false;
			}

			return true;
		}

		private int CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[32768];
			int bytesWritten = 0;
			while (true)
			{
				int read = input.Read(buffer, 0, buffer.Length);
				if (read <= 0)
					break;
				output.Write(buffer, 0, read);
				bytesWritten += read;
			}
			return bytesWritten;
		}
	}
}
