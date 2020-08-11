// <copyright file="EventQuery.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using Mono.Cecil;

    public class EventQuery : MethodQuery
    {
        public EventQuery()
            : this("*")
        {
        }

        public EventQuery(string query)
            : base("*")
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query string was empty");
            }

            if (query == "*")
            {
                return;
            }

            // Get cached regex
            this.Parser = EventQueryParser;

            Match match = this.Parser.Match(query);
            if (!match.Success)
            {
                throw new ArgumentException(string.Format("The event query string {0} was not a valid query.", query));
            }

            this.SetModifierFilter(match);
            this.EventTypeFilter = GenericTypeMapper.ConvertClrTypeNames(this.Value(match, "eventType"));
            this.EventTypeFilter = this.PrependStarBeforeGenericTypes(this.EventTypeFilter);

            if (!this.EventTypeFilter.StartsWith("*"))
            {
                this.EventTypeFilter = "*" + this.EventTypeFilter;
            }

            if (this.EventTypeFilter == "*")
            {
                this.EventTypeFilter = null;
            }

            this.NameFilter = this.Value(match, "eventName");
        }

        private string EventTypeFilter { get; }

        public static EventQuery PublicEvents { get; } = new EventQuery("public * *");

        public static EventQuery ProtectedEvents { get; } = new EventQuery("protected * *");

        public static EventQuery InternalEvents { get; } = new EventQuery("internal * *");

        public static EventQuery AllEvents { get; } = new EventQuery("* *");

        private string PrependStarBeforeGenericTypes(string eventTypeFilter)
        {
            return eventTypeFilter.Replace("<", "<*").Replace("**", "*");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override List<MethodDefinition> GetMethods(TypeDefinition type)
        {
            throw new NotSupportedException(
                "The event query does not support a method match. Use GetMatchingEvents to get the query result");
        }

        public List<EventDefinition> GetMatchingEvents(TypeDefinition type)
        {
            if (type == null)
            {
                throw new ArgumentException("The type instance to analyze was null. Can`t do that.");
            }

            var events = new List<EventDefinition>();

            foreach (EventDefinition ev in type.Events)
            {
                if (this.IsMatchingEvent(ev))
                {
                    events.Add(ev);
                }
            }

            return events;
        }

        private bool IsMatchingEvent(EventDefinition ev)
        {
            bool lret = true;

            lret = this.MatchMethodModifiers(ev.AddMethod);
            if (lret)
            {
                lret = this.MatchName(ev.Name);
            }

            if (lret)
            {
                lret = this.MatchEventType(ev.EventType);
            }

            return lret;
        }

        private bool MatchEventType(TypeReference evType)
        {
            if (string.IsNullOrEmpty(this.EventTypeFilter) || this.EventTypeFilter == "*")
            {
                return true;
            }

            return Matcher.MatchWithWildcards(this.EventTypeFilter, evType.FullName, StringComparison.OrdinalIgnoreCase);
        }
    }
}