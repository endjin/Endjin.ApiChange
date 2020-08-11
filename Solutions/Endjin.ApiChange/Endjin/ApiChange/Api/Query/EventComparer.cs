// <copyright file="EventComparer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    internal class EventComparer : IEqualityComparer<EventDefinition>
    {
        public bool Equals(EventDefinition x, EventDefinition y)
        {
            return x.AddMethod.IsEqual(y.AddMethod);
        }

        public int GetHashCode(EventDefinition obj)
        {
            return obj.AddMethod.Name.GetHashCode();
        }
    }
}