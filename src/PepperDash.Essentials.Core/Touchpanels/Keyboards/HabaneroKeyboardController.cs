using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Touchpanels.Keyboards
{
    /// <summary>
    /// Represents a HabaneroKeyboardController
    /// </summary>
    public class HabaneroKeyboardController
    {
        /// <summary>
        /// Single-key press events, rather than using a built-up text string on the OutputFeedback
        /// </summary>
        public event EventHandler<KeyboardControllerPressEventArgs> KeyPress;

        /// <summary>
        /// Gets or sets the TriList
        /// </summary>
        public BasicTriList TriList { get; private set; }

        /// <summary>
        /// Gets or sets the OutputFeedback
        /// </summary>
        public StringFeedback OutputFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the IsVisible
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Gets or sets the DotComButtonString
        /// </summary>
        public string DotComButtonString { get; set; }

        /// <summary>
        /// Gets or sets the GoButtonText
        /// </summary>
        public string GoButtonText { get; set; }

        /// <summary>
        /// Gets or sets the SecondaryButtonText
        /// </summary>
        public string SecondaryButtonText { get; set; }

        /// <summary>
        /// Gets or sets the GoButtonVisible
        /// </summary>
        public bool GoButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the SecondaryButtonVisible
        /// </summary>
        public bool SecondaryButtonVisible { get; set; }

        int ShiftMode = 0;
        
        StringBuilder Output;

        /// <summary>
        /// Gets or sets the HideAction
        /// </summary>
        public Action HideAction { get; set; }

		CTimer BackspaceTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trilist"></param>
        public HabaneroKeyboardController(BasicTriList trilist)
        {
            TriList = trilist;
            Output = new StringBuilder();
            OutputFeedback = new StringFeedback(() => Output.ToString());
            DotComButtonString = ".com";
        }

        /// <summary>
        /// Show method
        /// </summary>
        public void Show()
        {
            if (IsVisible)
                return;

            TriList.SetSigTrueAction(ClosePressJoin, Hide);
            TriList.SetSigTrueAction(GoButtonPressJoin, () => OnKeyPress(KeyboardSpecialKey.GoButton));
            TriList.SetSigTrueAction(SecondaryButtonPressJoin, () => OnKeyPress(KeyboardSpecialKey.SecondaryButton));
            TriList.SetSigTrueAction(2921, () => Press(A(ShiftMode)));
            TriList.SetSigTrueAction(2922, () => Press(B(ShiftMode)));
            TriList.SetSigTrueAction(2923, () => Press(C(ShiftMode)));
            TriList.SetSigTrueAction(2924, () => Press(D(ShiftMode)));
            TriList.SetSigTrueAction(2925, () => Press(E(ShiftMode)));
            TriList.SetSigTrueAction(2926, () => Press(F(ShiftMode)));
            TriList.SetSigTrueAction(2927, () => Press(G(ShiftMode)));
            TriList.SetSigTrueAction(2928, () => Press(H(ShiftMode)));
            TriList.SetSigTrueAction(2929, () => Press(I(ShiftMode)));
            TriList.SetSigTrueAction(2930, () => Press(J(ShiftMode)));
            TriList.SetSigTrueAction(2931, () => Press(K(ShiftMode)));
            TriList.SetSigTrueAction(2932, () => Press(L(ShiftMode)));
            TriList.SetSigTrueAction(2933, () => Press(M(ShiftMode)));
            TriList.SetSigTrueAction(2934, () => Press(N(ShiftMode)));
            TriList.SetSigTrueAction(2935, () => Press(O(ShiftMode)));
            TriList.SetSigTrueAction(2936, () => Press(P(ShiftMode)));
            TriList.SetSigTrueAction(2937, () => Press(Q(ShiftMode)));
            TriList.SetSigTrueAction(2938, () => Press(R(ShiftMode)));
            TriList.SetSigTrueAction(2939, () => Press(S(ShiftMode)));
            TriList.SetSigTrueAction(2940, () => Press(T(ShiftMode)));
            TriList.SetSigTrueAction(2941, () => Press(U(ShiftMode)));
            TriList.SetSigTrueAction(2942, () => Press(V(ShiftMode)));
            TriList.SetSigTrueAction(2943, () => Press(W(ShiftMode)));
            TriList.SetSigTrueAction(2944, () => Press(X(ShiftMode)));
            TriList.SetSigTrueAction(2945, () => Press(Y(ShiftMode)));
            TriList.SetSigTrueAction(2946, () => Press(Z(ShiftMode)));
            TriList.SetSigTrueAction(2947, () => Press('.'));
            TriList.SetSigTrueAction(2948, () => Press('@'));
            TriList.SetSigTrueAction(2949, () => Press(' '));
			TriList.SetSigHeldAction(2950, 500, StartBackspaceRepeat, StopBackspaceRepeat, Backspace);
			//TriList.SetSigTrueAction(2950, Backspace);
            TriList.SetSigTrueAction(2951, Shift);
            TriList.SetSigTrueAction(2952, NumShift);
            TriList.SetSigTrueAction(2953, Clear);
            TriList.SetSigTrueAction(2954, () => Press(DotComButtonString));

            TriList.SetBool(GoButtonVisibleJoin, GoButtonVisible);
            TriList.SetString(GoButtonTextJoin, GoButtonText);
            TriList.SetBool(SecondaryButtonVisibleJoin, SecondaryButtonVisible);
            TriList.SetString(SecondaryButtonTextJoin, SecondaryButtonText);

            TriList.SetBool(KeyboardVisible, true);
            ShowKeys();
            IsVisible = true;
        }

        /// <summary>
        /// Hide method
        /// </summary>
        public void Hide()
        {
            if (!IsVisible)
                return;

            for (uint i = 2901; i < 2970; i++)
                TriList.ClearBoolSigAction(i);

            // run attached actions
            if(HideAction != null)
                HideAction();

            TriList.SetBool(KeyboardVisible, false);
            IsVisible = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <summary>
        /// Press method
        /// </summary>
        public void Press(char c)
        {
            OnKeyPress(c.ToString());
            Output.Append(c);
            OutputFeedback.FireUpdate();
            ResetShift();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <summary>
        /// Press method
        /// </summary>
        public void Press(string s)
        {
            OnKeyPress(s);
            Output.Append(s);
            OutputFeedback.FireUpdate();
            ResetShift();
        }

        /// <summary>
        /// EnableGoButton method
        /// </summary>
        public void EnableGoButton()
        {
            TriList.SetBool(GoButtonEnableJoin, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisableGoButton()
        {
            TriList.SetBool(GoButtonEnableJoin, false);
        }

        void ResetShift()
        {
            if (ShiftMode == 1)
            {
                ShiftMode = 0;
                ShowKeys();
            }
            else if (ShiftMode == 3)
            {
                ShiftMode = 2;
                ShowKeys();
            }
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

		/// <summary>
		/// Does what it says
		/// </summary>
		void StartBackspaceRepeat()
		{
			if (BackspaceTimer == null)
			{
				BackspaceTimer = new CTimer(o => Backspace(), null, 0, 175);
			}
		}

		/// <summary>
		/// Does what it says
		/// </summary>
		void StopBackspaceRepeat()
		{
			if (BackspaceTimer != null)
			{
				BackspaceTimer.Stop();
				BackspaceTimer = null;
			}
		}

        void Backspace()
        {
            OnKeyPress(KeyboardSpecialKey.Backspace);

            if (Output.Length > 0)
            {
                Output.Remove(Output.Length - 1, 1);
                OutputFeedback.FireUpdate();
            }
        }

        void Clear()
        {
            OnKeyPress(KeyboardSpecialKey.Clear);

            Output.Remove(0, Output.Length);
            OutputFeedback.FireUpdate();
        }

        /* When in mode 0 (lowercase):
         *      shift button: up arrow 0
         *      numShift button: 123/#$@#$ 0
         *      
         *      - shift --> mode 1
         *      - double-tap shift --> caps lock
         *      - numShift --> mode 2
         *      
         * mode 1 (uppercase)
         *      shift button: down arrow 1
         *      numShift button: 123/##$# 0
         *      
         *      - shift --> mode 0
         *      - numShift --> mode 2
         *      
         *      - Tapping any key will go back to mode 0
         * 
         * mode 2 (numbers-sym)
         *      Shift button: #$#$#$ 2
         *      numShift: ABC 1
         *      
         *      - shift --> mode 3
         *      - double-tap shift --> caps lock
         *      - numShift --> mode 0
         * 
         * mode 3 (sym)
         *      Shift button: 123 3
         *      numShift: ABC 1
         *      
         *      - shift --> mode 2
         *      - numShift --> mode 0
         *      
         *      - Tapping any key will go back to mode 2
         */
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

            ShowKeys();
        }

        void NumShift()
        {
            if (ShiftMode == 0 || ShiftMode == 1)
                ShiftMode = 2;
            else if (ShiftMode == 2 || ShiftMode == 3)
                ShiftMode = 0;
            ShowKeys();
        }

        void ShowKeys()
        {
            TriList.SetString(2921, A(ShiftMode).ToString());
            TriList.SetString(2922, B(ShiftMode).ToString());
            TriList.SetString(2923, C(ShiftMode).ToString());
            TriList.SetString(2924, D(ShiftMode).ToString());
            TriList.SetString(2925, E(ShiftMode).ToString());
            TriList.SetString(2926, F(ShiftMode).ToString());
            TriList.SetString(2927, G(ShiftMode).ToString());
            TriList.SetString(2928, H(ShiftMode).ToString());
            TriList.SetString(2929, I(ShiftMode).ToString());
            TriList.SetString(2930, J(ShiftMode).ToString());
            TriList.SetString(2931, K(ShiftMode).ToString());
            TriList.SetString(2932, L(ShiftMode).ToString());
            TriList.SetString(2933, M(ShiftMode).ToString());
            TriList.SetString(2934, N(ShiftMode).ToString());
            TriList.SetString(2935, O(ShiftMode).ToString());
            TriList.SetString(2936, P(ShiftMode).ToString());
            TriList.SetString(2937, Q(ShiftMode).ToString());
            TriList.SetString(2938, R(ShiftMode).ToString());
            TriList.SetString(2939, S(ShiftMode).ToString());
            TriList.SetString(2940, T(ShiftMode).ToString());
            TriList.SetString(2941, U(ShiftMode).ToString());
            TriList.SetString(2942, V(ShiftMode).ToString());
            TriList.SetString(2943, W(ShiftMode).ToString());
            TriList.SetString(2944, X(ShiftMode).ToString());
            TriList.SetString(2945, Y(ShiftMode).ToString());
            TriList.SetString(2946, Z(ShiftMode).ToString());
            TriList.SetString(2954, DotComButtonString);

            TriList.SetUshort(2951, (ushort)ShiftMode); // 0 = up, 1 = down, 2 = #, 3 = 123 
            TriList.SetUshort(2952, (ushort)(ShiftMode < 2 ? 0 : 1)); // 0 = #, 1 = abc
        }

        /// <summary>
        /// Event fire helper for text 
        /// </summary>
        /// <param name="text"></param>
        void OnKeyPress(string text)
        {
            var handler = KeyPress;
            if (handler != null)
                KeyPress(this, new KeyboardControllerPressEventArgs(text));
        }

        /// <summary>
        /// event helper for special keys
        /// </summary>
        /// <param name="key"></param>
        void OnKeyPress(KeyboardSpecialKey key)
        {
            var handler = KeyPress;
            if (handler != null)
                KeyPress(this, new KeyboardControllerPressEventArgs(key));
        }


        /// <summary>
        /// 2901
        /// </summary>
        public const uint KeyboardVisible = 2901;
        /// <summary>
        /// 2902
        /// </summary>
        public const uint ClosePressJoin = 2902;
        /// <summary>
        /// 2903
        /// </summary>
        public const uint GoButtonPressJoin = 2903;
        /// <summary>
        /// 2903
        /// </summary>
        public const uint GoButtonTextJoin = 2903;
        /// <summary>
        /// 2904
        /// </summary>
        public const uint SecondaryButtonPressJoin = 2904;
        /// <summary>
        /// 2904
        /// </summary>
        public const uint SecondaryButtonTextJoin = 2904;        
        /// <summary>
        /// 2905
        /// </summary>
        public const uint GoButtonVisibleJoin = 2905;
        /// <summary>
        /// 2906
        /// </summary>
        public const uint SecondaryButtonVisibleJoin = 2906;
        /// <summary>
        /// 2907
        /// </summary>
        public const uint GoButtonEnableJoin = 2907;
        /// <summary>
        /// 2910
        /// </summary>
        public const uint ClearPressJoin = 2910;
        /// <summary>
        /// 2911
        /// </summary>
        public const uint ClearVisibleJoin = 2911;

    }

    /// <summary>
    /// Event args for keyboard key presses
    /// </summary>
    public class KeyboardControllerPressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Text
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets or sets the SpecialKey
        /// </summary>
        public KeyboardSpecialKey SpecialKey { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        public KeyboardControllerPressEventArgs(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">special keyboard key</param>
        public KeyboardControllerPressEventArgs(KeyboardSpecialKey key)
        {
            SpecialKey = key;
        }
    }

    /// <summary>
    /// Enumeration of KeyboardSpecialKey values
    /// </summary>
    public enum KeyboardSpecialKey
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0, 
        
        /// <summary>
        /// Backspace
        /// </summary>
        Backspace, 
        
        /// <summary>
        /// Clear
        /// </summary>
        Clear, 
        
        /// <summary>
        /// GoButton
        /// </summary>
        GoButton, 
        
        /// <summary>
        /// SecondaryButton
        /// </summary>
        SecondaryButton
    }
}