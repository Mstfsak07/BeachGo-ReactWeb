# JWT Authentication - Implementation Complete

## ✅ System Status

### Backend Files Modified
- ✅ `BeachRehberi.API/Program.cs` - CORS updated for HTTPS + HTTP dev URLs

### Frontend Files Created/Updated
- ✅ `beach-ui/src/api/axios.js` - HTTPS setup + token interceptors + refresh logic
- ✅ `beach-ui/src/services/authService.js` - login, logout, isAuthenticated, getUser
- ✅ `beach-ui/src/services/api.js` - All endpoints (beaches, events, reservations, reviews, business)
- ✅ `beach-ui/src/pages/Login.jsx` - Basic login component
- ✅ `beach-ui/src/pages/Beaches.js` - Beaches list with error handling
- ✅ `beach-ui/src/pages/LoginPage.jsx` - Already uses useAuth context
- ✅ `beach-ui/src/pages/Dashboard.jsx` - Already uses api instance
- ✅ `beach-ui/src/components/ProtectedRoute.jsx` - Protected routes
- ✅ `beach-ui/src/components/Navbar.js` - Updated with logout + user display
- ✅ `beach-ui/src/context/AuthContext.jsx` - Auth provider + logout events
- ✅ `/routes/ProtectedRoute.jsx` - Route protection with auth check

## 🚀 Quick Start

### 1. Backend - Run API
```powershell
# Windows
cd BeachRehberi.API

# Option A: HTTP (recommended for dev with self-signed cert issues)
$env:ASPNETCORE_URLS = "http://localhost:5144"
dotnet run

# Option B: HTTPS (requires certified certificate)
# dotnet dev-certs https --trust
# dotnet run
```

API will be available at:
- HTTP: `http://localhost:5144/api`
- HTTPS: `https://localhost:7296/api` (production style)

### 2. Frontend - Run React App
```bash
cd beach-ui
npm install  # if needed
npm start    # port 3000 default
```

Open: `http://localhost:3000`

### 3. Test Flow
1. Go to `http://localhost:3000/login`
2. Enter credentials (or register first)
   - Email: `test@example.com`
   - Password: `Test@1234`
3. Click "Giriş Yap"
4. Redirected to `/beaches` (now with Authorization header)
5. Beaches loaded from API
6. Click "Logout" in navbar to exit

## 📋 Architecture

### Token Flow

```
LOGIN PAGE
   ↓
1. User enters email + password
   ↓
2. POST /auth/login
   Request: { email, password }
   Response: { token, refreshToken, email, role }
   ↓
3. authService.login() stores:
   - Access Token → memory (setAccessToken)
   - Refresh Token → localStorage
   - User info → localStorage
   ↓
PROTECTED PAGES (Beaches, etc.)
   ↓
1. Every request includes interceptor:
   Authorization header += "Bearer {accessToken}"
   ↓
2. Response 401?
   ├─ YES → POST /auth/refresh
   │   └─ New Token → memory
   │   └─ Retry original request
   ├─ NO → Success, show data
   ↓
LOGOUT
   ├─ POST /auth/logout (optional, best effort)
   └─ Clear all tokens
   └─ Redirect to /login
```

## 🔑 Key Methods

### authService.js

```javascript
// Login
const result = await authService.login('user@email.com', 'password');
// → Returns: { user: { email, role }, accessToken: token }

// Logout
await authService.logout();
// → Clears tokens, navigates to /login

// Check Authentication
if (authService.isAuthenticated()) {
  // User is logged in
}

// Get User Info
const user = authService.getUser();
// → Returns: { email, role } or null
```

### axios.js Integration

```javascript
// All API calls automatically include Bearer token
import api from '../api/axios';

// Example: Get Beaches
const response = await api.get('/beaches');
// Header automatically: Authorization: Bearer {token}
// If 401 → auto refresh & retry
// If refresh fails → logout event triggered
```

### Use in Components

```javascript
import { useAuth } from '../context/AuthContext';

const MyComponent = () => {
  const { user, isAuthenticated, logout } = useAuth();

  if (!isAuthenticated) {
    return <div>Not logged in</div>;
  }

  return (
    <div>
      Welcome {user.email}
      <button onClick={logout}>Logout</button>
    </div>
  );
};
```

## 🛠️ API Response Formats

### Success Response
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh_token_here",
    "email": "user@email.com",
    "role": "user"
  },
  "message": "Success",
  "isSuccess": true
}
```

### Error Response
```json
{
  "message": "Invalid credentials",
  "isSuccess": false
}
```

### 401 Unauthorized
```json
{
  "message": "Unauthorized",
  "isSuccess": false
}
```

## ⚡ Error Handling

### Automatic (Built-in)

1. **Token Expired** → Auto refresh
2. **Refresh Failed** → Auto logout
3. **Network Error** → Console logged + error state in component
4. **CORS Error** → Check Program.cs CORS policy

### Manual (Component Level)

```javascript
const handleSubmit = async () => {
  try {
    const res = await getBeaches();
    // Success
  } catch (error) {
    // error.response.status === 401 → already refreshed
    // error.response.status === 403 → permission denied
    // error.response.status === 500 → server error
    console.error(error.response?.data?.message);
  }
};
```

## 🔒 Security Checklist

### Development
- ✅ Token stored in memory (XSS some protection)
- ✅ Refresh token in localStorage (trade-off for UX)
- ✅ HTTPS self-signed certificate handled
- ✅ 401 auto logout
- ✅ CORS restricted to localhost

### Production TODO
- [ ] Switch to HttpOnly cookies (refresh token)
- [ ] Real HTTPS certificate (not self-signed)
- [ ] CORS whitelist production domains only
- [ ] Token rotation (exchange old refresh for new)
- [ ] Add CSRF protection if using cookies
- [ ] Audit token expiry times
- [ ] Rate limit login attempts

## 📝 Common Issues & Solutions

### Issue: "CORS error" / "No Access-Control-Allow-Origin"
**Solution**: 
```csharp
// In Program.cs, add your frontend URL to CORS policy
policy.WithOrigins(
    "https://localhost:3000",
    "https://localhost:5173"
)
```

### Issue: "ERR_CERT_INVALID" in Chrome (HTTPS development)
**Solution Option 1 - Use HTTP**:
```powershell
$env:ASPNETCORE_URLS = "http://localhost:5144"
dotnet run
```

**Solution Option 2 - Trust Certificate**:
```powershell
dotnet dev-certs https --trust
```

### Issue: Token not sending in request header
**Check**:
1. Is user logged in? → `authService.isAuthenticated()`
2. Check browser localStorage → `refreshToken` present?
3. Check browser DevTools → Network tab → Authorization header showing?
4. Check console → Any axios interceptor errors?

### Issue: 401 infinite loop
**Cause**: Refresh token also expired
**Solution**: Both tokens expire → force logout
**Current behavior**: Line ~70 in axios.js handles this

### Issue: "No ProtectedRoute found" or routing not working
**Check**: 
- Is component wrapped in `<ProtectedRoute>`?
- Is app wrapped in `<AuthProvider>`?
- Is RouterBrowser Router set up?

## 📊 File Dependencies

```
App.js
├── Router, Routes
├── AuthProvider (context/AuthContext)
│   ├── axios (api/axios.js)
│   │   ├── authService.js [login/refresh]
│   │   └── localStorage [refreshToken]
│   └── Navbar
│       └── useAuth hook
├── Navbar (components/Navbar.js)
│   ├── useAuth → user, logout
│   └── useNavigate
└── Routes
    ├── / → Home
    ├── /login → LoginPage
    │   └── useAuth → login()
    ├── /beaches → ProtectedRoute
    │   └── Dashboard
    │       └── api.get('/beaches')
    └── /register → Register
```

## 🧪 Test Scenarios

### Scenario 1: Basic Login & Access Protected Route
```
1. Go to /login
2. Enter valid credentials
3. ✅ Redirected to /beaches
4. ✅ Beaches data displayed
```

### Scenario 2: Expired Token Auto Refresh
```
1. Login successfully
2. Wait for token expiry (check backend token lifetime)
3. Make request to /beaches
4. ✅ Auto refresh triggered
5. ✅ Request retried with new token
6. ✅ Data displayed
```

### Scenario 3: Refresh Token Expired
```
1. Valid access token, expired refresh token
2. Request to protected endpoint
3. Response: 401 Unauthorized
4. Attempt refresh with expired refresh token
5. ✅ Logout event triggered
6. ✅ Redirected to /login
```

### Scenario 4: Manual Logout
```
1. Login successfully
2. Click "Logout"
3. ✅ Tokens cleared
4. ✅ Redirected to /
5. ✅ Cannot access /beaches (ProtectedRoute blocks)
```

## 📞 Support

### Backend Issues
- Check `BeachRehberi.API/appsettings.json` → JWT settings
- Check `BeachRehberi.API/Program.cs` → CORS, Auth configuration
- Check database migrations → `dotnet ef database update`

### Frontend Issues
- Check `beach-ui/src/api/axios.js` → baseURL correct?
- Check `beach-ui/src/services/authService.js` → endpoint URLs
- Check browser DevTools → Network tab, Console for errors

### API Testing
- Use Postman or curl:
  ```bash
  # Login
  curl -X POST https://localhost:7296/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"Test@1234"}'

  # Get Beaches (with token)
  curl -X GET https://localhost:7296/api/beaches \
    -H "Authorization: Bearer {token_from_login}"
  ```

## 🎯 Next Steps

1. ✅ Run backend: `dotnet run`
2. ✅ Run frontend: `npm start`  
3. ✅ Test login flow
4. ✅ Verify beaches data loaded
5. ✅ Test logout
6. ✅ Test protected route access without token
7. Deploy to production with real HTTPS certificate

---

**System configured and ready to use!** 🚀
