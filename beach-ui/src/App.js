import React from \"react\";
import { BrowserRouter as Router, Routes, Route } from \"react-router-dom\";
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext';
import Navbar from \"./components/Navbar\";
import ProtectedRoute from \"./routes/ProtectedRoute\";

import Home from \"./pages/Home\";
import Dashboard from \"./pages/Dashboard\";
import LoginPage from \"./pages/LoginPage\";
import Register from \"./pages/Register\";

function App() {
  return (
    <AuthProvider>
      <Router>
        <Toaster position=\"top-right\" reverseOrder={false} />
        <Navbar />
        <Routes>
          {/* Halka Açýk Rotalar */}
          <Route path=\"/\" element={<Home />} />
          <Route path=\"/login\" element={<LoginPage />} />
          <Route path=\"/register\" element={<Register />} />
          
          {/* Korumalý Rotalar */}
          <Route 
            path=\"/beaches\" 
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } 
          />
          <Route 
            path=\"/beaches/:id\" 
            element={
              <ProtectedRoute>
                <div>Beach Detail (Korumalý)</div>
              </ProtectedRoute>
            } 
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
