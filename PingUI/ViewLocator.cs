using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PingUI.ViewModels;
using PingUI.Views;

namespace PingUI;

/// <summary>
/// Provides concrete views for given data contexts.
/// </summary>
public class ViewLocator : IDataTemplate
{
	/// <inheritdoc />
	public Control Build(object? data)
	{
		return data switch
		{
			null => new TextBlock() { Text = "<null>" },
			DeleteTargetViewModel => new DeleteTargetView(),
			EditTargetViewModel => new EditTargetView(),
			TargetViewModel => new TargetView(),
			PingResultViewModel => new PingResultView(),
			AskToEnableUpdateCheckViewModel => new AskToEnableUpdateCheckView(),
			UpdateNotificationViewModel => new UpdateNotificationView(),
			SettingsViewModel => new SettingsView(),
			DeleteAllTargetsViewModel => new DeleteAllTargetsView(),
			TargetTagViewModel => new TargetTagView(),
			EditAutomaticTagsDialogViewModel => new EditAutomaticTagsDialogView(),
			AutomaticTagEntryViewModel => new AutomaticTagEntryView(),
			FilterBuilderViewModel => new FilterBuilderView(),
			FilterTextViewModel => new FilterTextView(),
			FilterCombinedViewModel => new FilterCombinedView(),
			FilterNotViewModel => new FilterNotView(),
			_ => new TextBlock() { Text = "Not Found: " + data.GetType().FullName },
		};
	}

	/// <inheritdoc />
	public bool Match(object? data)
	{
		return data is ViewModelBase;
	}
}
