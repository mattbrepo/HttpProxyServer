using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace HttpProxyServer
{
	/// <summary>
	/// Scrive un file in maniera asincrona
	/// </summary>
	class AsyncFileWriter
	{
		private string _fileDir;
		private string _fileStartName;
		private string _fileExt;
		private string _content;

		public AsyncFileWriter(string fileDir, string fileStartName, string fileExt, string content)
		{
			_fileDir = fileDir;
			_fileStartName = fileStartName;
			_fileExt = fileExt;
			_content = content;

			Thread thread = new Thread(new ThreadStart(Write));
			thread.Start();
		}

		private void Write()
		{
			string fileName;
			string filePath;
			int i = 0;
			
			//-------------------------------- identificazione nome utilizzabile
			StreamWriter sw;
			do
			{
				fileName = _fileStartName + "_" + DateToString() + (i == 0 ? "" : "_" + i) + _fileExt;
				filePath = Path.Combine(_fileDir, fileName);
				i++;
			} while (!TryOpen(filePath, out sw));

			//-------------------------------- scrittura file
			try {
				sw.Write(_content);
			} catch (Exception) {
				return;
			} finally {
				sw.Close();
			}
			//File.WriteAllText(filePath, _content);
		}

		private bool TryOpen(string filePath, out StreamWriter sw)
		{
			try {
				FileStream fs = File.Open(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				sw = new StreamWriter(fs);
			} catch (Exception) {
				sw = null;
				return false;
			}
			return true;
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
	}
}
