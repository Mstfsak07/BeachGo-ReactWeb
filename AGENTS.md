# AGENTS.md

Bu repo icinde ise baslamadan once ilk okunacak dosya `AI_HANDOFF.md` dosyasidir.

## Codex Startup Order

1. `AI_HANDOFF.md` dosyasini oku.
2. Gerekirse `docs/PROJECT_NOTES.md` ve `ops/remaining-operations-runbook.md` ile current-state dogrulamasi yap.
3. `ops/state/` altindaki en yeni phase dosyasina bakarak yarim kalan isi ve son dogrulama durumunu anla.
4. Kullanici yeni bir hedef vermediyse `AI_HANDOFF.md` icindeki `Next Suggested Work` alanini referans al.

## Working Rules

- `AI_HANDOFF.md` current working memory dosyasidir; tarihsel analizden daha once gelir.
- Eski analiz veya archive notlari, `AI_HANDOFF.md` ile celisirse current-state olarak kabul edilmez.
- Kapsam disi degisiklik yapma.
- En kucuk guvenli diff'i tercih et.
- Kod veya operasyon degisikligi yaptiysan oturum sonunda `AI_HANDOFF.md` icindeki ilgili alanlari guncelle.
- Dogrulama yapildiysa sonucunu `AI_HANDOFF.md` icindeki `Validation Snapshot` alanina isle.

## Shared Handoff Rule

Bu repo birden fazla ajan ve hesap degisimi ile kullaniliyor. Codex, Claude ve Gemini arasinda baglam kaybini onlemek icin:

- Kalici durum ozeti `AI_HANDOFF.md`
- Daha derin operasyon baglami `ops/remaining-operations-runbook.md`
- Faz bazli kayitlar `ops/state/phase-*.json`

kaynak sirasi ile kullanilmalidir.
