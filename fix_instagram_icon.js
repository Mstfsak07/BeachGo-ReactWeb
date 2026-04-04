const fs = require('fs');

const pSettings = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachSettings.jsx';
let cSettings = fs.readFileSync(pSettings, 'utf8');
cSettings = cSettings.replace(/import \{ Instagram \} from 'lucide-react';/, '');
cSettings = cSettings.replace(/<Instagram/g, '<Camera');
fs.writeFileSync(pSettings, cSettings, 'utf8');

const pModal = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/components/admin/InstagramContentPreviewModal.jsx';
let cModal = fs.readFileSync(pModal, 'utf8');
cModal = cModal.replace(/Instagram \} from 'lucide-react';/, "} from 'lucide-react';\nimport { Camera } from 'lucide-react';");
cModal = cModal.replace(/<Instagram/g, '<Camera');
fs.writeFileSync(pModal, cModal, 'utf8');
