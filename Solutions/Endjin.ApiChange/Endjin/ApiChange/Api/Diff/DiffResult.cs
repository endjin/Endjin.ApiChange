// <copyright file="DiffResult.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    public class DiffResult<T>
    {
        public DiffResult(T v1, DiffOperation diffType)
        {
            this.ObjectV1 = v1;
            this.Operation = diffType;
        }

        public DiffOperation Operation { get; }

        public T ObjectV1 { get; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.ObjectV1, this.Operation.IsAdded ? "added" : "removed");
        }
    }
}