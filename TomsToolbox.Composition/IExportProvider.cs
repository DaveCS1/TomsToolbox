﻿namespace TomsToolbox.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    /// <summary>
    /// Retrieves exports which match a specified ImportDefinition object.
    /// </summary>
    public interface IExportProvider
    {
        /// <summary>
        /// Occurs when the exports in the IExportProvider change.
        /// </summary>
        event EventHandler<EventArgs> ExportsChanged;

        /// <summary>
        /// Gets the exported object with the specified contract name.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object</returns>
        [NotNull]
        T GetExportedValue<T>([CanBeNull] string contractName = null);

        /// <summary>
        /// Gets the exported object with the specified contract name.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object, or null if no such export exists.</returns>
        [CanBeNull]
        T GetExportedValueOrDefault<T>([CanBeNull] string contractName = null);

        /// <summary>
        /// Gets all the exported objects with the specified contract name.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object</returns>
        [NotNull, ItemNotNull]
        IEnumerable<T> GetExportedValues<T>([CanBeNull] string contractName = null);

        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <param name="contractType">The type of the requested object.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        IEnumerable<IExport<object>> GetExports([NotNull] Type contractType, [CanBeNull] string contractName = null);
    }

    /// <summary>
    /// Extension methods for the <see cref="IExportProvider"/> interface.
    /// </summary>
    public static class ExportProviderExtensions
    {
        /// <summary>
        /// Tries to the get exported value.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the value exists.</returns>
        [ContractAnnotation("=> false,value:null;=>true,value:notnull")]
        public static bool TryGetExportedValue<T>([NotNull] this IExportProvider exportProvider, [CanBeNull] out T value)
        {
            return (value = exportProvider.GetExportedValueOrDefault<T>()) != null;
        }

        /// <summary>
        /// Tries to the get exported value.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// true if the value exists.
        /// </returns>
        [ContractAnnotation("=> false,value:null;=>true,value:notnull")]
        public static bool TryGetExportedValue<T>([NotNull] this IExportProvider exportProvider, [CanBeNull] string contractName, [CanBeNull] out T value)
        {
            return (value = exportProvider.GetExportedValueOrDefault<T>(contractName)) != null;
        }

        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="type">The type of the requested object.</param>
        /// <param name="metadataFactory">The factory method to create the metadata object from the metadata dictionary.</param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<IExport<object, TMetadata>> GetExports<TMetadata>(this IExportProvider exportProvider, [NotNull] Type type, [NotNull] Func<IDictionary<string, object>, TMetadata> metadataFactory)
        {
            return GetExports(exportProvider, type, null, metadataFactory);
        }

        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="type">The type of the requested object.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="metadataFactory">The factory method to create the metadata object from the metadata dictionary.</param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<IExport<object, TMetadata>> GetExports<TMetadata>(this IExportProvider exportProvider, [NotNull] Type type, [CanBeNull] string contractName, [NotNull] Func<IDictionary<string, object>, TMetadata> metadataFactory)
        {
            return exportProvider
                .GetExports(type, contractName)
                .Select(item => new ExportAdapter<object, TMetadata>(item, metadataFactory));

        }

        private class ExportAdapter<TObject, TMetadata> : IExport<TObject, TMetadata>
        {
            private readonly IExport<TObject> _source;

            public ExportAdapter(IExport<TObject> source, Func<IDictionary<string, object>, TMetadata> metadataFactory)
            {
                _source = source;
                Metadata = metadataFactory(source.Metadata);
            }

            [CanBeNull]
            public TObject Value => _source.Value;

            [CanBeNull]
            public TMetadata Metadata { get; }
        }
    }
}
