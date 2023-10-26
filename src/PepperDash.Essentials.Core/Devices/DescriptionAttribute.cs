using System;

namespace PepperDash.Essentials.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class DescriptionAttribute : Attribute
    {
        private string _Description;

        public DescriptionAttribute(string description)
        {
            //Debug.Console(2, "Setting Description: {0}", description);
            _Description = description;
        }

        public string Description
        {
            get { return _Description; }
        }
    }
}