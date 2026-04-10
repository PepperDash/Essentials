using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Special container class for CrestronSecret provider
    /// </summary>
    public class CrestronSecret : ISecret
    {
        /// <summary>
        /// Gets the Provider
        /// </summary>
        public ISecretProvider Provider { get; private set; }

        /// <summary>
        /// Gets the Key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the Value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Constructor for CrestronSecret
        /// </summary>
        /// <param name="key">key for the secret</param>
        /// <param name="value">value of the secret</param>
        /// <param name="provider">provider of the secret</param>
        public CrestronSecret(string key, string value, ISecretProvider provider)
        {
            Key = key;
            Value = value;
            Provider = provider;
        }

    }

}