using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using static System.Text.Encodings.Web.JavaScriptEncoder;
using static System.Text.Json.JsonNamingPolicy;
using static System.Text.Json.JsonSerializer;

namespace Todo.Helpers
{
    public class PagedList<T> : List<T>
    {
        #region Properties

        private int TotalCount { get; }
        private int PageSize { get; }
        private int TotalPages { get; }
        private int CurrentPage { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        #endregion

        #region Constructors

        public PagedList(IQueryable<T> source, int pageSize, int pageNumber)
        {
            TotalCount = source.Count();
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
            CurrentPage = pageNumber;
            AddRange(source.Skip((pageNumber - 1) * pageSize).Take(pageSize));
        }

        #endregion

        #region Methods

        public void CreatePaginationHeader(HttpResponse response) => response.Headers.Add("X-Pagination", Serialize(new
        {
            TotalCount,
            PageSize,
            TotalPages,
            CurrentPage
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = CamelCase,
            Encoder = UnsafeRelaxedJsonEscaping
        }));

        #endregion
    }
}
