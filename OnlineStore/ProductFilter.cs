namespace OnlineStore;

public class ProductFilter
{
    public string? Text { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public string SortBy { get; set; } = "name";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
