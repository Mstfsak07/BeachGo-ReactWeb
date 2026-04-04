const fs = require('fs');

function fix(p) {
    if (!fs.existsSync(p)) return;
    let c = fs.readFileSync(p, 'utf8');
    c = c.replace(/\\\`/g, '`');
    c = c.replace(/\\\$/g, '$');
    fs.writeFileSync(p, c, 'utf8');
}

fix('C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/components/beach/BeachStoryBar.jsx');
fix('C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/components/beach/BeachStoryViewer.jsx');
fix('C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/components/beach/BeachGalleryLightbox.jsx');
console.log('SUCCESS');
