using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyVault.Infrastructure
{
    /// <summary>
    /// Represents DependencyInjection.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Performs the AddInfrastructureServices operation.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();

            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            return services;
        }
    }
}
