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

public class ReadOnlyObservableMappedCollection<TKey, TSourceCollection, TValue> :
	IReadOnlyList<TValue>,
	IList,
	INotifyPropertyChanged,
	INotifyCollectionChanged,
	IDisposable
	where TSourceCollection : INotifyCollectionChanged, IReadOnlyList<TKey>
{
	private readonly TSourceCollection _SourceCollection;

	private readonly ObservableCollectionExtended<TValue> _ValueCollection;

	private readonly Func<TKey, TValue> _Create;

	private readonly Action<TKey, TValue> _Modify;

	private readonly Func<TValue, TKey> _GetKey;

	private readonly IEqualityComparer<TKey> _KeyComparer;

	private readonly CompositeDisposable _Disposables;

	private bool disposedValue;

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

	public TValue this[int index] => _ValueCollection[index];

	public int Count => _ValueCollection.Count;

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => true;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get;
	} = new();

	object? IList.this[int index]
	{
		get => this[index];
		set => throw new InvalidOperationException();
	}

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

	public IEnumerator<TValue> GetEnumerator()
	{
		return _ValueCollection.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

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

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	int IList.Add(object? value)
	{
		throw new InvalidOperationException();
	}

	void IList.Clear()
	{
		throw new InvalidOperationException();
	}

	bool IList.Contains(object? value)
	{
		return value is TValue tValue && _ValueCollection.Contains(tValue);
	}

	int IList.IndexOf(object? value)
	{
		return value is TValue tValue ? _ValueCollection.IndexOf(tValue) : -1;
	}

	void IList.Insert(int index, object? value)
	{
		throw new InvalidOperationException();
	}

	void IList.Remove(object? value)
	{
		throw new InvalidOperationException();
	}

	void IList.RemoveAt(int index)
	{
		throw new InvalidOperationException();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_ValueCollection).CopyTo(array, index);
	}
}
