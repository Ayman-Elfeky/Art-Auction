export type Role = "Admin" | "Artist" | "User";

export type AuthUser = {
  id: string;
  email: string;
  name: string;
  role: Role;
  verified: boolean;
  createdAt: string;
};

export type AuthTokenResponse = {
  token: string;
  user: AuthUser;
};

export type Artwork = {
  id: string;
  title: string;
  artistName: string;
  categoryName: string;
  initialPrice: number;
  currentBid: number;
  auctionEndTime: string;
  status: string;
  imageUrl: string;
};

export type ArtworkDetail = {
  id: string;
  title: string;
  description: string;
  artistName: string;
  artistId: string;
  initialPrice: number;
  buyNowPrice?: number | null;
  currentBid: number;
  auctionStartTime: string;
  auctionEndTime: string;
  status: string;
  categoryName: string;
  tags: string[];
  totalBids: number;
  imageUrl: string;
  createdAt: string;
};

export type PagedArtwork = {
  items: Artwork[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

export type CreateArtworkInput = {
  title: string;
  description: string;
  initialPrice: number;
  buyNowPrice?: number | null;
  auctionStartTime: string;
  auctionEndTime: string;
  categoryName: string;
  tags: string[];
  imageUrl: string;
};

export type Bid = {
  id: string;
  issuerId: string;
  artId: string;
  amount: number;
  highestBidder: boolean;
  placed: string;
};

export type NotificationDto = {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  relatedArtworkId?: string | null;
};

export type PendingArtist = {
  id: string;
  username: string;
  email: string;
  createdAt: string;
};

export type PendingArtwork = {
  id: string;
  title: string;
  description: string;
  initialPrice: number;
  artistName: string;
  createdAt: string;
};

export type JwtClaims = {
  sub?: string;
  role?: Role;
  permission?: string[] | string;
  exp?: number;
};
