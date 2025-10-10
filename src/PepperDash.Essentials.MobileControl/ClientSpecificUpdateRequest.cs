using System;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Represents a ClientSpecificUpdateRequest
  /// </summary>
  public class ClientSpecificUpdateRequest
  {
    public ClientSpecificUpdateRequest(Action<string> action)
    {
      ResponseMethod = action;
    }

    /// <summary>
    /// Gets or sets the ResponseMethod
    /// </summary>
    public Action<string> ResponseMethod { get; private set; }
  }
}
