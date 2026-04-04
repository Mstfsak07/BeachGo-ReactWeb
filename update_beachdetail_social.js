const fs = require('fs');
const p = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachDetail.js';
let c = fs.readFileSync(p, 'utf8');

c = c.replace(
    "import { mockBeachStories, mockBeachGallery } from '../lib/mock/beachSocial';",
    "import { getBeachSocialProvider } from '../lib/social/getBeachSocialProvider';"
);

if (!c.includes('const [socialContent')) {
    c = c.replace(
        "const [isFavorite, setIsFavorite] = useState(false);",
        "const [isFavorite, setIsFavorite] = useState(false);\n  const [socialContent, setSocialContent] = useState({ stories: [], gallery: [] });"
    );
}

const fetchSearch = `      const data = await getBeachById(id);
      if (data) {
        setBeach(data);
      } else {`;

const fetchReplace = `      const data = await getBeachById(id);
      if (data) {
        setBeach(data);
        
        try {
          const provider = getBeachSocialProvider(data);
          const content = await provider.getContent(data);
          setSocialContent(content);
        } catch (socialErr) {
          console.error("Sosyal icerik yuklenirken hata:", socialErr);
        }
      } else {`;

if (!c.includes('getBeachSocialProvider(data)')) {
    c = c.replace(fetchSearch, fetchReplace);
}

c = c.replace("stories={mockBeachStories}", "stories={socialContent.stories}");
c = c.replace("images={mockBeachGallery}", "images={socialContent.gallery}");

fs.writeFileSync(p, c, 'utf8');
console.log('SUCCESS');
