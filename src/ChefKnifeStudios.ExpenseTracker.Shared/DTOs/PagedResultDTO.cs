namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class PagedResultDTO<T> where T : class
{
    public int TotalRecords { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }

    public List<T>? Records { get; set; }
}
