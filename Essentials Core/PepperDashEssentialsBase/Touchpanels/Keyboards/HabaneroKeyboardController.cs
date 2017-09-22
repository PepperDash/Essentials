using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Touchpanels.Keyboards
{
    public class HabaneroKeyboardController
    {
        public BasicTriList TriList { get; private set; }

        int ShiftMode;

        public StringFeedback OutputFeedback { get; private set; }
        StringBuilder Output;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trilist"></param>
        public HabaneroKeyboardController(BasicTriList trilist)
        {
            TriList = trilist;
            Output = new StringBuilder();
            OutputFeedback = new StringFeedback(() => Output.ToString());

        }

        /// <summary>
        /// Puts actions on buttons
        /// </summary>
        void SetUp()
        {
            TriList.SetSigTrueAction(KeyboardClosePress, Hide);

            TriList.SetSigTrueAction(2921, () => Append(A(ShiftMode)));
            TriList.SetSigTrueAction(2922, () => Append(B(ShiftMode)));
            TriList.SetSigTrueAction(2923, () => Append(C(ShiftMode)));
            TriList.SetSigTrueAction(2924, () => Append(D(ShiftMode)));
            TriList.SetSigTrueAction(2925, () => Append(E(ShiftMode)));
            TriList.SetSigTrueAction(2926, () => Append(F(ShiftMode)));
            TriList.SetSigTrueAction(2927, () => Append(G(ShiftMode)));
            TriList.SetSigTrueAction(2928, () => Append(H(ShiftMode)));
            TriList.SetSigTrueAction(2929, () => Append(I(ShiftMode)));
            TriList.SetSigTrueAction(2930, () => Append(J(ShiftMode)));
            TriList.SetSigTrueAction(2931, () => Append(K(ShiftMode)));
            TriList.SetSigTrueAction(2932, () => Append(L(ShiftMode)));
            TriList.SetSigTrueAction(2933, () => Append(M(ShiftMode)));
            TriList.SetSigTrueAction(2934, () => Append(N(ShiftMode)));
            TriList.SetSigTrueAction(2935, () => Append(O(ShiftMode)));
            TriList.SetSigTrueAction(2936, () => Append(P(ShiftMode)));
            TriList.SetSigTrueAction(2937, () => Append(Q(ShiftMode)));
            TriList.SetSigTrueAction(2938, () => Append(R(ShiftMode)));
            TriList.SetSigTrueAction(2939, () => Append(S(ShiftMode)));
            TriList.SetSigTrueAction(2940, () => Append(T(ShiftMode)));
            TriList.SetSigTrueAction(2941, () => Append(U(ShiftMode)));
            TriList.SetSigTrueAction(2942, () => Append(V(ShiftMode)));
            TriList.SetSigTrueAction(2943, () => Append(W(ShiftMode)));
            TriList.SetSigTrueAction(2944, () => Append(X(ShiftMode)));
            TriList.SetSigTrueAction(2945, () => Append(Y(ShiftMode)));
            TriList.SetSigTrueAction(2946, () => Append(Z(ShiftMode)));
            TriList.SetSigTrueAction(2947, () => Append('.'));
            TriList.SetSigTrueAction(2948, () => Append('@'));
            TriList.SetSigTrueAction(2949, () => Append(' '));
            TriList.SetSigTrueAction(2950, Backspace);
            TriList.SetSigTrueAction(2951, Clear);
            TriList.SetSigTrueAction(2952, Shift);
            TriList.SetSigTrueAction(2953, NumShift);

        }

        public void Show()
        {
            TriList.SetBool(KeyboardVisible, true);
        }

        public void Hide()
        {
            TriList.SetBool(KeyboardVisible, false);
        }

        void Append(char c)
        {
            Output.Append(c);
            OutputFeedback.FireUpdate();
        }

        char A(int i) { return new char[] { 'a', 'A', '?', '?' }[i]; }
        char B(int i) { return new char[] { 'b', 'B', ':', ':' }[i]; }
        char C(int i) { return new char[] { 'c', 'C', '>', '>' }[i]; }
        char D(int i) { return new char[] { 'd', 'D', '_', '_' }[i]; }
        char E(int i) { return new char[] { 'e', 'E', '3', '#' }[i]; }
        char F(int i) { return new char[] { 'f', 'F', '=', '=' }[i]; }
        char G(int i) { return new char[] { 'g', 'G', '+', '+' }[i]; }
        char H(int i) { return new char[] { 'h', 'H', '[', '[' }[i]; }
        char I(int i) { return new char[] { 'i', 'I', '8', '*' }[i]; }
        char J(int i) { return new char[] { 'j', 'J', ']', ']' }[i]; }
        char K(int i) { return new char[] { 'k', 'K', '/', '/' }[i]; }
        char L(int i) { return new char[] { 'l', 'L', '\\', '\\' }[i]; }
        char M(int i) { return new char[] { 'm', 'M', '"', '"' }[i]; }
        char N(int i) { return new char[] { 'n', 'N', '\'', '\'' }[i]; }
        char O(int i) { return new char[] { 'o', 'O', '9', '(' }[i]; }
        char P(int i) { return new char[] { 'p', 'P', '0', ')' }[i]; }
        char Q(int i) { return new char[] { 'q', 'Q', '1', '!' }[i]; }
        char R(int i) { return new char[] { 'r', 'R', '4', '$' }[i]; }
        char S(int i) { return new char[] { 's', 'S', '-', '-' }[i]; }
        char T(int i) { return new char[] { 't', 'T', '5', '%' }[i]; }
        char U(int i) { return new char[] { 'u', 'U', '7', '&' }[i]; }
        char V(int i) { return new char[] { 'v', 'V', ';', ';' }[i]; }
        char W(int i) { return new char[] { 'w', 'W', '2', '@' }[i]; }
        char X(int i) { return new char[] { 'x', 'X', '<', '<' }[i]; }
        char Y(int i) { return new char[] { 'y', 'Y', '6', '^' }[i]; }
        char Z(int i) { return new char[] { 'z', 'Z', ',', ',' }[i]; }

        void Backspace()
        {
            if (Output.Length > 0)
            {
                Output.Remove(Output.Length - 1, 1);
                OutputFeedback.FireUpdate();
            }
        }

        void Clear()
        {
            Output.Remove(0, Output.Length);
            OutputFeedback.FireUpdate();
        }

        void Shift()
        {
            if (ShiftMode == 0)
                ShiftMode = 1;
            else if (ShiftMode == 1)
                ShiftMode = 0;
            else if (ShiftMode == 2)
                ShiftMode = 3;
            else
                ShiftMode = 2;

            TriList.SetUshort(2951, 0); // 0 = up, 1 = down, 2 = #, 3 = 123 
            TriList.SetUshort(2952, 0); // 0 = #, 1 = abc
        }

        void NumShift()
        {
            if (ShiftMode == 0 || ShiftMode == 1)
                ShiftMode = 2;
            else if (ShiftMode == 2)
                ShiftMode = 3;
            else
                ShiftMode = 0;
        }


        /// <summary>
        /// 2901
        /// </summary>
        public const uint KeyboardVisible = 2901;
        /// <summary>
        /// 2902
        /// </summary>
        public const uint KeyboardClosePress = 2902;
        /// <summary>
        /// 2903
        /// </summary>
        public const uint KeyboardButton1Press = 2903;
        /// <summary>
        /// 2904
        /// </summary>
        public const uint KeyboardButton1Press = 2904;
        /// <summary>
        /// 2910
        /// </summary>
        public const uint KeyboardClearPress = 2910;
        /// <summary>
        /// 2911
        /// </summary>
        public const uint KeyboardClearVisible = 2911;

    }
}