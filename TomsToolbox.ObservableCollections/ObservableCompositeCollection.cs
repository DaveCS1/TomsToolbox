﻿namespace TomsToolbox.ObservableCollections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using TomsToolbox.Core;

    /// <summary>
    /// Factory methods for the <see cref="ObservableCompositeCollection{T}"/>
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public static class ObservableCompositeCollection
    {
        /// <summary>
        /// Create a collection initially containing one single item
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="singleItem">The first single item in the collection</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing one fixed list with one single item.</returns>
        public static ObservableCompositeCollection<T> FromSingleItem<T>(T singleItem)
        {
            Contract.Requires(!ReferenceEquals(singleItem, null));
            Contract.Ensures(Contract.Result<ObservableCompositeCollection<T>>() != null);

            return new ObservableCompositeCollection<T>(new[] { singleItem });
        }

        /// <summary>
        /// Create a collection initially containing one single item plus one list.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <typeparam name="TItem">The type of elements in the list.</typeparam>
        /// <param name="singleItem">The first single item in the collection</param>
        /// <param name="list">The list to add after the single item.</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing one fixed list with the single item plus all items from the list.</returns>
        public static ObservableCompositeCollection<T> FromSingleItemAndList<T, TItem>(T singleItem, IList<TItem> list)
            where TItem : T
        {
            Contract.Requires(list != null);
            Contract.Requires(!ReferenceEquals(singleItem, null));
            Contract.Ensures(Contract.Result<ObservableCompositeCollection<T>>() != null);

            return typeof(T) == typeof(TItem)
                ? new ObservableCompositeCollection<T>(new[] { singleItem }, (IList<T>)list)
                : new ObservableCompositeCollection<T>(new[] { singleItem }, list.ObservableCast<T>());
        }

        /// <summary>
        /// Create a collection initially containing one list plus one single item.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <typeparam name="TItem">The type of elements in the list.</typeparam>
        /// <param name="list">The list to add before the single item.</param>
        /// <param name="singleItem">The last single item in the collection</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing all items from the list plus one fixed list with the single item at the end.</returns>
        public static ObservableCompositeCollection<T> FromListAndSingleItem<TItem, T>(IList<TItem> list, T singleItem)
            where TItem : T
        {
            Contract.Requires(list != null);
            Contract.Requires(!ReferenceEquals(singleItem, null));
            Contract.Ensures(Contract.Result<ObservableCompositeCollection<T>>() != null);

            return typeof(T) == typeof(TItem)
            ? new ObservableCompositeCollection<T>((IList<T>)list, new[] { singleItem })
            : new ObservableCompositeCollection<T>(list.ObservableCast<T>(), new[] { singleItem });
        }
    }

    /// <summary>
    /// View a set of collections as one continuous list.
    /// <para/>
    /// Similar to the System.Windows.Data.CompositeCollection, plus:
    /// <list type="bullet">
    /// <item>Generic type</item>
    /// <item>Transparent separation of the real content and the resulting list</item>
    /// <item>Nestable, i.e. composite collections of composite collections</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection</typeparam>
    public sealed class ObservableCompositeCollection<T> : ReadOnlyObservableCollection<T>, IObservableCollection<T>
    {
        [ContractPublicPropertyName("Content")]
        private readonly ContentManager _content;

        /// <summary>
        /// Taking care of the physical content
        /// </summary>
        private class ContentManager : IList<IList<T>>
        {
            // The parts that make up the composite collection
            private readonly List<IList<T>> _parts = new List<IList<T>>();
            // The composite collection that we manage
            private readonly ObservableCompositeCollection<T> _owner;

            public ContentManager(ObservableCompositeCollection<T> owner)
            {
                Contract.Requires(owner != null);

                _owner = owner;
            }

            [ContractVerification(false)]
            private void parts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                // Monitor changes of parts and forward events properly
                // Offset to apply is the sum of all counts of all parts preceding this part
                var offset = _parts.TakeWhile(part => !part.Equals(sender)).Select(part => part.Count).Sum();

                _owner.ContentCollectionChanged(TranslateEventArgs(e, offset));
            }

            private static NotifyCollectionChangedEventArgs TranslateEventArgs(NotifyCollectionChangedEventArgs e, int offset)
            {
                // Translate given event args by adding the given offset to the starting index
                Contract.Requires(e != null);

                if (offset == 0)
                    return e; // no offset - nothing to translate

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.NewStartingIndex + offset);

                    case NotifyCollectionChangedAction.Reset:
                        return e;

                    case NotifyCollectionChangedAction.Remove:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, e.OldStartingIndex + offset);

                    case NotifyCollectionChangedAction.Move:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.NewStartingIndex + offset, e.OldStartingIndex + offset);

                    case NotifyCollectionChangedAction.Replace:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, e.OldStartingIndex + offset);

                    default:
                        throw new NotImplementedException();
                }
            }

            #region IList<IList<T>> Members

            [ContractVerification(false)] // Just forwarding...
            public int IndexOf(IList<T> item)
            {
                return _parts.IndexOf(item);
            }

            public void Insert(int index, IList<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                if (index > _parts.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                // now add this list to the list of all
                _parts.Insert(index, item);

                // If this list implements INotifyCollectionChanged monitor changes so they can be forwarded properly.
                // If it does not implement INotifyCollectionChanged, no notification should be neccesary.
                var dynamicPart = item as INotifyCollectionChanged;
                if (dynamicPart != null)
                    dynamicPart.CollectionChanged += parts_CollectionChanged;

                if (item.Count == 0)
                    return;

                // calculate the absolute offset of the first item (= sum of all preceding items in all lists before the new item)
                var offset = (_parts.GetRange(0, index).Select(p => p.Count)).Sum();
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)item, offset);
                _owner.ContentCollectionChanged(args);
            }

            [ContractVerification(false)] // Just forwarding...
            public void RemoveAt(int index)
            {
                var part = _parts[index];
                Contract.Assume(part != null);
                _parts.RemoveAt(index);

                // If this list implements INotifyCollectionChanged events must be unregistered!
                var dynamicPart = part as INotifyCollectionChanged;
                if (dynamicPart != null)
                    dynamicPart.CollectionChanged -= parts_CollectionChanged;

                if (part.Count == 0)
                    return;

                // calculate the absolute offset of the first item (sum of all items in all lists before the removed item)
                var offset = (_parts.GetRange(0, index).Select(p => p.Count)).Sum();

                _owner.ContentCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)part, offset));
            }

            [ContractVerification(false)] // Just forwarding...
            public IList<T> this[int index]
            {
                get
                {
                    return _parts[index];
                }
                set
                {
                    RemoveAt(index);
                    Insert(index, value);
                }
            }

            #endregion

            #region ICollection<IList<T>> Members

            [ContractVerification(false)] // just forwarding
            public void Add(IList<T> item)
            {
                Insert(_parts.Count, item);
            }

            public void Clear()
            {
                while (Count > 0)
                {
                    RemoveAt(0);
                }
            }

            public bool Contains(IList<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                return IndexOf(item) != -1;
            }

            public void CopyTo(IList<T>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count => _parts.Count;

            public bool IsReadOnly => false;

            public bool Remove(IList<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                var index = IndexOf(item);
                if (index == -1)
                    return false;
                RemoveAt(index);
                return true;
            }

            #endregion

            #region IEnumerable<IList> Members

            public IEnumerator<IList<T>> GetEnumerator()
            {
                return _parts.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _parts.GetEnumerator();
            }

            #endregion

            #region Contracts Invariant

            [ContractInvariantMethod]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
            private void ObjectInvariant()
            {
                Contract.Invariant(_owner != null);
                Contract.Invariant(_parts != null);
            }

            #endregion
        }

        /// <summary>
        /// Create an empty collection
        /// </summary>
        public ObservableCompositeCollection()
            : base(new ObservableCollection<T>())
        {
            _content = new ContentManager(this);
        }

        /// <summary>
        /// Create a collection initially wrapping a set of lists
        /// </summary>
        /// <param name="parts">The lists to wrap</param>
        /// <exception cref="System.ArgumentException">None of the parts may be null!</exception>
        public ObservableCompositeCollection(params IList<T>[] parts)
            : this()
        {
            Contract.Requires(parts != null);

            foreach (var part in parts)
            {
                if (part == null)
                    throw new ArgumentException(@"None of the parts may be null!");

                _content.Add(part);
            }
        }

        /// <summary>
        /// Access to the physical layer of the content
        /// </summary>
        public IList<IList<T>> Content
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<IList<T>>>() != null);

                return _content;
            }
        }

        [ContractVerification(false)]
        private void ContentCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var insertionIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems.Cast<T>())
                    {
                        Items.Insert(insertionIndex++, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var removeIndex = e.OldStartingIndex;
                    for (var k = 0; k < e.OldItems.Count; k++)
                    {
                        Items.RemoveAt(removeIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var replaceIndex = e.OldStartingIndex;
                    foreach (var item in e.NewItems.Cast<T>())
                    {
                        Items[replaceIndex++] = item;
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    Contract.Assume(e.OldItems.Count == 1);
                    ((ObservableCollection<T>)Items).Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    Items.AddRange(_content.SelectMany(list => list));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                base.CollectionChanged += value;
            }
            remove
            {
                base.CollectionChanged -= value;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                base.PropertyChanged += value;
            }
            remove
            {
                base.PropertyChanged -= value;
            }
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_content != null);
        }
    }
}