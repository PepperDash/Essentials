namespace PepperDash.Core.Abstractions;

/// <summary>
/// Allows pre-registration of Crestron service implementations before the <c>Debug</c>
/// static class initialises. Call <see cref="Register"/> from the composition root
/// (e.g. ControlSystem constructor) <em>before</em> any code touches <c>Debug.*</c>.
/// Test projects should call it with no-op / in-memory implementations so that the
/// <c>Debug</c> static constructor never tries to reach the real Crestron SDK.
/// </summary>
public static class DebugServiceRegistration
{
    /// <summary>Gets the registered environment abstraction, or <c>null</c> if not registered.</summary>
    public static ICrestronEnvironment? Environment { get; private set; }

    /// <summary>Gets the registered console abstraction, or <c>null</c> if not registered.</summary>
    public static ICrestronConsole? Console { get; private set; }

    /// <summary>Gets the registered data-store abstraction, or <c>null</c> if not registered.</summary>
    public static ICrestronDataStore? DataStore { get; private set; }

    /// <summary>
    /// Registers the service implementations that <c>Debug</c> will use when its
    /// static constructor runs. Any parameter may be <c>null</c> to leave the
    /// corresponding service unregistered (the <c>Debug</c> class will skip that
    /// capability gracefully).
    /// </summary>
    public static void Register(
        ICrestronEnvironment? environment,
        ICrestronConsole? console,
        ICrestronDataStore? dataStore)
    {
        Environment = environment;
        Console = console;
        DataStore = dataStore;
    }
}
