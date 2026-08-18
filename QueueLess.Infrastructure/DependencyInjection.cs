using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QueueLess.Application.Interfaces;
using QueueLess.Infrastructure.Identity;
using QueueLess.Infrastructure.Persistence;

namespace QueueLess.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        //DbContext Setup
        services.AddDbContext<QlDbContext>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("QueueLess.Infrastructure")));
        services.AddScoped<IQlDbContext>(provider => provider.GetRequiredService<QlDbContext>());

        //Identity Configuration
        services.AddIdentity<ApplicationUser, IdentityRole>(Options => 
        {
            Options.Password.RequireDigit = true;
            Options.Password.RequireLowercase = true;
            Options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<QlDbContext>()
        .AddDefaultTokenProviders();

        //Concrete mappings
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}