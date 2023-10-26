using System;

namespace PepperDash.Essentials.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ConfigSnippetAttribute : Attribute
    {
        private string _ConfigSnippet;

        public ConfigSnippetAttribute(string configSnippet)
        {
            //Debug.Console(2, "Setting Config Snippet {0}", configSnippet);
            _ConfigSnippet = configSnippet;
        }

        public string ConfigSnippet
        {
            get { return _ConfigSnippet; }
        }
    }
}