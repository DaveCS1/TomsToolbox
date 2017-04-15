﻿namespace TomsToolbox.Desktop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;

    using JetBrains.Annotations;

    /// <summary>
    /// Helper methods to interchange data via clipboard.
    /// </summary>
    public static class ClipboardHelper
    {
        private const string Quote = "\"";

        /// <summary>
        /// The text column separator
        /// </summary>
        public const char TextColumnSeparator = '\t';

        /// <summary>
        /// Gets the effectove CSV column separator for the current culture.
        /// </summary>
        public static char CsvColumnSeparator => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "," ? ';' : ',';

        /// <summary>
        /// Gets the clipboard data as a table.
        /// </summary>
        /// <returns>The parsed clipboard data as a table, or <c>null</c> if the clipboard is empty or does not contain normalized table data.</returns>
        /// <remarks>If no TEXT is present in the clipboard, CSV data is used.</remarks>
        [CanBeNull]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static IList<IList<string>> GetClipboardDataAsTable()
        {
            Contract.Ensures((Contract.Result<IList<IList<string>>>() == null) || (Contract.Result<IList<IList<string>>>().Count > 0));

            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
                return ParseTable(text, TextColumnSeparator);

            var csv = Clipboard.GetData(DataFormats.CommaSeparatedValue) as string;
            if (!string.IsNullOrEmpty(csv))
                return ParseTable(csv, CsvColumnSeparator);

            return null;
        }

        /// <summary>
        /// Sets the clipboard data for the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <remarks>
        /// This method sets the TEXT (tab delimited) and CSV data. Like in Excel the CSV delimiter is either comma or semicolon, depending on the current culture.
        /// </remarks>
        public static void SetClipboardData([CanBeNull] this IList<IList<string>> table)
        {
            if (table == null)
            {
                Clipboard.Clear();
                return;
            }

            var textString = table.ToTextString();
            var csvString = table.ToCsvString();

            var dataObject = new DataObject();

            dataObject.SetText(textString);
            dataObject.SetText(csvString, TextDataFormat.CommaSeparatedValue);

            Clipboard.SetDataObject(dataObject);
        }

        [NotNull]
        internal static string ToTextString([NotNull] this IList<IList<string>> table)
        {
            Contract.Requires(table != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return ToString(table, TextColumnSeparator);
        }

        [NotNull]
        internal static string ToCsvString([NotNull] this IList<IList<string>> table)
        {
            Contract.Requires(table != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return ToString(table, CsvColumnSeparator);
        }

        [NotNull]
        internal static string ToString([NotNull] this IList<IList<string>> table, char separator)
        {
            Contract.Requires(table != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if ((table.Count == 1) && (table[0] != null) && (table[0].Count == 1) && string.IsNullOrWhiteSpace(table[0][0]))
                return Quote + (table[0][0] ?? string.Empty) + Quote;

            return string.Join(Environment.NewLine, table.Select(line => string.Join(separator.ToString(), line?.Select(cell => Quoted(cell, separator)) ?? Enumerable.Empty<string>())));
        }

        [NotNull]
        internal static string Quoted([CanBeNull] string value, char separator)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Any(IsLineFeed) || value.Contains(separator) || value.StartsWith(Quote, StringComparison.Ordinal))
            {
                return Quote + value.Replace(Quote, Quote + Quote) + Quote;
            }

            return value;
        }

        [CanBeNull]
        internal static IList<IList<string>> ParseTable([NotNull] string text, char separator)
        {
            Contract.Requires(text != null);
            Contract.Ensures((Contract.Result<IList<IList<string>>>() == null) || Contract.Result<IList<IList<string>>>().Count > 0);

            var table = new List<IList<string>>();

            using (var reader = new StringReader(text))
            {
                while (reader.Peek() != -1)
                {
                    table.Add(ReadTableLine(reader, separator));
                }
            }

            if (!table.Any())
                return null;

            var headerColumns = table.First();

            return table.Any(columns => columns?.Count != headerColumns?.Count) ? null : table;
        }

        [NotNull]
        internal static IList<string> ReadTableLine([NotNull] TextReader reader, char separator)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<IList<string>>() != null);
            // TODO: crashes CC 1.10.10126.2-rc1 Contract.Ensures(Contract.ForAll(Contract.Result<IList<string>>(), item => item != null));

            var columns = new List<string>();

            while (true)
            {
                columns.Add(ReadTableColumn(reader, separator));

                if ((char)reader.Peek() == separator)
                {
                    reader.Read();
                    continue;
                }

                while (IsLineFeed(reader.Peek()))
                {
                    reader.Read();
                }

                break;
            }

            Contract.Assume(Contract.ForAll(columns, item => item != null));
            return columns;
        }

        [NotNull]
        internal static string ReadTableColumn([NotNull] TextReader reader, char separator)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var stringBuilder = new StringBuilder();
            int nextChar;

            if (IsDoubleQuote(reader.Peek()))
            {
                reader.Read();

                while ((nextChar = reader.Read()) != -1)
                {
                    if (IsDoubleQuote(nextChar))
                    {
                        if (IsDoubleQuote(reader.Peek()))
                        {
                            reader.Read();
                            stringBuilder.Append((char)nextChar);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        stringBuilder.Append((char)nextChar);
                    }
                }
            }
            else
            {
                while ((nextChar = reader.Peek()) != -1)
                {
                    if (IsLineFeed(nextChar) || (nextChar == separator))
                        break;

                    reader.Read();
                    stringBuilder.Append((char)nextChar);
                }
            }

            return stringBuilder.ToString();
        }

        private static bool IsDoubleQuote(int c)
        {
            return (c == '"');
        }

        private static bool IsLineFeed(int c)
        {
            return (c == '\r') || (c == '\n');
        }

        private static bool IsLineFeed(char c)
        {
            return IsLineFeed((int)c);
        }
    }
}
