using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// StringExtensions class
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns null if a string is empty, otherwise returns the string
        /// </summary>
        /// <param name="s">string input</param>
        /// <returns>null if the string is emtpy, otherwise returns the string</returns>
        public static string NullIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        /// <summary>
        /// Returns null if a string is empty or made of only whitespace characters, otherwise returns the string
        /// </summary>
        /// <param name="s">string input</param>
        /// <returns>null if the string is wempty or made of only whitespace characters, otherwise returns the string</returns>
        public static string NullIfWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s.Trim()) ? null : s;
        }

        /// <summary>
        /// Returns a replacement string if the input string is empty or made of only whitespace characters, otherwise returns the input string
        /// </summary>
        /// <param name="s">input string</param>
        /// <param name="newString">string to replace with if input string is empty or whitespace</param>
        /// <returns>returns newString if s is null, emtpy, or made of whitespace characters, otherwise returns s</returns>
        public static string ReplaceIfNullOrEmpty(this string s, string newString)
        {
            return string.IsNullOrEmpty(s) ? newString : s;
        }

        /// <summary>
        /// Overload for Contains that allows setting an explicit String Comparison
        /// </summary>
        /// <param name="source">Source String</param>
        /// <param name="toCheck">String to check in Source String</param>
        /// <param name="comp">Comparison parameters</param>
        /// <returns>true of string contains "toCheck"</returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Performs TrimStart() and TrimEnd() on source string
        /// </summary>
        /// <param name="source">String to Trim</param>
        /// <returns>Trimmed String</returns>
        public static string TrimAll(this string source)
        {
            return string.IsNullOrEmpty(source) ? string.Empty : source.TrimStart().TrimEnd();
        }

        /// <summary>
        /// Performs TrimStart(chars char[]) and TrimEnd(chars char[]) on source string.
        /// </summary>
        /// <param name="source">String to Trim</param>
        /// <param name="chars">Char Array to trim from string</param>
        /// <returns>Trimmed String</returns>
        public static string TrimAll(this string source, char[] chars)
        {
            return string.IsNullOrEmpty(source) ? string.Empty : source.TrimStart(chars).TrimEnd(chars);
        }

    }
}