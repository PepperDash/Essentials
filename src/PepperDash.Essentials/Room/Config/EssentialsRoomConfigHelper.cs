extern alias Full;
using System.Linq;
using System.Text;
using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Room.Config
{
	public class EssentialsRoomConfigHelper
	{
		/// <summary>
		/// Returns a room object from this config data
		/// </summary>
		/// <returns></returns>
		public static IKeyed GetRoomObject(DeviceConfig roomConfig)
		{
			var typeName = roomConfig.Type.ToLower();

		    switch (typeName)
		    {
		        case "huddle" : 
		        {
                    return new EssentialsHuddleSpaceRoom(roomConfig);
		        }
                case "huddlevtc1" :
		        {
                    return new EssentialsHuddleVtc1Room(roomConfig);
		        }
                case "ddvc01bridge" :
		        {
                    return new Device(roomConfig.Key, roomConfig.Name); // placeholder device that does nothing.
                }
                case "dualdisplay" :
		        {
                    return new EssentialsDualDisplayRoom(roomConfig);
		        }
                case "combinedhuddlevtc1" :
		        {
                    return new EssentialsCombinedHuddleVtc1Room(roomConfig);
		        }
                case "techroom" :
		        {
                    return new EssentialsTechRoom(roomConfig);
		        }
                default :
		        {
		  		    return Core.DeviceFactory.GetDevice(roomConfig);
		        }
		    }
		}

        /// <summary>
        /// Gets and operating, standalone emergegncy object that can be plugged into a room.
        /// Returns null if there is no emergency defined
        /// </summary>
        public static EssentialsRoomEmergencyBase GetEmergency(EssentialsRoomPropertiesConfig props, IEssentialsRoom room)
        {
            // This emergency 
            var emergency = props.Emergency;
            if (emergency != null)
            {
                //switch on emergency type here.  Right now only contact and shutdown
                var e = new EssentialsRoomEmergencyContactClosure(room.Key + "-emergency", props.Emergency, room);
                DeviceManager.AddDevice(e);
            }
            return null;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="props"></param>
		/// <param name="room"></param>
		/// <returns></returns>
		public static Core.Privacy.MicrophonePrivacyController GetMicrophonePrivacy(
			EssentialsRoomPropertiesConfig props, IPrivacy room)
		{
			var microphonePrivacy = props.MicrophonePrivacy;
			if (microphonePrivacy == null)
			{
				Debug.Console(0, "Cannot create microphone privacy with null properties");
				return null;
			}
			// Get the MicrophonePrivacy device from the device manager
			var mP = (DeviceManager.GetDeviceForKey(props.MicrophonePrivacy.DeviceKey) as
				Core.Privacy.MicrophonePrivacyController);
			// Set this room as the IPrivacy device
			if (mP == null)
			{
				Debug.Console(0, "ERROR: Selected device {0} is not MicrophonePrivacyController", props.MicrophonePrivacy.DeviceKey);
				return null;
			}
			mP.SetPrivacyDevice(room);

			var behaviour = props.MicrophonePrivacy.Behaviour.ToLower();

			if (behaviour == null)
			{
				Debug.Console(0, "WARNING: No behaviour defined for MicrophonePrivacyController");
				return null;
			}
			if (behaviour == "trackroomstate")
			{
				// Tie LED enable to room power state
                var essRoom = room as IEssentialsRoom;
                essRoom.OnFeedback.OutputChange += (o, a) =>
				{
                    if (essRoom.OnFeedback.BoolValue)
						mP.EnableLeds = true;
					else
						mP.EnableLeds = false;
				};

                mP.EnableLeds = essRoom.OnFeedback.BoolValue;
			}
			else if (behaviour == "trackcallstate")
			{
				// Tie LED enable to room power state
                var inCallRoom = room as IHasInCallFeedback;
                inCallRoom.InCallFeedback.OutputChange += (o, a) =>
				{
                    if (inCallRoom.InCallFeedback.BoolValue)
						mP.EnableLeds = true;
					else
						mP.EnableLeds = false;
				};

                mP.EnableLeds = inCallRoom.InCallFeedback.BoolValue;
			}

			return mP;
		}
	
	}
}