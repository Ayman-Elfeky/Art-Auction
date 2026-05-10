# Art Auction Platform Implementation Report

## 1) Authentication, Password Security, Authorization, RBAC

### Password Security (Salt + Hash)
- Implemented via BCrypt in register/login handlers:
  - `backend/src/Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs`
  - `backend/src/Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- BCrypt includes per-password salt as part of the stored hash string (`PasswordHash` column).

### Authentication Mechanism
- JWT bearer authentication configured in:
  - `backend/src/API/Program.cs`
  - `backend/src/Infrastructure/Security/JwtService.cs`
- SignalR JWT support via query token (`access_token`) added for hub connections.

### Authorization and RBAC
- Role-based endpoint protection added:
  - Admin endpoints: `[Authorize(Roles = "Admin")]`
  - Artist actions: `[Authorize(Roles = "Artist")]`
  - Buyer bidding/watchlist: `[Authorize(Roles = "Buyer")]`
- A reusable user context accessor was added:
  - `backend/src/API/Services/CurrentUserContext.cs`

### User Permissions Management
- Permission claims added to JWT by role:
  - `backend/src/Infrastructure/Security/JwtService.cs`
- Policies registered and enforced:
  - `backend/src/API/Program.cs`
  - Permissions constants:
    - `backend/src/Domain/Authorization/Permissions.cs`

## 2) Auction Core Logic

### Admin approval flows
- Existing account/artwork approval workflows retained and protected by authorization:
  - `backend/src/veldgenerated/Controllers/AdminController.cs`
  - `backend/src/API/Services/AdminService.cs`

### Artwork browsing/filtering
- Existing filters by artist/category/tag are used.
- Public browsing now excludes pending/rejected by default:
  - `backend/src/Application/Features/Artwork/Queries/GetArtworks/GetArtworksQueryHandler.cs`

### Bidding rules
- Enforced in:
  - `backend/src/API/Services/BidService.cs`
- Rules applied:
  - Buyer must be authenticated and active.
  - Artwork must be in a biddable state.
  - Current time must be between auction start/end.
  - Minimum increment: new bid must be at least `$10` above the previous bid.
  - If first bid: must be at least initial price.

### Watchlist
- Added buyer watchlist APIs and service:
  - `backend/src/API/Controllers/WatchlistController.cs`
  - `backend/src/API/Services/WatchlistService.cs`

### Bid history for visitors
- Added visitor-readable bid history endpoint with bidder name, price, timestamp:
  - `backend/src/API/Controllers/BidHistoryController.cs`

### Winner determination and notifications
- Background auction lifecycle worker added:
  - `backend/src/API/Services/AuctionLifecycleService.cs`
- Automatically:
  - Activates approved auctions when start time arrives.
  - Ends auctions after end time.
  - Determines highest bid winner.
  - Sends winner notification.

## 3) Real-time (SignalR)

- Added SignalR hub:
  - `backend/src/API/Hubs/AuctionHub.cs`
- Registered in app startup:
  - `backend/src/API/Program.cs`
- Real-time events:
  - `bid.placed` broadcast to artwork group.
  - `notification.received` broadcast to user group.

## 4) Notification persistence

- Implemented DB-backed notification service:
  - `backend/src/API/Services/NotificationService.cs`
- Added API to fetch current user notifications:
  - `backend/src/API/Controllers/NotificationsController.cs`
  - `backend/src/API/Services/NotificationQueryService.cs`

## 5) Database / PostgreSQL

- PostgreSQL EF Core setup is active and verified through runtime logs.
- DB schema HTML artifact added:
  - `backend/docs/db-schema.html`

## 6) Commands to run and validate

From project root:

```powershell
cd backend/src/API
dotnet build
dotnet run
```

Check migrations and DB update state:

```powershell
cd backend/src/API
dotnet ef migrations list --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```

Check PostgreSQL data directly:

```powershell
psql -h localhost -p 5432 -U postgres -d artauctiondb
```

Example SQL checks:

```sql
\dt
SELECT "Email","Role","IsApproved" FROM "Users";
SELECT "Title","Status","AuctionStartTime","AuctionEndTime" FROM "Artworks";
SELECT "ArtworkId","BuyerId","Amount","PlacedAt" FROM "Bids" ORDER BY "PlacedAt" DESC LIMIT 20;
SELECT "UserId","Title","Type","CreatedAt" FROM "Notifications" ORDER BY "CreatedAt" DESC LIMIT 20;
```
