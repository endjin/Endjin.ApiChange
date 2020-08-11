// <copyright file="FileNameComparer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class FileNameComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Compare(Path.GetFileName(x), Path.GetFileName(y), StringComparison.OrdinalIgnoreCase) == 0;
        }

        public int GetHashCode(string obj)
        {
            return Path.GetFileName(obj).ToLower().GetHashCode();
        }
    }
}