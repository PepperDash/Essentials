namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IVideoCodecUiExtensionsHandler : IVideoCodecUiExtensionsWebViewDisplayAction, IVideoCodecUiExtensionsClickedEvent
    {
    }

    public interface IVideoCodecUiExtensions
    {
        IVideoCodecUiExtensionsHandler VideoCodecUiExtensionsHandler { get; set; }
    }

}