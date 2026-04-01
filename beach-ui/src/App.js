import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";

import { AuthProvider } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import ProtectedRoute from "./routes/ProtectedRoute";
import { useAuth } from "./context/AuthContext";

import Home from "./pages/Home";
import Beaches from "./pages/Beaches";
import BeachDetail from "./pages/BeachDetail";
import Login from "./pages/Login";
import Register from "./pages/Register";
import BusinessRegister from "./pages/BusinessRegister";
import MyReservations from "./pages/MyReservations";
import ReservationCheck from "./pages/ReservationCheck";
import Dashboard from "./pages/Dashboard";
import AdminPanel from "./pages/AdminPanel";
import BeachSettings from "./pages/BeachSettings";
import Events from "./pages/Events";
import DashboardStats from "./pages/DashboardStats";
import DashboardReservations from "./pages/DashboardReservations";
import Unauthorized from "./pages/Unauthorized";

// Giriş yapmış kullanıcıyı login/register'dan uygun sayfaya yönlendir
const GuestOnlyRoute = ({ children }) => {
  const { isAuthenticated, loading, user } = useAuth();
  if (loading) return null;
  if (isAuthenticated) {
    if (user?.role === "Business" || user?.role === "Admin") return <Navigate to="/dashboard" replace />;
    return <Navigate to="/" replace />;
  }
  return children;
};

function App() {
  return (
    <AuthProvider>
      <Router>
        <Toaster position="top-right" reverseOrder={false} />
        <Navbar />

        <Routes>
          {/* Public */}
          <Route path="/" element={<Home />} />
          <Route path="/beaches" element={<Beaches />} />
          <Route path="/beaches/:id" element={<BeachDetail />} />
          <Route path="/events" element={<Events />} />
          <Route path="/reservation-check" element={<ReservationCheck />} />
          <Route path="/unauthorized" element={<Unauthorized />} />

          {/* Guest only — authenticated users are redirected */}
          <Route path="/login" element={<GuestOnlyRoute><Login /></GuestOnlyRoute>} />
          <Route path="/register" element={<GuestOnlyRoute><Register /></GuestOnlyRoute>} />
          <Route path="/business-register" element={<GuestOnlyRoute><BusinessRegister /></GuestOnlyRoute>} />

          {/* Auth required — any role */}
          <Route
            path="/my-reservations"
            element={
              <ProtectedRoute>
                <MyReservations />
              </ProtectedRoute>
            }
          />

          {/* Business + Admin only */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={["Business", "Admin"]}>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/beach-settings"
            element={
              <ProtectedRoute allowedRoles={["Business", "Admin"]}>
                <BeachSettings />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/stats"
            element={
              <ProtectedRoute allowedRoles={["Business", "Admin"]}>
                <DashboardStats />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/reservations"
            element={
              <ProtectedRoute allowedRoles={["Business", "Admin"]}>
                <DashboardReservations />
              </ProtectedRoute>
            }
          />

          {/* Admin only */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <AdminPanel />
              </ProtectedRoute>
            }
          />

          {/* Catch-all — bilinmeyen route'lar anasayfaya */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>

        <Footer />
      </Router>
    </AuthProvider>
  );
}

export default App;
