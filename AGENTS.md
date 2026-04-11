# AGENTS.md
<!-- dikkate alma
Bu repo için çalışma modeli:

1. İlk iş: verilen review/assessment dosyasını oku.
2. Sorunları severity + dependency'ye göre fazlara ayır:
   - Phase 1: critical/blocker
   - Phase 2: correctness/security
   - Phase 3: maintainability/refactor
   - Phase 4: tests/docs/cleanup
3. Her faz için task listesi üret.
4. Her task için executor seç:
   - Claude CLI -> büyük kod değişiklikleri
   - Gemini CLI -> gap filling, alternative fix, second-pass validation
5. Uygulama sonrası doğrulama zorunlu:
   - lint
   - typecheck
   - tests
   - build
6. Doğrulama geçmeden sonraki faza geçme.
7. Başarısız görevlerde retry yap, max 3.
8. Tüm işlemleri state dosyalarına ve loglara yaz.
9. Kapsam dışı değişiklik yapma.
10. Her aşamada en küçük güvenli diff'i tercih et.
11. Oluşturulan ve güncellenen tüm dosyalarda UTF-8 kullan, Türkçe karakterleri bozma. -->
