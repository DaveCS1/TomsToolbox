﻿namespace TomsToolbox.Wpf.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Attribute to apply to view models to support visual composition.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    [CLSCompliant(false)] // attributes must used arrays, even as properties.
    public sealed class VisualCompositionExportAttribute : ExportAttribute, IVisualCompositionMetadata
    {
        private readonly string[] _targetRegions;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualCompositionExportAttribute" /> class.
        /// </summary>
        /// <param name="targetRegions">The names of the region(s) where this view should appear.</param>
        public VisualCompositionExportAttribute(params string[] targetRegions)
            : base(typeof(IComposablePart))
        {
            Contract.Requires(targetRegions != null);

            _targetRegions = targetRegions;
        }

        /// <summary>
        /// Gets the role of the view model for visual composition.
        /// </summary>
        public object Role
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a sequence to support ordering of view model collections.
        /// </summary>
        public double Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the target regions for visual composition.
        /// </summary>
        public string[] TargetRegions
        {
            get
            {
                Contract.Ensures(Contract.Result<object[]>() != null);

                return _targetRegions;
            }
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_targetRegions != null);
        }
    }
}
