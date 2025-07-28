using Newtonsoft.Json;
using PepperDash.Essentials.Core.Secrets;

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
    /// Represents a WebSocketServerSecret
    /// </summary>
    public class WebSocketServerSecret : ISecret
    {
        /// <summary>
        /// Gets or sets the Provider
        /// </summary>
        public ISecretProvider Provider { get; private set; }

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public object Value { get; private set; }

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
