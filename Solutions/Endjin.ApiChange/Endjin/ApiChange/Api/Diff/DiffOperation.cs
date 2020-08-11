// <copyright file="DiffOperation.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    public class DiffOperation
    {
        public DiffOperation(bool isAdded)
        {
            this.IsAdded = isAdded;
        }

        public bool IsAdded { get; }

        public bool IsRemoved
        {
            get
            {
                return !this.IsAdded;
            }
        }
    }
}