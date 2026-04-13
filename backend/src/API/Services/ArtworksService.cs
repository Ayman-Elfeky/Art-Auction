using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class ArtworksService : IArtworksService
{
    public Task<PagedArtwork> GetArtworks(Dictionary<string, string> query)
    {
        throw new NotImplementedException("Implement database-backed artwork listing.");
    }

    public Task<ArtworkDetail> GetArtworkById(string Id)
    {
        throw new NotImplementedException("Implement database-backed artwork lookup.");
    }

    public Task<PagedArtwork> GetArtworksByArtist(string ArtistId, Dictionary<string, string> query)
    {
        throw new NotImplementedException("Implement database-backed artist artwork listing.");
    }

    public Task<Artwork> CreateArtwork(CreateArtworkInput input)
    {
        throw new NotImplementedException("Implement database-backed artwork creation.");
    }

    public Task<Artwork> UpdateArtwork(string Id, CreateArtworkInput input)
    {
        throw new NotImplementedException("Implement database-backed artwork update.");
    }

    public Task<AdminActionMessage> DeleteArtwork(string Id)
    {
        throw new NotImplementedException("Implement database-backed artwork deletion.");
    }

    public Task<Artwork> ExtendAuctionTime(string Id, ExtendAuctionTimeInput input)
    {
        throw new NotImplementedException("Implement database-backed auction extension.");
    }
}
