using BookKeeping2.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookKeeping2.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260514000000_AddMultiCurrencyBookkeeping")]
public partial class AddMultiCurrencyBookkeeping : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Budgets_CategoryId_BudgetMonth", table: "Budgets");

        migrationBuilder.Sql("UPDATE Accounts SET Currency = 'TWD' WHERE Currency IS NULL OR TRIM(Currency) = ''");

        migrationBuilder.AddColumn<string>(
            name: "Currency",
            table: "Transactions",
            type: "TEXT",
            maxLength: 3,
            nullable: false,
            defaultValue: "TWD");

        migrationBuilder.AddColumn<string>(
            name: "Currency",
            table: "Budgets",
            type: "TEXT",
            maxLength: 3,
            nullable: false,
            defaultValue: "TWD");

        migrationBuilder.CreateIndex(
            name: "IX_Budgets_CategoryId_BudgetMonth_Currency",
            table: "Budgets",
            columns: new[] { "CategoryId", "BudgetMonth", "Currency" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_IsDeleted_Currency_AccountId_TransactionDate",
            table: "Transactions",
            columns: new[] { "IsDeleted", "Currency", "AccountId", "TransactionDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_IsDeleted_Currency_CategoryId_TransactionDate",
            table: "Transactions",
            columns: new[] { "IsDeleted", "Currency", "CategoryId", "TransactionDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_IsDeleted_Currency_TransactionDate",
            table: "Transactions",
            columns: new[] { "IsDeleted", "Currency", "TransactionDate" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Budgets_CategoryId_BudgetMonth_Currency", table: "Budgets");
        migrationBuilder.DropIndex(name: "IX_Transactions_IsDeleted_Currency_AccountId_TransactionDate", table: "Transactions");
        migrationBuilder.DropIndex(name: "IX_Transactions_IsDeleted_Currency_CategoryId_TransactionDate", table: "Transactions");
        migrationBuilder.DropIndex(name: "IX_Transactions_IsDeleted_Currency_TransactionDate", table: "Transactions");

        migrationBuilder.DropColumn(name: "Currency", table: "Transactions");
        migrationBuilder.DropColumn(name: "Currency", table: "Budgets");

        migrationBuilder.CreateIndex(
            name: "IX_Budgets_CategoryId_BudgetMonth",
            table: "Budgets",
            columns: new[] { "CategoryId", "BudgetMonth" },
            unique: true);
    }
}
