using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Deed.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capitals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Balance = table.Column<float>(type: "real", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncludeInTotal = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capitals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PlannedPeriodAmount = table.Column<float>(type: "real", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalCurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Buy = table.Column<float>(type: "real", nullable: false),
                    Sale = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    SourceCapitalId = table.Column<int>(type: "int", nullable: true),
                    DestinationCapitalId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Capitals_DestinationCapitalId",
                        column: x => x.DestinationCapitalId,
                        principalTable: "Capitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Capitals_SourceCapitalId",
                        column: x => x.SourceCapitalId,
                        principalTable: "Capitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CapitalId = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Capitals_CapitalId",
                        column: x => x.CapitalId,
                        principalTable: "Capitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CapitalId = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incomes_Capitals_CapitalId",
                        column: x => x.CapitalId,
                        principalTable: "Capitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incomes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Capitals",
                columns: new[] { "Id", "Balance", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "IncludeInTotal", "IsDeleted", "Name", "OrderIndex", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "UAH", null, true, null, "Cash", 0, null, null },
                    { 2, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "UAH", null, true, null, "Bank", 1, null, null },
                    { 3, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "USD", null, true, null, "Investments", 2, null, null },
                    { 4, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "USD", null, true, null, "Savings", 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "IsDeleted", "Name", "Period", "PlannedPeriodAmount", "Type", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Groceries", 0, 0f, 1, null, null },
                    { 2, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Utilities", 0, 0f, 1, null, null },
                    { 3, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Rent", 0, 0f, 1, null, null },
                    { 4, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Transportation", 0, 0f, 1, null, null },
                    { 5, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Healthcare", 0, 0f, 1, null, null },
                    { 6, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Entertainment", 0, 0f, 1, null, null },
                    { 7, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Education", 0, 0f, 1, null, null },
                    { 8, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Clothing", 0, 0f, 1, null, null },
                    { 9, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Subscriptions", 0, 0f, 1, null, null },
                    { 10, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Travel", 0, 0f, 1, null, null },
                    { 11, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Gifts", 0, 0f, 1, null, null },
                    { 12, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Donations", 0, 0f, 1, null, null },
                    { 13, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Salary", 0, 0f, 2, null, null },
                    { 14, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Gifts", 0, 0f, 2, null, null },
                    { 15, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Grants", 0, 0f, 2, null, null },
                    { 16, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Sales", 0, 0f, 2, null, null }
                });

            migrationBuilder.InsertData(
                table: "Exchanges",
                columns: new[] { "Id", "Buy", "CreatedAt", "CreatedBy", "DeletedAt", "IsDeleted", "NationalCurrencyCode", "Sale", "TargetCurrencyCode", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 44.63f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 45.45455f, "EUR", null, null },
                    { 2, 41.2f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 34f, "USD", null, null },
                    { 3, 43f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 43f, "PLN", null, null },
                    { 4, 43f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 43f, "UAH", null, null },
                    { 5, 32f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 32f, "EUR", null, null },
                    { 6, 41f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 43f, "PLN", null, null },
                    { 7, 41f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 40f, "USD", null, null },
                    { 8, 39f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 38f, "UAH", null, null },
                    { 9, 30f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 32f, "PLN", null, null },
                    { 10, 20f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 10f, "UAH", null, null },
                    { 11, 7f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 6f, "USD", null, null },
                    { 12, 3f, new DateTimeOffset(new DateTime(2025, 9, 8, 16, 30, 35, 833, DateTimeKind.Unspecified).AddTicks(2601), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 20f, "EUR", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Capitals_Balance",
                table: "Capitals",
                column: "Balance");

            migrationBuilder.CreateIndex(
                name: "IX_Capitals_Name",
                table: "Capitals",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CapitalId",
                table: "Expenses",
                column: "CapitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_CapitalId",
                table: "Incomes",
                column: "CapitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_CategoryId",
                table: "Incomes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationCapitalId",
                table: "Transfers",
                column: "DestinationCapitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceCapitalId",
                table: "Transfers",
                column: "SourceCapitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Incomes");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Capitals");
        }
    }
}
