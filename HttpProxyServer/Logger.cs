//#define MY_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HttpProxyServer
{
	class Logger
	{
		#region singleton
		public static Logger Instance { get; private set; }

		/// <summary>
		/// Costruzione dell'oggetto di gestione del log
		/// </summary>
		/// <param name="eventLog"></param>
		/// <param name="logFile"></param>
		public static void CreateInstance(string logFile, bool append)
		{
			Instance = new Logger(logFile, append);
		}
		#endregion

		#region field
		private string _logFile;

		private object _lockErr;
		private object _lockDump;
		#endregion

		#region ctor
		private Logger(string logFile, bool append)
		{
			if (!append) {
				if (File.Exists(logFile)) File.Delete(logFile);
			}
			_logFile = logFile;
			_lockErr = new object();
			_lockDump = new object();
		}
		#endregion

		#region func
		/// <summary>
		/// Scrittura messaggio (sulla Console e file di log)
		/// </summary>
		/// <param name="message"></param>
		public void WriteLine(string message)
		{
			lock (_lockErr) {
				WriteMessageOnFile(_logFile, message);
				Console.WriteLine(message);
			}
		}

		/// <summary>
		/// Scrittura messaggio (sulla Console e file di log)
		/// </summary>
		/// <param name="message"></param>
		public void WriteDumpFile(string fileDir, string fileStartName, string message)
		{
#if MY_DEBUG
			lock (_lockDump) {
				new AsyncFileWriter(fileDir, fileStartName, ".txt", message);
			}
#endif
		}

		/// <summary>
		/// Scrittura generico messaggio su file
		/// </summary>
		/// <param name="path"></param>
		/// <param name="message"></param>
		private void WriteMessageOnFile(string path, string message)
		{
			try {
				message = "[" + DateToStringForLog() + "]: " + message;
				List<string> lines = new List<string>();
				lines.Add(message);
#if MY_DEBUG
				File.AppendAllLines(path, lines);
#endif
			}
			catch {
			}
		}

		private string DateToStringForLog()
		{
			try {
				return DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
			}
			catch (Exception) {
				//utile?
				return "";
			}
		}

		#endregion
	}
}
