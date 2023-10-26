using System;

namespace PepperDash.Essentials.Core
{
    public class PasswordPromptEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if the last submitted password was incorrect
        /// </summary>
        public bool LastAttemptWasIncorrect { get; private set; }

        /// <summary>
        /// Indicates that the login attempt has failed
        /// </summary>
        public bool LoginAttemptFailed { get; private set; }

        /// <summary>
        /// Indicates that the process was cancelled and the prompt should be dismissed
        /// </summary>
        public bool LoginAttemptCancelled { get; private set; }

        /// <summary>
        /// A message to be displayed to the user
        /// </summary>
        public string Message { get; private set; }

        public PasswordPromptEventArgs(bool lastAttemptIncorrect, bool loginFailed, bool loginCancelled, string message)
        {
            LastAttemptWasIncorrect = lastAttemptIncorrect;
            LoginAttemptFailed      = loginFailed;
            LoginAttemptCancelled   = loginCancelled;
            Message                 = message;
        }
    }
}