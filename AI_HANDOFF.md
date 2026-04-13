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

- Last updated: `2026-04-13`
- Updated by: `codex`
- In progress: `none`
- Last completed item: `converted Claude-Analiz.txt into execution-oriented mobile readiness backlog`
- Next concrete step: `if requested, start with P0 frontend mobile-readiness cleanup from Claude-Analiz.txt`
- Verification:
  - `documentation-only change; no code validation run`
- Notes:
  - `AI_HANDOFF.md` current shared context file olarak olusturuldu.
  - `AGENTS.md`, `CLAUDE.md` ve `GEMINI.md` bu dosyaya yonlenecek sekilde duzenlendi.
  - `Claude-Analiz.txt` soyut onerilerden cikarilip repo kaniti, oncelik ve kabul kriterleri olan backlog formatina cevrildi.

## Update Discipline

- Bu dosya kisa tutulmali; uzun log yazma.
- Kalici gercekler burada, ayrintili gecmis `ops/` altinda tutulmali.
- Yeni ajan devraldiginda ilk 2-3 dakikada durumu anlayabilmeli.
- Bir madde artik gecerli degilse sessizce biriktirme; guncelle veya kaldir.
