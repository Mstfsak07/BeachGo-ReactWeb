# Beach Data Import

Bu repo icinde plaj temel verilerini en hizli sekilde doldurmak icin mevcut `api/Admin/beaches/import` endpoint'i kullanilir.

## Kapsam

Bu import hatti su alanlari toplu yukler veya gunceller:

- `Beaches`
- ile plajin temel detaylari:
  - ad
  - aciklama
  - adres
  - telefon
  - website
  - instagram
  - saatler
  - ucretler
  - koordinatlar
  - kapasite
  - imkan bool alanlari
  - `todaySpecial`

Mevcut import sunlari kapsamaz:

- `BusinessUsers`
- `BeachPhotos`
- `BeachStories`
- `BeachEvents`
- `Reviews`

Bu alanlar ikinci adimda admin/business panel veya ayri seed/import ile doldurulmali.

## Dosya Formati

Ornek dosya:

- [beaches.template.json](/C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/ops/data/beaches.template.json)
- [kalypso-beach.json](/C:/Users/akMuratNET/Desktop/BeachGo/BeachGo-ReactWeb/ops/data/kalypso-beach.json)

Her kayit `UpdateBeachDto` seklinde olmalidir. Eslesme mantigi:

- ayni `name`
- ayni `address`

bulunursa kayit guncellenir, bulunmazsa yeni beach olusturulur.

## Calistirma

Backend local calisiyorsa:

```powershell
cd C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb
.\ops\scripts\import-beaches.ps1 `
  -Email "admin@beachgo.com" `
  -Password "YOUR_ADMIN_PASSWORD" `
  -ApiBaseUrl "http://localhost:5143/api" `
  -FilePath ".\ops\data\beaches.template.json"
```

Cloud Run API'ye import atmak icin:

```powershell
.\ops\scripts\import-beaches.ps1 `
  -Email "admin@beachgo.com" `
  -Password "YOUR_ADMIN_PASSWORD" `
  -ApiBaseUrl "https://api.beachgo.net/api" `
  -FilePath ".\ops\data\kalypso-beach.json"
```

## Sonraki Adim

Gercek urun gorunumu icin importtan hemen sonra sunlar yapilmali:

1. her plaja cover/galleri foto yukle
2. business hesabini ilgili plaja bagla
3. 1-3 aktif story ekle
4. yakin tarihli 1-2 event ekle
5. gerekiyorsa ilk yorumlari seed et

## Kalypso Notu

`kalypso-beach.json` icindeki alanlarin kaynagi:

- resmi site:
  - isim
  - website
  - WhatsApp telefonu
  - Instagram kullanici adi
  - beach club / gastronomy / bar / lounger bilgileri
- ucuncu taraf listeleme:
  - acik adres
- yaklasik inference:
  - koordinatlar
  - bazi boolean facility alanlari

Kalypso icin ikinci turda manuel netlestirilmesi gereken alanlar:

- tam calisma saatleri
- giris ucreti
- sezlong / daybed fiyatlari
- kapasite
- cover ve galeri gorselleri
