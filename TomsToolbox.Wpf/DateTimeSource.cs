﻿namespace TomsToolbox.Wpf
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Data;
    using System.Windows.Threading;

    using TomsToolbox.Desktop;

    /// <summary>
    /// Provides values for date and time suitable for bindings.
    /// </summary>
    /// <remarks>
    /// This expression in XAML would be static, since the Source is never updated and would always have 
    /// it's initial value, <see cref="BindingExpression.UpdateTarget"/> won't have any effect.
    /// <para/>
    /// MyDayOfWeek="{Binding Path=DayOfWeek, Source={x:Static system:DateTime.Today}}"
    /// <para/>
    /// Using <see cref="DateTimeSource"/> instead, <see cref="BindingExpression.UpdateTarget"/> will work, 
    /// and MyDayOfWeek will be updated with the actual value:
    /// <para/>
    /// MyDayOfWeek="{Binding Path=Today.DayOfWeek, Source={x:Static toms:DateTimeSource.Default}}"
    /// </remarks>
    public class DateTimeSource : ObservableObject
    {
        private readonly DispatcherTimer _updateTimer;

        /// <summary>
        /// The default singleton object. Use this as a source for binding that supports manual updating.
        /// </summary>
        public static readonly DateTimeSource Default = new DateTimeSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSource"/> class.
        /// </summary>
        public DateTimeSource()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        private void UpdateTimer_Tick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Now");
            OnPropertyChanged("Today");
            OnPropertyChanged("UtcNow");
        }

        /// <summary>
        /// Gets or sets the interval in which the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised for all properties.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get
            {
                return _updateTimer.Interval;
            }
            set
            {
                if (value > TimeSpan.Zero)
                {
                    _updateTimer.Restart();
                    _updateTimer.Interval = value;
                }
                else
                {
                    _updateTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.DateTime"/> object that is set to the current date and time on this computer, expressed as the local time.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DateTime"/> whose value is the current local date and time.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Should only be accessible via the Default singleton.")]
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DateTime"/> set to today's date, with the time component set to 00:00:00.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Should only be accessible via the Default singleton.")]
        public DateTime Today
        {
            get
            {
                return DateTime.Today;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.DateTime"/> object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DateTime"/> whose value is the current UTC date and time.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Should only be accessible via the Default singleton.")]
        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
