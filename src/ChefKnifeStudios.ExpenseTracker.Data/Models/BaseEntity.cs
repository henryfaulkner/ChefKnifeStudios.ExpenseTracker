using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using System.ComponentModel.DataAnnotations;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public abstract class BaseEntity : IAggregateRoot
{
    [Key]
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
}
