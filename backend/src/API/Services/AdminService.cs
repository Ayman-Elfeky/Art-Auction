using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class AdminService : IAdminService
{
    public Task<List<PendingArtist>> GetPendingArtists()
    {
        throw new NotImplementedException("Implement database-backed pending artist list.");
    }

    public Task<AdminActionMessage> ApproveArtist(string Id)
    {
        throw new NotImplementedException("Implement database-backed artist approval.");
    }

    public Task<AdminActionMessage> RejectArtist(string Id)
    {
        throw new NotImplementedException("Implement database-backed artist rejection.");
    }

    public Task<List<PendingArtwork>> GetPendingArtworks()
    {
        throw new NotImplementedException("Implement database-backed pending artwork list.");
    }

    public Task<AdminActionMessage> ApproveArtwork(string Id)
    {
        throw new NotImplementedException("Implement database-backed artwork approval.");
    }

    public Task<AdminActionMessage> RejectArtwork(string Id)
    {
        throw new NotImplementedException("Implement database-backed artwork rejection.");
    }
}
