// <copyright file="DiffCollection.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    using System.Collections.Generic;
    using System.Linq;

    public class DiffCollection<T> : List<DiffResult<T>>
    {
        public int AddedCount
        {
            get
            {
                int added = 0;
                foreach (DiffResult<T> obj in this)
                {
                    if (obj.Operation.IsAdded)
                    {
                        added++;
                    }
                }

                return added;
            }
        }

        public int RemovedCount
        {
            get
            {
                int removed = 0;
                foreach (DiffResult<T> obj in this)
                {
                    if (obj.Operation.IsRemoved)
                    {
                        removed++;
                    }
                }

                return removed;
            }
        }

        public IEnumerable<DiffResult<T>> Added
        {
            get
            {
                foreach (DiffResult<T> obj in this)
                {
                    if (obj.Operation.IsAdded)
                    {
                        yield return obj;
                    }
                }
            }
        }

        public IEnumerable<DiffResult<T>> Removed
        {
            get
            {
                foreach (DiffResult<T> obj in this)
                {
                    if (obj.Operation.IsRemoved)
                    {
                        yield return obj;
                    }
                }
            }
        }

        public List<T> RemovedList
        {
            get
            {
                return (from type in this.Removed select type.ObjectV1).ToList();
            }
        }

        public List<T> AddedList
        {
            get
            {
                return (from type in this.Added select type.ObjectV1).ToList();
            }
        }
    }
}