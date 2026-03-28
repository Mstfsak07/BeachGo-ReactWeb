import React from "react";
import { Link } from "react-router-dom";

const Navbar = () => {
  return (
    <nav className="fixed top-0 left-0 right-0 z-50 transition-all duration-300 backdrop-blur-md bg-white/70 shadow-sm">
      <div className="container mx-auto px-6 flex justify-between items-center h-16">

        {/* Logo */}
        <Link to="/" className="flex items-center space-x-2 font-bold text-lg">
          🏖️ <span>BeachGo</span>
        </Link>

        {/* Menü */}
        <div className="flex items-center space-x-4">
          <Link to="/" className="hover:text-blue-500">Home</Link>
          <Link to="/beaches" className="hover:text-blue-500">Beaches</Link>
          <Link to="/login" className="hover:text-blue-500">Login</Link>
          <Link to="/register" className="hover:text-blue-500">Register</Link>
        </div>

      </div>
    </nav>
  );
};

export default Navbar;