﻿namespace TomsToolbox.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using JetBrains.Annotations;

    /// <summary>
    /// Some emulations of .NetStandard methods
    /// </summary>
    public static class NetStandardExtensions
    {
        /// <summary>
        /// Gets the method information from a delegate.
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>The <see cref="MethodInfo"/></returns>
        [NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "CA seems to be wrong about this!")]
#if !NETSTANDARD1_0
        public static MethodInfo GetMethodInfo([NotNull] this Delegate @delegate)
        {

            // ReSharper disable once AssignNullToNotNullAttribute
            return @delegate.Method;
        }
#else
        // stub to generate the same external annotations, should not be used at all
        public static MethodInfo GetMethodInfo([NotNull] Delegate @delegate)
        {
            throw new NotImplementedException();
        }
#endif

        /// <summary>
        /// Gets the type information for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The specified type.</returns>
        [NotNull]
#if !NETSTANDARD1_0
        public static Type GetTypeInfo([NotNull] this Type type)
        {

            return type;
        }
#else
        // stub to generate the same external annotations, should not be used at all
        public static Type GetTypeInfo([NotNull] Type type)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
