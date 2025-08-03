using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChefKnifeStudios.ExpenseTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_ExpenseCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CategoryType = table.Column<int>(type: "integer", nullable: false),
                    LabelsJson = table.Column<string>(type: "text", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategorySemantics",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Labels = table.Column<string>(type: "text", nullable: false),
                    SemanticEmbedding = table.Column<byte[]>(type: "bytea", nullable: true),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySemantics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySemantics_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "ExpenseTracker",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                schema: "ExpenseTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpenseId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "ExpenseTracker",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalSchema: "ExpenseTracker",
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySemantics_CategoryId",
                schema: "ExpenseTracker",
                table: "CategorySemantics",
                column: "CategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_CategoryId",
                schema: "ExpenseTracker",
                table: "ExpenseCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_ExpenseId_CategoryId",
                schema: "ExpenseTracker",
                table: "ExpenseCategories",
                columns: new[] { "ExpenseId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorySemantics",
                schema: "ExpenseTracker");

            migrationBuilder.DropTable(
                name: "ExpenseCategories",
                schema: "ExpenseTracker");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "ExpenseTracker");
        }
    }
}
