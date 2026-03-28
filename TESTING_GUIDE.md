# 🧪 JWT Auth System - Complete Test Guide

## Pre-Test Setup

### 1. Database Seed Data
```sql
-- Test user for login
Email: test@example.com
Password: Test@1234
```

### 2. Postman Environment Variables
```json
{
  "api_url": "http://localhost:5144/api",
  "token": "",
  "refresh_token": ""
}
```

---

## 🏃 Test Scenarios

### Test 1: Backend API - Login Endpoint
**Purpose**: Verify token generation works

**Steps:**
```
1. Start Backend
   cd BeachRehberi.API
   $env:ASPNETCORE_URLS = "http://localhost:5144"
   dotnet run

2. Open Postman or Browser Console:
   curl -X POST http://localhost:5144/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test@1234"}'

3. Check Response:
   {
     "data": {
       "token": "eyJhbGciOi...",
       "refreshToken": "refresh_...",
       "email": "test@example.com",
       "role": "user"
     },
     "message": "Success",
     "isSuccess": true
   }
```

**Expected:**
- ✅ Status 200
- ✅ token field present
- ✅ refreshToken field present

---

### Test 2: Backend API - Protected Endpoint
**Purpose**: Verify JWT validation

**Steps:**
```
1. Get token from Test 1

2. Call protected endpoint:
   curl -X GET http://localhost:5144/api/beaches \
     -H "Authorization: Bearer {token_from_test_1}"

3. Check Response:
   [
     { "id": 1, "name": "Test Beach", "address": "...", "rating": 4.5 },
     ...
   ]
```

**Expected:**
- ✅ Status 200
- ✅ Beaches array returned

**Failure Test:**
```
curl -X GET http://localhost:5144/api/beaches
(no Authorization header)

Expected: 401 Unauthorized
```

---

### Test 3: Frontend - Login Page Load
**Purpose**: Frontend React app loads without errors

**Steps:**
```
1. Start Frontend
   cd beach-ui
   npm start

2. Browser opens to http://localhost:3000

3. Navigate to http://localhost:3000/login

4. Check:
   - Page renders
   - Email input visible
   - Password input visible
   - Login button visible
   - No console errors
```

**Expected:**
- ✅ LoginPage component visible
- ✅ No errors in DevTools → Console

---

### Test 4: Frontend - Login Success
**Purpose**: Verify login flow and token storage

**Steps:**
```
1. On login page (http://localhost:3000/login)

2. Fill form:
   Email: test@example.com
   Password: Test@1234
   
3. Click "Giriş Yap"

4. Check Results:
   - Should redirect to /beaches
   - Browser DevTools → Application → localStorage
     → "refreshToken" key present
     → "user" key present with { email, role }
   - DevTools → Console → No errors
```

**Expected:**
- ✅ Redirected to /beaches
- ✅ localStorage has auth data
- ✅ No console errors

---

### Test 5: Frontend - Beaches Data Display
**Purpose**: Verify protected data fetch with token

**Steps:**
```
1. Successfully logged in (from Test 4)

2. On /beaches page

3. Check:
   - Page renders
   - Beaches list appears
   - DevTools → Network tab:
     a. Find "beaches" GET request
     b. Click it
     c. Go to "Headers" section
     d. Look for: Authorization: Bearer eyJ...
     e. Response shows beach data
```

**Expected:**
- ✅ Beaches rendered on screen
- ✅ Authorization header present
- ✅ Response status 200

---

### Test 6: Frontend - Navbar User Display
**Purpose**: User info visible when logged in

**Steps:**
```
1. On /beaches (logged in)

2. Look at Navbar:
   - Shows user email
   - Shows "Logout" button
   
3. Check that:
   - Not showing "Login" link
   - Not showing "Register" link
   - Only shows "Logout"
```

**Expected:**
- ✅ User email displayed
- ✅ Logout button visible
- ✅ Login/Register hidden

---

### Test 7: Frontend - Manual Logout
**Purpose**: Logout functionality

**Steps:**
```
1. On /beaches (logged in)

2. Click "Logout" button in navbar

3. Check:
   - Redirected to /
   - localStorage empty:
     DevTools → Application → localStorage
     → refreshToken GONE
     → user GONE
   - Navbar shows Login/Register buttons
```

**Expected:**
- ✅ Redirected to home
- ✅ localStorage cleared
- ✅ isAuthenticated = false

---

### Test 8: Frontend - Protected Route Access
**Purpose**: Unauthorized access blocked

**Steps:**
```
1. Logout (from Test 7)

2. Try to access /beaches directly:
   http://localhost:3000/beaches

3. Check:
   - Redirected to /login
   - Cannot see beaches page
```

**Expected:**
- ✅ Redirected to /login
- ✅ ProtectedRoute blocks access

---

### Test 9: API - Token Refresh
**Purpose**: Verify refresh token mechanism

**Steps:**
```
1. Get valid token from Test 1

2. Calculate token expiry:
   - Decode JWT (jwt.io)
   - Note "exp" timestamp
   - Calculate remaining seconds

3. When token expires:
   - Frontend auto calls /auth/refresh
   - Get new token
   - Retry original request

4. Or manually test:
   curl -X POST http://localhost:5144/api/auth/refresh \
     -H "Content-Type: application/json" \
     -d '{"refreshToken":"refresh_token_from_test_1"}'

   Expected Response:
   {
     "data": {
       "token": "new_token",
       "refreshToken": "new_refresh_token"
     }
   }
```

**Expected:**
- ✅ New token received
- ✅ Status 200

---

### Test 10: CORS - Browser Cross-Origin
**Purpose**: Verify CORS works

**Steps:**
```
1. Frontend at http://localhost:3000
   Backend at http://localhost:5144

2. Login → beaches fetch happens automatically

3. If CORS error appears:
   → DevTools → Console → Red error message
   → Network tab → beaches request → NO response

4. Check Program.cs CORS policy includes:
   "http://localhost:3000"
```

**Expected:**
- ✅ No CORS errors
- ✅ Request succeeds

---

### Test 11: Error Handling - Invalid Credentials
**Purpose**: Verify error display on failed login

**Steps:**
```
1. On /login

2. Enter wrong password:
   Email: test@example.com
   Password: wrongpassword

3. Click "Giriş Yap"

4. Check:
   - Page stays on /login
   - Error message displays
   - Console shows error details
```

**Expected:**
- ✅ Error message shown
- ✅ Not redirected
- ✅ Form still visible

---

### Test 12: Error Handling - Network Error
**Purpose**: Verify network error handling

**Steps:**
```
1. Stop backend (kill dotnet process)

2. On /beaches page, refresh

3. Check:
   - Error message appears
   - Page doesn't crash
   - Can retry action
   
4. Restart backend and page should recover
```

**Expected:**
- ✅ Graceful error handling
- ✅ Error message displayed

---

## 🔍 Debugging Tests

### 1. Check Token in Memory
```javascript
// DevTools Console
import { getAccessToken } from './api/axios'
console.log(getAccessToken())
```

### 2. Decode JWT
```javascript
// jwt.io website
// Paste token to see payload
```

### 3. Monitor Network Requests
```
DevTools → Network tab
Filter: "auth" or "beach"
Check headers and responses
```

### 4. Check Auth Context
```javascript
// DevTools Console
// If component uses useAuth()
import { useAuth } from './context/AuthContext'
const { user, isAuthenticated } = useAuth()
console.log(user, isAuthenticated)
```

---

## ✅ Complete Test Checklist

### Backend
- [ ] API starts without errors
- [ ] Login endpoint returns token
- [ ] Protected endpoint requires auth
- [ ] 401 returned without token
- [ ] Token refresh works
- [ ] CORS allows localhost
- [ ] API logs show requests
- [ ] Database has test data

### Frontend
- [ ] App loads without errors
- [ ] Login page renders
- [ ] Login works with valid credentials
- [ ] Login fails with invalid credentials
- [ ] Redirects to /beaches on success
- [ ] Token stored in localStorage
- [ ] Token sent in Authorization header
- [ ] Beaches data displays
- [ ] User email shown in navbar
- [ ] Logout button works
- [ ] localStorage cleared after logout
- [ ] Cannot access /beaches without login
- [ ] Protected route redirects properly
- [ ] Error messages display correctly
- [ ] No console errors after login
- [ ] No CORS errors

### Security
- [ ] Token in Authorization header (not URL)
- [ ] Refresh token in localStorage
- [ ] Auto logout on 401
- [ ] CORS restricted to localhost
- [ ] No tokens in console output
- [ ] Errors don't expose sensitive data

### Integration
- [ ] Full login → beaches → logout flow works
- [ ] Token refresh happens automatically
- [ ] Multiple requests work with same token
- [ ] Manual logout works
- [ ] Protected route blocks unauthorized
- [ ] Page refresh maintains session

---

## 📊 Performance Expectations

| Operation | Expected Time |
|-----------|---|
| Login request | < 1 second |
| Beaches fetch | < 1 second |
| Token refresh | < 500ms |
| Page redirect | Immediate |
| Logout | Immediate |

---

## 🚨 Common Test Failures

| Error | Cause | Fix |
|-------|-------|-----|
| CORS error | Origin not in Program.cs | Add to CORS policy |
| 401 always | Invalid token | Check JWT secret |
| Login page doesn't submit | Form validation | Check console errors |
| Beaches don't load | Token not sent | Check axios interceptor |
| Manual logout doesn't work | logout() not called | Check Navbar onClick |
| Can access /beaches without login | ProtectedRoute broken | Check AuthContext |
| Token not in header | setAccessToken() missed | Check login flow |

---

## 🧩 Test with Postman Collection

### Create Collection: "BeachGo JWT"

**Request 1: Login**
```
Name: Login
Method: POST
URL: {{api_url}}/auth/login
Body (raw JSON):
{
  "email": "test@example.com",
  "password": "Test@1234"
}

Tests (JavaScript):
pm.environment.set("token", pm.response.json().data.token)
pm.environment.set("refresh_token", pm.response.json().data.refreshToken)
```

**Request 2: Get Beaches**
```
Name: Get Beaches
Method: GET
URL: {{api_url}}/beaches
Headers:
  Authorization: Bearer {{token}}

Tests:
pm.response.to.have.status(200)
```

**Request 3: Refresh Token**
```
Name: Refresh Token
Method: POST
URL: {{api_url}}/auth/refresh
Body (raw JSON):
{
  "refreshToken": "{{refresh_token}}"
}
```

---

## 📝 Test Coverage Report

After running all tests, confirm:

- **Backend API**: ✅ 100% endpoints tested
- **Frontend Pages**: ✅ All pages load correctly
- **Auth Flow**: ✅ Login, token storage, auto refresh
- **Protected Routes**: ✅ Access control working
- **Error Handling**: ✅ Errors displayed gracefully
- **Security**: ✅ Tokens handled correctly
- **Integration**: ✅ Full flow end-to-end

---

**Ready to test the complete JWT auth system!** 🚀
