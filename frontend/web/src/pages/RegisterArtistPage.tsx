import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";

export function RegisterArtistPage() {
  const { registerArtist } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const toast = useGlobalToast();
  const [name, setName]   = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    try {
      await registerArtist(name, email, password);
      setDone(true);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Registration failed.");
    } finally {
      setLoading(false);
    }
  }

  if (done) {
    return (
      <div className="container">
        <div className="card form-card" style={{ textAlign: "center" }}>
          <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>✅</div>
          <h2 className="form-title">{t("Application Submitted!", "تم إرسال طلبك!")}</h2>
          <p className="form-subtitle" style={{ marginTop: "0.5rem" }}>
            {t("Your artist account is pending admin approval. You'll be notified once approved.", "حسابك قيد المراجعة. سيتم إشعارك بعد الموافقة.")}
          </p>
          <button className="btn btn-primary" style={{ marginTop: "1.5rem" }} onClick={() => navigate("/login")}>
            {t("Go to Login", "الذهاب إلى الدخول")}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <form className="card form-card" onSubmit={onSubmit}>
        <div style={{ textAlign: "center", marginBottom: "1.5rem" }}>
          <div style={{ fontSize: "2.5rem", marginBottom: "0.5rem" }}>🖌️</div>
          <h1 className="form-title">{t("Join as Artist", "انضم كفنان")}</h1>
          <p className="form-subtitle">
            {t("Showcase your art and run live auctions. Admin approval required.", "اعرض فنك وأدر مزادات مباشرة. تتطلب موافقة الإدارة.")}
          </p>
        </div>

        <div className="form-inner">
          <div className="field">
            <label htmlFor="art-reg-name">{t("Artist Name", "اسم الفنان")}</label>
            <input id="art-reg-name" value={name} onChange={(e) => setName(e.target.value)} placeholder={t("Your artist name", "اسمك الفني")} required />
          </div>
          <div className="field">
            <label htmlFor="art-reg-email">{t("Email", "البريد الإلكتروني")}</label>
            <input id="art-reg-email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="artist@example.com" required />
          </div>
          <div className="field">
            <label htmlFor="art-reg-password">{t("Password", "كلمة المرور")}</label>
            <input id="art-reg-password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="••••••••" required />
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: "100%" }}>
            {loading ? t("Submitting…", "جاري الإرسال…") : t("Apply as Artist →", "تقديم طلب فنان →")}
          </button>

          <p style={{ textAlign: "center", fontSize: "0.82rem", color: "var(--text-muted)" }}>
            {t("Already have an account?", "لديك حساب؟")}{" "}
            <Link to="/login" style={{ color: "var(--gold-400)" }}>{t("Sign In", "دخول")}</Link>
          </p>
        </div>
      </form>
    </div>
  );
}
