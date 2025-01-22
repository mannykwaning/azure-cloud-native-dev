using System;
using System.Collections.Generic;
using System.Linq;

namespace HW7DocumentSearchSolution
{
    /// <summary>
    /// Utility class to assist with Pagination
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        /// <summary>
        /// To help control next/ previous navigation
        /// </summary>
        public bool HasPrevPage { get { return (PageIndex > 1); } }
        public bool HasNextPage { get { return (PageIndex < TotalPages); } }

        /// <summary>
        /// Paginates a List
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>Paginated List</returns>
        public static PaginatedList<T> Create(
            List<T> dataSource, int pageIndex, int pageSize)
        {
            var count = dataSource.Count();
            var items = dataSource.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
