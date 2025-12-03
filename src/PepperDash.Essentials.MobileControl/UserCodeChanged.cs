using System;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Defines the action to take when the User code changes
  /// </summary>
  public class UserCodeChanged
  {
    /// <summary>
    /// Action to take when the User Code changes
    /// </summary>
    public Action<string, string> UpdateUserCode { get; private set; }

    /// <summary>
    /// create an instance of the <see cref="UserCodeChanged"/> class
    /// </summary>
    /// <param name="updateMethod">action to take when the User Code changes</param>
    public UserCodeChanged(Action<string, string> updateMethod)
    {
      UpdateUserCode = updateMethod;
    }
  }
}
