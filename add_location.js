const fs = require('fs');
const p = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachDetail.js';
let c = fs.readFileSync(p, 'utf8');

const oldBtnRegex = /<motion\.button type="submit" disabled=\{resLoading\}.*?SIMDI REZERVE ET.*?<\/motion\.button>/s;

const newBtn = `<div className="flex flex-col sm:flex-row gap-3">
                          <motion.button type="submit" disabled={resLoading} whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }} className={\`w-full sm:flex-1 py-6 font-black text-lg rounded-[1.5rem] uppercase tracking-widest shadow-2xl transition-all flex items-center justify-center gap-3 \${!resLoading ? 'bg-gradient-to-r from-blue-600 to-indigo-700 text-white shadow-blue-500/30' : 'bg-slate-100 text-slate-400 cursor-not-allowed'}\`}>
                            {resLoading ? <Loader className="animate-spin" size={24} /> : <>ŞİMDİ REZERVE ET <TrendingUp size={20} /></>}
                          </motion.button>
                          
                          <motion.button 
                            type="button" 
                            disabled={!((beach?.latitude && beach?.longitude) || beach?.address)}
                            onClick={() => {
                              if (beach.latitude && beach.longitude) {
                                window.open(\`https://www.google.com/maps?q=\${beach.latitude},\${beach.longitude}\`, '_blank');
                              } else if (beach.address) {
                                window.open(\`https://www.google.com/maps/search/?api=1&query=\${encodeURIComponent(beach.address)}\`, '_blank');
                              }
                            }}
                            whileHover={((beach?.latitude && beach?.longitude) || beach?.address) ? { scale: 1.02 } : {}} 
                            whileTap={((beach?.latitude && beach?.longitude) || beach?.address) ? { scale: 0.98 } : {}} 
                            title={!((beach?.latitude && beach?.longitude) || beach?.address) ? "Konum bilgisi henüz eklenmedi" : "Haritada Göster"}
                            className={\`w-full sm:w-auto px-6 py-6 font-black text-sm rounded-[1.5rem] uppercase tracking-widest shadow-lg transition-all flex items-center justify-center gap-3 border-2 \${((beach?.latitude && beach?.longitude) || beach?.address) ? 'bg-white text-slate-800 border-slate-200 hover:bg-slate-50 cursor-pointer' : 'bg-slate-50 text-slate-400 border-slate-100 cursor-not-allowed'}\`}
                          >
                            Konumu Göster <MapPin size={20} />
                          </motion.button>
                        </div>`;

c = c.replace(oldBtnRegex, newBtn);

fs.writeFileSync(p, c, 'utf8');
console.log('SUCCESS NODE');
