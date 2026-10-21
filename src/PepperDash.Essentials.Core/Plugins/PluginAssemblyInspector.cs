// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace PepperDash.Essentials;

/// <summary>
/// An assembly a plugin references, as recorded in its metadata.
/// </summary>
public sealed class AssemblyReferenceInfo
{
    /// <summary>Creates a reference.</summary>
    public AssemblyReferenceInfo(string name, Version version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>The referenced assembly's simple name, e.g. <c>Crestron.SimplSharpPro.UI</c>.</summary>
    public string Name { get; }

    /// <summary>The assembly version the plugin was compiled against.</summary>
    public Version Version { get; }
}

/// <summary>
/// What a plugin assembly says about itself and how it was built, read without loading it.
/// </summary>
public sealed class PluginBuildInfo
{
    /// <summary>The assembly's simple name.</summary>
    public string AssemblyName { get; set; }

    /// <summary>The assembly version.</summary>
    public Version AssemblyVersion { get; set; }

    /// <summary>The informational version, which keeps any prerelease label. May be null.</summary>
    public string InformationalVersion { get; set; }

    /// <summary>
    /// The Essentials package versions the plugin was built against, keyed by package ID, read from the
    /// <c>AssemblyMetadata</c> attributes the Essentials NuGet packages stamp into plugins.
    /// </summary>
    public Dictionary<string, string> BuiltAgainst { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The assemblies the plugin references.</summary>
    public List<AssemblyReferenceInfo> References { get; } = new List<AssemblyReferenceInfo>();

    /// <summary>The full names of the types the plugin references from other assemblies.</summary>
    public List<string> TypeReferences { get; } = new List<string>();

    /// <summary>The plugin's own version: the informational version if present, otherwise the assembly version.</summary>
    public string DisplayVersion => !string.IsNullOrEmpty(InformationalVersion)
        ? InformationalVersion
        : AssemblyVersion?.ToString();
}

/// <summary>
/// Reads a plugin assembly's metadata without loading it into the runtime.
/// </summary>
public static class PluginAssemblyInspector
{
    /// <summary>
    /// The <c>AssemblyMetadata</c> key prefix the Essentials NuGet packages use to record which version a
    /// plugin was built against. The package ID follows the prefix, e.g.
    /// <c>PepperDash.Essentials.BuiltAgainst:PepperDashEssentials</c>.
    /// </summary>
    public const string BuiltAgainstMetadataPrefix = "PepperDash.Essentials.BuiltAgainst:";

    /// <summary>
    /// Reads a plugin assembly from disk.
    /// </summary>
    /// <exception cref="BadImageFormatException">The file is not a .NET assembly.</exception>
    public static PluginBuildInfo Inspect(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Inspect(stream);
    }

    /// <summary>
    /// Reads a plugin assembly from a stream.
    /// </summary>
    /// <exception cref="BadImageFormatException">The stream is not a .NET assembly.</exception>
    public static PluginBuildInfo Inspect(Stream stream)
    {
        using var peReader = new PEReader(stream, PEStreamOptions.LeaveOpen);

        if (!peReader.HasMetadata)
            throw new BadImageFormatException("Not a valid .NET assembly");

        var reader = peReader.GetMetadataReader();

        if (!reader.IsAssembly)
            throw new BadImageFormatException("Not a valid .NET assembly");

        var definition = reader.GetAssemblyDefinition();
        var info = new PluginBuildInfo
        {
            AssemblyName = reader.GetString(definition.Name),
            AssemblyVersion = definition.Version
        };

        foreach (var handle in reader.AssemblyReferences)
        {
            var reference = reader.GetAssemblyReference(handle);
            info.References.Add(new AssemblyReferenceInfo(reader.GetString(reference.Name), reference.Version));
        }

        foreach (var handle in reader.TypeReferences)
        {
            var typeReference = reader.GetTypeReference(handle);
            info.TypeReferences.Add($"{reader.GetString(typeReference.Namespace)}.{reader.GetString(typeReference.Name)}");
        }

        foreach (var handle in definition.GetCustomAttributes())
        {
            ReadAssemblyAttribute(reader, reader.GetCustomAttribute(handle), info);
        }

        return info;
    }

    private static void ReadAssemblyAttribute(MetadataReader reader, CustomAttribute attribute, PluginBuildInfo info)
    {
        var typeName = GetAttributeTypeName(reader, attribute);

        if (typeName != "System.Reflection.AssemblyInformationalVersionAttribute"
            && typeName != "System.Reflection.AssemblyMetadataAttribute")
            return;

        try
        {
            var blob = reader.GetBlobReader(attribute.Value);

            // Custom attribute blobs start with the 0x0001 prolog
            if (blob.ReadUInt16() != 1)
                return;

            if (typeName == "System.Reflection.AssemblyInformationalVersionAttribute")
            {
                info.InformationalVersion = blob.ReadSerializedString();
                return;
            }

            var key = blob.ReadSerializedString();
            var value = blob.ReadSerializedString();

            if (key != null
                && key.StartsWith(BuiltAgainstMetadataPrefix, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(value))
            {
                info.BuiltAgainst[key.Substring(BuiltAgainstMetadataPrefix.Length)] = value;
            }
        }
        catch (BadImageFormatException)
        {
            // A malformed attribute blob only costs us this one attribute
        }
    }

    private static string GetAttributeTypeName(MetadataReader reader, CustomAttribute attribute)
    {
        EntityHandle parent;

        switch (attribute.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                parent = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;
                break;
            case HandleKind.MethodDefinition:
                parent = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();
                break;
            default:
                return null;
        }

        switch (parent.Kind)
        {
            case HandleKind.TypeReference:
                var typeReference = reader.GetTypeReference((TypeReferenceHandle)parent);
                return $"{reader.GetString(typeReference.Namespace)}.{reader.GetString(typeReference.Name)}";
            case HandleKind.TypeDefinition:
                var typeDefinition = reader.GetTypeDefinition((TypeDefinitionHandle)parent);
                return $"{reader.GetString(typeDefinition.Namespace)}.{reader.GetString(typeDefinition.Name)}";
            default:
                return null;
        }
    }
}
