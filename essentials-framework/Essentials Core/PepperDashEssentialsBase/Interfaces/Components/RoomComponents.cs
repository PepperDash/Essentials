using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Interfaces.Components
{
    /// <summary>
    /// Describes a room comprised of components
    /// </summary>
    public interface IComponentRoom : IKeyed
    {
        List<IActivatableComponent> Components { get; }
        List<IRoomActivityComponent> Activities { get; }

        List<T> GetComponentsOfType<T>() where T : IActivatableComponent;
        List<IRoomActivityComponent> GetOrderedActvities();
    }

    /// <summary>
    /// Describes a component
    /// </summary>
    public interface IComponent : IKeyed
    {

    }

    /// <summary>
    /// Describes a room component
    /// </summary>
    public interface IRoomComponent : IComponent
    {
        IComponentRoom Parent { get; }
    }

    /// <summary>
    /// Describes a room activity component
    /// </summary>
    public interface IRoomActivityComponent : IRoomComponent
    {
        /// <summary>
        /// Indicates if the component is enabled
        /// </summary>
        BoolFeedback IsEnabledFeedback { get; }

        /// <summary>
        /// Set this value to enable or disable the component
        /// </summary>
        bool Enable { set; }
        /// <summary>
        /// Label to be displayed for the activity on the UI
        /// </summary>
        string Label { get; }
        /// <summary>
        /// Icon to be displayed for the activity on the UI
        /// </summary>
        string Icon { get;  }
        /// <summary>
        /// The component group that will be activated when this activty starts
        /// </summary>
        IRoomBehaviourGroupComponent Component { get; }
        /// <summary>
        /// Determines the order the activities will be displayed on the UI
        /// </summary>
        int Order { get; }


        /// <summary>
        /// Starts the activity
        /// </summary>
        void StartActivity();
        /// <summary>
        /// Ends the activity
        /// </summary>
        void EndActivity();
    }

    /// <summary>
    /// Describes a room component that can be "activated" 
    /// </summary>
    public interface IActivatableComponent : IRoomComponent
    {
        /// <summary>
        /// Indicates if the component is activated
        /// </summary>
        BoolFeedback ActivatedFeedback { get; }

        /// <summary>
        /// Activates the component
        /// </summary>
        void Activate();
        /// <summary>
        /// Dactivates the component
        /// </summary>
        void Deactivate();
    }

    /// <summary>
    /// Describes a group of room behaviour component.  Is able to contain a collection of components that aggregate
    /// together to behave as one
    /// </summary>
    public interface IRoomBehaviourGroupComponent : IRoomComponent
    {
        /// <summary>
        /// A collection of components that work together to achieve a common behaviour
        /// </summary>
        List<IActivatableComponent> Components { get; }

        void ActivateComponents();
        void DeactivateComponents();
    }

    /// <summary>
    /// Describes an individual room behaviour component
    /// </summary>
    public interface IRoomBehaviourComponent : IActivatableComponent
    {

    }


    /// <summary>
    /// Describes a room device component behaviour
    /// </summary>
    public interface IDeviceBehaviourComponent<T> : IActivatableComponent where T : EssentialsDevice
    {
        /// <summary>
        /// The device this component applies behaviour to
        /// </summary>
        T Device { get; }
    }
}