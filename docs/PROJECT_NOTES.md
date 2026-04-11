# BeachRehberi API — Proje Teknik Notları

> Bu dosya, repo kökünde dağınık halde bulunan teknik not dosyalarının birleştirilmiş ve düzenlenmiş halidir.
> Arşivlenme tarihi: 2026-04-02

---

## 1. Multi-Tenant Query Filtreleri (Global)

`BeachDbContext.OnModelCreating` içinde her entity için soft-delete ve tenant izolasyonu sağlayan global query filter'lar tanımlıdır:

```csharp
modelBuilder.Entity<Beach>().HasQueryFilter(e => !e.IsDeleted &&
    (_currentUserService == null ||
     _currentUserService.TenantId == null ||
     e.TenantId == _currentUserService.TenantId));

modelBuilder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted &&
    (_currentUserService == null ||
     _currentUserService.TenantId == null ||
     e.TenantId == _currentUserService.TenantId));
```

**Davranış:**
- `IsDeleted == true` olan kayıtlar otomatik olarak filtrelenir (soft-delete).
- `_currentUserService` null ise veya `TenantId` null ise filtre devre dışı kalır (admin/global erişim).
- Aksi halde sadece kullanıcının tenant'ına ait kayıtlar döner.

---

## 2. SaveChangesAsync — Otomatik Tenant Atama

`BeachDbContext.SaveChangesAsync` override'ında yeni eklenen entity'lere otomatik tenant atanır:

```csharp
if (!entry.Entity.TenantId.HasValue && _currentUserService?.TenantId != null)
{
    entry.Entity.TenantId = _currentUserService.TenantId;
}
```

**Davranış:**
- Yeni kayıt eklenirken `TenantId` boşsa, oturumdaki kullanıcının tenant'ı otomatik atanır.
- Zaten tenant'ı olan kayıtlara dokunulmaz.

---

## 3. Migration Başlatma (EF Core)

İlk migration oluşturmak için:

```bash
dotnet ef migrations add InitialCreate --project BeachRehberi.API
dotnet ef database update --project BeachRehberi.API
```

---

## 4. Pre-Commit Hook Taslağı (Claude Review)

Aşağıdaki PowerShell scripti, commit öncesi staged diff'i Claude'a gönderip otomatik review yaptırır:

```powershell
$review = git diff --cached | claude @"
[audit + redteam]
Bu değişiklikleri incele.
- kritik bug var mı
- null reference riski var mı
- güvenlik sorunu var mı
Ciddi sorun varsa BLOCK yaz. Sorun yoksa OK yaz.
"@

$review | Out-File .claude-review.txt

if ($review -match "BLOCK") {
    Write-Host $review
    exit 1
}
```

> Not: Bu script henüz aktif bir git hook olarak yapılandırılmamıştır, sadece bir taslaktır.

---

## 5. Uygulama Katmanı Gerçeklik Notu

Bu repoda `BeachRehberi.Application/` ve `BeachRehberi.API/Features/` altında benzer command/query isimleri bulunuyordu. Aktif HTTP yürütme yolu `BeachRehberi.API/Features/` ve `BeachRehberi.API/Services/` kombinasyonudur; Application altındaki boş auth command/validator stubları kaldırıldı ve MediatR scan yalnızca API assembly'sine indirildi.

Bu nedenle:
- Yeni davranış eklerken önce `BeachRehberi.API/Features/` ve ilgili servisleri kaynak gerçeklik olarak kabul edin.
- `BeachRehberi.Application/` altında kalan ortak DTO/command/query tipleri aktif HTTP handler kaynağı değildir.
- Orta vadede Application katmanı gerçek handler/DTO içeriğiyle doldurulmalı ya da kalan ortak tipler de API/Doman sınırına taşınmalıdır.
