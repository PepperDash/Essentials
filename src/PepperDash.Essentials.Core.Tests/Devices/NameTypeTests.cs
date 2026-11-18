using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Devices;

public class NameTypeTests
{
    // Declared out of value order, so declaration order and Enum.GetNames' value order differ
    private enum Plain { Second = 2, First = 1, Third = 3 }

    [Flags]
    private enum Signals { Audio = 1, Video = 2, AudioVideo = Audio | Video, Usb = 8 }

    private class Device
    {
        public void Route(object input, Signals signalType) { }
        public void SetMode(Plain mode = Plain.Third) { }
        public void SetSignals(Signals signals = Signals.Audio | Signals.Usb) { }
        public void SetOptionalMode(Plain? mode = null) { }
        public void SetNullableWithDefault(Plain? mode = Plain.First) { }
        public void Optionals(int count = 5, string? label = "abc", string? none = null) { }
        public bool GetFlag() => true;
    }

    private static ParameterInfo Param(string method, int index) =>
        typeof(Device).GetMethod(method)!.GetParameters()[index];

    private static JObject Json(string method, int index) =>
        JObject.Parse(JsonConvert.SerializeObject(NameType.FromParameter(Param(method, index))));

    [Fact]
    public void NonEnumParameter_SerializesAsNameAndTypeOnly()
    {
        JsonConvert.SerializeObject(NameType.FromParameter(Param(nameof(Device.Route), 0)))
            .Should().Be("{\"Name\":\"input\",\"Type\":\"Object\"}");
    }

    [Fact]
    public void EnumParameter_ListsValuesInDeclarationOrder()
    {
        var json = Json(nameof(Device.SetMode), 0);

        json["IsEnum"]!.Value<bool>().Should().BeTrue();
        json["EnumValues"]!.ToObject<string[]>().Should().Equal("Second", "First", "Third");
        json.ContainsKey("IsFlags").Should().BeFalse();
    }

    [Fact]
    public void FlagsEnumParameter_IsFlags()
    {
        var json = Json(nameof(Device.Route), 1);

        json["Type"]!.Value<string>().Should().Be("Signals");
        json["IsFlags"]!.Value<bool>().Should().BeTrue();
        json["EnumValues"]!.ToObject<string[]>().Should().Equal("Audio", "Video", "AudioVideo", "Usb");
        json.ContainsKey("DefaultValue").Should().BeFalse();
    }

    [Fact]
    public void OptionalEnumParameter_DefaultIsTheName()
    {
        Json(nameof(Device.SetMode), 0)["DefaultValue"]!.Value<string>().Should().Be("Third");
    }

    [Fact]
    public void OptionalFlagsParameter_DefaultIsTheCombinedNames()
    {
        Json(nameof(Device.SetSignals), 0)["DefaultValue"]!.Value<string>().Should().Be("Audio, Usb");
    }

    [Fact]
    public void NullableEnumParameter_ReportsUnderlyingEnum_WithTypeUnchanged()
    {
        var json = Json(nameof(Device.SetOptionalMode), 0);

        json["Type"]!.Value<string>().Should().Be("Nullable`1");
        json["IsEnum"]!.Value<bool>().Should().BeTrue();
        json["EnumValues"]!.ToObject<string[]>().Should().Equal("Second", "First", "Third");
        json.ContainsKey("DefaultValue").Should().BeFalse();
    }

    [Fact]
    public void NullableEnumParameter_WithDefault_DefaultIsTheName()
    {
        Json(nameof(Device.SetNullableWithDefault), 0)["DefaultValue"]!.Value<string>().Should().Be("First");
    }

    [Fact]
    public void OptionalParameters_ListDefaultValue()
    {
        Json(nameof(Device.Optionals), 0)["DefaultValue"]!.Value<int>().Should().Be(5);
        Json(nameof(Device.Optionals), 1)["DefaultValue"]!.Value<string>().Should().Be("abc");
        Json(nameof(Device.Optionals), 2).ContainsKey("DefaultValue").Should().BeFalse();
    }

    [Fact]
    public void MethodNameParams_SerializesParamsThroughNameType()
    {
        var json = JArray.Parse(JsonConvert.SerializeObject(new[] { new MethodNameParams(typeof(Device).GetMethod(nameof(Device.Route))!) }));

        json[0]!["Name"]!.Value<string>().Should().Be("Route");
        json[0]!["Params"]![1]!["IsEnum"]!.Value<bool>().Should().BeTrue();
    }
}
