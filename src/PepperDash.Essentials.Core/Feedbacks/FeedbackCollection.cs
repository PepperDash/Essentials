using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Basically a List , with an indexer to find feedbacks by key name
/// </summary>
public class FeedbackCollection<T> : List<T> where T : Feedback
{
    /// <summary>
    /// Case-insensitive port lookup linked to feedbacks' keys
    /// </summary>
    public T this[string key]
    {
        get
        {
            return this.FirstOrDefault(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }
}