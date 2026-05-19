using BookKeeping2.Data;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace BookKeeping2.Tests.Integration.Persistence;

public sealed class AccountTransferPersistenceTests
{
    private const string EntityName = "BookKeeping2.Models.AccountTransfers.AccountTransfer";

    [Fact]
    public void Ef_model_defines_account_transfer_columns_and_soft_delete_contract()
    {
        using AppDbContext context = CreateContext();
        IEntityType entity = FindAccountTransferEntity(context);

        Assert.Equal("AccountTransfers", entity.GetTableName());
        AssertRequiredProperty<DateOnly>(entity, "TransferDate");
        AssertRequiredProperty<string>(entity, "Currency", maxLength: 3);
        AssertRequiredProperty<long>(entity, "AmountMinorUnits");
        AssertRequiredProperty<long>(entity, "FromAccountId");
        AssertRequiredProperty<long>(entity, "ToAccountId");
        AssertRequiredProperty<string>(entity, "SubmissionToken", maxLength: 64);
        AssertRequiredProperty<DateTimeOffset>(entity, "CreatedAtUtc");
        AssertRequiredProperty<DateTimeOffset>(entity, "UpdatedAtUtc");
        AssertRequiredProperty<bool>(entity, "IsDeleted");
        AssertOptionalProperty<DateTimeOffset>(entity, "DeletedAtUtc");
        AssertOptionalProperty<string>(entity, "DeletionSummary", maxLength: 500);
        AssertRequiredProperty<string>(entity, "LastChangeSummary", maxLength: 500);
        AssertOptionalProperty<string>(entity, "Note", maxLength: 500);
    }

    [Fact]
    public void Ef_model_defines_restrict_foreign_keys_unique_submission_token_and_query_indexes()
    {
        using AppDbContext context = CreateContext();
        IEntityType entity = FindAccountTransferEntity(context);

        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            HasProperties(foreignKey, "FromAccountId") &&
            foreignKey.PrincipalEntityType.ClrType.Name == "Account" &&
            foreignKey.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            HasProperties(foreignKey, "ToAccountId") &&
            foreignKey.PrincipalEntityType.ClrType.Name == "Account" &&
            foreignKey.DeleteBehavior == DeleteBehavior.Restrict);

        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && HasProperties(index, "SubmissionToken"));
        Assert.Contains(entity.GetIndexes(), index => HasProperties(index, "IsDeleted", "TransferDate"));
        Assert.Contains(entity.GetIndexes(), index => HasProperties(index, "IsDeleted", "Currency", "TransferDate"));
        Assert.Contains(entity.GetIndexes(), index => HasProperties(index, "IsDeleted", "FromAccountId", "TransferDate"));
        Assert.Contains(entity.GetIndexes(), index => HasProperties(index, "IsDeleted", "ToAccountId", "TransferDate"));
    }

    [Fact]
    public async Task Migration_creates_account_transfers_table_and_history_entry()
    {
        await using var database = new SqliteTestDatabase();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());

        await context.Database.MigrateAsync();

        Assert.Contains("20260519000000_AddAccountTransfers", context.Database.GetAppliedMigrations());
        IReadOnlyDictionary<string, bool> columns = await GetTableColumnsAsync(context);
        Assert.True(columns["TransferDate"]);
        Assert.True(columns["Currency"]);
        Assert.True(columns["AmountMinorUnits"]);
        Assert.True(columns["FromAccountId"]);
        Assert.True(columns["ToAccountId"]);
        Assert.True(columns["SubmissionToken"]);
        Assert.True(columns["CreatedAtUtc"]);
        Assert.True(columns["UpdatedAtUtc"]);
        Assert.True(columns["IsDeleted"]);
        Assert.False(columns["DeletedAtUtc"]);
        Assert.False(columns["DeletionSummary"]);
        Assert.True(columns["LastChangeSummary"]);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType FindAccountTransferEntity(AppDbContext context)
    {
        IEntityType? entity = context.Model.FindEntityType(EntityName);
        Assert.NotNull(entity);
        return entity;
    }

    private static void AssertRequiredProperty<TProperty>(IEntityType entity, string name, int? maxLength = null)
    {
        IProperty property = Assert.IsAssignableFrom<IProperty>(entity.FindProperty(name));
        Assert.Equal(typeof(TProperty), property.ClrType);
        Assert.False(property.IsNullable);
        if (maxLength.HasValue)
        {
            Assert.Equal(maxLength.Value, property.GetMaxLength());
        }
    }

    private static void AssertOptionalProperty<TProperty>(IEntityType entity, string name, int? maxLength = null)
    {
        IProperty property = Assert.IsAssignableFrom<IProperty>(entity.FindProperty(name));
        Assert.Equal(typeof(TProperty), Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType);
        Assert.True(property.IsNullable);
        if (maxLength.HasValue)
        {
            Assert.Equal(maxLength.Value, property.GetMaxLength());
        }
    }

    private static bool HasProperties(IReadOnlyForeignKey foreignKey, params string[] propertyNames)
    {
        return foreignKey.Properties.Select(property => property.Name).SequenceEqual(propertyNames);
    }

    private static bool HasProperties(IReadOnlyIndex index, params string[] propertyNames)
    {
        return index.Properties.Select(property => property.Name).SequenceEqual(propertyNames);
    }

    private static async Task<IReadOnlyDictionary<string, bool>> GetTableColumnsAsync(AppDbContext context)
    {
        await using System.Data.Common.DbCommand command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA table_info('AccountTransfers');";

        var columns = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string columnName = reader.GetString(1);
            bool isRequired = reader.GetInt32(3) == 1;
            columns.Add(columnName, isRequired);
        }

        Assert.NotEmpty(columns);
        return columns;
    }
}
