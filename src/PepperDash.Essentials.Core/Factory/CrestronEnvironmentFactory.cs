using System;
using PepperDash.Essentials.Core.Abstractions;

namespace PepperDash.Essentials.Core.Factory
{
    /// <summary>
    /// Factory for creating Crestron environment dependencies
    /// Allows switching between real Crestron libraries and mock implementations
    /// </summary>
    public static class CrestronEnvironmentFactory
    {
        private static ICrestronEnvironmentProvider _provider;
        private static bool _isTestMode = false;

        static CrestronEnvironmentFactory()
        {
            // Default to runtime provider
            _provider = new CrestronRuntimeProvider();
        }

        /// <summary>
        /// Enables test mode with mock implementations
        /// </summary>
        public static void EnableTestMode()
        {
            _isTestMode = true;
            _provider = new CrestronMockProvider();
        }

        /// <summary>
        /// Disables test mode and returns to runtime implementations
        /// </summary>
        public static void DisableTestMode()
        {
            _isTestMode = false;
            _provider = new CrestronRuntimeProvider();
        }

        /// <summary>
        /// Sets a custom provider for Crestron environment
        /// </summary>
        public static void SetProvider(ICrestronEnvironmentProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Gets whether the factory is in test mode
        /// </summary>
        public static bool IsTestMode => _isTestMode;

        /// <summary>
        /// Gets the current control system instance
        /// </summary>
        public static ICrestronControlSystem GetControlSystem()
        {
            return _provider.GetControlSystem();
        }

        /// <summary>
        /// Creates a relay port
        /// </summary>
        public static IRelayPort CreateRelayPort(uint portNumber)
        {
            return _provider.CreateRelayPort(portNumber);
        }

        /// <summary>
        /// Creates a digital input
        /// </summary>
        public static IDigitalInput CreateDigitalInput(uint portNumber)
        {
            return _provider.CreateDigitalInput(portNumber);
        }

        /// <summary>
        /// Creates a versiport
        /// </summary>
        public static IVersiPort CreateVersiPort(uint portNumber)
        {
            return _provider.CreateVersiPort(portNumber);
        }

        /// <summary>
        /// Gets console manager for debugging
        /// </summary>
        public static IConsoleManager GetConsoleManager()
        {
            return _provider.GetConsoleManager();
        }

        /// <summary>
        /// Gets system information
        /// </summary>
        public static ISystemInfo GetSystemInfo()
        {
            return _provider.GetSystemInfo();
        }
    }

    /// <summary>
    /// Provider interface for Crestron environment dependencies
    /// </summary>
    public interface ICrestronEnvironmentProvider
    {
        ICrestronControlSystem GetControlSystem();
        IRelayPort CreateRelayPort(uint portNumber);
        IDigitalInput CreateDigitalInput(uint portNumber);
        IVersiPort CreateVersiPort(uint portNumber);
        IConsoleManager GetConsoleManager();
        ISystemInfo GetSystemInfo();
    }

    /// <summary>
    /// Console manager abstraction
    /// </summary>
    public interface IConsoleManager
    {
        void Print(string message);
        void PrintLine(string message);
        void RegisterCommand(string command, Action<string> handler, string help);
    }

    /// <summary>
    /// System information abstraction
    /// </summary>
    public interface ISystemInfo
    {
        string ProgramName { get; }
        string SerialNumber { get; }
        string MacAddress { get; }
        string IpAddress { get; }
        string FirmwareVersion { get; }
        DateTime SystemUpTime { get; }
    }
}