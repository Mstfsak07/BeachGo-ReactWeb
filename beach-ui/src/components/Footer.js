import React from 'react';
import { Link } from 'react-router-dom';
import { 
  Palmtree, 
  Mail, 
  MapPin, 
  ChevronRight,
  ShieldCheck
} from 'lucide-react';

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-slate-900 text-slate-300 pt-20 pb-10 border-t border-white/5">
      <div className="container mx-auto px-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12 mb-16">
          
          {/* Marka Bölümü */}
          <div className="space-y-6">
            <div className="flex items-center space-x-3 group w-fit cursor-pointer">
              <div className="bg-blue-600 p-2 rounded-xl shadow-lg shadow-blue-500/20 group-hover:rotate-12 transition-transform">
                <Palmtree className="text-white" size={24} />
              </div>
              <span className="text-2xl font-black tracking-tighter text-white">
                Beach<span className="text-blue-500">Go</span>
              </span>
            </div>
            <p className="text-slate-400 leading-relaxed font-medium">
              Antalya'nın en seçkin plajlarını keşfedin, anlık doluluk oranlarını görün ve yerinizi saniyeler içinde ayırtın.
            </p>
          </div>

          {/* Menü Bölümü */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">Hızlı Menü</h4>
            <ul className="space-y-4 font-bold text-sm">
              <li><Link to="/" className="hover:text-blue-400 transition-colors">Ana Sayfa</Link></li>
              <li><Link to="/beaches" className="hover:text-blue-400 transition-colors">Plajları Gez</Link></li>
              <li><Link to="/business-register" className="hover:text-blue-400 transition-colors">İşletme Kaydı</Link></li>
            </ul>
          </div>

          {/* İletişim Bölümü */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">İletişim</h4>
            <ul className="space-y-6">
              <li className="flex items-start gap-4">
                <MapPin className="text-blue-500" size={20} />
                <p className="text-slate-400 text-sm font-medium">Lara, Muratpaşa, Antalya</p>
              </li>
              <li className="flex items-start gap-4">
                <Mail className="text-blue-500" size={20} />
                <p className="text-slate-400 text-sm font-medium">hello@beachgo.com</p>
              </li>
            </ul>
          </div>

          {/* Bülten Bölümü */}
          <div>
            <h4 className="text-white font-bold text-lg mb-6">Bültene Katıl</h4>
            <div className="flex gap-2">
              <input 
                type="email" 
                placeholder="E-posta" 
                className="w-full bg-white/5 border border-white/10 rounded-2xl py-3 px-4 focus:outline-none focus:border-blue-500 transition-all text-white font-medium"
              />
              <button className="bg-blue-600 hover:bg-blue-700 text-white p-3 rounded-xl transition-all shadow-lg shadow-blue-500/20">
                <ChevronRight size={20} />
              </button>
            </div>
            <div className="mt-6 flex items-center gap-2 text-slate-500 text-[10px] font-black tracking-widest uppercase">
              <ShieldCheck size={16} className="text-emerald-500" /> %100 Güvenli Sistem
            </div>
          </div>

        </div>

        {/* Alt Bar */}
        <div className="pt-10 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-6 text-sm font-bold text-slate-500">
          <p>© {currentYear} BeachGo. Tüm hakları saklıdır.</p>
          <div className="flex gap-8">
            <span className="hover:text-white cursor-pointer">Gizlilik</span>
            <span className="hover:text-white cursor-pointer">KVKK</span>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
