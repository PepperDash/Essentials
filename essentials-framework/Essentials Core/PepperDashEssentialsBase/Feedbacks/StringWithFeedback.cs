using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class StringWithFeedback : StringFeedback
	{
		private string _Value;
		public string Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				this.FireUpdate();
			}
		}
		public StringWithFeedback()
			: base(() => Value)
		{

		}
	}
}