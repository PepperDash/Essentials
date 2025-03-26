using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.PasswordManagement
{
	/// <summary>
	/// JSON password configuration 
	/// </summary>
	public class PasswordConfig
	{
	    /// <summary>
	    /// Password object configured password
	    /// </summary>
	    public string password { get; set; }
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    public PasswordConfig()
	    {
	        
	    }
	}
}