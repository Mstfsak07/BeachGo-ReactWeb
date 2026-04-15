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

## Session Update Template

Her ajanin oturum sonunda bu bolumu guncellemesi beklenir:

- Last updated: `2026-04-16`
- Updated by: `codex`
- In progress: `web publishing is paused; current focus is filling real beach content before launch`
- Last completed item: `prepared and inserted the first real beach record (Kalypso Beach Club) into the local PostgreSQL dataset, and added reusable beach import assets/scripts under ops/`
- Next concrete step: `continue with the next real beach records, then handle business assignment and a safe gallery/story import flow`
- Verification:
  - `mobile: flutter analyze`
  - `mobile: flutter test`
  - `backend: dotnet build BeachRehberi.API/BeachRehberi.API/BeachRehberi.API.csproj`
- Notes:
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

## Update Discipline

- Bu dosya kisa tutulmali; uzun log yazma.
- Kalici gercekler burada, ayrintili gecmis `ops/` altinda tutulmali.
- Yeni ajan devraldiginda ilk 2-3 dakikada durumu anlayabilmeli.
- Bir madde artik gecerli degilse sessizce biriktirme; guncelle veya kaldir.
