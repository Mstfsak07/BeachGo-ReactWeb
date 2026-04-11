const { test, expect } = require('@playwright/test');

test.describe('Business / admin RBAC (unauthenticated)', () => {
  test('dashboard sends guest to login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/);
  });

  test('admin panel sends guest to login', async ({ page }) => {
    await page.goto('/admin');
    await expect(page).toHaveURL(/\/login/);
  });

  test('dashboard reservations route sends guest to login', async ({ page }) => {
    await page.goto('/dashboard/reservations');
    await expect(page).toHaveURL(/\/login/);
  });
});
