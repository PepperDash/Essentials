using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement IRoutingMidpointWithFeedback
    /// </summary>
    public class IRoutingMidpointWithFeedbackMessenger : MessengerBase
    {
        private readonly IRoutingMidpointWithFeedback _device;

        public IRoutingMidpointWithFeedbackMessenger(string key, string messagePath, IRoutingMidpointWithFeedback device)
            : base(key, messagePath, device as IKeyName)
        {
            _device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/route", (id, content) =>
            {
                var request = content.ToObject<MidpointRouteRequest>();
                _device.ExecuteSwitch(request.InputSelector, request.OutputSelector, request.SignalType);
            });

            AddAction("/clearRoute", (id, content) =>
            {
                var request = content.ToObject<MidpointClearRouteRequest>();
                _device.ClearRoute(request.OutputSelector, request.SignalType);
            });

            _device.RouteChanged += OnRouteChanged;
        }

        private void OnRouteChanged(IRoutingMidpointWithFeedback midpoint, RouteSwitchDescriptor newRoute)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                currentRoutes = _device.CurrentRoutes.Select(r => new
                {
                    inputPort = r.InputPort?.Key,
                    outputPort = r.OutputPort?.Key
                })
            }));
        }

        private void SendFullStatus(string id = null)
        {
            var message = JToken.FromObject(new
            {
                inputPorts = _device.InputPorts.Select(p => new { key = p.Key }),
                outputPorts = _device.OutputPorts.Select(p => new { key = p.Key }),
                currentRoutes = _device.CurrentRoutes.Select(r => new
                {
                    inputPort = r.InputPort?.Key,
                    outputPort = r.OutputPort?.Key
                })
            });

            PostStatusMessage(message, id);
        }
    }

    public class MidpointRouteRequest
    {
        [JsonProperty("inputSelector")]
        public object InputSelector { get; set; }

        [JsonProperty("outputSelector")]
        public object OutputSelector { get; set; }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("signalType")]
        public eRoutingSignalType SignalType { get; set; }
    }

    public class MidpointClearRouteRequest
    {
        [JsonProperty("outputSelector")]
        public object OutputSelector { get; set; }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("signalType")]
        public eRoutingSignalType SignalType { get; set; }
    }
}
