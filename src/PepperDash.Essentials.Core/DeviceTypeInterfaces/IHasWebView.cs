using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
        /// <summary>
        /// Defines the display mode for a webview event, with expected values of "Fullscreen", "Modal", or "Unknown".
        /// </summary>
         public enum eWebViewEventMode
        {
            /// <summary>
            /// The display mode for the webview event is unknown or not specified.  This value can be used as a default or fallback when the display mode is not provided or cannot be parsed into a known value.
            /// </summary>
            Unknown,

            /// <summary>
            /// The webview event should be displayed in fullscreen mode, covering the entire screen and typically used for immersive experiences or when maximum screen real estate is needed.  When a webview event with this display mode is shown, it will typically trigger the WebViewStatusChanged event with a status of "Fullscreen", and when it is cleared/closed, it will trigger the WebViewStatusChanged event with a status of "Cleared".
            /// </summary>
            Fullscreen,

            /// <summary>
            /// The webview event should be displayed in modal mode, which typically means it will be shown as a dialog or overlay on top of the existing content, allowing the user to interact with it while still being able to see the underlying content.  This display mode is often used for alerts, confirmations, or when the webview content is related to the current context but does not require full immersion.  When a webview event with this display mode is shown, it will typically trigger the WebViewStatusChanged event with a status of "Modal", and when it is cleared/closed, it will trigger the WebViewStatusChanged event with a status of "Cleared".
            /// </summary>
            Modal,
        }

        /// <summary>
        /// Defines the target for a webview event, with expected values of "OSD", "Controller", "PersistentWebApp", or "RoomScheduler".
        /// </summary>

        public enum eWebViewTarget
        {
            /// <summary>
            /// The target for the webview event is unknown or not specified.  This value can be used as a default or fallback when the target is not provided or cannot be parsed into a known value.
            /// </summary>
            Unknown,

            /// <summary>
            /// The webview event should be displayed on the On-Screen Display (OSD).
            /// </summary>
            OSD,

            /// <summary>
            /// The webview event should be displayed on the controller.
            /// </summary>
            Controller,

            /// <summary>
            /// The webview event should be displayed on the persistent web application.
            /// </summary>
            PersistentWebApp,

            /// <summary>
            /// The webview event should be displayed on the room scheduler.
            /// </summary>
            RoomScheduler
        }

        /// <summary>
        /// Represents the reason for an error in a webview event, which can provide additional information about what went wrong.  This class is typically only used in the Status property of a WebViewEvent when the status indicates an error, and may be null otherwise.
        /// </summary>
        public class Reason
        {
            /// <summary>
            /// The reason for an error in a webview event as a string, which can provide additional information about what went wrong.  This property is typically only populated in case of an error, and may be null otherwise.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the XPath of a webview event, which can provide information about where an error occurred in the webview.  This class is typically only used in the Status property of a WebViewEvent when the status indicates an error, and may be null otherwise.
        /// </summary>
        public class XPath
        {
            /// <summary>
            /// The XPath of a webview event as a string, which can provide information about where an error occurred in the webview.  This property is typically only populated in case of an error, and may be null otherwise.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents a base class for properties that have a string value and trigger an action when the value changes.  This class can be used as a base for properties like DisplayMode and Target in the WebViewEvent, which have string values that can be set directly or parsed into enums for easier handling of expected values.  The ValueChangedAction can be set to trigger any desired behavior when the value changes, such as updating the UI or triggering other events.
        /// </summary>
        public abstract class ValueProperty
        {
            /// <summary>
            /// Triggered when Value is set
            /// </summary>
            public Action ValueChangedAction { get; set; }

            /// <summary>
            /// Triggers the ValueChangedAction if it is set.  This method should be called whenever the Value property is set to ensure that any desired behavior associated with a change in value is executed.
            /// </summary>
            protected void OnValueChanged()
            {
                var a = ValueChangedAction;
                if (a != null)
                    a();
            }

        }
    
        /// <summary>
        /// Represents a webview event, which can include information about the status of the webview, the display parameters for the webview, and any error information if applicable.  This class can be used to represent both show and clear events for a webview, with the Status property indicating the current status of the webview (e.g., "Fullscreen", "Modal", "Cleared", "Error", or "Unknown"), the Display property providing details about how the webview is being displayed (e.g., mode, URL, target, title), and the Cleared property providing details about a cleared/closed webview event (e.g., target and ID).  The Id property can be used to correlate show and clear events for the same webview instance.
        /// </summary>
        public class WebViewEvent
        {
            /// <summary>
            /// The unique identifier for the webview event, which can be used to correlate show and clear events for the same webview instance.  This property is typically included in both show and clear events for a webview, allowing you to track the lifecycle of a specific webview instance from when it is shown to when it is cleared/closed.  The Id can be any string value, but it should be unique for each webview instance to ensure proper correlation between show and clear events.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// The status of the webview event, which can indicate the current state of the webview (e.g., "Fullscreen", "Modal", "Cleared", "Error", or "Unknown") as well as any error information if applicable (XPath and Reason).  The Value property can be used to get or set the current status of the webview, while the XPath and Reason properties can provide additional information in case of an error.  The StatusString property can be used to get or set the raw status string from the event, but it is recommended to use the Value property for easier handling of expected values.  Setting the Value property will trigger the ValueChangedAction if it is set, allowing you to respond to changes in the webview status as needed.
            /// </summary>
            [JsonProperty("status")]
            public Status Status { get; set; } // /Event/UserInterface/WebView/Status
    
            /// <summary>
            /// The display parameters for the webview event, which can include the display mode (e.g., "Fullscreen", "Modal", or "Unknown"), the URL to display in the webview, the target for the webview (e.g., "OSD", "Controller", "PersistentWebApp", or "RoomScheduler"), and the title to display on the webview.  This property is typically included in show events for a webview, providing details about how the webview is being displayed.  When a webview event with these display parameters is shown, it will typically trigger the WebViewStatusChanged event with a status of "Fullscreen" or "Modal" depending on the specified display mode, and when it is cleared/closed, it will trigger the WebViewStatusChanged event with a status of "Cleared".
            /// </summary>
            [JsonProperty("display")]
            public WebViewDisplay Display { get; set; } // /Event/UserInterface/WebView/Display
            
            /// <summary>
            /// The details for a cleared/closed webview event, which can include the target for the webview that was cleared (e.g., "OSD", "Controller", "PersistentWebApp", or "RoomScheduler") and the unique identifier for the webview event that was cleared.  This property is typically included in clear events for a webview, providing details about which webview instance was cleared/closed.  When a webview event with this property is cleared/closed, it will typically trigger the WebViewStatusChanged event with a status of "Cleared".
            /// </summary>
            [JsonProperty("cleared")]
            public WebViewClear Cleared { get; set; } // /Event/UserInterface/WebView/Cleared
        }

        /// <summary>
        /// Represents the display parameters for a webview event, which can include the display mode (e.g., "Fullscreen", "Modal", or "Unknown"), the URL to display in the webview, the target for the webview (e.g., "OSD", "Controller", "PersistentWebApp", or "RoomScheduler"), and the title to display on the webview.  This class is typically used in the Display property of a WebViewEvent to provide details about how the webview is being displayed when a show event occurs.  When a webview event with these display parameters is shown, it will typically trigger the WebViewStatusChanged event with a status of "Fullscreen" or "Modal" depending on the specified display mode, and when it is cleared/closed, it will trigger the WebViewStatusChanged event with a status of "Cleared".
        /// </summary>
        public class WebViewDisplay
        {
            /// <summary>
            /// The display mode for the webview event.  Expected values are "Fullscreen", "Modal", or "Unknown".  
            /// </summary>
            [JsonProperty("mode")]
            public DisplayMode Mode { get; set; }

            /// <summary>
            /// The URL to display in the webview.  
            /// </summary>
            [JsonProperty("url")]
            public string Url { get; set; }

            /// <summary>
            /// The target for the webview.  Expected values are "OSD", "Controller", "PersistentWebApp", or "RoomScheduler".
            /// </summary>
            [JsonProperty("target")]
            public Target Target { get; set; }

            /// <summary>
            /// The title to display on the webview.
            /// </summary>
            [JsonProperty("title")]
            public string Title { get; set; }

            /// <summary>
            /// The unique identifier for the webview event, used to correlate show and clear events for the same webview instance.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        /// <summary>
        /// Represents the data for a webview cleared event, which indicates that a webview with the specified ID and target has been cleared/closed.
        /// </summary>
        public class WebViewClear
        {
            /// <summary>
            /// The target for the webview that was cleared.  Expected values are "OSD", "Controller", "PersistentWebApp", or "RoomScheduler".
            /// </summary>
            [JsonProperty("target")]
            public Target Target { get; set; }

            /// <summary>
            /// The unique identifier for the webview event that was cleared, used to correlate show and clear events for the same webview instance.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        /// <summary>
        /// Represents the display mode for a webview event, with a string value and a corresponding enum property for easier handling of expected values.
        /// </summary>
        public class DisplayMode : ValueProperty
        {
            private string _value;

            /// <summary>
            /// The id of the webview event.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// The string value for the display mode, which can be set directly or parsed into the WebViewEventMode enum using the WebViewEventMode property.  Setting this property will also trigger the ValueChangedAction if it is set.
            /// </summary>
            public string Value { get { return _value; } set { _value = value; OnValueChanged(); } }

            /// <summary>
            /// The display mode for the webview event as an enum, which can be used for easier handling of expected values.  Expected values are Fullscreen, Modal, or Unknown.
            /// </summary>
            public eWebViewEventMode WebViewEventMode
            {
                get
                {
                    eWebViewEventMode mode;
                    System.Enum.TryParse(Value, true, out mode);
                    return mode;
                }
            }
        }

        /// <summary>
        /// Represents the target for a webview event, with a string value and a corresponding enum property for easier handling of expected values.  Setting the Value property will also trigger the ValueChangedAction if it is set.
        /// </summary>
        public class Target : ValueProperty
        {
            private string _value;

            /// <summary>
            /// The id of the webview event.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// The string value for the target, which can be set directly or parsed into the eWebViewTarget enum using the WebViewTarget property.  Expected values are "OSD", "Controller", "PersistentWebApp", or "RoomScheduler".  Setting this property will also trigger the ValueChangedAction if it is set.
            /// </summary>
            public string Value { get { return _value; } set { _value = value; OnValueChanged(); } }

            /// <summary>
            /// The target for the webview event as an enum, which can be used for easier handling of expected values.  Expected values are OSD, Controller, PersistentWebApp, or RoomScheduler.
            /// </summary>
            public eWebViewTarget WebViewTarget
            {
                get
                {
                    eWebViewTarget target;
                    System.Enum.TryParse(Value, true, out target);
                    return target;
                }
            }
        }

        /// <summary>
        /// Represents the status of a webview event, which can include error information (XPath and Reason) as well as the current status of the webview.  The Value property can be used to get or set the current status of the webview, while the XPath and Reason properties can provide additional information in case of an error.  The StatusString property can be used to get or set the raw status string from the event.
        /// </summary>
        public class Status
        {
            /// <summary>
            /// The XPath of the webview event, which can provide information about where an error occurred in the webview.  This property is typically only populated in case of an error, and may be null otherwise.
            /// </summary>
            [JsonProperty("XPath", NullValueHandling = NullValueHandling.Ignore)]
            public XPath XPath { get; set; }

            /// <summary>
            /// The reason for an error in the webview event, which can provide additional information about what went wrong.  This property is typically only populated in case of an error, and may be null otherwise.
            /// </summary>
            [JsonProperty("Reason", NullValueHandling = NullValueHandling.Ignore)]
            public Reason Reason { get; set; }

            /// <summary>
            /// The raw status string from the webview event, which can provide information about the current status of the webview.  This property can be used to get or set the status directly, but it is recommended to use the Value property for easier handling of expected values.  Setting this property will not trigger any actions, while setting the Value property will trigger the ValueChangedAction if it is set.
            /// </summary>
            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public string StatusString { get; set; }

            /// <summary>
            /// The current status of the webview as a string, which can be set directly or parsed into a WebViewEventMode enum using the WebViewEventMode property.  Expected values are "Fullscreen", "Modal", "Cleared", "Error", or "Unknown".  Setting this property will trigger the ValueChangedAction if it is set.
            /// </summary>
            [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
            public string Value { get; set; }
        }

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
        /// Gets or sets the WebViewEvent associated with the status change, which can provide additional information about the webview event that triggered the status change, such as display parameters or error information.  This property allows you to include the full WebViewEvent in the event args, giving you access to all relevant details about the webview event when handling the WebViewStatusChanged event.
        /// </summary>
        public WebViewEvent WebView { get; }

        /// <summary>
        /// Constructor for WebViewStatusChangedEventArgs
        /// </summary>
        /// <param name="status">the new status of the webview</param>
        public WebViewStatusChangedEventArgs(string status)
        {
            Status = status;
        }

        /// <summary>
        /// Constructor for WebViewStatusChangedEventArgs with WebViewEvent parameter, which can provide additional information about the webview event that triggered the status change, such as display parameters or error information.  This constructor allows you to include the full WebViewEvent in the event args, giving you access to all relevant details about the webview event when handling the WebViewStatusChanged event.
        /// </summary>
        /// <param name="status">the new status of the webview</param>
        /// <param name="webview">the WebViewEvent associated with the status change</param>
        public WebViewStatusChangedEventArgs(string status, WebViewEvent webview)
        {
            Status = status;
            WebView = webview;
        }
    }
}
