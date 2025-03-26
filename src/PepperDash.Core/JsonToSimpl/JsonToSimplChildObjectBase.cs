using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PepperDash.Core.JsonToSimpl
{
    /// <summary>
    /// Base class for JSON objects
    /// </summary>
	public abstract class JsonToSimplChildObjectBase : IKeyed
	{
        /// <summary>
        /// Notifies of bool change
        /// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
        /// <summary>
        /// Notifies of ushort change
        /// </summary>
		public event EventHandler<UshrtChangeEventArgs> UShortChange;
        /// <summary>
        /// Notifies of string change
        /// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;

        /// <summary>
        /// Delegate to get all values
        /// </summary>
		public SPlusValuesDelegate GetAllValuesDelegate { get; set; }

		/// <summary>
		/// Use a callback to reduce task switch/threading
		/// </summary>
		public SPlusValuesDelegate SetAllPathsDelegate { get; set; }

        /// <summary>
        /// Unique identifier for instance
        /// </summary>
		public string Key { get; protected set; }

		/// <summary>
		/// This will be prepended to all paths to allow path swapping or for more organized
		/// sub-paths
		/// </summary>
		public string PathPrefix { get; protected set; }

		/// <summary>
		/// This is added to the end of all paths
		/// </summary>
		public string PathSuffix { get; protected set; }

        /// <summary>
        /// Indicates if the instance is linked to an object
        /// </summary>
		public bool LinkedToObject { get; protected set; }

        /// <summary>
        /// Reference to Master instance
        /// </summary>
		protected JsonToSimplMaster Master;

        /// <summary>
        /// Paths to boolean values in JSON structure
        /// </summary>
        protected Dictionary<ushort, string> BoolPaths = new Dictionary<ushort, string>();
        /// <summary>
        /// Paths to numeric values in JSON structure
        /// </summary>
		protected Dictionary<ushort, string> UshortPaths = new Dictionary<ushort, string>();
        /// <summary>
        /// Paths to string values in JSON structure
        /// </summary>
		protected Dictionary<ushort, string> StringPaths = new Dictionary<ushort, string>();

		/// <summary>
		/// Call this before doing anything else
		/// </summary>
        /// <param name="masterUniqueId"></param>
		/// <param name="key"></param>
		/// <param name="pathPrefix"></param>
		/// <param name="pathSuffix"></param>
		public void Initialize(string masterUniqueId, string key, string pathPrefix, string pathSuffix)
		{
			Key = key;
			PathPrefix = pathPrefix;
			PathSuffix = pathSuffix;

			Master = J2SGlobal.GetMasterByFile(masterUniqueId);
			if (Master != null)
				Master.AddChild(this);
			else
				Debug.Console(1, "JSON Child [{0}] cannot link to master {1}", key, masterUniqueId);
		}

        /// <summary>
        /// Sets the path prefix for the object
        /// </summary>
        /// <param name="pathPrefix"></param>
		public void SetPathPrefix(string pathPrefix)
		{
			PathPrefix = pathPrefix;
		}
		/// <summary>
		/// Set the JPath to evaluate for a given bool out index.
		/// </summary>
		public void SetBoolPath(ushort index, string path)
		{
			Debug.Console(1, "JSON Child[{0}] SetBoolPath {1}={2}", Key, index, path);
			if (path == null || path.Trim() == string.Empty) return;
			BoolPaths[index] = path;
		}

		/// <summary>
		/// Set the JPath for a ushort out index.
		/// </summary>
		public void SetUshortPath(ushort index, string path)
		{
			Debug.Console(1, "JSON Child[{0}] SetUshortPath {1}={2}", Key, index, path);
			if (path == null || path.Trim() == string.Empty) return;
			UshortPaths[index] = path;
		}

		/// <summary>
		/// Set the JPath for a string output index. 
		/// </summary>
		public void SetStringPath(ushort index, string path)
		{
			Debug.Console(1, "JSON Child[{0}] SetStringPath {1}={2}", Key, index, path);
			if (path == null || path.Trim() == string.Empty) return;
			StringPaths[index] = path;
		}

		/// <summary>
		/// Evalutates all outputs with defined paths. called by S+ when paths are ready to process
		/// and by Master when file is read.
		/// </summary>
		public virtual void ProcessAll()
		{
			if (!LinkedToObject)
			{
				Debug.Console(1, this, "Not linked to object in file.  Skipping");
				return;
			}

			if (SetAllPathsDelegate == null)
			{
				Debug.Console(1, this, "No SetAllPathsDelegate set. Ignoring ProcessAll");
				return;
			}
			SetAllPathsDelegate();
			foreach (var kvp in BoolPaths)
				ProcessBoolPath(kvp.Key);
			foreach (var kvp in UshortPaths)
				ProcessUshortPath(kvp.Key);
			foreach (var kvp in StringPaths)
				ProcessStringPath(kvp.Key);
		}

		/// <summary>
		/// Processes a bool property, converting to bool, firing off a BoolChange event
		/// </summary>
		void ProcessBoolPath(ushort index)
		{
			string response;
			if (Process(BoolPaths[index], out response))
				OnBoolChange(response.Equals("true", StringComparison.OrdinalIgnoreCase),
					index, JsonToSimplConstants.BoolValueChange);
			else { }
			// OnBoolChange(false, index, JsonToSimplConstants.BoolValueChange);
		}

		// Processes the path to a ushort, converting to ushort if able, twos complement if necessary, firing off UshrtChange event
        void ProcessUshortPath(ushort index) {
            string response;
            if (Process(UshortPaths[index], out response)) {
                ushort val;
                try { val = Convert.ToInt32(response) < 0 ? (ushort)(Convert.ToInt16(response) + 65536) : Convert.ToUInt16(response); }
                catch { val = 0; }

                OnUShortChange(val, index, JsonToSimplConstants.UshortValueChange);
            }
            else { }
            // OnUShortChange(0, index, JsonToSimplConstants.UshortValueChange);
        }

		// Processes the path to a string property and fires of a StringChange event.
		void ProcessStringPath(ushort index)
		{
			string response;
			if (Process(StringPaths[index], out response))
				OnStringChange(response, index, JsonToSimplConstants.StringValueChange);
			else { }
			// OnStringChange("", index, JsonToSimplConstants.StringValueChange);
		}

		/// <summary>
		/// Processes the given path. 
		/// </summary>
		/// <param name="path">JPath formatted path to the desired property</param>
		/// <param name="response">The string value of the property, or a default value if it 
		/// doesn't exist</param>
		/// <returns> This will return false in the case that EvaulateAllOnJsonChange
		/// is false and the path does not evaluate to a property in the incoming JSON. </returns>
		bool Process(string path, out string response)
		{
			path = GetFullPath(path);
			Debug.Console(1, "JSON Child[{0}] Processing {1}", Key, path);
			response = "";
			if (Master == null)
			{
				Debug.Console(1, "JSONChild[{0}] cannot process without Master attached", Key);
				return false;
			}

			if (Master.JsonObject != null && path != string.Empty)
			{
				bool isCount = false;
				path = path.Trim();
				if (path.EndsWith(".Count"))
				{
					path = path.Remove(path.Length - 6, 6);
					isCount = true;
				}
				try // Catch a strange cast error on a bad path
				{
					var t = Master.JsonObject.SelectToken(path);
					if (t != null)
					{
						// return the count of children objects - if any
						if (isCount)
							response = (t.HasValues ? t.Children().Count() : 0).ToString();
						else
							response = t.Value<string>();
						Debug.Console(1, "   ='{0}'", response);
						return true;
					}
				}
				catch
				{
					response = "";
				}
			}
			// If the path isn't found, return this to determine whether to pass out the non-value or not.
			return false;
		}


		//************************************************************************************************
		// Save-related functions


		/// <summary>
		/// Called from Master to read inputs and update their values in master JObject
		/// Callback should hit one of the following four methods
		/// </summary>
		public void UpdateInputsForMaster()
		{
			if (!LinkedToObject)
			{
				Debug.Console(1, this, "Not linked to object in file.  Skipping");
				return;
			}

			if (SetAllPathsDelegate == null)
			{
				Debug.Console(1, this, "No SetAllPathsDelegate set. Ignoring UpdateInputsForMaster");
				return;
			}
			SetAllPathsDelegate();
			var del = GetAllValuesDelegate;
			if (del != null)
				GetAllValuesDelegate();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="theValue"></param>
		public void USetBoolValue(ushort key, ushort theValue)
		{
			SetBoolValue(key, theValue == 1);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="theValue"></param>
		public void SetBoolValue(ushort key, bool theValue)
		{
			if (BoolPaths.ContainsKey(key))
				SetValueOnMaster(BoolPaths[key], new JValue(theValue));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="theValue"></param>
		public void SetUShortValue(ushort key, ushort theValue)
		{
			if (UshortPaths.ContainsKey(key))
				SetValueOnMaster(UshortPaths[key], new JValue(theValue));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="theValue"></param>
		public void SetStringValue(ushort key, string theValue)
		{
			if (StringPaths.ContainsKey(key))
				SetValueOnMaster(StringPaths[key], new JValue(theValue));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="valueToSave"></param>
		public void SetValueOnMaster(string keyPath, JValue valueToSave)
		{
			var path = GetFullPath(keyPath);
			try
			{
				Debug.Console(1, "JSON Child[{0}] Queueing value on master {1}='{2}'", Key, path, valueToSave);

				//var token = Master.JsonObject.SelectToken(path);
				//if (token != null) // The path exists in the file
				Master.AddUnsavedValue(path, valueToSave);
			}
			catch (Exception e)
			{
				Debug.Console(1, "JSON Child[{0}] Failed setting value for path '{1}'\r{2}", Key, path, e);
			}
		}

		/// <summary>
		/// Called during Process(...) to get the path to a given property. By default, 
		/// returns PathPrefix+path+PathSuffix.  Override to change the way path is built.
		/// </summary>
		protected virtual string GetFullPath(string path)
		{
			return (PathPrefix != null ? PathPrefix : "") +
				path + (PathSuffix != null ? PathSuffix : "");
		}

		// Helpers for events
		//******************************************************************************************
        /// <summary>
        /// Event helper
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			var handler = BoolChange;
			if (handler != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		//******************************************************************************************
        /// <summary>
        /// Event helper
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
		protected void OnUShortChange(ushort state, ushort index, ushort type)
		{
			var handler = UShortChange;
			if (handler != null)
			{
				var args = new UshrtChangeEventArgs(state, type);
				args.Index = index;
				UShortChange(this, args);
			}
		}

        /// <summary>
        /// Event helper
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			var handler = StringChange;
			if (handler != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}
	}
}