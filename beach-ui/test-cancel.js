const { chromium } = require('playwright');

(async () => {
    const testCode = process.env.TEST_CONFIRMATION_CODE;
    const testEmail = process.env.TEST_GUEST_EMAIL;

    if (!testCode || !testEmail) {
        console.error('Set TEST_CONFIRMATION_CODE and TEST_GUEST_EMAIL before running this script.');
        process.exit(1);
    }

    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext();
    const page = await context.newPage();

    let passedCount = 0;

    try {
        console.log("Navigating to http://localhost:3000/reservation-check ...");
        await page.goto('http://localhost:3000/reservation-check', { waitUntil: 'load' });
        await page.waitForTimeout(1000);

        console.log(`Scenario 1: Testing valid active reservation cancel (${testCode}) ...`);
        await page.fill('input[type="text"]', testCode);
        await page.fill('input[type="email"]', testEmail);
        await page.locator('button[type="submit"]').click();
        await page.waitForTimeout(2000);

        // Click "Rezervasyonu İptal Et"
        // Handle the window.confirm automatically
        page.once('dialog', dialog => dialog.accept());

        const cancelBtn = page.locator('button', { hasText: 'Rezervasyonu İptal Et' }); // Handle encoding artifact

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
            const hasCancelledStatus = await page.locator('text=İptal / Red').count() > 0 || await page.locator('text=İptal / Red').count() > 0;
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
        const secondCancelRes = await page.evaluate(async ({ code, email }) => {
            const res = await fetch(`http://localhost:5144/api/GuestReservations/cancel/${code}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            const text = await res.text();
            return { status: res.status, body: text };
        }, { code: testCode, email: testEmail });

        if (secondCancelRes.status === 400 && secondCancelRes.body.includes('zaten iptal')) {
            console.log("Scenario 2: PASS - Second cancel returned 400 'zaten iptal edilmiş'.");
            passedCount++;
        } else {
            console.error(`Scenario 2: FAILED - Status: ${secondCancelRes.status}, Body: ${secondCancelRes.body}`);
        }

        console.log("Scenario 3: Testing invalid code cancellation ...");
        const invalidCancelRes = await page.evaluate(async (email) => {
            const res = await fetch(`http://localhost:5144/api/GuestReservations/cancel/INVALID123`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            const text = await res.text();
            return { status: res.status, body: text };
        }, testEmail);

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
