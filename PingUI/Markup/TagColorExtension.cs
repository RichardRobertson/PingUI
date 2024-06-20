using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PingUI.Extensions;
using PingUI.Models;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.Markup;

/// <summary>
/// Provides dynamic color values for tag borders based on <see cref="StyledElement.DataContext" />.
/// </summary>
public class TagColorExtension : MarkupExtension
{
	private static readonly Lazy<IObservable<ImmutableSortedSet<string>>> TargetCollectionTagsObservable;

	static TagColorExtension()
	{
		Latest = ImmutableSortedSet<string>.Empty;
		TargetCollectionTagsObservable = new Lazy<IObservable<ImmutableSortedSet<string>>>(
			static () =>
			{
				var observable = Locator.Current.GetRequiredService<IConfiguration>().Targets.WhenCollectionChanged<ObservableCollection<Target>, Target, ImmutableSortedSet<string>>(collection => collection.SelectMany(target => target.Tags).ToImmutableSortedSet());
				observable.SubscribeOn(RxApp.MainThreadScheduler)
					.Subscribe(latest => Latest = latest);
				return observable;
			});
	}

	/// <summary>
	/// Gets the latest values from the target tag collections.
	/// </summary>
	/// <value>An <see cref="ImmutableSortedSet{T}" /> of <see cref="string" /> that represents all unique tags in use on targets.</value>
	public static ImmutableSortedSet<string> Latest
	{
		get;
		private set;
	}

	/// <inheritdoc />
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget
			&& provideValueTarget.TargetObject is StyledElement element)
		{
			return new TagBrushProvider(element).WhenAnyValue(provider => provider.Brush).ToBinding();
		}
		return Brushes.Transparent;
	}

	private class TagBrushProvider : ReactiveObject
	{
		private static readonly Color DefaultColor = new(0xFF, 0x33, 0x33, 0x33);

		private static readonly double GoldenAngleDegrees = double.Pi * (3.0 - double.Sqrt(5.0)) * (180.0 / double.Pi);

		private readonly ObservableAsPropertyHelper<Brush> _Brush;

		public TagBrushProvider(StyledElement element)
		{
			_Brush = element.WhenAnyValue(border => border.DataContext)
				.CombineLatest(TargetCollectionTagsObservable.Value)
				.Select(((object? dataContext, ImmutableSortedSet<string> tags) value) => new SolidColorBrush(GetColor(value.tags, value.dataContext as string)))
				.ToProperty(this, tbp => tbp.Brush, scheduler: RxApp.MainThreadScheduler);
		}

		public Brush Brush
		{
			get => _Brush.Value;
		}

		public static Color GetColor(ImmutableSortedSet<string> tags, string? tag)
		{
			ArgumentNullException.ThrowIfNull(tags);
			return tag is not null && tags.Contains(tag)
				? new HsvColor(1.0, GoldenAngleDegrees * tags.IndexOf(tag) % 360.0, 0.45, 0.65).ToRgb()
				: DefaultColor;
		}
	}
}
