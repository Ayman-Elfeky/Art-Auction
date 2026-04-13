using ArtAuction.Application.Common.Interfaces;

namespace ArtAuction.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // TODO: Implement Azure Blob Storage upload
        // For now, return a mock URL
        await Task.Delay(100);
        return $"https://example.blob.core.windows.net/artworks/{fileName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        // TODO: Implement Azure Blob Storage delete
        await Task.Delay(100);
    }
}
