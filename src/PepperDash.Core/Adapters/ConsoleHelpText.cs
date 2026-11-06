using System.Text;

namespace PepperDash.Core.Adapters;

/// <summary>
/// Keeps console command help text within the Crestron console's limit.
/// </summary>
/// <remarks>
/// <c>CrestronConsole.AddNewConsoleCommand</c> throws if the help text is longer than
/// <see cref="MaxBytes"/> bytes ("Help is too long for command ..."). Commands are registered
/// from static constructors (e.g. <c>Debug</c>), where that exception aborts the rest of the
/// constructor, so one long help string can leave logging half-initialized on a processor.
/// Kept free of Crestron types so it can be unit tested.
/// </remarks>
public static class ConsoleHelpText
{
    /// <summary>The longest help text, in bytes, the Crestron console accepts.</summary>
    public const int MaxBytes = 79;

    /// <summary>
    /// Returns <paramref name="helpText"/> unchanged if it fits, otherwise cut to the longest
    /// prefix that fits in <see cref="MaxBytes"/> UTF-8 bytes. Null becomes an empty string.
    /// </summary>
    public static string Fit(string helpText)
    {
        if (string.IsNullOrEmpty(helpText)) return string.Empty;
        if (Encoding.UTF8.GetByteCount(helpText) <= MaxBytes) return helpText;

        var length = helpText.Length;
        while (length > 0 && Encoding.UTF8.GetByteCount(helpText.Substring(0, length)) > MaxBytes)
        {
            length--;
        }

        // Don't leave half of a surrogate pair at the end.
        if (length > 0 && char.IsHighSurrogate(helpText[length - 1])) length--;

        return helpText.Substring(0, length);
    }
}
