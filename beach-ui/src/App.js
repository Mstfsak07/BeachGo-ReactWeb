import React, { Suspense, lazy } from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";

import { AuthProvider } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import ProtectedRoute from "./routes/ProtectedRoute";
import { useAuth } from "./context/AuthContext";

import Home from "./pages/Home";
import Beaches from "./pages/Beaches";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Unauthorized from "./pages/Unauthorized";

const BeachDetail = lazy(() => import("./pages/BeachDetail"));
const BusinessRegister = lazy(() => import("./pages/BusinessRegister"));
const MyReservations = lazy(() => import("./pages/MyReservations"));
const ReservationCheck = lazy(() => import("./pages/ReservationCheck"));
const Dashboard = lazy(() => import("./pages/Dashboard"));
const AdminPanel = lazy(() => import("./pages/AdminPanel"));
const BeachSettings = lazy(() => import("./pages/BeachSettings"));
const Events = lazy(() => import("./pages/Events"));
const DashboardStats = lazy(() => import("./pages/DashboardStats"));
const DashboardReservations = lazy(() => import("./pages/DashboardReservations"));
const GuestReservation = lazy(() => import("./pages/GuestReservation"));

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

        <Suspense fallback={<div className="min-h-screen flex items-center justify-center"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div></div>}>
        <Routes>
          {/* Public */}
          <Route path="/" element={<Home />} />
          <Route path="/beaches" element={<Beaches />} />
          <Route path="/beaches/:id" element={<BeachDetail />} />
          <Route path="/events" element={<Events />} />
          <Route path="/reservation-check" element={<ReservationCheck />} />
          <Route path="/reservation/:beachId" element={<GuestReservation />} />
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
        </Suspense>

        <Footer />
      </Router>
    </AuthProvider>
  );
}

export default App;
