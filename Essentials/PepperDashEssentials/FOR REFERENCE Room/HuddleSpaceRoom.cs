//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials
//{
//    //***************************************************************************************************

//    public class HuddleSpaceRoom : EssentialsRoom
//    {
//        public override BoolFeedback RoomIsOnFeedback { get; protected set; }
//        public override BoolFeedback IsWarmingUpFeedback { get; protected set; }
//        public override BoolFeedback IsCoolingDownFeedback { get; protected set; }
//        public override BoolFeedback RoomIsOnStandby { get; protected set; }
//        public override BoolFeedback RoomIsOccupied { get; protected set; }

//        public override uint WarmupTime
//        {
//            get
//            {
//                if (_Display != null)
//                    return _Display.WarmupTime;
//                return base.WarmupTime;
//            }
//        }
//        public override uint CooldownTime
//        {
//            get
//            {
//                if (_Display != null)
//                    return _Display.CooldownTime;
//                return base.CooldownTime;
//            }
//        }

//        bool NoDisplayRoomIsOn = false;

//        public DisplayBase DefaultDisplay // PROTECT ---------------------------------------------
//        {
//            get
//            {
//                if (_Display == null)
//                    _Display = TwoWayDisplayBase.DefaultDisplay;
//                return _Display;
//            }
//            set
//            {
//                // Disconnect current display
//                if (_Display != null)
//                {
//                    _Display.PowerIsOnFeedback.OutputChange -= Display_Various_OutputChange;
//                    _Display.IsCoolingDownFeedback.OutputChange -= Display_Various_OutputChange;
//                    _Display.IsWarmingUpFeedback.OutputChange -= Display_Various_OutputChange;
//                }
//                _Display = value;
//                if (value != null)
//                {
//                    _Display.PowerIsOnFeedback.OutputChange += Display_Various_OutputChange;
//                    _Display.IsCoolingDownFeedback.OutputChange += Display_Various_OutputChange;
//                    _Display.IsWarmingUpFeedback.OutputChange += Display_Various_OutputChange;
//                }
//                CurrentAudioDevice = (value as IBasicVolumeControls);
//            }
//        }
//        DisplayBase _Display;

//        public IBasicVolumeControls DefaultVolumeControls
//        {
//            get { return DefaultDisplay as IBasicVolumeControls; }
//        }


//        public IntFeedback SourcesCount { get; private set; }
//        public StringFeedback CurrentSourceName { get; private set; }

//        public string SourceListKey { get; set; }
//        string LastSourceKey;

//        public HuddleSpaceRoom(string key, string name)
//            : base(key, name)
//        {
//            // Return the state of the display, unless no display, then return
//            // a local or default state.

//            RoomIsOnFeedback = new BoolFeedback(RoomCue.RoomIsOn,
//                () => DefaultDisplay != null ? DefaultDisplay.PowerIsOnFeedback.BoolValue : NoDisplayRoomIsOn);
//            IsWarmingUpFeedback = new BoolFeedback(RoomCue.RoomIsWarmingUp,
//                () => DefaultDisplay != null ? DefaultDisplay.IsWarmingUpFeedback.BoolValue : false);
//            IsCoolingDownFeedback = new BoolFeedback(RoomCue.RoomIsCoolingDown,
//                () => DefaultDisplay != null ? DefaultDisplay.IsCoolingDownFeedback.BoolValue : false);
//            RoomIsOnStandby = new BoolFeedback(RoomCue.RoomIsOnStandby,
//                () => false);
//            RoomIsOccupied = new BoolFeedback(RoomCue.RoomIsOccupied,
//                () => true);

//            Sources = new Dictionary<uint, Device>();

//            SourcesCount = new IntFeedback(RoomCue.SourcesCount,
//                () => Sources.Count);
//            CurrentSourceName = new StringFeedback(() => CurrentPresentationSourceInfo.PreferredName);// CurrentPresentationSource.Name);
//            //CurrentSourceType = new IntOutput(RoomCue.CurrentSourceType, 
//            //    () => CurrentPresentationSource.Type);

//            UnattendedShutdownTimeMs = 60000;
//        }

//        public override void RoomOn()
//        {
//            Debug.Console(0, this, "Room on");
//            if (IsCoolingDownFeedback.BoolValue || IsWarmingUpFeedback.BoolValue)
//            {
//                Debug.Console(2, this, "Room is warming or cooling. Ignoring room on");
//                return;
//            }
//            if (!RoomIsOnFeedback.BoolValue)
//            {
//                // Setup callback when powerOn happens
//                EventHandler<EventArgs> oneTimeHandler = null;
//                oneTimeHandler = (o, a) =>
//                {
//                    Debug.Console(0, this, "RoomOn received display power on: {0}", 
//                        DefaultDisplay.PowerIsOnFeedback.BoolValue);
//                    // if it's power on
//                    if (DefaultDisplay.PowerIsOnFeedback.BoolValue)
//                    {
//                        (DefaultDisplay as TwoWayDisplayBase).PowerIsOnFeedback.OutputChange -= oneTimeHandler;
//                        Debug.Console(1, this, "Display has powered on");
//                        RoomIsOnFeedback.FireUpdate();
//                        //if (callback != null)
//                        //    callback();
//                    }
//                };
//                DefaultDisplay.PowerIsOnFeedback.OutputChange += oneTimeHandler;
//                DefaultDisplay.PowerOn();
//            }
//        }

//        public override void RoomOff()
//        {
//            if (!RoomIsOnFeedback.BoolValue)
//            {
//                Debug.Console(2, this, "Room is already off");
//                return;
//            }
//            Debug.Console(0, this, "Room off");
//            DefaultDisplay.PowerOff();

//            //RoomIsOn.FireUpdate(); ---Display will provide this in huddle
//            //OnPresentationSourceChange(null);
//            //CurrentPresentationSource = null;
//        }


//        public override void SelectSource(uint sourceNum)
//        {
//            // Run this on a separate thread
//            new CTimer(o =>
//            {
//                var routeKey = "source-" + sourceNum;
//                Debug.Console(1, this, "Run room action '{0}'", routeKey);
//                if (string.IsNullOrEmpty(SourceListKey))
//                {
//                    Debug.Console(1, this, "WARNING: No source/action list defined for this room");
//                    return;
//                }

//                // Try to get the list from the config object, using SourceListKey string
//                if (!ConfigReader.ConfigObject.SourceLists.ContainsKey(SourceListKey))
//                {
//                    Debug.Console(1, this, "WARNING: Config source list '{0}' not found", SourceListKey);
//                    return;
//                }
//                var list = ConfigReader.ConfigObject.SourceLists[SourceListKey];

//                // Try to get the list item by it's string key
//                if (!list.ContainsKey(routeKey))
//                {
//                    Debug.Console(1, this, "WARNING: No item '{0}' found on config list '{1}'",
//                        routeKey, SourceListKey);
//                    return;
//                }

//                var item = list[routeKey];
//                Debug.Console(2, this, "Action {0} has {1} steps",
//                    item.SourceKey, item.RouteList.Count);

//                // Let's run it
//                if (routeKey.ToLower() != "roomoff")
//                    LastSourceKey = routeKey;

//                foreach (var route in item.RouteList)
//                {
//                    // if there is a $defaultAll on route, run two separate
//                    if (route.DestinationKey.Equals("$defaultAll", StringComparison.OrdinalIgnoreCase))
//                    {
//                        var tempAudio = new SourceRouteListItem
//                        {
//                            DestinationKey = "$defaultDisplay",
//                            SourceKey = route.SourceKey,
//                            Type = eRoutingSignalType.Video
//                        };
//                        DoRoute(tempAudio);

//                        var tempVideo = new SourceRouteListItem
//                        {
//                            DestinationKey = "$defaultAudio",
//                            SourceKey = route.SourceKey,
//                            Type = eRoutingSignalType.Audio
//                        };
//                        DoRoute(tempVideo);
//                        continue;
//                    }
//                    else
//                        DoRoute(route);
//                }

//                // Set volume control on room, using default if non provided
//                IBasicVolumeControls volDev = null;
//                // Handle special cases for volume control
//                if (string.IsNullOrEmpty(item.VolumeControlKey)
//                    || item.VolumeControlKey.Equals("$defaultAudio", StringComparison.OrdinalIgnoreCase))
//                    volDev = DefaultVolumeControls;
//                else if (item.VolumeControlKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
//                    volDev = DefaultDisplay as IBasicVolumeControls;
//                // Or a specific device, probably rarely used.
//                else
//                {
//                    var dev = DeviceManager.GetDeviceForKey(item.VolumeControlKey);
//                    if (dev is IBasicVolumeControls)
//                        volDev = dev as IBasicVolumeControls;
//                    else if (dev is IHasVolumeDevice)
//                        volDev = (dev as IHasVolumeDevice).VolumeDevice;
//                }
//                CurrentAudioDevice = volDev;

//                // store the name and UI info for routes
//                if (item.SourceKey != null)
//                    CurrentPresentationSourceInfo = item;
//                // And finally, set the "control".  This will trigger event
//                //CurrentControlDevice = DeviceManager.GetDeviceForKey(item.SourceKey) as Device;

//                RoomIsOnFeedback.FireUpdate();

//            }, 0); // end of CTimer

//            //Debug.Console(1, this, "Checking for source {0}", sourceNum);
//            //if (Sources.ContainsKey(sourceNum))
//            //{
//            //    var newSrc = Sources[sourceNum];
//            //    if (!RoomIsOn.BoolValue)
//            //    {
//            //        EventHandler<EventArgs> oneTimeHandler = null;
//            //        oneTimeHandler = (o, a) =>
//            //        {
//            //            RoomIsOn.OutputChange -= oneTimeHandler;
//            //            FinishSourceSelection(newSrc);
//            //        };
//            //        RoomIsOn.OutputChange -= oneTimeHandler;
//            //        RoomIsOn.OutputChange += oneTimeHandler;
//            //        RoomOn();
//            //    }
//            //    else FinishSourceSelection(newSrc);
//            //}
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="route"></param>
//        /// <returns></returns>
//        bool DoRoute(SourceRouteListItem route)
//        {
//            IRoutingSinkNoSwitching dest = null;

//            if (route.DestinationKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
//                dest = DefaultDisplay;
//            else
//                dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSinkNoSwitching;

//            if (dest == null)
//            {
//                Debug.Console(1, this, "Cannot route, unknown destination '{0}'", route.DestinationKey);
//                return false;
//            }

//            if (route.SourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
//            {
//                dest.ReleaseRoute();
//                if (dest is IPower)
//                    (dest as IPower).PowerOff();
//            }
//            else
//            {
//                var source = DeviceManager.GetDeviceForKey(route.SourceKey) as IRoutingOutputs;
//                if (source == null)
//                {
//                    Debug.Console(1, this, "Cannot route unknown source '{0}' to {1}", route.SourceKey, route.DestinationKey);
//                    return false;
//                }
//                dest.ReleaseAndMakeRoute(source, route.Type);
//            }
//            return true;
//        }

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        ///// <param name="newSrc"></param>
//        //public override void SelectSource(IPresentationSource newSrc)
//        //{
//        //    if (Sources.ContainsValue(newSrc))
//        //    {
//        //        if (!RoomIsOn.BoolValue)
//        //        {
//        //            EventHandler<EventArgs> oneTimeHandler = null;
//        //            oneTimeHandler = (o, a) =>
//        //                {
//        //                    RoomIsOn.OutputChange -= oneTimeHandler;
//        //                    FinishSourceSelection(newSrc);
//        //                };
//        //            RoomIsOn.OutputChange -= oneTimeHandler;
//        //            RoomIsOn.OutputChange += oneTimeHandler;
//        //            RoomOn();
//        //        }
//        //        else FinishSourceSelection(newSrc);
//        //    }
//        //}

//        void Display_Various_OutputChange(object sender, EventArgs e)
//        {
//            // Bounce through the output changes
//            if (sender == DefaultDisplay.PowerIsOnFeedback)
//                this.RoomIsOnFeedback.FireUpdate();
//            else if (sender == DefaultDisplay.IsCoolingDownFeedback)
//                this.IsCoolingDownFeedback.FireUpdate();
//            else if (sender == DefaultDisplay.IsWarmingUpFeedback)
//                this.IsWarmingUpFeedback.FireUpdate();
//        }

////        void FinishSourceSelection(IPresentationSource newSource)
////        {
////            Debug.Console(1, this, "Selecting source {0}", newSource.Key);
////            // Notify anyone watching source that it's leaving
////            OnPresentationSourceChange(newSource);
////            CurrentPresentationSource = newSource;
////            var routeableSource = CurrentPresentationSource as IRoutingOutputs;
////            if (routeableSource != null)
////#warning source route type will need clarification
////                DefaultDisplay.GetRouteToSource(routeableSource, eRoutingSignalType.AudioVideo);
////            else
////                Debug.Console(1, this, "New selected source {0} is not routeable", CurrentPresentationSource);

////            CurrentSourceName.FireUpdate();
////            //CurrentSourceType.FireUpdate();
////        }

//        public override List<Feedback> Feedbacks
//        {
//            get
//            {
//                var feedbacks = new List<Feedback>
//                {
//                    SourcesCount,
//                    CurrentSourceName,
//                    //CurrentSourceType,
//                };
//                feedbacks.AddRange(base.Feedbacks);
//                return feedbacks;
//            }
//        }
//    }
//}