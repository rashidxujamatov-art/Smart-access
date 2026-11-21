# Smart Access — kirish-chiqish nazorati va videokuzatuv platformasi

Sanoat korxonasi va ta'lim muassasasi uchun: yuz tanish (Face Recognition), avtomobil raqamini tanish (ANPR),
shlagbaum boshqaruvi, ko'p kamerali videokuzatuv, ish grafigi asosida davomat va PULSE bilan integratsiya.

- Biznes qoidalari va arxitektura: [CLAUDE.md](CLAUDE.md)
- Ekranlar: [UI-SPEC.md](UI-SPEC.md), dizayn: [design/](design/)
- Qarorlar: [docs/decisions.md](docs/decisions.md)
- Holat: [docs/progress.md](docs/progress.md)

## Tuzilma

| Papka | Nima |
|---|---|
| `backend/` | .NET 10, Clean Architecture: Domain → Application → Infrastructure → WebApi |
| `frontend/` | Web admin — React + TypeScript + Vite |
| `edge/` | Edge Gateway (korxona ichida, oflayn bufer) |
| `desktop/` | Qorovul va admin klienti (Electron) |
| `mobile/` | Android ilova (Flutter) |
| `deploy/` | Docker Compose, server skriptlari |

## Ishga tushirish (ishlab chiqish)

Kerak: .NET SDK 10, Node 24, Docker Desktop.

```powershell
Copy-Item .env.example .env          # qiymatlarni o'zgartiring
docker compose -f deploy/docker-compose.yml --env-file .env up -d

cd backend;  dotnet test                                     # Docker kerak (Testcontainers)
dotnet run --project src/SmartAccess.WebApi -- seed-demo     # demo korxona va hisoblar
dotnet run --project src/SmartAccess.WebApi --launch-profile http   # API: http://localhost:5148
cd frontend; npm install; npm run dev                       # UI: http://localhost:5173
```

Lokal ishga tushirishda `ConnectionStrings__Default` da `Host=localhost` bo'lishi kerak
(`.env` dagi `Host=postgres` — Docker tarmog'i ichidan ulanish uchun).

## Autentifikatsiya (CLAUDE.md 4.1)

- **Ro'yxatdan o'tish yo'q** (BR-AUTH-01). Birinchi korxona va administrator bir marta yaratiladi:
  ```powershell
  $env:BOOTSTRAP_TENANT_CODE="KORXONA-001"; $env:BOOTSTRAP_TENANT_NAME="..."
  $env:BOOTSTRAP_ADMIN_LOGIN="admin"; $env:BOOTSTRAP_ADMIN_FULL_NAME="..."; $env:BOOTSTRAP_ADMIN_PASSWORD="<kamida 10 belgi>"
  dotnet run --project src/SmartAccess.WebApi -- bootstrap
  ```
  Birinchi kirishda parolni almashtirish va 2FA ni yoqish majburiy.
- **Parollar** — Argon2id. **TOTP siri** va boshqa maxfiy maydonlar — AES-256-GCM (`ENCRYPTION_KEY_BASE64`).
- **Tokenlar:** access JWT 15 daqiqa; refresh 7 kun ("Eslab qolish" o'chiq — 12 soat), har yangilashda almashadi.
  Web — httpOnly cookie; desktop/mobil — `X-Client: native` sarlavhasi bilan javob tanasida.
- **Blok:** ketma-ket 5 xato — 15 daqiqa. Har urinish audit logda.
- **Sessiyalar:** administrator istalgan sessiyani tugatadi — token darhol yaroqsiz.
- **Audit log** baza darajasida o'zgartirib/o'chirib bo'lmaydi (trigger).
- **Demo:** `seed-demo` — korxona `KORXONA-001`, loginlar `admin`, `qorovul`, `qorovul2`, `mas_ul`, parol `demo`
  (faqat demo; demo korxonada admin uchun 2FA majburiyati o'chiq).

## Ochiq savollar va standart qiymatlar

CLAUDE.md 13-bo'lim. Javob berilganlari — [docs/decisions.md](docs/decisions.md).

| # | Savol | Holat |
|---|---|---|
| 1 | Kamera / terminal / ANPR vendori | Ochiq. Standart: Mock + umumiy webhook adapteri |
| 2 | Shlagbaum rele kontrolleri | Ochiq. Standart: Mock + GPIO |
| 3 | Backend tili | **Hal qilindi:** .NET 10 (ADR-001) |
| 4 | Desktop | Standart: Electron |
| 5 | Multi-tenant | Standart: `tenant_id` bor, bitta tenant |
| 6 | ANPR davomatga hisoblansinmi | Standart: yo'q |
| 7 | Push xizmati | FCM; Telegram — yo'q |
| 8 | Litsenziya chegaralari | Standart: cheklanmagan |
| 9 | Talabalar davomati qanday hisoblanadi | Ochiq. Standart: PULSE hisoblaydi (ADR-003) |
| 10 | Talabalar ro'yxati manbai | Ochiq. Standart: Excel import |
| 11 | Voyaga yetmagan talabalar biometrik roziligi | Ochiq. Standart: rozilik majburiy, kim bergani qayd etiladi |
| 12 | 2FA ni yoqish ekrani UI-SPEC'da yo'q | Ochiq. Standart: kirishdan keyin oyna (QR + kod); API `POST /auth/2fa/setup`, `/auth/2fa/enable` |
| 13 | Birinchi kirishda parolni almashtirish ekrani UI-SPEC'da yo'q | Ochiq. Standart: kirishdan keyin oyna |
| 14 | "DEMO" belgisi top bar'da (UI-SPEC A.1 da yo'q, CLAUDE.md 0.4 talab qiladi) | Standart: top bar'da, server holatidan chapda, faqat DEMO_MODE |

## Huquqiy eslatma (BR-PRIV-03)

Biometrik ma'lumotlarni qayta ishlash uchun har bir shaxsning (voyaga yetmaganlar uchun — ota-onasining)
yozma roziligi va huquqiy asos talab etiladi. Korxona O'zbekiston Respublikasining shaxsga doir
ma'lumotlar to'g'risidagi qonunchiligi talablarini bajarishi shart.
