// <copyright file="FieldPrintOptions.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System;

    [Flags]
    public enum FieldPrintOptions
    {
        Visibility = 1,

        Modifiers = 2,

        SimpleType = 4,

        Value = 8,

        All = Visibility | Modifiers | SimpleType | Value,
    }
}