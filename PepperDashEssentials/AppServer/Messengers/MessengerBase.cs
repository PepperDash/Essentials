using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;

using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
	/// <summary>
	/// Provides a messaging bridge for a VideoCodecBase
	/// </summary>
	public abstract class MessengerBase : IKeyed
	{
        public string Key { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public MobileControlSystemController AppServerController { get; private set; }

		public string MessagePath { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="codec"></param>
		public MessengerBase(string key, string messagePath)
		{
            Key = key;

			if (string.IsNullOrEmpty(messagePath))
				throw new ArgumentException("messagePath must not be empty or null");

			MessagePath = messagePath;
		}
		

		/// <summary>
		/// Registers this messenger with appserver controller
		/// </summary>
		/// <param name="appServerController"></param>
		public void RegisterWithAppServer(MobileControlSystemController appServerController)
		{
			if (appServerController == null)
				throw new ArgumentNullException("appServerController");
			
			AppServerController = appServerController;
			CustomRegisterWithAppServer(AppServerController);
		}

		/// <summary>
		/// Implemented in extending classes. Wire up API calls and feedback here
		/// </summary>
		/// <param name="appServerController"></param>
		abstract protected void CustomRegisterWithAppServer(MobileControlSystemController appServerController);

		/// <summary>
		/// Helper for posting status message
		/// </summary>
		/// <param name="contentObject">The contents of the content object</param>
		protected void PostStatusMessage(object contentObject)
		{
            if (AppServerController != null)
            {
                AppServerController.SendMessageToServer(JObject.FromObject(new
                {
                    type = MessagePath,
                    content = contentObject
                }));
            }
		}
	}
}