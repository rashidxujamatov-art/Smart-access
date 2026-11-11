# Bajarilish chek-listi

Manba: CLAUDE.md 12-bo'lim. Modul faqat barcha bandlari `[x]` bo'lganda tugagan hisoblanadi
(Definition of Done: kod + migratsiya + API + UI ekran + ruxsat tekshiruvi + audit + testlar + README + chek-list).
Qarorlar: [decisions.md](decisions.md).

## Asosiy chek-list

```
[~] 0. Monorepo, Docker Compose, .env.example, CI (lint + test)
[ ] 1. AUTH: login, 2FA, refresh, lockout, sessiyalar, logout, parolni almashtirish
[ ] 2. RBAC middleware + 3 rol menyulari
[ ] 3. ORG: bo'limlar, guruhlar, o'chirish bloklari
[ ] 4. EMP: CRUD, profil, Excel import/eksport, yuz enrollment + rozilik, soft delete
[ ] 5. SCH: 5 tur, ierarxiya, istisnolar, validatsiya, preview, kalendar + jadval
[ ] 6. ATT: hisoblash servisi (BR-ATT-01..20), tungi smena, qayta hisoblash, tuzatishlar, monitoring ekrani
[ ] 7. CAM: CRUD, ONVIF, ulanish testi, shifrlangan parol, health, recognition_mode
[ ] 8. VMS: MediaMTX integratsiya, grid layoutlar, drag&drop, oqim boshqaruvi, pleyer, arxiv, eksport + watermark
[ ] 9. GATE: holatlar mashinasi, driver adapterlari (mock/gpio/modbus), fix, watchdog, jurnal, avtomatik ochish
[ ] 10. FACE: provider adapterlari, qidiruv (FISH/rasm), chegara, operator tasdiqi, noma'lum shaxs
[ ] 11. ANPR: provider adapterlari, normallashtirish, allowlist, statistika
[ ] 12. MAP: reja yuklash, hududlar, kamera nuqtalari, oxirgi hudud
[ ] 13. EVT: real vaqt WebSocket, push (FCM), hodisa → video
[ ] 14. REP: 10 hisobot, filtrlar, xodim taqqoslashi, PDF/XLSX eksport (fon)
[ ] 15. USR: CRUD, parol generatsiya, matritsa
[ ] 16. AUD: append-only log, filtrlar
[ ] 17. SET: barcha sozlamalar, tizim holati, qo'lda sinxronlash
[ ] 18. OFF: Edge Gateway servis, SQLite bufer, idempotent sinxronlash
[ ] 19. Desktop EXE (Electron) — qorovul va admin
[ ] 20. Android APK (Flutter) — 11 ekran
[ ] 21. i18n (uz), Dark/Light mode, responsive
[ ] 22. Testlar (11-bo'lim) yashil, README to'liq
[ ] 23. DEMO_MODE: seed ma'lumot, mock kamera/rele/tanish, "DEMO" belgisi
[ ] 24. PULSE integratsiyasi: outbox, idempotent uzatish, ID moslash (ADR-004)
[ ] 25. Talabalar: shaxs turi, import, rozilik (ADR-003)
```

## 0-band tafsiloti

```
[x] Git repozitoriy (main)
[x] backend/: .NET 10 solution — Domain, Application, Infrastructure, WebApi, Tests (build: 0 xato)
[x] frontend/: React + TypeScript + Vite (build: OK)
[x] deploy/docker-compose.yml: PostgreSQL 16, Redis, MediaMTX (fayllar — lokal disk, ADR-005)
[x] .env.example (barcha maxfiy qiymatlar faqat .env da)
[x] .github/workflows/ci.yml: backend (format + build + test), frontend (lint + build)
[x] docs/decisions.md: ADR-001..005
[x] `docker compose up` tekshiruvi — postgres (healthy), redis, mediamtx ishlayapti
[ ] Health endpoint + PostgreSQL ulanishi (EF Core) — 1-band boshida
[x] edge/, desktop/, mobile/ — tegishli bandlarda quriladi (18, 19, 20); 0-band shart emas
```

## UI-SPEC tekshiruvi (UI-SPEC.md D-bo'lim)

Har ekran uchun: element bor · joyi to'g'ri · vazifasi ishlaydi · rol cheklovi to'g'ri.
Ekranlar qurilganda shu yerga jadval qo'shiladi.
