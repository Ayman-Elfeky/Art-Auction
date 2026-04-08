Responsibility:
  - Approve / reject artist accounts
  - Approve / reject artwork posts
  - Get pending artists list
  - Get pending artworks list
  - AdminController

Files you own:
  ├── Application/Features/Admin/
  │   ├── Commands/
  │   │   ├── ApproveArtistAccount/
  │   │   │   ├── ApproveArtistAccountCommand.cs
  │   │   │   └── ApproveArtistAccountCommandHandler.cs
  │   │   ├── RejectArtistAccount/
  │   │   │   ├── RejectArtistAccountCommand.cs
  │   │   │   └── RejectArtistAccountCommandHandler.cs
  │   │   ├── ApproveArtwork/
  │   │   │   ├── ApproveArtworkCommand.cs
  │   │   │   └── ApproveArtworkCommandHandler.cs
  │   │   └── RejectArtwork/
  │   │       ├── RejectArtworkCommand.cs
  │   │       └── RejectArtworkCommandHandler.cs
  │   ├── Queries/
  │   │   ├── GetPendingArtists/
  │   │   │   ├── GetPendingArtistsQuery.cs
  │   │   │   └── GetPendingArtistsQueryHandler.cs
  │   │   └── GetPendingArtworks/
  │   │       ├── GetPendingArtworksQuery.cs
  │   │       └── GetPendingArtworksQueryHandler.cs
  │   └── DTOs/
  │       ├── PendingArtistDto.cs
  │       └── PendingArtworkDto.cs
  │
  └── API/Controllers/
      └── AdminController.cs

Endpoints to implement:
  GET /api/admin/artists/pending
  PUT /api/admin/artists/{id}/approve
  PUT /api/admin/artists/{id}/reject
  GET /api/admin/artworks/pending
  PUT /api/admin/artworks/{id}/approve
  PUT /api/admin/artworks/{id}/reject

Business Rules:
  ✅ All endpoints require Admin role
  ✅ Approving artist → sets IsApproved = true
  ✅ Rejecting artist → sends notification
  ✅ Approving artwork → status becomes Approved
  ✅ Rejecting artwork → status becomes Rejected
  ✅ Both approval/rejection → notify the artist

Estimated time: 1–2 days