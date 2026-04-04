const fs = require('fs');

const pSettings = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachSettings.jsx';
let cSettings = fs.readFileSync(pSettings, 'utf8');

// Add imports
if (!cSettings.includes('InstagramContentPreviewModal')) {
    cSettings = cSettings.replace("import { motion } from 'framer-motion';", 
        "import { motion } from 'framer-motion';\nimport InstagramContentPreviewModal from '../components/admin/InstagramContentPreviewModal';\nimport { Instagram } from 'lucide-react';");
}

// Add state
if (!cSettings.includes('isPreviewModalOpen')) {
    cSettings = cSettings.replace('const [loading, setLoading] = useState(false);',
        'const [loading, setLoading] = useState(false);\n  const [isPreviewModalOpen, setIsPreviewModalOpen] = useState(false);');
}

// Add the Preview button and Modal
const btnHtml = `
                    <div className="col-span-1 md:col-span-2 mt-4">
                      <button
                        type="button"
                        onClick={() => setIsPreviewModalOpen(true)}
                        disabled={beach.socialContentSource !== 'instagram' || !beach.instagramUsername || beach.instagramUsername.length < 2}
                        className="flex items-center justify-center gap-2 w-full py-4 bg-gradient-to-r from-purple-600 to-rose-500 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-purple-500/30 hover:shadow-2xl transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Instagram size={20} /> Instagram İçeriğini Önizle
                      </button>
                      {(beach.socialContentSource !== 'instagram' || !beach.instagramUsername || beach.instagramUsername.length < 2) && (
                        <p className="text-[10px] text-slate-400 mt-2 text-center">Önizleme için önce geçerli bir Instagram kullanıcı adı girin.</p>
                      )}
                    </div>`;

if (!cSettings.includes('Instagram İçeriğini Önizle')) {
    const endOfSocialFields = /Bu kullanıcı adı ileride Instagram story ve galeri içeriğini otomatik doldurmak için kullanılacak\.<\/p>\s*\)\}\s*<\/div>/;
    cSettings = cSettings.replace(endOfSocialFields, match => match + '\n' + btnHtml);
    
    // Add modal at the end before final div
    const modalHtml = `
      <InstagramContentPreviewModal
        isOpen={isPreviewModalOpen}
        onClose={() => setIsPreviewModalOpen(false)}
        username={beach.instagramUsername}
        beachId={beach.id}
      />
    </div>
  );`;
    cSettings = cSettings.replace(/<\/div>\s*\);\s*\};\s*export default BeachSettings;/, match => modalHtml + '\n};\n\nexport default BeachSettings;');
}

fs.writeFileSync(pSettings, cSettings, 'utf8');
console.log('SUCCESS NODE');
