using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia.ReactiveUI;
using DialogHostAvalonia;
using PingUI.Models;
using PingUI.ViewModels;
using ReactiveUI;

namespace PingUI.Views;

public partial class TargetView : ReactiveUserControl<TargetViewModel>
{
	public TargetView()
	{
		InitializeComponent();
		this.WhenActivated(disposables =>
		{
			this.BindInteraction(
				ViewModel,
				vm => vm.PromptForDeleteInteraction,
				context => DialogHost.Show(new DeleteTargetViewModel(context.Input))
					.ContinueWith(result => context.SetOutput(result.Result as bool?)))
				.DisposeWith(disposables);
			this.BindInteraction(
				ViewModel,
				vm => vm.PromptForEditInteraction,
				context => DialogHost.Show(new EditTargetViewModel(context.Input))
					.ContinueWith(result => context.SetOutput(result.Result as Target)))
				.DisposeWith(disposables);
		});
	}
}
