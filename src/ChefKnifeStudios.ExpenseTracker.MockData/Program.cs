using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using ChefKnifeStudios.ExpenseTracker.MockData;
using Microsoft.Extensions.Configuration;

class Program
{
    public static async Task Main(string[] args)
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>();
                if (appSettings == null) throw new ApplicationException("AppSettings are not configured correctly.");

                services.AddDbContext<AppDbContext>();
                services
                    .RegisterDataServices(context.Configuration)
                    .AddSqliteVectorStore(_ => context.Configuration.GetConnectionString("ExpenseTrackerDB")!);
                services
                    .AddKernel()
                    .ConfigureSemanticKernel(appSettings);
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var embeddingGenerator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        if (db.Budgets.Any() || db.Expenses.Any() || db.ExpenseSemantics.Any())
        {
            Console.WriteLine("Database already seeded.");
            return;
        }

        // Name/label pairs for realistic data
        var expenseData = new (string Name, string[] Labels)[]
        {
            ("Coffee Shop", new[] { "Coffee", "Breakfast", "Cafe" }),
            ("Groceries", new[] { "Food", "Groceries", "Supermarket" }),
            ("Movie Night", new[] { "Entertainment", "Movies", "Cinema" }),
            ("Bookstore", new[] { "Books", "Reading", "Education" }),
            ("Gas Station", new[] { "Fuel", "Car", "Transport" }),
            ("Restaurant", new[] { "Dining", "Food", "Dinner" }),
            ("Gym Membership", new[] { "Fitness", "Health", "Exercise" }),
            ("Pharmacy", new[] { "Medicine", "Health", "Wellness" }),
            ("Online Shopping", new[] { "Shopping", "Online", "E-commerce" }),
            ("Taxi Ride", new[] { "Transport", "Taxi", "Travel" }),
            ("Fast Food", new[] { "Lunch", "Fast Food", "Takeout" }),
            ("Electronics", new[] { "Electronics", "Gadgets", "Tech" }),
            ("Clothing", new[] { "Clothing", "Apparel", "Fashion" }),
            ("Bakery", new[] { "Bakery", "Snacks", "Bread" }),
            ("Pet Store", new[] { "Pets", "Supplies", "Animals" })
        };

        var random = new Random();
        var budgets = new List<Budget>();
        var expenses = new List<Expense>();
        var semantics = new List<ExpenseSemantic>();

        // Create 5 budgets (months)
        for (int m = 0; m < 5; m++)
        {
            var month = DateTime.UtcNow.AddMonths(-m);
            var budget = new Budget
            {
                Name = month.ToString("MMMM yyyy"),
                ExpenseBudget = random.Next(800, 2000),
                StartDateUtc = new DateTime(month.Year, month.Month, 1),
                EndDateUtc = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)),
            };
            budgets.Add(budget);
        }
        db.Budgets.AddRange(budgets);
        await db.SaveChangesAsync();

        // For each budget, create 10 expenses with related labels
        foreach (var budget in budgets)
        {
            for (int i = 0; i < 10; i++)
            {
                var (name, labels) = expenseData[random.Next(expenseData.Length)];
                var cost = Math.Round((decimal)(random.NextDouble() * 100 + 5), 2);

                var expense = new Expense
                {
                    Name = name,
                    Cost = cost,
                    BudgetId = budget.Id,
                    LabelsJson = JsonSerializer.Serialize(labels),
                };
                expenses.Add(expense);
            }
        }
        db.Expenses.AddRange(expenses);
        await db.SaveChangesAsync();

        // For each expense, create a semantic record with real embedding
        foreach (var expense in expenses)
        {
            var labels = JsonSerializer.Deserialize<string[]>(expense.LabelsJson) ?? Array.Empty<string>();
            var labelText = string.Join(" ", labels);
            var embedding = await embeddingGenerator.GenerateAsync(labelText);
            var semantic = new ExpenseSemantic
            {
                ExpenseId = expense.Id,
                Labels = labelText,
                SemanticEmbedding = System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(embedding.Vector.ToArray().AsSpan()).ToArray()
            };
            semantics.Add(semantic);
        }
        db.ExpenseSemantics.AddRange(semantics);
        await db.SaveChangesAsync();

        Console.WriteLine($"Database seeded with {budgets.Count} budgets, {expenses.Count} expenses, and {semantics.Count} semantic records.");
    }
}
