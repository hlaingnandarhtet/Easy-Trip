namespace DotNet8.EasyTripBackendApi.Shared
{
    public class PaginationResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => PageNo < TotalPages;
        public bool HasPreviousPage => PageNo > 1;

        public PaginationResponse() { }

        public PaginationResponse(List<T> data, int count, int pageNo, int pageSize)
        {
            Data = data;
            TotalCount = count;
            PageNo = pageNo;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}
