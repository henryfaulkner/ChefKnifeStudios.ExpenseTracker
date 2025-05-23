using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ChefKnifeStudios.ExpenseTracker.Data.Repos;

public class BudgetSearchSpec : Specification<Budget>
{
    public BudgetSearchSpec(string? searchText, int pageNumber, int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            Query.Where(b => 
                EF.Functions.Like(b.Name, $"%{searchText}%") ||
                (b.Name != null && EF.Functions.Like(b.Name, $"%{searchText}%"))
            );
        }

        Query.OrderByDescending(b => b.CreatedOnUtc)
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize);
    }
}

public class BudgetFilterCountSpec : Specification<Budget>
{
    public BudgetFilterCountSpec(string? searchText)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            Query.Where(b => 
                EF.Functions.Like(b.Name, $"%{searchText}%") ||
                (b.Name != null && EF.Functions.Like(b.Name, $"%{searchText}%"))
            );
        }
    }
}

public interface IBudgetSearchRepository : IRepository<Budget>
{
    Task<PagedResult<Budget>> GetFilteredResultAsync(
        string? searchText,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default
    );
}

public class BudgetSearchRepository : EfRepository<Budget>, IBudgetSearchRepository 
{
    public BudgetSearchRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Budget>> GetFilteredResultAsync(
        string? searchText,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        // Validate parameters
        if (pageSize <= 0) pageSize = 10;
        if (pageNumber <= 0) pageNumber = 1;

        // Get total count first (without pagination)
        var countSpec = new BudgetFilterCountSpec(searchText);
        var totalCount = await CountAsync(countSpec, cancellationToken);

        // Get paginated results
        var searchSpec = new BudgetSearchSpec(searchText, pageNumber, pageSize);
        var results = await ListAsync(searchSpec, cancellationToken);

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasNextPage = pageNumber < totalPages;
        var hasPreviousPage = pageNumber > 1;

        return new PagedResult<Budget>
        {
            Records = results,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
        };
    }
}