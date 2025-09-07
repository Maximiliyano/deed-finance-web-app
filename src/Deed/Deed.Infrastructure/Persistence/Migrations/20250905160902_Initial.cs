using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Deed.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    private static readonly string[] columns = ["Id", "Buy", "CreatedAt", "CreatedBy", "DeletedAt", "IsDeleted", "NationalCurrencyCode", "Sale", "TargetCurrencyCode", "UpdatedAt", "UpdatedBy"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                MainCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "True"),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

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
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
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
            columns: ["Id", "Balance", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "IncludeInTotal", "IsDeleted", "Name", "OrderIndex", "UpdatedAt", "UpdatedBy"],
            values: new object[,]
            {
                { 1, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "UAH", null, true, null, "Cash", 0, null, null },
                { 2, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "UAH", null, true, null, "Bank", 1, null, null },
                { 3, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "USD", null, true, null, "Investments", 2, null, null },
                { 4, 1000f, new DateTimeOffset(new DateTime(2025, 9, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "USD", null, true, null, "Savings", 3, null, null }
            });

        migrationBuilder.InsertData(
            table: "Categories",
            columns: ["Id", "CreatedAt", "CreatedBy", "DeletedAt", "IsDeleted", "Name", "Period", "PlannedPeriodAmount", "Type", "UpdatedAt", "UpdatedBy"],
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
            columns: columns,
            values: new object[,]
            {
                { 1, 44.63f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 45.45455f, "EUR", null, null },
                { 2, 41.2f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 34f, "USD", null, null },
                { 3, 43f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "UAH", 43f, "PLN", null, null },
                { 4, 43f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 43f, "UAH", null, null },
                { 5, 32f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 32f, "EUR", null, null },
                { 6, 41f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "USD", 43f, "PLN", null, null },
                { 7, 41f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 40f, "USD", null, null },
                { 8, 39f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 38f, "UAH", null, null },
                { 9, 30f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "EUR", 32f, "PLN", null, null },
                { 10, 20f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 10f, "UAH", null, null },
                { 11, 7f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 6f, "USD", null, null },
                { 12, 3f, new DateTimeOffset(new DateTime(2025, 9, 5, 16, 9, 1, 444, DateTimeKind.Unspecified).AddTicks(3598), new TimeSpan(0, 0, 0, 0, 0)), 0, null, null, "PLN", 20f, "EUR", null, null }
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

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
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "Exchanges");

        migrationBuilder.DropTable(
            name: "Expenses");

        migrationBuilder.DropTable(
            name: "Incomes");

        migrationBuilder.DropTable(
            name: "Transfers");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Categories");

        migrationBuilder.DropTable(
            name: "Capitals");
    }
}
