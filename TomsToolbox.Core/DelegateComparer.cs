﻿namespace TomsToolbox.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// <see cref="IComparer{T}"/> implementation using a delegate function to compare the values.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    public class DelegateComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateComparer{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public DelegateComparer(Func<T, T, int> comparer)
        {
            Contract.Requires(comparer != null);

            _comparer = comparer;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(T x, T y)
        {
            if (!typeof(T).IsValueType)
            {
                if (ReferenceEquals(x, null))
                    return ReferenceEquals(y, null) ? 0 : -1;

                if (ReferenceEquals(y, null))
                    return 1;
            }

            return _comparer(x, y);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_comparer != null);
        }
    }
}
