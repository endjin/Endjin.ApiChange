// <copyright file="WhoHasFieldOfType.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    public class WhoHasFieldOfType : UsageVisitor
    {
        private readonly HashSet<string> mySearchTypeNames = new HashSet<string>();

        private readonly List<TypeDefinition> mySearchTypes;

        public WhoHasFieldOfType(UsageQueryAggregator aggregator, TypeDefinition fieldType)
            : this(
            aggregator,
            new List<TypeDefinition>
            {
                ThrowIfNull("fieldType", fieldType),
            })
        {
        }

        public WhoHasFieldOfType(UsageQueryAggregator aggregator, List<TypeDefinition> fieldTypes)
            : base(aggregator)
        {
            if (fieldTypes == null)
            {
                throw new ArgumentNullException("fieldTypes");
            }

            this.mySearchTypes = fieldTypes;

            foreach (TypeDefinition fieldType in fieldTypes)
            {
                this.mySearchTypeNames.Add(fieldType.Name);
                this.Aggregator.AddVisitScope(fieldType.Module.Assembly.Name.Name);
            }
        }

        public override void VisitField(FieldDefinition field)
        {
            TypeDefinition matchingType = null;
            if (this.IsMatching(this.mySearchTypeNames, this.mySearchTypes, field.FieldType, out matchingType))
            {
                var context = new MatchContext("Has Field Of Type", matchingType.Print());
                this.Aggregator.AddMatch(field, context);
                return;
            }

            var genType = field.FieldType as GenericInstanceType;
            if (genType != null)
            {
                foreach (TypeReference generic in genType.GenericArguments)
                {
                    if (this.IsMatching(this.mySearchTypeNames, this.mySearchTypes, generic, out matchingType))
                    {
                        var context = new MatchContext("Has Field Of Type", matchingType.Print());
                        this.Aggregator.AddMatch(field, context);
                        return;
                    }
                }
            }
        }
    }
}