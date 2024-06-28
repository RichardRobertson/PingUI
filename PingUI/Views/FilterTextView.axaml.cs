using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class FilterTextView : ReactiveUserControl<FilterBuilderViewModel>
{
	public FilterTextView()
	{
		InitializeComponent();
	}
}
