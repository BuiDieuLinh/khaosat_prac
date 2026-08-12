using khaosat_api.Models;
using System;
using System.Collections.Generic;

namespace khaosat_api.DTOs
{
    public class EmployeeFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    public class SurveyFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
        public SurveyStatus? Status { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult() { }

        public PagedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
