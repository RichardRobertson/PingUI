using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PingUI.ViewModels;
using PingUI.Views;

namespace PingUI;

public class ViewLocator : IDataTemplate
{
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
			_ => new TextBlock() { Text = "Not Found: " + data.GetType().FullName },
		};
	}

	public bool Match(object? data)
	{
		return data is ViewModelBase;
	}
}
