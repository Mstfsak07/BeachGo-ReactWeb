const { test, expect } = require('@playwright/test');

test.describe('Guest reservation & check (UI)', () => {
  test('reservation-check page shows code and email fields', async ({ page }) => {
    await page.goto('/reservation-check');
    await expect(page.getByRole('heading', { name: /rezervasyon sorgula/i })).toBeVisible();
    await expect(page.getByPlaceholder(/örn: abc123/i)).toBeVisible();
    await expect(page.getByPlaceholder(/rezervasyonda kullandığınız e-posta/i)).toBeVisible();
  });

  test('guest reservation wizard loads for a beach route', async ({ page }) => {
    await page.goto('/reservation/1');
    await expect(page.locator('body')).toContainText(/plaj|rezerv|beach|yüklen|loading/i);
  });

  test('submitting reservation-check with fake data shows not-found or error feedback', async ({
    page,
  }) => {
    await page.goto('/reservation-check');
    await page.getByPlaceholder(/örn: abc123/i).fill('FAKECODE12345');
    await page.getByPlaceholder(/rezervasyonda kullandığınız e-posta/i).fill('nobody@example.com');
    await page.getByRole('button', { name: /rezervasyonu bul/i }).click();
    await expect(
      page.getByText(/bulunamadı|hata|sorgulama/i).first(),
    ).toBeVisible({ timeout: 15_000 });
  });
});
