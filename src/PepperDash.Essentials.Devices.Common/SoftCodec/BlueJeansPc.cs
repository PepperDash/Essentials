using System;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.Sources;
using Serilog.Events;


namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    /// <summary>
    /// Represents a BlueJeansPc
    /// </summary>
    public class BlueJeansPc : InRoomPc, IRunRouteAction, IRoutingSink
    {

        /// <summary>
        /// Gets or sets the AnyVideoIn
        /// </summary>
        public RoutingInputPort AnyVideoIn { get; private set; }

        /// <summary>
        /// Gets the CurrentInputPort
        /// </summary>
        public RoutingInputPort CurrentInputPort => AnyVideoIn;

        #region IRoutingInputs Members

        /// <summary>
        /// Gets or sets the InputPorts
        /// </summary>
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BlueJeansPc"/> class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        public BlueJeansPc(string key, string name)
            : base(key, name)
        {
            InputPorts = new RoutingPortCollection<RoutingInputPort>
            {
                (AnyVideoIn = new RoutingInputPort(RoutingPortNames.AnyVideoIn, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this))
            };
        }

        #region IRunRouteAction Members

        /// <summary>
        /// RunRouteAction method
        /// </summary>
        public void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey, sourceListKey, null);
        }

        /// <summary>
        /// RunRouteAction method
        /// </summary>
        public void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
        {
            CrestronInvoke.BeginInvoke(o =>
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

        /// <summary>
        /// Gets or sets the CurrentSourceInfoKey
        /// </summary>
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

        /// <summary>
        /// Event fired when the current source changes
        /// </summary>
        public event SourceInfoChangeHandler CurrentSourceChange;

        #endregion
    }

}