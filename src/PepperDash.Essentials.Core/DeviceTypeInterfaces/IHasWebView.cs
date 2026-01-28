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
