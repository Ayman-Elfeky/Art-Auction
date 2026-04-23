import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";

export function LoginPage() {
  const { login } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const toast = useGlobalToast();
  const [passType, setPassType] = useState("password");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    try {
      await login(email, password);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Login failed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <form className="card form-card" onSubmit={onSubmit}>
        <div style={{ textAlign: "center", marginBottom: "1.5rem" }}>
          <div style={{ fontSize: "2.5rem", marginBottom: "0.5rem" }}>🎨</div>
          <h1 className="form-title">{t("Sign In", "تسجيل الدخول")}</h1>
          <p className="form-subtitle">
            {t("Access your account to bid, manage artworks, or review approvals.", "ادخل حسابك للمزايدة وإدارة الأعمال الفنية.")}
          </p>
        </div>

        <div className="form-inner">
          <div className="field">
            <label htmlFor="login-email">{t("Email", "البريد الإلكتروني")}</label>
            <input
              id="login-email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </div>
          <div className="field">
            <label htmlFor="login-password">{t("Password", "كلمة المرور")}</label>
            <input
              id="login-password"
              type={passType}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
            <button type="button" onClick={() => setPassType(passType === "password" ? "text" : "password")}>
              {passType === "password" ? "👁️" : "🙈"}
            </button>
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: "100%" }}>
            {loading ? t("Signing in…", "جاري الدخول…") : t("Sign In →", "دخول →")}
          </button>

          <p style={{ textAlign: "center", fontSize: "0.82rem", color: "var(--text-muted)" }}>
            {t("No account?", "ليس لديك حساب؟")}{" "}
            <Link to="/register" style={{ color: "var(--gold-400)" }}>{t("Register as Buyer", "سجّل كمشترٍ")}</Link>
            {" · "}
            <Link to="/register-artist" style={{ color: "var(--gold-400)" }}>{t("Join as Artist", "انضم كفنان")}</Link>
          </p>
        </div>
      </form>
    </div>
  );
}
