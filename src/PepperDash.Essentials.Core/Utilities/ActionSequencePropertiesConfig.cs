extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Utilities
{
    /// <summary>
    /// Configuration Properties for ActionSequence
    /// </summary>
    public class ActionSequencePropertiesConfig
    {
        [JsonProperty("actionSequence")]
        public List<SequencedDeviceActionWrapper> ActionSequence { get; set; }

        public ActionSequencePropertiesConfig()
        {
            ActionSequence = new List<SequencedDeviceActionWrapper>();
        }
    }
}