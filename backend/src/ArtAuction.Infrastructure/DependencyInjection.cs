using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using ArtAuction.Application.Common.Interfaces;
// using ArtAuction.Infrastructure.Persistence;
// using ArtAuction.Infrastructure.Services;

namespace ArtAuction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // services.AddScoped<IApplicationDbContext>(provider =>
        //     provider.GetRequiredService<ApplicationDbContext>());

        // // Services
        // services.AddScoped<IJwtService, JwtService>();
        // services.AddScoped<IFileStorageService, AZUREStorageService>();
        // services.AddScoped<INotificationService, NotificationService>();
        // services.AddScoped<IAuctionHubService, AuctionHubService>();

        // // Background service to auto-end auctions
        // services.AddHostedService<AuctionBackgroundService>();

        return services;
    }
}