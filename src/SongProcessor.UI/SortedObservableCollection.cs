﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SongProcessor.UI;

public class SortedObservableCollection<T>(IComparer<T> comparer, IEnumerable<T>? collection = null) : ObservableCollection<T>(collection ?? [])
{
	public IComparer<T> Comparer { get; } = comparer;

	protected int GetSortedIndex(T item)
	{
		lock (((ICollection)this).SyncRoot)
		{
			var i = 0;
			for (; i < Items.Count; ++i)
			{
				if (Comparer.Compare(item, Items[i]) < 1)
				{
					break;
				}
			}
			return i;
		}
	}

	protected override void InsertItem(int index, T item)
	{
		index = GetSortedIndex(item);
		base.InsertItem(index, item);
	}

	protected override void MoveItem(int oldIndex, int newIndex)
		=> throw new NotSupportedException($"Items cannot be moved in a {nameof(SortedObservableCollection<T>)}.");

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items)));
		base.OnCollectionChanged(e);
	}

	protected override void SetItem(int index, T item)
		=> throw new NotSupportedException($"Items cannot be set in a {nameof(SortedObservableCollection<T>)}.");
}