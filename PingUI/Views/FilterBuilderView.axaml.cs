using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class FilterBuilderView : ReactiveUserControl<FilterBuilderViewModel>
{
	public FilterBuilderView()
	{
		InitializeComponent();
	}
}
