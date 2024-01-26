using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class EditTargetView : ReactiveUserControl<EditTargetViewModel>
{
	public EditTargetView()
	{
		InitializeComponent();
		AddressText.AttachedToVisualTree += (sender, e) =>
		{
			if (sender is TextBox textBox)
			{
				textBox.Focus();
			}
		};
	}
}
