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

cd backend;  dotnet build; dotnet test
cd frontend; npm install; npm run dev
```

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

## Huquqiy eslatma (BR-PRIV-03)

Biometrik ma'lumotlarni qayta ishlash uchun har bir shaxsning (voyaga yetmaganlar uchun — ota-onasining)
yozma roziligi va huquqiy asos talab etiladi. Korxona O'zbekiston Respublikasining shaxsga doir
ma'lumotlar to'g'risidagi qonunchiligi talablarini bajarishi shart.
