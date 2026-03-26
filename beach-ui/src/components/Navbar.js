import React, { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';

const Navbar = () => {
  const [isScrolled, setIsScrolled] = useState(false);
  const location = useLocation();

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const navLinks = [
    { name: 'Keşfet', path: '/beaches' },
    { name: 'Etkinlikler', path: '/events' },
    { name: 'Rezervasyon Sordu', path: '/reservation-check' },
  ];

  return (
    <nav className={`glass-nav transition-all duration-300 ${
      isScrolled ? 'py-2 shadow-md bg-white/90' : 'py-4 shadow-none bg-white/50'
    }`}>
      <div className="container mx-auto px-6 flex justify-between items-center">
        {/* Logo */}
        <Link to="/" className="flex items-center space-x-2 group">
          <div className="bg-primary-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
             <span className="text-white text-xl font-black">B</span>
          </div>
          <span className="text-2xl font-black tracking-tighter text-slate-800">
            Beach<span className="text-primary-500">Go</span>
          </span>
        </Link>

        {/* Desktop Links */}
        <div className="hidden md:flex items-center space-x-8">
          {navLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              className={`text-sm font-bold tracking-wide uppercase transition-colors hover:text-primary-500 ${
                location.pathname === link.path ? 'text-primary-500' : 'text-slate-600'
              }`}
            >
              {link.name}
            </Link>
          ))}
          
          <Link to="/login" className="btn-secondary py-2 text-sm">
            İşletme Girişi
          </Link>
        </div>

        {/* Mobile Menu Icon (Simple version) */}
        <div className="md:hidden text-slate-800">
           <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16m-7 6h7" />
           </svg>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
