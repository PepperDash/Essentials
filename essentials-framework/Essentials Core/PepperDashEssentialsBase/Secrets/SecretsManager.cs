using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    public static class SecretsManager
    {
        public static List<ISecretProvider> Secrets { get; set; }

        /// <summary>
        /// Initialize the SecretsManager
        /// </summary>
        public static void Initialize()
        {
            Secrets = new List<ISecretProvider> {new CrestronSecretsProvider("default")};
            
            CrestronConsole.AddNewConsoleCommand(SetSecretProcess, "setsecret",
                "Adds secrets to secret provider",
                ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(UpdateSecretProcess, "updatesecret",
                "Updates secrets in secret provider",
                ConsoleAccessLevelEnum.AccessAdministrator);

            CrestronConsole.AddNewConsoleCommand(DeleteSecretProcess, "deletesecret",
                "Deletes secrets in secret provider",
                ConsoleAccessLevelEnum.AccessAdministrator);
             

        }

        /// <summary>
        /// Method to return a ISecretProvider to Set, Get, and Delete Secrets
        /// </summary>
        /// <param name="key">Secret Provider Key</param>
        /// <returns></returns>
        public static ISecretProvider GetSecretProviderByKey(string key)
        {
            var secret = Secrets.FirstOrDefault(o => o.Key == key);
            if (secret == null)
            {
                Debug.Console(1, "SecretsManager unable to retrieve SecretProvider with the key '{0}'", key);
            }
            return secret;
        }

        private static void SetSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Adds secrets to secret provider. Format 'setsecret <provider> <secretKey> <secret>";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            if (args.Length == 1 && args[0] == "?")
            {
                response = "Adds secrets to secret provider. Format 'setsecret <provider> <secretKey> <secret>";
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            if (args.Length < 3)
            {
                response =  "Improper number of arguments";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var provider = Secrets.FirstOrDefault(o => o.Key == args[0]);

            if (provider == null)
            {
                //someFail
                response =  "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];
            var secret = args[2];

            if (provider.GetSecret(key) == null)
            {
                provider.SetSecret(key, secret);
                response =
                    String.Format(
                        "Secret successfully set for {0}:{1}",
                        provider.Key, key);
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }
            response =
                String.Format(
                    "Unable to set secret for {0}:{1} - Please use the 'UpdateSecret' command to modify it");
            CrestronConsole.ConsoleCommandResponse(response);
        }

        private static void UpdateSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Updates secrets in secret provider. Format 'updatesecret <provider> <secretKey> <secret>";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            if (args.Length == 1 && args[0] == "?")
            {
                response = "Updates secrets in secret provider. Format 'updatesecret <provider> <secretKey> <secret>";
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

            var provider = Secrets.FirstOrDefault(o => o.Key == args[0]);

            if (provider == null)
            {
                //someFail
                response = "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];
            var secret = args[2];

            if (provider.GetSecret(key) != null)
            {
                provider.SetSecret(key, secret);
                response =
                    String.Format(
                        "Secret successfully updated for {0}:{1}",
                        provider.Key, key);
                CrestronConsole.ConsoleCommandResponse(response);
                return;
            }

            response =
                String.Format(
                    "Unable to update secret for {0}:{1} - Please use the 'SetSecret' command to create a new secret");
            CrestronConsole.ConsoleCommandResponse(response);
        }

        private static void DeleteSecretProcess(string cmd)
        {
            string response;
            var args = cmd.Split(' ');

            if (args.Length == 0)
            {
                //some Instructional Text
                response = "Deletes secrets in secret provider. Format 'deletesecret <provider> <secretKey>";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }
            if (args.Length == 1 && args[0] == "?")
            {
                response = "Deletes secrets in secret provider. Format 'deletesecret <provider> <secretKey>";
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

            var provider = Secrets.FirstOrDefault(o => o.Key == args[0]);

            if (provider == null)
            {
                //someFail
                response =  "Provider key invalid";
                CrestronConsole.ConsoleCommandResponse(response);
                return;

            }

            var key = args[1];


            provider.SetSecret(key, "");
            response =
                String.Format(
                    "Secret successfully deleted for {0}:{1}",
                    provider.Key, key);
            CrestronConsole.ConsoleCommandResponse(response);


        }
    }


}