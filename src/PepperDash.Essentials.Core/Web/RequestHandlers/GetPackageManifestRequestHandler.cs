using System;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
	/// <summary>
	/// Represents a GetPackageManifestRequestHandler
	/// </summary>
	public class GetPackageManifestRequestHandler : WebApiBaseRequestHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// base(true) enables CORS support by default
		/// </remarks>
		public GetPackageManifestRequestHandler()
			: base(true)
		{
		}

		/// <summary>
		/// Handles GET method requests
		/// </summary>
		/// <param name="context"></param>
		protected override void HandleGet(HttpCwsContext context)
		{
			try
			{
				var result = CloneVersionData(ConfigReader.ConfigObject?.Versions) ?? new VersionData();

				PopulateEssentials(result);
				PopulatePackages(result);

				var js = JsonConvert.SerializeObject(result, Formatting.Indented);

				context.Response.StatusCode = 200;
				context.Response.StatusDescription = "OK";
				context.Response.ContentType = "application/json";
				context.Response.ContentEncoding = System.Text.Encoding.UTF8;
				context.Response.Write(js, false);
				context.Response.End();
			}
			catch (Exception ex)
			{
				PepperDash.Core.Debug.LogMessage(ex, "Exception handling GET /packageManifest request");
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.End();
			}
		}

		/// <summary>
		/// Deep-copies the config's VersionData so the live config object is never mutated
		/// </summary>
		private static VersionData CloneVersionData(VersionData source)
		{
			if (source == null)
			{
				return null;
			}

			var json = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<VersionData>(json);
		}

		/// <summary>
		/// Enriches (or creates) the essentials entry from the loaded PepperDash.Essentials.Core assembly
		/// </summary>
		private static void PopulateEssentials(VersionData result)
		{
			var essentials = result.Essentials ?? new NugetVersion();

			essentials.Version = Global.AssemblyVersion;

			// The main program assembly (PackageId "PepperDashEssentials") is what's actually published
			// to NuGet, but this handler lives in PepperDash.Essentials.Core, which can't reference that
			// project's types directly (Essentials -> Core, not the reverse). PluginLoader.EssentialsAssembly.Assembly
			// is unreliable (often left null - see PluginLoader.SetEssentialsAssembly), so look it up
			// directly from the loaded AppDomain by its Directory.Build.props-embedded PackageId metadata
			// (every project's .csproj sets its own PackageId explicitly), falling back to this handler's
			// own (Core) assembly if it can't be found.
			var essentialsAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.FirstOrDefault(a => string.Equals(GetAssemblyMetadataValue(a, "PackageId"), "PepperDashEssentials", StringComparison.OrdinalIgnoreCase))
				?? typeof(GetPackageManifestRequestHandler).Assembly;

			var repoUrl = TrimTrailingGit(GetAssemblyMetadataValue(essentialsAssembly, "RepositoryUrl"));
			if (!string.IsNullOrEmpty(repoUrl))
			{
				essentials.RepoUrl = repoUrl;
			}

			var name = GetAssemblyProduct(essentialsAssembly);
			if (!string.IsNullOrEmpty(name))
			{
				essentials.Name = name;
			}

			// Prefer the PackageId embedded via Directory.Build.props' AssemblyMetadata item (the
			// authoritative source, matching the actual published PackageId) over any config-supplied
			// or hardcoded value.
			var reflectedPackageId = GetAssemblyMetadataValue(essentialsAssembly, "PackageId");
			if (!string.IsNullOrEmpty(reflectedPackageId))
			{
				essentials.PackageId = reflectedPackageId;
			}
			else if (string.IsNullOrEmpty(essentials.PackageId))
			{
				essentials.PackageId = "PepperDashEssentials";
			}

			result.Essentials = essentials;
		}

		/// <summary>
		/// Merges reflection data from loaded plugin assemblies with the config's packages list
		/// </summary>
		private static void PopulatePackages(VersionData result)
		{
			var configPackages = result.Packages ?? new System.Collections.Generic.List<NugetVersion>();
			var matchedConfigPackages = new System.Collections.Generic.HashSet<NugetVersion>();
			var mergedPackages = new System.Collections.Generic.List<NugetVersion>();

			foreach (var loaded in PluginLoader.EssentialsPluginAssemblies.Where(a => a.Assembly != null))
			{
				var reflectedVersion = loaded.Version;
				if (string.IsNullOrEmpty(reflectedVersion))
				{
					// Never emit an entry with no version - the extension's parser drops entries
					// whose version isn't a string.
					continue;
				}

				var reflectedRepoUrl = TrimTrailingGit(GetAssemblyMetadataValue(loaded.Assembly, "RepositoryUrl"));
				var reflectedName = GetAssemblyProduct(loaded.Assembly);

				// Plugins built from a Directory.Build.props that embeds
				// <AssemblyMetadata Include="PackageId" Value="$(PackageId)" /> carry their PackageId
				// directly - this is authoritative and should be preferred over the title/name fallback chain.
				var reflectedPackageId = GetAssemblyMetadataValue(loaded.Assembly, "PackageId");

				var assemblyTitle = GetAssemblyTitle(loaded.Assembly);
				var assemblyName = loaded.Assembly.GetName().Name;
				var assemblyNameNoSeriesSuffix = StripTrailingSeriesSuffix(assemblyName);

				var match = configPackages.FirstOrDefault(p =>
					!matchedConfigPackages.Contains(p) &&
					!string.IsNullOrEmpty(p.PackageId) &&
					(string.Equals(p.PackageId, reflectedPackageId, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(p.PackageId, assemblyTitle, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(p.PackageId, assemblyName, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(p.PackageId, assemblyNameNoSeriesSuffix, StringComparison.OrdinalIgnoreCase)));

				if (match != null)
				{
					matchedConfigPackages.Add(match);

					mergedPackages.Add(new NugetVersion
					{
						Name = !string.IsNullOrEmpty(match.Name) ? match.Name : reflectedName,
						RepoUrl = !string.IsNullOrEmpty(match.RepoUrl) ? match.RepoUrl : reflectedRepoUrl,
						PackageId = !string.IsNullOrEmpty(reflectedPackageId) ? reflectedPackageId : match.PackageId,
						Version = reflectedVersion
					});
				}
				else
				{
					// Loaded but not present (or not matched) in config - emit the reflected PackageId
					// when the assembly carries one, otherwise leave it null as before.
					mergedPackages.Add(new NugetVersion
					{
						Name = reflectedName,
						RepoUrl = reflectedRepoUrl,
						PackageId = reflectedPackageId,
						Version = reflectedVersion,						
					});
				}
			}

			// Configured but not currently loaded - pass through unchanged
			mergedPackages.AddRange(configPackages.Where(p => !matchedConfigPackages.Contains(p)));

			result.Packages = mergedPackages;
		}

		private static string GetAssemblyMetadataValue(Assembly assembly, string key)
		{
			if (assembly == null)
			{
				return null;
			}

			var match = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
				.Cast<AssemblyMetadataAttribute>()
				.FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase));

			return match?.Value;
		}

		private static string GetAssemblyProduct(Assembly assembly)
		{
			if (assembly == null)
			{
				return null;
			}

			var attribute = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)
				.FirstOrDefault() as AssemblyProductAttribute;

			return attribute?.Product;
		}

		private static string GetAssemblyTitle(Assembly assembly)
		{
			if (assembly == null)
			{
				return null;
			}

			var attribute = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
				.FirstOrDefault() as AssemblyTitleAttribute;

			return attribute?.Title;
		}

		private static string StripTrailingSeriesSuffix(string assemblyName)
		{
			const string suffix = ".4Series";

			if (string.IsNullOrEmpty(assemblyName) || !assemblyName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				return assemblyName;
			}

			return assemblyName.Substring(0, assemblyName.Length - suffix.Length);
		}

		private static string TrimTrailingGit(string repoUrl)
		{
			const string suffix = ".git";

			if (string.IsNullOrEmpty(repoUrl) || !repoUrl.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				return repoUrl;
			}

			return repoUrl.Substring(0, repoUrl.Length - suffix.Length);
		}
	}
}
