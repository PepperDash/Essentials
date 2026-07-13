using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PepperDash.Essentials.Core;

 /// <summary>
 /// Represents a RoutingPortCollection, which behaves like a List with an indexer for case-insensitive lookup of
 /// ports by their key names. Enforces unique port keys (case-insensitive) - attempting to add a port whose key
 /// already exists in the collection throws an <see cref="ArgumentException"/>.
 /// </summary>
 /// <remarks>
 /// Derives from <see cref="Collection{T}"/> rather than <see cref="List{T}"/> so key-uniqueness can be enforced
 /// via the virtual Insert/Set/Remove/Clear hooks (List{T}'s members are not virtual). Public surface area
 /// (int indexer, Add/Insert/Remove/Clear, collection-initializer support, enumeration) is preserved; an
 /// <see cref="AddRange"/> method is provided for source compatibility with existing List{T}.AddRange call sites.
 /// </remarks>
	public class RoutingPortCollection<T> : Collection<T> where T: RoutingPort
	{
		private readonly Dictionary<string, T> _portsByKey = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Case-insensitive port lookup linked to ports' keys
		/// </summary>
		public T this[string key]
		{
			get
			{
				return key != null && _portsByKey.TryGetValue(key, out var port) ? port : null;
			}
		}

		/// <summary>
		/// Adds a range of ports to the collection. Provided for source compatibility with List{T}.AddRange.
		/// </summary>
		/// <param name="items">The ports to add.</param>
		public void AddRange(IEnumerable<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			foreach (var item in items)
			{
				Add(item);
			}
		}

		/// <inheritdoc/>
		protected override void InsertItem(int index, T item)
		{
			ValidateNewItem(item);
			base.InsertItem(index, item);
			_portsByKey[item.Key] = item;
		}

		/// <inheritdoc/>
		protected override void SetItem(int index, T item)
		{
			ValidateNewItem(item, indexBeingReplaced: index);
			var replaced = Items[index];
			base.SetItem(index, item);
			if (replaced != null && !string.Equals(replaced.Key, item.Key, StringComparison.OrdinalIgnoreCase))
			{
				_portsByKey.Remove(replaced.Key);
			}
			_portsByKey[item.Key] = item;
		}

		/// <inheritdoc/>
		protected override void RemoveItem(int index)
		{
			var removed = Items[index];
			base.RemoveItem(index);
			if (removed != null)
			{
				_portsByKey.Remove(removed.Key);
			}
		}

		/// <inheritdoc/>
		protected override void ClearItems()
		{
			base.ClearItems();
			_portsByKey.Clear();
		}

		private void ValidateNewItem(T item, int? indexBeingReplaced = null)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			if (string.IsNullOrEmpty(item.Key))
				throw new ArgumentException("RoutingPort must have a non-empty Key", nameof(item));

			if (_portsByKey.TryGetValue(item.Key, out var existing))
			{
				var existingIndex = Items.IndexOf(existing);
				if (existingIndex != indexBeingReplaced)
				{
					throw new ArgumentException(
						$"A RoutingPort with key '{item.Key}' already exists in this RoutingPortCollection", nameof(item));
				}
			}
		}
	}
