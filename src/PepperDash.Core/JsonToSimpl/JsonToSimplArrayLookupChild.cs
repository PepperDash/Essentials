using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog.Events;

namespace PepperDash.Core.JsonToSimpl
{
 /// <summary>
 /// Represents a JsonToSimplArrayLookupChild
 /// </summary>
	public class JsonToSimplArrayLookupChild : JsonToSimplChildObjectBase
	{
        /// <summary>
        /// 
        /// </summary>
		public string SearchPropertyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string SearchPropertyValue { get; set; }

		int ArrayIndex;

		/// <summary>
		/// For gt2.4.1 array lookups
		/// </summary>
		/// <param name="file"></param>
		/// <param name="key"></param>
		/// <param name="pathPrefix"></param>
		/// <param name="pathSuffix"></param>
		/// <param name="searchPropertyName"></param>
		/// <param name="searchPropertyValue"></param>
		public void Initialize(string file, string key, string pathPrefix, string pathSuffix,
			string searchPropertyName, string searchPropertyValue)
		{
			base.Initialize(file, key, pathPrefix, pathSuffix);
			SearchPropertyName = searchPropertyName;
			SearchPropertyValue = searchPropertyValue;
		}


		/// <summary>
		/// For newer >=2.4.1 array lookups. 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="key"></param>
		/// <param name="pathPrefix"></param>
		/// <param name="pathAppend"></param>
		/// <param name="pathSuffix"></param>
		/// <param name="searchPropertyName"></param>
		/// <param name="searchPropertyValue"></param>
		public void InitializeWithAppend(string file, string key, string pathPrefix, string pathAppend,
			string pathSuffix, string searchPropertyName, string searchPropertyValue)
		{
			string pathPrefixWithAppend = (pathPrefix != null ? pathPrefix : "") + GetPathAppend(pathAppend);
			base.Initialize(file, key, pathPrefixWithAppend, pathSuffix);

			SearchPropertyName = searchPropertyName;
			SearchPropertyValue = searchPropertyValue;
		}



		//PathPrefix+ArrayName+[x]+path+PathSuffix
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected override string GetFullPath(string path)
		{
			return string.Format("{0}[{1}].{2}{3}",
				PathPrefix == null ? "" : PathPrefix,
				ArrayIndex,
				path,
				PathSuffix == null ? "" : PathSuffix);
		}

  /// <summary>
  /// ProcessAll method
  /// </summary>
  /// <inheritdoc />
		public override void ProcessAll()
		{
			if (FindInArray())
				base.ProcessAll();
		}

		/// <summary>
		/// Provides the path append for GetFullPath
		/// </summary>
		/// <returns></returns>
		string GetPathAppend(string a)
		{
			if (string.IsNullOrEmpty(a))
			{
				return "";
			}
			if (a.StartsWith("."))
			{
				return a;
			}
			else
			{
				return "." + a;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		bool FindInArray()
		{
			if (Master == null)
				throw new InvalidOperationException("Cannot do operations before master is linked");
			if (Master.JsonObject == null)
				throw new InvalidOperationException("Cannot do operations before master JSON has read");
			if (PathPrefix == null)
				throw new InvalidOperationException("Cannot do operations before PathPrefix is set");


			var token = Master.JsonObject.SelectToken(PathPrefix);
			if (token is JArray)
			{
				var array = token as JArray;
				try
				{
					var item = array.FirstOrDefault(o =>
					{
						var prop = o[SearchPropertyName];
						return prop != null && prop.Value<string>()
						.Equals(SearchPropertyValue, StringComparison.OrdinalIgnoreCase);
					});
					if (item == null)
					{
						Debug.LogMessage(LogEventLevel.Debug,"JSON Child[{0}] Array '{1}' '{2}={3}' not found: ", Key,
							PathPrefix, SearchPropertyName, SearchPropertyValue);
						this.LinkedToObject = false;
						return false;
					}

					this.LinkedToObject = true;
					ArrayIndex = array.IndexOf(item);
					OnStringChange(string.Format("{0}[{1}]", PathPrefix, ArrayIndex), 0, JsonToSimplConstants.FullPathToArrayChange);
					Debug.LogMessage(LogEventLevel.Debug, "JSON Child[{0}] Found array match at index {1}", Key, ArrayIndex);
					return true;
				}
				catch (Exception e)
				{
					Debug.LogMessage(e, "JSON Child[{key}] Array '{pathPrefix}' lookup error: '{searchPropertyName}={searchPropertyValue}'", null, Key,
						PathPrefix, SearchPropertyName, SearchPropertyValue, e);
				}
			}
			else
			{
				Debug.LogMessage(LogEventLevel.Debug, "JSON Child[{0}] Path '{1}' is not an array", Key, PathPrefix);
			}

			return false;
		}
	}
}