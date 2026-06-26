using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Obsolete. Use <see cref="IMobileControlMessenger"/> directly.
  /// Subscriptions are now always enabled in MessengerBase.
  /// </summary>
  [Obsolete("Use IMobileControlMessenger directly. Subscriptions are always enabled.")]
  public interface IMobileControlMessengerWithSubscriptions : IMobileControlMessenger
  {
  }
}