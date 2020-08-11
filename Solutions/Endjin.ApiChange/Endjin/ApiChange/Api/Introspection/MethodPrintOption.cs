// <copyright file="MethodPrintOption.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System;

    [Flags]
    public enum MethodPrintOption
    {
        ShortNames = 1,

        Visiblity = 2,

        Modifier = 4,

        ReturnType = 8,

        Parameters = 16,

        ParamNames = 32,

        Full = ShortNames | Visiblity | Modifier | ReturnType | Parameters | ParamNames,
    }
}