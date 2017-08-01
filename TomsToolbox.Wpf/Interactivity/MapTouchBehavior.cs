﻿namespace TomsToolbox.Wpf.Interactivity
{
    using System.Diagnostics.Contracts;
    using System.Windows.Interactivity;

    using TomsToolbox.Wpf.Controls;

    /// <summary>
    /// Behavior to add support for touch manipulation to the <see cref="Map"/> object.
    /// </summary>
    public class MapTouchBehavior : Behavior<Map>
    {
        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var map = AssociatedObject;
            Contract.Assume(map != null);

            map.ManipulationDelta += Map_ManipulationDelta;
        }

        private void Map_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var map = AssociatedObject;
            Contract.Assume(map != null);

            map.Center += e.DeltaManipulation.Translation;
        }
    }
}
