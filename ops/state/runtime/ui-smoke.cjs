const { chromium } = require('playwright');

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  const results = [];

  async function record(name, fn) {
    try {
      const detail = await fn();
      results.push({ name, status: 'PASS', detail });
    } catch (error) {
      results.push({ name, status: 'FAIL', detail: error.message });
    }
  }

  await record('home-renders', async () => {
    await page.goto('http://localhost:3000', { waitUntil: 'networkidle' });
    const title = await page.title();
    if (!title.includes('Beach')) throw new Error(`unexpected title: ${title}`);
    return title;
  });

  await record('login-page-renders', async () => {
    await page.goto('http://localhost:3000/login', { waitUntil: 'networkidle' });
    await page.locator('input[type="email"]').waitFor();
    await page.locator('input[type="password"]').waitFor();
    const heading = await page.locator('h2').first().innerText();
    return heading;
  });

  await record('login-validation', async () => {
    await page.goto('http://localhost:3000/login', { waitUntil: 'networkidle' });
    await page.fill('input[type="email"]', 'bad-email');
    await page.fill('input[type="password"]', '123');
    await page.click('button[type="submit"]');
    await page.locator('text=Geçerli bir e-posta adresi girin.').waitFor();
    await page.locator('text=Şifre en az 6 karakter olmalıdır.').waitFor();
    return 'client validation errors shown';
  });

  await record('register-page-renders', async () => {
    await page.goto('http://localhost:3000/register', { waitUntil: 'networkidle' });
    await page.locator('input[name="name"]').waitFor();
    await page.locator('input[name="email"]').waitFor();
    await page.locator('input[name="password"]').waitFor();
    await page.locator('input[name="confirmPassword"]').waitFor();
    const heading = await page.locator('h2').first().innerText();
    return heading;
  });

  await record('register-validation', async () => {
    await page.goto('http://localhost:3000/register', { waitUntil: 'networkidle' });
    await page.fill('input[name="name"]', 'Test User');
    await page.fill('input[name="email"]', 'wrong');
    await page.fill('input[name="password"]', '123456');
    await page.fill('input[name="confirmPassword"]', '654321');
    await page.click('button[type="submit"]');
    await page.locator('text=Geçerli bir e-posta adresi girin.').waitFor();
    await page.locator('text=Şifreler eşleşmiyor.').waitFor();
    return 'client validation errors shown';
  });

  await record('protected-route-redirect', async () => {
    await page.goto('http://localhost:3000/dashboard', { waitUntil: 'networkidle' });
    if (!page.url().includes('/login')) throw new Error(`unexpected url: ${page.url()}`);
    return page.url();
  });

  console.log(JSON.stringify(results, null, 2));
  await browser.close();
}

run().catch((error) => {
  console.error(error);
  process.exit(1);
});
