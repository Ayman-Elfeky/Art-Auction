using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public sealed class AuctionLifecycleService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionLifecycleService> _logger;

    public AuctionLifecycleService(IServiceScopeFactory scopeFactory, ILogger<AuctionLifecycleService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process auction lifecycle.");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private async Task ProcessAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var now = DateTime.UtcNow;

        var activateCandidates = await dbContext.Artworks
            .Where(a =>
                a.Status == ArtworkStatus.Approved &&
                a.AuctionStartTime <= now &&
                a.AuctionEndTime > now)
            .ToListAsync(cancellationToken);

        foreach (var artwork in activateCandidates)
        {
            artwork.Status = ArtworkStatus.Active;
        }

        var endedCandidates = await dbContext.Artworks
            .Where(a =>
                (a.Status == ArtworkStatus.Active || a.Status == ArtworkStatus.Approved) &&
                a.AuctionEndTime <= now)
            .ToListAsync(cancellationToken);

        foreach (var artwork in endedCandidates)
        {
            artwork.Status = ArtworkStatus.Ended;

            var winningBid = await dbContext.Bids
                .Where(b => b.ArtworkId == artwork.Id)
                .OrderByDescending(b => b.Amount)
                .ThenByDescending(b => b.PlacedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (winningBid is null)
            {
                continue;
            }

            await notificationService.SendNotificationAsync(
                winningBid.BuyerId,
                "Auction won",
                $"You won '{artwork.Title}' with bid ${winningBid.Amount:F2}.",
                NotificationType.AuctionWon,
                artwork.Id);
        }

        if (activateCandidates.Count > 0 || endedCandidates.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
