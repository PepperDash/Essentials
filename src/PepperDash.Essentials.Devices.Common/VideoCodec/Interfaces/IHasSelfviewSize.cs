﻿using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	public interface IHasSelfviewSize
	{
		StringFeedback SelfviewPipSizeFeedback { get; }

		void SelfviewPipSizeSet(CodecCommandWithLabel size);

		void SelfviewPipSizeToggle();
	}
}