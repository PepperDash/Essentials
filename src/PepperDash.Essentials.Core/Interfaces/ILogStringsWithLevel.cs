namespace PepperDash.Essentials.Core.Interfaces
{
 /// <summary>
 /// Defines the contract for ILogStringsWithLevel
 /// </summary>
	public interface ILogStringsWithLevel : IKeyed
	{
		/// <summary>
		/// Defines a class that is capable of logging a string with an int level
		/// </summary>
		void SendToLog(IKeyed device, Debug.ErrorLogLevel level,string logMessage);
	}

}