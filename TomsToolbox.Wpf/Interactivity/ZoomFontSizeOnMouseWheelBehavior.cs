﻿namespace TomsToolbox.Wpf.Interactivity
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    /// <summary>
    /// When attached to a framework element, the <see cref="P:System.Windows.Documents.TextElement.FontSize"/> property 
    /// will be changed upon Ctrl+MouseWheel events.
    /// </summary>
    public class ZoomFontSizeOnMouseWheelBehavior : Behavior<FrameworkElement>
    {
        private double? _initialFontSize;
        private int _zoomOffset;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            Contract.Assume(AssociatedObject != null);

            AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            Contract.Assume(AssociatedObject != null);

            AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
        }

        private void AssociatedObject_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if ((!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) || (e.Delta == 0))
                return;

            e.Handled = true;

            var newZoomOffset = _zoomOffset + Math.Sign(e.Delta);

            if (!_initialFontSize.HasValue)
            {
                _initialFontSize = TextElement.GetFontSize(AssociatedObject);
            }

            var newFontSize = _initialFontSize.Value + newZoomOffset;

            if ((newFontSize >= 4) && (newFontSize < 48))
            {
                _zoomOffset = newZoomOffset;
                TextElement.SetFontSize(AssociatedObject, newFontSize);

                if (newZoomOffset == 0)
                {
                    _initialFontSize = null;
                }
            }
        }
    }
}
