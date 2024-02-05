

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public class DeviceJsonApi
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="json"></param>
		public static void DoDeviceActionWithJson(string json)
		{
		    if (String.IsNullOrEmpty(json))
		    {
		        CrestronConsole.ConsoleCommandResponse(
		            "Please provide a JSON object matching the format {\"deviceKey\":\"myDevice\", \"methodName\":\"someMethod\", \"params\": [\"param1\", true]}.\r\nIf the method has no parameters, the \"params\" object may be omitted.");
		        return;
		    }
		    try
		    {
		        var action = JsonConvert.DeserializeObject<DeviceActionWrapper>(json);

		        DoDeviceAction(action);
		    }
		    catch (Exception ex)
		    {
		        CrestronConsole.ConsoleCommandResponse("Incorrect format for JSON. Please check that the format matches {\"deviceKey\":\"myDevice\", \"methodName\":\"someMethod\", \"params\": [\"param1\", true]}");
		    }
		    
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public static void DoDeviceAction(DeviceActionWrapper action)
		{
			var key = action.DeviceKey;
			var obj = FindObjectOnPath(key);
		    if (obj == null)
		    {
		        CrestronConsole.ConsoleCommandResponse("Unable to find object at path {0}", key);
		        return;
		    }

		    if (action.Params == null)
		    {
                //no params, so setting action.Params to empty array
		        action.Params = new object[0];
		    }

		    CType t = obj.GetType();
		    try
		    {
		        var methods = t.GetMethods().Where(m => m.Name == action.MethodName).ToList();

		        var method = methods.Count == 1 ? methods[0] : methods.FirstOrDefault(m => m.GetParameters().Length == action.Params.Length);

		        if (method == null)
		        {
		            CrestronConsole.ConsoleCommandResponse(
		                "Unable to find method with name {0} and that matches parameters {1}", action.MethodName,
		                action.Params);
		            return;
		        }
                var mParams = method.GetParameters();

                var convertedParams = mParams
                                    .Select((p, i) => ConvertType(action.Params[i], p.ParameterType))
                                    .ToArray();
                method.Invoke(obj, convertedParams);

		        CrestronConsole.ConsoleCommandResponse("Method {0} successfully called on device {1}", method.Name,
		            action.DeviceKey);
		    }
		    catch (Exception ex)
		    {
		        CrestronConsole.ConsoleCommandResponse("Unable to call method with name {0}. {1}", action.MethodName,
		            ex.Message);}
		}

	    private static object ConvertType(object value, Type conversionType)
	    {
	        if (!conversionType.IsEnum)
	        {
	            return Convert.ChangeType(value, conversionType, System.Globalization.CultureInfo.InvariantCulture);
	        }

	        var stringValue = Convert.ToString(value);

	        if (String.IsNullOrEmpty(stringValue))
	        {
	            throw new InvalidCastException(
	                String.Format("{0} cannot be converted to a string prior to conversion to enum"));
	        }
	        return Enum.Parse(conversionType, stringValue, true);
	    }

		/// <summary>
		/// Gets the properties on a device
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetProperties(string deviceObjectPath)
		{
			var obj = FindObjectOnPath(deviceObjectPath);
			if (obj == null)
				return "{ \"error\":\"No Device\"}";

			CType t = obj.GetType();
			// get the properties and set them into a new collection of NameType wrappers
			var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
			return JsonConvert.SerializeObject(props, Formatting.Indented);
		}

        /// <summary>
        /// Gets a property from a device path by name
        /// </summary>
        /// <param name="deviceObjectPath"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyByName(string deviceObjectPath, string propertyName)
        {
            var dev = FindObjectOnPath(deviceObjectPath);
            if(dev == null)
                return "{ \"error\":\"No Device\"}";
	
            object prop = dev.GetType().GetCType().GetProperty(propertyName).GetValue(dev, null);

            // var prop = t.GetProperty(propertyName);
            if (prop != null)
            {
                return prop;
            }
            else
            {
                Debug.Console(1, "Unable to find Property: {0} on Device with path: {1}", propertyName, deviceObjectPath);
                return null;
            }
        }

		/// <summary>
		/// Gets the methods on a device
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetMethods(string deviceObjectPath)
		{
			var obj = FindObjectOnPath(deviceObjectPath);
			if (obj == null)
				return "{ \"error\":\"No Device\"}";

			// Package up method names using helper objects
			CType t = obj.GetType();
			var methods = t.GetMethods()
				.Where(m => !m.IsSpecialName)
				.Select(p => new MethodNameParams(p));
			return JsonConvert.SerializeObject(methods, Formatting.Indented);
		}

		public static string GetApiMethods(string deviceObjectPath)
		{
			var obj = FindObjectOnPath(deviceObjectPath);
			if (obj == null)
				return "{ \"error\":\"No Device\"}";

			// Package up method names using helper objects
			CType t = obj.GetType();
			var methods = t.GetMethods()
				.Where(m => !m.IsSpecialName)
				.Where(m => m.GetCustomAttributes(typeof(ApiAttribute), true).Any())
				.Select(p => new MethodNameParams(p));
			return JsonConvert.SerializeObject(methods, Formatting.Indented);
		}
			

		/// <summary>
		///  Walks down a dotted object path, starting with a Device, and returns the object
		///  at the end of the path
		/// </summary>
		public static object FindObjectOnPath(string deviceObjectPath)
		{
			var path = deviceObjectPath.Split('.');

			var dev = DeviceManager.GetDeviceForKey(path[0]);
			if (dev == null)
			{
				Debug.Console(0, "Device {0} not found", path[0]);
				return null;
			}

			// loop through any dotted properties
			object obj = dev;
			if (path.Length > 1)
			{
				for (int i = 1; i < path.Length; i++)
				{
					var objName = path[i];
					string indexStr = null;
					var indexOpen = objName.IndexOf('[');
					if (indexOpen != -1)
					{
						var indexClose = objName.IndexOf(']');
						if (indexClose == -1)
						{
							Debug.Console(0, dev, "ERROR Unmatched index brackets");
							return null;
						}
						// Get the index and strip quotes if any
						indexStr = objName.Substring(indexOpen + 1, indexClose - indexOpen - 1).Replace("\"", "");
						objName = objName.Substring(0, indexOpen);
						Debug.Console(0, dev, "  Checking for collection '{0}', index '{1}'", objName, indexStr);
					}

					CType oType = obj.GetType();
					var prop = oType.GetProperty(objName);
					if (prop == null)
					{
						Debug.Console(0, dev, "Property {0} not found on {1}", objName, path[i - 1]);
						return null;
					}
					// if there's an index, try to get the property
					if (indexStr != null)
					{
						if (!typeof(ICollection).IsAssignableFrom(prop.PropertyType))
						{
							Debug.Console(0, dev, "Property {0} is not collection", objName);
							return null;
						}
						var collection = prop.GetValue(obj, null) as ICollection;
						// Get the indexed items "property"
						var indexedPropInfo = prop.PropertyType.GetProperty("Item");
						// These are the parameters for the indexing. Only care about one
						var indexParams = indexedPropInfo.GetIndexParameters();
						if (indexParams.Length > 0)
						{
							Debug.Console(0, "  Indexed, param type: {0}", indexParams[0].ParameterType.Name);
							var properParam = Convert.ChangeType(indexStr, indexParams[0].ParameterType,
								System.Globalization.CultureInfo.InvariantCulture);
							try
							{
								obj = indexedPropInfo.GetValue(collection, new object[] { properParam });
							}
							// if the index is bad, catch it here.
							catch (Crestron.SimplSharp.Reflection.TargetInvocationException e)
							{
								if (e.InnerException is ArgumentOutOfRangeException)
									Debug.Console(0, "  Index Out of range");
								else if (e.InnerException is KeyNotFoundException)
									Debug.Console(0, "  Key not found");
								return null;
							}
						}

					}
					else
						obj = prop.GetValue(obj, null);
				}
			}
			return obj;
		}

        /// <summary>
        /// Sets a property on an object.
        /// </summary>
        /// <param name="deviceObjectPath"></param>
        /// <returns></returns>
        public static string SetProperty(string deviceObjectPath)
        {
            throw new NotImplementedException("This could be really useful. Finish it please");

            //var obj = FindObjectOnPath(deviceObjectPath);
            //if (obj == null)
            //    return "{\"error\":\"No object found\"}";

            //CType t = obj.GetType();


            //// get the properties and set them into a new collection of NameType wrappers
            //var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
            //return JsonConvert.SerializeObject(props, Formatting.Indented);
        }

        
	}

	public class DeviceActionWrapper
	{
		public string DeviceKey { get; set; }
		public string MethodName { get; set; }
		public object[] Params { get; set; }
	}

	public class PropertyNameType
	{
        object Parent;

		[JsonIgnore]
		public PropertyInfo PropInfo { get; private set; }
		public string Name { get { return PropInfo.Name; } }
		public string Type { get { return PropInfo.PropertyType.Name; } }
        public string Value { get 
        {
            if (PropInfo.CanRead)
            {
                try
                {
                    return PropInfo.GetValue(Parent, null).ToString();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
                return null;    
        } }

        public bool CanRead { get { return PropInfo.CanRead; } }
        public bool CanWrite { get { return PropInfo.CanWrite; } }


		public PropertyNameType(PropertyInfo info, object parent)
		{
			PropInfo = info;
            Parent = parent;
		}
	}

	public class MethodNameParams
	{
		[JsonIgnore]
		public MethodInfo MethodInfo { get; private set; }

		public string Name { get { return MethodInfo.Name; } }
		public IEnumerable<NameType> Params { get {
			return MethodInfo.GetParameters().Select(p => 
				new NameType { Name = p.Name, Type = p.ParameterType.Name });
		} }

		public MethodNameParams(MethodInfo info)
		{
			MethodInfo = info;
		}
	}

	public class NameType
	{
		public string Name { get; set; }
		public string Type { get; set; }
	}

	[AttributeUsage(AttributeTargets.All)]
	public class ApiAttribute : CAttribute
	{

	}
}