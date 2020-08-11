// <copyright file="AssemblyDiffCollection.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Mono.Cecil;

    [DebuggerDisplay("Add {AddedRemovedTypes.AddedCount} Remove {AddedRemovedTypes.RemovedCount} Changed {ChangedTypes.Count}")]
    public class AssemblyDiffCollection
    {
        public AssemblyDiffCollection()
        {
            this.AddedRemovedTypes = new DiffCollection<TypeDefinition>();
            this.ChangedTypes = new List<TypeDiff>();
        }

        public DiffCollection<TypeDefinition> AddedRemovedTypes { get; set; }

        public List<TypeDiff> ChangedTypes { get; set; }
    }
}