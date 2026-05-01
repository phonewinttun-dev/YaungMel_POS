namespace YaungMel_POS.domain.DTOs
{
    public class SearchDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; } 
        public string? Description { get; set; }
        public string? Type { get; set; }
        public decimal? Price { get; set; }
    }

    public class SearchProductRequestDTO
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinStockQuantity { get; set; }
        public int? MaxStockQuantity { get; set; }
        public string? Name { get; set; }

        public enum SortOptions
        {
            name,
            price,
            createdDate
        }
        public SortOptions SortBy { get; set; } = 0;
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SearchCategoryRequestDTO
    {
        public string? Name { get; set; }
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
