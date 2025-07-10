﻿using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Shades;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for shade control operations.
    /// Handles shade open, close, and stop commands for shades that support these operations.
    /// </summary>
    public class IShadesOpenCloseStopMessenger : MessengerBase
    {
        private readonly IShadesOpenCloseStop device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IShadesOpenCloseStopMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="shades">The shade device that provides open/close/stop functionality.</param>
        /// <param name="messagePath">The message path for shade control messages.</param>
        public IShadesOpenCloseStopMessenger(string key, IShadesOpenCloseStop shades, string messagePath)
            : base(key, messagePath, shades as IKeyName)
        {
            device = shades;
        }

        /// <summary>
        /// Registers actions for handling shade control operations.
        /// Includes shade open, close, stop, and full status reporting.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/shadeUp", (id, content) =>
                {

                    device.Open();

                });

            AddAction("/shadeDown", (id, content) =>
                {

                    device.Close();

                });

            var stopDevice = device;
            if (stopDevice != null)
            {
                AddAction("/stopOrPreset", (id, content) =>
                {
                    stopDevice.Stop();
                });
            }

            if (device is IShadesOpenClosedFeedback feedbackDevice)
            {
                feedbackDevice.ShadeIsOpenFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>(ShadeIsOpenFeedback_OutputChange);
                feedbackDevice.ShadeIsClosedFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>(ShadeIsClosedFeedback_OutputChange);
            }
        }

        private void ShadeIsOpenFeedback_OutputChange(object sender, Core.FeedbackEventArgs e)
        {
            var state = new ShadeBaseStateMessage
            {
                IsOpen = e.BoolValue
            };

            PostStatusMessage(state);
        }

        private void ShadeIsClosedFeedback_OutputChange(object sender, Core.FeedbackEventArgs e)
        {
            var state = new ShadeBaseStateMessage
            {
                IsClosed = e.BoolValue
            };

            PostStatusMessage(state);
        }


        private void SendFullStatus(string id = null)
        {
            var state = new ShadeBaseStateMessage();

            if (device is IShadesOpenClosedFeedback feedbackDevice)
            {
                state.IsOpen = feedbackDevice.ShadeIsOpenFeedback.BoolValue;
                state.IsClosed = feedbackDevice.ShadeIsClosedFeedback.BoolValue;
            }

            PostStatusMessage(state, id);
        }
    }

    /// <summary>
    /// Represents a shade state message containing shade status and control information.
    /// </summary>
    public class ShadeBaseStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the label for the middle button control.
        /// </summary>
        [JsonProperty("middleButtonLabel", NullValueHandling = NullValueHandling.Ignore)]
        public string MiddleButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the shade is open.
        /// </summary>
        [JsonProperty("isOpen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the shade is closed.
        /// </summary>
        [JsonProperty("isClosed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsClosed { get; set; }
    }
}