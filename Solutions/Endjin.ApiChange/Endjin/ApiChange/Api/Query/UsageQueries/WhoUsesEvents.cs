// <copyright file="WhoUsesEvents.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Collections.Generic;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class WhoUsesEvents : UsageVisitor
    {
        public const string DefiningAssemblyKey = "Assembly";

        public const string AddEventReason = "AddEvent";

        public const string RemoveEventReason = "RemoveEvent";

        private readonly HashSet<string> myEventNames = new HashSet<string>();

        private readonly List<EventDefinition> myEvents;

        public WhoUsesEvents(UsageQueryAggregator aggregator, EventDefinition ev)
            : this(
            aggregator,
            new List<EventDefinition>
            {
                ThrowIfNull("ev", ev),
            })
        {
        }

        public WhoUsesEvents(UsageQueryAggregator aggreagator, List<EventDefinition> events)
            : base(aggreagator)
        {
            if (events == null)
            {
                throw new ArgumentException("The events list was null.");
            }

            this.myEvents = events;

            foreach (EventDefinition ev in this.myEvents)
            {
                this.Aggregator.AddVisitScope(ev.AddMethod.DeclaringType.Module.Assembly.Name.Name);
                this.myEventNames.Add(ev.AddMethod.Name);
                this.myEventNames.Add(ev.RemoveMethod.Name);
            }
        }

        private string GetPrettyEventName(EventDefinition ev)
        {
            return string.Format("{0}.{1}", ev.DeclaringType.FullName, ev.Name);
        }

        private MatchContext DoesMatch(MethodReference method)
        {
            MatchContext context = null;

            if (!this.myEventNames.Contains(method.Name))
            {
                return context;
            }

            foreach (EventDefinition searchEvent in this.myEvents)
            {
                if (method.IsEqual(searchEvent.AddMethod, false))
                {
                    context = new MatchContext(AddEventReason, this.GetPrettyEventName(searchEvent));
                    context[DefiningAssemblyKey] = searchEvent.DeclaringType.Module.FileName;
                    break;
                }

                if (method.IsEqual(searchEvent.RemoveMethod, false))
                {
                    context = new MatchContext(RemoveEventReason, this.GetPrettyEventName(searchEvent));
                    context[DefiningAssemblyKey] = searchEvent.DeclaringType.Module.FileName;
                    break;
                }
            }

            return context;
        }

        public override void VisitMethod(MethodDefinition method)
        {
            if (method.Body == null)
            {
                return;
            }

            MatchContext context = null;
            foreach (Instruction ins in method.Body.Instructions)
            {
                if (Code.Callvirt == ins.OpCode.Code) // normal instance call
                {
                    context = this.DoesMatch((MethodReference)ins.Operand);
                    if (context != null)
                    {
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Call == ins.OpCode.Code) // static function call
                {
                    context = this.DoesMatch((MethodReference)ins.Operand);
                    if (context != null)
                    {
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Ldftn == ins.OpCode.Code) // Delegate assignment
                {
                    context = this.DoesMatch((MethodReference)ins.Operand);
                    if (context != null)
                    {
                        this.Aggregator.AddMatch(ins, method, false, context);
                    }
                }
            }
        }
    }
}