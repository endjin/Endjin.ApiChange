// <copyright file="MethodComparer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    internal class MethodComparer : IEqualityComparer<MethodDefinition>
    {
        public bool Equals(MethodDefinition x, MethodDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(MethodDefinition obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}