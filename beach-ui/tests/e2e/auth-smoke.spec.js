const { test, expect } = require('@playwright/test');

test('home page renders', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Beach/i);
  await expect(page.locator('body')).toContainText(/beach|plaj|rezerv/i);
});

test('login page renders and validates invalid input', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: /Giriş Yap/i })).toBeVisible();
  const emailInput = page.locator('form input[type="email"]');
  const passwordInput = page.locator('form input[type="password"]');
  await expect(emailInput).toBeVisible();
  await expect(passwordInput).toBeVisible();

  await emailInput.fill('bad-email');
  await passwordInput.fill('123');
  await page.getByRole('button', { name: /Giriş Yap/i }).click();

  const validationMessage = await emailInput.evaluate((el) => el.validationMessage);
  expect(validationMessage).toBeTruthy();
});

test('register page renders and validates mismatched passwords', async ({ page }) => {
  await page.goto('/register');
  await expect(page.getByRole('heading', { name: /Kayıt Ol/i })).toBeVisible();
  const nameInput = page.locator('input[name="name"]');
  const emailInput = page.locator('input[name="email"]');
  const passwordInput = page.locator('input[name="password"]');
  const confirmPasswordInput = page.locator('input[name="confirmPassword"]');
  await expect(nameInput).toBeVisible();
  await expect(emailInput).toBeVisible();

  await nameInput.fill('Test User');
  await emailInput.fill('test@example.com');
  await passwordInput.fill('123456');
  await confirmPasswordInput.fill('654321');
  await page.getByRole('button', { name: /Hesap Oluştur/i }).click();

  await expect(page.getByText('Şifreler eşleşmiyor.')).toBeVisible();
});

test('protected route redirects unauthenticated user to login', async ({ page }) => {
  await page.goto('/dashboard');
  await expect(page).toHaveURL(/\/login$/);
});
