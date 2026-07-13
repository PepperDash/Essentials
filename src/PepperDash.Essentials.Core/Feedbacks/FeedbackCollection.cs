using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Behaves like a List, with an indexer to find feedbacks by key name. Enforces unique feedback keys
/// (case-insensitive) - attempting to add a feedback whose key already exists in the collection throws an
/// <see cref="ArgumentException"/>. Feedbacks with no explicit key (<see cref="Feedback.Key"/> == "") are exempt
/// from the uniqueness check, since many feedbacks are intentionally constructed without a key and are not meant
/// to be looked up by the string indexer.
/// </summary>
/// <remarks>
/// Derives from <see cref="Collection{T}"/> rather than <see cref="List{T}"/> so key-uniqueness can be enforced
/// via the virtual Insert/Set/Remove/Clear hooks (List{T}'s members are not virtual). Public surface area
/// (int indexer, Add/Insert/Remove/Clear, collection-initializer support, enumeration) is preserved; an
/// <see cref="AddRange"/> method is provided for source compatibility with existing List{T}.AddRange call sites.
/// </remarks>
public class FeedbackCollection<T> : Collection<T> where T : Feedback
{
    private readonly Dictionary<string, T> _feedbacksByKey = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Case-insensitive port lookup linked to feedbacks' keys
    /// </summary>
    public T this[string key]
    {
        get
        {
            return key != null && _feedbacksByKey.TryGetValue(key, out var feedback) ? feedback : null;
        }
    }

    /// <summary>
    /// Adds a range of feedbacks to the collection. Provided for source compatibility with List{T}.AddRange.
    /// </summary>
    /// <param name="items">The feedbacks to add.</param>
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
        if (!string.IsNullOrEmpty(item.Key))
        {
            _feedbacksByKey[item.Key] = item;
        }
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        ValidateNewItem(item, indexBeingReplaced: index);
        var replaced = Items[index];
        base.SetItem(index, item);
        if (replaced != null && !string.IsNullOrEmpty(replaced.Key) &&
            !string.Equals(replaced.Key, item.Key, StringComparison.OrdinalIgnoreCase))
        {
            _feedbacksByKey.Remove(replaced.Key);
        }
        if (!string.IsNullOrEmpty(item.Key))
        {
            _feedbacksByKey[item.Key] = item;
        }
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        var removed = Items[index];
        base.RemoveItem(index);
        if (removed != null && !string.IsNullOrEmpty(removed.Key))
        {
            _feedbacksByKey.Remove(removed.Key);
        }
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        base.ClearItems();
        _feedbacksByKey.Clear();
    }

    private void ValidateNewItem(T item, int? indexBeingReplaced = null)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (string.IsNullOrEmpty(item.Key))
            return;

        if (_feedbacksByKey.TryGetValue(item.Key, out var existing))
        {
            var existingIndex = Items.IndexOf(existing);
            if (existingIndex != indexBeingReplaced)
            {
                throw new ArgumentException(
                    $"A Feedback with key '{item.Key}' already exists in this FeedbackCollection", nameof(item));
            }
        }
    }
}