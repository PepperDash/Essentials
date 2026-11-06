using System;
using System.Runtime.CompilerServices;
using Crestron.SimplSharp;
using PdCore = PepperDash.Core.Abstractions;

namespace PepperDash.Essentials.Core.Queues;

/// <summary>
/// Tells the queues when the program is stopping, so they can shut their worker threads down.
/// </summary>
/// <remarks>
/// Uses the <see cref="PdCore.ICrestronEnvironment"/> registered with
/// <see cref="PdCore.DebugServiceRegistration"/> when there is one - the Essentials control system
/// registers the Crestron adapter before anything else runs, and tests register a fake. Only without
/// one does it use the Crestron SDK's <see cref="CrestronEnvironment"/>, whose type initializer needs
/// assemblies that only exist on a processor (issue #1553).
/// </remarks>
internal static class ProgramStop
{
    /// <summary>Calls <paramref name="onStopping"/> when the program starts stopping.</summary>
    public static void Subscribe(Action onStopping)
    {
        var environment = PdCore.DebugServiceRegistration.Environment;
        if (environment == null)
        {
            SubscribeToCrestronEnvironment(onStopping);
            return;
        }

        environment.ProgramStatusChanged += (_, args) =>
        {
            if (args.EventType == PdCore.ProgramStatusEventType.Stopping)
                onStopping();
        };
    }

    // Kept out of Subscribe, and never inlined, so compiling Subscribe doesn't load CrestronEnvironment.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SubscribeToCrestronEnvironment(Action onStopping)
    {
        CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
        {
            if (programEvent == eProgramStatusEventType.Stopping)
                onStopping();
        };
    }
}
