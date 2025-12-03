using System;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Send an update request for a specific client
  /// </summary>
  [Obsolete]
  public class ClientSpecificUpdateRequest
  {
    /// <summary>
    /// Initialize an instance of the <see cref="ClientSpecificUpdateRequest"/> class.
    /// </summary>
    /// <param name="action"></param>
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
