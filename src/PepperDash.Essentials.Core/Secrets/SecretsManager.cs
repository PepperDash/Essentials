using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Manages secret providers and their associated secrets. Provides methods to initialize, add, retrieve, and manage secret providers.
/// </summary>
public static class SecretsManager
{
    private static readonly object SecretsLock = new object();
    private static readonly Dictionary<string, ISecretProvider> _secrets =
        new Dictionary<string, ISecretProvider>();

    /// <summary>
    /// The collection of secret providers, keyed by their unique identifier.
    /// </summary>
    /// <remarks>
    /// Returns a snapshot. The backing dictionary is mutated from both the console thread and CWS
    /// request threads, so handing out the live instance would let a caller enumerate it while it
    /// is being written.
    /// </remarks>
    public static Dictionary<string, ISecretProvider> Secrets
    {
        get
        {
            lock (SecretsLock)
            {
                return new Dictionary<string, ISecretProvider>(_secrets);
            }
        }
    }

    /// <summary>
    /// Initialize the SecretsManager
    /// </summary>
    public static void Initialize()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError(e, "SecretsManager Initialize failed");
        }
    }



    /// <summary>
    /// Get Secret Provider from dictionary by key
    /// </summary>
    /// <param name="key">Dictionary Key for provider</param>
    /// <returns>ISecretProvider</returns>
    public static ISecretProvider GetSecretProviderByKey(string key)
    {
        ISecretProvider secret;

        lock (SecretsLock)
        {
            _secrets.TryGetValue(key, out secret);
        }

        if (secret == null)
        {
            Debug.LogMessage(LogEventLevel.Debug, "SecretsManager unable to retrieve SecretProvider with the key '{0}'", key);
        }
        return secret;
    }

    /// <summary>
    /// Gets information about a specific secrets provider.
    /// </summary>
    /// <param name="cmd"></param>
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
    public static void AddSecretProvider(string key, ISecretProvider provider)
    {
        lock (SecretsLock)
        {
            if (!_secrets.ContainsKey(key))
            {
                _secrets.Add(key, provider);
                Debug.LogMessage(LogEventLevel.Debug, "Secrets provider '{0}' added to SecretsManager", key);
                return;
            }
        }
        Debug.LogMessage(LogEventLevel.Information, "Unable to add Provider '{0}' to Secrets.  Provider with that key already exists", key );
    }

    /// <summary>
    /// Add secret provider to secrets dictionary, with optional overwrite parameter
    /// </summary>
    /// <param name="key">Key of new entry</param>
    /// <param name="provider">New provider entry</param>
    /// <param name="overwrite">true to overwrite any existing providers in the dictionary</param>
    public static void AddSecretProvider(string key, ISecretProvider provider, bool overwrite)
    {
        lock (SecretsLock)
        {
            if (!_secrets.ContainsKey(key))
            {
                _secrets.Add(key, provider);
                Debug.LogMessage(LogEventLevel.Debug, "Secrets provider '{0}' added to SecretsManager", key);
                return;
            }
        }
        if (overwrite)
        {
            // Indexer, not Add: Add on an existing key throws, so this branch never overwrote
            // anything - it threw ArgumentException instead.
            lock (SecretsLock)
            {
                _secrets[key] = provider;
            }
            Debug.LogMessage(LogEventLevel.Debug, "Provider with the key '{0}' already exists in secrets.  Overwriting with new secrets provider.", key);
            return;
        }
        Debug.LogMessage(LogEventLevel.Information, "Unable to add Provider '{0}' to Secrets.  Provider with that key already exists", key);
    }

    private static void SetSecretProcess(string cmd)
    {
        string response;
        var args = cmd.Split(' ');

        if (cmd.Length == 0)
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

        if (cmd.Length == 0)
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
                    "Unable to update secret for {0}:{1} - Please use the 'SetSecret' command to modify it",
                    provider.Key, key);
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
                    "Unable to set secret for {0}:{1} - Please use the 'UpdateSecret' command to modify it",
                    provider.Key, key);
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

        if (cmd.Length == 0)
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

        // A blank key is not a harmless no-op: it reaches clearLocal("")/clearGlobal(""), which
        // deletes EVERY record belonging to this application - including Mobile Control's pairing
        // tokens. "deletesecret default " with a trailing space lands here.
        if (String.IsNullOrWhiteSpace(key))
        {
            CrestronConsole.ConsoleCommandResponse("A secret key is required");
            return;
        }

        // Prefer the guarded delete where the provider supports it; it reports why the store
        // refused rather than collapsing every failure into a bare false.
        bool deleted;
        if (provider is IEnumerableSecretProvider enumerable)
        {
            var result = enumerable.DeleteSecret(key);
            deleted = result.Success;
            if (!deleted && !String.IsNullOrEmpty(result.Message))
            {
                CrestronConsole.ConsoleCommandResponse(result.Message);
                return;
            }
        }
        else
        {
            // Called once. Calling it twice - as this did - means the tested call runs against an
            // already-deleted record and reports failure for a delete that worked.
            deleted = provider.SetSecret(key, "");
        }

        response = deleted
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