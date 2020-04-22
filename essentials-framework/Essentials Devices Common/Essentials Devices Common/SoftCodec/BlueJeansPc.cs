using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    public class BlueJeansPc : InRoomPc, IRoutingInputs, IRunRouteAction, IRoutingSinkNoSwitching
    {

        public RoutingInputPort AnyVideoIn { get; private set; }

        #region IRoutingInputs Members

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        #endregion

        public BlueJeansPc(string key, string name)
            : base(key, name)
        {
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            InputPorts.Add(AnyVideoIn = new RoutingInputPort(RoutingPortNames.AnyVideoIn, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this));
        }

        #region IRunRouteAction Members

        public void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey, sourceListKey, null);
        }

        public void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
        {
            CrestronInvoke.BeginInvoke(o =>
                {
                    Debug.Console(1, this, "Run route action '{0}' on SourceList: {1}", routeKey, sourceListKey);

                    var dict = ConfigReader.ConfigObject.GetSourceListForKey(sourceListKey);
                    if (dict == null)
                    {
                        Debug.Console(1, this, "WARNING: Config source list '{0}' not found", sourceListKey);
                        return;
                    }

                    // Try to get the list item by it's string key
                    if (!dict.ContainsKey(routeKey))
                    {
                        Debug.Console(1, this, "WARNING: No item '{0}' found on config list '{1}'",
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
            IRoutingSinkNoSwitching dest = null;

            dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSinkNoSwitching;

            if (dest == null)
            {
                Debug.Console(1, this, "Cannot route, unknown destination '{0}'", route.DestinationKey);
                return false;
            }

            if (route.SourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
            {
                dest.ReleaseRoute();
                if (dest is IPower)
                    (dest as IPower).PowerOff();
            }
            else
            {
                var source = DeviceManager.GetDeviceForKey(route.SourceKey) as IRoutingOutputs;
                if (source == null)
                {
                    Debug.Console(1, this, "Cannot route unknown source '{0}' to {1}", route.SourceKey, route.DestinationKey);
                    return false;
                }
                dest.ReleaseAndMakeRoute(source, route.Type);
            }
            return true;
        }



        #region IHasCurrentSourceInfoChange Members

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

        public event SourceInfoChangeHandler CurrentSourceChange;

        #endregion
    }

    public class BlueJeansPcFactory : EssentialsDeviceFactory<BlueJeansPc>
    {
        public BlueJeansPcFactory()
        {
            TypeNames = new List<string>() { "bluejeanspc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BlueJeansPc Device");
            return new SoftCodec.BlueJeansPc(dc.Key, dc.Name);
        }
    }

}