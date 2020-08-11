// <copyright file="WhoReferencesAssembly.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.IO;
    using Mono.Cecil;

    public class WhoReferencesAssembly : UsageVisitor
    {
        private readonly string myAssembly;

        public WhoReferencesAssembly(UsageQueryAggregator aggregator, string fileName)
            : base(aggregator)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name was null or empty.");
            }

            this.myAssembly = Path.GetFileNameWithoutExtension(fileName);
            aggregator.AddVisitScope(fileName);
        }

        public override void VisitAssemblyReference(AssemblyNameReference assemblyRef, AssemblyDefinition current)
        {
            if (string.Compare(assemblyRef.Name, this.myAssembly, true) == 0)
            {
                this.Aggregator.AddMatch(current);
            }
        }
    }
}