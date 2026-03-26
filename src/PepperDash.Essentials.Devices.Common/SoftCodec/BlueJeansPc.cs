using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Sources;
using Serilog.Events;


namespace PepperDash.Essentials.Devices.Common.SoftCodec;

/// <summary>
/// Class representing a BlueJeans soft codec running on an in-room PC.
/// </summary>
public class BlueJeansPc : InRoomPc, IRunRouteAction, IRoutingSink
{

    /// <summary>
    /// The input port for any video source.
    /// </summary>
    public RoutingInputPort AnyVideoIn { get; private set; }

    /// <summary>
    /// The currently active input port, which for this device is always AnyVideoIn
    /// This is used by the routing system to determine where to route video sources when this device is a destination
    /// </summary>
    public RoutingInputPort CurrentInputPort => AnyVideoIn;

    /// <inheritdoc/> 
    public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; private set; }

    /// <inheritdoc/>
    public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

    /// <inheritdoc />
    public event EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged;

    #region IRoutingInputs Members

    /// <summary>
    /// Collection of the input ports for this device
    /// </summary>
    public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueJeansPc"/> class.
    /// </summary>
    /// <param name="key">The key for the device.</param>
    /// <param name="name">The name of the device.</param>
    public BlueJeansPc(string key, string name)
        : base(key, name)
    {
        InputPorts = new RoutingPortCollection<RoutingInputPort>
        {
            (AnyVideoIn = new RoutingInputPort(RoutingPortNames.AnyVideoIn, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this))
        };

        CurrentSources = new Dictionary<eRoutingSignalType, IRoutingSource>
            {
                { eRoutingSignalType.Audio, null },
                { eRoutingSignalType.Video, null },
            };

        CurrentSourceKeys = new Dictionary<eRoutingSignalType, string>
            {
                { eRoutingSignalType.Audio, string.Empty },
                { eRoutingSignalType.Video, string.Empty },
            };
    }

    /// <inheritdoc />
    public virtual void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
    {
        foreach (eRoutingSignalType type in Enum.GetValues(typeof(eRoutingSignalType)))
        {
            var flagValue = Convert.ToInt32(type);
            // Skip if flagValue is 0 or not a power of two (i.e., not a single-bit flag).
            // (flagValue & (flagValue - 1)) != 0 checks if more than one bit is set.
            if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
            {
                this.LogDebug("Skipping {type}", type);
                continue;
            }

            this.LogDebug("setting {type}", type);

            var previousSource = CurrentSources[type];

            if (signalType.HasFlag(type))
            {
                UpdateCurrentSources(type, previousSource, sourceDevice);
            }
        }
    }

    private void UpdateCurrentSources(eRoutingSignalType signalType, IRoutingSource previousSource, IRoutingSource sourceDevice)
    {
        if (CurrentSources.ContainsKey(signalType))
        {
            CurrentSources[signalType] = sourceDevice;
        }
        else
        {
            CurrentSources.Add(signalType, sourceDevice);
        }

        // Update the current source key for the specified signal type
        if (CurrentSourceKeys.ContainsKey(signalType))
        {
            CurrentSourceKeys[signalType] = sourceDevice.Key;
        }
        else
        {
            CurrentSourceKeys.Add(signalType, sourceDevice.Key);
        }

        // Raise the CurrentSourcesChanged event
        CurrentSourcesChanged?.Invoke(this, new CurrentSourcesChangedEventArgs(signalType, previousSource, sourceDevice));
    }

    #region IRunRouteAction Members

    /// <summary>
    /// Runs a route action for the specified route key and source list key. Optionally, a callback can be provided to be executed upon successful completion.
    /// </summary>
    /// <param name="routeKey"></param>
    /// <param name="sourceListKey"></param>
    public void RunRouteAction(string routeKey, string sourceListKey)
    {
        RunRouteAction(routeKey, sourceListKey, null);
    }

    /// <summary>
    /// Runs a route action for the specified route key and source list key. Optionally, a callback can be provided to be executed upon successful completion.
    /// </summary>
    /// <param name="routeKey"></param>
    /// <param name="sourceListKey"></param>
    /// <param name="successCallback"></param>
    public void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
    {
        Task.Run(() =>
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Run route action '{0}' on SourceList: {1}", routeKey, sourceListKey);

                var dict = ConfigReader.ConfigObject.GetSourceListForKey(sourceListKey);
                if (dict == null)
                {
                    Debug.LogMessage(LogEventLevel.Debug, this, "WARNING: Config source list '{0}' not found", sourceListKey);
                    return;
                }

                // Try to get the list item by it's string key
                if (!dict.ContainsKey(routeKey))
                {
                    Debug.LogMessage(LogEventLevel.Debug, this, "WARNING: No item '{0}' found on config list '{1}'",
                        routeKey, sourceListKey);
                    return;
                }

                var item = dict[routeKey];

                foreach (var route in item.RouteList)
                {
                    DoRoute(route);
                }

                // store the name and UI info for routes
                if (item.SourceKey == "none")
                {
                    CurrentSourceInfoKey = routeKey;
                    CurrentSourceInfo = null;
                }
                else if (item.SourceKey != null)
                {
                    CurrentSourceInfoKey = routeKey;
                    CurrentSourceInfo = item;
                }

                // report back when done
                if (successCallback != null)
                    successCallback();
            });
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    bool DoRoute(SourceRouteListItem route)
    {
        IRoutingSink dest = null;

        dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSink;

        if (dest == null)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Cannot route, unknown destination '{0}'", route.DestinationKey);
            return false;
        }

        if (route.SourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
        {
            dest.ReleaseRoute();
            if (dest is IHasPowerControl)
                (dest as IHasPowerControl).PowerOff();
        }
        else
        {
            var source = DeviceManager.GetDeviceForKey(route.SourceKey) as IRoutingOutputs;
            if (source == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Cannot route unknown source '{0}' to {1}", route.SourceKey, route.DestinationKey);
                return false;
            }
            dest.ReleaseAndMakeRoute(source, route.Type);
        }
        return true;
    }



    #region IHasCurrentSourceInfoChange Members

    /// <inheritdoc />
    public string CurrentSourceInfoKey { get; set; }

    /// <summary>
    /// The SourceListItem last run - containing names and icons 
    /// </summary>
    public SourceListItem CurrentSourceInfo
    {
        get { return _CurrentSourceInfo; }
        set
        {
            if (value == _CurrentSourceInfo) return;

            var handler = CurrentSourceChange;
            // remove from in-use tracker, if so equipped
            if (_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
                (_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.RemoveUser(this, "control");

            if (handler != null)
                handler(_CurrentSourceInfo, ChangeType.WillChange);

            _CurrentSourceInfo = value;

            // add to in-use tracking
            if (_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
                (_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.AddUser(this, "control");
            if (handler != null)
                handler(_CurrentSourceInfo, ChangeType.DidChange);
        }
    }
    SourceListItem _CurrentSourceInfo;

    /// <inheritdoc />
    public event SourceInfoChangeHandler CurrentSourceChange;

    #endregion
}
