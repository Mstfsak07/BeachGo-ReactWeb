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
  BarChart3
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
    <aside className="fixed left-0 top-0 h-screen w-72 bg-slate-900 text-slate-300 flex flex-col z-50 border-r border-white/5">
      {/* Brand */}
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

      {/* Navigation */}
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

      {/* Footer / User */}
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
  );
};

export default Sidebar;
