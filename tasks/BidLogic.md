Responsibility:
  - Place bid logic
  - Bid history (public) 
  - SignalR real-time broadcasting
  - Auto end auction background service
  - Winner determination
  - BidsController

Files you own:
  ├── Application/Features/Bids/
  │   ├── Commands/
  │   │   └── PlaceBid/
  │   │       ├── PlaceBidCommand.cs
  │   │       ├── PlaceBidCommandHandler.cs
  │   │       └── PlaceBidCommandValidator.cs
  │   ├── Queries/
  │   │   └── GetBidHistory/
  │   │       ├── GetBidHistoryQuery.cs
  │   │       └── GetBidHistoryQueryHandler.cs
  │   └── DTOs/
  │       └── BidDto.cs
  │
  └── API/Controllers/
      └── BidsController.cs

Endpoints to implement:
  POST /api/bids                    ← Buyer only
  GET  /api/artworks/{id}/bids      ← public

SignalR events to broadcast:
  NewBid         → to auction_{artworkId} group
  AuctionEnded   → to auction_{artworkId} group

Business Rules:
  ✅ Buyer must be logged in to bid
  ✅ Each bid must be >= currentBid + $10
  ✅ Cannot bid on own artwork
  ✅ Auction must be currently active
  ✅ Bid history is public (anyone can view)
  ✅ After auction ends → highest bid wins
  ✅ Winner gets notification with purchase details
  ✅ Background service checks every minute for
     ended auctions and processes them

Estimated time: 2–3 days