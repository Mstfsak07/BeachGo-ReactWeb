import React, { Suspense, lazy } from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { AuthProvider, useAuth } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import PrivateRoute from "./components/PrivateRoute";
import ProtectedRoute from "./routes/ProtectedRoute";
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
const ReservationSuccess = lazy(() => import("./pages/ReservationSuccess"));

// Placeholder components for requested routes
const Profile = () => <div className="pt-32 px-10"><h1>Profilim</h1><p>Yakında...</p></div>;
const Favorites = () => <div className="pt-32 px-10"><h1>Favorilerim</h1><p>Yakında...</p></div>;
const Reservations = () => <div className="pt-32 px-10"><h1>Rezervasyonlarım</h1><p>Yakında...</p></div>;
const ForgotPassword = () => <div className="pt-32 px-10"><h1>Şifremi Unuttum</h1><p>Yakında...</p></div>;

const GuestOnlyRoute = ({ children }) => {
  const { isAuthenticated, loading, user } = useAuth();
  if (loading) return null;
  if (isAuthenticated) {
    if (user?.role === "Business" || user?.role === "Admin") return <Navigate to="/dashboard" replace />;
    return <Navigate to="/" replace />;
  }
  return children;
};

const Spinner = () => (
  <div className="min-h-screen flex items-center justify-center">
    <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600" />
  </div>
);

const AppContent = () => {
  const { loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <div className="ml-4 font-bold text-slate-600">Yukleniyor...</div>
      </div>
    );
  }

  return (
    <Router>
      <Toaster position="top-right" reverseOrder={false} />
      <Navbar />
      <Suspense fallback={<Spinner />}>
        <Routes>
          {/* Public Routes */}
          <Route path="/" element={<Home />} />
          <Route path="/beaches" element={<Beaches />} />
          <Route path="/beaches/:id" element={<BeachDetail />} />
          <Route path="/events" element={<Events />} />
          <Route path="/reservation-check" element={<ReservationCheck />} />
          <Route path="/reservation/:beachId" element={<GuestReservation />} />
          <Route path="/reservation-success" element={<ReservationSuccess />} />
          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />

          {/* Guest Only Routes */}
          <Route path="/login" element={<GuestOnlyRoute><Login /></GuestOnlyRoute>} />
          <Route path="/register" element={<GuestOnlyRoute><Register /></GuestOnlyRoute>} />
          <Route path="/business-register" element={<GuestOnlyRoute><BusinessRegister /></GuestOnlyRoute>} />

          {/* Protected Routes */}
          <Route element={<PrivateRoute />}>
            <Route path="/profile" element={<Profile />} />
            <Route path="/reservations" element={<Reservations />} />
            <Route path="/favorites" element={<Favorites />} />
            <Route path="/my-reservations" element={<MyReservations />} />
          </Route>

          {/* Role Protected Routes */}
          <Route path="/dashboard" element={<ProtectedRoute allowedRoles={["Business","Admin"]}><Dashboard /></ProtectedRoute>} />
          <Route path="/dashboard/stats" element={<ProtectedRoute allowedRoles={["Business","Admin"]}><DashboardStats /></ProtectedRoute>} />
          <Route path="/dashboard/reservations" element={<ProtectedRoute allowedRoles={["Business","Admin"]}><DashboardReservations /></ProtectedRoute>} />
          <Route path="/beach-settings" element={<ProtectedRoute allowedRoles={["Business","Admin"]}><BeachSettings /></ProtectedRoute>} />
          <Route path="/admin" element={<ProtectedRoute allowedRoles={["Admin"]}><AdminPanel /></ProtectedRoute>} />
          
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
      <Footer />
    </Router>
  );
};

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
