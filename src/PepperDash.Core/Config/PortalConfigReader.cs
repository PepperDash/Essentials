using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Core.Config
{


    /// <summary>
    /// Reads a Portal formatted config file
    /// </summary>
	public class PortalConfigReader
	{
        const string template = "template";
        const string system = "system";
		const string systemUrl = "system_url";
        const string templateUrl = "template_url";
		const string info = "info";
        const string devices = "devices";
        const string rooms = "rooms";
        const string sourceLists = "sourceLists";
        const string destinationLists = "destinationLists";
        const string cameraLists = "cameraLists";
        const string audioControlPointLists = "audioControlPointLists";

		const string tieLines = "tieLines";
        const string joinMaps = "joinMaps";
		const string global = "global";


        /// <summary>
        /// Reads the config file, checks if it needs a merge, merges and saves, then returns the merged Object.
        /// </summary>
        /// <returns>JObject of config file</returns>
        public static void ReadAndMergeFileIfNecessary(string filePath, string savePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					Debug.LogError(
						"ERROR: Configuration file not present. Please load file to {0} and reset program", filePath);
				}

				using (StreamReader fs = new StreamReader(filePath))
				{
					var jsonObj = JObject.Parse(fs.ReadToEnd());
					if(jsonObj[template] != null && jsonObj[system] != null)
					{
						// it's a double-config, merge it.
						var merged = MergeConfigs(jsonObj);
						if (jsonObj[systemUrl] != null)
						{
							merged[systemUrl] = jsonObj[systemUrl].Value<string>();
						}

						if (jsonObj[templateUrl] != null)
						{
							merged[templateUrl] = jsonObj[templateUrl].Value<string>();
						}

						jsonObj = merged;
					}

					using (StreamWriter fw = new StreamWriter(savePath))
					{
						fw.Write(jsonObj.ToString(Formatting.Indented));
						Debug.LogMessage(LogEventLevel.Debug, "JSON config merged and saved to {0}", savePath);
					}

				}
			}
			catch (Exception e)
			{
				Debug.LogMessage(e, "ERROR: Config load failed");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="doubleConfig"></param>
		/// <returns></returns>
  /// <summary>
  /// MergeConfigs method
  /// </summary>
		public static JObject MergeConfigs(JObject doubleConfig)
		{
			var system = JObject.FromObject(doubleConfig["system"]);
			var template = JObject.FromObject(doubleConfig["template"]);
			var merged = new JObject();

			// Put together top-level objects
			if (system[info] != null)
				merged.Add(info, Merge(template[info], system[info], info));
			else
				merged.Add(info, template[info]);

			merged.Add(devices, MergeArraysOnTopLevelProperty(template[devices] as JArray,
				system[devices] as JArray, "key", devices));

			if (system[rooms] == null)
				merged.Add(rooms, template[rooms]);
			else
				merged.Add(rooms, MergeArraysOnTopLevelProperty(template[rooms] as JArray,
					system[rooms] as JArray, "key", rooms));

			if (system[sourceLists] == null)
				merged.Add(sourceLists, template[sourceLists]);
			else
				merged.Add(sourceLists, Merge(template[sourceLists], system[sourceLists], sourceLists));

		    if (system[destinationLists] == null)
		        merged.Add(destinationLists, template[destinationLists]);
		    else
		        merged.Add(destinationLists,
		            Merge(template[destinationLists], system[destinationLists], destinationLists));


            if (system[cameraLists] == null)
                merged.Add(cameraLists, template[cameraLists]);
            else
                merged.Add(cameraLists, Merge(template[cameraLists], system[cameraLists], cameraLists));

            if (system[audioControlPointLists] == null)
                merged.Add(audioControlPointLists, template[audioControlPointLists]);
            else
                merged.Add(audioControlPointLists,
                    Merge(template[audioControlPointLists], system[audioControlPointLists], audioControlPointLists));


            // Template tie lines take precedence.  Config tool doesn't do them at system
            // level anyway...
            if (template[tieLines] != null)
				merged.Add(tieLines, template[tieLines]);
			else if (system[tieLines] != null)
				merged.Add(tieLines, system[tieLines]);
			else
				merged.Add(tieLines, new JArray());

            if (template[joinMaps] != null)
                merged.Add(joinMaps, template[joinMaps]);
            else
                merged.Add(joinMaps, new JObject());

			if (system[global] != null)
				merged.Add(global, Merge(template[global], system[global], global));
			else
				merged.Add(global, template[global]);

			//Debug.Console(2, "MERGED CONFIG RESULT: \x0d\x0a{0}", merged);
			return merged;
		}

		/// <summary>
		/// Merges the contents of a base and a delta array, matching the entries on a top-level property
		/// given by propertyName.  Returns a merge of them. Items in the delta array that do not have
		/// a matched item in base array will not be merged. Non keyed system items will replace the template items.
		/// </summary>
		static JArray MergeArraysOnTopLevelProperty(JArray a1, JArray a2, string propertyName, string path)
		{
			var result = new JArray();
			if (a2 == null || a2.Count == 0) // If the system array is null or empty, return the template array
				return a1;
			else if (a1 != null)
			{
                if (a2[0]["key"] == null) // If the first item in the system array has no key, overwrite the template array
                {                                                       // with the system array
                    return a2;
                }
                else    // The arrays are keyed, merge them by key
                {
                    for (int i = 0; i < a1.Count(); i++)
                    {
                        var a1Dev = a1[i];
                        // Try to get a system device and if found, merge it onto template
                        var a2Match = a2.FirstOrDefault(t => t[propertyName].Equals(a1Dev[propertyName]));// t.Value<int>("uid") == tmplDev.Value<int>("uid"));
                        if (a2Match != null)
                        {
                            var mergedItem = Merge(a1Dev, a2Match, string.Format("{0}[{1}].", path, i));// Merge(JObject.FromObject(a1Dev), JObject.FromObject(a2Match));
                            result.Add(mergedItem);
                        }
                        else
                            result.Add(a1Dev);
                    }
                }
			}
			return result;
		}


		/// <summary>
		/// Helper for using with JTokens.  Converts to JObject 
		/// </summary>
		static JObject Merge(JToken t1, JToken t2, string path)
		{
			return Merge(JObject.FromObject(t1), JObject.FromObject(t2), path);
		}

		/// <summary>
		/// Merge o2 onto o1
		/// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <param name="path"></param>
		static JObject Merge(JObject o1, JObject o2, string path)
		{
			foreach (var o2Prop in o2)
			{
				var propKey = o2Prop.Key;
				var o1Value = o1[propKey];
				var o2Value = o2[propKey];

				// if the property doesn't exist on o1, then add it.
				if (o1Value == null)
				{
					o1.Add(propKey, o2Value);
				}
				// otherwise merge them
				else
				{	
					// Drill down
					var propPath = String.Format("{0}.{1}", path, propKey);
					try
					{

						if (o1Value is JArray)
						{
							if (o2Value is JArray)
							{
								o1Value.Replace(MergeArraysOnTopLevelProperty(o1Value as JArray, o2Value as JArray, "key", propPath));
							}
						}
						else if (o2Prop.Value.HasValues && o1Value.HasValues)
						{
							o1Value.Replace(Merge(JObject.FromObject(o1Value), JObject.FromObject(o2Value), propPath));
						}
						else
						{
							o1Value.Replace(o2Prop.Value);
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"Cannot merge items at path {propPath}: \r{e}");
					}
				}
			}
			return o1;
		}
	}
}