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
/// Represents a readonly collection wrapper that wraps the individual items of the source collection and provides update notifications.
/// </summary>
/// <typeparam name="TKey">The type of the items in the source collection.</typeparam>
/// <typeparam name="TSourceCollection">The type of the source collection.</typeparam>
/// <typeparam name="TValue">The type of the item wrapper.</typeparam>
public class ReadOnlyObservableMappedCollection<TKey, TSourceCollection, TValue> :
	IReadOnlyList<TValue>,
	IList,
	INotifyPropertyChanged,
	INotifyCollectionChanged,
	IDisposable
	where TKey : notnull
	where TSourceCollection : INotifyCollectionChanged, IReadOnlyList<TKey>
{
	/// <summary>
	/// Reference to the source collection.
	/// </summary>
	private readonly TSourceCollection _SourceCollection;

	/// <summary>
	/// Backing store for the wrapper collection.
	/// </summary>
	private readonly ObservableCollectionExtended<TValue> _ValueCollection;

	/// <summary>
	/// A function that creates a new wrapper item from a source item.
	/// </summary>
	private readonly Func<TKey, TValue> _Create;

	/// <summary>
	/// A function that modifies an existing wrapper item with a new source item.
	/// </summary>
	private readonly Action<TKey, TValue> _Modify;

	/// <summary>
	/// A function that gets the source item from a wrapper item.
	/// </summary>
	private readonly Func<TValue, TKey> _GetKey;

	/// <summary>
	/// Equality comparer for source items to verify changes.
	/// </summary>
	private readonly IEqualityComparer<TKey> _KeyComparer;

	/// <summary>
	/// Storage for <see cref="Dispose(bool)" />.
	/// </summary>
	private readonly CompositeDisposable _Disposables;

	/// <summary>
	/// Signal for <see cref="Dispose(bool)" />.
	/// </summary>
	private bool disposedValue;

	/// <summary>
	/// Initializes a new <see cref="ReadOnlyObservableMappedCollection{TKey, TSourceCollection, TValue}" />.
	/// </summary>
	/// <param name="sourceCollection">The source collection to wrap items from.</param>
	/// <param name="create">A function that creates a new wrapper item from a source item.</param>
	/// <param name="modify">A function that modifies an existing wrapper item with a new source item.</param>
	/// <param name="getKey">A function that gets the source item from a wrapper item.</param>
	/// <param name="keyComparer">Equality comparer for source items to verify changes.</param>
	public ReadOnlyObservableMappedCollection(TSourceCollection sourceCollection, Func<TKey, TValue> create, Action<TKey, TValue> modify, Func<TValue, TKey> getKey, IEqualityComparer<TKey>? keyComparer = null)
	{
		ArgumentNullException.ThrowIfNull(sourceCollection);
		ArgumentNullException.ThrowIfNull(create);
		ArgumentNullException.ThrowIfNull(modify);
		ArgumentNullException.ThrowIfNull(getKey);
		_SourceCollection = sourceCollection;
		_Create = create;
		_Modify = modify;
		_GetKey = getKey;
		_KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
		_ValueCollection = [];
		_Disposables = [_SourceCollection.ToObservableChangeSet<TSourceCollection, TKey>().Subscribe(OnSourceCollectionChanged)];
	}

	/// <summary>
	/// Handle changes to the source collection.
	/// </summary>
	/// <param name="changes">The set of changes published.</param>
	private void OnSourceCollectionChanged(IChangeSet<TKey> changes)
	{
		foreach (var change in changes)
		{
			switch (change.Reason)
			{
				case ListChangeReason.Add:
					if (change.Item.CurrentIndex == -1)
					{
						_ValueCollection.Add(_Create(change.Item.Current));
					}
					else
					{
						_ValueCollection.Insert(change.Item.CurrentIndex, _Create(change.Item.Current));
					}
					break;
				case ListChangeReason.AddRange:
					if (change.Range.Index == -1)
					{
						_ValueCollection.AddRange(change.Range.Select(_Create));
					}
					else
					{
						_ValueCollection.InsertRange(change.Range.Select(_Create), change.Range.Index);
					}
					break;
				case ListChangeReason.Replace:
					_Modify(change.Item.Current, _ValueCollection[change.Item.CurrentIndex]);
					break;
				case ListChangeReason.Remove:
					if (change.Item.CurrentIndex == -1)
					{
						_ValueCollection.Remove(_ValueCollection.First(value => _KeyComparer.Equals(_GetKey(value), change.Item.Current)));
					}
					else
					{
						_ValueCollection.RemoveAt(change.Item.CurrentIndex);
					}
					break;
				case ListChangeReason.RemoveRange:
					if (change.Range.Index == -1)
					{
						_ValueCollection.Remove(_ValueCollection.Where(value => change.Range.Contains(_GetKey(value), _KeyComparer)));
					}
					else
					{
						_ValueCollection.RemoveRange(change.Range.Index, change.Range.Count);
					}
					break;
				case ListChangeReason.Refresh:
					using (_ValueCollection.SuspendNotifications())
					{
						var oldValues = _ValueCollection.ToArray();
						var oldKeys = oldValues.Select(_GetKey).ToArray();
						_ValueCollection.Clear();
						foreach (var newKey in _SourceCollection)
						{
							var oldIndex = Array.IndexOf(oldKeys, newKey);
							if (oldIndex == -1)
							{
								_ValueCollection.Add(_Create(newKey));
							}
							else
							{
								_ValueCollection.Add(oldValues[oldIndex]);
							}
						}
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
				if (typeof(IDisposable).IsAssignableFrom(typeof(TValue)))
				{
					foreach (var item in _ValueCollection.Cast<IDisposable>())
					{
						item.Dispose();
					}
				}
				_ValueCollection.Clear();
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
