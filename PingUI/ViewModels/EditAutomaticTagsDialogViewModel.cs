using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DialogHostAvalonia;
using DynamicData.Binding;
using PingUI.Extensions;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.ViewModels;

public class EditAutomaticTagsDialogViewModel : ViewModelBase
{
	public EditAutomaticTagsDialogViewModel()
	{
		AutomaticTagEntries = [];
		AutomaticTagEntries.AddRange(Locator.Current.GetRequiredService<IConfiguration>().AutomaticTagEntries.Select(entry =>
		{
			var viewModel = new AutomaticTagEntryViewModel();
			viewModel.SetAutomaticTagEntry(entry);
			return viewModel;
		}));
		AddEntryCommand = ReactiveCommand.Create(() => AutomaticTagEntries.Add(new AutomaticTagEntryViewModel()));
		DeleteEntryCommand = ReactiveCommand.Create<AutomaticTagEntryViewModel>(entry => AutomaticTagEntries.Remove(entry));
		CancelDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
		AcceptDialogCommand = ReactiveCommand.Create(
			() =>
			{
				Locator.Current.GetRequiredService<IConfiguration>().AutomaticTagEntries = AutomaticTagEntries.Select(entry => entry.GetAutomaticTagEntry()).ToImmutableArray();
				DialogHost.GetDialogSession(null)?.Close();
			},
			AutomaticTagEntries.WhenCollectionItemChanged<ObservableCollectionExtended<AutomaticTagEntryViewModel>, AutomaticTagEntryViewModel, bool>(entries => !entries.Any(entry => entry.HasErrors)).ObserveOn(RxApp.MainThreadScheduler));
	}

	public ObservableCollectionExtended<AutomaticTagEntryViewModel> AutomaticTagEntries
	{
		get;
	}

	public ReactiveCommand<Unit, Unit> AddEntryCommand
	{
		get;
	}

	public ReactiveCommand<AutomaticTagEntryViewModel, Unit> DeleteEntryCommand
	{
		get;
	}

	/// <summary>
	/// A command to cancel the changes.
	/// </summary>
	public ReactiveCommand<Unit, Unit> CancelDialogCommand
	{
		get;
	}

	/// <summary>
	/// A command to accept the changes.
	/// </summary>
	public ReactiveCommand<Unit, Unit> AcceptDialogCommand
	{
		get;
	}
}
