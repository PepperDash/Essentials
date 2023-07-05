using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }
        public static string NullIfWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s.Trim()) ? null : s;
        }
        public static string ReplaceIfNullOrEmpty(this string s, string newString)
        {
            return string.IsNullOrEmpty(s) ? newString : s;
        }

        /// <summary>
        /// Formats string to meet specified parameters
        /// </summary>
        /// <param name="inputString">string to be formatted</param>
        /// <param name="outputStringLength">length of output string</param>
        /// <param name="padCharacter">character to pad string with to reach desired output length</param>
        /// <param name="justification">Justification of input string with regards to the overall string</param>
        /// <param name="separateInput">If true, seperate input string from pad characters with a single space</param>
        /// <returns></returns>
        public static string AutoPadAndJustify(this string inputString, int outputStringLength, char padCharacter,
                    AutoPadJustification justification, bool separateInput)
        {
            var returnString = inputString;
            var justifiedAndSeparateLength = (
                separateInput
                    ? (justification == AutoPadJustification.Center ? 2 : 1)
                    : 0);
            if (outputStringLength <= inputString.Length + justifiedAndSeparateLength) return returnString;
            var fillLength = outputStringLength - inputString.Length - justifiedAndSeparateLength;
            switch (justification)
            {
                case (AutoPadJustification.Left):
                    {
                        returnString =
                            inputString +
                            new string(' ', separateInput ? 1 : 0) +
                            new string(padCharacter, fillLength);
                        break;
                    }
                case (AutoPadJustification.Right):
                    {
                        returnString =
                            new string(padCharacter, fillLength) +
                            new string(' ', separateInput ? 1 : 0) +
                            inputString;
                        break;
                    }
                case (AutoPadJustification.Center):
                    {
                        var halfFill = fillLength / 2;
                        returnString =
                            new string(padCharacter, halfFill + (fillLength % 2)) +
                            new string(' ', separateInput ? 1 : 0) +
                            inputString +
                            new string(' ', separateInput ? 1 : 0) +
                            new string(padCharacter, halfFill);
                        break;
                    }
            }
            return returnString;
        }

        /// <summary>
        /// Formats string to meet specified parameters
        /// </summary>
        /// <param name="inputString">string to be formatted</param>
        /// <param name="options">String formatting options</param>
        /// <returns></returns>
        public static string AutoPadAndJustify(this string inputString, AutoPadJustificationOptions options)
        {
            if (options == null)
                return inputString;

            var outputStringLength = options.OutputStringLength;
            var padCharacter = options.PadCharacter;
            var justification = options.Justification;
            var separateInput = options.SeparateInput;



            return AutoPadAndJustify(inputString, outputStringLength, padCharacter, justification,
                separateInput);
        }
    }

    public enum AutoPadJustification
    {
        Center,
        Left,
        Right
    }

    /// <summary>
    /// Options for setting AutoPadJustification
    /// </summary>
    public class AutoPadJustificationOptions
    {
        /// <summary>
        /// Text Justification for the string, relative to the length
        /// </summary>
        public AutoPadJustification Justification { get; set; }
        /// <summary>
        /// If true, separate input string from pad characters by a single ' ' character
        /// </summary>
        public bool SeparateInput { get; set; }
        /// <summary>
        /// Pad character to be inserted
        /// </summary>
        public char PadCharacter { get; set; }
        /// <summary>
        /// Total length of the output string
        /// </summary>
        public int OutputStringLength { get; set; }
    }
}