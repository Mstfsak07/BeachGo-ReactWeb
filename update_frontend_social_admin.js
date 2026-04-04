const fs = require('fs');

// 1. BeachSettings.jsx
const pSettings = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachSettings.jsx';
let cSettings = fs.readFileSync(pSettings, 'utf8');

if (!cSettings.includes('Share2')) {
    cSettings = cSettings.replace('Camera,', 'Camera,\n    Share2,\n    AtSign,');
}

if (!cSettings.includes('socialContentSource')) {
    // Add to initial state
    cSettings = cSettings.replace(
        "instagram: '',", 
        "instagram: '',\n      instagramUsername: '',\n      socialContentSource: 'mock',"
    );

    // Add to fetch data mapping
    cSettings = cSettings.replace(
        "instagram: data.instagram || '',",
        "instagram: data.instagram || '',\n            instagramUsername: data.instagramUsername || '',\n            socialContentSource: data.socialContentSource || 'mock',"
    );

    // Add the form fields
    const socialFields = `                    <div className="space-y-2">
                      <label className={labelClass}>Sosyal İçerik Kaynağı</label>
                      <div className="relative group">
                        <Share2 className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                        <select
                          value={beach.socialContentSource}
                          onChange={(e) => setBeach({...beach, socialContentSource: e.target.value, instagramUsername: e.target.value === 'mock' ? '' : beach.instagramUsername})}
                          className={inputClass}
                        >
                          <option value="mock">Mock Veri</option>
                          <option value="instagram">Instagram</option>
                        </select>
                      </div>
                    </div>
                    <div className="space-y-2">
                      <label className={labelClass}>Instagram Kullanıcı Adı</label>
                      <div className="relative group">
                        <AtSign className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                        <input 
                          type="text" 
                          value={beach.instagramUsername} 
                          onChange={(e) => setBeach({...beach, instagramUsername: e.target.value.replace(/^@+/, '').trim()})} 
                          placeholder="ornekhesap" 
                          disabled={beach.socialContentSource === 'mock'}
                          required={beach.socialContentSource === 'instagram'}
                          minLength={beach.socialContentSource === 'instagram' ? 2 : 0}
                          pattern="^[^\\\\s]+$"
                          className={\`\${inputClass} \${beach.socialContentSource === 'mock' ? 'opacity-50 cursor-not-allowed' : ''}\`} 
                        />
                      </div>
                      {beach.socialContentSource === 'instagram' && (
                        <p className="text-[10px] font-bold text-blue-600 mt-1 pl-2">Bu kullanıcı adı ileride Instagram story ve galeri içeriğini otomatik doldurmak için kullanılacak.</p>
                      )}
                    </div>`;
    
    // Insert after Instagram field
    const instaFieldRegex = /<label className=\{labelClass\}>Instagram<\/label>[\s\S]*?<\/div>\s*<\/div>/;
    cSettings = cSettings.replace(instaFieldRegex, match => match + '\n' + socialFields);

    fs.writeFileSync(pSettings, cSettings, 'utf8');
}

// 2. AdminPanel.jsx
const pAdmin = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/AdminPanel.jsx';
let cAdmin = fs.readFileSync(pAdmin, 'utf8');

if (!cAdmin.includes('Social Source')) {
    // Add column header
    cAdmin = cAdmin.replace(
        '<th className="px-8 py-5 text-left">Durum</th>',
        '<th className="px-8 py-5 text-left">Durum</th>\n                  <th className="px-8 py-5 text-left">Social Source</th>'
    );

    // Add column data
    const badgeHtml = `                      <td className="px-8 py-5">
                        <span className={\`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest \${
                          beach.socialContentSource === 'instagram' ? 'bg-purple-50 text-purple-600' : 'bg-slate-100 text-slate-600'
                        }\`}>
                          {beach.socialContentSource === 'instagram' ? \`Instagram · \${beach.instagramUsername}\` : 'Mock'}
                        </span>
                      </td>`;
                      
    cAdmin = cAdmin.replace(
        /beach\.isActive \? 'Aktif' : 'Pasif'\}[\s\S]*?<\/span>[\s\S]*?<\/td>/,
        match => match + '\n' + badgeHtml
    );

    fs.writeFileSync(pAdmin, cAdmin, 'utf8');
}

console.log("SUCCESS NODE");
