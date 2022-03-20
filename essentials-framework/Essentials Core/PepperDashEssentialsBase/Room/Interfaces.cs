using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with in call feedback
    /// </summary>
    public interface IHasInCallFeedback
    {
        BoolFeedback InCallFeedback { get; }
    }

    /// <summary>
    /// For rooms with a single display
    /// </summary>
    public interface IHasDefaultDisplay
    {
        IRoutingSinkWithSwitching DefaultDisplay { get; }
    }

    /// <summary>
    /// For rooms with multiple displays
    /// </summary>
    public interface IHasMultipleDisplays
    {
        Dictionary<eSourceListItemDestinationTypes, IRoutingSinkWithSwitching> Displays { get; }
    }

    /// <summary>
    /// For rooms with routing
    /// </summary>
    public interface IRunRouteAction
    {
        void RunRouteAction(string routeKey, string sourceListKey);

        void RunRouteAction(string routeKey, string sourceListKey, Action successCallback);

    }

    /// <summary>
    /// Simplified routing direct from source to destination
    /// </summary>
    public interface IRunDirectRouteAction
    {
        void RunDirectRoute(string sourceKey, string destinationKey);
    }

    /// <summary>
    /// For rooms that default presentation only routing
    /// </summary>
    public interface IRunDefaultPresentRoute
    {
        bool RunDefaultPresentRoute();
    }

    /// <summary>
    /// For rooms that have default presentation and calling routes
    /// </summary>
    public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
    {
        bool RunDefaultCallRoute();
    }

    /// <summary>
    /// Describes environmental controls available on a room such as lighting, shades, temperature, etc.
    /// </summary>
    public interface IEnvironmentalControls
    {
        List<EssentialsDevice> EnvironmentalControlDevices { get; }

        bool HasEnvironmentalControlDevices { get; }
    }

}