// <copyright file="ISymChkExecutor.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System.Collections.Generic;

    internal interface ISymChkExecutor
    {
        List<string> FailedPdbs { get; set; }

        int SucceededPdbCount { get; }

        bool DownLoadPdb(string fullBinaryName, string symbolServer, string downloadDir);
    }
}