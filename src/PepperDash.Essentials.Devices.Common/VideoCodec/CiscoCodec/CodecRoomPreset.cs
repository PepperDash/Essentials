using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Represents a room preset on a video codec.  Typically stores camera position(s) and video routing.  Can be recalled by Far End if enabled.
    /// </summary>
    public class CodecRoomPreset : PresetBase
    {
        public CodecRoomPreset(int id, string description, bool def, bool isDef)
            : base(id, description, def, isDef)
        {

        }
    }
}