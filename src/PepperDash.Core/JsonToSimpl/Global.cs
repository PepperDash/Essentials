using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

//using PepperDash.Core;

namespace PepperDash.Core.JsonToSimpl
{
    /// <summary>
    /// The global class to manage all the instances of JsonToSimplMaster 
    /// </summary>
	public class J2SGlobal
	{
		static List<JsonToSimplMaster> Masters = new List<JsonToSimplMaster>();


		/// <summary>
		/// Adds a file master.  If the master's key or filename is equivalent to any existing 
		/// master, this will fail
		/// </summary>
		/// <param name="master">New master to add</param>
        /// 
		public static void AddMaster(JsonToSimplMaster master)
		{
			if (master == null)
				throw new ArgumentNullException("master");

			if (string.IsNullOrEmpty(master.UniqueID))
				throw new InvalidOperationException("JSON Master cannot be added with a null UniqueId");
			
			Debug.Console(1, "JSON Global adding master {0}", master.UniqueID);

			if (Masters.Contains(master)) return;

			var existing = Masters.FirstOrDefault(m =>
				m.UniqueID.Equals(master.UniqueID, StringComparison.OrdinalIgnoreCase));
			if (existing == null)
			{
				Masters.Add(master);
			}
			else
			{
				var msg = string.Format("Cannot add JSON Master with unique ID '{0}'.\rID is already in use on another master.", master.UniqueID);
				CrestronConsole.PrintLine(msg);
				ErrorLog.Warn(msg);
			}
		}

		/// <summary>
		/// Gets a master by its key.  Case-insensitive
		/// </summary>
		public static JsonToSimplMaster GetMasterByFile(string file)
		{
			return Masters.FirstOrDefault(m => m.UniqueID.Equals(file, StringComparison.OrdinalIgnoreCase));
		}
	}
}