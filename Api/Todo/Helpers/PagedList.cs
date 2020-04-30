using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Todo.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize) =>
            new PagedList<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(), source.Count(), pageNumber, pageSize);
    }
}
