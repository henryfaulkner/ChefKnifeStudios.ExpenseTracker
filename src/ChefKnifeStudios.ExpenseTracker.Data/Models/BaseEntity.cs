using ChefKnifeStudios.ExpenseTracker.Data.Repos;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public abstract class BaseEntity : IAggregateRoot
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
}
