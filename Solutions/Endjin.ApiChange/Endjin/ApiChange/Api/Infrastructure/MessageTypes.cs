// <copyright file="MessageTypes.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;

    /// <summary>
    ///     Severity of trace messages.
    /// </summary>
    [Flags]
    public enum MessageTypes
    {
        None = 0,

        Info = 1,

        Instrument = 2,

        Warning = 4,

        Error = 8,

        InOut = 16,

        Exception = 32,

        All = InOut | Info | Instrument | Warning | Error | Exception,
    }
}