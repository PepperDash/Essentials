using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common;

/// <summary>
/// A <see cref="StatusMonitorBase"/> for mock devices, whose status is set directly rather than
/// derived from polling a real connection. Reports <see cref="MonitorStatus.IsOk"/> until told otherwise.
/// </summary>
public class MockCommunicationMonitor : StatusMonitorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockCommunicationMonitor"/> class, reporting online.
    /// </summary>
    public MockCommunicationMonitor(IKeyed parent) : base(parent, 30000, 60000)
    {
        Status = MonitorStatus.IsOk;
    }

    /// <inheritdoc />
    public override void Start() { }

    /// <inheritdoc />
    public override void Stop() { }

    /// <summary>
    /// Sets the monitor's current status.
    /// </summary>
    public void SetStatus(MonitorStatus status) => Status = status;
}
