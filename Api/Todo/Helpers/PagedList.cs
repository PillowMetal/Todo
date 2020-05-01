﻿using System;
using System.Collections.Generic;
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

        public static PagedList<T> Create(IQueryable<T> source, int pageSize, int pageNumber) =>
            new PagedList<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(), source.Count(), pageSize, pageNumber);
    }
}