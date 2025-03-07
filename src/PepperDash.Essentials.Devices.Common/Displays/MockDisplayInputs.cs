using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Displays
{
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

        public string Name { get; set; }

        public string Key { get; set; }

        public event EventHandler ItemUpdated;

        public MockDisplayInput(string key, string name, MockDisplay parent)
        {
            Key = key;
            Name = name;
            _parent = parent;
        }

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
