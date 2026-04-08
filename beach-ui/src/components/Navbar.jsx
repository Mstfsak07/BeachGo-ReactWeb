import React, { useState, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { LogOut, User, Menu, X, Palmtree } from "lucide-react";

const Navbar = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/login");
    setIsMobileMenuOpen(false);
  };

  const navLinks = [
    { name: "Ana Sayfa", path: "/" },
    { name: "Plajlar", path: "/beaches" },
    { name: "Etkinlikler", path: "/events" },
  ];

  const authLinks = [
    { name: "Profilim", path: "/profile" },
    { name: "Rezervasyonlarım", path: "/reservations" },
    { name: "Favorilerim", path: "/favorites" },
  ];

  const isHomePage = location.pathname === "/";
  const shouldShowSolid = isScrolled || !isHomePage;

  return (
    <nav 
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-500 ${
        shouldShowSolid 
          ? "bg-white/80 backdrop-blur-xl shadow-lg shadow-slate-200/50 py-3" 
          : "bg-transparent py-5"
      }`}
    >
      <div className="container mx-auto px-3 sm:px-6 flex justify-between items-center">
        
        <Link to="/" className="flex items-center space-x-3 group">
          <div className="bg-blue-600 p-2.5 rounded-2xl shadow-xl shadow-blue-200 group-hover:rotate-12 transition-transform duration-300">
            <Palmtree className="text-white" size={24} />
          </div>
          <span className={`text-2xl md:text-3xl font-black tracking-tighter transition-colors duration-500 ${
            shouldShowSolid ? "text-slate-900" : "text-white"
          }`}>
            Beach<span className="text-blue-600 underline decoration-blue-200 decoration-4 underline-offset-4">Go</span>
          </span>
        </Link>

        <div className="hidden md:flex items-center space-x-10">
          {navLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              className={`text-base md:text-lg font-bold tracking-tight transition-all duration-300 hover:text-blue-400 relative group ${
                location.pathname === link.path 
                  ? "text-blue-500" 
                  : (shouldShowSolid ? "text-slate-600" : "text-white/90")
              }`}
            >
              {link.name}
            </Link>
          ))}
          {isAuthenticated && authLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              className={`text-base md:text-lg font-bold tracking-tight transition-all duration-300 hover:text-blue-400 relative group ${
                location.pathname === link.path 
                  ? "text-blue-500" 
                  : (shouldShowSolid ? "text-slate-600" : "text-white/90")
              }`}
            >
              {link.name}
            </Link>
          ))}
        </div>

        <div className="hidden md:flex items-center space-x-6">
          {isAuthenticated ? (
            <div className={`flex items-center gap-6 backdrop-blur-md pl-6 pr-2 py-2 rounded-3xl border transition-all duration-500 shadow-sm ${
              shouldShowSolid ? "bg-slate-50/80 border-slate-100" : "bg-white/10 border-white/20"
            }`}>
              <div className="flex flex-col items-end">
                <span className={`text-[11px] font-black uppercase tracking-[0.2em] leading-none mb-1.5 ${
                  shouldShowSolid ? "text-slate-400" : "text-white/50"
                }`}>Kullanıcı</span>
                <span className={`text-sm font-bold leading-none truncate max-w-[120px] ${
                  shouldShowSolid ? "text-slate-800" : "text-white"
                }`}>{user?.name || user?.email?.split('@')[0]}</span>
              </div>
              <button
                onClick={handleLogout}
                className="p-3 bg-white text-rose-500 rounded-2xl hover:bg-rose-500 hover:text-white transition-all shadow-md border border-slate-100 group"
                title="Çıkış Yap"
              >
                <LogOut size={20} className="group-hover:-translate-x-1 transition-transform" />
              </button>
            </div>
          ) : (
            <div className="flex items-center space-x-4">
              <Link
                to="/login"
                className={`text-sm font-black tracking-[0.15em] uppercase transition-colors ${
                  shouldShowSolid ? "text-slate-600 hover:text-blue-600" : "text-white hover:text-blue-200"
                }`}
              >
                Giriş Yap
              </Link>
              <Link
                to="/register"
                className="px-6 py-3 bg-blue-600 text-white text-sm font-black tracking-[0.15em] uppercase rounded-2xl hover:bg-blue-700 transition-all active:scale-95 shadow-xl shadow-blue-100"
              >
                Kayıt Ol
              </Link>
            </div>
          )}
        </div>

        <button 
          className={`md:hidden p-2 transition-colors ${shouldShowSolid ? "text-slate-600" : "text-white"}`}
          onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
        >
          {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>

      <div className={`md:hidden absolute top-full left-0 right-0 bg-white border-t border-slate-100 shadow-2xl transition-all duration-300 ${
        isMobileMenuOpen ? "opacity-100 translate-y-0" : "opacity-0 -translate-y-4 pointer-events-none"
      }`}>
        <div className="p-6 flex flex-col space-y-4">
          {navLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              onClick={() => setIsMobileMenuOpen(false)}
              className={`text-lg font-bold ${
                location.pathname === link.path ? "text-blue-600" : "text-slate-600"
              }`}
            >
              {link.name}
            </Link>
          ))}
          {isAuthenticated && authLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              onClick={() => setIsMobileMenuOpen(false)}
              className={`text-lg font-bold ${
                location.pathname === link.path ? "text-blue-600" : "text-slate-600"
              }`}
            >
              {link.name}
            </Link>
          ))}
          <div className="pt-4 border-t border-slate-100">
            {isAuthenticated ? (
              <div className="space-y-4">
                <div className="flex items-center gap-3 text-slate-700">
                  <User size={20} className="text-blue-600" />
                  <span className="font-bold truncate max-w-[200px]">{user?.name || user?.email}</span>
                </div>
                <button
                  onClick={handleLogout}
                  className="w-full py-4 bg-red-50 text-red-500 font-black tracking-widest uppercase rounded-xl flex items-center justify-center gap-2"
                >
                  <LogOut size={18} /> Çıkış Yap
                </button>
              </div>
            ) : (
              <div className="flex flex-col space-y-3">
                <Link
                  to="/login"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="block w-full py-4 bg-slate-100 text-slate-800 text-center font-black tracking-widest uppercase rounded-xl"
                >
                  Giriş Yap
                </Link>
                <Link
                  to="/register"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="block w-full py-4 bg-blue-600 text-white text-center font-black tracking-widest uppercase rounded-xl shadow-lg shadow-blue-200"
                >
                  Kayıt Ol
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
