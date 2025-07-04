using System;


namespace PepperDash.Essentials;

/// <summary>
/// Indicates whether the assembly is compatible with .NET 8.
/// </summary>
/// <remarks>This attribute is used to specify compatibility with .NET 8 for an assembly.  By default, the
/// assembly is considered compatible unless explicitly marked otherwise.</remarks>
/// <param name="isCompatible">A boolean value indicating whether the assembly is compatible with .NET 8.  The default value is <see
/// langword="true"/>.</param>
[AttributeUsage(AttributeTargets.Assembly)]
public class Net8CompatibleAttribute(bool isCompatible = true) : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the current object is compatible with the required conditions.
    /// </summary>
    public bool IsCompatible { get; } = isCompatible;
}