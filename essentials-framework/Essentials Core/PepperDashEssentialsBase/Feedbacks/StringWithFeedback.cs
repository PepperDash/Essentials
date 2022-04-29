using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class StringWithFeedback 
	{
		private string _Value;
		public StringFeedback Feedback;
		public string Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				Feedback.FireUpdate();
			}
		}
		public StringWithFeedback()
			
		{
			Feedback = new StringFeedback(() => Value);
		}
	}
}