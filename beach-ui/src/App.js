import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Toaster } from 'react-hot-toast';
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";

import Home from "./pages/Home";
import Beaches from "./pages/Beaches";
import BeachDetail from "./pages/BeachDetail";
import Events from "./pages/Events";
import ReservationCheck from "./pages/ReservationCheck";
import Login from "./pages/Login";
import Register from "./pages/Register";
import BusinessDashboard from "./pages/BusinessDashboard";

function App() {
  return (
    <Router>
      <Toaster position="top-right" reverseOrder={false} />
      <Navbar />
      <Routes>
        {/* Herkese AĂ§Äąk Rotalar */}
        <Route path="/" element={<Home />} />
        <Route path="/beaches" element={<Beaches />} />
        <Route path="/beach/:id" element={<BeachDetail />} />
        <Route path="/events" element={<Events />} />
        <Route path="/reservation-check" element={<ReservationCheck />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* KorumalÄą Rotalar (Sadece Ä°Ĺąletme Sahipleri) */}
        <Route 
          path="/business" 
          element={
            <ProtectedRoute allowedRoles={['BusinessOwner', 'Admin']}>
              <BusinessDashboard />
            </ProtectedRoute>
          } 
        />
      </Routes>
    </Router>
  );
}

export default App;
