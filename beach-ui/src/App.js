import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";

import { AuthProvider } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import ProtectedRoute from "./routes/ProtectedRoute";

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

function App() {
  return (
    <AuthProvider>
      <Router>
        <Toaster position="top-right" reverseOrder={false} />
        <Navbar />

        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/business-register" element={<BusinessRegister />} />

          <Route path="/beaches" element={<Beaches />} />
          <Route path="/beaches/:id" element={<BeachDetail />} />

          <Route
            path="/my-reservations"
            element={
              <ProtectedRoute>
                <MyReservations />
              </ProtectedRoute>
            }
          />

          <Route
            path="/reservation-check"
            element={<ReservationCheck />}
          />

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

          <Route path="/events" element={<Events />} />

          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <AdminPanel />
              </ProtectedRoute>
            }
          />
        </Routes>
        <Footer />
      </Router>
    </AuthProvider>
  );
}

export default App;
