const { chromium } = require('playwright');
const sqlite3 = require('sqlite3').verbose();
const path = require('path');

// Database connection
const dbPath = path.resolve(__dirname, '../BeachRehberi.API/BeachRehberi.API/beachrehberi.db');
const db = new sqlite3.Database(dbPath, sqlite3.OPEN_READONLY, (err) => {
    if (err) {
        console.error("Database Error:", err.message);
    } else {
        console.log("Connected to the SQLite database.");
    }
});

function getOtpCodeFromDb(phone) {
    return new Promise((resolve, reject) => {
        const query = `SELECT Code FROM VerificationCodes WHERE Phone = ? ORDER BY Id DESC LIMIT 1`;
        db.get(query, [phone], (err, row) => {
            if (err) {
                reject(err);
            } else {
                resolve(row ? row.Code : null);
            }
        });
    });
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  let failedStep = '';
  let reqBody = '';
  let resBody = '';
  let resStatus = '';
  let isFailed = false;

  const logFailure = (step, req, status, res, msg) => {
      console.log(`Hangi adım başarısız: ${step}`);
      console.log(`Request body: ${req}`);
      console.log(`Response status/body: ${status} / ${res}`);
      console.log(`Sorunun sebebi: ${msg}`);
      isFailed = true;
  };

  // Intercept responses to check for success/failure
  page.on('response', async (response) => {
      const url = response.url();
      if (url.includes('/api/GuestReservations/send-otp') && response.request().method() === 'POST') {
          const status = response.status();
          if (status !== 200) {
              const reqText = response.request().postData() || '';
              const resText = await response.text();
              logFailure('OTP Gönderme (send-otp)', reqText, status, resText, 'API 200 dönmedi');
          }
      }
      if (url.includes('/api/GuestReservations/verify-otp') && response.request().method() === 'POST') {
          const status = response.status();
          const reqText = response.request().postData() || '';
          const resText = await response.text();
          if (status !== 200) {
              logFailure('OTP Doğrulama (verify-otp)', reqText, status, resText, 'API 200 dönmedi');
          } else {
              try {
                  const data = JSON.parse(resText);
                  if (!data.success || !data.data.verified) {
                      logFailure('OTP Doğrulama (verify-otp)', reqText, status, resText, 'OTP verification failed in response body');
                  }
              } catch(e){}
          }
      }
      if (url.includes('/api/GuestReservations') && !url.includes('-otp') && !url.includes('cancel') && response.request().method() === 'POST') {
          const status = response.status();
          const reqText = response.request().postData() || '';
          const resText = await response.text();
          if (status !== 200 && status !== 201) {
              logFailure('Rezervasyon Oluşturma', reqText, status, resText, 'API başarılı dönmedi');
          } else {
              try {
                  const data = JSON.parse(resText);
                  if (data.success && data.data && data.data.confirmationCode) {
                      console.log(`\n\nBaşarılı! Confirmation Code: ${data.data.confirmationCode}`);
                  } else {
                      logFailure('Rezervasyon Oluşturma', reqText, status, resText, 'confirmationCode alınamadı');
                  }
              } catch(e){}
          }
      }
  });

  try {
      console.log("Navigating to http://localhost:3000/reservation/4 ...");
      await page.goto('http://localhost:3000/reservation/4', { waitUntil: 'load' });
      await page.waitForTimeout(2000);

      // Step 1: Date & Type
      console.log("Step 1: Tarih ve Tip seçiliyor...");
      // Find date input by type
      const dateStr = new Date();
      dateStr.setDate(dateStr.getDate() + 1); // tomorrow
      const tomorrow = dateStr.toISOString().split('T')[0];
      
      await page.fill('input[type="date"]', tomorrow);
      await page.fill('input[type="time"]', '12:00');
      
      // Select type "Loca" (find button containing "Loca")
      await page.locator('button', { hasText: 'Loca' }).click();
      
      // Click Devam Et
      await page.locator('button', { hasText: 'Devam Et' }).click();
      
      // Wait for transition
      await page.waitForTimeout(1000);
      if (isFailed) throw new Error("Step 1 Failed.");

      // Step 2: Personal Info
      console.log("Step 2: Kişisel Bilgiler giriliyor...");
      await page.fill('input[placeholder="Adınız"]', 'Murat');
      await page.fill('input[placeholder="Soyadınız"]', 'Test');
      await page.fill('input[placeholder="+90 5XX XXX XX XX"]', '5551234567');
      await page.fill('input[type="email"]', 'test@test.com');
      
      // Devam Et (triggers OTP)
      await page.locator('button', { hasText: 'Devam Et' }).click();
      
      // Wait for OTP request to complete
      await page.waitForTimeout(2000);
      if (isFailed) throw new Error("Step 2 Failed (OTP Send).");

      // Step 3: Verify OTP
      console.log("Step 3: OTP Doğrulanıyor...");
      const phoneToSearch = '+905551234567';
      const otpCode = await getOtpCodeFromDb(phoneToSearch);
      
      if (!otpCode) {
          logFailure('OTP Okuma DB', '', '', '', 'DB de OTP kodu bulunamadı.');
          throw new Error("DB Error");
      }
      console.log(`DB'den okunan OTP Kodu: ${otpCode}`);

      // OTP has 6 digits, we need to fill them
      for (let i = 0; i < 6; i++) {
          await page.locator('input[inputmode="numeric"]').nth(i).fill(otpCode[i]);
      }
      
      await page.locator('button', { hasText: 'Doğrula' }).click();
      await page.waitForTimeout(2000);
      if (isFailed) throw new Error("Step 3 Failed (OTP Verify).");

      // Step 4: Payment
      console.log("Step 4: Ödeme / Onay...");
      // Check the terms checkbox
      await page.locator('input[type="checkbox"]').check();
      
      // Click Onayla
      await page.locator('button', { hasText: 'Rezervasyonu Onayla' }).click();
      
      // Wait for reservation request
      await page.waitForTimeout(3000);
      if (isFailed) throw new Error("Step 4 Failed (Create Reservation).");

      console.log("Rezervasyon başarıyla tamamlandı!");

  } catch (error) {
      if (!isFailed) {
          console.error("Test execution error:", error);
      }
  } finally {
      await browser.close();
      db.close();
  }
})();
