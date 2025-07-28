namespace PepperDash.Essentials.Core.Secrets
{

    /// <summary>
    /// interface for delivering secrets in Essentials.
    /// </summary>
    public interface ISecret
    {
        /// <summary>
        /// Instance of ISecretProvider that the secret belongs to
        /// </summary>
        ISecretProvider Provider { get; }

        /// <summary>
        /// Key of the secret in the provider
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Value of the secret
        /// </summary>
        object Value { get; }
    }
}