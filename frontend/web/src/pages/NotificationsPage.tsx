import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";
import { api } from "../lib/api";
import { buildAuctionConnection } from "../lib/signalr";
import type { NotificationDto } from "../types";

const TYPE_ICON: Record<string, string> = {
  AuctionWon:      "🏆",
  BidPlaced:       "🔨",
  AuctionEnded:    "🏁",
  ArtworkApproved: "✅",
  ArtworkRejected: "❌",
  Default:         "🔔",
};

export function NotificationsPage() {
  const { token } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const toast = useGlobalToast();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);

  async function load() {
    if (!token) return;
    try { setNotifications(await api.getNotifications(token)); }
    catch (err) { toast.error(err instanceof Error ? err.message : "Failed loading notifications."); }
  }

  useEffect(() => { load(); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, [token]);

  useEffect(() => {
    if (!token) return;
    const connection = buildAuctionConnection(token);
    connection.on("notification.received", () => { load().catch(() => undefined); });
    connection.start()
      .then(async () => { await connection.invoke("JoinMyNotifications"); })
      .catch(() => undefined);
    return () => { connection.stop().catch(() => undefined); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  async function markRead(id: string) {
    if (!token) return;
    try {
      await api.markNotificationRead(token, id);
      setNotifications((prev) => prev.map((n) => n.id === id ? { ...n, isRead: true } : n));
    } catch (err) { toast.error(err instanceof Error ? err.message : "Failed."); }
  }

  async function markAllRead() {
    if (!token) return;
    try {
      await api.markAllNotificationsRead(token);
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      toast.success(t("All notifications marked as read.", "تم تحديد كل الإشعارات كمقروءة."));
    } catch (err) { toast.error(err instanceof Error ? err.message : "Failed."); }
  }

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: "1rem", marginBottom: "0.35rem" }}>
        <h1 className="section-title" style={{ margin: 0 }}>
          {t("Notifications", "الإشعارات")}
          {unreadCount > 0 && (
            <span style={{ marginLeft: "0.6rem", background: "var(--blue-600)", color: "#fff", borderRadius: "var(--radius-pill)", fontSize: "0.7rem", padding: "0.15rem 0.5rem", fontFamily: "Inter, sans-serif", fontWeight: 700 }}>
              {unreadCount}
            </span>
          )}
        </h1>
        {unreadCount > 0 && (
          <button className="btn btn-secondary btn-sm" onClick={markAllRead} type="button">
            ✓ {t("Mark all as read", "تحديد الكل كمقروء")}
          </button>
        )}
      </div>
      <p className="section-sub">{t("Stay up to date with your auctions.", "ابق على اطلاع بمزاداتك.")}</p>

      <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem" }}>
        {notifications.length === 0 && (
          <div className="card" style={{ textAlign: "center", color: "var(--text-muted)", padding: "2.5rem" }}>
            <div style={{ fontSize: "2.5rem", marginBottom: "0.75rem" }}>🔔</div>
            <p>{t("No notifications yet.", "لا إشعارات حتى الآن.")}</p>
          </div>
        )}
        {notifications.map((n) => (
          <div key={n.id} className={`notif-card${!n.isRead ? " notif-card--unread" : ""}`}>
            <span className="notif-icon">{TYPE_ICON[n.type] ?? TYPE_ICON.Default}</span>
            <div className="notif-body" style={{ flex: 1 }}>
              <div className="notif-title">{n.title}</div>
              <div className="notif-msg">{n.message}</div>
              <div className="notif-time">{new Date(n.createdAt).toLocaleString()}</div>
            </div>
            {!n.isRead ? (
              <button
                className="btn btn-secondary btn-sm"
                onClick={() => markRead(n.id)}
                type="button"
                style={{ flexShrink: 0, alignSelf: "center" }}
              >
                ✓ {t("Mark read", "تحديد كمقروء")}
              </button>
            ) : (
              <span style={{ fontSize: "0.7rem", color: "var(--text-muted)", alignSelf: "center", flexShrink: 0 }}>
                {t("Read", "مقروء")} ✓
              </span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
