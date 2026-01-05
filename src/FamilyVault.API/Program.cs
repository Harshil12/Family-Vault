using FamilyVault.API.EndPoints.Document;
using FamilyVault.Application;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Application.Validators.Document;
using FamilyVault.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace FamilyVault.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();

        builder.Services.AddValidatorsFromAssemblyContaining<CreateDocumentValidators>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //builder.Services.AddOpenApi();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddScoped<IUserService, Userservice>();
        builder.Services.AddScoped<IFamilymemeberService, FamilyMemberService>();
        builder.Services.AddScoped<IFamilyService, FamilyService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();


        builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker));

        var app = builder.Build();

        app.UseMiddleware<MiddlewearGlobalExceaption>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        //app.MapControllers();

        //app.MapGet("/test", () =>
        //{
        //    var response = ApiResponse<string>.Success("Service is working fine.", "Wow");

        //    return Results.Ok(response); // HTTP 200
        //});

        app.MapDocumentEndPoints(); 

        app.Run();
    }
}
