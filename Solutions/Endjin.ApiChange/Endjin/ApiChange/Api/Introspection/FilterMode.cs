// <copyright file="FilterMode.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System;

    [Flags]
    public enum FilterMode
    {
        Private = 1,

        Public = 2,

        Internal = 4,

        Protected = 8,

        NotInternalProtected = 16,

        All = Private | Public | Internal | Protected,
    }
}