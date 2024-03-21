using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    public class GenericSoftCodec : EssentialsDevice, IRoutingInputsOutputs
    {
        public GenericSoftCodec(string key, string name, GenericSoftCodecProperties props) : base(key, name)
        {
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            for(var i = 1; i <= props.OutputCount; i++)
            {
                var outputPort = new RoutingOutputPort($"{Key}-output{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

                OutputPorts.Add(outputPort);
            }

            for(var i = 1; i<= props.ContentInputCount; i++)
            {
                var inputPort = new RoutingInputPort($"{Key}-contentInput{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

                InputPorts.Add(inputPort);
            }

            if (!props.HasCameraInputs)
            {
                return;
            }

            for(var i = 1; i <=props.CameraInputCount; i++)
            {
                var cameraPort = new RoutingInputPort($"{Key}-cameraInput{i}", eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null, this);

                InputPorts.Add(cameraPort);
            }
        }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }
    }

    public class GenericSoftCodecProperties
    {
        [JsonProperty("hasCameraInputs")]
        public bool HasCameraInputs { get; set; }

        [JsonProperty("cameraInputCount")]
        public int CameraInputCount { get; set; }

        [JsonProperty("contentInputCount")]
        public int ContentInputCount { get; set; }

        [JsonProperty("contentOutputCount")]
        public int OutputCount { get; set; }
    }

    public class GenericSoftCodecFactory: EssentialsDeviceFactory<GenericSoftCodec>
    {
        public GenericSoftCodecFactory()
        {
            TypeNames = new List<string> { "genericsoftcodec" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Attempting to create new Generic SoftCodec Device");

            var props = dc.Properties.ToObject<GenericSoftCodecProperties>();

            return new GenericSoftCodec(dc.Key, dc.Name, props);
        }
    }
}
