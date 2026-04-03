const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  page.on('console', msg => {
    if (msg.type() === 'error') {
      console.log(`Browser Error: ${msg.text()}`);
    }
  });

  console.log("Navigating to http://localhost:3000/beaches ...");
  await page.goto('http://localhost:3000/beaches', { waitUntil: 'load' });
  await page.waitForTimeout(2000); // Give React time to render
  
  const h3s = await page.evaluate(() => {
    return Array.from(document.querySelectorAll('h3')).map(h => h.innerText || h.textContent);
  });
  console.log("Found h3 tags:", h3s.length);
  
  if (h3s.length > 0) {
    const firstBeachName = h3s[0];
    console.log(`Clicking on beach card: ${firstBeachName}`);
    
    const beachCard = page.locator(`text="${firstBeachName}"`).first();
    await beachCard.click();
    
    await page.waitForLoadState('load');
    await page.waitForTimeout(2000);
    
    const resBtn = page.locator('button', { hasText: /giris yapmadan devam et/i }).first();
    if (await resBtn.count() > 0) {
        console.log("Found button, clicking...");
        await resBtn.click();
        
        await page.waitForLoadState('load');
        await page.waitForTimeout(2000);
        console.log("Wizard page loaded URL:", page.url());
        
        const wizardHtml = await page.evaluate(() => {
            const root = document.querySelector('#root');
            return root ? root.innerHTML.substring(0, 800) : "No root found";
        });
        console.log("Wizard HTML Preview:", wizardHtml);
    } else {
      console.log("Reservation button not found.");
    }
  } else {
     console.log("No beaches found.");
  }
  
  await browser.close();
})();
