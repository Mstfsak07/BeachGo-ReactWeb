import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { 
  LayoutDashboard, 
  CalendarCheck, 
  Users, 
  Palmtree, 
  Settings, 
  LogOut,
  ChevronRight,
  ShieldCheck,
  BarChart3,
  Home
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const Sidebar = ({ role }) => {
  const location = useLocation();
  const { logout, user } = useAuth();

  const businessLinks = [
    { name: 'Genel Bakış', path: '/dashboard', icon: LayoutDashboard },
    { name: 'Rezervasyonlar', path: '/dashboard/reservations', icon: CalendarCheck },
    { name: 'Plaj Bilgileri', path: '/dashboard/beach-settings', icon: Palmtree },
    { name: 'İstatistikler', path: '/dashboard/stats', icon: BarChart3 },
  ];

  const adminLinks = [
    { name: 'Admin Panel', path: '/admin', icon: ShieldCheck },
    { name: 'Tüm Plajlar', path: '/admin/beaches', icon: Palmtree },
    { name: 'Kullanıcılar', path: '/admin/users', icon: Users },
    { name: 'Sistem Ayarları', path: '/admin/settings', icon: Settings },
  ];

  const links = role === 'Admin' ? adminLinks : businessLinks;

  return (
    <>
      <div className="sticky top-0 z-40 w-full border-b border-slate-200 bg-white/95 backdrop-blur md:hidden">
        <div className="flex items-center justify-between gap-4 px-4 py-4">
          <Link to="/" className="flex min-w-0 items-center gap-3">
            <div className="rounded-xl bg-blue-600 p-2 shadow-lg shadow-blue-500/20">
              <Palmtree className="text-white" size={20} />
            </div>
            <div className="min-w-0">
              <p className="truncate text-sm font-black tracking-tight text-slate-900">
                Beach<span className="text-blue-500">Go</span>
              </p>
              <p className="truncate text-[10px] font-black uppercase tracking-widest text-blue-500">{role} Paneli</p>
            </div>
          </Link>
          <button
            onClick={logout}
            className="rounded-xl bg-rose-50 p-3 text-rose-500 transition-colors hover:bg-rose-100"
            aria-label="Çıkış Yap"
          >
            <LogOut size={18} />
          </button>
        </div>
        <nav className="flex flex-wrap gap-2 px-4 pb-4">
          {links.map((link) => {
            const isActive = location.pathname === link.path;
            return (
              <Link
                key={link.path}
                to={link.path}
                className={`inline-flex items-center gap-2 rounded-full px-4 py-2 text-xs font-black uppercase tracking-wide transition-all ${
                  isActive
                    ? 'bg-blue-600 text-white shadow-lg shadow-blue-200'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                <link.icon size={14} />
                {link.name}
              </Link>
            );
          })}
          <Link
            to="/"
            className="inline-flex items-center gap-2 rounded-full bg-slate-100 px-4 py-2 text-xs font-black uppercase tracking-wide text-slate-600 hover:bg-slate-200"
          >
            <Home size={14} />
            Ana Sayfa
          </Link>
        </nav>
      </div>

      <aside className="fixed left-0 top-0 z-50 hidden h-screen w-72 flex-col border-r border-white/5 bg-slate-900 text-slate-300 md:flex">
        <div className="p-8 border-b border-white/5">
          <Link to="/" className="flex items-center space-x-3 group">
            <div className="bg-blue-600 p-2 rounded-xl shadow-lg shadow-blue-500/20 group-hover:rotate-12 transition-transform">
              <Palmtree className="text-white" size={24} />
            </div>
            <span className="text-2xl font-black tracking-tighter text-white">
              Beach<span className="text-blue-500">Go</span>
            </span>
          </Link>
          <div className="mt-4 px-3 py-1 bg-blue-500/10 border border-blue-500/20 rounded-lg w-fit">
            <span className="text-[10px] font-black uppercase tracking-widest text-blue-400">{role} Paneli</span>
          </div>
        </div>

        <nav className="flex-1 p-6 space-y-2 overflow-y-auto">
          {links.map((link) => {
            const isActive = location.pathname === link.path;
            return (
              <Link
                key={link.path}
                to={link.path}
                className={`flex items-center justify-between p-4 rounded-2xl transition-all group ${
                  isActive
                    ? 'bg-blue-600 text-white shadow-xl shadow-blue-600/20'
                    : 'hover:bg-white/5 hover:text-white'
                }`}
              >
                <div className="flex items-center gap-4">
                  <link.icon size={20} strokeWidth={isActive ? 2.5 : 2} />
                  <span className={`font-bold text-sm ${isActive ? 'text-white' : ''}`}>{link.name}</span>
                </div>
                <ChevronRight size={16} className={`transition-transform duration-300 ${isActive ? 'opacity-100 rotate-90' : 'opacity-0'}`} />
              </Link>
            );
          })}
        </nav>

        <div className="p-6 border-t border-white/5 space-y-4">
          <div className="bg-white/5 p-4 rounded-2xl flex items-center gap-4">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-blue-500 to-indigo-600 flex items-center justify-center font-black text-white shadow-lg">
              {user?.email?.charAt(0).toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-bold text-white truncate">{user?.email?.split('@')[0]}</p>
              <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest truncate">İşletme Yetkilisi</p>
            </div>
          </div>
          <button
            onClick={logout}
            className="w-full flex items-center gap-4 p-4 rounded-2xl text-rose-400 hover:bg-rose-500/10 transition-all font-bold text-sm"
          >
            <LogOut size={20} />
            Çıkış Yap
          </button>
        </div>
      </aside>
    </>
  );
};

export default Sidebar;
