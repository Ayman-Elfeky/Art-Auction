using ArtAuction.Domain.Enums;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Services;

public class AuctionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public AuctionBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndEndAuctionsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in AuctionBackgroundService: {ex.Message}");
            }
        }
    }

    private async Task CheckAndEndAuctionsAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var endedAuctions = await context.Artworks
                .Where(a => a.Status == ArtworkStatus.Active && a.AuctionEndTime <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var artwork in endedAuctions)
            {
                artwork.Status = ArtworkStatus.Ended;
                artwork.UpdatedAt = DateTime.UtcNow;
            }

            if (endedAuctions.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
