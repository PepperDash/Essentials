using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasWebView
    /// </summary>
    public interface IHasWebView
    {
        /// <summary>
        /// Indicates whether the webview is currently visible
        /// </summary>
        bool WebviewIsVisible { get; }

        /// <summary>
        /// Shows the webview with the specified parameters
        /// </summary>
        /// <param name="url">the URL to display in the webview</param>
        /// <param name="mode">the display mode for the webview</param>
        /// <param name="title">the title to display on the webview</param>
        /// <param name="target">the target for the webview</param>
        void ShowWebView(string url, string mode, string title, string target);

        /// <summary>
        /// Hides the webview
        /// </summary>
        void HideWebView();

        /// <summary>
        /// Event raised when the webview status changes
        /// </summary>
        event EventHandler<WebViewStatusChangedEventArgs> WebViewStatusChanged;
    }


    /// <summary>
    /// Defines the contract for IHasWebViewWithPwaMode
    /// </summary>
    public interface IHasWebViewWithPwaMode : IHasWebView
    {
        /// <summary>
        /// Indicates whether the webview is currently in PWA mode
        /// </summary>
        bool IsInPwaMode { get; }

        /// <summary>
        /// Gets the BoolFeedback indicating whether the webview is currently in PWA mode
        /// </summary>
        BoolFeedback IsInPwaModeFeedback { get; }

        /// <summary>
        /// Sends navigators to the specified PWA URL.  Accepts an absolute URL or a relative URL for a mobile control app
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        void SendNavigatorsToPwaUrl(string url);

        /// <summary> 
        /// Exits navigators from PWA mode
        /// </summary>
        void ExitNavigatorsPwaMode();
    }


    /// <summary>
    /// Represents a WebViewStatusChangedEventArgs
    /// </summary>
    public class WebViewStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Constructor for WebViewStatusChangedEventArgs
        /// </summary>
        /// <param name="status">the new status of the webview</param>
        public WebViewStatusChangedEventArgs(string status)
        {
            Status = status;
        }
    }
}
