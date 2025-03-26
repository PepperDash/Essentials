using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.WebApi.Presets
{
	/// <summary>
	/// 
	/// </summary>
	public class User
	{
        /// <summary>
        /// 
        /// </summary>
		public int Id { get; set; }
		
        /// <summary>
        /// 
        /// </summary>
		public string ExternalId { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public string LastName { get; set; }
	}


	/// <summary>
	/// 
	/// </summary>
	public class UserReceivedEventArgs : EventArgs
	{
        /// <summary>
        /// True when user is found
        /// </summary>
        public bool LookupSuccess { get; private set; }

        /// <summary>
        /// For stupid S+
        /// </summary>
        public ushort ULookupSuccess { get { return (ushort)(LookupSuccess ? 1 : 0); } }

        /// <summary>
        /// 
        /// </summary>
		public User User { get; private set; }

		/// <summary>
		/// For Simpl+
		/// </summary>
		public UserReceivedEventArgs() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user"></param>
        /// <param name="success"></param>
		public UserReceivedEventArgs(User user, bool success)
		{
            LookupSuccess = success;
			User = user;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class UserAndRoomMessage
	{
        /// <summary>
        /// 
        /// </summary>
		public int UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public int RoomTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public int PresetNumber { get; set; }
	}
}