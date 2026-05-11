using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Security;
using BookKeeping2.Services.Time;
using BookKeeping2.Validation;
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

        string connectionString = builder.Configuration.GetConnectionString("BookKeepingDatabase")
            ?? "Data Source=App_Data/bookkeeping.db";

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddScoped<ITaipeiDateService, TaipeiDateService>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddSingleton<AuditLogMaskingPolicy>();
        builder.Services.AddSingleton<TextInputSanitizer>();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            DatabaseInitializer.InitializeAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseBookKeepingSecurityHeaders();
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }
}
