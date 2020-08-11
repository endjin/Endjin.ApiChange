// <copyright file="TypeQueryExtensions.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System.Collections.Generic;
    using Mono.Cecil;

    public static class TypeQueryExtensions
    {
        public static IEnumerable<TypeDefinition> GetMatchingTypes(
            this List<TypeQuery> list,
            AssemblyDefinition assembly)
        {
            foreach (TypeQuery query in list)
            {
                List<TypeDefinition> matchingTypes = query.GetTypes(assembly);
                foreach (TypeDefinition matchingType in matchingTypes)
                {
                    yield return matchingType;
                }
            }
        }
    }
}