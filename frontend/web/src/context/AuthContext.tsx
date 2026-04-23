import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import { canAccessAdminPortal, clearStoredToken, decodeJwt, getStoredToken, setStoredToken } from "../lib/auth";
import type { AuthUser, JwtClaims, Role } from "../types";

type AuthContextValue = {
  token: string | null;
  user: AuthUser | null;
  claims: JwtClaims;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  registerBuyer: (name: string, email: string, password: string) => Promise<void>;
  registerArtist: (name: string, email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function roleRedirect(role: Role | undefined): string {
  if (role === "Admin")  return "/admin";
  if (role === "Artist") return "/artist";
  return "/";
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(getStoredToken());
  const [user, setUser]   = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const claims = useMemo(() => (token ? decodeJwt(token) : {}), [token]);

  useEffect(() => {
    async function bootstrap() {
      if (!token) { setLoading(false); return; }
      try {
        const me = await api.getMe(token);
        setUser(me);
      } catch {
        clearStoredToken();
        setToken(null);
        setUser(null);
      } finally {
        setLoading(false);
      }
    }
    bootstrap();
  }, [token]);

  const applyAuth = useCallback(
    async (executor: Promise<{ token: string; user: AuthUser }>, redirect = true) => {
      const result = await executor;
      setStoredToken(result.token);
      setToken(result.token);
      setUser(result.user);
      if (redirect) {
        const c = decodeJwt(result.token);
        if (canAccessAdminPortal(c)) {
          navigate("/admin");
        } else {
          navigate(roleRedirect(result.user.role));
        }
      }
    },
    [navigate],
  );

  const value: AuthContextValue = {
    token,
    user,
    claims,
    loading,
    login:          (email, password) => applyAuth(api.login(email, password)),
    registerBuyer:  (name, email, password) => applyAuth(api.registerBuyer(name, email, password)),
    registerArtist: (name, email, password) => applyAuth(api.registerArtist(name, email, password), false),
    logout: () => {
      clearStoredToken();
      setToken(null);
      setUser(null);
      navigate("/");
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
