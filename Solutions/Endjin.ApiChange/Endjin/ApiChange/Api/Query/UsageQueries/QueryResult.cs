// <copyright file="QueryResult.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;

    public class QueryResult<T> where T : MemberReference
    {
        private MatchContext myAnnotations;

        public QueryResult(T match, string fileName, int lineNumber)
        {
            this.Match = match ?? throw new ArgumentNullException(nameof(match));
            this.SourceFileName = fileName;
            this.LineNumber = lineNumber;
        }

        public QueryResult(T match, string fileName, int lineNumber, MatchContext context)
            : this(match, fileName,
            lineNumber)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (KeyValuePair<string, object> kvp in context)
            {
                this.Annotations[kvp.Key] = kvp.Value;
            }
        }

        public T Match { get; }

        public string SourceFileName { get; }

        public int LineNumber { get; }

        public MatchContext Annotations
        {
            get
            {
                if (this.myAnnotations == null)
                {
                    this.myAnnotations = new MatchContext();
                }

                return this.myAnnotations;
            }
        }
    }
}