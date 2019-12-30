using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace HttpProxyServer
{
	class HttpProxyServerConnection
	{
		#region const
		private const string HTTP_VERSION = "HTTP/1.0";
		private const string CRLF = "\r\n";
		#endregion

		#region field
		private byte[] _read;
		private Socket _clientSocket;
		private WebClient _proxyWebClient;
		#endregion

		#region ctor
		public HttpProxyServerConnection(Socket socket, WebProxy webProxy)
		{
			_read = new byte[1024];
			this._clientSocket = socket;
			this._proxyWebClient = new WebClient();
			this._proxyWebClient.Proxy = webProxy;
		}
		#endregion

		#region func

		public void Run()
		{
			string message;
			ManageCommunication(out message);
			Logger.Instance.WriteLine(message);
			
			try {
				_clientSocket.Close();
			}
			catch (Exception) { }
		}

		private bool ManageCommunication(out string message)
		{
			message = "From " + _clientSocket.RemoteEndPoint;
			
			//-------------------------------- lettura messaggio del client (Web Request)
			string clientMessage = "";
			try {
				int bytes = ReadMessage(_read, _clientSocket, ref clientMessage);
				if (bytes == 0) {
					message += ": ERROR: ReadMessage: No bytes to read!";
					return false;
				}
			} catch (Exception ex) {
				message += ": ERROR: ReadMessage: \r\n" + ex.ToString();
				return false;
			}

			//-------------------------------- parsing della Web Request
			string sAddress = "";
			try 
			{
				int index1 = clientMessage.IndexOf(' ');
				int index2 = clientMessage.IndexOf(' ', index1 + 1);
				if ((index1 == -1) || (index2 == -1)) {
					message += ": ERROR: Parsing Message: clientMessage format error!";
					return false;
				}

				//per ora ignora le post
				bool bGet = clientMessage.Substring(0, index1) == "GET";
				if (!bGet) {
					//si gestiscono solo GET
					message += ": ERROR: Parsing Message: clientMessage is not a GET";
					return false;
				}

				sAddress = clientMessage.Substring(index1 + 1, index2 - index1 - 1);
			} catch (Exception ex) {
				message += ": ERROR: Parsing Address: \r\n" + ex.ToString();
				return false;
			}

			//-------------------------------- richiesta corretta
			message += " to " + sAddress;

			//-------------------------------- lettura dei dati dal proxy server reale
			byte[] data = null;
			try {
				data = _proxyWebClient.DownloadData(sAddress);
			} catch (System.Net.WebException wex) {

				if (wex.Message.Contains("404")) {
					//%%da indagare
					message += ": ERROR: 404";
					return false;
				}
				
				//bool b = wex.Response is HttpWebResponse;
				message += ": ERROR: WebException: \r\n" + wex.ToString();

				if (wex.Response != null) clientMessage += "\r\n--------------------------------- response header:\r\n\r\n" + wex.Response.Headers.ToString();
				Logger.Instance.WriteDumpFile(@"c:\temp\", "clientMessage_err", clientMessage); //vedi MY_DEBUG
				return false;
			} catch (Exception ex) {
				message += ": ERROR: DownloadData: \r\n" + ex.ToString();
				Logger.Instance.WriteDumpFile(@"c:\temp\", "clientMessage_err", clientMessage);  //vedi MY_DEBUG
				return false;
			}

			Logger.Instance.WriteDumpFile(@"c:\temp\", "clientMessage_ok", clientMessage);  //vedi MY_DEBUG

			//-------------------------------- risposta al client
			int resSend = -1;
			try {
				resSend = _clientSocket.Send(data);
			}
			catch (Exception ex) {
				message += ": ERROR: Send: \r\n" + ex.ToString();
				return false;
			}

			if (resSend < data.Length) {
				message += ": ERROR: resSend < data.Length";
				return false;
			}

			//-------------------------------- tutto ok
			message += ": OK";
			return true;
		}

		/// <summary>
		/// Legge i primi 1024 byte e li converte in ASCII
		/// </summary>
		/// <param name="b"></param>
		/// <param name="s"></param>
		/// <param name="clientmessage"></param>
		/// <returns></returns>
		private int ReadMessage(byte[] b, Socket s, ref string clientmessage)
		{
			
			int bytes = s.Receive(b, 1024, 0);
			if (bytes == 0) return 0;

			string messagefromclient = Encoding.ASCII.GetString(b, 0, bytes);
			clientmessage = (string)messagefromclient;
			return bytes;
		}

		private string DateToString()
		{
			try {
				return DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss");
			}
			catch (Exception) {
				//utile?
				return "";
			}
		}

		#endregion
	}
}
