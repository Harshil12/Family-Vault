
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Infrastructure;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //builder.Services.AddOpenApi();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddScoped<IUserService, Userservice>();
        builder.Services.AddScoped<IFamilymemeberService, FamilyMemberService>();
        builder.Services.AddScoped<IFamilyService, FamilyService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
           // app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
