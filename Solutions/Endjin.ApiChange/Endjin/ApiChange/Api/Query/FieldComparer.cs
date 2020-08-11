// <copyright file="FieldComparer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    internal class FieldComparer : IEqualityComparer<FieldDefinition>
    {
        public bool Equals(FieldDefinition x, FieldDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(FieldDefinition obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}