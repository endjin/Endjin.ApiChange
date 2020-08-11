// <copyright file="PdbInformationReader.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Introspection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Endjin.ApiChange.Api.Infrastructure;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Pdb;

    public class PdbInformationReader : IDisposable
    {
        private static readonly TypeHashes MyType = new TypeHashes(typeof(PdbInformationReader));
        private readonly PdbDownLoader myDownLoader = new PdbDownLoader();
        private readonly HashSet<string> myFailedPdbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ISymbolReader> myFile2PdbMap = new Dictionary<string, ISymbolReader>(StringComparer.OrdinalIgnoreCase);
        private readonly PdbReaderProvider myPdbFactory = new PdbReaderProvider();
        private readonly string mySymbolServer;

        public PdbInformationReader()
        {
        }

        public PdbInformationReader(string symbolServer)
        {
            this.mySymbolServer = symbolServer;
        }

        public void Dispose()
        {
            // release pdbs
            foreach (ISymbolReader reader in this.myFile2PdbMap.Values)
            {
                reader.Dispose();
            }

            this.myFile2PdbMap.Clear();
        }

        public void ReleasePdbForModule(ModuleDefinition module)
        {
            string fileName = module.Assembly.MainModule.FileName;
            ISymbolReader reader;

            if (this.myFile2PdbMap.TryGetValue(fileName, out reader))
            {
                reader.Dispose();
                this.myFile2PdbMap.Remove(fileName);
            }
        }

        public ISymbolReader LoadPdbForModule(ModuleDefinition module)
        {
            using (var t = new Tracer(MyType, "LoadPdbForModule"))
            {
                string fileName = module.Assembly.MainModule.FileName;
                t.Info("Module file name: {0}", fileName);
                ISymbolReader reader = null;

                if (!this.myFile2PdbMap.TryGetValue(fileName, out reader))
                {
                    if (this.myFailedPdbs.Contains(fileName))
                    {
                        t.Warning("This pdb could not be successfully downloaded");
                        return reader;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            reader = this.myPdbFactory.GetSymbolReader(module, fileName);
                            this.myFile2PdbMap[fileName] = reader;
                            break;
                        }
                        catch (Exception ex)
                        {
                            t.Error(Level.L3, ex, "Pdb did not match or it is not present");

                            string pdbFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".pdb");

                            try
                            {
                                File.Delete(pdbFileName);
                            }
                            catch (Exception delex)
                            {
                                t.Error(Level.L2, delex, "Could not delete pdb {0}", pdbFileName);
                            }

                            // When we have symbol server we try to make us of it for matches.
                            if (string.IsNullOrEmpty(this.mySymbolServer))
                            {
                                break;
                            }

                            t.Info("Try to download pdb from symbol server {0}", this.mySymbolServer);
                            bool bDownloaded = this.myDownLoader.DownloadPdbs(new FileQuery(fileName), this.mySymbolServer);
                            t.Info("Did download pdb {0} from symbol server with return code: {1}", fileName, bDownloaded);

                            if (bDownloaded == false || i == 1) // second try did not work out as well
                            {
                                this.myFailedPdbs.Add(fileName);
                                break;
                            }
                        }
                    }
                }

                return reader;
            }
        }

        /// <summary>
        /// Try to get the file name where the type is defined from the pdb via walking
        /// through some methods.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public KeyValuePair<string, int> GetFileLine(TypeDefinition type)
        {
            var fileLine = new KeyValuePair<string, int>(string.Empty, 0);

            for (int i = 0; i < type.Methods.Count; i++)
            {
                fileLine = this.GetFileLine(type.Methods[i].Body);
                if (!string.IsNullOrEmpty(fileLine.Key))
                {
                    break;
                }
            }

            return fileLine;
        }

        public KeyValuePair<string, int> GetFileLine(MethodDefinition method)
        {
            return this.GetFileLine(method.Body);
        }

        public KeyValuePair<string, int> GetFileLine(MethodBody body)
        {
            if (body != null)
            {
                ISymbolReader symbolReader = this.LoadPdbForModule(body.Method.DeclaringType.Module);

                if (symbolReader != null)
                {
                    foreach (Instruction ins in body.Instructions)
                    {
                        SequencePoint seqPoint = body.Method.DebugInformation.GetSequencePoint(ins);

                        if (seqPoint != null)
                        {
                            return new KeyValuePair<string, int>(
                                this.PatchDriveLetter(seqPoint.Document.Url),
                                0);
                        }
                    }
                }
            }

            return new KeyValuePair<string, int>(string.Empty, 0);
        }

        private bool HasValidFileAndLineNumber(MethodDebugInformation debugInformation, Instruction ins)
        {
            bool lret = true;
            if (ins == null)
            {
                lret = false;
            }

            SequencePoint seqPoint = debugInformation.GetSequencePoint(ins);

            if (lret)
            {
                if (seqPoint == null)
                {
                    lret = false;
                }
            }

            if (lret)
            {
                if (seqPoint.StartLine == 0xfeefee)
                {
                    lret = false;
                }
            }

            return lret;
        }

        private Instruction GetILInstructionWithLineNumber(MethodDebugInformation debugInformation, Instruction ins, bool bSearchForward)
        {
            Instruction current = ins;
            if (bSearchForward)
            {
                while (current != null && !this.HasValidFileAndLineNumber(debugInformation, current))
                {
                    current = current.Next;
                }
            }
            else
            {
                while (current != null && !this.HasValidFileAndLineNumber(debugInformation, current))
                {
                    current = current.Previous;
                }
            }

            return current;
        }

        /// <summary>
        /// Get for a specific IL instruction the matching file and line.
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="method"></param>
        /// <param name="bSearchForward">
        /// Search the next il instruction first if set to true for the line number from the pdb. If
        /// nothing is found we search backward.
        /// </param>
        /// <returns></returns>
        public KeyValuePair<string, int> GetFileLine(Instruction ins, MethodDefinition method, bool bSearchForward)
        {
            using (var t = new Tracer(MyType, "GetFileLine"))
            {
                t.Info("Try to get file and line info for {0} {1} forwardSearch {2}", method.DeclaringType.FullName, method.Name, bSearchForward);

                ISymbolReader symReader = this.LoadPdbForModule(method.DeclaringType.Module);

                if (symReader != null && method.Body != null)
                {
                    Instruction current = ins;

                    if (bSearchForward)
                    {
                        current = this.GetILInstructionWithLineNumber(method.DebugInformation, ins, true);

                        if (current == null)
                        {
                            current = this.GetILInstructionWithLineNumber(method.DebugInformation, ins, false);
                        }
                    }
                    else
                    {
                        current = this.GetILInstructionWithLineNumber(method.DebugInformation, ins, false);

                        if (current == null)
                        {
                            current = this.GetILInstructionWithLineNumber(method.DebugInformation, ins, true);
                        }
                    }

                    if (current != null)
                    {
                        SequencePoint seqPoint = method.Body.Method.DebugInformation.GetSequencePoint(current);
                        return new KeyValuePair<string, int>(this.PatchDriveLetter(seqPoint.Document.Url), seqPoint.StartLine);
                    }
                }
                else
                {
                    t.Info("No symbol reader present or method has no body");
                }

                return new KeyValuePair<string, int>(string.Empty, 0);
            }
        }

        private string PatchDriveLetter(string url)
        {
            string root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            var sb = new StringBuilder(url);
            sb[0] = root[0];

            return sb.ToString();
        }
    }
}