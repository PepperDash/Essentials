using System;
using System.Collections.Generic;
using System.Timers;

namespace PepperDash.Core.PasswordManagement;

/// <summary>
/// Allows passwords to be stored and managed
/// </summary>
public class PasswordManager
{
	/// <summary>
	/// Public dictionary of known passwords
	/// </summary>
	public static Dictionary<uint, string> Passwords = new Dictionary<uint, string>();
	/// <summary>
	/// Private dictionary, used when passwords are updated
	/// </summary>
	private Dictionary<uint, string> _passwords = new Dictionary<uint, string>();

	/// <summary>
	/// Timer used to wait until password changes have stopped before updating the dictionary
	/// </summary>
	Timer PasswordTimer;
	/// <summary>
	/// Timer length
	/// </summary>
	public long PasswordTimerElapsedMs = 5000;

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
	/// Event to notify clients of an updated password at the specified index (uint)
	/// </summary>
	public static event EventHandler<StringChangeEventArgs> PasswordChange;

	/// <summary>
	/// Constructor
	/// </summary>
	public PasswordManager()
	{

	}

	/// <summary>
	/// Initialize password manager
	/// </summary>
	public void Initialize()
	{
		if (Passwords == null)
			Passwords = new Dictionary<uint, string>();

		if (_passwords == null)
			_passwords = new Dictionary<uint, string>();

		OnBoolChange(true, 0, PasswordManagementConstants.PasswordInitializedChange);
	}

	/// <summary>
	/// Updates password stored in the dictonary
	/// </summary>
	/// <param name="key"></param>
	/// <param name="password"></param>
	/// <summary>
	/// UpdatePassword method
	/// </summary>
	public void UpdatePassword(ushort key, string password)
	{
		// validate the parameters
		if (key > 0 && string.IsNullOrEmpty(password))
		{
			Debug.LogDebug("PasswordManager.UpdatePassword: key [{0}] or password are not valid", key, password);
			return;
		}

		try
		{
			// if key exists, update the value
			if (_passwords.ContainsKey(key))
				_passwords[key] = password;
			// else add the key & value
			else
				_passwords.Add(key, password);

			Debug.LogDebug("PasswordManager.UpdatePassword: _password[{0}] = {1}", key, _passwords[key]);

			if (PasswordTimer == null)
			{
				PasswordTimer = new Timer(PasswordTimerElapsedMs) { AutoReset = false };
				PasswordTimer.Elapsed += (s, e) => PasswordTimerElapsed(s, e);
				PasswordTimer.Start();
				Debug.LogDebug("PasswordManager.UpdatePassword: Timer Started");
				OnBoolChange(true, 0, PasswordManagementConstants.PasswordUpdateBusyChange);
			}
			else
			{
				PasswordTimer.Stop();
				PasswordTimer.Interval = PasswordTimerElapsedMs;
				PasswordTimer.Start();
				Debug.LogDebug("PasswordManager.UpdatePassword: Timer Reset");
			}
		}
		catch (Exception e)
		{
			var msg = string.Format("PasswordManager.UpdatePassword key-value[{0}, {1}] failed:\r{2}", key, password, e);
			Debug.LogError(e, msg);
			Debug.LogVerbose("Stack Trace:\r{0}", e.StackTrace);
		}
	}

	/// <summary>
	/// Timer callback function
	/// </summary>
	private void PasswordTimerElapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			PasswordTimer.Stop();
			Debug.LogDebug("PasswordManager.PasswordTimerElapsed: Timer Stopped");
			OnBoolChange(false, 0, PasswordManagementConstants.PasswordUpdateBusyChange);
			foreach (var pw in _passwords)
			{
				// if key exists, continue
				if (Passwords.ContainsKey(pw.Key))
				{
					Debug.LogDebug("PasswordManager.PasswordTimerElapsed: pw.key[{0}] = {1}", pw.Key, pw.Value);
					if (Passwords[pw.Key] != _passwords[pw.Key])
					{
						Passwords[pw.Key] = _passwords[pw.Key];
						Debug.LogDebug("PasswordManager.PasswordTimerElapsed: Updated Password[{0} = {1}", pw.Key, Passwords[pw.Key]);
						OnPasswordChange(Passwords[pw.Key], (ushort)pw.Key, PasswordManagementConstants.StringValueChange);
					}
				}
				// else add the key & value
				else
				{
					Passwords.Add(pw.Key, pw.Value);
				}
			}
			OnUshrtChange((ushort)Passwords.Count, 0, PasswordManagementConstants.PasswordManagerCountChange);
		}
		catch (Exception ex)
		{
			var msg = string.Format("PasswordManager.PasswordTimerElapsed failed:\r{0}", ex.Message);
			Debug.LogError(ex, msg);
			Debug.LogVerbose("Stack Trace:\r{0}", ex.StackTrace);
		}
	}

	/// <summary>
	/// Method to change the default timer value, (default 5000ms/5s)
	/// </summary>
	/// <param name="time"></param>
	/// <summary>
	/// PasswordTimerMs method
	/// </summary>
	public void PasswordTimerMs(ushort time)
	{
		PasswordTimerElapsedMs = Convert.ToInt64(time);
	}

	/// <summary>
	/// Helper method for debugging to see what passwords are in the lists
	/// </summary>
	public void ListPasswords()
	{
		Debug.LogInformation("PasswordManager.ListPasswords:\r");
		foreach (var pw in Passwords)
			Debug.LogInformation("Passwords[{0}]: {1}\r", pw.Key, pw.Value);
		Debug.LogInformation("\n");
		foreach (var pw in _passwords)
			Debug.LogInformation("_passwords[{0}]: {1}\r", pw.Key, pw.Value);
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
	/// Protected password change event handler
	/// </summary>
	/// <param name="value"></param>
	/// <param name="index"></param>
	/// <param name="type"></param>
	protected void OnPasswordChange(string value, ushort index, ushort type)
	{
		var handler = PasswordChange;
		if (handler != null)
		{
			var args = new StringChangeEventArgs(value, type);
			args.Index = index;
			PasswordChange(this, args);
		}
	}
}