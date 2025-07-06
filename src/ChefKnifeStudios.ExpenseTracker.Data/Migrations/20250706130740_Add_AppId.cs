using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChefKnifeStudios.ExpenseTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "RecurringExpenseConfigs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "Expenses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "Budgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "RecurringExpenseConfigs");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "ExpenseTracker",
                table: "Budgets");
        }
    }
}
