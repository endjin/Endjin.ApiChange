// <copyright file="WhoUsesMethod.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Infrastructure;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class WhoUsesMethod : UsageVisitor
    {
        private const MethodPrintOption myMethodFormat = MethodPrintOption.ReturnType | MethodPrintOption.ShortNames | MethodPrintOption.Parameters;

        private static TypeHashes myType = new TypeHashes(typeof(WhoUsesMethod));

        // Do fast compares with Hashsets instead of string comparisons
        private readonly Dictionary<string, List<MethodDefinition>> myMethodNames = new Dictionary<string, List<MethodDefinition>>();

        public WhoUsesMethod(UsageQueryAggregator aggregator, List<MethodDefinition> methods)
            : base(aggregator)
        {
            if (methods == null)
            {
                throw new ArgumentNullException("The method list to query for was null.");
            }

            foreach (MethodDefinition method in methods)
            {
                this.Aggregator.AddVisitScope(method.DeclaringType.Module.Assembly.Name.Name);
                List<MethodDefinition> typeMethods = null;

                if (!this.myMethodNames.TryGetValue(method.DeclaringType.Name, out typeMethods))
                {
                    typeMethods = new List<MethodDefinition>();
                    this.myMethodNames[method.DeclaringType.Name] = typeMethods;
                }

                this.myMethodNames[method.DeclaringType.Name].Add(method);
            }
        }

        private bool IsMatchingMethod(MethodReference methodReference, out MethodDefinition matchingMethod)
        {
            bool lret = false;
            matchingMethod = null;

            string declaringType = string.Empty;

            if (methodReference != null && methodReference.DeclaringType != null &&
                methodReference.DeclaringType.GetElementType() != null)
            {
                declaringType = methodReference.DeclaringType.GetElementType().Name;
            }

            List<MethodDefinition> typeMethods = null;
            if (this.myMethodNames.TryGetValue(declaringType, out typeMethods))
            {
                foreach (MethodDefinition searchMethod in typeMethods)
                {
                    if (methodReference.IsEqual(searchMethod, false))
                    {
                        lret = true;
                        matchingMethod = searchMethod;
                        break;
                    }
                }
            }

            return lret;
        }

        public override void VisitMethod(MethodDefinition method)
        {
            if (method.Body == null)
            {
                return;
            }

            MethodDefinition matchingMethod = null;

            foreach (Instruction ins in method.Body.Instructions)
            {
                if (Code.Callvirt == ins.OpCode.Code) // normal instance call
                {
                    if (this.IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        var context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Call == ins.OpCode.Code) // static function call
                {
                    if (this.IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        var context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Ldftn == ins.OpCode.Code) // Load Function Pointer for delegate call
                {
                    if (this.IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        var context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }
            }
        }
    }
}