using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Displays
{
  /// <summary>
  /// Abstract base class for two-way display devices that provide feedback capabilities.
  /// Extends DisplayBase with routing feedback and power control feedback functionality.
  /// </summary>
  public abstract class TwoWayDisplayBase : DisplayBase, IRoutingFeedback, IHasPowerControlWithFeedback
  {
    /// <summary>
    /// Gets feedback for the current input selection on the display.
    /// </summary>
    public StringFeedback CurrentInputFeedback { get; private set; }

    /// <summary>
    /// Abstract function that must be implemented by derived classes to provide the current input feedback value.
    /// Must be implemented by concrete sub-classes.
    /// </summary>
    abstract protected Func<string> CurrentInputFeedbackFunc { get; }

    /// <summary>
    /// Gets feedback indicating whether the display is currently powered on.
    /// </summary>
    public BoolFeedback PowerIsOnFeedback { get; protected set; }

    /// <summary>
    /// Abstract function that must be implemented by derived classes to provide the power state feedback value.
    /// Must be implemented by concrete sub-classes.
    /// </summary>
    abstract protected Func<bool> PowerIsOnFeedbackFunc { get; }

    /// <summary>
    /// Gets the default mock display instance for testing and development purposes.
    /// </summary>
    public static MockDisplay DefaultDisplay
    {
      get
      {
        if (_DefaultDisplay == null)
          _DefaultDisplay = new MockDisplay("default", "Default Display");
        return _DefaultDisplay;
      }
    }
    static MockDisplay _DefaultDisplay;

    /// <summary>
    /// Initializes a new instance of the TwoWayDisplayBase class.
    /// </summary>
    /// <param name="key">The unique key identifier for this display device.</param>
    /// <param name="name">The friendly name for this display device.</param>
    public TwoWayDisplayBase(string key, string name)
      : base(key, name)
    {
      CurrentInputFeedback = new StringFeedback("currentInput", CurrentInputFeedbackFunc);

      WarmupTime = 7000;
      CooldownTime = 15000;

      PowerIsOnFeedback = new BoolFeedback("PowerOnFeedback", PowerIsOnFeedbackFunc);

      Feedbacks.Add(CurrentInputFeedback);
      Feedbacks.Add(PowerIsOnFeedback);

      PowerIsOnFeedback.OutputChange += PowerIsOnFeedback_OutputChange;

    }

    void PowerIsOnFeedback_OutputChange(object sender, EventArgs e)
    {
      if (UsageTracker != null)
      {
        if (PowerIsOnFeedback.BoolValue)
          UsageTracker.StartDeviceUsage();
        else
          UsageTracker.EndDeviceUsage();
      }
    }

    /// <summary>
    /// Event that is raised when a numeric switch change occurs on the display.
    /// </summary>
    public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

    /// <summary>
    /// Raise an event when the status of a switch object changes.
    /// </summary>
    /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
    protected void OnSwitchChange(RoutingNumericEventArgs e)
    {
      NumericSwitchChange?.Invoke(this, e);
    }
  }
}