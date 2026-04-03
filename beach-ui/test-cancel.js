const { chromium } = require('playwright');
const assert = require('assert');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  let passedCount = 0;

  try {
      console.log("Navigating to http://localhost:3000/reservation-check ...");
      await page.goto('http://localhost:3000/reservation-check', { waitUntil: 'load' });
      await page.waitForTimeout(1000);

      // We need an active reservation code. Assuming ST9EAMMS is still Pending.
      // Alternatively, we can intercept the network to just test the backend endpoints directly 
      // without relying on UI state, but testing through UI validates the requirements fully.
      
      const testCode = 'ST9EAMMS';
      
      console.log(`Scenario 1: Testing valid active reservation cancel (${testCode}) ...`);
      await page.fill('input[type="text"]', testCode);
      await page.locator('button[type="submit"]').click();
      await page.waitForTimeout(2000);

      // Click "Rezervasyonu İptal Et"
      // Handle the window.confirm automatically
      page.once('dialog', dialog => dialog.accept());
      
      const cancelBtn = page.locator('button', { hasText: 'Rezervasyonu Ä°ptal Et' }); // Handle encoding artifact
      
      // If button is not found with encoding artifact, try clean text
      let cancelBtnFound = await cancelBtn.count() > 0;
      if (!cancelBtnFound) {
          const cleanCancelBtn = page.locator('button', { hasText: 'Rezervasyonu İptal Et' });
          if (await cleanCancelBtn.count() > 0) {
              await cleanCancelBtn.click();
          } else {
              console.log("Cancel button not found! It might already be cancelled.");
              // If it's already cancelled, we can't run Scenario 1 properly. Let's just mock the API call to test the second cancellation.
          }
      } else {
          await cancelBtn.click();
          await page.waitForTimeout(2000);
          
          // Verify status changed to Cancelled in UI
          const hasCancelledStatus = await page.locator('text=Ä°ptal / Red').count() > 0 || await page.locator('text=İptal / Red').count() > 0;
          if (hasCancelledStatus) {
              console.log("Scenario 1: PASS - Reservation cancelled successfully.");
              passedCount++;
          } else {
              console.error("Scenario 1: FAILED - Status did not change.");
          }
      }

      console.log("Scenario 2: Testing second cancellation (Zaten iptal edilmiş) ...");
      // To trigger this, we need to bypass the UI button hiding.
      // We can do this by executing a fetch directly in the browser context.
      const secondCancelRes = await page.evaluate(async (code) => {
          const res = await fetch(`http://localhost:5144/api/GuestReservations/cancel/${code}`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' }
          });
          const text = await res.text();
          return { status: res.status, body: text };
      }, testCode);

      if (secondCancelRes.status === 400 && secondCancelRes.body.includes('zaten iptal')) {
          console.log("Scenario 2: PASS - Second cancel returned 400 'zaten iptal edilmiş'.");
          passedCount++;
      } else {
          console.error(`Scenario 2: FAILED - Status: ${secondCancelRes.status}, Body: ${secondCancelRes.body}`);
      }

      console.log("Scenario 3: Testing invalid code cancellation ...");
      const invalidCancelRes = await page.evaluate(async () => {
          const res = await fetch(`http://localhost:5144/api/GuestReservations/cancel/INVALID123`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' }
          });
          const text = await res.text();
          return { status: res.status, body: text };
      });

      if (invalidCancelRes.status === 404 && invalidCancelRes.body.includes('bulunamad')) {
          console.log("Scenario 3: PASS - Invalid code returned 404 'bulunamadı'.");
          passedCount++;
      } else {
          console.error(`Scenario 3: FAILED - Status: ${invalidCancelRes.status}, Body: ${invalidCancelRes.body}`);
      }

  } catch (error) {
      console.error("Test execution error:", error);
  } finally {
      await browser.close();
  }
})();
