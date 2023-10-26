using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality required to prompt a user to enter a password
    /// </summary>
    public interface IPasswordPrompt
    {
        /// <summary>
        /// Notifies when a password is required or is entered incorrectly
        /// </summary>
        event EventHandler<PasswordPromptEventArgs> PasswordRequired;

        /// <summary>
        /// Submits the password
        /// </summary>
        /// <param name="password"></param>
        void SubmitPassword(string password);
    }
}