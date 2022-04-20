using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class BoolWithFeedback : BoolFeedback
	{
		private bool _Value;
		public bool Value
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
		public BoolWithFeedback()
			: base(() => Value)
		{

		}
	}

}