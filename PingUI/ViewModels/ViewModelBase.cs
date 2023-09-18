using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace PingUI.ViewModels;

/// <summary>
/// A base class for all view models that includes change notifications.
/// </summary>
public abstract class ViewModelBase : ReactiveValidationObject
{
}
