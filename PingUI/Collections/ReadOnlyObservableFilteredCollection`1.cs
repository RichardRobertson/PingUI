using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;

namespace PingUI.Collections;

/// <summary>
/// Represents a readonly collection wrapper that filters the items of the source collection and provides update notifications.
/// </summary>
/// <typeparam name="TSourceCollection">The type of the source collection.</typeparam>
/// <typeparam name="TValue">The type of the items in the source collection.</typeparam>
public class ReadOnlyObservableFilteredCollection<TSourceCollection, TValue> :
	IReadOnlyList<TValue>,
	IList,
	INotifyPropertyChanged,
	INotifyCollectionChanged,
	IDisposable
	where TSourceCollection : INotifyCollectionChanged, IReadOnlyList<TValue>
	where TValue : notnull
{
	/// <summary>
	/// Reference to the source collection.
	/// </summary>
	private readonly TSourceCollection _SourceCollection;

	/// <summary>
	/// Backing store for filtered collection.
	/// </summary>
	private readonly ObservableCollectionExtended<TValue> _ValueCollection;

	/// <summary>
	/// Storage for <see cref="Dispose(bool)" />.
	/// </summary>
	private readonly CompositeDisposable _Disposables;

	/// <summary>
	/// Signal for <see cref="Dispose(bool)" />.
	/// </summary>
	private bool disposedValue;

	private Func<TValue, bool> _Filter;

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyObservableFilteredCollection{TCollection, TValue}" />.
	/// </summary>
	/// <param name="sourceCollection">The source collection to filter items from.</param>
	/// <param name="filterObservable">An observable that produces filtering functions.</param>
	public ReadOnlyObservableFilteredCollection(TSourceCollection sourceCollection, IObservable<Func<TValue, bool>> filterObservable)
	{
		ArgumentNullException.ThrowIfNull(sourceCollection);
		ArgumentNullException.ThrowIfNull(filterObservable);
		_SourceCollection = sourceCollection;
		_ValueCollection = [];
		_Filter = _ => true;
		_Disposables = [filterObservable.Subscribe(OnFilterUpdated), _SourceCollection.ToObservableChangeSet<TSourceCollection, TValue>().Subscribe(OnSourceCollectionChanged)];
	}

	/// <summary>
	/// Handle changes to the filter.
	/// </summary>
	/// <param name="filter">The new filter function</param>
	private void OnFilterUpdated(Func<TValue, bool> filter)
	{
		_Filter = filter;
		OnSourceCollectionChanged(
			new ChangeSet<TValue>()
			{
				new Change<TValue>(ListChangeReason.Refresh, default(TValue)!, 0)
			});
	}

	/// <summary>
	/// Handle changes to the source collection.
	/// </summary>
	/// <param name="changes">The set of changes published.</param>
	private void OnSourceCollectionChanged(IChangeSet<TValue> changes)
	{
		foreach (var change in changes)
		{
			switch (change.Reason)
			{
				case ListChangeReason.Add:
					if (_Filter(change.Item.Current))
					{
						if (change.Item.CurrentIndex == -1)
						{
							_ValueCollection.Add(change.Item.Current);
						}
						else
						{
							_ValueCollection.Insert(change.Item.CurrentIndex, change.Item.Current);
						}
					}
					break;
				case ListChangeReason.AddRange:
					if (change.Range.Index == -1)
					{
						_ValueCollection.AddRange(change.Range.Where(_Filter));
					}
					else
					{
						_ValueCollection.InsertRange(change.Range.Where(_Filter), change.Range.Index);
					}
					break;
				case ListChangeReason.Replace:
					_ValueCollection[change.Item.CurrentIndex] = change.Item.Current;
					break;
				case ListChangeReason.Remove:
					if (change.Item.CurrentIndex == -1)
					{
						_ValueCollection.Remove(change.Item.Current);
					}
					else
					{
						_ValueCollection.RemoveAt(change.Item.CurrentIndex);
					}
					break;
				case ListChangeReason.RemoveRange:
					if (change.Range.Index == -1)
					{
						_ValueCollection.Remove(_ValueCollection.Where(change.Range.Contains));
					}
					else
					{
						_ValueCollection.RemoveRange(change.Range.Index, change.Range.Count);
					}
					break;
				case ListChangeReason.Refresh:
					using (_ValueCollection.SuspendNotifications())
					{
						_ValueCollection.Clear();
						_ValueCollection.AddRange(_SourceCollection.Where(_Filter));
					}
					break;
				case ListChangeReason.Moved:
					_ValueCollection.Move(change.Item.PreviousIndex, change.Item.CurrentIndex);
					break;
				case ListChangeReason.Clear:
					_ValueCollection.Clear();
					break;
			}
		}
	}

	/// <inheritdoc />
	public TValue this[int index] => _ValueCollection[index];

	/// <inheritdoc />
	public int Count => _ValueCollection.Count;

	/// <inheritdoc />
	bool IList.IsFixedSize => false;

	/// <inheritdoc />
	bool IList.IsReadOnly => true;

	/// <inheritdoc />
	bool ICollection.IsSynchronized => false;

	/// <inheritdoc />
	object ICollection.SyncRoot
	{
		get;
	} = new();

	/// <inheritdoc />
	object? IList.this[int index]
	{
		get => this[index];
		set => throw new InvalidOperationException();
	}

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler? CollectionChanged
	{
		add
		{
			_ValueCollection.CollectionChanged += value;
		}
		remove
		{
			_ValueCollection.CollectionChanged -= value;
		}
	}

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged
	{
		add
		{
			((INotifyPropertyChanged)_ValueCollection).PropertyChanged += value;
		}
		remove
		{
			((INotifyPropertyChanged)_ValueCollection).PropertyChanged -= value;
		}
	}

	/// <inheritdoc />
	public IEnumerator<TValue> GetEnumerator()
	{
		return _ValueCollection.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Implementation for <see cref="Dispose()" />.
	/// </summary>
	/// <param name="disposing"><see langword="true" /> if called from <see cref="Dispose()" />; <see langword="false" /> if called from finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_Disposables.Dispose();
				_ValueCollection.Clear();
				(_SourceCollection as IDisposable)?.Dispose();
			}
			disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	int IList.Add(object? value)
	{
		throw new InvalidOperationException();
	}

	/// <inheritdoc />
	void IList.Clear()
	{
		throw new InvalidOperationException();
	}

	/// <inheritdoc />
	bool IList.Contains(object? value)
	{
		return value is TValue tValue && _ValueCollection.Contains(tValue);
	}

	/// <inheritdoc />
	int IList.IndexOf(object? value)
	{
		return value is TValue tValue ? _ValueCollection.IndexOf(tValue) : -1;
	}

	/// <inheritdoc />
	void IList.Insert(int index, object? value)
	{
		throw new InvalidOperationException();
	}

	/// <inheritdoc />
	void IList.Remove(object? value)
	{
		throw new InvalidOperationException();
	}

	/// <inheritdoc />
	void IList.RemoveAt(int index)
	{
		throw new InvalidOperationException();
	}

	/// <inheritdoc />
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_ValueCollection).CopyTo(array, index);
	}
}
