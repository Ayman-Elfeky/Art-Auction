import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { canAccessAdminPortal } from "../lib/auth";
import { api } from "../lib/api";



export function NavBar() {
  const { user, claims, token, logout } = useAuth();
  const { lang, toggle, t } = useLang();
  const location = useLocation();
  const [unread, setUnread] = useState(0);

  useEffect(() => {
    if (!token) { setUnread(0); return; }
    api.getNotifications(token)
      .then((n) => setUnread(n.filter((x) => !x.isRead).length))
      .catch(() => undefined);
  }, [token, location.pathname]);



  return (
    <header className="nav">
      {/* Brand */}
      <Link to="/" className="nav-brand">
        <span className="nav-brand-en">🎨 Art Auction</span>
        <span className="nav-brand-ar">مزاد الفن</span>
      </Link>



      {/* Right links */}
      <nav className="nav-right" aria-label="User navigation">
        <Link to="/" className="nav-link">{t("Artworks", "الأعمال")}</Link>

        {user?.role === "User" && (
          <Link to="/watchlist" className="nav-link">{t("Watchlist", "قائمتي")}</Link>
        )}
        {user?.role === "Artist" && (
          <Link to="/artist" className="nav-link">{t("Dashboard", "لوحتي")}</Link>
        )}
        {user && canAccessAdminPortal(claims) && (
          <Link to="/admin" className="nav-link">{t("Admin", "الإدارة")}</Link>
        )}

        {user && (
          <Link to="/notifications" className="nav-bell" aria-label="Notifications">
            🔔
            {unread > 0 && <span className="nav-badge">{unread > 9 ? "9+" : unread}</span>}
          </Link>
        )}

        {!user && <Link to="/login" className="nav-link">{t("Login", "دخول")}</Link>}
        {!user && <Link to="/register" className="nav-link">{t("Register", "تسجيل")}</Link>}
        {!user && (
          <Link to="/register-artist" className="nav-link">
            {t("Become Artist", "كن فناناً")}
          </Link>
        )}

        {user && (
          <button className="nav-link nav-logout" onClick={logout} type="button">
            {user.name.split(" ")[0]} · {t("Logout", "خروج")}
          </button>
        )}

        <button className="lang-toggle" onClick={toggle} type="button">
          {lang === "en" ? "عربي" : "EN"}
        </button>
      </nav>
    </header>
  );
}
