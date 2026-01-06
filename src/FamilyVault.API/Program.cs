using FamilyVault.API.EndPoints;
using FamilyVault.Application;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Application.Validators.Document;
using FamilyVault.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;

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

        // Register swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var app = builder.Build();

        app.UseMiddleware<MiddlewearGlobalExceaption>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapAllEndpoints();

        app.Run();
    }
}
