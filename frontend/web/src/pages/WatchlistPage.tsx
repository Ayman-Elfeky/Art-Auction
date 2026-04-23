import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { api } from "../lib/api";
import { IMAGE_PLACEHOLDER, resolveImageUrl } from "../lib/images";
import { useCountdown } from "../lib/useCountdown";
import type { Artwork } from "../types";

function WatchlistCard({ item, onRemove }: { item: Artwork; onRemove: () => void }) {
  const { t } = useLang();
  const cd = useCountdown(item.auctionEndTime);
  const statusClass = cd.isExpired ? "badge--ended" : cd.isUpcoming ? "badge--upcoming" : "badge--live";
  const statusLabel = cd.isExpired ? t("Ended","انتهى") : cd.isUpcoming ? t("Soon","قريباً") : t("Live","مباشر");

  return (
    <Link to={`/artworks/${item.id}`} className="artwork-card" style={{ textDecoration: "none" }}>
      <div className="artwork-card__img-wrap">
        <img
          src={resolveImageUrl(item.imageUrl)}
          alt={item.title}
          onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }}
        />
        <div className="artwork-card__overlay">
          <span className="artwork-card__bid-live">${item.currentBid || item.initialPrice}</span>
          {!cd.isExpired && !cd.isUpcoming && (
            <span style={{ fontSize: "0.8rem", color: "var(--text-primary)", fontVariantNumeric: "tabular-nums" }}>
              {String(cd.hours).padStart(2,"0")}:{String(cd.minutes).padStart(2,"0")}:{String(cd.seconds).padStart(2,"0")}
            </span>
          )}
        </div>
      </div>
      <div className="artwork-card__body">
        <div className="row" style={{ justifyContent: "space-between" }}>
          <h3 className="artwork-card__title">{item.title}</h3>
          <span className={`badge ${statusClass}`}>{statusLabel}</span>
        </div>
        <p className="artwork-card__meta">{item.artistName}</p>
        <div className="artwork-card__footer">
          <span className="artwork-card__cta">{t("View", "عرض")}</span>
          <button
            className="btn btn-danger btn-sm"
            onClick={(e) => { e.preventDefault(); onRemove(); }}
            type="button"
          >
            {t("Remove", "إزالة")}
          </button>
        </div>
      </div>
    </Link>
  );
}

export function WatchlistPage() {
  const { token } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const [items, setItems] = useState<Artwork[]>([]);
  const [error, setError] = useState("");

  async function load() {
    if (!token) return;
    try {
      const data = await api.getWatchlist(token);
      setItems(data as Artwork[]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed loading watchlist.");
    }
  }

  useEffect(() => { load(); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, [token]);

  async function removeItem(id: string) {
    if (!token) return;
    await api.removeWatchlist(token, id);
    await load();
  }

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <h1 className="section-title">{t("My Watchlist", "قائمتي")}</h1>
      <p className="section-sub">{t("Track your favourite auctions.", "تابع مزاداتك المفضلة.")}</p>
      {error && <p className="error">{error}</p>}
      {items.length === 0 && !error && (
        <div className="card" style={{ textAlign: "center", padding: "2.5rem", color: "var(--text-muted)" }}>
          <div style={{ fontSize: "2.5rem", marginBottom: "0.75rem" }}>👁️</div>
          <p>{t("Your watchlist is empty.", "قائمتك فارغة.")}</p>
        </div>
      )}
      <div className="grid">
        {items.map((item) => (
          <WatchlistCard key={item.id} item={item} onRemove={() => removeItem(item.id)} />
        ))}
      </div>
    </div>
  );
}
