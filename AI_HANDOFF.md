# AI Handoff

Bu dosya repo icindeki ortak ajan hafizasidir. Hesap degistiginde veya farkli bir ajan devraldiginda once burayi oku, sonra gerekirse ayrintiya in.

## Read First

- Codex: `AGENTS.md` sonra bu dosya
- Claude: `CLAUDE.md` sonra bu dosya
- Gemini: `GEMINI.md` sonra bu dosya

## Project Identity

- Repo: `BeachGo-ReactWeb`
- Ana alanlar:
  - `beach-ui/` React/Vite frontend
  - `BeachRehberi.API/` .NET backend
  - `ops/` operasyon, state ve runbook dosyalari

## Current State Summary

- Mevcut aktif teknik durum icin ana referans `docs/PROJECT_NOTES.md`.
- Faz bazli calisma kayitlari `ops/state/phase-*.json` altinda tutuluyor.
- Son bilinen operasyon ozeti `ops/remaining-operations-runbook.md` icinde.
- Daha eski analizler veya `docs/archive/` altindaki dosyalar tarihsel baglamdir; current-state olarak esas alinmaz.

## What Is Already Done

- Auth, reservation ve payment tarafinda birden fazla sertlestirme ve correctness duzeltmesi daha once uygulanmis.
- Stripe webhook idempotency ve `Pending -> Approved` status update daha once tamamlanmis.
- Business reservations server pagination ve dashboard entegrasyonu daha once tamamlanmis.
- Compose/Redis cleanup, smoke checklist ve project notes current-state duzeltmeleri yapilmis.
- 2026-04-13 tarihinde domain mapping, Cloud Run ops hardening ve secret rotation tarafinda ek operasyon notlari runbook'a islenmis.

## Open / External Blockers

- `api.beachgo.net` icin public DNS tarafinda `api CNAME ghs.googlehosted.com.` kaydi henuz ekli degil; SSL durumu `CertificatePending`.
- Stripe production setup bilincli olarak kapali:
  - `Features__UseRealPayment=false`
  - live Stripe secret'lari henuz tanimli degil
- Git history secret cleanup icin force-push / rewrite karari henuz alinmadi.

## Next Suggested Work

- Kullanici yeni bir is vermediyse once `ops/remaining-operations-runbook.md` icindeki acik maddelerden birini sec.
- Operasyonel devam isi yapiliyorsa `ops/state/phase-12.json` ve runbook birlikte okunmali.
- Uygulama kodunda degisiklik yapmadan once ilgili alanin mevcut davranisini dosya bazinda dogrula; eski analizlere dayanarak direkt edit yapma.

## Validation Snapshot

- Bu repo icinde dogrulama sirasi genel olarak:
  - `lint`
  - `typecheck`
  - `tests`
  - `build`
- Sonraki ajan, yaptigi degisiklik hangi alani etkiliyorsa en az ilgili dogrulamayi yeniden kosmali.
- Yeni bir dogrulama kosuldugunda sonucunu asagidaki bolume ekle.
- 2026-04-16:
  - `backend: dotnet build BeachRehberi.API/BeachRehberi.API/BeachRehberi.API.csproj`
  - `backend: dotnet test BeachRehberi.API/BeachRehberi.API.Tests/BeachRehberi.API.Tests.csproj --filter LoginHandlerTests`
  - `frontend: npm run typecheck`
  - `ops: gcloud run deploy beachrehberi-api`
  - `ops: gcloud run deploy beachgo-ui`
  - `ops: gcloud services enable places.googleapis.com`
  - `ops: dotnet run --project ops/tmp/prod-beach-seed`
  - `ops: GET https://api.beachgo.net/api/Beaches?page=1&pageSize=20`
  - `ops: GET https://api.beachgo.net/api/Stories`
  - `frontend: npm run typecheck`
  - `ops: gcloud run deploy beachgo-ui`
  - `frontend: npm run typecheck` (pass)
  - `frontend: npm run lint` (fail: `src/pages/PremiumPreview.tsx` unused imports `CalendarDays`, `Waves`)
  - `ops: GET https://beachgo.net` / `/beaches` / `/beaches/1` -> `200`
  - `ops: GET https://api.beachgo.net/api/Beaches?page=1&pageSize=3` -> `200`
  - `ops: GET https://api.beachgo.net/api/Stories` -> `200`
  - `ops: Playwright smoke https://beachgo.net{/,/beaches,/beaches/1,/premium-preview}` -> nav `200`, repeated browser error `400 /api/Auth/refresh`
  - `frontend: npm run lint` (pass)
  - `frontend: npm run typecheck` (pass)
  - `ops: gcloud builds submit --config cloudbuild.frontend.yaml .` (pass)
  - `ops: gcloud run deploy beachgo-ui` -> `latestReadyRevisionName=beachgo-ui-00025-c4p`
  - `ops: GET https://beachgo.net` / `/beaches` / `/beaches/1` / `/premium-preview` -> `200`
  - `ops: Playwright smoke https://beachgo.net{/,/beaches,/beaches/1,/premium-preview}` -> nav `200`, browser errors `none`
  - `frontend: npm run lint` (pass)
  - `frontend: npm run typecheck` (pass)
  - `ops: gcloud builds submit --config cloudbuild.frontend.yaml .` (pass)
  - `ops: gcloud run deploy beachgo-ui` -> `latestReadyRevisionName=beachgo-ui-00026-d58`
  - `ops: Playwright smoke https://beachgo.net` -> nav `200`, `Bugun Hava ve Deniz` section visible, browser errors `none`
  - `frontend: npm run lint` (pass)
  - `frontend: npm run typecheck` (pass)
  - `ops: gcloud builds submit --config cloudbuild.frontend.yaml .` (pass)
  - `ops: gcloud run deploy beachgo-ui` -> `latestReadyRevisionName=beachgo-ui-00027-sf9`
  - `ops: Playwright smoke https://beachgo.net` -> nav `200`, featured order `Kalypso Beach Club` -> `La Bohem Beach` -> `Dubai Beach Konyaalti`, browser errors `none`

## Session Update Template

Her ajanin oturum sonunda bu bolumu guncellemesi beklenir:

- Last updated: `2026-04-16`
- Updated by: `codex`
- In progress: `Kullanicidan yeni web hata/fix hedefi bekleniyor`
- Last completed item: `PremiumPreview lint hatalari temizlendi ve public route'larda gereksiz auth bootstrap refresh cagrisi atlandi; beachgo-ui deploy edilip beachgo.net smoke testlerinde /api/Auth/refresh 400 browser error temizlendi`
- Next concrete step: `Auth bootstrap davranisini protected/guest route gecislerinde izleyip gerekirse route-aware refresh stratejisini ilerlet`
- Verification:
  - `mobile: flutter analyze`
  - `mobile: flutter test`
  - `backend: dotnet build BeachRehberi.API/BeachRehberi.API/BeachRehberi.API.csproj`
  - `backend: dotnet test BeachRehberi.API/BeachRehberi.API.Tests/BeachRehberi.API.Tests.csproj --filter LoginHandlerTests`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run revision beachrehberi-api-00025-qj7 register smoke test -> 200`
  - `ops: Cloud Logging 'Verification email sent to ...'`
  - `ops: production DB BusinessUsers count -> 0`
  - `ops: Cloud Run beachrehberi-api latestReadyRevisionName -> 00026-nbl`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00002-7r2`
  - `ops: Cloud Run beachrehberi-api latestReadyRevisionName -> 00028-4c9`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00003-nx7`
  - `ops: GET /api/Beaches/1/google-reviews -> hasPlaceMatch=true, rating=4.6, userRatingCount=2911`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00004-8v9`
  - `ops: Playwright smoke https://beachgo.net/beaches/1 -> Google yorumlari gorunur, Kalypso story gorselleri DOM'da render`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00006-csp`
  - `ops: Playwright smoke https://beachgo.net/beaches/1 story modal -> image rect 1440x1200, caption visible`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00007-4js`
  - `ops: Playwright smoke https://beachgo.net/beaches/1 -> Story Akisi visible, Fotoğraf Galerisi visible, 13 kalypsobeach gallery image rendered`
  - `ops: production DB Beaches.Id=1 CoverImageUrl -> https://kalypsobeach.com.tr/public/rawImage/background/main-background.jpg`
  - `ops: GET /api/Beaches/1 -> imageUrl populated with Kalypso hero`
  - `ops: Playwright smoke https://beachgo.net/ and /beaches/1 -> homepage card image visible, detail hero visible`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00008-hwn`
  - `ops: Playwright smoke home -> /beaches -> /beaches/1 scrollY -> 1400 before nav, 0 after /beaches, 0 after detail`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00009-8gf`
  - `ops: Playwright smoke https://beachgo.net/premium-preview -> sandbox text visible, preview route alive, Kalypso content visible`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00010-lb9`
  - `ops: Playwright smoke https://beachgo.net/premium-preview -> hero visible, curated collection and next move text present in DOM`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00011-xkc`
  - `ops: Playwright smoke https://beachgo.net/ -> home loaded, CTA visible after kum/deniz arka plan denemesi`
  - `frontend: npm run typecheck`
  - `ops: gcloud builds submit --config cloudbuild.frontend.yaml .`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00020-bjp`
  - `ops: GET https://api.beachgo.net/api/Stories -> 2 aktif Kalypso story`
  - `ops: Playwright smoke https://beachgo.net/ -> home story strip title visible, 2 story image rendered above beach cards`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00021-gsq`
  - `ops: Playwright smoke https://beachgo.net/ -> Kalypso home story strip tek kart, viewer icinde 2 progress slot ve caption visible`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00022-ws6`
  - `ops: Playwright smoke https://beachgo.net/beaches/1 -> Sezlong, Loca, Restaurant Masası seçenekleri visible; Loca seçimi /reservation/1 ekranına taşınıyor`
  - `ops: dotnet run --project ops/tmp/prod-beach-seed -> beaches upserted with ids 2-7`
  - `ops: GET https://api.beachgo.net/api/Beaches?page=1&pageSize=20 -> totalCount=7, Kalypso + 6 yeni beach`
  - `ops: GET https://api.beachgo.net/api/Stories -> yeni beachler icin aktif story kayitlari gorunur`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00024-4sb`
  - `ops: Playwright smoke https://beachgo.net/beaches -> Sunshine/Twenty/Dubai kartlari yeni hero image src ile render`
  - `ops: Playwright smoke https://beachgo.net/beaches/{2,3,4,5,6,7} -> detail hero ve galeri gorselleri render`
  - `frontend: npm run lint`
  - `frontend: npm run typecheck`
  - `ops: Cloud Run beachgo-ui latestReadyRevisionName -> 00025-c4p`
  - `ops: Playwright smoke https://beachgo.net{/,/beaches,/beaches/1,/premium-preview} -> nav 200, console/page/http error yok`
- Notes:
  - `2026-04-16`: web auth bootstrap akisina route guard eklendi. `AuthProvider` artik public route'larda (`/`, `/beaches`, `/beaches/:id`, `/premium-preview` vb.) otomatik `/Auth/refresh` cagrisi yapmiyor; bu sayede guest kullanicida browser console'a dusen `400 /api/Auth/refresh` gürültüsü temizlendi.
  - `2026-04-16`: web anasayfaya hero altinda `Bugun Hava ve Deniz` bolumu eklendi. Veri kaynagi olarak ilk featured beach icin `/api/Beaches/{id}/weather` endpoint'i kullaniliyor; veri yoksa kullanici dostu fallback metni gosteriliyor.
  - `2026-04-16`: web anasayfa featured beach secimi sabitlendi: kartlar `Kalypso`, `La Bohem`, `Dubai` olarak geliyor ve ilk kart her zaman `Kalypso`.
  - `2026-04-16`: `C:\Users\akMuratNET\Desktop\Beachs.txt` listesindeki 6 beach icin kaynak toplandi. Resmi site olanlar dogrudan siteden, sitesi olmayanlar Instagram public meta + Google Places ile beslendi.
  - `2026-04-16`: production DB import'u API admin login yerine Cloud SQL Proxy uzerinden dogrudan Npgsql helper ile yapildi. Gecici ops helper `ops/tmp/prod-beach-seed/` altinda tutuluyor; `ops/data/beachs-import-20260416.json` ve `ops/data/beachs-assets-20260416.md` bu importun referans veri dosyalari.
  - `2026-04-16`: production `Beaches` tablosuna su yeni kayitlar eklendi: `Flamingo Lounge`, `Roxy Beach Lounge Antalya`, `Sunshine Beach`, `Twenty Beach & Bistro`, `Dubai Beach Konyaalti`, `La Bohem Beach`.
  - `2026-04-16`: resmi sitesi olmayan Sunshine, Twenty ve Dubai Beach icin hero/story gorseli olarak Instagram profil gorselleri kullanildi; Google Places rating/count/placeId alanlari da production beach kayitlarina yazildi.
  - `2026-04-16`: gorsel kalite polish'inde Sunshine, Twenty ve Dubai Beach icin production `CoverImageUrl` ve aktif `BeachStories` kayitlari Google Places place photo tabanli daha yuksek kaliteli gorsellerle guncellendi. Frontend `beach-ui/src/lib/beachVisuals.ts` merkezi curated hero + gallery fallback haritasi olarak eklendi; `BeachCard` ve `BeachDetail` bunu kullaniyor.
  - `2026-04-16`: beach detail icin `/api/Beaches/{id}/google-reviews` endpoint'i eklendi. `googlePlaceId` yoksa Google Places Text Search ile place id bulmaya calisiyor, sonra Place Details ile `reviews`, `rating`, `userRatingCount`, `googleMapsUri` cekiyor ve ilk basarili eslesmede `Beach.GooglePlaceId` alanina cacheliyor.
  - `2026-04-16`: frontend `GoogleReviewsSection` artik placeholder yerine gerçek backend endpoint'ini kullaniyor; yazar adi/profil linki ve Google Maps linki attribution amaciyla korunuyor.
  - `2026-04-16`: `GOOGLE_PLACES_API_KEY` Secret Manager secret'i olusturuldu ve Cloud Run'a `ApiKeys__GooglePlaces` secret env olarak baglandi.
  - `2026-04-16`: Google tarafinda `places.googleapis.com` ilk etapta disable oldugu icin Places API (New) 403 `SERVICE_DISABLED` veriyordu; API enable edildikten sonra Kalypso icin gerçek review verisi alinabildi.
  - `2026-04-16`: web `Profile` ekranina telefon numarasi alani eklendi. Backend `UsersController` profile response artik `PhoneNumber` donuyor; `PUT /api/users/profile` phone number update ediyor.
  - `2026-04-16`: web `BeachDetail` story strip'i artik `/api/Stories/beach/{id}` endpoint'inden veri cekiyor; Kalypso icin `gallery/5.webp` ve `gallery/1.webp` story gorselleri production DOM'da dogrulandi.
  - `2026-04-16`: web `BeachStoryViewer` artik `document.body` portal'i ile render oluyor; onceki fixed viewer, `transform` tasiyan parent altinda kaldigi icin kucuk aciliyordu. Yeni layout full-screen `object-cover` medya, gradient overlay ve alt caption ile production'da dogrulandi.
  - `2026-04-16`: Kalypso resmi site gallery assetleri (`https://kalypsobeach.com.tr/public/gallery/*.webp`) web detail icin curated fallback olarak baglandi. Bu secim, backend `Photos` relation'inda once not edilen serialize dongusu riskine girmeden gallery section'i dolduruyor.
  - `2026-04-16`: web `BeachStoryBar` premium kart/hero diliyle yenilendi; daha buyuk avatar, story count badge, baslik ve dokunma ipucu eklendi.
  - `2026-04-16`: production DB'de Kalypso (`Beaches.Id=1`) `CoverImageUrl` resmi site hero gorseli `https://kalypsobeach.com.tr/public/rawImage/background/main-background.jpg` olarak guncellendi. API `imageUrl` bu alandan turetildigi icin anasayfa karti ve detail hero ek kod deploy'u olmadan doldu.
  - `2026-04-16`: web router seviyesinde `ScrollToTop` eklendi. Route degistiginde `window.scrollTo({ top: 0, left: 0 })` cagriliyor; artik anasayfa, beaches ve detail gecislerinde onceki scroll pozisyonu korunmuyor.
  - `2026-04-16`: yeni tasarim denemeleri icin ayrik `/premium-preview` route'u eklendi. Sayfa mevcut siteyi etkilemeden hero, kart dili ve editorial landing yaklasimini test etmek icin gercek beach verisini kullanan bir sandbox olarak tasarlandi.
  - `2026-04-16`: premium preview ikinci iterasyonda daha cesur hale getirildi: daha buyuk luxury hero, curated mood metric paneli, concept direction kartlari, curated collection vitrini ve ana siteye parcali tasima icin `Next Move` roadmap bloklari eklendi.
  - `2026-04-16`: home sayfasinda beyaz zeminlerin bir kismi yumuşak kum tonuna cekildi. Kategori, trend ve CTA section'larinda dusuk opaklikli deniz/foto texture katmanlari ile daha atmosferik ama still readable bir canli deneme acildi.
  - `2026-04-16`: home sayfasinda beach kartlarinin ustune `GET /api/Stories` ile beslenen aktif story strip'i eklendi. Mapping `coverImage` icin beach hero yerine dogrudan `story.mediaUrl` kullaniyor; production DOM'da `gallery/5.webp` ve `gallery/1.webp` story kartlari goruldu.
  - `2026-04-16`: home story strip mapping'i beach bazinda gruplanacak sekilde guncellendi. Ayni beach'e ait coklu story kayitlari artik tek kartta toplanip viewer icinde sirali media akisi olarak aciliyor; Kalypso production'da tek kart + 2 medya olarak dogrulandi.
  - `2026-04-16`: web rezervasyon akisina `Sezlong`, `Loca`, `Restaurant Masası` tip secenekleri eklendi. Beach detail sticky reservation kartinda seciliyor, guest reservation formunda degistirilebiliyor ve summary/list ekranlarinda `reservationType` gorunuyor.
  - `2026-04-16`: deploy sonrasi aktif revision'lar `beachrehberi-api-00028-4c9` ve `beachgo-ui-00003-nx7`.
  - `2026-04-16`: production DB cleanup uygulandi. `BusinessUsers` tablosundaki 6 kullanici kaydi ile ilgili `VerificationCodes` ve `RefreshTokens` kayitlari transaction icinde silindi; kalan `BusinessUsers` sayisi `0`.
  - `2026-04-16`: login handler artik tum account tipleri icin `IsEmailVerified=false` durumunda `403` ile `Giriş yapmadan önce e-posta adresinizi doğrulamanız gerekiyor.` mesaji donuyor.
  - `2026-04-16`: Cloud Run `beachrehberi-api` aktif revision `00025-qj7`; `APP_URL=https://beachgo.net` env'i mevcut.
  - `2026-04-16`: `RESEND_API_KEY` Secret Manager secret'i olusturuldu ve Cloud Run'a secret env olarak baglandi. Ilk secret versiyonu newline nedeniyle `ResendClient` header parse hatasi verdi; version `2` no-newline olarak yuklenip sorun giderildi.
  - `2026-04-16`: loglarda `Verification email sent to ...` goruldu; production verification mail delivery aktif.
  - `2026-04-16`: `https://beachgo-ui-837681809323.europe-west1.run.app/register` browser tarafinda `api.beachgo.net` preflight CORS blokaji nedeniyle fail ediyordu; production `AllowFrontend` origin listesine preview Cloud Run UI origin'i eklendi.
  - `window.confirm` kullanimlari shared confirm dialog context uzerine tasindi.
  - `document.body.style.overflow` ve modal history push/pop davranisi hook'lara izole edildi.
  - `localStorage` erisimi storage abstraction altina alindi; `AdminPanel` ve `BeachSettings` axios cagrilari service katmanina tasindi.
  - `GuestReservation/StepDateType` ve `StepPayment` dosyalari `.tsx` formatina tasindi; `BeachSettings` ve `BeachDetail` icindeki sessiz catch path'leri gorunur log/toast ile kapatildi.
  - `mobile/lib/core/models/*` kaynak koddan cikmis durumda; mobile uygulama artik DTO + domain entity ayrimi ile ilerliyor.
  - `mobile/README.md` ve `mobile/FAZLAR.md` legacy `core/models` / freezed anlatimindan temizlendi.
  - `mobile/pubspec.yaml` icinden kullanilmayan `freezed`, `freezed_annotation`, `json_serializable`, `json_annotation` direct bagimliliklari kaldirildi.
  - Generic pagination altyapisi `mobile/lib/core/pagination/` altina tasindi: `PaginatedResult<T>`, `PaginatedState<T,...>`, `PaginatedNotifier<T,...>`.
  - Beach list notifier artik bu generic temel sinifi kullaniyor; infinite scroll sadece default paginated liste modunda aktif, aktif arama/filtre sonucunda mevcut davranis korunup loadMore devre disi kaliyor.
  - `mobile/test/beach_list_provider_test.dart` append, duplicate-request guard ve refresh-failure item preservation senaryolarini kapsiyor.
  - `mobile/test/core/pagination/paginated_notifier_test.dart` generic pagination temelinin favorite-benzeri ikinci bir notifier/state ile tekrar kullanilabildigini dogruluyor.
  - Backend `Stories` endpointleri aktif/non-expired filtre ile calisiyor ve DTO sozlesmesi mobile icin `mediaUrl`, `mediaType`, `expiresAt` alanlarina sadeleştirildi; `IStoryService` DI kaydi eklendi.
  - Mobile `features/stories/` altinda domain/data/presentation akisi eklendi. Beach list tepesinde Instagram-benzeri story strip var; story yoksa section gizleniyor.
  - `/admin` rotasi artik minimal story ekleme formuna gidiyor. Form URL tabanli create kullaniyor; upload sistemi yok.
  - Story viewer image storyleri tam ekran `BoxFit.cover` ile gosteriyor; top progress bar, hold-to-pause, tap next/prev, fade transition, drag-to-dismiss ve subtle loading state eklendi.
  - Story media yuklenemezse viewer blank ekrana dusmeden sonraki story'e geciyor; son story fail olursa viewer kapanıyor.
  - Video URL'leri su an bilincli olarak guvenli lightweight placeholder ile gosteriliyor; gerçek video playback henüz eklenmedi.
  - Mobile reservation feature artik `features/reservation/` altinda aktif: repository DTO/domain mapping, submit notifier, reservation screen ve basic confirmation screen eklendi.
  - Beach detail ekraninda sticky `Rezervasyon Yap` CTA var; rezervasyon rotasi auth-protected ve duplicate submit guard hem UI hem notifier seviyesinde uygulanıyor.
  - Reservation form `date + time + guestCount + optional note` alanlarini topluyor; hata durumunda kullanici girdileri korunuyor ve backend/network mesajlari failure system uzerinden kullanıcı dostu sekilde gosteriliyor.
  - Backend authenticated reservation endpoint'i `ReservationTime` kabul edip response'ta geri donduruyor; mobile success screen secilen saat bilgisini dogrudan gosterebiliyor.
  - Mobile startup artik `/` uzerinden auth gate ile aciliyor: login olmayan kullanici `LoginScreen` goruyor, authenticated kullanici `/app/home` shell'ine gidiyor.
  - Mobile app shell `features/app/` altinda eklendi; alt sekmeler `Kesfet`, `Favoriler`, `Rezervasyonlar`, `Profil`. Home sekmesi mevcut beach list screen'i kullaniyor, digerleri kasitli clean placeholder durumda.
  - `AppConfig.baseUrl` local backend gercegine gore guncellendi: Android emulatorde default `http://10.0.2.2:5143/api`, diger local platformlarda `http://localhost:5143/api`; `API_BASE_URL` dart-define override destekleniyor.
  - `AppConfig` artik `API_BASE_URL` yaninda `API_HOST` ve `API_PORT` dart-define override'larini da destekliyor; fiziksel cihaz icin yalnizca local PC IP'sini vermek yeterli.
  - Backend `launchSettings.json` icinde `http` profili `localhost:5143` olarak normalize edildi; `lan-http` profili `0.0.0.0:5143` bind ederek fiziksel cihaz/LAN testini kolaylastiriyor.
  - Bu makinede `BEACHGO_DB_CONN` su an bos oldugu icin API runtime baslatma/direkt endpoint smoke testi kosulmadi; problem artık mobile baseUrl degil, eksik local DB startup env'i.
  - Mobile auth flow guest-first olacak sekilde guncellendi: uygulama acilista home shell'e girebiliyor, login sadece protected action/route'larda gerekiyor.
  - Mobil login UI artik calisir ve daha derli toplu durumda; email+sifre formu, loading, validation, kayit ol ve misafir olarak devam et aksiyonlari mevcut.
  - `AppTheme` coastal/premium yone cekildi: renkler, typography, button/input/card/navigation stilleri daha tutarli ve daha rafine hale getirildi.
  - `shared_widgets.dart` icindeki loading, error ve skeleton state'leri daha intentional kart/panel sunumuna tasindi; ham/teknik his azaltildi.
  - Beach list ekrani daha guclu hero/search/filter hiyerarsisi ile geliyor; liste hata metinleri `Connection refused` gibi ham teknik ifadeleri gostermeden daha kullanici dostu sekilde sunuluyor.
  - Beach detail ekrani hero overlay, info kartlari, section card yapisi ve daha guclu sticky CTA ile daha premium ve daha kolay taranabilir hale getirildi.
  - Reservation ekrani ust ozet karti, section basliklari, daha temiz form spacing'i ve daha guven veren hata/submit sunumuyla polish edildi.
  - `beachgo_animated_redesign.html` referansi baz alinarak mobile tema koyu coastal stile yaklastirildi: `google_fonts` ile `Syne + Inter`, daha koyu panel/surface renkleri, neon mavi/yesil vurgu ve HTML referansina yakin chip/card/navigation dili uygulandi.
  - Beach detail ekrani artik backend `Reviews` endpoint'inden son yorumlari cekiyor; `Haritada Ac` Google Maps search ve `Yorumlari Goster` Google search aksiyonlari `url_launcher` ile dis uygulamada aciliyor.
  - Beach detail icin `beachWeatherProvider` eklendi; mevcut backend weather endpoint'i kullanilarak hava, deniz sicakligi, ruzgar ve dalga bilgileri kucuk bir `Hava ve Deniz` panelinde gosteriliyor.
  - Mobile auth akisi artik gerçek `RegisterScreen` ile tamamlandi: `/register` kullanici formu, `/business-register` isletme formu. Auth repository/provider `businessRegister` cagrisi destekliyor.
  - Favoriler tabi placeholder olmaktan cikip `Users/favorites` endpoint'ine baglandi; detail ekranindan auth-protected favorite toggle yapilabiliyor.
  - Rezervasyonlar tabi placeholder olmaktan cikip `Reservations/my` endpoint'ine baglandi ve kullanicinin rezervasyonlari listeleniyor.
  - Beach detail ekranindan auth-protected yorum ekleme bottom sheet'i eklendi; submit sonrasi reviews/detail provider'lari invalidate edilerek ekran tazeleniyor.
  - 2026-04-15 sonunda local backend `http://192.168.1.6:5143/api` fiziksel cihaz gelistirme akisi icin tekrar dogrulandi: `GET /api/Beaches?page=1&pageSize=3` 200 dondu, `app-debug.apk` `2412DPC0AG` cihazina adb ile kuruldu ve `com.example.beachgo/.MainActivity` launch edildi.
  - 2026-04-16 itibariyla web launch hazirligi icin mevcut `api/Admin/beaches/import` endpoint'i etrafinda pratik bir import hatti eklendi: `ops/data/beaches.template.json`, `ops/scripts/import-beaches.ps1`, `ops/beach-data-import.md`.
  - `ops/data/kalypso-beach.json` ve `ops/data/kalypso-assets.md` ile ilk gercek beach kaydi hazirlandi; Kalypso Beach Club local PostgreSQL'e dogrudan insert edildi ve `GET /api/Beaches/7` 200 ile dogrulandi.
  - Mevcut backend entity-serialization davranisinda `Photos` relation'i eklendiginde `Beach -> Photos -> Beach` dongusu yuzunden detail endpoint 500 verebiliyor; bu nedenle Kalypso icin simdilik sadece `CoverImageUrl` yazildi, `Photos` tablosundaki kayitlar geri alindi.
  - Cloud Run servisleri guncel: `beachrehberi-api` revision `00022-mg2`, `beachgo-ui` revision `00001-fhg`.
  - `https://beachgo-ui-837681809323.europe-west1.run.app` canli ve `200` donuyor; `https://beachgo-ui-837681809323.europe-west1.run.app/beaches` da `200`.
  - `api.beachgo.net` mapping'i `Ready=True` durumda; `https://api.beachgo.net/api/Beaches?page=1&pageSize=5` `200` donuyor.
  - `beachgo.net` ve `www.beachgo.net` icin Cloud Run domain mapping'leri olusturuldu ancak sertifika durumu `CertificatePending`; public DNS hala eski Cloudflare/Squarespace kayitlarinda.
  - Gerekli DNS kayitlari:
    - `@ A 216.239.32.21`
    - `@ A 216.239.34.21`
    - `@ A 216.239.36.21`
    - `@ A 216.239.38.21`
    - `@ AAAA 2001:4860:4802:32::15`
    - `@ AAAA 2001:4860:4802:34::15`
    - `@ AAAA 2001:4860:4802:36::15`
    - `@ AAAA 2001:4860:4802:38::15`
    - `www CNAME ghs.googlehosted.com.`

## Update Discipline

- Bu dosya kisa tutulmali; uzun log yazma.
- Kalici gercekler burada, ayrintili gecmis `ops/` altinda tutulmali.
- Yeni ajan devraldiginda ilk 2-3 dakikada durumu anlayabilmeli.
- Bir madde artik gecerli degilse sessizce biriktirme; guncelle veya kaldir.
