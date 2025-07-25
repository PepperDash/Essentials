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

    public class PasswordPromptEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if the last submitted password was incorrect
        /// </summary>
        public bool LastAttemptWasIncorrect { get; private set; }

        /// <summary>
        /// Gets or sets the LoginAttemptFailed
        /// </summary>
        public bool LoginAttemptFailed { get; private set; }

        /// <summary>
        /// Gets or sets the LoginAttemptCancelled
        /// </summary>
        public bool LoginAttemptCancelled { get; private set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; private set; }

        public PasswordPromptEventArgs(bool lastAttemptIncorrect, bool loginFailed, bool loginCancelled, string message)
        {
            LastAttemptWasIncorrect = lastAttemptIncorrect;
            LoginAttemptFailed = loginFailed;
            LoginAttemptCancelled = loginCancelled;
            Message = message;
        }
    }
}