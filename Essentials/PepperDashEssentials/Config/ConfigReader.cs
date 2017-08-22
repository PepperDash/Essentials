using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class ConfigReader
	{
		public static EssentialsConfig ConfigObject { get; private set; }

		public static void LoadConfig2()
		{
			Debug.Console(0, "Using unmerged system/template configs.");
			try
			{
				using (StreamReader fs = new StreamReader(string.Format(@"\NVRAM\program{0}\ConfigurationFile.json", 
                    InitialParametersClass.ApplicationNumber)))
				{
					var doubleObj = JObject.Parse(fs.ReadToEnd());
					ConfigObject = MergeConfigs(doubleObj).ToObject<EssentialsConfig>();

                    // Extract SystemUrl and TemplateUrl

                    if (doubleObj["system_url"] != null)
                    {
                        ConfigObject.SystemUrl = doubleObj["system_url"].Value<string>();
                    }

                    if (doubleObj["template_url"] != null)
                    {
                        ConfigObject.TemplateUrl= doubleObj["template_url"].Value<string>();
                    }

                    
				}
			}
			catch (Exception e)
			{
				Debug.Console(0, "Config failed: \r{0}", e);
			}
		}


		static JObject MergeConfigs(JObject doubleConfig)
		{
			var system = JObject.FromObject(doubleConfig["system"]);
			var template = JObject.FromObject(doubleConfig["template"]);
			var merged = new JObject();

			// Put together top-level objects
			if (system["info"] != null)
				merged.Add("info", Merge(template["info"], system["info"]));
			else
				merged.Add("info", template["info"]);

			merged.Add("devices", MergeArraysOnTopLevelProperty(template["devices"] as JArray, 
                system["devices"] as JArray, "uid"));

			if (system["rooms"] == null)
				merged.Add("rooms", template["rooms"]);
			else
				merged.Add("rooms", MergeArraysOnTopLevelProperty(template["rooms"] as JArray, 
                    system["rooms"] as JArray, "key"));

			if (system["sourceLists"] == null)
				merged.Add("sourceLists", template["sourceLists"]);
			else
				merged.Add("sourceLists", Merge(template["sourceLists"], system["sourceLists"]));

            // Template tie lines take precdence.  Config tool probably can't do them at system
            // level anyway...
			if (template["tieLines"] != null)
				merged.Add("tieLines", template["tieLines"]);
			else if (system["tieLines"] != null)
				merged.Add("tieLines", system["tieLines"]);
			else
				merged.Add("tieLines", new JArray());

			//Debug.Console(0, "MERGED RESULT: \x0d\x0a{0}", merged);
			return merged;
		}

		/// <summary>
		/// Merges the contents of a base and a delta array, matching the entries on a top-level property
		/// given by propertyName.  Returns a merge of them. Items in the delta array that do not have
		/// a matched item in base array will not be merged. 
		/// </summary>
		static JArray MergeArraysOnTopLevelProperty(JArray a1, JArray a2, string propertyName)
		{
			var result = new JArray();
			if (a2 == null)
				result = a1;
			else if (a1 != null)
			{
				for (int i = 0; i < a1.Count(); i++)
				{
					var a1Dev = a1[i];
					// Try to get a system device and if found, merge it onto template
					var a2Match = a2.FirstOrDefault(t => t[propertyName].Equals(a1Dev[propertyName]));// t.Value<int>("uid") == tmplDev.Value<int>("uid"));
					if (a2Match != null)
					{
						var mergedItem = Merge(a1Dev, a2Match);// Merge(JObject.FromObject(a1Dev), JObject.FromObject(a2Match));
						result.Add(mergedItem);
					}
					else
						result.Add(a1Dev);
				}
			}
			return result;
		}


		/// <summary>
		/// Helper for using with JTokens.  Converts to JObject 
		/// </summary>
		static JObject Merge(JToken t1, JToken t2)
		{
			return Merge(JObject.FromObject(t1), JObject.FromObject(t2));
		}

		/// <summary>
		/// Merge b ONTO a
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		static JObject Merge(JObject o1, JObject o2)
		{
			//Console.WriteLine("Merging {0}\ronto {1}", o2, o1);
			foreach (var o2Prop in o2)
			{
				var o1Value = o1[o2Prop.Key];
				if (o1Value == null)
					o1.Add(o2Prop.Key, o2Prop.Value);
				else
				{
					JToken replacement = null;
					if (o2Prop.Value.HasValues && o1Value.HasValues) // Drill down
						replacement = Merge(JObject.FromObject(o1Value), JObject.FromObject(o2Prop.Value));
					else
						replacement = o2Prop.Value;
					o1[o2Prop.Key].Replace(replacement);
				}
			}
			return o1;
		}

        public static string GetGroupForDeviceKey(string key)
        {
            var dev = ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return dev == null ? null : dev.Group;
        }

	}
}