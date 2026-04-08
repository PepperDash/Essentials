using System.Runtime.CompilerServices;
using PepperDash.Core.Abstractions;
using PepperDash.Core.Tests.Fakes;

namespace PepperDash.Core.Tests;

/// <summary>
/// Runs once before any type in this assembly is accessed.
/// Registers fake Crestron service implementations with <see cref="DebugServiceRegistration"/>
/// so that the <c>Debug</c> static constructor uses them instead of the real Crestron SDK.
/// This must remain a module initializer (not a test fixture) because the static constructor
/// fires the first time <em>any</em> type in PepperDash.Core is referenced — before xUnit
/// has a chance to run fixture setup code.
/// </summary>
internal static class TestInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        DebugServiceRegistration.Register(
            new FakeCrestronEnvironment
            {
                DevicePlatform = DevicePlatform.Server, // avoids any appliance-only code paths
                RuntimeEnvironment = RuntimeEnvironment.Other, // skips console command registration
            },
            new NoOpCrestronConsole(),
            new InMemoryCrestronDataStore());
    }
}
