namespace TomsToolbox.Wpf.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.Contracts;
    using System.Windows;

    /// <summary>
    /// Attribute to apply to views to dynamically generate the <see cref="System.Windows.DataTemplate"/> that associates the view with it's view model.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataTemplateAttribute : ExportAttribute, IDataTemplateMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTemplateAttribute" /> class.
        /// </summary>
        /// <param name="viewModel">The type of the view model for which the template is designed.</param>
        public DataTemplateAttribute(Type viewModel)
            : base(DataTemplateManager.ContractName, typeof(DependencyObject))
        {
            Contract.Requires(viewModel != null);

            ViewModel = viewModel;
        }

        /// <summary>
        /// Gets the type of the view model that this visual has a representation for.
        /// </summary>
        public Type ViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the role of this visual. 
        /// If a role is set, a <see cref="RoleBasedDataTemplateKey"/> will be created for this view; else a simple <see cref="DataTemplateKey"/> is used.
        /// </summary>
        public object Role
        {
            get;
            set;
        }
    }
}