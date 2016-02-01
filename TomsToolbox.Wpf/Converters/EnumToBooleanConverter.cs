﻿namespace TomsToolbox.Wpf.Converters
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Tests if an enum value matches one of the given values provides as the converter parameter. 
    /// If the enum has a <see cref="FlagsAttribute"/>, the match is done with the logic "is any flag set".
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// The singleton instance of the converter.
        /// </summary>
        public static readonly IValueConverter Default = new EnumToBooleanConverter();

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, parameter as string);
        }

        /// <summary>
        /// Converts the specified enum value into a boolean.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="matches">A comma separated list of enum names to match.</param>
        /// <returns>True if the value matches one of the enum names.</returns>
        public static bool Convert(object value, string matches)
        {
            if (value == null)
                return false;
            if (matches == null)
                return false;

            var valueType = value.GetType();
            if (!valueType.IsEnum)
                return false;

            try
            {
                var valueValue = System.Convert.ToInt64(value, CultureInfo.InvariantCulture);

                var typeConverter = TypeDescriptor.GetConverter(value);
                var matchesList = matches.Split(',').Select(typeConverter.ConvertFromInvariantString).Select(x => System.Convert.ToInt64(x, CultureInfo.InvariantCulture));

                return Attribute.IsDefined(valueType, typeof(FlagsAttribute))
                    ? matchesList.Any(x => (valueValue & x) != 0)
                    : matchesList.Contains(valueValue);
            }
            catch (SystemException ex)
            {
                if (Debugger.IsAttached)
                {
                    Debug.Fail(ex.ToString());
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
