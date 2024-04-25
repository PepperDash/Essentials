using System;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IVideoCodecUiExtensionsClickedEvent
    {
        event EventHandler<UiExtensionsClickedEventArgs> UiExtensionsClickedEvent;
    }

    public class UiExtensionsClickedEventArgs : EventArgs
    {
        public bool Clicked { get; set; }
        public string Id { get; set; }

        public UiExtensionsClickedEventArgs(bool clicked, string id)
        {
            Clicked = clicked;
            Id = id;
        }

        public UiExtensionsClickedEventArgs()
        {
        }
    }
}
