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
    [ModuleInitializer]
    internal static void Initialize()
    {
        DebugServiceRegistration.Register(
            new FakeCrestronEnvironment
            {
                DevicePlatform = DevicePlatform.Server,
                RuntimeEnvironment = RuntimeEnvironment.Other,
            },
            new NoOpCrestronConsole(),
            new InMemoryCrestronDataStore());
    }
}


