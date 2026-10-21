
namespace Domain.Filters;

public class ProductFilter
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool OnlyActive { get; set; }

    public string SortBy { get; set; } = "name";

    public bool Desc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
