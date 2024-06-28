using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class FilterCombinedView : ReactiveUserControl<FilterBuilderViewModel>
{
	public FilterCombinedView()
	{
		InitializeComponent();
	}
}
