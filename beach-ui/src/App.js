import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";

import { AuthProvider } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./routes/ProtectedRoute";

import Home from "./pages/Home";
import Beaches from "./pages/Beaches";
import Login from "./pages/Login";
import Register from "./pages/Register";
import BusinessRegister from "./pages/BusinessRegister";

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

          <Route
            path="/beaches"
            element={
              <ProtectedRoute>
                <Beaches />
              </ProtectedRoute>
            }
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;