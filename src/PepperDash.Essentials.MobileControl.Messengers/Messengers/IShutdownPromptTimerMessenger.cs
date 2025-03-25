using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IShutdownPromptTimerMessenger : MessengerBase
    {
        private readonly IShutdownPromptTimer _room;

        public IShutdownPromptTimerMessenger(string key, string messagePath, IShutdownPromptTimer room)
            : base(key, messagePath, room as Device)
        {
            _room = room;
        }

        protected override void RegisterActions()
        {
            AddAction("/status", (id, content) =>
            {
                SendFullStatus();
            });

            AddAction("/setShutdownPromptSeconds", (id, content) =>
            {
                var response = content.ToObject<int>();

                _room.SetShutdownPromptSeconds(response);

                SendFullStatus();
            });

            AddAction("/shutdownStart", (id, content) => _room.StartShutdown(eShutdownType.Manual));

            AddAction("/shutdownEnd", (id, content) => _room.ShutdownPromptTimer.Finish());

            AddAction("/shutdownCancel", (id, content) => _room.ShutdownPromptTimer.Cancel());


            _room.ShutdownPromptTimer.HasStarted += (sender, args) =>
            {
                PostEventMessage("timerStarted");
            };

            _room.ShutdownPromptTimer.HasFinished += (sender, args) =>
            {
                PostEventMessage("timerFinished");
            };

            _room.ShutdownPromptTimer.WasCancelled += (sender, args) =>
            {
                PostEventMessage("timerCancelled");
            };

            _room.ShutdownPromptTimer.SecondsRemainingFeedback.OutputChange += (sender, args) =>
            {
                var status = new
                {
                    secondsRemaining = _room.ShutdownPromptTimer.SecondsRemainingFeedback.IntValue,
                    percentageRemaining = _room.ShutdownPromptTimer.PercentFeedback.UShortValue
                };

                PostStatusMessage(JToken.FromObject(status));
            };
        }

        private void SendFullStatus()
        {
            var status = new IShutdownPromptTimerStateMessage
            {
                ShutdownPromptSeconds = _room.ShutdownPromptTimer.SecondsToCount,
                SecondsRemaining = _room.ShutdownPromptTimer.SecondsRemainingFeedback.IntValue,
                PercentageRemaining = _room.ShutdownPromptTimer.PercentFeedback.UShortValue
            };

            PostStatusMessage(status);
        }
    }


    public class IShutdownPromptTimerStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("secondsRemaining")]
        public int SecondsRemaining { get; set; }

        [JsonProperty("percentageRemaining")]
        public int PercentageRemaining { get; set; }

        [JsonProperty("shutdownPromptSeconds")]
        public int ShutdownPromptSeconds { get; set; }
    }
}
