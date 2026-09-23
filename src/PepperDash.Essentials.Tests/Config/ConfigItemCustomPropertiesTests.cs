using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Tests.Config;

/// <summary>
/// Tests for the <c>[JsonExtensionData]</c> <c>CustomProperties</c> member on the config metadata
/// list item types, which lets a project carry deployment-specific properties in its config file
/// without a framework change.
/// </summary>
/// <remarks>
/// The subtle part is the interaction with computed properties. Several of these types expose a
/// value such as <c>preferredName</c> that is serialized out but derived at runtime. Newtonsoft
/// routes a JSON property into extension data whenever it cannot bind it to a <em>writable</em>
/// member, so a purely get-only computed property would swallow its own config value into
/// <c>CustomProperties</c> and then re-emit it as a duplicate top-level key. Those properties
/// therefore carry a no-op setter; <see cref="ComputedPropertyIsNotDivertedIntoCustomProperties"/>
/// and <see cref="SerializedOutputHasNoDuplicateKeys"/> are the guards for that.
/// </remarks>
public class ConfigItemCustomPropertiesTests
{
    /// <summary>
    /// Every config metadata type that declares (or inherits) <c>CustomProperties</c>, paired with
    /// the JSON names of its computed properties.
    /// </summary>
    public static TheoryData<Type, string[]> MetadataTypes => new()
    {
        { typeof(SourceListItem), new[] { "preferredName" } },
        { typeof(DestinationListItem), new[] { "preferredName" } },
        { typeof(CameraListItem), new[] { "preferredName" } },
        // Both inherit CustomProperties from AudioControlListItemBase, which is abstract.
        { typeof(LevelControlListItem), new[] { "preferredName", "deviceKey" } },
        { typeof(PresetListItem), new[] { "preferredName" } },
    };

    // ---------------------------------------------------------------------------
    // Capture
    // ---------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(MetadataTypes))]
    public void UnknownPropertiesAreCapturedInCustomProperties(Type type, string[] computed)
    {
        _ = computed;

        var item = Deserialize(type, BuildJson(type, includeExtras: true));

        var extras = GetCustomProperties(item);

        extras.Should().NotBeNull(
            "project-specific properties must survive deserialization rather than being discarded");
        extras!.Should().ContainKeys("uiGroup", "hasDeviceControls", "nested");
    }

    [Fact]
    public void CapturedValuesKeepTheirJsonTypes()
    {
        var item = JsonConvert.DeserializeObject<SourceListItem>(@"{
            ""sourceKey"": ""laptop"",
            ""name"": ""Laptop"",
            ""customButtonColor"": ""#FF5733"",
            ""showInHelpMenu"": true,
            ""weight"": 2.5,
            ""nested"": { ""a"": 1, ""b"": [1, 2, 3] }
        }")!;

        var extras = item.CustomProperties;

        extras["customButtonColor"].Value<string>().Should().Be("#FF5733");
        extras["showInHelpMenu"].Value<bool>().Should().BeTrue();
        extras["weight"].Value<double>().Should().Be(2.5);
        extras["nested"]["a"]!.Value<int>().Should().Be(1);
        extras["nested"]["b"]![2]!.Value<int>().Should().Be(3);
    }

    // ---------------------------------------------------------------------------
    // Computed-property regression guards
    // ---------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(MetadataTypes))]
    public void ComputedPropertyIsNotDivertedIntoCustomProperties(Type type, string[] computed)
    {
        // The JSON carries a stale value for each computed property, as a config produced by
        // round-tripping these objects would.
        var item = Deserialize(type, BuildJson(type, includeExtras: true));

        var extras = GetCustomProperties(item);

        extras.Should().NotBeNull();
        extras!.Keys.Should().NotIntersectWith(
            computed,
            "a computed property is a known member and must be read and discarded, not captured");
    }

    [Theory]
    [MemberData(nameof(MetadataTypes))]
    public void SerializedOutputHasNoDuplicateKeys(Type type, string[] computed)
    {
        _ = computed;

        var item = Deserialize(type, BuildJson(type, includeExtras: true));

        var names = TopLevelPropertyNames(JsonConvert.SerializeObject(item));

        names.Should().OnlyHaveUniqueItems(
            "extension data re-serializes as top-level keys, so a captured computed property " +
            "would emit a duplicate that JSON.parse resolves to the stale config value");
    }

    [Fact]
    public void ComputedPropertyStillReportsTheRuntimeValue()
    {
        var item = JsonConvert.DeserializeObject<DestinationListItem>(@"{
            ""sinkKey"": ""display1"",
            ""name"": ""Main Display"",
            ""preferredName"": ""STALE VALUE FROM CONFIG""
        }")!;

        item.PreferredName.Should().Be("Main Display");
    }

    [Fact]
    public void ComputedPropertyIsStillSerializedOut()
    {
        var item = JsonConvert.DeserializeObject<DestinationListItem>(
            @"{""sinkKey"":""display1"",""name"":""Main Display""}")!;

        var json = JObject.Parse(JsonConvert.SerializeObject(item));

        json["preferredName"]!.Value<string>().Should().Be(
            "Main Display", "the React app reads this value from the serialized metadata");
    }

    // ---------------------------------------------------------------------------
    // Backward compatibility
    // ---------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(MetadataTypes))]
    public void ConfigWithNoExtraPropertiesLeavesCustomPropertiesNull(Type type, string[] computed)
    {
        _ = computed;

        var item = Deserialize(type, BuildJson(type, includeExtras: false));

        GetCustomProperties(item).Should().BeNull();
    }

    [Fact]
    public void KnownPropertiesStillBind()
    {
        var item = JsonConvert.DeserializeObject<SourceListItem>(@"{
            ""sourceKey"": ""laptop"",
            ""name"": ""Laptop"",
            ""icon"": ""Laptop"",
            ""order"": 3,
            ""includeInSourceList"": true,
            ""uiGroup"": ""overflow""
        }")!;

        item.SourceKey.Should().Be("laptop");
        item.Name.Should().Be("Laptop");
        item.Icon.Should().Be("Laptop");
        item.Order.Should().Be(3);
        item.IncludeInSourceList.Should().BeTrue();
        item.CustomProperties.Should().ContainKey("uiGroup");
    }

    [Theory]
    [MemberData(nameof(MetadataTypes))]
    public void RoundTripIsStable(Type type, string[] computed)
    {
        _ = computed;

        var once = JsonConvert.SerializeObject(Deserialize(type, BuildJson(type, includeExtras: true)));
        var twice = JsonConvert.SerializeObject(Deserialize(type, once));

        twice.Should().Be(once, "config must survive being read and written back unchanged");
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Builds a config payload for <paramref name="type"/>. Always sets the properties each type's
    /// computed getters read, so no getter falls through to a DeviceManager lookup, and seeds a
    /// stale value for every computed property.
    /// </summary>
    private static string BuildJson(Type type, bool includeExtras)
    {
        var o = new JObject { ["name"] = "Configured Name" };

        if (type == typeof(SourceListItem)) o["sourceKey"] = "laptop";
        else if (type == typeof(DestinationListItem)) o["sinkKey"] = "display1";
        else if (type == typeof(CameraListItem)) o["deviceKey"] = "camera1";
        else o["parentDeviceKey"] = "dsp1";   // LevelControlListItem / PresetListItem

        foreach (var name in MetadataTypes
                     .Where(row => (Type)row[0] == type)
                     .SelectMany(row => (string[])row[1]))
        {
            // CameraListItem's deviceKey is a real settable member, not a computed one; leave the
            // value set above rather than overwriting it with a marker.
            if (o.ContainsKey(name)) continue;
            o[name] = "STALE VALUE FROM CONFIG";
        }

        if (includeExtras)
        {
            o["uiGroup"] = "overflow";
            o["hasDeviceControls"] = true;
            o["nested"] = new JObject { ["a"] = 1, ["b"] = new JArray(1, 2, 3) };
        }

        return o.ToString();
    }

    private static object Deserialize(Type type, string json) =>
        JsonConvert.DeserializeObject(json, type)!;

    private static Dictionary<string, JToken>? GetCustomProperties(object item) =>
        (Dictionary<string, JToken>?)item.GetType()
            .GetProperty(nameof(SourceListItem.CustomProperties))!
            .GetValue(item);

    /// <summary>
    /// Property names at the root of the JSON object, in document order and including duplicates.
    /// <c>JObject.Parse</c> cannot be used here: it collapses duplicate keys, which is the exact
    /// condition under test.
    /// </summary>
    private static List<string> TopLevelPropertyNames(string json)
    {
        var names = new List<string>();
        using var reader = new JsonTextReader(new StringReader(json));

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName && reader.Depth == 1)
                names.Add((string)reader.Value!);
        }

        return names;
    }
}
