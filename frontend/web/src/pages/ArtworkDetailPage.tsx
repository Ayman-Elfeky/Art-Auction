import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { api } from "../lib/api";
import { IMAGE_PLACEHOLDER, resolveImageUrl } from "../lib/images";
import { buildAuctionConnection } from "../lib/signalr";
import { useCountdown } from "../lib/useCountdown";
import { useToast } from "../lib/useToast";
import { ToastContainer } from "../components/Toast";
import { Modal } from "../components/Modal";
import type { ArtworkDetail, Bid } from "../types";

type LiveBidEvent = {
  artworkId: string;
  bidderId: string;
  bidderName: string;
  amount: number;
  timestamp: string;
};

function CountdownBlock({ endTime, startTime, status }: { endTime: string; startTime: string; status: string }) {
  const cd = useCountdown(endTime, startTime);
  const { t } = useLang();
  
  const isEnded = status.toLowerCase() === "ended" || cd.isExpired;

  if (cd.isUpcoming && status.toLowerCase() !== "ended") return <p style={{ color: "var(--blue-300)" }}>⏳ {t("Auction starts soon", "المزاد يبدأ قريباً")}</p>;
  if (isEnded) return <p style={{ color: "var(--text-muted)" }}>🏁 {t("Auction ended", "انتهى المزاد")}</p>;
  return (
    <div className="countdown">
      {cd.days > 0 && (
        <>
          <div className="countdown-unit">
            <span className="countdown-num">{cd.days}</span>
            <span className="countdown-label">{t("days", "يوم")}</span>
          </div>
          <span className="countdown-sep">:</span>
        </>
      )}
      {[
        { val: cd.hours,   lbl: t("hrs", "ساعة") },
        { val: cd.minutes, lbl: t("min", "دقيقة") },
        { val: cd.seconds, lbl: t("sec", "ثانية") },
      ].map(({ val, lbl }, i, arr) => (
        <>
          <div key={lbl} className="countdown-unit">
            <span className="countdown-num">{String(val).padStart(2, "0")}</span>
            <span className="countdown-label">{lbl}</span>
          </div>
          {i < arr.length - 1 && <span className="countdown-sep">:</span>}
        </>
      ))}
    </div>
  );
}

export function ArtworkDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { token, user } = useAuth();
  const { t } = useLang();
  const { toasts, push, dismiss } = useToast();
  const [artwork, setArtwork] = useState<ArtworkDetail | null>(null);
  const [bids, setBids] = useState<Bid[]>([]);
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(true);
  const [showBuyNowConfirm, setShowBuyNowConfirm] = useState(false);

  const canBid = useMemo(() => user?.role === "User" && !!token, [user, token]);

  async function load() {
    setLoading(true);
    const [artworkData, bidData] = await Promise.all([api.getArtwork(id), api.getArtworkBids(id)]);
    setArtwork(artworkData);
    setBids(bidData);
    setLoading(false);
  }

  useEffect(() => {
    load().catch((err) => {
      push(err instanceof Error ? err.message : "Load failed.", "error");
      setLoading(false);
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  useEffect(() => {
    const connection = buildAuctionConnection(token ?? undefined);
    connection.on("bid.placed", (event: LiveBidEvent) => {
      if (event.artworkId !== id) return;
      push(`🔨 ${event.bidderName} ${t("bid", "زايد بـ")} $${event.amount}`, "bid");
      load().catch(() => undefined);
    });
    connection.on("auction.ended", (event: { winnerName: string; finalPrice: number }) => {
      push(`🏆 ${event.winnerName} ${t("won the auction with", "فاز بالمزاد بـ")} $${event.finalPrice}!`, "success");
      load().catch(() => undefined);
    });
    connection.start()
      .then(async () => { await connection.invoke("JoinArtworkGroup", id); })
      .catch(() => undefined);
    return () => {
      connection.invoke("LeaveArtworkGroup", id).catch(() => undefined)
        .finally(() => connection.stop().catch(() => undefined));
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, token]);

  async function submitBid(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    try {
      await api.placeBid(token, id, Number(amount));
      setAmount("");
      push(t("Bid placed successfully!", "تمت المزايدة بنجاح!"), "success");
      await load();
    } catch (err) {
      push(err instanceof Error ? err.message : "Bid failed.", "error");
    }
  }

  async function buyNow() {
    if (!token || !artwork?.buyNowPrice) return;
    setShowBuyNowConfirm(true);
  }

  async function confirmBuyNow() {
    if (!token || !artwork?.buyNowPrice) return;
    setShowBuyNowConfirm(false);
    try {
      await api.placeBid(token, id, artwork.buyNowPrice);
      push(t("Purchased successfully! Auction ended.", "تم الشراء بنجاح! انتهى المزاد."), "success");
      await load();
    } catch (err) {
      push(err instanceof Error ? err.message : "Purchase failed.", "error");
    }
  }

  async function addWatchlist() {
    if (!token) return;
    try {
      await api.addWatchlist(token, id);
      push(t("Added to watchlist!", "تمت الإضافة إلى القائمة!"), "success");
    } catch (err) {
      push(err instanceof Error ? err.message : "Failed adding watchlist.", "error");
    }
  }

  if (loading) return (
    <div className="container" style={{ textAlign: "center", paddingTop: "4rem", color: "var(--text-muted)" }}>
      <div style={{ fontSize: "2rem", marginBottom: "1rem" }}>⏳</div>
      <p>{t("Loading artwork…", "جاري التحميل…")}</p>
    </div>
  );
  if (!artwork) return <div className="container"><p className="error">{t("Artwork not found.", "العمل غير موجود.")}</p></div>;

  const highestBid = bids[0];

  return (
    <>
      <ToastContainer toasts={toasts} dismiss={dismiss} />
      <div className="container">
        <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
          {t("← Back", "← عودة")}
        </button>
        <div className="detail-grid">
          {/* Left — image */}
          <div>
            <img
              className="detail-img"
              src={resolveImageUrl(artwork.imageUrl)}
              alt={artwork.title}
              onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }}
            />
          </div>

          {/* Right — info */}
          <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <h1 className="detail-title">{artwork.title}</h1>

            <div className="row" style={{ gap: "0.5rem" }}>
              {artwork.tags.map((tag) => (
                <span key={tag} className="chip chip-gold">{tag}</span>
              ))}
            </div>

            <div className="detail-meta-row">
              🎨 <strong>{t("Artist:", "الفنان:")}</strong> {artwork.artistName}
            </div>
            <div className="detail-meta-row">
              🗂️ <strong>{t("Category:", "التصنيف:")}</strong> {artwork.categoryName}
            </div>
            <p style={{ color: "var(--text-secondary)", fontSize: "0.9rem", lineHeight: "1.6" }}>
              {artwork.description}
            </p>

            {/* Price block */}
            <div className="detail-price-block">
              <div>
                <div className="detail-price-label">{t("Current Bid", "المزايدة الحالية")}</div>
                <div className="detail-price-val">${artwork.currentBid || artwork.initialPrice}</div>
              </div>
              <div>
                <div className="detail-price-label">{t("Starting Price", "السعر الابتدائي")}</div>
                <div className="detail-price-val" style={{ fontSize: "1rem", color: "var(--text-secondary)" }}>
                  ${artwork.initialPrice}
                </div>
              </div>
              {artwork.buyNowPrice && (
                <div>
                  <div className="detail-price-label">{t("Buy Now", "اشتري الآن")}</div>
                  <div className="detail-price-val" style={{ fontSize: "1rem", color: "var(--blue-300)" }}>
                    ${artwork.buyNowPrice}
                  </div>
                </div>
              )}
            </div>

            {/* Countdown */}
            <div className="card" style={{ padding: "1rem 1.2rem" }}>
              <p style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: "0.5rem", textTransform: "uppercase", letterSpacing: "0.05em" }}>
                {t("Auction ends in", "ينتهي المزاد بعد")}
              </p>
              <CountdownBlock endTime={artwork.auctionEndTime} startTime={artwork.auctionStartTime} status={artwork.status} />
            </div>

            {/* Bid form */}
            {canBid && artwork.status.toLowerCase() === "active" && (
              <form className="card" onSubmit={submitBid} style={{ display: "flex", flexDirection: "column", gap: "0.85rem" }}>
                <h3 style={{ fontFamily: "'Cinzel', serif", fontSize: "1rem", color: "var(--gold-400)" }}>
                  {t("Place Your Bid", "ضع مزايدتك")}
                </h3>
                {highestBid && (
                  <p style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>
                    {t("Minimum bid:", "الحد الأدنى:")} ${highestBid.amount + 10}
                  </p>
                )}
                <div className="field">
                  <label htmlFor="bid-amount">{t("Bid Amount (USD)", "المبلغ (دولار)")}</label>
                  <input
                    id="bid-amount"
                    type="number"
                    placeholder={highestBid ? `Min $${highestBid.amount + 10}` : `Min $${artwork.initialPrice}`}
                    value={amount}
                    onChange={(e) => setAmount(e.target.value)}
                    required
                  />
                </div>
                <div className="row">
                  <button type="submit" className="btn btn-primary">{t("Place Bid 🔨", "زايد الآن 🔨")}</button>
                  {artwork.buyNowPrice && (
                    <button type="button" className="btn btn-success" onClick={buyNow}>
                      {t("Buy Now 🛒", "اشترِ الآن 🛒")}
                    </button>
                  )}
                  <button type="button" className="btn btn-secondary" onClick={addWatchlist}>
                    {t("+ Watchlist", "+ قائمتي")}
                  </button>
                </div>
              </form>
            )}

            {!canBid && !user && artwork.status.toLowerCase() === "active" && (
              <div className="card" style={{ textAlign: "center", color: "var(--text-muted)" }}>
                <p>{t("Login as Buyer to place bids.", "سجّل دخولك كمشترٍ للمزايدة.")}</p>
              </div>
            )}
          </div>
        </div>

        {/* Bid history */}
        <div className="card" style={{ marginTop: "2rem" }}>
          <h2 style={{ fontFamily: "'Cinzel', serif", fontSize: "1.2rem", color: "var(--text-primary)", marginBottom: "1rem" }}>
            {t("Bid History", "سجل المزايدات")} ({bids.length})
          </h2>
          {bids.length === 0 ? (
            <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "1rem" }}>
              {t("No bids yet. Be the first!", "لا مزايدات حتى الآن. كن الأول!")}
            </p>
          ) : (
            <table className="bid-history-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>{t("Bidder", "المزايد")}</th>
                  <th>{t("Amount", "المبلغ")}</th>
                  <th>{t("Time", "الوقت")}</th>
                </tr>
              </thead>
              <tbody>
                {bids.map((bid, i) => (
                  <tr key={bid.id} className={i === 0 ? "top-bid" : ""}>
                    <td>{i === 0 ? "🏆" : i + 1}</td>
                    <td>{bid.issuerId ? bid.issuerId.slice(0, 8) + "…" : "—"}</td>
                    <td style={{ fontWeight: 700, color: i === 0 ? "var(--gold-400)" : undefined }}>${bid.amount}</td>
                    <td>{new Date(bid.placed).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      <Modal
        isOpen={showBuyNowConfirm}
        onClose={() => setShowBuyNowConfirm(false)}
        title={t("Confirm Purchase", "تأكيد الشراء")}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setShowBuyNowConfirm(false)}>
              {t("Cancel", "إلغاء")}
            </button>
            <button className="btn btn-success" onClick={confirmBuyNow}>
              {t("Confirm & Buy", "تأكيد وشراء")}
            </button>
          </>
        }
      >
        <div style={{ textAlign: "center", padding: "1rem 0" }}>
          <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>🛍️</div>
          <p style={{ fontSize: "1.1rem", marginBottom: "0.5rem" }}>
            {t("Are you sure you want to buy", "هل أنت متأكد أنك تريد شراء")}
          </p>
          <h3 style={{ color: "var(--text-primary)", marginBottom: "1rem" }}>{artwork.title}</h3>
          <p style={{ fontSize: "1.5rem", fontWeight: "bold", color: "var(--gold-400)" }}>
            ${artwork.buyNowPrice}
          </p>
          <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", marginTop: "1rem" }}>
            {t("This will end the auction immediately and secure the item for you.", "سيؤدي هذا لإنهاء المزاد فوراً وتأمين القطعة لك.")}
          </p>
        </div>
      </Modal>
    </>
  );
}
