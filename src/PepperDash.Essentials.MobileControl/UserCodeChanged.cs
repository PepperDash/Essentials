using System;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Represents a UserCodeChanged
  /// </summary>
  public class UserCodeChanged
  {
    public Action<string, string> UpdateUserCode { get; private set; }

    public UserCodeChanged(Action<string, string> updateMethod)
    {
      UpdateUserCode = updateMethod;
    }
  }
}
