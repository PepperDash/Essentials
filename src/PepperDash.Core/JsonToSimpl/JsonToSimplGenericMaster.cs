using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;

namespace PepperDash.Core.JsonToSimpl
{
    /// <summary>
    /// Generic Master
    /// </summary>
	public class JsonToSimplGenericMaster : JsonToSimplMaster
    {
		/*****************************************************************************************/
		/** Privates **/

        
		// The JSON file in JObject form
		// For gathering the incoming data
		object StringBuilderLock = new object();
		// To prevent multiple same-file access
		static object WriteLock = new object();

        /// <summary>
        /// Callback action for saving
        /// </summary>
		public Action<string> SaveCallback { get; set; }

		/*****************************************************************************************/

		/// <summary>
        /// SIMPL+ default constructor.
        /// </summary>
		public JsonToSimplGenericMaster()
        {
		}

		/// <summary>
		/// Loads in JSON and triggers evaluation on all children
		/// </summary>
		/// <param name="json"></param>
		public void LoadWithJson(string json)
		{
			OnBoolChange(false, 0, JsonToSimplConstants.JsonIsValidBoolChange);
			try
			{
				JsonObject = JObject.Parse(json);
				foreach (var child in Children)
					child.ProcessAll();
				OnBoolChange(true, 0, JsonToSimplConstants.JsonIsValidBoolChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("JSON parsing failed:\r{0}", e);
				CrestronConsole.PrintLine(msg);
				ErrorLog.Error(msg);
			}
		}

		/// <summary>
		/// Loads JSON into JsonObject, but does not trigger evaluation by children
		/// </summary>
		/// <param name="json"></param>
		public void SetJsonWithoutEvaluating(string json)
		{
			try
			{
				JsonObject = JObject.Parse(json);
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "JSON parsing failed:\r{0}", e);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Save()
		{
			// this code is duplicated in the other masters!!!!!!!!!!!!!
 			UnsavedValues = new Dictionary<string, JValue>();
			// Make each child update their values into master object
			foreach (var child in Children)
			{
				Debug.Console(1, this, "Master. checking child [{0}] for updates to save",  child.Key);
				child.UpdateInputsForMaster();
			}

			if (UnsavedValues == null || UnsavedValues.Count == 0)
			{
				Debug.Console(1, this, "Master. No updated values to save. Skipping");
				return;
			}

			lock (WriteLock)
			{
				Debug.Console(1, this, "Saving");
				foreach (var path in UnsavedValues.Keys)
				{
					var tokenToReplace = JsonObject.SelectToken(path);
					if (tokenToReplace != null)
					{// It's found
						tokenToReplace.Replace(UnsavedValues[path]);
						Debug.Console(1, this, "Master Updating '{0}'", path);
					}
					else // No token.  Let's make one 
					{
						Debug.Console(1, "Master Cannot write value onto missing property: '{0}'", path);
					}
				}
			}
			if (SaveCallback != null)
				SaveCallback(JsonObject.ToString());
			else
				Debug.Console(0, this, "WARNING: No save callback defined.");
		}
	}
}