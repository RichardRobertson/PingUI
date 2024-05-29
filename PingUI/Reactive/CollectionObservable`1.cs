using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PingUI.Reactive;

public class CollectionObservable<TCollection, TMap> : IObservable<TMap> where TCollection : INotifyCollectionChanged
{
	private readonly Func<TCollection, TMap> _Map;

	private readonly List<Subscriber> _Subscribers;

	private readonly object _SubscriberLock;

	public CollectionObservable(TCollection collection, Func<TCollection, TMap> map)
	{
		ArgumentNullException.ThrowIfNull(collection);
		ArgumentNullException.ThrowIfNull(map);
		Collection = collection;
		collection.CollectionChanged += OnCollectionChanged;
		_Map = map;
		_Subscribers = [];
		_SubscriberLock = new object();
	}

	public TCollection Collection
	{
		get;
	}

	public IDisposable Subscribe(IObserver<TMap> observer)
	{
		var subscriber = new Subscriber(this, observer);
		observer.OnNext(_Map(Collection));
		lock (_SubscriberLock)
		{
			_Subscribers.Add(subscriber);
		}
		return subscriber;
	}

	private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		var mapped = _Map(Collection);
		lock (_SubscriberLock)
		{
			foreach (var subscriber in _Subscribers)
			{
				subscriber.Observer.OnNext(mapped);
			}
		}
	}

	private record Subscriber(CollectionObservable<TCollection, TMap> Parent, IObserver<TMap> Observer) : IDisposable
	{
		public void Dispose()
		{
			lock (Parent._SubscriberLock)
			{
				Parent._Subscribers.Remove(this);
				Observer.OnCompleted();
			}
		}
	}
}
