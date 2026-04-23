import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const HUB_URL = `${import.meta.env.VITE_API_URL ?? "http://localhost:5161"}/hubs/auctions`;

export function buildAuctionConnection(token?: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => token ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
