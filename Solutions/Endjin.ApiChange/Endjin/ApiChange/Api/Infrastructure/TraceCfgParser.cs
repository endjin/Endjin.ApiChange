// <copyright file="TraceCfgParser.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    internal class TraceCfgParser
    {
        private static readonly Dictionary<string, MessageTypes> MyFlagTranslator =
            new Dictionary<string, MessageTypes>(StringComparer.OrdinalIgnoreCase)
            {
                { "inout", MessageTypes.InOut },
                { "info", MessageTypes.Info },
                { "i", MessageTypes.Info },
                { "information", MessageTypes.Info },
                { "instrument", MessageTypes.Instrument },
                { "warning", MessageTypes.Warning },
                { "warn", MessageTypes.Warning },
                { "w", MessageTypes.Warning },
                { "error", MessageTypes.Error },
                { "e", MessageTypes.Error },
                { "exception", MessageTypes.Exception },
                { "ex", MessageTypes.Exception },
                { "all", MessageTypes.All },
                { "*", MessageTypes.All },
            };

        private static readonly Dictionary<string, Level> MyLevelTranslator =
            new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase)
            {
                { "l1", Level.L1 },
                { "l2", Level.L2 },
                { "l3", Level.L3 },
                { "l4", Level.L4 },
                { "l5", Level.L5 },
                { "ldispose", Level.Dispose },
                { "l*", Level.All },
                { "level1", Level.L1 },
                { "level2", Level.L2 },
                { "level3", Level.L3 },
                { "level4", Level.L4 },
                { "level5", Level.L5 },
                { "leveldispose", Level.Dispose },
                { "level*", Level.All },
            };

        private bool bHasError;

        public TraceFilter Filters;

        public TraceFilter NotFilters;

        public TraceListener OutDevice;

        public bool UseAppConfigListeners;

        /// <summary>
        ///     Format string is of the form
        ///     outDevice; type flag1+flag2+...;type flags; ...
        ///     where flags are a combination of trace markers.
        /// </summary>
        /// <param name="config"></param>
        public TraceCfgParser(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                return;
            }

            string[] parts = config.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim())
                .ToArray();

            foreach (KeyValuePair<string, string[]> filter in this.GetFilters(parts, 1).Reverse())
            {
                string typeName = filter.Key.TrimStart('!');
                bool bIsNotFilter = filter.Key.IndexOf('!') == 0;

                KeyValuePair<Level, MessageTypes> levelAndMsgFilter = this.ParseMsgTypeFilter(filter.Value);

                var curFilterInstance = new TraceFilter(typeName, levelAndMsgFilter.Value, levelAndMsgFilter.Key,
                    bIsNotFilter ? this.NotFilters : this.Filters);

                if (bIsNotFilter)
                {
                    this.NotFilters = curFilterInstance;
                }
                else
                {
                    this.Filters = curFilterInstance;
                }
            }

            if (parts.Length > 0)
            {
                this.OpenOutputDevice(parts[0].ToLower());
            }

            // when only output device was configured or wrong mask was entere we enable full tracing
            // by default
            if (this.Filters == null)
            {
                this.Filters = new TraceFilterMatchAll();
            }

            if (this.bHasError)
            {
                InternalError.PrintHelp();
            }
        }

        public static string DefaultTraceFileBaseName
        {
            get
            {
                string mainModule = Process.GetCurrentProcess().MainModule.FileName;

                return Path.Combine(
                    Path.GetDirectoryName(mainModule),
                    Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".txt");
            }
        }

        public static string DefaultExpandedTraceFileName
        {
            get
            {
                return AddPIDAndAppDomainNameToFileName(DefaultTraceFileBaseName);
            }
        }

        private IEnumerable<KeyValuePair<string, string[]>> GetFilters(string[] filters, int nSkip)
        {
            foreach (string current in filters.Skip(nSkip))
            {
                string[] filterParts = current.Split(new[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterParts.Length < 2)
                {
                    this.bHasError = true;
                    InternalError.Print(
                        "The configuration string {0} did have an unmatched type severity or level filter part: {0}",
                        current);
                }

                yield return new KeyValuePair<string, string[]>(filterParts[0], filterParts.Skip(1).ToArray());
            }
        }

        private void OpenOutputDevice(string outDevice)
        {
            string[] parts = outDevice.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string deviceName = parts[0];
            string deviceConfig = string.Join(" ", parts.Skip(1).ToArray());

            switch (deviceName)
            {
                case "file":
                    if (deviceConfig == string.Empty)
                    {
                        deviceConfig = DefaultTraceFileBaseName;
                    }

                    this.OutDevice = new TextWriterTraceListener(CreateTraceFile(deviceConfig));
                    break;

                case "debugoutput":
                    this.OutDevice = new DefaultTraceListener();
                    break;

               // case "console":
                //    this.OutDevice = new ConsoleTraceListener();
                //    break;
                case "null":
                    this.OutDevice = new NullTraceListener();
                    break;

                case "default":
                    this.UseAppConfigListeners = true;
                    this.OutDevice = new NullTraceListener();
                    break;

                default:
                    InternalError.Print("The trace output device {0} is not supported.", outDevice);
                    this.bHasError = true;
                    break;
            }
        }

        private KeyValuePair<Level, MessageTypes> ParseMsgTypeFilter(string[] typeFilters)
        {
            MessageTypes msgTypeFilter = MessageTypes.None;
            Level level = Level.None;

            foreach (string filter in typeFilters)
            {
                MessageTypes curFilter = MessageTypes.None;
                Level curLevel = Level.None;

                if (!MyFlagTranslator.TryGetValue(filter.Trim(), out curFilter))
                {
                    if (!MyLevelTranslator.TryGetValue(filter.Trim(), out curLevel))
                    {
                        InternalError.Print("The trace message type filter string {0} was not expected.", filter);
                        this.bHasError = true;
                    }
                    else
                    {
                        level |= curLevel;
                    }
                }
                else
                {
                    msgTypeFilter |= curFilter;
                }
            }

            // if nothing was enabled we do enable full tracing by default
            msgTypeFilter = msgTypeFilter == MessageTypes.None ? MessageTypes.All : msgTypeFilter;
            level = level == Level.None ? Level.All : level;

            return new KeyValuePair<Level, MessageTypes>(level, msgTypeFilter);
        }

        public static TextWriter CreateTraceFile(string filebaseName)
        {
            string traceFileName = AddPIDAndAppDomainNameToFileName(Path.GetFullPath(filebaseName));
            string traceDir = Path.GetDirectoryName(traceFileName);

            FileStream fstream = null;
            bool successFullyOpened = false;
            for (int i = 0; i < 2; i++) // Retry the open operation in case of errors
            {
                try
                {
                    Directory.CreateDirectory(
                        Path.GetDirectoryName(
                            traceFileName)); // if the directory to the trace file does not exist create it
                    fstream = new FileStream(traceFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    successFullyOpened = true;
                }
                catch (IOException) // try to open the file with another name in case of a locking error
                {
                    traceDir = traceFileName + Guid.NewGuid();
                }

                if (successFullyOpened)
                {
                    break;
                }
            }

            if (fstream != null)
            {
                TextWriter writer = new StreamWriter(fstream);
                // Create a synchronized TextWriter to enforce proper locking in case of concurrent tracing to file
                writer = TextWriter.Synchronized(writer);
                return writer;
            }

            return null;
        }

        public static string AddPIDAndAppDomainNameToFileName(string file)
        {
            // any supplied PID would be useless since we always append the PID
            string fileName = Path.GetFileName(file).Replace("PID", string.Empty);

            int idx = fileName.LastIndexOf('.');
            if (idx == -1)
            {
                idx = fileName.Length;
            }

            string strippedAppDomainName = AppDomain.CurrentDomain.FriendlyName.Replace('.', '_');
            strippedAppDomainName = strippedAppDomainName.Replace(':', '_').Replace('\\', '_').Replace('/', '_');

            string pidAndAppDomainName = "_" + Process.GetCurrentProcess().Id + "_" + strippedAppDomainName;

            // insert process id and AppDomain name
            fileName = fileName.Insert(idx, pidAndAppDomainName);

            return Path.Combine(Path.GetDirectoryName(file), fileName);
        }
    }
}