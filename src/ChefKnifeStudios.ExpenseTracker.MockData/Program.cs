using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Enums;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.MockData;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using System.Text.Json;

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

                string connectionString = context.Configuration.GetConnectionString("ExpenseTrackerDB")!;

                services.AddDbContext<AppDbContext>();
                services
                    .RegisterDataServices(connectionString)
                    .AddPostgresVectorStore(_ => connectionString);
                services
                    .AddKernel()
                    .ConfigureSemanticKernel(appSettings);
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var embeddingGenerator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        var vectorStore = scope.ServiceProvider.GetRequiredService<PostgresVectorStore>();

        // await CreateMockExpensesAsync(db, embeddingGenerator, vectorStore);
        await CreateCategoriesAsync(db, embeddingGenerator, vectorStore);
    }

    static async Task CreateCategoriesAsync(
        AppDbContext db,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        PostgresVectorStore vectorStore)
    {
        var categories = new[]
        {
            new Category
            {
                DisplayName = "Groceries",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "groceries", "supermarket", "food shopping", "produce", "meat", "dairy", "bakery", "household essentials", "weekly shopping", "grocery store", "fresh food", "pantry", "market"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Rent",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "rent", "apartment", "housing", "monthly payment", "lease", "landlord", "residential", "living expenses", "home", "flat", "rental property"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Utilities",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "utilities", "electricity", "water bill", "gas bill", "internet", "wifi", "cable", "phone bill", "energy", "power", "trash", "sewage", "monthly utilities"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Transportation",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "transportation", "commute", "bus", "train", "subway", "metro", "public transit", "taxi", "rideshare", "uber", "lyft", "car", "fuel", "gasoline", "parking", "vehicle maintenance", "auto insurance"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Dining Out",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "dining out", "restaurant", "cafe", "coffee shop", "fast food", "takeout", "delivery", "lunch", "dinner", "breakfast", "food service", "eating out", "bistro", "bar", "pub"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Entertainment",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "entertainment", "movies", "cinema", "theater", "concert", "music", "games", "video games", "streaming", "netflix", "hulu", "disney+", "subscriptions", "events", "shows", "amusement park", "nightlife"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Healthcare",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "healthcare", "doctor", "medical", "pharmacy", "medicine", "prescription", "hospital", "clinic", "health insurance", "dental", "vision", "wellness", "therapy", "mental health", "checkup"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Fitness",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "fitness", "gym", "workout", "exercise", "personal trainer", "yoga", "pilates", "fitness class", "membership", "sports", "athletics", "health club", "recreation"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Shopping",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "shopping", "clothing", "apparel", "fashion", "shoes", "accessories", "retail", "mall", "boutique", "online shopping", "e-commerce", "electronics", "gadgets", "tech", "appliances"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Education",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "education", "school", "tuition", "books", "supplies", "courses", "university", "college", "learning", "training", "workshop", "seminar", "online course", "study", "class"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Travel",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "travel", "vacation", "trip", "flight", "airfare", "hotel", "accommodation", "lodging", "tourism", "car rental", "sightseeing", "holiday", "journey", "cruise", "passport", "visa"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Insurance",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "insurance", "health insurance", "auto insurance", "car insurance", "home insurance", "renter's insurance", "life insurance", "policy", "premium", "coverage", "claim"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Pets",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "pets", "pet food", "veterinarian", "vet", "pet supplies", "dog", "cat", "animal care", "grooming", "pet insurance", "boarding", "pet sitting"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Gifts & Donations",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "gifts", "donations", "charity", "present", "birthday", "holiday gift", "wedding gift", "fundraiser", "nonprofit", "giving", "contribution"
                }),
                AppId = Guid.Empty
            },
            new Category
            {
                DisplayName = "Personal Care",
                CategoryType = CategoryTypes.Expense,
                LabelsJson = JsonSerializer.Serialize(new[]
                {
                    "personal care", "haircut", "salon", "spa", "cosmetics", "skincare", "beauty", "hygiene", "barber", "massage", "nail salon", "wellness"
                }),
                AppId = Guid.Empty
            }
        };
        var semantics = new List<CategorySemantic>();

        await db.AddRangeAsync(categories);
        await db.SaveChangesAsync();

        string collectionName = Collections.CategorySemantics;
        var categorySemanticCollection = vectorStore.GetCollection<int, CategorySemantic>(collectionName);
        await categorySemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

        foreach (var category in categories)
        {
            var labels = JsonSerializer.Deserialize<string[]>(category.LabelsJson) ?? Array.Empty<string>();
            var labelText = string.Join(" ", labels);
            var embedding = await embeddingGenerator.GenerateAsync(labelText);
            var semantic = new CategorySemantic
            {
                CategoryId = category.Id,
                Labels = labelText,
                SemanticEmbedding = System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(embedding.Vector.ToArray().AsSpan()).ToArray()
            };
            semantics.Add(semantic);
        }

        db.CategorySemantics.AddRange(semantics);
        await db.SaveChangesAsync();
        await categorySemanticCollection.UpsertAsync(semantics);
    }

    static async Task CreateMockExpensesAsync(
        AppDbContext db, 
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        PostgresVectorStore vectorStore)
    {
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
                StartDateUtc = new DateTime(month.Year, month.Month, 1).ToUniversalTime(),
                EndDateUtc = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)).ToUniversalTime(),
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

        string collectionName = Collections.ExpenseSemantics;
        var expenseSemanticCollection = vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
        await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

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
        await expenseSemanticCollection.UpsertAsync(semantics);

        Console.WriteLine($"Database seeded with {budgets.Count} budgets, {expenses.Count} expenses, and {semantics.Count} semantic records.");
    }
}
