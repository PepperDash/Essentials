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

        List<T> GetRoomComponentsOfType<T>();
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
        BoolFeedback IsEnabledFeedback { get; }

        bool Enable { set; }
        string Label { get; }
        string Icon { get;  }
        IRoomBehaviourGroupComponent Component { get; }
        int Order { get; }


        void StartActivity();
        void EndActivity();
    }

    /// <summary>
    /// Describes a room component that can be "used" by a user
    /// </summary>
    public interface IActivatableComponent : IRoomComponent
    {
        BoolFeedback ActivatedFeedback { get; }

        void Activate();
        void Deactivate();
    }

    /// <summary>
    /// Describes a room behaviour component.  Is able to contain a collection of components that aggregate
    /// together to behave as one
    /// </summary>
    public interface IRoomBehaviourGroupComponent
    {
        List<IActivatableComponent> Components { get; }

        void ActivateComponents();
        void DeactivateComponents();
    }

    public interface IRoomBehaviourComponent : IActivatableComponent
    {

    }


    /// <summary>
    /// Describes a room device component
    /// </summary>
    public interface IRoomDeviceComponent<T> : IActivatableComponent where T : EssentialsDevice
    {
        public T Device { get; }
    }
}