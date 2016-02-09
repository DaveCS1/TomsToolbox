﻿namespace TomsToolbox.Wpf.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    using TomsToolbox.Desktop;

    /// <summary>
    /// Converts WGS-84 coordinates into logical XY coordinates in the range 0..1 and back.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class CoordinatesToPointConverter : IValueConverter
    {
        /// <summary>
        /// The singleton instance of the converter.
        /// </summary>
        public static readonly IValueConverter Default = new CoordinatesToPointConverter();

        /// <summary>
        /// Converts a value.
        /// Null and UnSet are unchanged.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        private static object Convert(object value)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;

            if (value is Point)
            {
                return (Coordinates)(Point)value;
            }

            if (value is Coordinates)
            {
                return (Point)(Coordinates)value;
            }

            PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 9000, "{0} failed: {1}", typeof(CoordinatesToPointConverter).Name, "Source in neither of type Point no of type Coordinates.");
            return DependencyProperty.UnsetValue;
        }
    }
}
