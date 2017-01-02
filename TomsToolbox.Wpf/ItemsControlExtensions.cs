﻿namespace TomsToolbox.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using JetBrains.Annotations;

    /// <summary>
    /// Extensions and helpers for the <see cref="ItemsControl"/> or derived classes.
    /// </summary>
    public static class ItemsControlExtensions
    {
        /// <summary>
        /// Gets the default item command. See <see cref="P:TomsToolbox.Wpf.ItemsControlExtensions.DefaultItemCommand"/> attached property for details.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The command.</returns>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static ICommand GetDefaultItemCommand([NotNull] this ItemsControl obj)
        {
            Contract.Requires(obj != null);
            return (ICommand)obj.GetValue(DefaultItemCommandProperty);
        }
        /// <summary>
        /// Sets the default item command. See <see cref="P:TomsToolbox.Wpf.ItemsControlExtensions.DefaultItemCommand"/> attached property for details.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The command.</param>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static void SetDefaultItemCommand([NotNull] this ItemsControl obj, ICommand value)
        {
            Contract.Requires(obj != null);
            obj.SetValue(DefaultItemCommandProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:TomsToolbox.Wpf.ItemsControlExtensions.DefaultItemCommand"/> attached property
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// The default item command is the command that will be executed when an item of the items control has received a mouse double click or enter key. 
        /// It is not executed when the double-click is on the background or on the scrollbar.
        /// This command avoids the ubiquitous wrong implementations as well as code duplication when handling double-clicks in items controls like the <see cref="ListBox"/>
        /// <para/>
        /// The command parameter for the command is the item that has been clicked.
        /// </summary>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty DefaultItemCommandProperty =
            DependencyProperty.RegisterAttached("DefaultItemCommand", typeof(ICommand), typeof(ItemsControlExtensions), new FrameworkPropertyMetadata(DefaultItemCommand_Changed));

        private static void DefaultItemCommand_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = d as ItemsControl;
            if (itemsControl == null)
                return;

            itemsControl.MouseDoubleClick -= ItemsControl_MouseDoubleClick;
            itemsControl.KeyDown -= ItemsControl_KeyDown;

            if (e.NewValue == null)
                return;

            itemsControl.MouseDoubleClick += ItemsControl_MouseDoubleClick;
            itemsControl.KeyDown += ItemsControl_KeyDown;
        }

        static void ItemsControl_KeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            if ((e.Key != Key.Enter) || e.Handled)
                return;

            ExecuteCommand(sender, e);
        }

        static void ItemsControl_MouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            ExecuteCommand(sender, e);
        }

        private static void ExecuteCommand([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            if (itemsControl == null)
                return;

            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
                return;

            var command = GetDefaultItemCommand(itemsControl);
            if (command == null)
                return;

            // Bubble up until we find the ItemContainer and it's associated item.
            foreach (var element in originalSource.AncestorsAndSelf())
            {
                if (ReferenceEquals(element, itemsControl))
                    return; // we have clicked somewhere else, but not on the ItemContainer.

                if (element is GroupItem)
                    return;

                // ReSharper disable once PossibleNullReferenceException
                var item = itemsControl.ItemContainerGenerator.ItemFromContainer(element);

                if ((item == null) || (item == DependencyProperty.UnsetValue))
                    continue;

                if (command.CanExecute(item))
                    command.Execute(item);

                return;
            }
        }

        /// <summary>
        /// Gets the object that will be observed for changes. 
        /// A change of the object will trigger a refresh on the collection view of the attached items control.
        /// </summary>
        /// <param name="obj">The <see cref="ItemsControl"/> to refresh.</param>
        /// <returns>The object to observe.</returns>
        public static object GetRefreshOnSourceChanges([NotNull] ItemsControl obj)
        {
            Contract.Requires(obj != null);
            return obj.GetValue(RefreshOnSourceChangesProperty);
        }
        /// <summary>
        /// Sets the object that will be observed for changes. 
        /// A change of the object will trigger a refresh on the collection view of the attached items control.
        /// </summary>
        /// <param name="obj">The <see cref="ItemsControl"/> to refresh.</param>
        /// <param name="value">The object to observe.</param>
        public static void SetRefreshOnSourceChanges([NotNull] ItemsControl obj, object value)
        {
            Contract.Requires(obj != null);
            obj.SetValue(RefreshOnSourceChangesProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:TomsToolbox.Wpf.ItemsControlExtensions.RefreshOnSourceChanges"/> attached property
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// The object that will be observed for changes. A change of the object will trigger a refresh on the collection view of the attached items control.
        /// </summary>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty RefreshOnSourceChangesProperty =
            DependencyProperty.RegisterAttached("RefreshOnSourceChanges", typeof(object), typeof(ItemsControlExtensions), new FrameworkPropertyMetadata(Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = d as ItemsControl;
            if (itemsControl == null)
                return;

            // Collection views are maybe nested! Must recurse through all views, the top one might not own the filter!
            ICollectionView itemCollection = itemsControl.Items;
            while (itemCollection != null)
            {
                itemCollection.Refresh();

                var sourceCollection = itemCollection.SourceCollection as ICollectionView;
                if (itemCollection == sourceCollection)
                    break;

                itemCollection = sourceCollection;
            }
        }


        /// <summary>
        /// Gets the item containers for a items control.
        /// </summary>
        /// <typeparam name="T">The type of the containers.</typeparam>
        /// <param name="itemsControl">The items control.</param>
        /// <returns>The list of containers; contains <c>null</c> entries for unrealized containers (see <see cref="ItemContainerGenerator.ContainerFromIndex"/>).</returns>
        [NotNull]
        public static IEnumerable<T> GetItemContainers<T>([NotNull] this ItemsControl itemsControl)
            where T : DependencyObject
        {
            Contract.Requires(itemsControl != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var generator = itemsControl.ItemContainerGenerator;

            var itemsCount = Math.Max(0, itemsControl.Items.Count);

            return Enumerable.Range(0, itemsCount)
                .Select(i => generator.ContainerFromIndex(i) as T);
        }
    }
}
