using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface IHasFeedback : IKeyed
    {
        /// <summary>
        /// This method shall return a list of all Output objects on a device,
        /// including all "aggregate" devices.
        /// </summary>
        FeedbackCollection<Feedback> Feedbacks { get; }

    }
}