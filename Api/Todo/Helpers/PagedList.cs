using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using static System.Text.Encodings.Web.JavaScriptEncoder;

namespace Todo.Helpers
{
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
    public class PagedList<T> : List<T>
    {
        #region Properties

        public int TotalCount { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int CurrentPage { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        #endregion

        #region Constructors

        public PagedList(IEnumerable<T> items, int count, int pageSize, int pageNumber)
        {
            TotalCount = count;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            CurrentPage = pageNumber;
            AddRange(items);
        }

        #endregion

        #region Methods

        public static PagedList<T> Create(IQueryable<T> source, int pageSize, int pageNumber) => new
            PagedList<T>(source.Skip((pageNumber - 1) * pageSize).Take(pageSize), source.Count(), pageSize, pageNumber);

        public static void CreatePaginationHeader(HttpResponse response, PagedList<T> list) => response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
        {
            totalCount = list.TotalCount,
            pageSize = list.PageSize,
            totalPages = list.TotalPages,
            currentPage = list.CurrentPage
        }, new JsonSerializerOptions { Encoder = UnsafeRelaxedJsonEscaping }));

        #endregion
    }
}
