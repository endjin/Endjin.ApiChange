// <copyright file="WhoInstantiatesType.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Linq;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class WhoInstantiatesType : UsageVisitor
    {
        private const string ConstructorCalledContext = "Constructor called";

        private readonly TypeDefinition myType;

        public WhoInstantiatesType(UsageQueryAggregator aggregator, TypeDefinition type)
            : base(aggregator)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type was null");
            }

            this.myType = type;
            this.Aggregator.AddVisitScope(type.Module.Assembly.Name.Name);
        }

        public override void VisitMethodBody(MethodBody body)
        {
            var methodDefinitions = this.myType.Methods.Where(x => x.IsConstructor).ToList();
            foreach (Instruction instr in body.Instructions)
            {
                if (instr.OpCode.Code == Code.Newobj)
                {
                    foreach (MethodReference method in methodDefinitions)
                    {
                        if (method.IsEqual((MethodReference)instr.Operand, false))
                        {
                            var context = new MatchContext(ConstructorCalledContext, this.myType.Print());
                            this.Aggregator.AddMatch(instr, body.Method, true, context);
                        }
                    }
                }
                else if (instr.OpCode.Code == Code.Initobj)
                {
                    var typeRef = (TypeReference)instr.Operand;
                    if (typeRef.IsEqual(this.myType))
                    {
                        var context = new MatchContext("Struct Constructor() called", this.myType.Print());
                        this.Aggregator.AddMatch(instr, body.Method, true, context);
                    }
                }
                else if (instr.OpCode.Code == Code.Call)
                {
                    // Value types are instantiated by directly calling the corresponding ctor
                    var methodRef = (MethodReference)instr.Operand;
                    if (methodRef.Name == ".ctor")
                    {
                        if (methodRef.DeclaringType.IsEqual(this.myType))
                        {
                            var context = new MatchContext(ConstructorCalledContext, this.myType.Print());
                            this.Aggregator.AddMatch(instr, body.Method, false, context);
                        }
                    }
                }
            }
        }
    }
}