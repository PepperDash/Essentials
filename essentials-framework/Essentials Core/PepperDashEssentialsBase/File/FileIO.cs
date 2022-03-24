using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core;
using Crestron.SimplSharpPro.CrestronThread;

namespace PepperDash.Essentials.Core
{
	public static class FileIO
	{

		static CCriticalSection fileLock = new CCriticalSection();
		public delegate void GotFileEventHandler(object sender, FileEventArgs e);
		public static event GotFileEventHandler GotFileEvent;

		/// <summary>
		/// Get the full file info from a path/filename, can include wildcards.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo[] GetFiles(string fileName)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));
			var files = dirInfo.GetFiles(Path.GetFileName(fileName));
			Debug.Console(0, "FileIO found: {0}, {1}", files.Count(), fileName);
			if (files.Count() > 0)
			{
				return files;
			}
			else
			{
				return null;
			}
		}

		public static FileInfo GetFile(string fileName)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));
			var files = dirInfo.GetFiles(Path.GetFileName(fileName));
			Debug.Console(0, "FileIO found: {0}, {1}", files.Count(), fileName);
			if (files.Count() > 0)
			{
				return files.FirstOrDefault();
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Get the data from string path/filename
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string ReadDataFromFile(string fileName)
		{
			try
			{
				return ReadDataFromFile(GetFile(fileName));
			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO read failed: \r{0}", e);
				return "";
			}
		}

		/// <summary>
		/// Get the data with fileInfo object 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string ReadDataFromFile(FileInfo file)
		{
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo(file.Name);
				Debug.Console(2, "FileIO Getting Data {0}", file.FullName);

				if (File.Exists(file.FullName))
				{
					using (StreamReader r = new StreamReader(file.FullName))
					{
						return r.ReadToEnd();
					}
				}
				else
				{
					Debug.Console(2, "File {0} does not exsist", file.FullName);
					return "";
				}
				
			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO read failed: \r{0}", e);
				return "";
			}
		}


		public static void ReadDataFromFileASync(string fileName)
		{
			try
			{
				ReadDataFromFileASync(GetFile(fileName));
			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO read failed: \r{0}", e);
			}
		}

		public static void ReadDataFromFileASync(FileInfo file)
		{
			try
			{
				CrestronInvoke.BeginInvoke(o => _ReadDataFromFileASync(file));
			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO read failed: \r{0}", e);
			}
		}

		private static void _ReadDataFromFileASync(FileInfo file)
		{
			string data;
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo(file.Name);
				Debug.Console(2, "FileIO Getting Data {0}", file.FullName);


				if (File.Exists(file.FullName))
				{
					using (StreamReader r = new StreamReader(file.FullName))
					{
						data = r.ReadToEnd();
					}
				}
				else
				{
					Debug.Console(2, "File {0} Does not exsist", file.FullName);
					data = "";
				}
				GotFileEvent.Invoke(null, new FileEventArgs(data));

			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO read failed: \r{0}", e);
				data = "";
			}



		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		public static void WriteDataToFile(string data, string filePath)
		{
			Thread _WriteFileThread;
			_WriteFileThread = new Thread((O) => _WriteFileMethod(data, filePath), null, Thread.eThreadStartOptions.CreateSuspended);
			_WriteFileThread.Priority = Thread.eThreadPriority.LowestPriority;
			_WriteFileThread.Start();
			Debug.Console(0, Debug.ErrorLogLevel.Notice, "New WriteFile Thread");

		}

		static object _WriteFileMethod(string data, string filePath)
		{
			Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to write file: '{0}'", filePath);

			try
			{
				if (fileLock.TryEnter())
				{
					using (StreamWriter sw = new StreamWriter(filePath))
					{
						sw.Write(data);
						sw.Flush();
					}

				}
				else
				{
					Debug.Console(0, Debug.ErrorLogLevel.Error, "FileIO Unable to enter FileLock");
				}

			}
			catch (Exception e)
			{
				Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: FileIO write failed: \r{0}", e);
			}
			finally
			{
				if (fileLock != null && !fileLock.Disposed)
					fileLock.Leave();

			}
			return null;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static bool FileIoUnitTest()
		{
			var testData = "Testing FileIO";
			FileIO.WriteDataToFile(testData, "\\user\\FileIOTest.pdt");

			var file = FileIO.GetFile("\\user\\*FileIOTest*");
			
			var readData = FileIO.ReadDataFromFile(file);
			Debug.Console(0, "Returned {0}", readData);
			File.Delete(file.FullName);
			if (testData == readData)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	}
	public class FileEventArgs
	{
		public FileEventArgs(string data) { Data = data; }
		public string Data { get; private set; } // readonly

	}
}