using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PepperDash.Essentials.Core;

// No Crestron references by design, so the method and parameter descriptions served by devmethods and
// deviceMethods can be covered by PepperDash.Essentials.Core.Tests.

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
            return MethodInfo.GetParameters().Select(NameType.FromParameter);
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
/// The enum and default value fields are only serialized when they apply, so other parameters serialize as just Name and Type.
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

    /// <summary>
    /// True when the parameter is an enum, or a nullable enum. Omitted otherwise.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsEnum { get; set; }

    /// <summary>
    /// True when the parameter's enum has the [Flags] attribute, so its values can be combined, e.g. "Audio, Video". Omitted otherwise.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsFlags { get; set; }

    /// <summary>
    /// The names of the parameter's enum values, in declaration order. Omitted when the parameter isn't an enum.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[] EnumValues { get; set; }

    /// <summary>
    /// The default value of an optional parameter, with an enum default given as its name.
    /// Omitted when the parameter isn't optional or its default is null.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object DefaultValue { get; set; }

    /// <summary>
    /// Describes a method parameter. A nullable enum parameter reports its underlying enum's values, with Type unchanged.
    /// </summary>
    /// <param name="p">The parameter to describe</param>
    /// <returns>The parameter's name and type, plus its enum values and default value where they apply</returns>
    public static NameType FromParameter(ParameterInfo p)
    {
        var nameType = new NameType { Name = p.Name, Type = p.ParameterType.Name };

        var enumType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;

        if (enumType.IsEnum)
        {
            nameType.IsEnum = true;
            nameType.EnumValues = GetEnumNames(enumType);

            if (enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                nameType.IsFlags = true;
            }
        }

        if (p.IsOptional)
        {
            nameType.DefaultValue = GetDefaultValue(p.DefaultValue, enumType);
        }

        return nameType;
    }

    /// <summary>
    /// Enum.GetNames sorts the names by value; the enum's fields keep the order they're declared in.
    /// </summary>
    private static string[] GetEnumNames(Type enumType)
    {
        return enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.Name)
            .ToArray();
    }

    private static object GetDefaultValue(object value, Type enumType)
    {
        // An optional parameter without a default value (e.g. [Optional]) reports DBNull or Missing
        if (value == null || value is DBNull || value == Missing.Value)
        {
            return null;
        }

        if (enumType.IsEnum)
        {
            // The default can come back as the enum or as its underlying integer; either way, report the name.
            // A [Flags] combination comes back as "Audio, Video", which the enum conversion in devjson accepts.
            return (value.GetType().IsEnum ? value : Enum.ToObject(enumType, value)).ToString();
        }

        return value;
    }
}
