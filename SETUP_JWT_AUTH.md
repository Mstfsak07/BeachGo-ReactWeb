# JWT Authentication Setup Guide

## Overview

Bu dosya React frontend'den .NET Core API'ye JWT authentication ile güvenli veri çekme sistemini işlıklandırmak için yapılan ayarları açıklar.

## Backend Configuration

### 1. Program.cs (Güncellemeler)

- **CORS**: HTTP ve HTTPS localhost URL'leri eklendi (development)
- **JWT**: HttpsRedirection aktif (self-signed certificates işlenmesineMüdahale edebilir)
- **Authentication**: Bearer token gereksinimi konfigüre

```csharp
// CORS Domains (Production)
policy.WithOrigins(
    "https://beachgo.com", 
    "https://www.beachgo.com"
)

// Development HTTPS workaround gerekirse env variable kullan
// ASPNETCORE_URLS=http://localhost:5144
```

### 2. Login Endpoint
- **URL**: POST https://localhost:7296/api/auth/login
- **Request**: `{ "email": "user@email.com", "password": "password" }`
- **Response**: 
  ```json
  {
    "data": {
      "token": "eyJhbGc...",
      "refreshToken": "refresh_token...",
      "email": "user@email.com",
      "role": "user"
    },
    "message": "Success",
    "isSuccess": true
  }
  ```

## Frontend Configuration

### 1. axios.js - API Instance

**File**: `/beach-ui/src/api/axios.js`

- **Base URL**: `https://localhost:7296/api`
- **Request Interceptor**: Tüm isteklere `Authorization: Bearer {token}` header'ı ekler
- **Response Interceptor**: 
  - 401 hatalarında token refresh yapılır
  - Başarısız refresh'te logout tetiklenir
  - Hata detayları console'a yazılır
- **HTTPS Self-Signed Certificate**: 
  - Development'ta `rejectUnauthorized: false` ile işlenir
  - Production'da kaldırılmalı

**Token Storage**:
- Access Token: Memory'de (refreshToken'dan yenilenir)
- Refresh Token: localStorage'da

### 2. authService.js - Auth Business Logic

**File**: `/beach-ui/src/services/authService.js`

**Methods**:
- `login(email, password)`: Giriş yapıp token'ları saklar
- `logout()`: Token'ları temizler
- `isAuthenticated()`: Auth durumunu kontrol eder
- `getUser()`: Kullanıcı bilgisini döndürür
- `refreshToken()`: Token'ı yeniler

**Event Listener**:
- `logout` event'i tetiklenirse otomatik logout yapılır

### 3. api.js - API Endpoints

**File**: `/beach-ui/src/services/api.js`

Tüm API endpoint'leri wrapper fonksiyonlarıyla sağlanır:

```javascript
// Beaches
export const getBeaches = () => api.get('/beaches');
export const searchBeaches = (query) => api.get(`/beaches/search?query=${query}`);

// Events, Reservations, Reviews, Business endpoints...
```

### 4. Login.jsx - Login Component

**File**: `/beach-ui/src/pages/Login.jsx`

- Email/password formu
- Error message display
- Loading state
- Başarılı giriş sonrası /beaches'e yönlendirme

### 5. Beaches.js - Protected Page

**File**: `/beach-ui/src/pages/Beaches.js`

- JWT gerekli (ProtectedRoute wrapper içine alınmalı)
- Plajları listeler
- Arama yetenekleri
- Error handling
- Loading skeleton

### 6. ProtectedRoute.jsx

**File**: `/beach-ui/src/components/ProtectedRoute.jsx`

- Auth kontrolü yapan HOC
- Giriş yapmayan kullanıcıları /login'e yönlendirir

## Setup Steps

### Backend

1. **Self-Signed Certificate Sorunu**
   
   Development'ta HTTPS self-signed certificate'ı bypass etmek için:
   
   ```powershell
   # Windows
   $env:ASPNETCORE_URLS = "http://localhost:5144"
   dotnet run
   ```
   
   Veya HTTPS çalışsın istiyorsanız:
   ```powershell
   # Development certificate oluştur
   dotnet dev-certs https --trust
   ```

2. **Database Migration**
   ```bash
   dotnet ef database update
   ```

3. **Test User Create**
   ```bash
   # POST https://localhost:7296/api/auth/register
   {
     "email": "test@example.com",
     "password": "Password123!"
   }
   ```

### Frontend

1. **Dependencies Check**
   ```bash
   cd beach-ui
   npm install
   ```

2. **Environment Variables** (.env)
   ```
   REACT_APP_API_URL=https://localhost:7296/api
   REACT_APP_ENV=development
   ```

3. **Routes Setup** (App.js)
   
   Protected routes için ProtectedRoute wrapper kullanın:
   
   ```jsx
   <Route path="/beaches" element={<ProtectedRoute><Beaches /></ProtectedRoute>} />
   ```

4. **Logout Hook** (useEffect in App.js)
   
   ```javascript
   useEffect(() => {
     window.addEventListener('logout', () => {
       // User'ı logout et ve /login'e yönlendir
     });
   }, []);
   ```

5. **Run**
   ```bash
   npm start  # port 3000 default
   ```

## HTTPS Self-Signed Certificate Solution

### Geliştirme Ortamında Sorun

Chrome'da `ERR_CERT_INVALID` hatası alırsanız:

**Solution 1: HTTP Kullanın**
```csharp
// Program.cs
app.UseHttpsRedirection(); // Comment out
```

**Solution 2: Certificate Güvenilir Yap**
```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

**Solution 3: axios'ta Bypass**
```javascript
// Sadece development'ta
if (process.env.NODE_ENV === 'development') {
  // Already handled in axios.js
}
```

## Common Errors

### 1. 401 Unauthorized
- **Cause**: Token süresinin dolması/invalid
- **Fix**: Refresh token mekanizması otomatik yapılır
- **Error Handler**: Başarısız refresh'te logout tetiklenir

### 2. CORS Error
- **Cause**: Backend CORS policy'sinde frontend domain yok
- **Fix**: Program.cs'te WithOrigins() listesine ekle
- **Frontend URL**: `https://localhost:3000` veya `https://localhost:5173`

### 3. Network Error / Failed to fetch
- **Cause**: Browser self-signed certificate'ı reddet
- **Fix**: HTTPS certificate trust et veya HTTP kullan
- **Dev**: `dotnet dev-certs https --trust`

### 4. 425 Too Early
- **Cause**: Http/2 push state sorun (nadiren)
- **Fix**: Tarayıcı cache'i temizle, CTRL+Shift+Delete

## Token Refresh Flow

```
1. Request yapılır (Authorization header token ile)
   ↓
2. Token expired mi?
   ├─ EVET → 401 response
   │   ↓
   │   Other requests queued
   │   ↓
   │   Refresh endpoint'e istek
   │   ├─ SUCCESS → new token
   │   │   ↓
   │   │   Queued requests tekrar yapılır
   │   │
   │   └─ FAIL → logout event tetiklenir
   │
   └─ HAYIR → Normal response
```

## Security Notes

1. **Access Token**: Memory'de tutulur (XSS'ten bazı koruma)
2. **Refresh Token**: `HttpOnly` cookie ideal, şu an localStorage
3. **Production**:
   - self-signed certificate kaldır
   - HTTPS certificate'ı güvenilir sağlayıcıdan al
   - CORS domain'i production domain'e ayarlama
   - Refresh token rotation implement edir

## Testing

### Postman Flow

1. **Login**
   ```
   POST https://localhost:7296/api/auth/login
   Body: { "email": "test@example.com", "password": "Password123!" }
   ```
   Response'dan token'ı kopyala

2. **Protected Endpoint**
   ```
   GET https://localhost:7296/api/beaches
   Header: Authorization: Bearer {token}
   ```

### React App Flow

1. Login page açı → `/login`
2. Email/password gir → submit
3. Token'lar localStorage'a kaydedilir
4. `/beaches` page'e yönlendir
5. Plajlar mü API'den çekiliş gösterilir

## Files Modified/Created

- ✅ `beach-ui/src/api/axios.js` - Updated with HTTPS & token refresh
- ✅ `beach-ui/src/services/authService.js` - Complete auth service
- ✅ `beach-ui/src/services/api.js` - API endpoints
- ✅ `beach-ui/src/pages/Login.jsx` - Login component
- ✅ `beach-ui/src/pages/Beaches.js` - Updated with error handling
- ✅ `beach-ui/src/components/ProtectedRoute.jsx` - Route protection
- ✅ `BeachRehberi.API/Program.cs` - CORS configuration updated

## Production Deployment

1. HTTPS certificate'ı güvenilir sağlayıcıdan al
2. axios.js'te `https://production-domain.com` ayarla
3. Program.cs'te production domain'leri CORS whitelist'ine ekle
4. Self-signed certificate bypass code'u kaldır
5. Token expiry süresini güvenli ayarla (e.g., 15 min access, 7 days refresh)
6. Encrypt refresh token storage (localStorage replacement)
