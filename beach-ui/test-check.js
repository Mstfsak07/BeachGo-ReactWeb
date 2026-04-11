const { chromium } = require('playwright');

(async () => {
  const testCode = process.env.TEST_CONFIRMATION_CODE;
  const testEmail = process.env.TEST_GUEST_EMAIL;

  if (!testCode || !testEmail) {
    console.error('Set TEST_CONFIRMATION_CODE and TEST_GUEST_EMAIL before running this script.');
    process.exit(1);
  }

  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  let passedCount = 0;

  try {
      console.log("Navigating to http://localhost:3000/reservation-check ...");
      await page.goto('http://localhost:3000/reservation-check', { waitUntil: 'load' });
      await page.waitForTimeout(1000);

      console.log(`Scenario 1: Testing valid confirmation code (${testCode}) ...`);
      await page.fill('input[type="text"]', testCode);
      await page.fill('input[type="email"]', testEmail);
      await page.locator('button[type="submit"]').click();
      
      // wait for result to appear
      await page.waitForTimeout(2000);
      
      // Look for the beach name or guest name
      const hasReservationCard = await page.locator('text=Rezervasyon Durumu').count();
      if (hasReservationCard > 0) {
          console.log("Scenario 1: PASS - Reservation details shown.");
          passedCount++;
      } else {
          console.error("Scenario 1: FAILED - Details not shown.");
      }

      console.log("Scenario 2: Testing invalid confirmation code (INVALID123) ...");
      await page.fill('input[type="text"]', 'INVALID123');
      await page.fill('input[type="email"]', testEmail);
      await page.locator('button[type="submit"]').click();
      
      await page.waitForTimeout(2000);
      
      const hasNotFoundMessage = await page.locator('text=Bu kod ile').count();
      if (hasNotFoundMessage > 0) {
          console.log("Scenario 2: PASS - 404 Not Found message shown.");
          passedCount++;
      } else {
          console.error("Scenario 2: FAILED - Not found message not shown.");
      }

      if (passedCount === 2) {
          console.log("\nAll scenarios PASS.");
      }

  } catch (error) {
      console.error("Test execution error:", error);
  } finally {
      await browser.close();
  }
})();
