using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Todo.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int TotalCount { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int CurrentPage { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(IEnumerable<T> items, int count, int pageSize, int pageNumber)
        {
            TotalCount = count;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            CurrentPage = pageNumber;
            AddRange(items);
        }

        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
        public static PagedList<T> Create(IQueryable<T> source, int pageSize, int pageNumber) =>
            new PagedList<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize), source.Count(), pageSize, pageNumber);
    }
}
