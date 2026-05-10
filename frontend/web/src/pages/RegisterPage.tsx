import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";


function validatePassword(password: string, t: (en: string, ar: string) => string): string | null {
  if (password.length < 8)
    return t("Password must be at least 8 characters.", "كلمة المرور يجب أن تكون 8 أحرف على الأقل.");
  if (!/[A-Z]/.test(password))
    return t("Password must contain at least one uppercase letter.", "كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل.");
  if (!/[a-z]/.test(password))
    return t("Password must contain at least one lowercase letter.", "كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.");
  if (!/\d/.test(password))
    return t("Password must contain at least one number.", "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.");
  return null;
}

export function RegisterPage() {
  const { registerBuyer } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const toast = useGlobalToast();
  const [name, setName]         = useState("");
  const [email, setEmail]       = useState("");
  const [password, setPassword] = useState("");
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [loading, setLoading]   = useState(false);

  function onPasswordChange(value: string) {
    setPassword(value);
    setPasswordError(value ? validatePassword(value, t) : null);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    const err = validatePassword(password, t);
    if (err) {
      setPasswordError(err);
      return;
    }
    setLoading(true);
    try {
      await registerBuyer(name, email, password);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Registration failed.");
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
          <div style={{ fontSize: "2.5rem", marginBottom: "0.5rem" }}>🛒</div>
          <h1 className="form-title">{t("Create Buyer Account", "إنشاء حساب مشترٍ")}</h1>
          <p className="form-subtitle">
            {t("Register to bid on exclusive artworks.", "سجّل للمزايدة على الأعمال الفنية الحصرية.")}
          </p>
        </div>

        <div className="form-inner">
          <div className="field">
            <label htmlFor="reg-name">{t("Full Name", "الاسم الكامل")}</label>
            <input id="reg-name" value={name} onChange={(e) => setName(e.target.value)} placeholder={t("Your name", "اسمك")} required />
          </div>
          <div className="field">
            <label htmlFor="reg-email">{t("Email", "البريد الإلكتروني")}</label>
            <input id="reg-email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="you@example.com" required />
          </div>
          <div className="field">
            <label htmlFor="reg-password">{t("Password", "كلمة المرور")}</label>
            <input
              id="reg-password"
              type="password"
              value={password}
              onChange={(e) => onPasswordChange(e.target.value)}
              placeholder="••••••••"
              required
              aria-describedby="reg-password-hint"
            />
            {passwordError ? (
              <p id="reg-password-hint" style={{ fontSize: "0.78rem", color: "var(--red-400, #f87171)", marginTop: "0.3rem" }}>
                {passwordError}
              </p>
            ) : (
              <p id="reg-password-hint" style={{ fontSize: "0.78rem", color: "var(--text-muted)", marginTop: "0.3rem" }}>
                {t(
                  "Min 8 characters — uppercase, lowercase & number required.",
                  "8 أحرف على الأقل — يجب أن تحتوي على حرف كبير وصغير ورقم.",
                )}
              </p>
            )}
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading || !!passwordError} style={{ width: "100%" }}>
            {loading ? t("Creating account…", "جاري الإنشاء…") : t("Create Account →", "إنشاء حساب →")}
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
