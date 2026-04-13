namespace ArtAuction.Application.Features.Admin.DTOs
{
    public class PendingArtworkDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal InitialPrice { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}