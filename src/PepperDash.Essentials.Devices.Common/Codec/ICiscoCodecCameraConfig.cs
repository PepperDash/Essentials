namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Describes a cisco codec device that can allow configuration of cameras
    /// </summary>
    public interface ICiscoCodecCameraConfig
    {
        /// <summary>
        /// Sets the assigned serial number for the specified camera
        /// </summary>
        /// <param name="cameraId">The camera identifier</param>
        /// <param name="serialNumber">The serial number to assign</param>
        void SetCameraAssignedSerialNumber(uint cameraId, string serialNumber);

        /// <summary>
        /// Sets the name for the camera on the specified video connector
        /// </summary>
        /// <param name="videoConnectorId">The video connector identifier</param>
        /// <param name="name">The name to assign</param>
        void SetCameraName(uint videoConnectorId, string name);

        /// <summary>
        /// Sets the input source type for the specified video connector
        /// </summary>
        /// <param name="videoConnectorId">The video connector identifier</param>
        /// <param name="sourceType">The source type to set</param>
        void SetInputSourceType(uint videoConnectorId, eCiscoCodecInputSourceType sourceType);
    }

    /// <summary>
    /// Enumeration of Cisco codec input source types
    /// </summary>
    public enum eCiscoCodecInputSourceType
    {
        /// <summary>
        /// PC source type
        /// </summary>
        PC,

        /// <summary>
        /// Camera source type
        /// </summary>
        camera,

        /// <summary>
        /// Document camera source type
        /// </summary>
        document_camera,

        /// <summary>
        /// Media player source type
        /// </summary>
        mediaplayer,

        /// <summary>
        /// Other source type
        /// </summary>
        other,

        /// <summary>
        /// Whiteboard source type
        /// </summary>
        whiteboard
    }
}
