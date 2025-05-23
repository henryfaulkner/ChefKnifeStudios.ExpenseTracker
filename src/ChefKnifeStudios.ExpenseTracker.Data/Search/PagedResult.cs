namespace ChefKnifeStudios.ExpenseTracker.Data.Search;

public class PagedResult<T> where T : class
{
    public int TotalRecords { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }

    public List<T>? Records { get; set; }
}
