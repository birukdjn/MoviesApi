namespace Backend.Common.Pagination
{
    public class PagedResult
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex + 1 < TotalPages;
            

    }
}
