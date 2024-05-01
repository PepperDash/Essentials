using System;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
	public interface IVideoCodecUiExtensionsWebViewDisplayAction
	{
		Action<UiWebViewDisplayActionArgs> UiWebViewDisplayAction { get; set; }
	}

	public class UiWebViewDisplayActionArgs
	{

		/// <summary>
		/// Required <0 - 2000>	The URL of the web page.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Fullscreen, Modal Full screen: Display the web page on the entire screen.Modal: Display the web page in a window.
		/// </summary>

		public string Mode { get; set; }

		/// <summary>
		/// <0 - 255> The title of the web page.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// <0 - 8192> An HTTP header field.You can add up 15 Header parameters in one command, each holding one HTTP header field.
		/// </summary>
		public string Header { get; set; }

		/// <summary>
		/// OSD, Controller, PersistentWebApp Controller: Only for Cisco internal use.
		/// OSD: Close the web view that is displayed on the screen of the device.PersistentWebApp: Only for Cisco internal use.
		/// </summary>
		public string Target { get; set; }
	}

}
