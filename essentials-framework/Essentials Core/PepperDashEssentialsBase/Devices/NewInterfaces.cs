using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
	//public interface IVolumeFunctions
	//{
	//    BoolCueActionPair VolumeUpCueActionPair { get; }
	//    BoolCueActionPair VolumeDownCueActionPair { get; }
	//    BoolCueActionPair MuteToggleCueActionPair { get; }
	//}

	//public interface IVolumeTwoWay : IVolumeFunctions
	//{
	//    IntFeedback VolumeLevelFeedback { get; }
	//    UShortCueActionPair VolumeLevelCueActionPair { get; }
	//    BoolFeedback IsMutedFeedback { get; }
	//}

	///// <summary>
	///// 
	///// </summary>
	//public static class IFunctionListExtensions
	//{
	//    public static string GetFunctionsConsoleList(this IHasCueActionList device)
	//    {
	//        var sb = new StringBuilder();
	//        var list = device.CueActionList;
	//        foreach (var cap in list)
	//            sb.AppendFormat("{0,-15} {1,4} {2}\r", cap.Cue.Name, cap.Cue.Number, cap.GetType().Name);
	//        return sb.ToString();
	//    }
	//}

	public enum AudioChangeType
	{
		Mute, Volume
	}

	public class AudioChangeEventArgs
	{
		public AudioChangeType ChangeType { get; private set; }
		public IBasicVolumeControls AudioDevice { get; private set; }

		public AudioChangeEventArgs(IBasicVolumeControls device, AudioChangeType changeType)
		{
			ChangeType = changeType;
			AudioDevice = device;
		}
	}
}