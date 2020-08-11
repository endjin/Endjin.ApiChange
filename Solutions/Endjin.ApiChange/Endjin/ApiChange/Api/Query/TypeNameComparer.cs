// <copyright file="TypeNameComparer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal class TypeNameComparer : IEqualityComparer<TypeDefinition>
    {
        public bool Equals(TypeDefinition x, TypeDefinition y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeDefinition obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}