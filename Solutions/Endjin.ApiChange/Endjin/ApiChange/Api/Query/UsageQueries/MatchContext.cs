// <copyright file="MatchContext.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Query.UsageQueries
{
    using System.Collections.Generic;

    public class MatchContext : Dictionary<string, object>
    {
        public const string MatchReason = "Match Reason";

        public const string MatchItem = "Match Item";

        public MatchContext(string matchReason, string matchItem)
        {
            this[MatchReason] = matchReason;
            this[MatchItem] = matchItem;
        }

        public MatchContext()
        {
        }

        public string Reason
        {
            get
            {
                object lret = string.Empty;
                if (!this.TryGetValue(MatchReason, out lret))
                {
                    lret = string.Empty;
                }

                return lret.ToString();
            }
        }

        public string Item
        {
            get
            {
                object lret;
                if (!this.TryGetValue(MatchItem, out lret))
                {
                    lret = string.Empty;
                }

                return lret.ToString();
            }
        }
    }
}