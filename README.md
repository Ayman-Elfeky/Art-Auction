# 🎨 ArtAuction — Online Art Auction Platform

A full-stack real-time online art auction platform where artists can showcase their artworks and buyers can place bids live.

---

## 🧱 Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | React.js + Vite + Tailwind CSS |
| **Backend** | .NET 8 Web API (Clean Architecture) |
| **Database** | PostgreSQL |
| **Real-time** | SignalR |
| **ORM** | Entity Framework Core |
| **Auth** | JWT Bearer Tokens |
| **State Management** | Redux Toolkit + React Query |

---

## 👥 Actors

- **Admin** — Approves/rejects artist accounts and artwork posts
- **Artist** — Creates and manages artwork listings and auctions
- **Buyer** — Browses artworks, places bids, manages watchlist

---

## ✨ Features

- 🔐 Role-based authentication (Admin / Artist / Buyer)
- 🖼️ Artwork browsing without login; bidding requires login
- ✅ Admin approval flow for artist accounts and artwork posts
- 🔍 Filter artworks by artist name, category, or tags
- ⏱️ Real-time bidding with SignalR — live bid updates for all viewers
- 💰 Each new bid must be at least **$10 higher** than the previous
- 📋 Public bid history (bidder name, price, timestamp)
- ❤️ Buyer watchlist to track auctions
- ⏰ Artist can extend auction end time
- 🏆 Automatic winner determination after auction ends
- 🔔 Winner notification with purchase details

---

## 🗄️ Database Schema

```
Users
├── Id (PK, Guid)
├── Username
├── Email
├── PasswordHash
├── Role (Admin=0, Artist=1, Buyer=2)
├── IsApproved
├── IsActive
├── ProfileImageUrl
├── CreatedAt
└── UpdatedAt

Artworks
├── Id (PK, Guid)
├── ArtistId (FK → Users)
├── Title
├── Description
├── InitialPrice
├── BuyNowPrice (optional)
├── CurrentBid
├── AuctionStartTime
├── AuctionEndTime
├── CategoryId (FK → Categories)
├── Status (Pending=0, Approved=1, Rejected=2, Active=3, Ended=4)
├── ImageUrl
├── CreatedAt
└── UpdatedAt

Categories
├── Id (PK, Guid)
├── Name
└── Description

ArtworkTags
├── Id (PK, Guid)
├── ArtworkId (FK → Artworks)
└── Tag

Bids
├── Id (PK, Guid)
├── ArtworkId (FK → Artworks)
├── BuyerId (FK → Users)
├── Amount
├── PlacedAt
└── IsWinning

Watchlist
├── Id (PK, Guid)
├── BuyerId (FK → Users)
├── ArtworkId (FK → Artworks)
└── AddedAt

Notifications
├── Id (PK, Guid)
├── UserId (FK → Users)
├── Title
├── Message
├── Type (BidPlaced=0, AuctionWon=1, ArtworkApproved=2, ArtworkRejected=3, AccountApproved=4)
├── IsRead
├── RelatedArtworkId (FK → Artworks, optional)
└── CreatedAt
```

---

## 📁 Project Structure

### Backend — Clean Architecture

```
backend/
├── ArtAuction.sln
└── src/
    ├── ArtAuction.API/              # Controllers, SignalR Hub, Middleware
    ├── ArtAuction.Application/      # CQRS Commands/Queries, DTOs, Validators
    ├── ArtAuction.Domain/           # Entities, Enums, Domain Events
    └── ArtAuction.Infrastructure/   # EF Core, JWT, File Storage, Background Services
```

### Frontend — Vertical Slice Architecture

```
frontend/
└── src/
    ├── features/
    │   ├── auth/                    # Login, Register, Protected Routes
    │   ├── artworks/                # Browse, Detail, Create, Edit
    │   ├── bidding/                 # Real-time bid panel, history, timer
    │   ├── watchlist/               # Add/remove/view watchlist
    │   ├── notifications/           # Bell, dropdown, real-time alerts
    │   ├── artist/                  # Artist dashboard, auction management
    │   └── admin/                   # Manage artists & artworks approval
    ├── shared/                      # Layout, UI components, hooks, API client
    ├── store/                       # Redux store configuration
    └── routes/                      # React Router v6 setup
```

---

## 🔄 Real-Time Flow (SignalR)

```
Buyer places bid
      ↓
POST /api/bids  (REST)
      ↓
PlaceBidCommandHandler validates:
  - Auction is active
  - Bid >= currentBid + $10
  - Buyer is authenticated
      ↓
Saves bid to PostgreSQL
      ↓
Broadcasts via SignalR → auction_{artworkId} group
      ↓
All connected clients receive live update instantly
```

---

## 🌐 API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/register` | None | Register buyer or artist |
| `POST` | `/api/auth/login` | None | Login |
| `GET` | `/api/artworks` | None | Browse & filter artworks |
| `GET` | `/api/artworks/{id}` | None | Artwork detail |
| `POST` | `/api/artworks` | Artist | Create artwork |
| `PUT` | `/api/artworks/{id}` | Artist | Update artwork |
| `DELETE` | `/api/artworks/{id}` | Artist | Delete artwork |
| `PUT` | `/api/artworks/{id}/extend` | Artist | Extend auction time |
| `POST` | `/api/bids` | Buyer | Place a bid |
| `GET` | `/api/artworks/{id}/bids` | None | Get bid history |
| `POST` | `/api/watchlist/{artworkId}` | Buyer | Add to watchlist |
| `DELETE` | `/api/watchlist/{artworkId}` | Buyer | Remove from watchlist |
| `GET` | `/api/watchlist` | Buyer | Get my watchlist |
| `GET` | `/api/admin/artists/pending` | Admin | Pending artist accounts |
| `PUT` | `/api/admin/artists/{id}/approve` | Admin | Approve artist |
| `PUT` | `/api/admin/artists/{id}/reject` | Admin | Reject artist |
| `GET` | `/api/admin/artworks/pending` | Admin | Pending artworks |
| `PUT` | `/api/admin/artworks/{id}/approve` | Admin | Approve artwork |
| `PUT` | `/api/admin/artworks/{id}/reject` | Admin | Reject artwork |
| `GET` | `/api/notifications` | Auth | My notifications |
| `WS` | `/hubs/auction` | Optional | SignalR real-time hub |

---

## ⚙️ Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 18+ or 20+ | https://nodejs.org |
| PostgreSQL | 16.x | https://www.postgresql.org/download/windows |
| pgAdmin | Latest | Bundled with PostgreSQL |
| EF Core CLI | 8.0+ | `dotnet tool install --global dotnet-ef` |

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/ArtAuction.git
cd ArtAuction
```

### 2. Backend Setup

```bash
cd backend

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json first (see Configuration)

# Run migrations
dotnet ef migrations add InitialCreate \
  --project src/ArtAuction.Infrastructure \
  --startup-project src/ArtAuction.API

dotnet ef database update \
  --project src/ArtAuction.Infrastructure \
  --startup-project src/ArtAuction.API

# Run the API
dotnet run --project src/ArtAuction.API
# → http://localhost:5000
# → Swagger: http://localhost:5000/swagger
```

### 3. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start dev server
npm run dev
# → http://localhost:5173
```

---

## 🔧 Configuration

### Backend — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ArtAuctionDb;Username=postgres;Password=YourPasswordHere"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ArtAuction",
    "Audience": "ArtAuctionClients",
    "ExpiryInDays": 7
  },
  "FileStorage": {
    "UploadPath": "wwwroot/uploads"
  }
}
```

### Frontend — `.env`

```env
VITE_API_URL=http://localhost:5000/api
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/auction
```

---

## 🐘 Why PostgreSQL?

- ✅ 100% Free — no size limits (SQL Server Express caps at 10GB)
- ✅ Open source and widely used in industry
- ✅ Supported on all cloud providers (AWS, Azure, GCP)
- ✅ Works seamlessly with EF Core via `Npgsql`

---

## 📦 Key NuGet Packages

| Package | Layer | Purpose |
|---------|-------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | PostgreSQL + EF Core |
| `MediatR` | Application | CQRS pattern |
| `FluentValidation.DependencyInjectionExtensions` | Application | Request validation |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | Application | Object mapping |
| `Microsoft.AspNetCore.SignalR` | API | Real-time WebSockets |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | API | JWT authentication |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Infrastructure | Identity management |
| `Microsoft.EntityFrameworkCore.Design` | Infrastructure | EF Core design-time tools |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure | EF Core CLI tools |
| `Microsoft.Extensions.Configuration` | Infrastructure | Configuration management |
| `Microsoft.Extensions.DependencyInjection` | Infrastructure | Dependency injection |
| `Microsoft.Extensions.Hosting` | Infrastructure | Hosting abstractions |
| `System.IdentityModel.Tokens.Jwt` | Infrastructure/API | JWT token handling |
| `Microsoft.AspNetCore.OpenApi` | API | OpenAPI/Swagger integration |
| `Swashbuckle.AspNetCore` | API | Swagger UI |

### Installation Commands

To add these packages to your projects, use the following commands:

**Application Layer:**
```bash
dotnet add src/ArtAuction.Application/ArtAuction.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add src/ArtAuction.Application/ArtAuction.Application.csproj package FluentValidation.DependencyInjectionExtensions --version 11.9.0
dotnet add src/ArtAuction.Application/ArtAuction.Application.csproj package MediatR --version 12.2.0
```

**Infrastructure Layer:**
```bash
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.4
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.4
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.4
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.4
dotnet add src/ArtAuction.Infrastructure/ArtAuction.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt --version 7.3.1
```

**API Layer:**
```bash
dotnet add src/ArtAuction.API/ArtAuction.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.4
dotnet add src/ArtAuction.API/ArtAuction.API.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.25
dotnet add src/ArtAuction.API/ArtAuction.API.csproj package Microsoft.AspNetCore.SignalR --version 1.2.9
dotnet add src/ArtAuction.API/ArtAuction.API.csproj package Swashbuckle.AspNetCore --version 6.5.0
dotnet add src/ArtAuction.API/ArtAuction.API.csproj package System.IdentityModel.Tokens.Jwt --version 7.3.1
```

## 📦 Key npm Packages

| Package | Purpose |
|---------|---------|
| `@microsoft/signalr` | Real-time SignalR client |
| `@reduxjs/toolkit` | State management |
| `@tanstack/react-query` | Server state & caching |
| `react-hook-form` + `zod` | Forms & validation |
| `axios` | HTTP client |
| `react-router-dom` | Routing |
| `tailwindcss` | Styling |

---

## 📄 License

This project is for educational purposes.
