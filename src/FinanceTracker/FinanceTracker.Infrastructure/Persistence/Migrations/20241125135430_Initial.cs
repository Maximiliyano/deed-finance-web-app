﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceTracker.Infrastructure.Persistence.Migrations;

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
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Capitals", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_Categories", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_Exchanges", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Transfers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Amount = table.Column<float>(type: "real", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                SourceCapitalId = table.Column<int>(type: "int", nullable: true),
                DestinationCapitalId = table.Column<int>(type: "int", nullable: true)
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
                Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CategoryId = table.Column<int>(type: "int", nullable: false),
                CapitalId = table.Column<int>(type: "int", nullable: false),
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
                Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CategoryId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdatedBy = table.Column<int>(type: "int", nullable: true),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                CapitalId = table.Column<int>(type: "int", nullable: true)
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
            table: "Categories",
            columns: ["Id", "CreatedAt", "CreatedBy", "DeletedAt", "IsDeleted", "Name", "Period", "PlannedPeriodAmount", "Type", "UpdatedAt", "UpdatedBy"],
            values: new object[,]
            {
                { 1, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Groceries", 0, 0f, 1, null, null },
                { 2, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Utilities", 0, 0f, 1, null, null },
                { 3, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Rent", 0, 0f, 1, null, null },
                { 4, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Transportation", 0, 0f, 1, null, null },
                { 5, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Healthcare", 0, 0f, 1, null, null },
                { 6, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Entertainment", 0, 0f, 1, null, null },
                { 7, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Education", 0, 0f, 1, null, null },
                { 8, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Clothing", 0, 0f, 1, null, null },
                { 9, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Subscriptions", 0, 0f, 1, null, null },
                { 10, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Travel", 0, 0f, 1, null, null },
                { 11, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Gifts", 0, 0f, 1, null, null },
                { 12, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Donations", 0, 0f, 1, null, null },
                { 13, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Salary", 0, 0f, 2, null, null },
                { 14, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Gifts", 0, 0f, 2, null, null },
                { 15, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Grants", 0, 0f, 2, null, null },
                { 16, new DateTimeOffset(new DateTime(2024, 11, 25, 13, 54, 29, 995, DateTimeKind.Unspecified).AddTicks(6621), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "Sales", 0, 0f, 2, null, null }
            });

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
