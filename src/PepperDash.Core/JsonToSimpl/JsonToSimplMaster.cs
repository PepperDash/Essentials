using System;
using System.Collections.Generic;
using System.IO;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Core.JsonToSimpl
{
    /// <summary>
    /// Abstract base class for JsonToSimpl interactions
    /// </summary>
	public abstract class JsonToSimplMaster : IKeyed
	{
        /// <summary>
        /// Notifies of bool change
        /// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
        /// <summary>
        /// Notifies of ushort change
        /// </summary>
		public event EventHandler<UshrtChangeEventArgs> UshrtChange;
        /// <summary>
        /// Notifies of string change
        /// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;

        /// <summary>
        /// A collection of associated child modules
        /// </summary>
		protected List<JsonToSimplChildObjectBase> Children = new List<JsonToSimplChildObjectBase>();

		/*****************************************************************************************/

		/// <summary>
		/// Mirrors the Unique ID for now.
		/// </summary>
		public string Key { get { return UniqueID; } }

        /// <summary>
        /// A unique ID
        /// </summary>
		public string UniqueID { get; protected set; }

		/// <summary>
		/// Merely for use in debug messages
		/// </summary>
		public string DebugName
		{
			get { return _DebugName; }
			set { if (DebugName == null) _DebugName = ""; else _DebugName = value; }
		}
		string _DebugName = "";

		/// <summary>
		/// This will be prepended to all paths to allow path swapping or for more organized
		/// sub-paths
		/// </summary>
		public string PathPrefix { get; set; }

		/// <summary>
		/// This is added to the end of all paths
		/// </summary>
		public string PathSuffix { get; set; }

		/// <summary>
		/// Enables debugging output to the console.  Certain error messages will be logged to the 
		/// system's error log regardless of this setting
		/// </summary>
		public bool DebugOn { get; set; }

		/// <summary>
		/// Ushort helper for Debug property
		/// </summary>
		public ushort UDebug
		{
			get { return (ushort)(DebugOn ? 1 : 0); }
			set
			{
				DebugOn = (value == 1);
				CrestronConsole.PrintLine("JsonToSimpl debug={0}", DebugOn);
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public JObject JsonObject { get; protected set; }

		/*****************************************************************************************/
		/** Privates **/


		// The JSON file in JObject form
		// For gathering the incoming data
		protected Dictionary<string, JValue> UnsavedValues = new Dictionary<string, JValue>();

		/*****************************************************************************************/

		/// <summary>
		/// SIMPL+ default constructor.
		/// </summary>
		public JsonToSimplMaster()
		{
		}


		/// <summary>
		/// Sets up class - overriding methods should always call this.
		/// </summary>
		/// <param name="uniqueId"></param>
		public virtual void Initialize(string uniqueId)
		{
			UniqueID = uniqueId;
			J2SGlobal.AddMaster(this); // Should not re-add
		}

		/// <summary>
		/// Adds a child "module" to this master
		/// </summary>
		/// <param name="child"></param>
		public void AddChild(JsonToSimplChildObjectBase child)
		{
			if (!Children.Contains(child))
			{
				Children.Add(child);
			}
		}

		/// <summary>
		/// Called from the child to add changed or new values for saving
		/// </summary>
		public void AddUnsavedValue(string path, JValue value)
		{
			if (UnsavedValues.ContainsKey(path))
			{
				Debug.Console(0, "Master[{0}] WARNING - Attempt to add duplicate value for path '{1}'.\r Ingoring. Please ensure that path does not exist on multiple modules.", UniqueID, path);
			}
			else
				UnsavedValues.Add(path, value);
			//Debug.Console(0, "Master[{0}] Unsaved size={1}", UniqueID, UnsavedValues.Count);
		}

        /// <summary>
        /// Saves the file
        /// </summary>
		public abstract void Save();


		/// <summary>
		/// 
		/// </summary>
		public static class JsonFixes
		{
            /// <summary>
            /// Deserializes a string into a JObject
            /// </summary>
            /// <param name="json"></param>
            /// <returns></returns>
			public static JObject ParseObject(string json)
			{
				#if NET6_0
                using (var reader = new JsonTextReader(new System.IO.StringReader(json)))
#else
                using (var reader = new JsonTextReader(new Crestron.SimplSharp.CrestronIO.StringReader(json)))
#endif
				{
					var startDepth = reader.Depth;
					var obj = JObject.Load(reader);
					if (startDepth != reader.Depth)
						throw new JsonSerializationException("Unenclosed json found");
					return obj;
				}
			}

            /// <summary>
            /// Deserializes a string into a JArray
            /// </summary>
            /// <param name="json"></param>
            /// <returns></returns>
			public static JArray ParseArray(string json)
			{
				#if NET6_0
                using (var reader = new JsonTextReader(new System.IO.StringReader(json)))
#else
                using (var reader = new JsonTextReader(new Crestron.SimplSharp.CrestronIO.StringReader(json)))
#endif
				{
					var startDepth = reader.Depth;
					var obj = JArray.Load(reader);
					if (startDepth != reader.Depth)
						throw new JsonSerializationException("Unenclosed json found");
					return obj;
				}
			}
		}

		/// <summary>
		/// Helper event
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			if (BoolChange != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		/// <summary>
		/// Helper event
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnUshrtChange(ushort state, ushort index, ushort type)
		{
			if (UshrtChange != null)
			{
				var args = new UshrtChangeEventArgs(state, type);
				args.Index = index;
				UshrtChange(this, args);
			}
		}

        /// <summary>
        /// Helper event
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			if (StringChange != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}
	}
}
