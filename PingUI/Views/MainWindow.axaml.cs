using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void CloseWindowButton_Click(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
