using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core
{


	public abstract class BoolFeedbackLogic
	{
  /// <summary>
  /// Gets or sets the Output
  /// </summary>
		public BoolFeedback Output { get; private set; }

		/// <summary>
		/// List of all connected outputs
		/// </summary>
		protected List<BoolFeedback> OutputsIn = new List<BoolFeedback>();

		protected bool ComputedValue;

	    protected BoolFeedbackLogic()
		{
			Output = new BoolFeedback(() => ComputedValue);
		}	

		public void AddOutputIn(BoolFeedback output)
		{
			// Don't double up outputs
			if(OutputsIn.Contains(output)) return;

			OutputsIn.Add(output);
			output.OutputChange += AnyInput_OutputChange;
			Evaluate();
		}

  /// <summary>
  /// AddOutputsIn method
  /// </summary>
		public void AddOutputsIn(List<BoolFeedback> outputs)
		{
		    foreach (var o in outputs.Where(o => !OutputsIn.Contains(o)))
		    {
		        OutputsIn.Add(o);
		        o.OutputChange += AnyInput_OutputChange;
		    }
		    Evaluate();
		}

     /// <summary>
     /// RemoveOutputIn method
     /// </summary>
	    public void RemoveOutputIn(BoolFeedback output)
		{
			// Don't double up outputs
			if (!OutputsIn.Contains(output)) return;

			OutputsIn.Remove(output);
			output.OutputChange -= AnyInput_OutputChange;
			Evaluate();
		}

  /// <summary>
  /// RemoveOutputsIn method
  /// </summary>
		public void RemoveOutputsIn(List<BoolFeedback> outputs)
		{
			foreach (var o in outputs)
			{
				OutputsIn.Remove(o);
				o.OutputChange -= AnyInput_OutputChange;
			}
			Evaluate();
		}

     /// <summary>
     /// ClearOutputs method
     /// </summary>
	    public void ClearOutputs()
	    {
	        OutputsIn.Clear();
            Evaluate();
	    }

		void AnyInput_OutputChange(object sender, EventArgs e)
		{
			Evaluate();
		}

		protected abstract void Evaluate();
	}

 /// <summary>
 /// Represents a BoolFeedbackAnd
 /// </summary>
	public class BoolFeedbackAnd : BoolFeedbackLogic
	{
		protected override void Evaluate()
		{
			var prevValue = ComputedValue;
			var newValue = OutputsIn.All(o => o.BoolValue);
		    if (newValue == prevValue)
		    {
		        return;
		    }
		    ComputedValue = newValue;
		    Output.FireUpdate();
		}
	}

 /// <summary>
 /// Represents a BoolFeedbackOr
 /// </summary>
	public class BoolFeedbackOr : BoolFeedbackLogic
	{
		protected override void Evaluate()
		{
			var prevValue = ComputedValue;
			var newValue = OutputsIn.Any(o => o.BoolValue);
		    if (newValue == prevValue)
		    {
		        return;
		    }
		    ComputedValue = newValue;
		    Output.FireUpdate();
		}
	}

 /// <summary>
 /// Represents a BoolFeedbackLinq
 /// </summary>
	public class BoolFeedbackLinq : BoolFeedbackLogic
	{
	    readonly Func<IEnumerable<BoolFeedback>, bool> _predicate;

		public BoolFeedbackLinq(Func<IEnumerable<BoolFeedback>, bool> predicate)
			: base()
		{
			_predicate = predicate;
		}

		protected override void Evaluate()
		{
			var prevValue = ComputedValue;
			var newValue = _predicate(OutputsIn);
		    if (newValue == prevValue)
		    {
		        return;
		    }
		    ComputedValue = newValue;
		    Output.FireUpdate();
		} 
	}
}