using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Devices;

public class DeviceMethodResultTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    private class Device
    {
        public bool GetFlag() => true;
        public string GetName() => "avr";
        public object GetStatus() => new { power = true, input = "HDMI 1" };
        public void DoNothing() { }
        public int Fail() => throw new InvalidOperationException("no power");
        public async Task<int> GetCountAsync() { await Task.Delay(10); return 42; }
        public async Task RunAsync() { await Task.Delay(10); }
        public async Task<int> FailAsync() { await Task.Delay(10); throw new InvalidOperationException("async no power"); }
        public Task<string> CompletedTask() => Task.FromResult("done");
        public int Slow() { Thread.Sleep(3000); return 1; }
    }

    private static DeviceMethodResult Call(string methodName, TimeSpan? timeout = null)
    {
        var device = new Device();
        var method = typeof(Device).GetMethod(methodName)!;
        var work = Task.Run(() => DeviceMethodResult.InvokeAsync(() => method.Invoke(device, Array.Empty<object>())!, method.ReturnType));

        return DeviceMethodResult.Wait(work, timeout ?? Timeout);
    }

    [Theory]
    [InlineData(nameof(Device.GetFlag), "Result: true")]
    [InlineData(nameof(Device.GetName), "Result: \"avr\"")]
    [InlineData(nameof(Device.GetStatus), "Result: {\"power\":true,\"input\":\"HDMI 1\"}")]
    [InlineData(nameof(Device.GetCountAsync), "Result: 42")]
    [InlineData(nameof(Device.CompletedTask), "Result: \"done\"")]
    [InlineData(nameof(Device.RunAsync), "Result: null")]
    public void ConsoleLine_ShowsTheValueAsJson(string methodName, string expected)
    {
        Call(methodName).ToConsoleLine().Should().Be(expected);
    }

    [Theory]
    [InlineData(nameof(Device.Fail), "Result error: no power")]
    [InlineData(nameof(Device.FailAsync), "Result error: async no power")]
    public void ConsoleLine_ShowsTheMethodsOwnError(string methodName, string expected)
    {
        Call(methodName).ToConsoleLine().Should().Be(expected);
    }

    [Fact]
    public void Error_IsTheMethodsException_NotTheReflectionWrapper()
    {
        Call(nameof(Device.Fail)).Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void SlowMethod_StopsWaitingAtTheTimeout()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var result = Call(nameof(Device.Slow), TimeSpan.FromMilliseconds(200));

        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        result.Completed.Should().BeFalse();
        result.ToConsoleLine().Should().Be("Result: (still running after 0.2 s)");
    }

    [Fact]
    public void DefaultTimeout_IsTwoSeconds_AndSaysSo()
    {
        DeviceMethodResult.DefaultTimeout.Should().Be(TimeSpan.FromSeconds(2));

        var neverFinishes = new TaskCompletionSource<object>().Task;
        DeviceMethodResult.Wait(neverFinishes, TimeSpan.FromMilliseconds(10)).ToConsoleLine()
            .Should().Be("Result: (still running after 0.01 s)");
    }

    [Fact]
    public void Json_WrapsTheValue()
    {
        Call(nameof(Device.GetStatus)).ToJson().Should().Be("{\"result\":{\"power\":true,\"input\":\"HDMI 1\"}}");
        Call(nameof(Device.RunAsync)).ToJson().Should().Be("{\"result\":null}");
    }

    [Fact]
    public void Json_ReportsTheError()
    {
        Call(nameof(Device.Fail)).ToJson().Should().Be("{\"error\":\"no power\"}");
    }

    [Fact]
    public void Json_ReportsTheTimeout()
    {
        Call(nameof(Device.Slow), TimeSpan.FromMilliseconds(100)).ToJson()
            .Should().Be("{\"error\":\"still running after 0.1 s\"}");
    }

    private class Loop
    {
        public Loop Self => this;
    }

    [Fact]
    public void UnserializableValue_IsReportedAsAnError()
    {
        var work = Task.FromResult<object>(new Loop());
        var result = DeviceMethodResult.Wait(work, Timeout);

        result.ToConsoleLine().Should().StartWith("Result error: Unable to serialize the result:");
        JsonConvert.DeserializeObject<Dictionary<string, string>>(result.ToJson())!.Should().ContainKey("error");
    }

    [Fact]
    public void ReturnTypeName_IsListedForEachMethod()
    {
        string ReturnType(string name) => new MethodNameParams(typeof(Device).GetMethod(name)!).ReturnType;

        ReturnType(nameof(Device.DoNothing)).Should().Be("Void");
        ReturnType(nameof(Device.GetFlag)).Should().Be("Boolean");
        ReturnType(nameof(Device.GetName)).Should().Be("String");
        ReturnType(nameof(Device.GetCountAsync)).Should().Be("Task`1");
        ReturnType(nameof(Device.RunAsync)).Should().Be("Task");
    }

    [Fact]
    public void MethodNameParams_SerializesReturnType()
    {
        JsonConvert.SerializeObject(new MethodNameParams(typeof(Device).GetMethod(nameof(Device.GetFlag))!))
            .Should().Be("{\"Name\":\"GetFlag\",\"Params\":[],\"ReturnType\":\"Boolean\"}");
    }
}
