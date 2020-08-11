// <copyright file="ListExtensions.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ListExtensions
    {
        public static string GetSearchDirs(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            string ret = string.Empty;
            foreach (FileQuery q in queries)
            {
                ret += q.SearchDir + ";";
            }

            return ret.TrimEnd(';');
        }

        public static string GetQueries(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            string lret = string.Empty;
            foreach (FileQuery q in queries)
            {
                lret += q.Query + " ";
            }

            return lret.Trim();
        }

        public static IEnumerable<string> GetFiles(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            foreach (FileQuery q in queries)
            {
                q.BeginSearch();
            }

            foreach (FileQuery q in queries)
            {
                foreach (string file in q.EnumerateFiles)
                {
                    yield return file;
                }
            }
        }

        public static bool HasMatches(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            bool lret = false;
            foreach (FileQuery q in queries)
            {
                lret = q.HasMatches;
                if (lret)
                {
                    break;
                }
            }

            return lret;
        }

        public static string GetMatchingFileByName(this List<FileQuery> queries, string fileName)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName to filter for was null or empty");
            }

            string match = null;
            foreach (FileQuery q in queries)
            {
                match = q.GetMatchingFileByName(fileName);
                if (match != null)
                {
                    break;
                }
            }

            return match;
        }

        public static List<string> GetNotExistingFilesInOtherQuery(
            this List<FileQuery> queries,
            List<FileQuery> otherQueries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries");
            }

            if (otherQueries == null)
            {
                throw new ArgumentNullException("otherQueries");
            }

            var query1 = new HashSet<string>(queries.GetFiles(), new FileNameComparer());
            var query2 = new HashSet<string>(otherQueries.GetFiles(), new FileNameComparer());

            var removedFiles = new HashSet<string>(query1, new FileNameComparer());
            removedFiles.ExceptWith(query2);

            return removedFiles.ToList();
        }
    }
}