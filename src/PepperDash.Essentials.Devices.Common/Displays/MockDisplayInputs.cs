using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Displays
{
    /// <summary>
    /// Represents a MockDisplayInputs
    /// </summary>
    public class MockDisplayInputs : ISelectableItems<string>
    {
        private Dictionary<string, ISelectableItem> _items;

        public Dictionary<string, ISelectableItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value)
                    return;

                _items = value;

                ItemsUpdated?.Invoke(this, null);
            }
        }

        private string _currentItem;

        public string CurrentItem
        { 
            get
            {
                return _currentItem;
            }
            set
            {
                if (_currentItem == value)
                    return;

                _currentItem = value;

                CurrentItemChanged?.Invoke(this, null);
            }
        }

        public event EventHandler ItemsUpdated;
        public event EventHandler CurrentItemChanged;
    }

    /// <summary>
    /// Represents a MockDisplayInput
    /// </summary>
    public class MockDisplayInput : ISelectableItem
    {
        private MockDisplay _parent;

        private bool _isSelected;
        
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;

                ItemUpdated?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        public event EventHandler ItemUpdated;

        public MockDisplayInput(string key, string name, MockDisplay parent)
        {
            Key = key;
            Name = name;
            _parent = parent;
        }

        /// <summary>
        /// Select method
        /// </summary>
        public void Select()
        {
            if (!_parent.PowerIsOnFeedback.BoolValue) _parent.PowerOn();

            foreach(var input in _parent.Inputs.Items)
            {
                input.Value.IsSelected = input.Key == this.Key;
            }
        }
    }
}
