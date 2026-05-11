using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookKeeping2.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                IconKey = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                OpeningBalanceMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", account => account.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppSettings",
            columns: table => new
            {
                Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSettings", setting => setting.Key);
            });

        migrationBuilder.CreateTable(
            name: "AuditEvents",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                EventType = table.Column<int>(type: "INTEGER", nullable: false),
                EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                EntityId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Summary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditEvents", auditEvent => auditEvent.Id);
            });

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                IconKey = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", category => category.Id);
            });

        migrationBuilder.CreateTable(
            name: "CsvImportBatches",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                ImportedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TotalRows = table.Column<int>(type: "INTEGER", nullable: false),
                SucceededRows = table.Column<int>(type: "INTEGER", nullable: false),
                FailedRows = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedCategoryCount = table.Column<int>(type: "INTEGER", nullable: false),
                Summary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CsvImportBatches", batch => batch.Id);
            });

        migrationBuilder.CreateTable(
            name: "Budgets",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CategoryId = table.Column<long>(type: "INTEGER", nullable: false),
                BudgetMonth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                AmountMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Budgets", budget => budget.Id);
                table.ForeignKey(
                    name: "FK_Budgets_Categories_CategoryId",
                    column: budget => budget.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CsvImportErrors",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CsvImportBatchId = table.Column<long>(type: "INTEGER", nullable: false),
                RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                FieldName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                RawValuePreview = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CsvImportErrors", error => error.Id);
                table.ForeignKey(
                    name: "FK_CsvImportErrors_CsvImportBatches_CsvImportBatchId",
                    column: error => error.CsvImportBatchId,
                    principalTable: "CsvImportBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TransactionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                AmountMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                CategoryId = table.Column<long>(type: "INTEGER", nullable: false),
                AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                DeletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                DeletionSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                LastChangeSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", transaction => transaction.Id);
                table.ForeignKey(
                    name: "FK_Transactions_Accounts_AccountId",
                    column: transaction => transaction.AccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Transactions_Categories_CategoryId",
                    column: transaction => transaction.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Accounts_IsArchived_DisplayOrder", table: "Accounts", columns: new[] { "IsArchived", "DisplayOrder" });
        migrationBuilder.CreateIndex(name: "IX_Accounts_NormalizedName", table: "Accounts", column: "NormalizedName", unique: true);
        migrationBuilder.CreateIndex(name: "IX_AuditEvents_EntityType_EntityId", table: "AuditEvents", columns: new[] { "EntityType", "EntityId" });
        migrationBuilder.CreateIndex(name: "IX_AuditEvents_EventType_OccurredAtUtc", table: "AuditEvents", columns: new[] { "EventType", "OccurredAtUtc" });
        migrationBuilder.CreateIndex(name: "IX_Budgets_CategoryId_BudgetMonth", table: "Budgets", columns: new[] { "CategoryId", "BudgetMonth" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Categories_Type_IsArchived_DisplayOrder", table: "Categories", columns: new[] { "Type", "IsArchived", "DisplayOrder" });
        migrationBuilder.CreateIndex(name: "IX_Categories_Type_NormalizedName", table: "Categories", columns: new[] { "Type", "NormalizedName" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_CsvImportErrors_CsvImportBatchId_RowNumber", table: "CsvImportErrors", columns: new[] { "CsvImportBatchId", "RowNumber" });
        migrationBuilder.CreateIndex(name: "IX_Transactions_AccountId", table: "Transactions", column: "AccountId");
        migrationBuilder.CreateIndex(name: "IX_Transactions_CategoryId", table: "Transactions", column: "CategoryId");
        migrationBuilder.CreateIndex(name: "IX_Transactions_IsDeleted_AccountId_TransactionDate", table: "Transactions", columns: new[] { "IsDeleted", "AccountId", "TransactionDate" });
        migrationBuilder.CreateIndex(name: "IX_Transactions_IsDeleted_AmountMinorUnits", table: "Transactions", columns: new[] { "IsDeleted", "AmountMinorUnits" });
        migrationBuilder.CreateIndex(name: "IX_Transactions_IsDeleted_CategoryId_TransactionDate", table: "Transactions", columns: new[] { "IsDeleted", "CategoryId", "TransactionDate" });
        migrationBuilder.CreateIndex(name: "IX_Transactions_IsDeleted_TransactionDate", table: "Transactions", columns: new[] { "IsDeleted", "TransactionDate" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppSettings");
        migrationBuilder.DropTable(name: "AuditEvents");
        migrationBuilder.DropTable(name: "Budgets");
        migrationBuilder.DropTable(name: "CsvImportErrors");
        migrationBuilder.DropTable(name: "Transactions");
        migrationBuilder.DropTable(name: "CsvImportBatches");
        migrationBuilder.DropTable(name: "Accounts");
        migrationBuilder.DropTable(name: "Categories");
    }
}
