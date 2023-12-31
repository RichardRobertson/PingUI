using System.Collections.ObjectModel;
using PingUI.Models;

namespace PingUI.ServiceModels;

/// <summary>
/// Represents the base configuration of the application.
/// </summary>
public interface IConfiguration
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
}
