const fs = require('fs');

const p = 'C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/beach-ui/src/pages/BeachDetail.js';
let c = fs.readFileSync(p, 'utf8');

// 1. Add imports
const imports = `import BeachStoryBar from '../components/beach/BeachStoryBar';
import BeachGallery from '../components/beach/BeachGallery';
import { mockBeachStories, mockBeachGallery } from '../lib/mock/beachSocial';`;

if (!c.includes('BeachStoryBar')) {
  c = c.replace("import GoogleReviewsPlaceholder from '../components/GoogleReviewsPlaceholder';",
                "import GoogleReviewsPlaceholder from '../components/GoogleReviewsPlaceholder';\n" + imports);
}

// 2. Add Story Bar
const storyBarSearch = `<div className="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4 md:gap-6">`;
const storyBarReplace = `<BeachStoryBar stories={mockBeachStories} />\n              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4 md:gap-6">`;

if (!c.includes('<BeachStoryBar')) {
  c = c.replace(storyBarSearch, storyBarReplace);
}

// 3. Add Gallery
const facilitiesEndRegex = /\{facilities\.map\(\(f, i\) => \([\s\S]*?<\/div>\s*<\/div>\s*\)\}/;
if (!c.includes('<BeachGallery')) {
  const match = c.match(facilitiesEndRegex);
  if (match) {
    c = c.replace(facilitiesEndRegex, match[0] + "\n\n              <BeachGallery images={mockBeachGallery} />");
  }
}

fs.writeFileSync(p, c, 'utf8');
console.log('SUCCESS NODE');
