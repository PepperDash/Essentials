

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges.JoinMaps;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Represents a GenericIrController
    /// </summary>
    public class GenericIrController: EssentialsBridgeableDevice
    {
        //data storage for bridging
        private BasicTriList _trilist;
        private uint _joinStart;
        private string _joinMapKey;
        private EiscApiAdvanced _bridge;

        private readonly IrOutputPortController _port; 

        /// <summary>
        /// Gets or sets the IrCommands
        /// </summary>
        public string[] IrCommands {get { return _port.IrFileCommands; }}	    

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key for the device</param>
        /// <param name="name">name of the device</param>
        /// <param name="irPort">IR output port controller</param>
        public GenericIrController(string key, string name, IrOutputPortController irPort) : base(key, name)
        {
            _port = irPort;
            if (_port == null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "IR Port is null, device will not function");
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

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
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

	        if (_port.UseBridgeJoinMap)
	        {
				Debug.LogMessage(LogEventLevel.Information, this, "Using new IR bridge join map");

		        var bridgeJoins = joinMap.Joins.Where((kv) => _port.IrFileCommands.Any(cmd => cmd == kv.Key)).ToDictionary(kv => kv.Key);
		        if (bridgeJoins == null)
		        {
					Debug.LogMessage(LogEventLevel.Information, this, "Failed to link new IR bridge join map");
			        return;
		        }

				joinMap.Joins.Clear();

		        foreach (var bridgeJoin in bridgeJoins)
		        {
			        var key = bridgeJoin.Key;
			        var joinDataKey = bridgeJoin.Value.Key;
			        var joinDataValue = bridgeJoin.Value.Value;
			        var joinNumber = bridgeJoin.Value.Value.JoinNumber;					

					Debug.LogMessage(LogEventLevel.Verbose, this, @"bridgeJoin: Key-'{0}'
                                                                    Value.Key-'{1}'
                                                                    Value.JoinNumber-'{2}'
                                                                    Value.Metadata.Description-'{3}'", 
						key,
						joinDataKey,
						joinNumber,
						joinDataValue.Metadata.Description);


					joinMap.Joins.Add(key, joinDataValue);

			        trilist.SetBoolSigAction(joinNumber, (b) => Press(key, b));
		        }
	        }
	        else
	        {
				Debug.LogMessage(LogEventLevel.Information, this, "Using legacy IR join mapping based on available IR commands");

				joinMap.Joins.Clear();

				for (uint i = 0; i < _port.IrFileCommands.Length; i++)
				{
					var cmd = _port.IrFileCommands[i];
					var joinData = new JoinDataComplete(new JoinData { JoinNumber = i, JoinSpan = 1 },
						new JoinMetadata
						{
							Description = cmd,
							JoinCapabilities = eJoinCapabilities.FromSIMPL,
							JoinType = eJoinType.Digital
						});

					joinData.SetJoinOffset(joinStart);

					joinMap.Joins.Add(cmd, joinData);

					trilist.SetBoolSigAction(joinData.JoinNumber, (b) => Press(cmd, b));
				}   
	        }            

            joinMap.PrintJoinMapInfo();

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }
        }

        #endregion

        /// <summary>
        /// Press method
        /// </summary>
        public void Press(string command, bool pressRelease)
        {
            _port.PressRelease(command, pressRelease);
        }
    }

    /// <summary>
    /// Represents a GenericIrControllerFactory
    /// </summary>
    public class GenericIrControllerFactory : EssentialsDeviceFactory<GenericIrController>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GenericIrControllerFactory()
        {
            TypeNames = new List<string> {"genericIrController"};
        }
        #region Overrides of EssentialsDeviceFactory<GenericIRController>

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic IR Controller Device");

            var irPort = IRPortHelper.GetIrOutputPortController(dc);

            return new GenericIrController(dc.Key, dc.Name, irPort);
        }

        #endregion
    }
}