using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Provides methods for interacting with devices using JSON-formatted commands.
/// </summary>
public class DeviceJsonApi
{
    /// <summary>
    /// Executes a method on a device based on a JSON-formatted command string.
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
        catch (Exception)
        {
            CrestronConsole.ConsoleCommandResponse("Incorrect format for JSON. Please check that the format matches {\"deviceKey\":\"myDevice\", \"methodName\":\"someMethod\", \"params\": [\"param1\", true]}");
        }

    }


    /// <summary>
    /// Executes a method on a device based on a JSON-formatted command string, awaiting the result if the method is asynchronous.
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

        Type t = obj.GetType();
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

            Task.Run(() =>
                {
                    try
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Calling method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
                        method.Invoke(obj, convertedParams);
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(e, "Error invoking method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
                    }
                });

            CrestronConsole.ConsoleCommandResponse("Method {0} successfully called on device {1}", method.Name,
                action.DeviceKey);
        }
        catch (Exception ex)
        {
            CrestronConsole.ConsoleCommandResponse("Unable to call method with name {0}. {1}", action.MethodName,
                ex.Message);
        }
    }

    /// <summary>
    /// Executes a method on a device based on a JSON-formatted command string, awaiting the result if the method is asynchronous.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task DoDeviceActionAsync(DeviceActionWrapper action)
    {
        var key = action.DeviceKey;
        var obj = FindObjectOnPath(key);
        if (obj == null)
        {
            Debug.LogMessage(LogEventLevel.Warning, "Unable to find object at path {deviceKey}", null, key);
            return;
        }

        if (action.Params == null)
        {
            //no params, so setting action.Params to empty array
            action.Params = new object[0];
        }

        Type t = obj.GetType();
        try
        {
            var methods = t.GetMethods().Where(m => m.Name == action.MethodName).ToList();

            var method = methods.Count == 1 ? methods[0] : methods.FirstOrDefault(m => m.GetParameters().Length == action.Params.Length);

            if (method == null)
            {
                Debug.LogMessage(LogEventLevel.Warning,
                    "Unable to find method with name {methodName} and that matches parameters {@parameters}", null, action.MethodName,
                    action.Params);
                return;
            }
            var mParams = method.GetParameters();

            var convertedParams = mParams
                                .Select((p, i) => ConvertType(action.Params[i], p.ParameterType))
                                .ToArray();

            try
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Calling method {methodName} on device {deviceKey} with {@params}", null, method.Name, action.DeviceKey, action.Params);
                var result = method.Invoke(obj, convertedParams);

                // If the method returns a Task, await it
                if (result is Task task)
                {
                    await task;
                }
                // If the method returns a Task<T>, await it
                else if (result != null && result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                {
                    await (Task)result;
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(e, "Error invoking method {methodName} on device {deviceKey}", null, method.Name, action.DeviceKey);
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Unable to call method with name {methodName} with {@parameters}", null, action.MethodName, action.Params);
        }
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
    /// <param name="deviceObjectPath">The path to the device object.</param>
    /// <returns>A JSON-formatted string representing the properties of the device.</returns>
    public static string GetProperties(string deviceObjectPath)
    {
        var obj = FindObjectOnPath(deviceObjectPath);
        if (obj == null)
            return "{ \"error\":\"No Device\"}";

        Type t = obj.GetType();
        // get the properties and set them into a new collection of NameType wrappers
        var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
        return JsonConvert.SerializeObject(props, Formatting.Indented);
    }

    /// <summary>
    /// Gets a property from a device path by name
    /// </summary>
    /// <param name="deviceObjectPath">The path to the device object.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The value of the property, or a JSON-formatted error string if the device or property is not found.</returns>
    public static object GetPropertyByName(string deviceObjectPath, string propertyName)
    {
        var dev = FindObjectOnPath(deviceObjectPath);
        if (dev == null)
            return "{ \"error\":\"No Device\"}";

        object prop = dev.GetType().GetProperty(propertyName).GetValue(dev, null);

        // var prop = t.GetProperty(propertyName);
        if (prop != null)
        {
            return prop;
        }
        else
        {
            Debug.LogMessage(LogEventLevel.Debug, "Unable to find Property: {0} on Device with path: {1}", propertyName, deviceObjectPath);
            return null;
        }
    }

    /// <summary>
    /// Gets the methods on a device
    /// </summary>
    /// <param name="deviceObjectPath">The path to the device object.</param>
    /// <returns>A JSON-formatted string representing the methods of the device.</returns>
    public static string GetMethods(string deviceObjectPath)
    {
        var obj = FindObjectOnPath(deviceObjectPath);
        if (obj == null)
            return "{ \"error\":\"No Device\"}";

        // Package up method names using helper objects
        Type t = obj.GetType();
        var methods = t.GetMethods()
            .Where(m => !m.IsSpecialName)
            .Select(p => new MethodNameParams(p));
        return JsonConvert.SerializeObject(methods, Formatting.Indented);
    }

    /// <summary>
    /// Gets the API methods on a device, which are the methods marked with the ApiAttribute.
    /// These are the methods intended to be called through the JSON API.
    /// This allows for hiding certain methods from the API if desired.
    /// </summary>
    /// <param name="deviceObjectPath">The path to the device object.</param>
    /// <returns>A JSON-formatted string representing the API methods of the device.</returns>
    public static string GetApiMethods(string deviceObjectPath)
    {
        var obj = FindObjectOnPath(deviceObjectPath);
        if (obj == null)
            return "{ \"error\":\"No Device\"}";

        // Package up method names using helper objects
        Type t = obj.GetType();
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
            Debug.LogMessage(LogEventLevel.Information, "Device {0} not found", path[0]);
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
                        Debug.LogMessage(LogEventLevel.Information, dev, "ERROR Unmatched index brackets");
                        return null;
                    }
                    // Get the index and strip quotes if any
                    indexStr = objName.Substring(indexOpen + 1, indexClose - indexOpen - 1).Replace("\"", "");
                    objName = objName.Substring(0, indexOpen);
                    Debug.LogMessage(LogEventLevel.Information, dev, "  Checking for collection '{0}', index '{1}'", objName, indexStr);
                }

                Type oType = obj.GetType();
                var prop = oType.GetProperty(objName);
                if (prop == null)
                {
                    Debug.LogMessage(LogEventLevel.Information, dev, "Property {0} not found on {1}", objName, path[i - 1]);
                    return null;
                }
                // if there's an index, try to get the property
                if (indexStr != null)
                {
                    if (!typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                    {
                        Debug.LogMessage(LogEventLevel.Information, dev, "Property {0} is not collection", objName);
                        return null;
                    }
                    var collection = prop.GetValue(obj, null) as ICollection;
                    // Get the indexed items "property"
                    var indexedPropInfo = prop.PropertyType.GetProperty("Item");
                    // These are the parameters for the indexing. Only care about one
                    var indexParams = indexedPropInfo.GetIndexParameters();
                    if (indexParams.Length > 0)
                    {
                        Debug.LogMessage(LogEventLevel.Information, "  Indexed, param type: {0}", indexParams[0].ParameterType.Name);
                        var properParam = Convert.ChangeType(indexStr, indexParams[0].ParameterType,
                            System.Globalization.CultureInfo.InvariantCulture);
                        try
                        {
                            obj = indexedPropInfo.GetValue(collection, new object[] { properParam });
                        }
                        // if the index is bad, catch it here.
                        catch (TargetInvocationException e)
                        {
                            if (e.InnerException is ArgumentOutOfRangeException)
                                Debug.LogMessage(LogEventLevel.Information, "  Index Out of range");
                            else if (e.InnerException is KeyNotFoundException)
                                Debug.LogMessage(LogEventLevel.Information, "  Key not found");
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

        //Type t = obj.GetType();


        //// get the properties and set them into a new collection of NameType wrappers
        //var props = t.GetProperties().Select(p => new PropertyNameType(p, obj));
        //return JsonConvert.SerializeObject(props, Formatting.Indented);
    }


}

/// <summary>
/// Attribute to mark methods as part of the JSON API. Only methods marked with this attribute will be returned by GetApiMethods and intended to be called through the JSON API.
/// </summary>
public class DeviceActionWrapper
{
    /// <summary>
    /// The key of the device to call the method on
    /// </summary>
    public string DeviceKey { get; set; }

    /// <summary>
    /// The name of the method to call
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// The parameters to pass to the method. This should be an array of objects matching the parameters of the method being called. If the method has no parameters, this can be omitted or set to null.
    /// </summary>
    public object[] Params { get; set; }
}

/// <summary>
/// Helper class for serializing properties with their name, type, and value, and whether they can be read or written to.
/// </summary>
public class PropertyNameType
{
    private object Parent;

    /// <summary>
    /// The PropertyInfo for the property being represented. This is ignored for JSON serialization.
    /// </summary>
    [JsonIgnore]
    public PropertyInfo PropInfo { get; private set; }

    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get { return PropInfo.Name; } }

    /// <summary>
    /// The type of the property
    /// </summary>
    public string Type { get { return PropInfo.PropertyType.Name; } }

    /// <summary>
    /// The value of the property, or null if the property cannot be read or an error occurs when trying to read it.
    /// </summary>
    public string Value
    {
        get
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
        }
    }

    /// <summary>
    /// Indicates whether the property can be read from
    /// </summary>
    public bool CanRead { get { return PropInfo.CanRead; } }
    /// <summary>
    /// Indicates whether the property can be written to
    /// </summary>
    public bool CanWrite { get { return PropInfo.CanWrite; } }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="parent"></param>
    public PropertyNameType(PropertyInfo info, object parent)
    {
        PropInfo = info;
        Parent = parent;
    }
}

/// <summary>
/// Helper class for serializing methods with their name and parameters. The MethodInfo is ignored for JSON serialization.
/// </summary>
public class MethodNameParams
{
    /// <summary>
    /// The MethodInfo for the method being represented. This is ignored for JSON serialization.
    /// </summary>
    [JsonIgnore]
    public MethodInfo MethodInfo { get; private set; }

    /// <summary>
    /// The name of the method
    /// </summary>
    public string Name { get { return MethodInfo.Name; } }

    /// <summary>
    /// The parameters of the method, represented as an array of NameType objects with the parameter name and type.
    /// </summary>
    public IEnumerable<NameType> Params
    {
        get
        {
            return MethodInfo.GetParameters().Select(p =>
                new NameType { Name = p.Name, Type = p.ParameterType.Name });
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="info"></param>
    public MethodNameParams(MethodInfo info)
    {
        MethodInfo = info;
    }
}

/// <summary>
/// Helper class for serializing a name and type pair, used for method parameters in MethodNameParams.
/// </summary>
public class NameType
{
    /// <summary>
    /// The name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of the parameter
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
/// Attribute to mark methods as part of the JSON API. Only methods marked with this attribute will be returned by GetApiMethods and intended to be called through the JSON API.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class ApiAttribute : Attribute
{

}