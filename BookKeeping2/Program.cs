using System.Globalization;
using BookKeeping2.Data;
using BookKeeping2.Localization;
using BookKeeping2.Services.Accounts;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Budgets;
using BookKeeping2.Services.Categories;
using BookKeeping2.Services.Csv;
using BookKeeping2.Services.Reports;
using BookKeeping2.Services.Security;
using BookKeeping2.Services.Time;
using BookKeeping2.Services.Transactions;
using BookKeeping2.Validation;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2;

/// <summary>
/// Application entry point.
/// </summary>
public class Program
{
    /// <summary>
    /// Starts the Open BookKeeping web application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<BookKeepingDbOptions>(builder.Configuration.GetSection(BookKeepingDbOptions.SectionName));
        var dbOptions = builder.Configuration.GetSection(BookKeepingDbOptions.SectionName).Get<BookKeepingDbOptions>()
            ?? new BookKeepingDbOptions();
        string connectionString = builder.Configuration.GetConnectionString("BookKeepingDatabase")
            ?? dbOptions.ToConnectionString();

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddHostedService<DatabaseStartupService>();
        builder.Services.AddScoped<ITaipeiDateService, TaipeiDateService>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IBudgetService, BudgetService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ICsvExportService, CsvExportService>();
        builder.Services.AddScoped<ICsvImportService, CsvImportService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<TransactionFormOptionsService>();
        builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddSingleton<AuditLogMaskingPolicy>();
        builder.Services.AddSingleton<TextInputSanitizer>();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var formattingCulture = new CultureInfo(UiLanguageOptions.DefaultCultureName);
            CultureInfo[] supportedUiCultures =
            [
                new(UiLanguageOptions.DefaultUiCultureName),
                new(UiLanguageOptions.EnglishUiCultureName)
            ];

            options.DefaultRequestCulture = new RequestCulture(
                UiLanguageOptions.DefaultCultureName,
                UiLanguageOptions.DefaultUiCultureName);
            options.SupportedCultures = [formattingCulture];
            options.SupportedUICultures = supportedUiCultures;
            options.RequestCultureProviders = [new UiLanguageRequestCultureProvider()];
        });
        builder.Services
            .AddRazorPages()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource));
            });

        var app = builder.Build();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseBookKeepingSecurityHeaders();
        app.UseHttpsRedirection();

        app.UseRequestLocalization();

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }
}
