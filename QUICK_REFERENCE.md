# 🏖️ BeachGo JWT Auth - Quick Reference

## ⚡ Start System (5 min)

### Backend
```powershell
cd BeachRehberi.API
$env:ASPNETCORE_URLS = "http://localhost:5144"
dotnet run
# → API ready at http://localhost:5144/api
```

### Frontend  
```bash
cd beach-ui
npm start
# → App ready at http://localhost:3000
```

### Test
```
Visit: http://localhost:3000/login
Login → /beaches → See plajlar list
```

---

## 📍 Key URLs

| Component | URL |
|-----------|-----|
| Frontend | `http://localhost:3000` |
| Backend API | `http://localhost:5144/api` |
| Login Page | `http://localhost:3000/login` |
| Beaches | `http://localhost:3000/beaches` |
| Swagger Docs | `http://localhost:5144/swagger` |

---

## 🔑 API Endpoints

### Auth
```
POST /api/auth/login
  Request: { email: string, password: string }
  Response: { token, refreshToken, email, role }

POST /api/auth/refresh
  Request: { refreshToken: string }
  Response: { token, refreshToken }

POST /api/auth/logout
  Request: { refreshToken: string }
  Response: { message: "Logged out" }
```

### Beaches (Protected 🔐)
```
GET /api/beaches
  Header: Authorization: Bearer {token}
  Response: [ { id, name, address, rating, ... } ]

GET /api/beaches/search?query={search}
  Header: Authorization: Bearer {token}
  
GET /api/beaches/{id}
  Header: Authorization: Bearer {token}
```

---

## 💻 React Usage Examples

### Login
```jsx
import { useAuth } from '../context/AuthContext';

const LoginForm = () => {
  const { login } = useAuth();
  
  const handleLogin = async () => {
    const result = await login('user@email.com', 'password');
    // → { user: { email, role }, accessToken }
  };
};
```

### Use Auth State
```jsx
const { user, isAuthenticated, logout } = useAuth();

if (isAuthenticated) {
  return <>
    <p>Welcome {user.email}</p>
    <button onClick={logout}>Logout</button>
  </>;
}
```

### Protected Route
```jsx
<Route path="/beaches" element={
  <ProtectedRoute>
    <Beaches />
  </ProtectedRoute>
} />
```

### Call Protected API
```jsx
import { getBeaches } from '../services/api';

const Beaches = () => {
  useEffect(async () => {
    const res = await getBeaches();
    // Token automatically added to header
    console.log(res.data.data);
  }, []);
};
```

---

## 🛠️ File Quick Map

```
src/
├── api/
│   └── axios.js ..................... Axios instance + token refresh
├── services/
│   ├── authService.js .............. login(), logout(), isAuthenticated()
│   └── api.js ...................... API endpoint wrappers (getBeaches, etc)
├── pages/
│   ├── Login.jsx ................... Login form
│   ├── LoginPage.jsx ............... Alternative login (uses context)
│   ├── Beaches.js .................. Beach list with error handling
│   └── Dashboard.jsx ............... Also displays beaches
├── components/
│   ├── Navbar.js ................... Nav + user + logout
│   └── ProtectedRoute.jsx .......... Route protection wrapper
├── context/
│   └── AuthContext.jsx ............. Auth provider + useAuth hook
└── routes/
    └── ProtectedRoute.jsx .......... Protected route component
```

---

## 🔄 Token Flow Diagram

```
┌─────────────┐
│  User Login │
└──────┬──────┘
       │
       v
┌─────────────────────────┐
│ POST /auth/login        │
│ {email, password}       │
└──────┬──────────────────┘
       │
       v
┌─────────────────────────────┐
│ Response:                   │
│ {token, refreshToken, ...}  │
└──────┬──────────────────────┘
       │
       v
┌─────────────────────────────┐
│ Store:                      │
│ - token (memory)            │
│ - refreshToken (localStorage)│
│ - user (localStorage)       │
└──────┬──────────────────────┘
       │
       v
┌──────────────────────────┐
│ API Request              │
│ Header: "Bearer {token}" │
└──────┬───────────────────┘
       │
       ├─ Valid    → Success
       │
       └─ 401      → Refresh
                     ├─ Success → Retry request
                     └─ Fail    → Logout event
```

---

## ⚠️ Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| 401 on first request | User not logged in yet |
| Token not in header | `setAccessToken()` not called |
| CORS error | Add origin to `Program.cs` CORS policy |
| Self-signed cert error | Use HTTP or trust certificate |
| Infinite redirect loop | Check `isAuthenticated()` logic |
| Manual logout not working | Call `useAuth().logout()` |

---

## 🔒 What's Secure

✅ Token sent via Authorization header (not URL/body)  
✅ Auto logout on 401  
✅ Token refresh mechanism  
✅ Protected routes with auth check  
✅ CORS restricted to localhost

⚠️ Development Only

- Self-signed HTTPS cert
- Refresh token in localStorage
- Multiple CORS origins

---

## 📋 Test Checklist

- [ ] Backend running (http://localhost:5144)
- [ ] Frontend running (http://localhost:3000)
- [ ] Can access login page
- [ ] Login with valid credentials works
- [ ] Redirected to /beaches after login
- [ ] Beaches data displays
- [ ] Logout button visible + works
- [ ] Can't access /beaches without login
- [ ] Token shows in DevTools → Network → Authorization header
- [ ] No console errors after login

---

## 🚀 Deployment Checklist

Before pushing to production:

- [ ] Change `baseURL` to production API domain
- [ ] Update `Program.cs` CORS to production domain
- [ ] Replace self-signed cert with valid SSL
- [ ] Change refresh token storage (localStorage → HttpOnly cookie)
- [ ] Set appropriate token expiry times
- [ ] Enable CSRF protection if using cookies
- [ ] Add rate limiting (already configured in backend)
- [ ] Test with production certificate
- [ ] Audit security headers
- [ ] Add monitoring/logging for auth failures

---

## 🆘 Debug Commands

### Chrome DevTools Console
```javascript
// Check if logged in
localStorage.getItem('refreshToken')

// Check user data
JSON.parse(localStorage.getItem('user'))

// Clear all auth
localStorage.clear()
```

### Network Tab
1. Filter by "beaches" request
2. Look for request headers
3. Should see: `Authorization: Bearer eyJ...`

### Terminal (Backend)
```powershell
# Watch for logs
dotnet run

# Check migrations
dotnet ef database update

# Reset database
dotnet ef database drop
```

---

## 📞 Contact Points

- **Frontend Auth** → `/beach-ui/src/context/AuthContext.jsx`
- **API Calls** → `/beach-ui/src/api/axios.js`
- **Auth Methods** → `/beach-ui/src/services/authService.js`
- **Backend Config** → `/BeachRehberi.API/Program.cs`
- **Backend Auth Logic** → `/BeachRehberi.API/Services/AuthService.cs`

---

**System configured and tested! Ready for development!** 🎉
