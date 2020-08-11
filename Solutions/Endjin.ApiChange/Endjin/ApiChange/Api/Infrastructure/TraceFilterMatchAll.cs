// <copyright file="TraceFilterMatchAll.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    internal class TraceFilterMatchAll : TraceFilter
    {
        public override bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            return true;
        }
    }
}