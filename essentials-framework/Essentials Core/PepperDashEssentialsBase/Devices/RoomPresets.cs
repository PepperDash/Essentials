using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Interface for camera presets
    /// </summary>
    public interface IHasCodecRoomPresets
    {
        event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        List<CodecRoomPreset> NearEndPresets { get; }

        List<CodecRoomPreset> FarEndRoomPresets { get; }

        void CodecRoomPresetSelect(int preset);

        void CodecRoomPresetStore(int preset, string description);
    }

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