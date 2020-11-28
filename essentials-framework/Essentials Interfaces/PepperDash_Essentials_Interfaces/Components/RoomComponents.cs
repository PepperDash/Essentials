using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Interfaces.Components
{
    /// <summary>
    /// Describes a room comprised of components
    /// </summary>
    public interface IComponentRoom : IKeyed
    {
        List<IRoomComponent> Components { get; }
        List<IRoomActivityComponent> Activities { get; }

        List<IRoomComponent> GetRoomComponentsOfType(Type type);
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
        string Label { get; }
        string Icon { get;  }
        IRoomComponent Component { get; }
        int Order { get; }

        void StartActivity();
        void EndActivity();
    }

    /// <summary>
    /// Describes a room component that can be "used" by a user
    /// </summary>
    public interface IUsableRoomComponent
    {
        bool InUse { get; }

        void StartUse();
        void EndUse();
    }

    /// <summary>
    /// Describes a room behaviour component
    /// </summary>
    public interface IRoomBehaviourComponent : IUsableRoomComponent
    {

    }

    /// <summary>
    /// Describes a room device component
    /// </summary>
    public interface IRoomDeviceComponent : IUsableRoomComponent
    {

    }
}