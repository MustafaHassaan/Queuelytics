using Application.Commands.Auth;
using Application.Interfaces;
using Application.Security;
using ConsumerService.Analytics;
using Infrastructure.Dataaccess;
using Infrastructure.Messaging;
using Infrastructure.Repository;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProducerService.Analytics;
using StackExchange.Redis;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================
            // 1️- CONFIGURATION & SERVICES
            // ==========================

            // Load configuration
            var config = builder.Configuration;

            // Database
            builder.Services.AddDbContext<QualyticsDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null
                        );
                    }));

            // ==========================
            // 2️- DEPENDENCY INJECTION
            // ==========================
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var redisConnection = config.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(redisConnection);
            });
            builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
            builder.Services.AddScoped<IJwtProvider, JwtProvider>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            // MediatR (Application Layer)
            builder.Services.AddMediatR(cfg =>
                  cfg.RegisterServicesFromAssembly(typeof(IUnitOfWork).Assembly));

            // ===================================
            // 3️- AUTHENTICATION (JWT) & RabbitMQ
            // ===================================

            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(config["Jwt:Key"])
            );
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = config["Jwt:Issuer"],
                                    ValidAudience = config["Jwt:Audience"],
                                    IssuerSigningKey = key
                                };
                            });
            // ==========================
            // 4️- MVC + SWAGGER
            // ==========================
            builder.Services.AddAuthorization();
            builder.Services.AddHostedService<Workerproducer>();
            builder.Services.AddHostedService<Workerconsumer>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Qualytics API",
                    Version = "v1"
                });

                // 🔐 تعريف التوكن
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "أدخل التوكن بهذا الشكل: Bearer {token}"
                });

                // 🔐 ربط التوكن بكل العمليات
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            // ==========================
            // 5️- BUILD APP
            // ==========================

            var app = builder.Build();

            // ==========================
            // 6️- MIDDLEWARE PIPELINE
            // ==========================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseMiddleware<RedisTokenValidation>();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            // =====================================
            // Migration Helper Function
            // =====================================
            static async Task ApplyMigrationsAsync(IServiceProvider services)
            {
                const int maxRetries = 30;
                const int delaySeconds = 3;

                Console.WriteLine("========================================");
                Console.WriteLine("🔄 Starting Database Migration Process");
                Console.WriteLine("========================================");

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        using var scope = services.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<QualyticsDbContext>();

                        Console.WriteLine($"[{attempt}/{maxRetries}] 🔍 Checking database connection...");

                        // Test connection
                        var canConnect = await context.Database.CanConnectAsync();

                        if (canConnect)
                        {
                            Console.WriteLine($"[{attempt}/{maxRetries}] ✅ Database connection successful!");

                            // Get pending migrations
                            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                            var pendingCount = pendingMigrations.Count();

                            if (pendingCount > 0)
                            {
                                Console.WriteLine($"[{attempt}/{maxRetries}] 🔄 Found {pendingCount} pending migration(s)");
                                Console.WriteLine($"[{attempt}/{maxRetries}] 🔄 Applying migrations...");

                                await context.Database.MigrateAsync();

                                Console.WriteLine($"[{attempt}/{maxRetries}] ✅ Migrations applied successfully!");
                            }
                            else
                            {
                                Console.WriteLine($"[{attempt}/{maxRetries}] ✅ Database is up to date (no pending migrations)");
                            }

                            Console.WriteLine("========================================");
                            Console.WriteLine("✅ Migration Process Completed");
                            Console.WriteLine("========================================");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"[{attempt}/{maxRetries}] ⚠️  Cannot connect to database");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{attempt}/{maxRetries}] ❌ Error: {ex.Message}");

                        if (attempt == maxRetries)
                        {
                            Console.WriteLine("========================================");
                            Console.WriteLine("❌ Migration Failed After All Retries");
                            Console.WriteLine("⚠️  Application will start without migrations");
                            Console.WriteLine("========================================");
                            return;
                        }
                    }

                    Console.WriteLine($"[{attempt}/{maxRetries}] ⏳ Retrying in {delaySeconds} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
    }
}
