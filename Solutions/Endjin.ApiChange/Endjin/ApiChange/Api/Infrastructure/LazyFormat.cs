﻿// <copyright file="LazyFormat.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;

    /// <summary>
    ///     Support lazy formatting of strings. This is useful to delay the cration of
    ///     trace messages until the trace is enabled.
    /// </summary>
    public class LazyFormat
    {
        private readonly Func<string> myFormatMethod;

        /// <summary>
        ///     Supply the method which will generate a string with captured variables of your enclosing method.
        /// </summary>
        /// <param name="formatMethod"></param>
        public LazyFormat(Func<string> formatMethod)
        {
            if (formatMethod == null)
            {
                throw new ArgumentNullException("formatMethod");
            }

            this.myFormatMethod = formatMethod;
        }

        /// <summary>
        ///     Return the string generated by the formatMethod.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return this.myFormatMethod();
        }
    }
}