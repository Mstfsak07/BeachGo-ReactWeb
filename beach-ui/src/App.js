import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
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
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/beaches" element={<Beaches />} />
        <Route path="/beach/:id" element={<BeachDetail />} />
        <Route path="/events" element={<Events />} />
        <Route path="/reservation-check" element={<ReservationCheck />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/business" element={<BusinessDashboard />} />
      </Routes>
    </Router>
  );
}

export default App;
