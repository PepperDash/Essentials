using System;

namespace PepperDash.Core.PasswordManagement
{
    /// <summary>
    /// A class to allow user interaction with the PasswordManager
    /// </summary>
	public class PasswordClient
	{
		/// <summary>
		/// Password selected
		/// </summary>
		public string Password { get; set; }
		/// <summary>
		/// Password selected key
		/// </summary>
		public ushort Key { get; set; }
		/// <summary>
		/// Used to build the password entered by the user
		/// </summary>
		public string PasswordToValidate { get; set; }

		/// <summary>
		/// Boolean event 
		/// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
		/// <summary>
		/// Ushort event
		/// </summary>
		public event EventHandler<UshrtChangeEventArgs> UshrtChange;
		/// <summary>
		/// String event
		/// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;

		/// <summary>
		/// Constructor
		/// </summary>
		public PasswordClient()
		{
			PasswordManager.PasswordChange += new EventHandler<StringChangeEventArgs>(PasswordManager_PasswordChange);
		}		

		/// <summary>
		/// Initialize method
		/// </summary>
		public void Initialize()
		{
			OnBoolChange(false, 0, PasswordManagementConstants.PasswordInitializedChange);

			Password = "";
			PasswordToValidate = "";

			OnUshrtChange((ushort)PasswordManager.Passwords.Count, 0, PasswordManagementConstants.PasswordManagerCountChange);
			OnBoolChange(true, 0, PasswordManagementConstants.PasswordInitializedChange);
		}

		/// <summary>
		/// Retrieve password by index
		/// </summary>
		/// <param name="key"></param>
		public void GetPasswordByIndex(ushort key)
		{
			OnUshrtChange((ushort)PasswordManager.Passwords.Count, 0, PasswordManagementConstants.PasswordManagerCountChange);

			Key = key;

			var pw = PasswordManager.Passwords[Key];
			if (pw == null)
			{
				OnUshrtChange(0, 0, PasswordManagementConstants.PasswordLengthChange);
				return;
			}

			Password = pw;
			OnUshrtChange((ushort)Password.Length, 0, PasswordManagementConstants.PasswordLengthChange);
			OnUshrtChange(key, 0, PasswordManagementConstants.PasswordSelectIndexChange);
		}

		/// <summary>
		/// Password validation method
		/// </summary>
		/// <param name="password"></param>
		public void ValidatePassword(string password)
		{
			if (string.IsNullOrEmpty(password))
				return;

			if (string.Equals(Password, password))
				OnBoolChange(true, 0, PasswordManagementConstants.PasswordValidationChange);
			else
				OnBoolChange(false, 0, PasswordManagementConstants.PasswordValidationChange);

			ClearPassword();
		}

		/// <summary>
		/// Builds the user entered passwrod string, will attempt to validate the user entered
		/// password against the selected password when the length of the 2 are equal
		/// </summary>
		/// <param name="data"></param>
		public void BuildPassword(string data)
		{
			PasswordToValidate = String.Concat(PasswordToValidate, data);
			OnBoolChange(true, (ushort)PasswordToValidate.Length, PasswordManagementConstants.PasswordLedFeedbackChange);

			if (PasswordToValidate.Length == Password.Length)
				ValidatePassword(PasswordToValidate);
		}

		/// <summary>
		/// Clears the user entered password and resets the LEDs
		/// </summary>
		public void ClearPassword()
		{
			PasswordToValidate = "";
			OnBoolChange(false, (ushort)PasswordToValidate.Length, PasswordManagementConstants.PasswordLedFeedbackChange);
		}

		/// <summary>
		/// Protected boolean change event handler
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			var handler = BoolChange;
			if (handler != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		/// <summary>
		/// Protected ushort change event handler
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnUshrtChange(ushort value, ushort index, ushort type)
		{
			var handler = UshrtChange;
			if (handler != null)
			{
				var args = new UshrtChangeEventArgs(value, type);
				args.Index = index;
				UshrtChange(this, args);
			}
		}

		/// <summary>
		/// Protected string change event handler
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			var handler = StringChange;
			if (handler != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}

		/// <summary>
		/// If password changes while selected change event will be notifed and update the client
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected void PasswordManager_PasswordChange(object sender, StringChangeEventArgs args)
		{
			//throw new NotImplementedException();
			if (Key == args.Index)
			{
				//PasswordSelectedKey = args.Index;
				//PasswordSelected = args.StringValue;
				GetPasswordByIndex(args.Index);
			}
		}
	}
}