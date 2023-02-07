extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Full.Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    public class GenericIrController: EssentialsBridgeableDevice
    {
        //data storage for bridging
        private BasicTriList _trilist;
        private uint _joinStart;
        private string _joinMapKey;
        private EiscApiAdvanced _bridge;

        private readonly IrOutputPortController _port; 

        public string[] IrCommands {get { return _port.IrFileCommands; }}

        public GenericIrController(string key, string name, IrOutputPortController irPort) : base(key, name)
        {
            _port = irPort;

            if (_port == null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "IR Port is null, device will not function");
                return;
            }
            DeviceManager.AddDevice(_port);

            _port.DriverLoaded.OutputChange += DriverLoadedOnOutputChange;
        }

        private void DriverLoadedOnOutputChange(object sender, FeedbackEventArgs args)
        {
            if (!args.BoolValue)
            {
                return;
            }

            if (_trilist == null || _bridge == null)
            {
                return;
            }

            LinkToApi(_trilist, _joinStart, _joinMapKey, _bridge);
        }

        #region Overrides of EssentialsBridgeableDevice

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            //if driver isn't loaded yet, store the variables until it is loaded, then call the LinkToApi method again
            if (!_port.DriverIsLoaded)
            {
                _trilist = trilist;
                _joinStart = joinStart;
                _joinMapKey = joinMapKey;
                _bridge = bridge;
                return;
            }

            var joinMap = new GenericIrControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GenericIrControllerJoinMap>(joinMapSerialized);

            for (uint i = 0; i < _port.IrFileCommands.Length; i++)
            {
                var cmd = _port.IrFileCommands[i];
                var joinData = new JoinDataComplete(new JoinData {JoinNumber = i, JoinSpan = 1},
                    new JoinMetadata
                    {
                        Description = cmd,
                        JoinCapabilities = eJoinCapabilities.FromSIMPL,
                        JoinType = eJoinType.Digital
                    });

                joinData.SetJoinOffset(joinStart);

                joinMap.Joins.Add(cmd,joinData);

                trilist.SetBoolSigAction(joinData.JoinNumber, (b) => Press(cmd, b));
            }

            joinMap.PrintJoinMapInfo();

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }
        }

        #endregion

        public void Press(string command, bool pressRelease)
        {
            _port.PressRelease(command, pressRelease);
        }
    }

    public sealed class GenericIrControllerJoinMap : JoinMapBaseAdvanced
    {
        public GenericIrControllerJoinMap(uint joinStart) : base(joinStart)
        {
        }
    }

    public class GenericIrControllerFactory : EssentialsDeviceFactory<GenericIrController>
    {
        public GenericIrControllerFactory()
        {
            TypeNames = new List<string> {"genericIrController"};
        }
        #region Overrides of EssentialsDeviceFactory<GenericIRController>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic IR Controller Device");

            var irPort = IRPortHelper.GetIrOutputPortController(dc);

            return new GenericIrController(dc.Key, dc.Name, irPort);
        }

        #endregion
    }
}