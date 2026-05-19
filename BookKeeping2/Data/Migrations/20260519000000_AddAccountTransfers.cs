using System;
using BookKeeping2.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookKeeping2.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260519000000_AddAccountTransfers")]
public partial class AddAccountTransfers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccountTransfers",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TransferDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "TWD"),
                AmountMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                FromAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                ToAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                SubmissionToken = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                DeletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                DeletionSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                LastChangeSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountTransfers", transfer => transfer.Id);
                table.ForeignKey(
                    name: "FK_AccountTransfers_Accounts_FromAccountId",
                    column: transfer => transfer.FromAccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AccountTransfers_Accounts_ToAccountId",
                    column: transfer => transfer.ToAccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_FromAccountId",
            table: "AccountTransfers",
            column: "FromAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_IsDeleted_Currency_TransferDate",
            table: "AccountTransfers",
            columns: new[] { "IsDeleted", "Currency", "TransferDate" });

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_IsDeleted_FromAccountId_TransferDate",
            table: "AccountTransfers",
            columns: new[] { "IsDeleted", "FromAccountId", "TransferDate" });

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_IsDeleted_ToAccountId_TransferDate",
            table: "AccountTransfers",
            columns: new[] { "IsDeleted", "ToAccountId", "TransferDate" });

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_IsDeleted_TransferDate",
            table: "AccountTransfers",
            columns: new[] { "IsDeleted", "TransferDate" });

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_SubmissionToken",
            table: "AccountTransfers",
            column: "SubmissionToken",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccountTransfers_ToAccountId",
            table: "AccountTransfers",
            column: "ToAccountId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AccountTransfers");
    }
}
