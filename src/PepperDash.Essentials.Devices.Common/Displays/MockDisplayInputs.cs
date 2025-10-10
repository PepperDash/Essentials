using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Devices.Common.Displays
{
    /// <summary>
    /// Represents a MockDisplayInputs
    /// </summary>
    public class MockDisplayInputs : ISelectableItems<string>
    {
        private Dictionary<string, ISelectableItem> _items;

        /// <summary>
        /// Gets or sets the collection of selectable items
        /// </summary>
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

        /// <summary>
        /// Gets or sets the currently selected item
        /// </summary>
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

        /// <summary>
        /// Occurs when the items collection is updated
        /// </summary>
        public event EventHandler ItemsUpdated;
        /// <summary>
        /// Occurs when the current item changes
        /// </summary>
        public event EventHandler CurrentItemChanged;
    }

    /// <summary>
    /// Represents a MockDisplayInput
    /// </summary>
    public class MockDisplayInput : ISelectableItem
    {
        private MockDisplay _parent;

        private bool _isSelected;

        /// <summary>
        /// Gets or sets a value indicating whether this input is selected
        /// </summary>
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

        /// <summary>
        /// Occurs when this item is updated
        /// </summary>
        public event EventHandler ItemUpdated;

        /// <summary>
        /// Initializes a new instance of the MockDisplayInput class
        /// </summary>
        /// <param name="key">The input key</param>
        /// <param name="name">The input name</param>
        /// <param name="parent">The parent mock display</param>
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

            foreach (var input in _parent.Inputs.Items)
            {
                input.Value.IsSelected = input.Key == this.Key;
            }
        }
    }
}
