using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackageTracking.API.Data;
using PackageTracking.API.Middleware;
using PackageTracking.API.Repositories;
using PackageTracking.API.Services;
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
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value.Errors.Count > 0)
                            .Select(e => new
                            {
                                Field = e.Key,
                                Errors = e.Value.Errors.Select(x => x.ErrorMessage)
                            });

                        return new BadRequestObjectResult(errors);
                    };
                });

            // -------------------------
            // Swagger / OpenAPI
            // -------------------------
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // -------------------------
            // ?? AUTO APPLY MIGRATIONS (IMPORTANT)
            // -------------------------
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // -------------------------
            // Middleware Pipeline
            // -------------------------
            app.UseMiddleware<ErrorHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}