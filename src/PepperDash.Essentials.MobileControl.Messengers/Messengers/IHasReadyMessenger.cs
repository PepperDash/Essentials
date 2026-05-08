using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasReady"/>
    /// </summary>
    public class IHasReadyMessenger : MessengerBase
    {
        private readonly IHasReady _hasReady;

        public IHasReadyMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _hasReady = device as IHasReady ?? throw new ArgumentNullException(nameof(device));
            _hasReady.IsReadyEvent += HasReady_IsReadyEvent;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/isReady", (id, content) => SendFullStatus(id));
            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
        }

        private void HasReady_IsReadyEvent(object sender, IsReadyEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasReadyStateMessage
                {
                    IsReady = e.IsReady
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting ready state");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasReadyStateMessage
                {
                    IsReady = _hasReady.IsReady
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending ready full status");
            }
        }
    }

    public class IHasReadyStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("isReady", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReady { get; set; }
    }
}
