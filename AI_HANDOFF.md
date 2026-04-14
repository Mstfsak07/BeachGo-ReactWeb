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

- Last updated: `2026-04-14`
- Updated by: `codex`
- In progress: `none`
- Last completed item: `implemented minimal Stories feature across backend and Flutter: active story endpoints, mobile repository/providers, beach-top story strip, full-screen viewer and admin URL-based create form`
- Next concrete step: `optional follow-up: add real video playback for story viewer or surface story management into web admin if requested`
- Verification:
  - `beach-ui: npm run lint`
  - `beach-ui: npm run typecheck`
  - `beach-ui: npm run build`
  - `mobile: flutter analyze`
  - `mobile: flutter test`
  - `mobile: flutter build web`
  - `backend: dotnet build BeachRehberi.API/BeachRehberi.API.csproj`
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
  - Story viewer image storyleri tam ekran gosteriyor; video URL'leri su an bilincli olarak guvenli placeholder ile gosteriliyor, gerçek video playback henüz eklenmedi.

## Update Discipline

- Bu dosya kisa tutulmali; uzun log yazma.
- Kalici gercekler burada, ayrintili gecmis `ops/` altinda tutulmali.
- Yeni ajan devraldiginda ilk 2-3 dakikada durumu anlayabilmeli.
- Bir madde artik gecerli degilse sessizce biriktirme; guncelle veya kaldir.
