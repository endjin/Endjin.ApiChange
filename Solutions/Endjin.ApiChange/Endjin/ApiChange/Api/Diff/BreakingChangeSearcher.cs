// <copyright file="BreakingChangeSearcher.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Endjin.ApiChange.Api.Introspection;
    using Endjin.ApiChange.Api.Query.UsageQueries;
    using Mono.Cecil;

    internal class BreakingChangeSearcher
    {
        private readonly UsageQueryAggregator myAggregator;

        public BreakingChangeSearcher(List<AssemblyDiffCollection> diffs, UsageQueryAggregator aggregator)
        {
            if (diffs == null)
            {
                throw new ArgumentNullException(nameof(diffs));
            }

            this.myAggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));

            foreach (AssemblyDiffCollection diff in diffs)
            {
                this.CreateUsageQueriesFromAssemblyDiff(diff);
            }
        }

        internal void CreateUsageQueriesFromAssemblyDiff(AssemblyDiffCollection diff)
        {
            List<TypeDefinition> removedTypes = diff.AddedRemovedTypes.RemovedList;

            if (removedTypes.Count > 0)
            {
                // removed types
                new WhoUsesType(this.myAggregator, removedTypes);
            }

            // changed types
            foreach (TypeDiff changedType in diff.ChangedTypes)
            {
                if (changedType.TypeV1.IsInterface)
                {
                    this.AddIntefaceChange(changedType);
                }
                else if (changedType.TypeV1.IsEnum)
                {
                    List<FieldDefinition> removedEnumConstants = changedType.Fields.RemovedList;
                    if (removedEnumConstants.Count > 0)
                    {
                        new WhoUsesType(this.myAggregator, changedType.TypeV1);
                    }
                }
                else
                {
                    List<EventDefinition> removedEvents = changedType.Events.RemovedList;
                    if (removedEvents.Count > 0)
                    {
                        new WhoUsesEvents(this.myAggregator, removedEvents);
                    }

                    List<MethodDefinition> removedMethods = changedType.Methods.RemovedList;
                    if (removedMethods.Count > 0)
                    {
                        new WhoUsesMethod(this.myAggregator, removedMethods);
                    }

                    this.CheckFieldChanges(changedType);

                    if (changedType.Interfaces.RemovedCount > 0)
                    {
                        new WhoUsesType(this.myAggregator, changedType.TypeV1);
                    }

                    if (changedType.HasChangedBaseType)
                    {
                        new WhoUsesType(this.myAggregator, changedType.TypeV1);
                    }
                }
            }
        }

        private void CheckFieldChanges(TypeDiff changedType)
        {
            List<FieldDefinition> removedFields = changedType.Fields.RemovedList;
            if (removedFields.Count > 0)
            {
                var nonConstFields = (from field in removedFields where !field.HasConstant select field).ToList();

                new WhoAccessesField(this.myAggregator, nonConstFields);

                foreach (DiffResult<FieldDefinition> field in changedType.Fields.Removed)
                {
                    if (field.ObjectV1.HasConstant)
                    {
                        Console.WriteLine(
                            "Warning: Constants are not referenced by other assemblies but copied by value: field {0} declaring type {1} in assembly {2}",
                            field.ObjectV1.Print(FieldPrintOptions.All),
                            field.ObjectV1.DeclaringType.Name,
                            field.ObjectV1.DeclaringType.Module.Assembly.Name.Name);
                        new WhoUsesType(this.myAggregator, changedType.TypeV1);
                    }
                }
            }
        }

        private void AddIntefaceChange(TypeDiff changedType)
        {
            // A changed interface breaks all its implementers
            new WhoImplementsInterface(this.myAggregator, changedType.TypeV1);

            // If methods are removed we must also search for all users of this
            // interface method
            List<MethodDefinition> removedInterfaceMethods = changedType.Methods.RemovedList;
            if (removedInterfaceMethods.Count > 0)
            {
                new WhoUsesMethod(this.myAggregator, removedInterfaceMethods);
            }

            List<EventDefinition> removedInterfaceEvents = changedType.Events.RemovedList;
            if (removedInterfaceEvents.Count > 0)
            {
                new WhoUsesEvents(this.myAggregator, removedInterfaceEvents);
            }
        }
    }
}