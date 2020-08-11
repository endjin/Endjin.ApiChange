// <copyright file="SymChkExecutor.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using Endjin.ApiChange.Api.Infrastructure;

    internal class SymChkExecutor : ISymChkExecutor
    {
        private static readonly TypeHashes MyType = new TypeHashes(typeof(SymChkExecutor));

        internal static bool bCanStartSymChk = true;

        private static readonly Regex SymPassedFileCountParser = new Regex(@"SYMCHK: PASSED \+ IGNORED files = (?<succeeded>\d+) *", RegexOptions.IgnoreCase);

        private static readonly Regex SymFailedFileParser = new Regex(@"SYMCHK: (?<filename>.*?) +FAILED", RegexOptions.IgnoreCase);

        internal string SymChkExeName = "symchk.exe";

        public SymChkExecutor()
        {
            this.FailedPdbs = new List<string>();
        }

        public int SucceededPdbCount { get; private set; }

        public List<string> FailedPdbs { get; set; }

        public bool DownLoadPdb(string fullbinaryName, string symbolServer, string downloadDir)
        {
            using (var t = new Tracer(MyType, "DownLoadPdb"))
            {
                bool lret = bCanStartSymChk;

                if (lret)
                {
                    var startInfo = new ProcessStartInfo(
                        this.SymChkExeName,
                        this.BuildCmdLine(fullbinaryName, symbolServer, downloadDir));

                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    Process proc = null;

                    try
                    {
                        proc = Process.Start(startInfo);
                        proc.OutputDataReceived += this.proc_OutputDataReceived;
                        proc.ErrorDataReceived += this.proc_OutputDataReceived;
                        proc.BeginErrorReadLine();
                        proc.BeginOutputReadLine();

                        proc.WaitForExit();
                    }
                    catch (Win32Exception ex)
                    {
                        bCanStartSymChk = false;
                        t.Error(ex, "Could not start symchk.exe to download pdb files");
                        lret = false;
                    }
                    finally
                    {
                        if (proc != null)
                        {
                            proc.OutputDataReceived -= this.proc_OutputDataReceived;
                            proc.ErrorDataReceived -= this.proc_OutputDataReceived;
                            proc.Dispose();
                        }
                    }
                }

                if (this.FailedPdbs.Count > 0)
                {
                    lret = false;
                }

                return lret;
            }
        }

        internal string BuildCmdLine(string binaryFileName, string symbolServer, string downloadDir)
        {
            string lret = $"\"{binaryFileName}\" /su \"{symbolServer}\" /oc \"{downloadDir ?? Path.GetDirectoryName(binaryFileName)}\"";

            Tracer.Info(Level.L1, MyType, "BuildCmdLine", "Symcheck command is {0} {1}", this.SymChkExeName, lret);

            return lret;
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string line = e.Data;

                Match m = SymPassedFileCountParser.Match(line);

                if (m.Success)
                {
                    lock (this)
                    {
                        this.SucceededPdbCount += int.Parse(m.Groups["succeeded"].Value, CultureInfo.InvariantCulture);
                    }
                }

                m = SymFailedFileParser.Match(line);

                if (m.Success)
                {
                    lock (this)
                    {
                        this.FailedPdbs.Add(m.Groups["filename"].Value);
                    }
                }
            }
        }
    }
}