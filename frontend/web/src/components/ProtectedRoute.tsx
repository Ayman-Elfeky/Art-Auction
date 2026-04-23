import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { hasAnyPermission, hasPermission } from "../lib/auth";
import type { Role } from "../types";

type ProtectedRouteProps = {
  roles?: Role[];
  /** Single permission (exact claim). */
  permission?: string;
  /** User may open the route if they have at least one of these (exact claims). */
  anyPermission?: readonly string[];
};

export function ProtectedRoute({ roles, permission, anyPermission }: ProtectedRouteProps) {
  const { loading, token, user, claims } = useAuth();

  if (loading) return <p>Loading session...</p>;
  if (!token || !user) return <Navigate to="/login" replace />;
  if (anyPermission && anyPermission.length > 0) {
    if (!hasAnyPermission(claims, anyPermission)) return <Navigate to="/" replace />;
  } else {
    if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />;
    if (permission && !hasPermission(claims, permission)) return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
