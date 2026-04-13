using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;
using ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;
using ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;
using ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworks;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VeldGenerated.Services;
using ArtworkModel = VeldGenerated.Models.Artwork;
using ArtworkDetailModel = VeldGenerated.Models.ArtworkDetail;
using PagedArtworkModel = VeldGenerated.Models.PagedArtwork;
using CreateArtworkInputModel = VeldGenerated.Models.CreateArtworkInput;
using ExtendAuctionTimeInputModel = VeldGenerated.Models.ExtendAuctionTimeInput;
using AdminActionMessageModel = VeldGenerated.Models.AdminActionMessage;

namespace Api.Services;

public class ArtworksService : IArtworksService
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArtworksService(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedArtworkModel> GetArtworks(Dictionary<string, string> query)
    {
        var filter = new ArtworkFilterParams
        {
            ArtistName = GetValue(query, "artistName"),
            CategoryName = GetValue(query, "categoryName"),
            TagName = GetValue(query, "tagName"),
            Status = GetValue(query, "status"),
            PageNumber = ParseInt(query, "pageNumber", 1),
            PageSize = ParseInt(query, "pageSize", 10)
        };

        var result = await _mediator.Send(new GetArtworksQuery(filter));
        return MapPaged(result);
    }

    public async Task<ArtworkDetailModel> GetArtworkById(string Id)
    {
        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var result = await _mediator.Send(new GetArtworkByIdQuery(artworkId));
        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        return MapArtworkDetail(result.Data);
    }

    public async Task<PagedArtworkModel> GetArtworksByArtist(string ArtistId, Dictionary<string, string> query)
    {
        if (!Guid.TryParse(ArtistId, out var artistId))
        {
            throw new InvalidOperationException("Invalid artist id.");
        }

        var pagination = new PaginationParams
        {
            PageNumber = ParseInt(query, "pageNumber", 1),
            PageSize = ParseInt(query, "pageSize", 10)
        };

        var result = await _mediator.Send(new GetArtworksByArtistQuery(artistId, pagination));
        return MapPaged(result);
    }

    public async Task<ArtworkModel> CreateArtwork(CreateArtworkInputModel input)
    {
        var currentUserId = GetCurrentUserId();

        var command = new CreateArtworkCommand(new CreateArtworkDto
        {
            Title = input.Title,
            Description = input.Description,
            InitialPrice = (decimal)input.InitialPrice,
            BuyNowPrice = input.BuyNowPrice.HasValue ? (decimal)input.BuyNowPrice.Value : null,
            AuctionStartTime = input.AuctionStartTime,
            AuctionEndTime = input.AuctionEndTime,
            CategoryName = input.CategoryName,
            Tags = input.Tags ?? [],
            ImageUrl = input.ImageUrl
        }, currentUserId);

        var result = await _mediator.Send(command);
        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        return MapArtwork(result.Data);
    }

    public async Task<ArtworkModel> UpdateArtwork(string Id, CreateArtworkInputModel input)
    {
        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var currentUserId = GetCurrentUserId();

        var command = new UpdateArtworkCommand(artworkId, new CreateArtworkDto
        {
            Title = input.Title,
            Description = input.Description,
            InitialPrice = (decimal)input.InitialPrice,
            BuyNowPrice = input.BuyNowPrice.HasValue ? (decimal)input.BuyNowPrice.Value : null,
            AuctionStartTime = input.AuctionStartTime,
            AuctionEndTime = input.AuctionEndTime,
            CategoryName = input.CategoryName,
            Tags = input.Tags ?? [],
            ImageUrl = input.ImageUrl
        }, currentUserId);

        var result = await _mediator.Send(command);
        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        return MapArtwork(result.Data);
    }

    public async Task<AdminActionMessageModel> DeleteArtwork(string Id)
    {
        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new DeleteArtworkCommand(artworkId, currentUserId));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        return new AdminActionMessageModel("Artwork deleted.");
    }

    public async Task<ArtworkModel> ExtendAuctionTime(string Id, ExtendAuctionTimeInputModel input)
    {
        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(
            new ExtendAuctionTimeCommand(artworkId, input.NewEndTime, currentUserId));

        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        return MapArtwork(result.Data);
    }

    private static string? GetValue(Dictionary<string, string> query, string key)
    {
        return query.TryGetValue(key, out var value) ? value : null;
    }

    private static int ParseInt(Dictionary<string, string> query, string key, int fallback)
    {
        return query.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : fallback;
    }

    private Guid GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context found.");
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Missing bearer token.");
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(sub, out var userId))
        {
            throw new InvalidOperationException("Invalid token subject.");
        }

        return userId;
    }

    private static ArtworkModel MapArtwork(ArtworkDto dto)
    {
        return new ArtworkModel(
            dto.Id,
            dto.Title,
            dto.ArtistName,
            dto.CategoryName,
            (double)dto.InitialPrice,
            (double)dto.CurrentBid,
            dto.AuctionEndTime,
            dto.Status,
            dto.ImageUrl);
    }

    private static ArtworkDetailModel MapArtworkDetail(ArtworkDetailDto dto)
    {
        return new ArtworkDetailModel(
            dto.Id,
            dto.Title,
            dto.Description,
            dto.ArtistName,
            dto.ArtistId,
            (double)dto.InitialPrice,
            dto.BuyNowPrice.HasValue ? (double)dto.BuyNowPrice.Value : null,
            (double)dto.CurrentBid,
            dto.AuctionStartTime,
            dto.AuctionEndTime,
            dto.Status,
            dto.CategoryName,
            dto.Tags,
            dto.TotalBids,
            dto.ImageUrl,
            dto.CreatedAt);
    }

    private static PagedArtworkModel MapPaged(PagedResult<ArtworkDto> paged)
    {
        var totalPages = paged.PageSize == 0 ? 0 : (int)Math.Ceiling((double)paged.TotalCount / paged.PageSize);
        return new PagedArtworkModel(
            paged.Items.Select(MapArtwork).ToList(),
            paged.TotalCount,
            paged.PageNumber,
            paged.PageSize,
            totalPages,
            paged.PageNumber < totalPages,
            paged.PageNumber > 1);
    }
}
