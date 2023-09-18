using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class PingResultView : ReactiveUserControl<PingResultViewModel>
{
	public PingResultView()
	{
		InitializeComponent();
	}
}
