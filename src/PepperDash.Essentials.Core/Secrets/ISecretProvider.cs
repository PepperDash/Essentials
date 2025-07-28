namespace PepperDash.Essentials.Core.Secrets
{
    /// <summary>
    /// Defines the contract for ISecretProvider
    /// </summary>
    public interface ISecretProvider : IKeyed
    {
        /// <summary>
        /// Set secret value for provider by key
        /// </summary>
        /// <param name="key">key of secret to set</param>
        /// <param name="value">value to set secret to</param>
        /// <returns></returns>
        bool SetSecret(string key, object value);

        /// <summary>
        /// Return object containing secret from provider
        /// </summary>
        /// <param name="key">key of secret to retrieve</param>
        /// <returns></returns>
        ISecret GetSecret(string key);

        /// <summary>
        /// Verifies presence of secret
        /// </summary>
        /// <param name="key">key of secret to chek</param>
        /// <returns></returns>
        bool TestSecret(string key);

        /// <summary>
        /// Description of the secrets provider
        /// </summary>
        string Description { get; }
    }
}