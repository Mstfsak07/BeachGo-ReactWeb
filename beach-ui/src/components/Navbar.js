import React, { useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Navbar = () => {
  const [isScrolled, setIsScrolled] = useState(false);
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const navLinks = [
    { name: 'Keþfet', path: '/beaches' },
    { name: 'Etkinlikler', path: '/events' },
    { name: 'Rezervasyon Sorgu', path: '/reservation-check' },
  ];

  return (
    <nav className={\ixed top-0 left-0 right-0 z-50 transition-all duration-300 \ backdrop-blur-sm\}>
      <div className=\"container mx-auto px-6 flex justify-between items-center\">
        {/* Logo */}
        <Link to=\"/\" className=\"flex items-center space-x-2 group\">
          <div className=\"bg-blue-600 p-2 rounded-xl group-hover:rotate-12 transition-transform\">
             <span className=\"text-white text-xl font-black\">B</span>
          </div>
          <span className=\"text-2xl font-black tracking-tighter text-slate-800\">
            Beach<span className=\"text-blue-600\">Go</span>
          </span>
        </Link>

        {/* Desktop Links */}
        <div className=\"hidden md:flex items-center space-x-8\">
          {navLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              className={\	ext-sm font-bold tracking-wide uppercase transition-colors hover:text-blue-600 \\}
            >
              {link.name}
            </Link>
          ))}

          {user ? (
            <div className=\"flex items-center space-x-4\">
              <span className=\"text-sm font-medium text-slate-500\">{user.email}</span>
              <button 
                onClick={handleLogout}
                className=\"bg-slate-100 text-slate-700 px-6 py-2 rounded-xl text-sm font-bold hover:bg-slate-200 transition-colors\"
              >
                Çýkýþ Yap
              </button>
            </div>
          ) : (
            <Link 
              to=\"/login\" 
              className=\"bg-blue-600 text-white px-6 py-2 rounded-xl text-sm font-bold hover:bg-blue-700 transition-all shadow-lg shadow-blue-200\"
            >
              Giriþ Yap
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
