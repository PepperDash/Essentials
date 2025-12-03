using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.WebSocketServer
{
    internal class WebSocketServerSecretProvider : CrestronLocalSecretsProvider
    {
        public WebSocketServerSecretProvider(string key)
            : base(key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Stores a secret value using the provided secret store provider
    /// </summary>
    public class WebSocketServerSecret : ISecret
    {
        /// <summary>
        /// Gets the Secret Provider associated with this secret
        /// </summary>
        public ISecretProvider Provider { get; private set; }

        /// <summary>
        /// Gets the Key associated with this secret
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the Value associated with this secret
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Initialize and instance of the <see cref="WebSocketServerSecret"/> class
        /// </summary>
        public WebSocketServerSecret(string key, object value, ISecretProvider provider)
        {
            Key = key;
            Value = JsonConvert.SerializeObject(value);
            Provider = provider;
        }

        /// <summary>
        /// DeserializeSecret method
        /// </summary>
        public ServerTokenSecrets DeserializeSecret()
        {
            return JsonConvert.DeserializeObject<ServerTokenSecrets>(Value.ToString());
        }
    }


}
