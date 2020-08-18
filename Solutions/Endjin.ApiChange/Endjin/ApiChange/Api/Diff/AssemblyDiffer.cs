// <copyright file="AssemblyDiffer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    using System;
    using System.Collections.Generic;
    using Introspection;
    using Query;
    using Mono.Cecil;

    public class AssemblyDiffer : IDisposable
    {
        private readonly AssemblyDiffCollection myDiff = new AssemblyDiffCollection();

        private readonly AssemblyDefinition myV1;

        private readonly AssemblyDefinition myV2;

        private readonly bool ownAssemblyDefinitions;

        /// <summary>
        /// Creates an <see cref="AssemblyDiffer" /> from existing <see cref="AssemblyDefinition"/>
        /// objects.
        /// </summary>
        /// <param name="v1">The assembly file v1.</param>
        /// <param name="v2">The assembly file v2.</param>
        /// <remarks>
        /// When you use this constructor, the resulting instance will NOT dispose either of the
        /// <see cref="AssemblyDefinition"/> objects. The assumption is that when the caller
        /// supplies these objects, the caller owns them. (If you use path-based overload,
        /// <see cref="AssemblyDiffer(string,string)"/>, this object will dispose the
        /// <see cref="AssemblyDefinition"/> objects because in that case, this object created them
        /// so only this object is able to dipose of them.)
        /// </remarks>
        public AssemblyDiffer(AssemblyDefinition v1, AssemblyDefinition v2)
        {
            this.myV1 = v1 ?? throw new ArgumentNullException(nameof(v1));
            this.myV2 = v2 ?? throw new ArgumentNullException(nameof(v2));

            ownAssemblyDefinitions = false;
        }

        /// <summary>
        /// Creates an <see cref="AssemblyDiffer" />.
        /// </summary>
        /// <param name="assemblyFileV1">The assembly file v1.</param>
        /// <param name="assemblyFileV2">The assembly file v2.</param>
        public AssemblyDiffer(string assemblyFileV1, string assemblyFileV2)
        {
            if (string.IsNullOrEmpty(assemblyFileV1))
            {
                throw new ArgumentNullException(nameof(assemblyFileV1));
            }

            if (string.IsNullOrEmpty(assemblyFileV2))
            {
                throw new ArgumentNullException(nameof(assemblyFileV2));
            }

            this.myV1 = AssemblyLoader.LoadCecilAssembly(assemblyFileV1);
            if (this.myV1 == null)
            {
                throw new ArgumentException($"Could not load assemblyV1 {assemblyFileV1}");
            }

            this.myV2 = AssemblyLoader.LoadCecilAssembly(assemblyFileV2);
            if (this.myV2 == null)
            {
                throw new ArgumentException($"Could not load assemblyV2 {assemblyFileV2}");
            }

            ownAssemblyDefinitions = true;
        }

        private void OnAddedType(TypeDefinition type)
        {
            var diff = new DiffResult<TypeDefinition>(type, new DiffOperation(true));
            this.myDiff.AddedRemovedTypes.Add(diff);
        }

        private void OnRemovedType(TypeDefinition type)
        {
            var diff = new DiffResult<TypeDefinition>(type, new DiffOperation(false));
            this.myDiff.AddedRemovedTypes.Add(diff);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.ownAssemblyDefinitions)
            {
                this.myV1.Dispose();
                this.myV2.Dispose();
            }
        }

        public AssemblyDiffCollection GenerateTypeDiff(QueryAggregator queries)
        {
            if (queries == null || queries.TypeQueries.Count == 0)
            {
                throw new ArgumentNullException("queries is null or contains no queries");
            }

            List<TypeDefinition> typesV1 = queries.ExeuteAndAggregateTypeQueries(this.myV1);
            List<TypeDefinition> typesV2 = queries.ExeuteAndAggregateTypeQueries(this.myV2);

            var differ = new ListDiffer<TypeDefinition>(this.ShallowTypeComapare);

            differ.Diff(typesV1, typesV2, this.OnAddedType, this.OnRemovedType);

            this.DiffTypes(typesV1, typesV2, queries);

            return this.myDiff;
        }

        private bool ShallowTypeComapare(TypeDefinition v1, TypeDefinition v2)
        {
            return v1.FullName == v2.FullName;
        }

        private void DiffTypes(List<TypeDefinition> typesV1, List<TypeDefinition> typesV2, QueryAggregator queries)
        {
            TypeDefinition typeV2;
            foreach (TypeDefinition typeV1 in typesV1)
            {
                typeV2 = this.GetTypeByDefinition(typeV1, typesV2);
                if (typeV2 != null)
                {
                    var diffed = TypeDiff.GenerateDiff(typeV1, typeV2, queries);
                    if (TypeDiff.None != diffed)
                    {
                        this.myDiff.ChangedTypes.Add(diffed);
                    }
                }
            }
        }

        private TypeDefinition GetTypeByDefinition(TypeDefinition search, List<TypeDefinition> types)
        {
            foreach (TypeDefinition type in types)
            {
                if (type.IsEqual(search))
                {
                    return type;
                }
            }

            return null;
        }
    }
}