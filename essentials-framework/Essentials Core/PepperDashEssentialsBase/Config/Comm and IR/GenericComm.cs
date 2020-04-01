using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core
{


    /// <summary>
    /// Serves as a generic wrapper class for all styles of IBasicCommuncation ports
    /// </summary>
    public class GenericComm : ReconfigurableDevice
    {

        EssentialsControlPropertiesConfig PropertiesConfig;

        public IBasicCommunication CommPort { get; private set; }

        public GenericComm(DeviceConfig config)
            : base(config)
        {
            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            CommPort = CommFactory.CreateCommForDevice(config);

                   
        }

        public static IKeyed BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }

        public void SetPortConfig(string portConfig)
        {
            // TODO: Deserialize new EssentialsControlPropertiesConfig and handle as necessary
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsControlPropertiesConfig>
                    (portConfig.ToString());
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error deserializing port config: {0}", e);
            }
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            ConfigWriter.UpdateDeviceConfig(config);
        }

        public class Factory : Essentials.Core.Factory
        {
            #region IDeviceFactory Members

            List<string> TypeNames = new List<string>() { "genericComm" };

            #endregion

            public override IKeyed BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
                return new GenericComm(dc);
            }
        }
     }


}