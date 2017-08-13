﻿namespace TomsToolbox.Desktop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using JetBrains.Annotations;

#if NETFRAMEWORK_4_5
    using System.Collections;
    using TomsToolbox.Core;
#endif

    /// <summary>
    /// A validation template for using Validar.Fody (<see href="https://github.com/Fody/Validar"/>) with data annotations (<see cref="N:System.ComponentModel.DataAnnotations"/>).<para/>
    /// </summary>
    /// <example>
    /// To activate the validation template just add this line to your AssemblyInfo.cs after you have installed the Validar.Fody package:<para/>
    /// <code language="CS">
    /// [assembly: ValidationTemplateAttribute(typeof(TomsToolbox.Desktop.ValidationTemplate))]
    /// </code>
    /// </example>
    /// <remarks>When using the net45 package, INotifyDataErrorInfo is supported as well.</remarks>
    public class ValidationTemplate : IDataErrorInfo
#if NETFRAMEWORK_4_5
        , INotifyDataErrorInfo
#endif
    {
        [NotNull]
        private readonly INotifyPropertyChanged _target;
        [NotNull]
        private readonly ValidationContext _validationContext;
        [NotNull, ItemNotNull]
        private List<ValidationResult> _validationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationTemplate"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public ValidationTemplate([NotNull] INotifyPropertyChanged target)
        {
            Contract.Requires(target != null);

            _target = target;
            _validationContext = new ValidationContext(target, null, null);
            _validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(target, _validationContext, _validationResults, true);

            target.PropertyChanged += Validate;
        }

        private void Validate(object sender, PropertyChangedEventArgs e)
        {
            _validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(_target, _validationContext, _validationResults, true);

#if NETFRAMEWORK_4_5

            _validationResults
                .SelectMany(x => x.MemberNames)
                .Distinct()
                .ForEach(RaiseErrorsChanged);
#endif 
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        [NotNull]
        public string Error
        {
            get
            {
                var strings = _validationResults
                    .Select(x => x.ErrorMessage);

                return string.Join(Environment.NewLine, strings);
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        [NotNull]
        public string this[[CanBeNull] string columnName]
        {
            get
            {
                var strings = _validationResults
                    .Where(x => x.MemberNames.Contains(columnName))
                    .Select(x => x.ErrorMessage);

                return string.Join(Environment.NewLine, strings);
            }
        }

#if NETFRAMEWORK_4_5

        /// <summary>
        /// Raised when the errors for a property has changed.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return _validationResults
                .Where(x => x.MemberNames.Contains(propertyName))
                .Select(x => x.ErrorMessage);
        }

        bool INotifyDataErrorInfo.HasErrors => _validationResults.Count > 0;

#endif

        [ContractInvariantMethod, UsedImplicitly]
        [SuppressMessage("Microsoft.Performance", "CA1822: MarkMembersAsStatic", Justification = "Required for code contracts.")]
        [Conditional("CONTRACTS_FULL")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_target != null);
            Contract.Invariant(_validationContext != null);
            Contract.Invariant(_validationResults != null);
        }
    }
}
