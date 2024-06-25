using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media;
using PingUI.Extensions;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.Models;

public record TargetTag(string Text, bool Automatic = false) : IObservable<Brush>
{
	private static readonly IObservable<ImmutableSortedSet<string>> AllTagsObservable;

	private static readonly Color DefaultColor = new(0xFF, 0x33, 0x33, 0x33);

	private static readonly double GoldenAngleDegrees = double.Pi * (3.0 - double.Sqrt(5.0)) * (180.0 / double.Pi);

	private static readonly ObservableAsPropertyHelper<ImmutableSortedSet<string>> _LatestAllTags;

	static TargetTag()
	{
		var configuration = Locator.Current.GetRequiredService<IConfiguration>();
		var automaticTagsObservable = configuration.WhenAnyValue(conf => conf.AutomaticTagEntries).Select(tags => tags.Select(tag => tag.Tag).ToImmutableSortedSet());
		var targetCollectionTagsObservable = configuration.Targets.WhenCollectionChanged<ObservableCollection<Target>, Target, ImmutableSortedSet<string>>(collection => collection.SelectMany(target => target.Tags).ToImmutableSortedSet());
		AllTagsObservable = automaticTagsObservable.CombineLatest(targetCollectionTagsObservable).Select(((ImmutableSortedSet<string> automatic, ImmutableSortedSet<string> targets) pair) => pair.automatic.Concat(pair.targets).ToImmutableSortedSet());
		_LatestAllTags = new ObservableAsPropertyHelper<ImmutableSortedSet<string>>(AllTagsObservable, _ => { }, ImmutableSortedSet<string>.Empty, false, RxApp.MainThreadScheduler);
	}

	public static ImmutableSortedSet<string> LatestAllTags => _LatestAllTags.Value;

	public IObservable<Brush> Background => this;

	private static Color GetColor(ImmutableSortedSet<string> tags, string tag)
	{
		return tags.Contains(tag)
			? new HsvColor(1.0, GoldenAngleDegrees * tags.IndexOf(tag) % 360.0, 0.45, 0.65).ToRgb()
			: DefaultColor;
	}

	public IDisposable Subscribe(IObserver<Brush> observer)
	{
		return AllTagsObservable
			.Select(allTags => new SolidColorBrush(GetColor(allTags, Text)))
			.Subscribe(observer);
	}
}
