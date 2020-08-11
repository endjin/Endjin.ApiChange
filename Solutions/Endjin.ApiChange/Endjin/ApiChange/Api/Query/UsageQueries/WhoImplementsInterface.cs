// <copyright file="WhoImplementsInterface.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    internal class WhoImplementsInterface : UsageVisitor
    {
        private readonly Dictionary<string, TypeDefinition> myInterfaceNames = new Dictionary<string, TypeDefinition>();

        public WhoImplementsInterface(UsageQueryAggregator aggregator, TypeDefinition itf)
            : this(
            aggregator,
            new List<TypeDefinition>
            {
                ThrowIfNull("itf", itf),
            })
        {
        }

        public WhoImplementsInterface(UsageQueryAggregator aggreator, List<TypeDefinition> interfaces)
            : base(aggreator)
        {
            if (interfaces == null || interfaces.Count == 0)
            {
                throw new ArgumentException("The interfaces collection was null.");
            }

            foreach (TypeDefinition type in interfaces)
            {
                if (!type.IsInterface)
                {
                    throw new ArgumentException(string.Format("The type {0} is not an interface", type.Print()));
                }

                this.Aggregator.AddVisitScope(type.Module.Assembly.Name.Name);
                if (!this.myInterfaceNames.ContainsKey(type.Name))
                {
                    this.myInterfaceNames.Add(type.Name, type);
                }
            }
        }

        private bool IsMatchingInterface(TypeReference itf, out TypeDefinition searchItf)
        {
            if (this.myInterfaceNames.TryGetValue(itf.Name, out searchItf))
            {
                if (itf.IsEqual(searchItf, false))
                {
                    return true;
                }
            }

            return false;
        }

        public override void VisitType(TypeDefinition type)
        {
            if (type.Interfaces == null)
            {
                return;
            }

            TypeDefinition searchItf = null;
            foreach (InterfaceImplementation itf in type.Interfaces)
            {
                if (this.IsMatchingInterface(itf.InterfaceType, out searchItf))
                {
                    var context = new MatchContext();
                    context[MatchContext.MatchReason] = "Implements interface";
                    context[MatchContext.MatchItem] = searchItf.Print();
                    this.Aggregator.AddMatch(type, context);
                }
            }
        }
    }
}