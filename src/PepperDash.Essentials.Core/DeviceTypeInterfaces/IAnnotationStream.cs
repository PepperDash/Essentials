namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

    /// <summary>
    /// Defines minimum functionality for a device that can provide an annotation stream.
    /// </summary>
    public interface IAnnotationStream
    {
        /// <summary>
        /// Starts the annotation stream.
        /// </summary>
        /// <param name="streamIdx">The index of the annotation stream to start.</param>
        bool StartAnnotationForStream(ushort streamIdx);
        
        /// <summary>
        /// Stops the annotation stream.
        /// </summary>
        /// <param name="streamIdx">The index of the annotation stream to stop.</param>
        bool StopAnnotationForStream(ushort streamIdx);
    }
