using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class EditTargetView : ReactiveUserControl<EditTargetViewModel>
{
	public EditTargetView()
	{
		InitializeComponent();
	}
}
