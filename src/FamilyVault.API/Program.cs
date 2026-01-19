using FamilyVault.API.EndPoints;
using FamilyVault.Application;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace FamilyVault.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // -------------------- JWT --------------------
        var jWTSettingsSection = builder.Configuration.GetSection("Jwt");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jWTSettingsSection["Issuer"],
                    ValidAudience = jWTSettingsSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jWTSettingsSection["Key"] ?? string.Empty)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

        builder.Services.AddAuthorization();
        // -------------------- CORS --------------------
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var allowedMethods = builder.Configuration
            .GetSection("Cors:AllowedMethods")
            .Get<string[]>() ?? Array.Empty<string>();

        builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods(allowedMethods)
                          .AllowAnyHeader();
                });
            });

        // -------------------- FluentValidation --------------------
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();
        builder.Services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

        // -------------------- Application & Infrastructure --------------------
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddScoped<IUserService, Userservice>();
        builder.Services.AddScoped<IFamilymemeberService, FamilyMemberService>();
        builder.Services.AddScoped<IFamilyService, FamilyService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<ICryptoService, CryptoService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // -------------------- AutoMapper --------------------
        builder.Services.AddAutoMapper(cfg =>
        {
            // optional custom configuration
        }, typeof(ApplicationAssemblyMarker).Assembly);


        // -------------------- Swagger (Minimal API) --------------------
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // 🔐 Bearer Token Configuration
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            });

//            options.AddSecurityRequirement(new OpenApiSecurityRequirement
//            {
//                {
//                    new OpenApiSecurityScheme
//                    {
//                      Name = "Bearer",
//                      Scheme = "oauth2",
//Reference = new OpenApiReference
//                      {
//                          Type = ReferenceType.SecurityScheme,
//                          Id = "Bearer"
//                      },
//                      In = ParameterLocation.Header
//                    },
//                    Array.Empty<string>()
//                }
//            });
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "HH:mm:ss ";
            options.SingleLine = true;
            options.IncludeScopes = true; // shows scope values like TraceId/RequestId
        });

        builder.Services.AddMemoryCache();


        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("FamilyVaultAPI"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("FamilyVaultAPI"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());


        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? "no-trace";
            using (context.RequestServices.GetRequiredService<ILoggerFactory>()
                   .CreateLogger("Scope")
                   .BeginScope(new Dictionary<string, object>
                   {
                       ["RequestId"] = context.TraceIdentifier,
                       ["TraceId"] = traceId
                   }))
            {
                await next();
            }

        });
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogInformation("App starting");

        // ============================================================
        // Middleware Pipeline (ORDER MATTERS)
        // ============================================================

        // Global exception handling (FIRST)
        app.UseMiddleware<MiddlewearGlobalExceaption>();

        // HTTPS redirection
        app.UseHttpsRedirection();

        // CORS (before endpoints)
        app.UseCors("CorsPolicy");

        // Swagger (Dev only)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // Authentication and Authorization 
            app.UseAuthentication();
            app.UseAuthorization();
        }

        // Map Minimal API endpoints (LAST)
        app.MapAllEndpoints();

        app.Run();
    }
}
