using Newtonsoft.Json;

namespace PepperDash.Essentials.AppServer
{
    public class MobileControlSimpleContent<T>
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }
    }
}
