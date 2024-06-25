using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using PingUI.Models;
using ReactiveUI;

namespace PingUI.ViewModels;

public class TargetTagViewModel : ViewModelBase, IComparable<TargetTagViewModel>
{
	private readonly ObservableAsPropertyHelper<Brush> _Background;

	private readonly ObservableAsPropertyHelper<bool> _CanDelete;

	private bool _AllowDelete;

	public TargetTagViewModel(TargetTag tag)
	{
		Text = tag.Text;
		IsAutomatic = tag.Automatic;
		_Background = tag.Background.ToProperty(this, vm => vm.Background);
		var canDelete = this.WhenAnyValue(vm => vm.AllowDelete)
			.Select(allowDelete => allowDelete && !IsAutomatic);
		_CanDelete = canDelete.ToProperty(this, vm => vm.CanDelete);
		DeleteSelfInteraction = new Interaction<TargetTagViewModel, Unit>();
		DeleteSelfCommand = ReactiveCommand.Create(() => { DeleteSelfInteraction.Handle(this).Subscribe(); }, canDelete);
	}

	public bool AllowDelete
	{
		get => _AllowDelete;
		set => this.RaiseAndSetIfChanged(ref _AllowDelete, value);
	}

	public Brush Background => _Background.Value;

	public bool CanDelete => _CanDelete.Value;

	public bool IsAutomatic
	{
		get;
	}

	public string Text
	{
		get;
	}

	public ReactiveCommand<Unit, Unit> DeleteSelfCommand
	{
		get;
	}

	public Interaction<TargetTagViewModel, Unit> DeleteSelfInteraction
	{
		get;
	}

	public int CompareTo(TargetTagViewModel? other)
	{
		if (other is null)
		{
			return -1;
		}
		var automatic = -IsAutomatic.CompareTo(other.IsAutomatic);
		if (automatic != 0)
		{
			return automatic;
		}
		return Text.CompareTo(other.Text);
	}
}
