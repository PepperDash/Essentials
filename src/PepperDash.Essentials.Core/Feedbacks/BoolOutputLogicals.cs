using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{

	/// <summary>
	/// Abstract base class for BoolOutputLogicals
	/// </summary>
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

		/// <summary>
		/// Gets or sets the ComputedValue
		/// </summary>
		protected bool ComputedValue;

		/// <summary>
		/// Constructor
		/// </summary>
	    protected BoolFeedbackLogic()
		{
			Output = new BoolFeedback(() => ComputedValue);
		}	

		/// <summary>
		/// AddOutputIn method
		/// </summary>
		/// <param name="output">feedback to add</param>
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
		/// <param name="outputs">feedbacks to add</param>
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
		/// <param name="output">feedback to remove</param>
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
		/// <param name="outputs">feedbacks to remove</param>
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

		/// <summary>
		/// AnyInput_OutputChange event handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void AnyInput_OutputChange(object sender, EventArgs e)
		{
			Evaluate();
		}

		/// <summary>
		/// Evaluate method
		/// </summary>
		protected abstract void Evaluate();
	}

 /// <summary>
 /// Represents a BoolFeedbackAnd
 /// </summary>
	public class BoolFeedbackAnd : BoolFeedbackLogic
	{
		/// <summary>
		/// Evaluate method
		/// </summary>
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
		/// <summary>
		/// Evaluate method
		/// </summary>
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="predicate"></param>
		public BoolFeedbackLinq(Func<IEnumerable<BoolFeedback>, bool> predicate)
			: base()
		{
			_predicate = predicate;
		}

		/// <summary>
		/// Evaluate method
		/// </summary>
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