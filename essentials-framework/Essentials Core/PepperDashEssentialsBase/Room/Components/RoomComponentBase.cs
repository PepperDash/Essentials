using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.Interfaces.Components;

namespace PepperDash.Essentials.Core.Room.Components
{
    /// <summary>
    /// The base class from which Room Components should be derived
    /// </summary>
    public abstract class RoomComponentBase : IRoomComponent
    {
        private string _componentKey;

        /// <summary>
        /// The key of the component, which is composed of the parent room key, plus the specific component key
        /// </summary>
        public string Key {
            get
            {
                return Parent.Key + "-" + _componentKey;
            }
        }

        public IComponentRoom Parent { get; private set; }

        public RoomComponentBase(string key, IComponentRoom parent)
        {
            _componentKey = key;
            Parent = parent;
        }

    }

    /// <summary>
    /// The base class from which Room Activities should be derived
    /// </summary>
    public abstract class RoomActivityComponentBase : IRoomActivityComponent
    {

        #region IRoomActivityComponent Members

        public BoolFeedback IsEnabledFeedback { get; private set; }

        private bool _enable;

        public bool Enable
        {
            set 
            {
                if (value != _enable)
                {
                    _enable = value;
                    IsEnabledFeedback.FireUpdate();
                }
            }
        }

        public string Label { get; private set; }

        public string Icon { get; private set; }

        public IRoomBehaviourGroupComponent Component { get; private set; }

        public int Order { get; private set; }

        #endregion

        #region IRoomComponent Members

        public IComponentRoom Parent { get; private set; }

        #endregion

        #region IKeyed Members

        private string _componentKey;

        public string Key
        {
            get
            {
                return Parent.Key + "-" + _componentKey;
            }
        }

        #endregion

        public RoomActivityComponentBase(string key, IComponentRoom parent)
        {
            _componentKey = key;
            Parent = parent;

            IsEnabledFeedback = new BoolFeedback(() => _enable);
        }

        public void StartActivity()
        {
            throw new NotImplementedException();
        }

        public void EndActivity()
        {
            throw new NotImplementedException();
        }
    }


}