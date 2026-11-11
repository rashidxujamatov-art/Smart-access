# Arxitektura qarorlari (ADR)

CLAUDE.md 13-bo'limdagi ochiq savollarga berilgan javoblar va undan kelib chiqqan qarorlar.
Har bir qaror sanasi va kim qabul qilgani bilan.

---

## ADR-001 — Backend: .NET 10 (Clean Architecture)

- **Sana:** 2026-11-11. **Qaror:** loyiha egasi.
- **Javob:** 13-bo'lim, 3-savol. CLAUDE.md standarti NestJS edi, egasining tanlovi — .NET 10.
- **Sabab:** egasining PULSE loyihasi .NET 10 da; bir xil uslub kuzatish va qo'llab-quvvatlashni osonlashtiradi.
- **Tuzilma:** `backend/src/` — `SmartAccess.Domain` → `SmartAccess.Application` → `SmartAccess.Infrastructure` → `SmartAccess.WebApi`; testlar `backend/tests/SmartAccess.Tests`.
- **Baza:** PostgreSQL 16 (CLAUDE.md 2.1), EF Core migratsiyalari (CLAUDE.md 0-bo'lim, 7-qoida).
- **O'zgarmaydi:** web admin — React + TypeScript + Vite; desktop — Electron; mobil — Flutter.

## ADR-002 — Joylashtirish: gibrid (korxona ichi + alohida Hetzner server)

- **Sana:** 2026-11-11. **Qaror:** loyiha egasi.
- **Korxona ichidagi server (on-premise):** Edge Gateway, VMS (MediaMTX + FFmpeg yozish, arxiv), yuz va raqam tanish (BR-ARC-02 — bulutsiz), shlagbaum kontrolleri. Sabab: video oqimi hajmi, 500 ms rele talabi, oflayn ishlash (BR-OFF-01..06).
- **Markaziy API:** PULSE serveridan ALOHIDA yangi Hetzner x86 vServer (CX seriyasi). PostgreSQL, Redis, API, hisobotlar, PULSE integratsiyasi.
- **Deploy uslubi PULSE'dan olinadi:** Docker + Caddy (HTTPS avtomatik), `deploy/deploy.sh`, tungi zaxira (02:00, 14 kun — CLAUDE.md 10-bo'lim), UFW (22/80/443), fail2ban (PULSE'dagi ochiq masalani bu yerda takrorlamaymiz).
- **Web admin:** Vercel (ixtiyoriy) yoki Caddy orqali statik fayl sifatida.

## ADR-003 — Kuzatiladigan shaxslar: xodimlar VA talabalar

- **Sana:** 2026-11-11. **Qaror:** loyiha egasi.
- Smart Access xodimlar bilan bir qatorda talabalarning kirish-chiqishini ham qayd etadi.
- **Ochiq savollar** (CLAUDE.md 13-bo'lim, 9–11) — xavfsiz standart bilan davom etiladi:
  - Talabalar uchun davomat grafigi qayerdan olinadi? Standart: talabalar uchun Smart Access faqat kirish/chiqish hodisasini qayd etadi va PULSE'ga uzatadi; dars jadvali bo'yicha davomatni PULSE hisoblaydi.
  - Talabalar ro'yxati qayerdan keladi? Standart: Excel import (BR-EMP-07 bilan bir xil oqim) + keyinchalik PULSE'dan sinxronlash.
  - Talabalar biometrik roziligi (voyaga yetmaganlar — ota-ona roziligi). Standart: `biometric_consent` majburiy, rozilikni kim bergani qayd etiladi.

## ADR-004 — PULSE integratsiyasi (chiquvchi)

- **Sana:** 2026-11-11. **Qaror:** loyiha egasi (talab), texnik shakl — taklif.
- Smart Access har bir kirish/chiqish hodisasini PULSE'ga uzatadi; PULSE kim kelgan/kelmaganini shu orqali biladi.
- **Shakl:** transactional outbox (`integration_outbox` jadvali) → fon ishchisi → PULSE HTTP API. Har hodisada `event_id` (UUID) — PULSE tomonida idempotent qabul.
- **Moslash:** `external_links` (Smart Access shaxs ID ↔ PULSE ID).
- **Hozircha:** PULSE o'zgartirilmaydi. Smart Access tomoni `PULSE_ENABLED=false` bilan quriladi; qabul qiluvchi endpoint PULSE'ga keyin qo'shiladi.
