using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Binding;
using ReactiveUI;

namespace PingUI.Extensions;

public static class NotifyingCollectionExtensions
{
	public static IObservable<TMap> WhenCollectionChanged<TCollection, TItem, TMap>(this TCollection @this, Func<TCollection, TMap> map)
	where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
	where TItem : notnull
	{
		return @this.ToObservableChangeSet<TCollection, TItem>().Select(_ => map(@this));
	}

	public static IObservable<TMap> WhenCollectionItemChanged<TCollection, TItem, TMap>(this TCollection @this, Func<TCollection, TMap> map)
	where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
	where TItem : notnull, IReactiveObject
	{
		return Observable.Create<TMap>(observer =>
		{
			var subject = new Subject<TMap>();
			var changeSetObservable = @this.ToObservableChangeSet<TCollection, TItem>();
			return new CompositeDisposable()
			{
				changeSetObservable.ActOnEveryObject(
					item => item.PropertyChanged += OnItemPropertyChanged,
					item => item.PropertyChanged -= OnItemPropertyChanged),
				changeSetObservable.Subscribe(_ => subject.OnNext(map(@this))),
				subject.Subscribe(observer),
			};

			void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				subject.OnNext(map(@this));
			}
		});
	}
}
