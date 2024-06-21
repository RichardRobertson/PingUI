using System.Collections.ObjectModel;
using System.ComponentModel;
using PingUI.Interop;
using PingUI.Models;

namespace PingUI.ServiceModels;

/// <summary>
/// Represents the base configuration of the application.
/// </summary>
public interface IConfiguration : INotifyPropertyChanged, INotifyPropertyChanging
{
	/// <summary>
	/// The <see cref="Target" /> values that need to be remembered.
	/// </summary>
	/// <value>An <see cref="ObservableCollection{T}" /> of <see cref="Target" /> values.</value>
	ObservableCollection<Target> Targets
	{
		get;
	}

	/// <summary>
	/// Saves this configuration to the backing store.
	/// </summary>
	void Save();

	/// <summary>
	/// Indicates whether the main window should display on top of other windows at all times.
	/// </summary>
	bool Topmost
	{
		get;
		set;
	}

	/// <summary>
	/// Indicates whether the app should check for updates on startup.
	/// </summary>
	/// <value><see langword="null" /> for unknown, <see langword="true" /> to check for updates, <see langword="false" /> not to check for updates.</value>
	bool? CheckOnlineForUpdates
	{
		get;
		set;
	}

	/// <summary>
	/// Indicates whether the app should remember and restore the window location on startup.
	/// </summary>
	/// <value>A <see cref="WindowPlacement" /> value to save and restore; otherwise <see langword="null" /> to not save and restore.</value>
	WindowPlacement.WindowPlacementRecord? WindowBounds
	{
		get;
		set;
	}
}
