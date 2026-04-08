Responsibility:
  - Add to watchlist
  - Remove from watchlist
  - Get my watchlist
  - Get my notifications
  - Mark notification as read
  - WatchlistController
  - NotificationsController

Files you own:
  ├── Application/Features/Watchlist/
  │   ├── Commands/
  │   │   ├── AddToWatchlist/
  │   │   │   ├── AddToWatchlistCommand.cs
  │   │   │   └── AddToWatchlistCommandHandler.cs
  │   │   └── RemoveFromWatchlist/
  │   │       ├── RemoveFromWatchlistCommand.cs
  │   │       └── RemoveFromWatchlistCommandHandler.cs
  │   ├── Queries/
  │   │   └── GetWatchlist/
  │   │       ├── GetWatchlistQuery.cs
  │   │       └── GetWatchlistQueryHandler.cs
  │   └── DTOs/
  │       └── WatchlistDto.cs
  │
  ├── Application/Features/Notifications/
  │   ├── Commands/
  │   │   └── MarkNotificationRead/
  │   │       ├── MarkNotificationReadCommand.cs
  │   │       └── MarkNotificationReadCommandHandler.cs
  │   ├── Queries/
  │   │   └── GetNotifications/
  │   │       ├── GetNotificationsQuery.cs
  │   │       └── GetNotificationsQueryHandler.cs
  │   └── DTOs/
  │       └── NotificationDto.cs
  │
  └── API/Controllers/
      ├── WatchlistController.cs
      └── NotificationsController.cs

Endpoints to implement:
  POST   /api/watchlist/{artworkId}   ← Buyer only
  DELETE /api/watchlist/{artworkId}   ← Buyer only
  GET    /api/watchlist               ← Buyer only
  GET    /api/notifications           ← Authenticated
  PUT    /api/notifications/{id}/read ← Authenticated

Business Rules:
  ✅ Buyer only can add to watchlist
  ✅ Cannot add same artwork twice
  ✅ Notifications are user specific
  ✅ Can mark notification as read
  ✅ Watchlist shows auction status in real time

Estimated time: 1–2 days