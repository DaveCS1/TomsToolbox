﻿namespace TomsToolbox.Desktop
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows.Threading;

    /// <summary>
    /// Implements a simple timed throttle.<para/>
    /// Calling <see cref="Tick()"/> multiple times will restart the timer; there will be one single 
    /// call to the action when the delay time has elapsed after the last tick.
    /// </summary>
    public class Throttle
    {
        private readonly Action _target;
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class with a default timeout of 100ms.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public Throttle(Action target)
            : this(TimeSpan.FromMilliseconds(100), target)
        {
            Contract.Requires(target != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class.
        /// </summary>
        /// <param name="timeout">The timeout to wait for after the last <see cref="Tick()"/>.</param>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public Throttle(TimeSpan timeout, Action target)
        {
            Contract.Requires(target != null);

            _target = target;
            _timer = new DispatcherTimer { Interval = timeout };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs eventArgs)
        {
            _timer.Stop();
            _target();
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_target != null);
            Contract.Invariant(_timer != null);
        }
    }
}
