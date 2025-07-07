using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// Provides in use tracking.  Objects can register with this.  InUseFeedback can provide 
	/// events when usage changes.
	/// </summary>
	public class InUseTracking
	{
		/// <summary>
		/// Returns a copied list of all users of this tracker.
		/// </summary>
		public IEnumerable<InUseTrackingObject> Users { get { return new List<InUseTrackingObject>(_Users); } }
		List<InUseTrackingObject> _Users = new List<InUseTrackingObject>();

		/// <summary>
		/// Feedback that changes when this goes in/out of use
		/// </summary>
		public BoolFeedback InUseFeedback { get; private set; }

		/// <summary>
		/// Feedback that changes with the count of users
		/// </summary>
		public IntFeedback InUseCountFeedback { get; private set; }

		public InUseTracking()
		{
			InUseFeedback = new BoolFeedback(() => _Users.Count > 0);
			InUseCountFeedback = new IntFeedback(() => _Users.Count);
		}

		/// <summary>
		/// Add a "user" object to this tracker. A user can be added to this tracker 
		/// multiple times, provided that the label is different
		/// </summary>
		/// <param name="label">A label to identify the instance of the user. Treated like a "role", etc.</param>
		public void AddUser(object objectToAdd, string label)
		{
			// check if an exact object/label pair exists and ignore if so.  No double-registers.
			var check = _Users.FirstOrDefault(u => u.Label == label && u.User == objectToAdd);
			if (check != null) return;

			var prevCount = _Users.Count;
			_Users.Add(new InUseTrackingObject(objectToAdd, label));
			// if this is the first add, fire an update
			if (prevCount == 0 && _Users.Count > 0)
				InUseFeedback.FireUpdate();
			InUseCountFeedback.FireUpdate();
		}

		/// <summary>
		/// Remove a user object from this tracking
		/// </summary>
		public void RemoveUser(object objectToRemove, string label)
		{
			// Find the user object if exists and remove it
			var toRemove = _Users.FirstOrDefault(u => u.Label == label && u.User == objectToRemove);
			if (toRemove != null)
			{
				_Users.Remove(toRemove);
				if (_Users.Count == 0)
					InUseFeedback.FireUpdate();
				InUseCountFeedback.FireUpdate();
			}
		}
	}

	/// <summary>
	/// Wrapper for label/object pair representing in-use status.  Allows the same object to
	/// register for in-use with different roles.
	/// </summary>
	public class InUseTrackingObject
	{
		public string Label { get; private set; }
		public object User { get; private set; }

		public InUseTrackingObject(object user, string label)
		{
			User = user;
			Label = label;
		}
	}

	//public class InUseEventArgs
	//{
	//    public int EventType { get; private set; }
	//    public InUseTracking Tracker { get; private set; }

	//    public InUseEventArgs(InUseTracking tracker, int eventType)
	//    {
	//        Tracker = tracker;
	//        EventType = eventType;
	//    }
	//}