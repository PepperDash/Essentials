using System.Runtime.CompilerServices;
using PepperDash.Core.Abstractions;
using PepperDash.Essentials.Tests.Fakes;

namespace PepperDash.Essentials.Tests;

/// <summary>
/// Runs once before any type in this assembly is accessed.
/// Registers fake Crestron service implementations so that the <c>Debug</c> static
/// constructor never tries to reach the real Crestron SDK.
/// </summary>
internal static class TestInitializer
{
    /// <summary>The environment registered for every test in this assembly.</summary>
    internal static FakeCrestronEnvironment Environment { get; } = new()
    {
        DevicePlatform = DevicePlatform.Server,
        RuntimeEnvironment = RuntimeEnvironment.Other,
    };

    [ModuleInitializer]
    internal static void Initialize()
    {
        DebugServiceRegistration.Register(
            Environment,
            new NoOpCrestronConsole(),
            new InMemoryCrestronDataStore());
    }
}


