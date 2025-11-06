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
        }
    }
}
