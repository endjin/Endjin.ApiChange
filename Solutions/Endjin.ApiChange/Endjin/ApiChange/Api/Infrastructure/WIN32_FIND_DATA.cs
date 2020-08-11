// <copyright file="WIN32_FIND_DATA.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Contains information about the file that is found
    ///     by the FindFirstFile or FindNextFile functions.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    [BestFitMapping(false)]
    internal class WIN32_FIND_DATA
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;

        public FileAttributes dwFileAttributes;

        public int dwReserved0;

        public int dwReserved1;

        public uint ftCreationTime_dwHighDateTime;

        public uint ftCreationTime_dwLowDateTime;

        public uint ftLastAccessTime_dwHighDateTime;

        public uint ftLastAccessTime_dwLowDateTime;

        public uint ftLastWriteTime_dwHighDateTime;

        public uint ftLastWriteTime_dwLowDateTime;

        public uint nFileSizeHigh;

        public uint nFileSizeLow;

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return "File name=" + this.cFileName;
        }
    }
}