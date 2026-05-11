using BookKeeping2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BookKeeping2.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    /// <inheritdoc />
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.3");

        modelBuilder.Entity("BookKeeping2.Models.Accounts.Account", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");
            b.Property<string>("Currency").IsRequired().HasMaxLength(3).HasColumnType("TEXT");
            b.Property<int>("DisplayOrder").HasColumnType("INTEGER");
            b.Property<string>("IconKey").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
            b.Property<bool>("IsArchived").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("NormalizedName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<long>("OpeningBalanceMinorUnits").HasColumnType("INTEGER");
            b.Property<int>("Type").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("IsArchived", "DisplayOrder");
            b.HasIndex("NormalizedName").IsUnique();
            b.ToTable("Accounts");
        });

        modelBuilder.Entity("BookKeeping2.Models.Settings.AppSetting", b =>
        {
            b.Property<string>("Key").HasMaxLength(100).HasColumnType("TEXT");
            b.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnType("TEXT");
            b.Property<string>("Value").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            b.HasKey("Key");
            b.ToTable("AppSettings");
        });

        modelBuilder.Entity("BookKeeping2.Models.Audit.AuditEvent", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("CorrelationId").HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("EntityId").HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("EntityType").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<int>("EventType").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("OccurredAtUtc").HasColumnType("TEXT");
            b.Property<string>("Severity").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
            b.Property<string>("Summary").IsRequired().HasMaxLength(1000).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("EntityType", "EntityId");
            b.HasIndex("EventType", "OccurredAtUtc");
            b.ToTable("AuditEvents");
        });

        modelBuilder.Entity("BookKeeping2.Models.Categories.Category", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");
            b.Property<int>("DisplayOrder").HasColumnType("INTEGER");
            b.Property<string>("IconKey").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
            b.Property<bool>("IsArchived").HasColumnType("INTEGER");
            b.Property<bool>("IsDefault").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("NormalizedName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<int>("Type").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Type", "IsArchived", "DisplayOrder");
            b.HasIndex("Type", "NormalizedName").IsUnique();
            b.ToTable("Categories");
        });

        modelBuilder.Entity("BookKeeping2.Models.CsvImports.CsvImportBatch", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<int>("CreatedCategoryCount").HasColumnType("INTEGER");
            b.Property<int>("FailedRows").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("ImportedAtUtc").HasColumnType("TEXT");
            b.Property<string>("OriginalFileName").IsRequired().HasMaxLength(255).HasColumnType("TEXT");
            b.Property<int>("SucceededRows").HasColumnType("INTEGER");
            b.Property<string>("Summary").IsRequired().HasMaxLength(1000).HasColumnType("TEXT");
            b.Property<int>("TotalRows").HasColumnType("INTEGER");
            b.HasKey("Id");
            b.ToTable("CsvImportBatches");
        });

        modelBuilder.Entity("BookKeeping2.Models.Budgets.Budget", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<long>("AmountMinorUnits").HasColumnType("INTEGER");
            b.Property<DateOnly>("BudgetMonth").HasColumnType("TEXT");
            b.Property<long>("CategoryId").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");
            b.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CategoryId", "BudgetMonth").IsUnique();
            b.ToTable("Budgets");
        });

        modelBuilder.Entity("BookKeeping2.Models.CsvImports.CsvImportError", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<long>("CsvImportBatchId").HasColumnType("INTEGER");
            b.Property<string>("FieldName").HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("RawValuePreview").HasMaxLength(200).HasColumnType("TEXT");
            b.Property<string>("Reason").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            b.Property<int>("RowNumber").HasColumnType("INTEGER");
            b.HasKey("Id");
            b.HasIndex("CsvImportBatchId", "RowNumber");
            b.ToTable("CsvImportErrors");
        });

        modelBuilder.Entity("BookKeeping2.Models.Transactions.Transaction", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<long>("AccountId").HasColumnType("INTEGER");
            b.Property<long>("AmountMinorUnits").HasColumnType("INTEGER");
            b.Property<long>("CategoryId").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");
            b.Property<DateTimeOffset?>("DeletedAtUtc").HasColumnType("TEXT");
            b.Property<string>("DeletionSummary").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<bool>("IsDeleted").HasColumnType("INTEGER");
            b.Property<string>("LastChangeSummary").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            b.Property<string>("Note").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<DateOnly>("TransactionDate").HasColumnType("TEXT");
            b.Property<int>("Type").HasColumnType("INTEGER");
            b.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("AccountId");
            b.HasIndex("CategoryId");
            b.HasIndex("IsDeleted", "AccountId", "TransactionDate");
            b.HasIndex("IsDeleted", "AmountMinorUnits");
            b.HasIndex("IsDeleted", "CategoryId", "TransactionDate");
            b.HasIndex("IsDeleted", "TransactionDate");
            b.ToTable("Transactions");
        });

        modelBuilder.Entity("BookKeeping2.Models.Budgets.Budget", b =>
        {
            b.HasOne("BookKeeping2.Models.Categories.Category", "Category")
                .WithMany()
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Category");
        });

        modelBuilder.Entity("BookKeeping2.Models.CsvImports.CsvImportError", b =>
        {
            b.HasOne("BookKeeping2.Models.CsvImports.CsvImportBatch", "CsvImportBatch")
                .WithMany("Errors")
                .HasForeignKey("CsvImportBatchId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("CsvImportBatch");
        });

        modelBuilder.Entity("BookKeeping2.Models.Transactions.Transaction", b =>
        {
            b.HasOne("BookKeeping2.Models.Accounts.Account", "Account")
                .WithMany("Transactions")
                .HasForeignKey("AccountId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("BookKeeping2.Models.Categories.Category", "Category")
                .WithMany("Transactions")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Account");
            b.Navigation("Category");
        });

        modelBuilder.Entity("BookKeeping2.Models.Accounts.Account", b => b.Navigation("Transactions"));
        modelBuilder.Entity("BookKeeping2.Models.Categories.Category", b => b.Navigation("Transactions"));
        modelBuilder.Entity("BookKeeping2.Models.CsvImports.CsvImportBatch", b => b.Navigation("Errors"));
#pragma warning restore 612, 618
    }
}
