using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using PingUI.I18N;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class DialogErrorReporterView : ReactiveWindow<DialogErrorReporterViewModel>
{
	public DialogErrorReporterView()
	{
		InitializeComponent();
	}

	private async void CopyButton_Click(object sender, RoutedEventArgs e)
	{
		var button = (Button)sender;
		button.IsEnabled = false;
		if (Clipboard is not null && ViewModel is not null)
		{
			await Clipboard.SetTextAsync($"{ViewModel.Context}\n\n{ViewModel.Exception}").ConfigureAwait(true);
			button.IsEnabled = true;
		}
		else
		{
			button.Content = Strings.DialogErrorReporterView_Copy_Fail;
		}
	}

	private void DismissButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
