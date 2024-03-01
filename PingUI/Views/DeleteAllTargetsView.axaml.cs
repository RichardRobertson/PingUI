using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class DeleteAllTargetsView : ReactiveUserControl<DeleteTargetViewModel>
{
	public DeleteAllTargetsView()
	{
		InitializeComponent();
	}
}
