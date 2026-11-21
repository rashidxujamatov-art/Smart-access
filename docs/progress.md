# Bajarilish chek-listi

Manba: CLAUDE.md 12-bo'lim. Modul faqat barcha bandlari `[x]` bo'lganda tugagan hisoblanadi
(Definition of Done: kod + migratsiya + API + UI ekran + ruxsat tekshiruvi + audit + testlar + README + chek-list).
Qarorlar: [decisions.md](decisions.md).

## Asosiy chek-list

```
[x] 0. Monorepo, Docker Compose, .env.example, CI (lint + test)
[~] 1. AUTH: login, 2FA, refresh, lockout, sessiyalar, logout, parolni almashtirish
[~] 2. RBAC middleware + 3 rol menyulari
[~] 3. ORG: bo'limlar, guruhlar, o'chirish bloklari
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
[x] Health endpoint + PostgreSQL ulanishi (EF Core, AddDbContextCheck)
[x] edge/, desktop/, mobile/ — tegishli bandlarda quriladi (18, 19, 20); 0-band shart emas
```

## 1-band tafsiloti — AUTH

```
[x] Migratsiya InitialAuth: tenants, users, sessions, audit_logs, settings
[x] audit_logs append-only — UPDATE/DELETE/TRUNCATE trigger bilan taqiqlangan (BR-AUD-03)
[x] BR-AUTH-01 ro'yxatdan o'tish endpointi yo'q; birinchi admin — `bootstrap` buyrug'i
[x] BR-AUTH-02 Argon2id (m=19 MiB, t=2, p=1); parol logga/auditga tushmaydi
[x] BR-AUTH-03 tenant_code + login + parol; umumiy xato "invalid_credentials"
[x] BR-AUTH-04 5 xato → 15 daqiqa blok (security.lockout_attempts); har urinish auditda (IP, qurilma, login)
[x] BR-AUTH-05 access JWT 15 daq; refresh 7 kun / 12 soat; httpOnly cookie (web) yoki tana (native); rotatsiya
[x] BR-AUTH-06 TOTP (shifrlangan sir); 2FA kod oynasi; admin uchun majburiy sozlash (security.require_2fa_for_admin)
[x] BR-AUTH-07 GET/DELETE /sessions (admin); tugatilgan sessiya tokeni darhol yaroqsiz (har so'rovda tekshiriladi)
[x] BR-AUTH-08 rolga qarab bosh ekran (guard → /guard, admin → /admin, manager → /manager)
[x] BR-AUTH-09 logout refresh tokenni bekor qiladi + audit
[ ] BR-AUTH-10 o'z hisobini / oxirgi adminni o'chirish taqiqi — DELETE /users endpointi bilan 15-bandda
[x] BR-USR-02 birinchi kirishda parolni almashtirish majburiy (cheklangan token)
[x] API: login, 2fa, refresh, logout, me, change-password, 2fa/setup, 2fa/enable, sessions
[x] UI: D01 Login + 2FA kod oynasi + parolni almashtirish + 2FA sozlash oynalari
[x] Testlar: 29 ta (dotnet test, Testcontainers PostgreSQL) — yashil
[x] Brauzerda qo'lda tekshirildi: 3 rol kirishi, noto'g'ri parol, yangilashda sessiya, chiqish, boshqa rol sahifasi
[x] README: Autentifikatsiya bo'limi
```

## 2-band tafsiloti — RBAC va menyular

```
[x] Backend: [Authorize(Roles=...)] + 403 JSON + audit "forbidden" (BR-RBAC-01, BR-RBAC-04)
[x] Frontend: AppShell (UI-SPEC A.1) — sidebar, rol menyusi dizayn tartibida, top bar, mavzu, chiqish
[x] BR-RBAC-02 boshqa rol manziliga kirish → o'z bosh ekraniga qaytarish
[ ] Har bir modul endpointlari uchun rol tekshiruvi va 403 testlari — modullar bilan birga (3–17-bandlar)
[ ] Menyu bandlari ortidagi ekranlar — tegishli bandlarda (hozir faqat sarlavha)
```

## 3-band tafsiloti — ORG

```
[x] Migratsiya Org: departments, groups (soft delete, deleted_at)
[x] BR-ORG-01 Bo'lim → Guruh; guruh albatta bitta bo'limga tegishli; boshqa korxona bo'limi ko'rinmaydi
[ ] BR-ORG-02 har guruhga bitta grafik (majburiy) — grafiklar jadvali 5-bandda (SCH)
[ ] BR-ORG-03 guruh grafigi o'zgarsa xodimlarga avtomatik (effective_from) — 5-band
[x] BR-ORG-04 ichida guruh bor bo'limni o'chirish taqiqlanadi (409 department_has_groups)
[ ] BR-ORG-04 ichida xodim bor guruhni o'chirish taqiqi — xodimlar jadvali 4-bandda (EMP)
[x] BR-ORG-05 nom unikal (bo'lim — korxonada, guruh — bo'lim ichida), katta-kichik harfsiz; o'chirilgan nom qayta ishlatiladi
[x] API: GET/POST /departments, PATCH/DELETE /departments/:id, GET/POST /groups, PATCH/DELETE /groups/:id
[ ] API: POST /groups/:id/schedule — 5-band
[x] Ruxsat: CRUD — admin; o'qish — admin va mas'ul shaxs (davomat/hisobot filtrlari); qorovul — 403 + audit
[x] Audit: yaratish/tahrirlash/o'chirish (eski va yangi qiymat), rad etilgan o'chirish ham
[x] UI: D11 — toolbar, guruh kartalari, o'chirish tasdig'i, bo'limlar oynasi; yuklanish/bo'sh/xato holatlari
[ ] UI: D11 kartadagi "N xodim" + avatarlar — 4-band; "Guruh grafigi" select — 5-band
[x] Testlar: 12 ta (OrgTests) — yashil
[x] Brauzerda tekshirildi: bo'lim yaratish, takroriy nom, bo'limsiz guruh, guruh yaratish, guruhi bor bo'limni o'chirish
```

## UI-SPEC tekshiruvi (UI-SPEC.md D-bo'lim)

Har ekran uchun: element bor · joyi to'g'ri · vazifasi ishlaydi · rol cheklovi to'g'ri.
Ekranlar qurilganda shu yerga jadval qo'shiladi.

### D01 — Login

| Element | Bor | Joyi | Vazifasi | Rol |
|---|---|---|---|---|
| Logo, sarlavha, 3 qator, DEMO eslatmasi (chap, navy) | [x] | [x] | [x] | — |
| Forma kartasi (max 400 px) | [x] | [x] | — | — |
| Korxona identifikatori | [x] | [x] | [x] | — |
| Login | [x] | [x] | [x] | — |
| Parol + ko'z tugmasi | [x] | [x] | [x] | — |
| Eslab qolish | [x] | [x] | [x] (12 soat / 7 kun) | — |
| Xato banneri | [x] | [x] | [x] | — |
| Tizimga kirish (to'liq kenglik) | [x] | [x] | [x] → D02/D07/D29, 2FA oynasi | [x] |
| Izoh matni | [x] | [x] | — | — |
| Demo hisoblar chiplari (faqat DEMO_MODE) | [x] | [x] | [x] | — |
| Enter formani yuboradi | [x] | — | [x] | — |

### A.1 — Karkas

| Element | Bor | Joyi | Vazifasi |
|---|---|---|---|
| Logo + SMART ACCESS + rol nomi | [x] | [x] | — |
| Menyu bandlari (3-bo'lim tartibida), faol band ko'k | [x] | [x] | [x] |
| Avatar + FISH + server holati | [x] | [x] | [x] |
| Chiqish tugmasi | [x] | [x] | [x] |
| Breadcrumb + H1 | [x] | [x] | — |
| Fix indikatori | [ ] | — | 9-band (GATE) |
| Server holati badge | [x] | [x] | [x] (/health, 30 s) |
| Dark/Light tugmasi | [x] | [x] | [x] (saqlanadi) |
| Oflayn banner | [ ] | — | 18-band (OFF) |

### D11 — Guruhlar va bo'limlar

| Element | Bor | Joyi | Vazifasi | Rol |
|---|---|---|---|---|
| Ierarxiya izohi (toolbar chapida) | [x] | [x] | — | admin |
| Bo'lim filtri / Bo'lim qo'shish | [x] | [x] | [x] (filtr; bo'limlar CRUD oynasi) | [x] |
| "Yangi guruh nomi" input | [x] | [x] | [x] | [x] |
| Guruh yaratish (asosiy, eng o'ngda) | [x] | [x] | [x] (tanlangan bo'limga) | [x] |
| Guruh kartalari gridi | [x] | [x] | — | — |
| Karta: nom, bo'lim badge, O'chirish | [x] | [x] | [x] (tasdiq oynasi) | [x] |
| "N xodim" + avatarlar | [ ] | — | 4-band | — |
| "Guruh grafigi" select | [ ] | — | 5-band | — |
