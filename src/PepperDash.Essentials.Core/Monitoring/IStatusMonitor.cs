using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface IStatusMonitor
    {
        IKeyed Parent { get; }
        event EventHandler<MonitorStatusChangeEventArgs> StatusChange;
        MonitorStatus Status { get; }
        string Message { get; }
        BoolFeedback IsOnlineFeedback { get; set; }
        void Start();
        void Stop();
    }
}