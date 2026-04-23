import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import { ADMIN_PORTAL_ACCESS_PERMISSIONS } from "./lib/auth";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { NavBar } from "./components/NavBar";
import { LangProvider } from "./context/LangContext";
import { AuthProvider } from "./context/AuthContext";
import { ToastProvider } from "./context/ToastContext";
import { AdminPage } from "./pages/AdminPage";
import { ArtistDashboardPage } from "./pages/ArtistDashboardPage";
import { ArtworkDetailPage } from "./pages/ArtworkDetailPage";
import { ArtworksPage } from "./pages/ArtworksPage";
import { LoginPage } from "./pages/LoginPage";
import { NotificationsPage } from "./pages/NotificationsPage";
import { RegisterArtistPage } from "./pages/RegisterArtistPage";
import { RegisterPage } from "./pages/RegisterPage";
import { SchemaPage } from "./pages/SchemaPage";
import { WatchlistPage } from "./pages/WatchlistPage";

function App() {
  return (
    <LangProvider>
      <BrowserRouter>
        <AuthProvider>
          <ToastProvider>
            <NavBar />
            <main>
              <Routes>
                <Route path="/" element={<ArtworksPage />} />
                <Route path="/artworks/:id" element={<ArtworkDetailPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/register-artist" element={<RegisterArtistPage />} />
                <Route path="/schema" element={<SchemaPage />} />

                <Route element={<ProtectedRoute roles={["User"]} permission="watchlist.manage" />}>
                  <Route path="/watchlist" element={<WatchlistPage />} />
                </Route>

                <Route
                  element={(
                    <ProtectedRoute
                      anyPermission={ADMIN_PORTAL_ACCESS_PERMISSIONS}
                    />
                  )}
                >
                  <Route path="/admin" element={<AdminPage />} />
                </Route>

                <Route element={<ProtectedRoute roles={["Artist"]} permission="artworks.create" />}>
                  <Route path="/artist" element={<ArtistDashboardPage />} />
                </Route>

                <Route element={<ProtectedRoute />}>
                  <Route path="/notifications" element={<NotificationsPage />} />
                </Route>
              </Routes>
            </main>
          </ToastProvider>
        </AuthProvider>
      </BrowserRouter>
    </LangProvider>
  );
}

export default App;
