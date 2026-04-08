Responsibility:
  - Full artwork CRUD (Create, Read, Update, Delete)
  - Browse with filters (artist, category, tag)
  - Pagination
  - Extend auction end time
  - ArtworksController

Files you own:
  ├── Application/Features/Artworks/
  │   ├── Commands/
  │   │   ├── CreateArtwork/
  │   │   │   ├── CreateArtworkCommand.cs
  │   │   │   ├── CreateArtworkCommandHandler.cs
  │   │   │   └── CreateArtworkCommandValidator.cs
  │   │   ├── UpdateArtwork/
  │   │   │   ├── UpdateArtworkCommand.cs
  │   │   │   ├── UpdateArtworkCommandHandler.cs
  │   │   │   └── UpdateArtworkCommandValidator.cs
  │   │   ├── DeleteArtwork/
  │   │   │   ├── DeleteArtworkCommand.cs
  │   │   │   └── DeleteArtworkCommandHandler.cs
  │   │   └── ExtendAuctionTime/
  │   │       ├── ExtendAuctionTimeCommand.cs
  │   │       ├── ExtendAuctionTimeCommandHandler.cs
  │   │       └── ExtendAuctionTimeCommandValidator.cs
  │   ├── Queries/
  │   │   ├── GetArtworks/
  │   │   │   ├── GetArtworksQuery.cs
  │   │   │   ├── GetArtworksQueryHandler.cs
  │   │   │   └── ArtworkFilterParams.cs
  │   │   ├── GetArtworkById/
  │   │   │   ├── GetArtworkByIdQuery.cs
  │   │   │   └── GetArtworkByIdQueryHandler.cs
  │   │   └── GetArtworksByArtist/
  │   │       ├── GetArtworksByArtistQuery.cs
  │   │       └── GetArtworksByArtistQueryHandler.cs
  │   └── DTOs/
  │       ├── ArtworkDto.cs
  │       ├── ArtworkDetailDto.cs
  │       └── CreateArtworkDto.cs
  │
  └── API/Controllers/
      └── ArtworksController.cs

Endpoints to implement:
  GET    /api/artworks              ← public, supports filters
  GET    /api/artworks/{id}         ← public
  POST   /api/artworks              ← Artist only
  PUT    /api/artworks/{id}         ← Artist only (owner)
  DELETE /api/artworks/{id}         ← Artist only (owner)
  PUT    /api/artworks/{id}/extend  ← Artist only (owner)

Business Rules:
  ✅ New artworks go to Pending status
  ✅ Cannot update/delete active auction
  ✅ Cannot delete active auction
  ✅ Extend only works on active auctions
  ✅ New end time must be after current end time
  ✅ Filter by artist name, category, tag
  ✅ Results paginated

Estimated time: 2–3 days