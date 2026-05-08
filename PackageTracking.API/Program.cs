using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PackageTracking.API.Data;
using PackageTracking.API.Middleware;
using PackageTracking.API.Repositories;
using PackageTracking.API.Services;
using PackageTracking.API.Swagger;
using Serilog;

namespace PackageTracking.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // -------------------------
            // Serilog Configuration
            // -------------------------
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            // -------------------------
            // Database Configuration (WITH RETRY)
            // -------------------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));

            // -------------------------
            // Dependency Injection
            // -------------------------
            builder.Services.AddScoped<IPackageRepository, PackageRepository>();
            builder.Services.AddScoped<IPackageService, PackageService>();

            // -------------------------
            // Controllers + Validation
            // -------------------------
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value is { Errors.Count: > 0 })
                            .Select(e => new
                            {
                                Field = e.Key,
                                Errors = e.Value!.Errors.Select(x => x.ErrorMessage)
                            });

                        IActionResult result = new BadRequestObjectResult(errors);
                        return result;
                    };
                });

            // -------------------------
            // CORS
            // -------------------------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader();
                });
            });

            // -------------------------
            // Swagger (Swashbuckle only — avoids conflicting with Microsoft.AspNetCore.OpenApi)
            // -------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Package Tracking API",
                    Version = "v1"
                });
                options.SchemaFilter<EnumAsStringSchemaFilter>();
            });

            var app = builder.Build();

            var listensForHttps = ListensForHttps(builder.Configuration);

            // SQL Server in Docker is often still starting when the API container runs; retry instead of exiting.
            ApplyMigrations(app);

            // -------------------------
            // Middleware Pipeline
            // -------------------------
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // HTTP-only Docker / Kestrel: redirecting to HTTPS breaks Swagger UI and browser fetch() to http://localhost:5001
            if (listensForHttps)
                app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll");

            var showSwagger = app.Environment.IsDevelopment()
                || string.Equals(
                    app.Configuration["ENABLE_SWAGGER"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

            if (showSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Package Tracking API v1");
                });
            }

            app.UseAuthorization();

            app.MapControllers();

            Log.Information(
                "Starting host. Environment={Environment}, Swagger={Swagger}, HttpsRedirection={HttpsRedirect}",
                app.Environment.EnvironmentName,
                showSwagger,
                listensForHttps);

            app.Run();
        }

        private static void ApplyMigrations(WebApplication app, int maxAttempts = 30, int delaySeconds = 2)
        {
            Exception? last = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                    Log.Information("Database migrations applied");
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt < maxAttempts)
                    {
                        Log.Warning(
                            ex,
                            "Database not ready; retrying migrations (attempt {Attempt}/{Max})",
                            attempt,
                            maxAttempts);
                        Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }

            Log.Fatal(last, "Could not apply database migrations");
            throw new InvalidOperationException(
                "Could not apply database migrations. Ensure SQL Server is reachable (Docker: wait for sqlserver, connection Server=sqlserver) or run sqlserver and use Server=localhost,1433 for local runs.",
                last);
        }

        private static bool ListensForHttps(IConfiguration configuration)
        {
            var urls = configuration["ASPNETCORE_URLS"] ?? string.Empty;
            if (urls.Contains("https://", StringComparison.OrdinalIgnoreCase))
                return true;

            var httpsPorts = configuration["ASPNETCORE_HTTPS_PORTS"];
            return !string.IsNullOrWhiteSpace(httpsPorts);
        }
    }
}