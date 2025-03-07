using Crestron.SimplSharpPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Codec
{ 
    /// <summary>
    /// Describes a cisco codec device that can allow configuration of cameras
    /// </summary>
    public interface ICiscoCodecCameraConfig
    {
        void SetCameraAssignedSerialNumber(uint cameraId, string serialNumber);

        void SetCameraName(uint videoConnectorId, string name);

        void SetInputSourceType(uint videoConnectorId, eCiscoCodecInputSourceType sourceType);
    }

    public enum eCiscoCodecInputSourceType
    {
        PC,
        camera,
        document_camera,
        mediaplayer,
        other,
        whiteboard
    }
}
