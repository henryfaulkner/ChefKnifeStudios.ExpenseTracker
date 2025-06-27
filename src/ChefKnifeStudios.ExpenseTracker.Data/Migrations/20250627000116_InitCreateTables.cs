using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChefKnifeStudios.ExpenseTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitCreateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ExpenseTracker");

            migrationBuilder.CreateTable(
                name: "Budgets",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ExpenseBudget = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurringExpenseConfigs",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    LabelsJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringExpenseConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BudgetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    LabelsJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "ExpenseTracker",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseSemantics",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExpenseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Labels = table.Column<string>(type: "TEXT", nullable: false),
                    SemanticEmbedding = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseSemantics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseSemantics_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalSchema: "ExpenseTracker",
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetId",
                schema: "ExpenseTracker",
                table: "Expenses",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseSemantics_ExpenseId",
                schema: "ExpenseTracker",
                table: "ExpenseSemantics",
                column: "ExpenseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseSemantics",
                schema: "ExpenseTracker");

            migrationBuilder.DropTable(
                name: "RecurringExpenseConfigs",
                schema: "ExpenseTracker");

            migrationBuilder.DropTable(
                name: "Expenses",
                schema: "ExpenseTracker");

            migrationBuilder.DropTable(
                name: "Budgets",
                schema: "ExpenseTracker");
        }
    }
}
