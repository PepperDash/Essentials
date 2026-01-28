using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// SecretsManager static class
    /// </summary>
    public static class SecretsManager
    {
        /// <summary>
        /// Gets the Secrets dictionary
        /// </summary>
        public static Dictionary<string, ISecretProvider> Secrets { get; private set; }

        /// <summary>
        /// Initialize method
        /// </summary>
        public static void Initialize()
        {

            AddSecretProvider("default", new CrestronLocalSecretsProvider("default"));

            AddSecretProvider("CrestronGlobalSecrets", new CrestronGlobalSecretsProvider("CrestronGlobalSecrets"));

            CrestronConsole.AddNewConsoleCommand(SetSecretProcess, "setsecret",
                "Adds secret to secrets provider",
                ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(UpdateSecretProcess, "updatesecret",
                "Updates secret in secrets provider",
                ConsoleAccessLevelEnum.AccessAdministrator);

            CrestronConsole.AddNewConsoleCommand(DeleteSecretProcess, "deletesecret",
                "Deletes secret from secrest provider",
                ConsoleAccessLevelEnum.AccessAdministrator);

            CrestronConsole.AddNewConsoleCommand(ListProviders, "secretproviderlist",
                "Return list of all valid secrets providers",
                ConsoleAccessLevelEnum.AccessAdministrator);

            CrestronConsole.AddNewConsoleCommand(GetProviderInfo, "secretproviderinfo",
                "Return data about secrets provider",
                ConsoleAccessLevelEnum.AccessAdministrator);
        }

        static SecretsManager()
        {
            Secrets = new Dictionary<string, ISecretProvider>();
        }

        /// <summary>
        /// Get Secret Provider from dictionary by key
        /// </summary>
        /// <param name="key">Dictionary Key for provider</param>
        /// <returns>ISecretProvider</returns>
        /// <summary>
        /// GetSecretProviderByKey method
        /// </summary>
        public static ISecretProvider GetSecretProviderByKey(string key)
        {
            ISecretProvider secret;

            Secrets.TryGetValue(key, out secret);

            if (secret == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, "SecretsManager unable to retrieve SecretProvider with the key '{0}'", key);
            }
            return secret;
        }

        /// <summary>
        /// GetProviderInfo method
        /// </summary>
        public static void GetProviderInfo(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (cmd.Length == 0 || (args.Length == 1 && args[0] == "?"))
            {
                response = "Returns data about secrets provider.  Format 'secretproviderinfo <provider>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            if (args.Length == 1)
            {
                var provider = GetSecretProviderByKey(args[0]);

                if (provider == null)
                {
                    response = "Invalid secrets provider key";
                    CrestronConsole.ConsoleCommandResponse(response);
                    return;
                }

                response = String.Format("{0} : {1}", provider.Key, provider.Description);
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            response = "Improper number of arguments";
            CrestronConsole.ConsoleCommandResponse(response);

        }


        /// <summary>
        /// Console Command that returns all valid secrets in the essentials program.
        /// </summary>
        /// <param name="cmd"></param>
        /// <summary>
        /// ListProviders method
        /// </summary>
        public static void ListProviders(string cmd)
        {
            var response = String.Empty;
            var args = cmd.Split(' ');

            if (cmd.Length == 0)
            {
                if (Secrets != null && Secrets.Count > 0)
                {
                    response = Secrets.Aggregate(response,
                        (current, secretProvider) => current + (secretProvider.Key + "\n\r"));
                }
                else
                {
                    response = "No Secrets Providers Available";
                }
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            if (args.Length == 1 && args[0] == "?")
            {
                response = "Reports all valid and preset Secret providers";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }


            response = "Improper number of arguments";
            CrestronConsole.ConsoleCommandResponse(response);

        }

        /// <summary>
        /// Add secret provider to secrets dictionary
        /// </summary>
        /// <param name="key">Key of new entry</param>
        /// <param name="provider">New Provider Entry</param>
        /// <summary>
        /// AddSecretProvider method
        /// </summary>
        public static void AddSecretProvider(string key, ISecretProvider provider)
        {
            if (!Secrets.ContainsKey(key))
            {
                Secrets.Add(key, provider);
                Debug.LogMessage(LogEventLevel.Debug, "Secrets provider '{0}' added to SecretsManager", key);
                return;
            }
            Debug.LogMessage(LogEventLevel.Information, "Unable to add Provider '{0}' to Secrets.  Provider with that key already exists", key );
        }

        /// <summary>
        /// Add secret provider to secrets dictionary, with optional overwrite parameter
        /// </summary>
        /// <param name="key">Key of new entry</param>
        /// <param name="provider">New provider entry</param>
        /// <param name="overwrite">true to overwrite any existing providers in the dictionary</param>
        /// <summary>
        /// AddSecretProvider method
        /// </summary>
        public static void AddSecretProvider(string key, ISecretProvider provider, bool overwrite)
        {
            if (!Secrets.ContainsKey(key))
            {
                Secrets.Add(key, provider);
                Debug.LogMessage(LogEventLevel.Debug, "Secrets provider '{0}' added to SecretsManager", key);
                return;
            }
            if (overwrite)
            {
                Secrets.Add(key, provider);
                Debug.LogMessage(LogEventLevel.Debug, "Provider with the key '{0}' already exists in secrets.  Overwriting with new secrets provider.", key);
                return;
            }
            Debug.LogMessage(LogEventLevel.Information, "Unable to add Provider '{0}' to Secrets.  Provider with that key already exists", key);
        }

        private static void SetSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Adds secrets to secret provider. Format 'setsecret <provider> <secretKey> <secret>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            if (args.Length == 1 && args[0] == "?")
            {
                response = "Adds secrets to secret provider. Format 'setsecret <provider> <secretKey> <secret>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            if (args.Length < 3)
            {
                response =  "Improper number of arguments";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var provider = GetSecretProviderByKey(args[0]);

            if (provider == null)
            {
                //someFail
                response =  "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];
            var secret = args[2];

            CrestronConsole.ConsoleCommandResponse(SetSecret(provider, key, secret));
        }

        private static void UpdateSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Updates secrets in secret provider. Format 'updatesecret <provider> <secretKey> <secret>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            if (args.Length == 1 && args[0] == "?")
            {
                response = "Updates secrets in secret provider. Format 'updatesecret <provider> <secretKey> <secret>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }


            if (args.Length < 3)
            {
                //someFail
                response = "Improper number of arguments";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var provider = GetSecretProviderByKey(args[0]);

            if (provider == null)
            {
                //someFail
                response = "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];
            var secret = args[2];

            CrestronConsole.ConsoleCommandResponse(UpdateSecret(provider, key, secret));

        }

        private static string UpdateSecret(ISecretProvider provider, string key, string secret)
        {
            var secretPresent = provider.TestSecret(key);

            Debug.LogMessage(LogEventLevel.Verbose, provider, "SecretsProvider {0} {1} contain a secret entry for {2}", provider.Key, secretPresent ? "does" : "does not", key);

            if (!secretPresent)
                return
                    String.Format(
                        "Unable to update secret for {0}:{1} - Please use the 'SetSecret' command to modify it");
            var response = provider.SetSecret(key, secret)
                ? String.Format(
                    "Secret successfully set for {0}:{1}",
                    provider.Key, key)
                : String.Format(
                    "Unable to set secret for {0}:{1}",
                    provider.Key, key);
            return response;
        }

        private static string SetSecret(ISecretProvider provider, string key, string secret)
        {
            var secretPresent = provider.TestSecret(key);

            Debug.LogMessage(LogEventLevel.Verbose, provider, "SecretsProvider {0} {1} contain a secret entry for {2}", provider.Key, secretPresent ? "does" : "does not", key);

            if (secretPresent)
                return
                    String.Format(
                        "Unable to set secret for {0}:{1} - Please use the 'UpdateSecret' command to modify it");
            var response = provider.SetSecret(key, secret)
                ? String.Format(
                    "Secret successfully set for {0}:{1}",
                    provider.Key, key)
                : String.Format(
                    "Unable to set secret for {0}:{1}",
                    provider.Key, key);
            return response;

        }

        private static void DeleteSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Deletes secrets in secret provider. Format 'deletesecret <provider> <secretKey>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }
            if (args.Length == 1 && args[0] == "?")
            {
                response = "Deletes secrets in secret provider. Format 'deletesecret <provider> <secretKey>'";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }



            if (args.Length < 2)
            {
                //someFail
                response =  "Improper number of arguments";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var provider = GetSecretProviderByKey(args[0]);

            if (provider == null)
            {
                //someFail
                response =  "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];


            provider.SetSecret(key, "");
            response = provider.SetSecret(key, "")
                ? String.Format(
                    "Secret successfully deleted for {0}:{1}",
                    provider.Key, key)
                : String.Format(
                    "Unable to delete secret for {0}:{1}",
                    provider.Key, key);
            CrestronConsole.ConsoleCommandResponse(response);
            return;


        }
    }


}