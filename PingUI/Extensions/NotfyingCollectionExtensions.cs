using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using DynamicData.Binding;

namespace PingUI.Extensions;

public static class NotifyingCollectionExtensions
{
	public static IObservable<TCollection> WhenCollectionChanged<TCollection, TItem>(this TCollection @this)
	where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
	where TItem : notnull
	{
		return @this.ToObservableChangeSet<TCollection, TItem>().Select(_ => @this);
	}

	public static IObservable<TMap> WhenCollectionChanged<TCollection, TItem, TMap>(this TCollection @this, Func<TCollection, TMap> map)
	where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
	where TItem : notnull
	{
		return @this.ToObservableChangeSet<TCollection, TItem>().Select(_ => map(@this));
	}
}
