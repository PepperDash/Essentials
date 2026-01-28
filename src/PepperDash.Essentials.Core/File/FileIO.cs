using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core;
using Crestron.SimplSharpPro.CrestronThread;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Static class for FileIO operations
	/// </summary>
	public static class FileIO
	{

		static CCriticalSection fileLock = new CCriticalSection();
		/// <summary>
		/// Delegate for GotFileEventHandler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void GotFileEventHandler(object sender, FileEventArgs e);

		/// <summary>
		/// Event for GotFileEvent
		/// </summary>
		public static event GotFileEventHandler GotFileEvent;

		/// <summary>
		/// Get the full file info from a path/filename, can include wildcards.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
        /// <summary>
        /// GetFiles method
        /// </summary>
        public static FileInfo[] GetFiles(string fileName)
        {
            string fullFilePath = Global.FilePathPrefix + fileName;
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fullFilePath));
            var files = dirInfo.GetFiles(Path.GetFileName(fullFilePath));
            Debug.LogMessage(LogEventLevel.Information, "FileIO found: {0}, {1}", files.Count(), fullFilePath);
            if (files.Count() > 0)
            {
                return files;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetFile method
        /// </summary>
        public static FileInfo GetFile(string fileName)
        {
            string fullFilePath = Global.FilePathPrefix + fileName;
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(fullFilePath));
            var files = dirInfo.GetFiles(Path.GetFileName(fullFilePath));
            Debug.LogMessage(LogEventLevel.Information, "FileIO found: {0}, {1}", files.Count(), fullFilePath);
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
  /// <summary>
  /// ReadDataFromFile method
  /// </summary>
		public static string ReadDataFromFile(string fileName)
		{
			try
			{
				return ReadDataFromFile(GetFile(fileName));
			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Error: FileIO read failed: \r{0}", e);
				return "";
			}
		}

        /// <summary>
        /// Get the data with fileInfo object 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <summary>
        /// ReadDataFromFile method
        /// </summary>
        public static string ReadDataFromFile(FileInfo file)
		{
			try
			{
				if (fileLock.TryEnter())
				{
					DirectoryInfo dirInfo = new DirectoryInfo(file.DirectoryName);
					Debug.LogMessage(LogEventLevel.Verbose, "FileIO Getting Data {0}", file.FullName);

					if (File.Exists(file.FullName))
					{
						using (StreamReader r = new StreamReader(file.FullName))
						{
							return r.ReadToEnd();
						}
					}
					else
					{
						Debug.LogMessage(LogEventLevel.Verbose, "File {0} does not exsist", file.FullName);
						return "";
					}
				}
				else
				{
					Debug.LogMessage(LogEventLevel.Information, "FileIO Unable to enter FileLock");
					return "";
				}
				
			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Error: FileIO read failed: \r{0}", e);
				return "";
			}
			finally
			{
				if (fileLock != null && !fileLock.Disposed)
					fileLock.Leave();

			}
		}


  /// <summary>
  /// ReadDataFromFileASync method
  /// </summary>
		public static void ReadDataFromFileASync(string fileName)
		{
			try
			{
				ReadDataFromFileASync(GetFile(fileName));
			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Error: FileIO read failed: \r{0}", e);
			}
		}

  /// <summary>
  /// ReadDataFromFileASync method
  /// </summary>
		public static void ReadDataFromFileASync(FileInfo file)
		{
			try
			{
				CrestronInvoke.BeginInvoke(o => _ReadDataFromFileASync(file));
			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Error: FileIO read failed: \r{0}", e);
			}
		}

		private static void _ReadDataFromFileASync(FileInfo file)
		{
			string data;
			try
			{
				if (fileLock.TryEnter())
				{
					DirectoryInfo dirInfo = new DirectoryInfo(file.Name);
					Debug.LogMessage(LogEventLevel.Verbose, "FileIO Getting Data {0}", file.FullName);


					if (File.Exists(file.FullName))
					{
						using (StreamReader r = new StreamReader(file.FullName))
						{
							data = r.ReadToEnd();
						}
					}
					else
					{
						Debug.LogMessage(LogEventLevel.Verbose, "File {0} Does not exsist", file.FullName);
						data = "";
					}
					GotFileEvent.Invoke(null, new FileEventArgs(data));
				}
				else
				{
					Debug.LogMessage(LogEventLevel.Information, "FileIO Unable to enter FileLock");
				}

			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Error: FileIO read failed: \r{0}", e);
				data = "";
			}
			finally
			{
				if (fileLock != null && !fileLock.Disposed)
					fileLock.Leave();

			}



		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>		
		public static void WriteDataToFile(string data, string filePath)
		{
			Thread _WriteFileThread;
            _WriteFileThread = new Thread((O) => _WriteFileMethod(data, Global.FilePathPrefix + "/" + filePath), null, Thread.eThreadStartOptions.CreateSuspended);
			_WriteFileThread.Priority = Thread.eThreadPriority.LowestPriority;
			_WriteFileThread.Start();
			Debug.LogMessage(LogEventLevel.Information, "New WriteFile Thread");

		}

		static object _WriteFileMethod(string data, string filePath)
		{
			Debug.LogMessage(LogEventLevel.Information, "Attempting to write file: '{0}'", filePath);

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
					Debug.LogMessage(LogEventLevel.Information, "FileIO Unable to enter FileLock");
				}

			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Error, "Error: FileIO write failed: \r{0}", e);
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
  /// <summary>
  /// FileIoUnitTest method
  /// </summary>
		public static bool FileIoUnitTest()
		{
			var testData = "Testing FileIO";
			FileIO.WriteDataToFile(testData, "\\user\\FileIOTest.pdt");

			var file = FileIO.GetFile("\\user\\*FileIOTest*");
			
			var readData = FileIO.ReadDataFromFile(file);
			Debug.LogMessage(LogEventLevel.Information, "Returned {0}", readData);
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
 /// <summary>
 /// Represents a FileEventArgs
 /// </summary>
	public class FileEventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data"></param>
		public FileEventArgs(string data) { Data = data; }
  /// <summary>
  /// Gets or sets the Data
  /// </summary>
		public string Data { get; private set; } // readonly

	}
}