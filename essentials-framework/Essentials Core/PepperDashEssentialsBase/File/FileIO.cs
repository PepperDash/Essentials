using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public static class FileIO
	{

		static CCriticalSection fileLock = new CCriticalSection();

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		public static void WriteDataToFile(string data, string filePath)
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
}