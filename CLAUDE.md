# CLAUDE.md — Industrial Smart Access Control & Surveillance Platform

> Bu fayl Claude Code uçun loyihaning yagona manbasi (single source of truth).
> Kod yozişdan oldin töliq öqing. Har bir bölimdagi qoidalar MAJBURIY.
> Hujjat yangi özbek lotin alifbosida yozilgan (Ö, Ğ, Ş, Ç). Kod, jadval, API va özgaruvçi nomlari — inglizça (ASCII).

---

## 0. CLAUDE CODE UÇUN İŞ QOIDALARI (eng muhim)

1. **Heç narsani taşlab ketma.** Har bir modul uçun 4-bölimdagi barça biznes qoidalari, 6-bölimdagi barça jadvallar, 7-bölimdagi barça API'lar va 9-bölimdagi barça ekranlar amalga oşirilişi şart.
2. **Reja → kod → tekşiruv.** Har bir modulni boşlaşdan oldin `docs/progress.md` faylida çeklist yarat (pastdagi 12-bölimdan nusxa), har bir band bajarilganda `[x]` qöy. Çeklist tölmagunça modul tugagan hisoblanmaydi.
3. **TODO, "keyinroq", böş funksiya (stub) qoldirma.** Agar biror narsa haqiqatan apparatga boğliq bölsa — interfeys (adapter) va `Mock` amalga oşirişini yoz, real adapter uçun aniq `NotImplemented` xatosi va README'da izoh qoldir.
4. **Demo/simulyatsiya alohida.** `DEMO_MODE=true` bölganda soxta kamera oqimi, soxta rele, soxta tanış natijalari işlaydi. UI'da har doim "DEMO" belgisi körinadi. Simulyatsiyani heç qaçon haqiqiy apparat işlayapti deb körsatma.
5. **Taxmin qilma.** Hujjatda yöq narsani öziça qöşma. Noaniq joy bölsa — 13-bölimdagi "Oçiq savollar" röyxatiga qöş va xavfsiz standart qiymat bilan davom et.
6. **Har bir biznes qoidasi uçun test.** 4-bölimdagi har bir `BR-xxx` qoida kamida bitta avtomatik test bilan qoplanadi (test nomida qoida raqami: `test_BR_SCH_07_night_shift_crosses_midnight`).
7. **Migratsiyalar.** Ma'lumotlar bazasi faqat migratsiya orqali özgaradi. Seed (demo ma'lumot) alohida skriptda.
8. **Xavfsizlik standart holatda yoqiq.** Parollar xeşlanadi, biometrik şablonlar şifrlanadi, har bir endpoint rolni tekşiradi, har bir muhim amal audit logga yoziladi.
9. **Matnlar.** Barça UI matnlari i18n faylida (`uz`), kodda qattiq yozilmaydi. Texnik atamalar kerak joyda xalqaro nomi bilan (Face Recognition, ANPR, RTSP).
10. **Definition of Done (har bir modul):** kod + migratsiya + API + UI ekran + ruxsat tekşiruvi + audit + testlar + README bölimi + çeklist `[x]`.
11. **UI-SPEC.md MAJBURIY.** Har bir ekranni qurişdan oldin `UI-SPEC.md` dagi şu ekran bölimini töliq öqi. Unda har bir tugma va element qayerda turişi va nima qilişi yozilgan. Taxmin bilan joylaştirma; u yerda yöq tugma qöşma, bor tugmani taşlab ketma.

---

## 1. LOYİHA MAQSADİ

Sanoat korxonasi uçun kiriş-çiqiş nazorati va intellektual videokuzatuv platformasi:

- Xodimlarni yuz orqali tanış (Face Recognition).
- Avtomobillarni davlat raqami orqali tanış (ANPR / LPR).
- Kiriş va çiqişni avtomatik qayd etiş.
- Şlagbaumni masofadan boşqariş (oçiş / yopiş / fix rejimlari).
- Köp kamerali videokuzatuv, jonli va arxiv köriş.
- Xodimning oxirgi aniqlangan hududini körsatiş (aniq koordinata EMAS).
- İş grafigi asosida keçikiş, öz vaqtida keliş, erta ketiş, qöşimça iş vaqtini hisoblaş.
- Ma'lumotlarni markaziy serverga uzatiş, onlayn hisobot.
- İnternet uzilganda lokal işlaş va keyin sinxronlaş.

Platformalar: **Windows desktop (EXE)**, **Android (APK)**, **Web admin panel**. Barçasi bitta dizayn tizimi va bitta backend API'dan foydalanadi.

Dizayn manbasi: "Smart Access Control va Videokuzatuv" canvas (Claude Design) — 30 ta desktop ekran + 11 ta mobil ekran + dizayn tizimi. UI aynan şu dizaynga mos qilinadi.

---

## 2. ARXİTEKTURA VA TEXNOLOGİYALAR

### 2.1 Komponentlar

| Komponent | Vazifasi | Tavsiya etilgan texnologiya |
|---|---|---|
| Central API | Autentifikatsiya, biznes logika, hisobotlar, sinxronlaş | **.NET 10 (C#), Clean Architecture** — tanlangan (docs/decisions.md ADR-001) |
| Ma'lumotlar bazasi | Asosiy saqlaş | PostgreSQL 16 |
| Keş / navbat | Real vaqt hodisalari, navbat | Redis (pub/sub + streams) |
| Real vaqt kanali | UI'ga hodisalar | WebSocket (Socket.IO yoki native WS) |
| Video Management Service (VMS) | RTSP qabul qiliş, WebRTC/HLS'ga aylantiriş, yoziş, arxiv | MediaMTX + FFmpeg (segmentli yoziş) |
| Recognition Service | Yuz va raqamni tanış adapterlari | Adapter interfeysi (2.3 ga qara) |
| Edge Gateway | Lokal qayd, rele boşqaruvi, oflayn bufer | Raspberry Pi / mini-PC, Python servis + SQLite |
| Gate Controller | Şlagbaum relesi + qayta aloqa datçigi | Rele moduli (GPIO yoki Modbus/TCP), limit switch |
| Web admin | Administrator va mas'ul şaxs paneli | React + TypeScript + Vite |
| Desktop (EXE) | Qorovul va admin uçun asosiy klient | Electron yoki Tauri (web ilovani örab oladi) + lokal video pleyer |
| Android (APK) | Tezkor nazorat, bildirişnomalar | Flutter |
| Fayl saqlaş | Snapshot, eksport, biometrik şablon (şifrlangan) | S3-mos (MinIO) yoki lokal disk |

### 2.2 Tarmoq sxemasi

```
IP kameralar (RTSP/ONVIF) ──► Edge Gateway (lokal) ──► Central API ◄──► Web / Desktop / Android
        │                         │    ▲
        └──► VMS (yoziş, arxiv)   │    └── İnternet uzilsa: lokal bufer (SQLite)
                                  └──► Gate Controller (rele + datçik)
```

### 2.3 Tanış (recognition) — "faqat zarur joyda AI" tamoyili

**BR-ARC-01.** AI hamma kamerada YOQILMAYDI. Har bir kamerada `recognition_mode` maydoni bor:
- `off` — faqat yoziş (ichki hudud, perimetr kameralari). Standart qiymat.
- `on_camera` — tanış kameraning / terminalning öz içida (ANPR kamera, yuz tanış terminali). Server faqat natijani (hodisa + rasm) qabul qiladi.
- `local_server` — tanış lokal serverda (Edge yoki on-premise GPU/CPU), internet kerak emas.

**BR-ARC-02.** Bulutli (cloud) AI işlatilmaydi. Barça tanış lokal tarmoqda bajariladi. Oylik AI tölovi yöq.

**BR-ARC-03.** Yuz tanış uçun xodimning 3–5 ta rasmi oldindan röyxatga olinadi (enrollment). Avtomobil uçun ruxsat berilgan raqamlar röyxati oldindan kiritiladi.

**BR-ARC-04.** Recognition adapteri interfeysi:
```
interface FaceProvider   { enroll(employeeId, images[]): TemplateRef; match(frame|event): MatchResult[]; remove(employeeId) }
interface PlateProvider  { read(frame|event): PlateResult[] }
MatchResult = { employeeId|null, confidence 0..100, cameraId, ts, snapshotRef }
PlateResult = { plateText, confidence 0..100, cameraId, ts, snapshotRef, direction: in|out }
```
Amalga oşirişlar: `MockFaceProvider`, `MockPlateProvider` (DEMO), `CameraVendorFaceProvider` (kamera/terminal SDK yoki HTTP push), `LocalFaceProvider` (lokal model), `CameraVendorPlateProvider`, `LocalPlateProvider`. Vendor tanlanmagan — 13-bölimga qara.

**BR-ARC-05.** Kamera soni böyiça dasturiy qattiq limit YÖQ. Real çegaralar server resursi va litsenziya (`license.max_cameras`, `license.max_ai_channels`) orqali boşqariladi; çegaraga yetganda UI tuşunarli xabar beradi.

### 2.4 Oflayn rejim

**BR-OFF-01.** İnternet yoki markaziy server bilan aloqa uzilganda Edge Gateway kiriş-çiqiş hodisalari, şlagbaum buyruqlari va tanış natijalarini lokal SQLite'ga yozadi.
**BR-OFF-02.** Har bir lokal yozuv `local_id (UUID)`, `created_at`, `synced=false` bilan saqlanadi.
**BR-OFF-03.** Aloqa tiklanganda yozuvlar `created_at` tartibida serverga yuboriladi; server `local_id` böyiça idempotent qabul qiladi (takror kelsa — qayta yaratmaydi).
**BR-OFF-04.** UI'da oflayn holat banner bilan körsatiladi: "İnternet uzildi — hodisalar lokal qayd etilmoqda (N ta kutilmoqda)".
**BR-OFF-05.** Oflayn paytda şlagbaum lokal tarmoqdagi qorovul klientidan boşqarila oladi (Edge orqali).
**BR-OFF-06.** Sinxronlaş tugagaç davomat qayta hisoblanadi (4.6 bölim).

---

## 3. ROLLAR VA RUXSATLAR

Tizimda aynan 3 ta asosiy rol bor. Foydalanuvçi bitta rolga ega.

| Modul / amal | Qorovul (`guard`) | Administrator (`admin`) | Mas'ul şaxs (`manager`) |
|---|---|---|---|
| Qorovul paneli, jonli kameralar | Ha | Ha | Yöq |
| Şlagbaum oçiş/yopiş | Ha | Ha | Yöq |
| Şlagbaum fix rejimi | Ha (tasdiq bilan) | Ha | Yöq |
| Video arxiv köriş | Ha | Ha | Yöq |
| Video eksport / snapshot | Ha (audit) | Ha (audit) | Yöq |
| Tezkor qidiruv (FİŞ, raqam, yuz) | Ha | Ha | Yöq |
| Xodimlar CRUD, Excel import/eksport | Yöq | Ha | Yöq |
| Bölim / guruh CRUD | Yöq | Ha | Yöq |
| İş grafiklari CRUD va biriktiriş | Yöq | Ha | Yöq |
| Kameralar, hududlar, xarita sozlamasi | Yöq | Ha | Yöq |
| Face Recognition qidiruvi, tasdiqlaş | Faqat qidiruv | Ha | Yöq |
| ANPR, ruxsat röyxati | Faqat köriş | Ha | Yöq |
| Davomat monitoringi | Yöq | Ha | Faqat köriş |
| Hisobotlar va eksport | Yöq | Ha | Ha |
| Foydalanuvçilar, rollar, sessiyalar | Yöq | Ha | Yöq |
| Tizim sozlamalari | Yöq | Ha | Yöq |
| Audit log | Yöq | Ha | Yöq |

**BR-RBAC-01.** Ruxsat backend'da har bir endpoint'da tekşiriladi (faqat UI'da yaşiriş yetarli EMAS).
**BR-RBAC-02.** Kirişdan keyin foydalanuvçi faqat öz roliga tegişli menyu va ekranlarni köradi.
**BR-RBAC-03.** Mas'ul şaxs tizim konfiguratsiyasi va foydalanuvçilarni boşqara olmaydi.
**BR-RBAC-04.** Ruxsatsiz urinış 403 qaytaradi va audit logga yoziladi.

Menyular (dizayndagi tartibda):
- **Qorovul:** Boş panel, Multi-kamera, Şlagbaum, Video pleyer, Video arxiv.
- **Administrator:** Dashboard, Xodimlar, Bölimlar va guruhlar, İş grafiklari, Kameralar, Kamera hududlari va xaritasi, Şlagbaumlar va kiriş nuqtalari, Face Recognition, ANPR / LPR, Davomat va monitoring, Video arxiv, Hisobotlar, Foydalanuvçilar va rollar, Tizim sozlamalari, Audit log.
- **Mas'ul şaxs:** Boş sahifa, Davomat, Hisobotlar.

---

## 4. MODULLAR VA BİZNES QOİDALARİ

### 4.1 Autentifikatsiya va sessiyalar (AUTH)

Login oynasi maydonlari: Korxona identifikatori, Login, Parol (körsatiş tugmasi bilan), Eslab qoliş, Tizimga kiriş.

- **BR-AUTH-01.** Login va parollarni faqat administrator yaratadi. Öz-özini röyxatdan ötkaziş YÖQ.
- **BR-AUTH-02.** Parollar Argon2id (yoki bcrypt cost ≥ 12) bilan xeşlanadi. Oçiq parol heç qayerda saqlanmaydi va logga yozilmaydi.
- **BR-AUTH-03.** Kirişda `tenant_code + login + password` tekşiriladi. Xato bölsa umumiy xabar: "Login yoki parol notöğri" (qaysi biri xato ekani aytilmaydi).
- **BR-AUTH-04.** Ketma-ket 5 marta xato urinışdan keyin hisob 15 daqiqaga bloklanadi. Har bir xato urinış audit logga yoziladi (IP, qurilma, login).
- **BR-AUTH-05.** Sessiya: qisqa muddatli access token (JWT, 15 daqiqa) + refresh token (httpOnly cookie yoki xavfsiz saqlaş, 7 kun; "Eslab qoliş" öçiq bölsa — 12 soat).
- **BR-AUTH-06.** Ikki bosqiçli autentifikatsiya (TOTP) — sozlamadan yoqiladi; administrator roli uçun tavsiya etiladi. Yoqilgan bölsa, parol tögri bölgandan keyin 6 xonali kod söraladi.
- **BR-AUTH-07.** Faol sessiyalar röyxati (qurilma, IP, boşlangan vaqt). Administrator istalgan sessiyani tugata oladi; tugatilgan sessiya tokeni darhol yaroqsiz böladi.
- **BR-AUTH-08.** Kirişdan keyin rol aniqlanadi va tegişli boş ekran oçiladi: guard → Qorovul paneli, admin → Admin dashboard, manager → Mas'ul dashboard.
- **BR-AUTH-09.** Çiqiş (logout) refresh tokenni bekor qiladi va audit logga yoziladi.
- **BR-AUTH-10.** Foydalanuvçi öz hisobini öçira olmaydi. Oxirgi faol administratorni öçiriş yoki bloklaş taqiqlanadi.

### 4.2 Xodimlar (EMP)

Maydonlar: FİŞ, profil rasmi, biometrik xavfsiz identifikator, ID raqam (tabel), lavozim, bölim, iş guruhi, telefon, işga qabul sanasi, individual grafik (ixtiyoriy), faollik holati (faol / nofaol / ta'tilda), avtomobil raqamlari (bir neçta).

- **BR-EMP-01.** ID raqam (tabel raqami) korxona içida unikal.
- **BR-EMP-02.** Xodim albatta bitta guruhga tegişli; guruh orqali bölimga boğlanadi.
- **BR-EMP-03.** Xodimga individual grafik biriktirilmagan bölsa — guruh grafigi amal qiladi (4.4 ierarxiya).
- **BR-EMP-04.** Nofaol xodim tanılganda kirişi "Nofaol xodim" holati bilan qayd etiladi va qorovulga ogohlantiriş çiqadi; davomatga hisoblanmaydi.
- **BR-EMP-05.** Yuz röyxatga oliş: 3–5 ta rasm (old, çap, ong, turli yoritiş). Har bir rasm sifati tekşiriladi (yuz topildimi, ölçam, xiralik); sifatsiz rasm rad etiladi va sababi körsatiladi.
- **BR-EMP-06.** Biometrik rasmlarni yuklaşdan oldin xodim roziligi belgisi (`biometric_consent=true`, sana, kim tasdiqlagan) majburiy. Rozilik yöq bölsa — enrollment taqiqlanadi.
- **BR-EMP-07.** Excel import: şablon yuklab oliş → tölduriş → yuklaş → oldindan köriş (har bir qator: OK / xato sababi) → tasdiqlaş. Xato qatorlar import qilinmaydi, alohida röyxat qaytariladi. Takror ID raqam — xato.
- **BR-EMP-08.** Excel eksport: joriy filtr (qidiruv, bölim) böyiça.
- **BR-EMP-09.** Xodimni öçiriş — "soft delete" (`deleted_at`). Tarixiy davomat, hodisalar va hisobotlar saqlanadi. Biometrik şablon DARHOL qaytarib bölmaydigan qilib öçiriladi (BR-PRIV-04).
- **BR-EMP-10.** Oxirgi qolgan xodimni öçiriş blok emas (dizayndagi demo çeklovi faqat DEMO uçun).
- **BR-EMP-11.** Xodim profili körsatadi: asosiy ma'lumot, biometrik rasmlar soni va sifati, xavfsiz identifikator (maskalangan), biriktirilgan grafik (darajasi bilan: Umumiy / Guruh / İndividual), bugungi reja va holat, oylik davomat foizi, keçikiş/erta ketiş/kelmagan kunlar soni, oxirgi aniqlanişlar.
- **BR-EMP-12.** Har bir yaratiş, tahrirlaş, öçiriş audit logga yoziladi (eski va yangi qiymatlar bilan, biometrik ma'lumotlarsiz).

### 4.3 Bölimlar va guruhlar (ORG)

- **BR-ORG-01.** Tuzilma: Bölim (department) → Guruh (group) → Xodim.
- **BR-ORG-02.** Har bir guruhga bitta grafik biriktiriladi (majburiy).
- **BR-ORG-03.** Guruh grafigi özgarsa, şu guruhning individual grafigi yöq barça xodimlari avtomatik yangi grafikni oladi (keyingi kundan boşlab yoki tanlangan sanadan — `effective_from`).
- **BR-ORG-04.** İçida xodim bor guruhni öçiriş TAQİQLANADI ("Avval xodimlarni boşqa guruhga ötkazing"). İçida guruh bor bölimni öçiriş ham taqiqlanadi.
- **BR-ORG-05.** Nomlar bölim içida unikal.

### 4.4 İş grafiklari (SCH) — ENG MUHİM MODUL

#### 4.4.1 Ierarxiya

- **BR-SCH-01.** Uç daraja: **Umumiy grafik** → **Guruh / bölim grafigi** → **Alohida xodim grafigi (istisno)**.
- **BR-SCH-02.** Xodim uçun amaldagi grafik (effective schedule) aniqlaş tartibi, berilgan sana `D` uçun:
  1. `D` sanaga tögri keladigan vaqtinçalik individual istisno (`employee_schedule_override` with `date_from ≤ D ≤ date_to`);
  2. doimiy individual grafik (`date_to IS NULL`);
  3. xodim guruhining grafigi (`group_schedule_assignment`, `effective_from ≤ D`);
  4. topilmasa — xato holati "Grafik biriktirilmagan" (davomat hisoblanmaydi, ogohlantiriş).
- **BR-SCH-03.** Hisob-kitobda HAR DOİM xodimning öziga tegişli eng ustuvor grafik işlatiladi.
- **BR-SCH-04.** Grafik özgarişi ötgan kunlarning hisobotini özgartirmaydi (tarixiy biriktirişlar saqlanadi, `effective_from` bilan versiyalanadi). Agar administrator ötgan sanani tanlab özgartirsa — şu davr davomati qayta hisoblanadi va audit logga yoziladi.

#### 4.4.2 Grafik turlari

Har bir grafikda umumiy maydonlar: nomi, turi, darajasi, tuşlik/tanaffus (daqiqa yoki vaqt oralığı), grace period (ixtiyoriy, aks holda global), izoh.

**A. Haftalik (`weekly`)**
- **BR-SCH-05.** Haftaning har bir kuni uçun: işlaydi/işlamaydi, boşlaniş, tugaş, tanaffus(lar). Tezkor shablonlar: "Duşanba–Juma", "Duşanba–Şanba".
- **BR-SCH-06.** Har bir kunning vaqti turliça bölişi mumkin.

**B. Smenali (`shift`)**
- **BR-SCH-07.** Smenalar röyxati (masalan: 1-smena 06:00–14:00, 2-smena 14:00–22:00, 3-smena 22:00–06:00). Smenalar soni va vaqtlari sozlanadi.
- **BR-SCH-08.** Aylaniş qoidasi: davr (kunlik / haftalik / N kunlik), boşlanğiç sana, boşlanğiç smena. Dam oliş kunlari belgilanadi.
- **BR-SCH-09. Tungi smena:** tugaş vaqti boşlaniş vaqtidan kiçik yoki teng bölsa, smena KEYİNGİ KUNDA tugaydi. Smena boşlangan sanaga tegişli hisoblanadi (bir sana, bir yozuv). Davomiylik = (tugaş + 24 soat) − boşlaniş.

**C. Kunora (`alternate`)**
- **BR-SCH-10.** 1 kun iş, 1 kun dam. Boşlanğiç sana (iş kuni) va iş vaqti. Sana `D` iş kuni ⇔ `(D − start_date) mod 2 = 0`.

**D. Uç kunda bir (`every_n`, n=3)**
- **BR-SCH-11.** Har 3 kunda 1 kun iş. Sana `D` iş kuni ⇔ `(D − start_date) mod 3 = 0`. Kod umumiy `every_n` sifatida yoziladi (n sozlanadi), UI'da "Uç kunda bir" preset.

**E. İndividual (`individual`)**
- **BR-SCH-12.** Alohida xodim uçun haftaning har bir kunida turli soat. Turi: doimiy yoki vaqtinçalik (`date_from`, `date_to`). Vaqtinçalik muddat tugagaç avtomatik guruh grafigiga qaytadi.
- **BR-SCH-13.** Bir xodimda bir xil sanaga tögri keladigan ikkita individual istisno bölişi mumkin emas (kesişuv validatsiyasi).

#### 4.4.3 Validatsiya

- **BR-SCH-14.** Boşlaniş = tugaş bölsa xato ("İş vaqti 0 soat").
- **BR-SCH-15.** Tanaffus iş vaqtidan uzun bölişi mumkin emas.
- **BR-SCH-16.** Guruh yoki xodimga biriktirilgan grafikni öçiriş TAQİQLANADI ("Avval boşqa grafik biriktiring"). Arxivlaş (nofaol qiliş) mumkin.
- **BR-SCH-17.** Grafik saqlanişdan oldin 14 kunlik oldindan köriş (preview) körsatiladi.

#### 4.4.4 Köriniş

- **BR-SCH-18.** Grafik kalendar körinişida (oy, iş kuni / dam / tungi smena ranglar bilan, bugun belgilangan) va jadval körinişida (sana, kun, iş vaqti, sof soat, izoh) körsatiladi.
- **BR-SCH-19.** Grafiklar röyxatida har biri uçun: turi, darajasi, neçta guruh va neçta xodimga amal qilişi.

### 4.5 Davomat hisoblaş (ATT)

#### 4.5.1 Kiriş va çiqişni aniqlaş

- **BR-ATT-01.** Manba hodisalar: kiriş nuqtasidagi yuz tanış (`direction=in`), çiqiş nuqtasidagi yuz tanış (`direction=out`), xodimga boğlangan avtomobilning ANPR hodisasi (sozlamaga qarab), qölda kiritilgan tuzatiş (admin, sabab bilan).
- **BR-ATT-02.** Faqat işonçlilik çegarasidan yuqori (yoki operator tasdiqlagan) yuz hodisalari davomatga kiradi (BR-FACE-04).
- **BR-ATT-03.** "İş kuni oynasi": reja boşlanişidan 4 soat oldin → reja tugaşidan 6 soat keyin (sozlanadi). Şu oynadagi birinçi kiriş = keliş, oxirgi çiqiş = ketiş.
- **BR-ATT-04.** Tungi smenada oyna keyingi kalendar kunga ötadi (BR-SCH-09). Masalan 22:00–06:00 smena uçun 18:00 dan ertasi 12:00 gaça.
- **BR-ATT-05.** 60 soniya içidagi takroriy bir xil hodisalar (bitta kamera, bitta xodim) bitta deb hisoblanadi (debounce).

#### 4.5.2 Holatlar

Kiriş holati (`arrival_status`):
- **BR-ATT-06.** `on_time` — keliş ≤ reja boşlanişi + grace period.
- **BR-ATT-07.** `late` — keliş > reja boşlanişi + grace. Keçikiş daqiqasi = keliş − reja boşlanişi (grace qöşilmaydi).
- **BR-ATT-08.** `late_excused` — keçikiş bor, lekin sabab qayd etilgan (admin yoki mas'ul tomonidan, izoh majburiy).
- **BR-ATT-09.** `absent` — reja böyiça iş kuni, oyna tugadi (yoki hozir > reja boşlanişi + grace va kun tugagan), keliş yöq. Bugungi kun uçun reja boşlanmagan bölsa — `pending` ("Hali boşlanmagan").
- **BR-ATT-10.** `day_off` — grafik böyiça dam oliş kuni. Dam oliş kuni kelsa — `day_off_worked` (qöşimça iş sifatida).

Çiqiş holati (`departure_status`):
- **BR-ATT-11.** `left_on_time` — ketiş ≥ reja tugaşi va ≤ reja tugaşi + 30 daqiqa.
- **BR-ATT-12.** `left_early` — ketiş < reja tugaşi. Erta ketiş daqiqasi qayd etiladi.
- **BR-ATT-13.** `overtime` — ketiş > reja tugaşi + 30 daqiqa (çegara sozlanadi). Qöşimça vaqt = ketiş − reja tugaşi.
- **BR-ATT-14.** `on_site` — bugun, kelgan, hali çiqmagan ("Korxonada").
- **BR-ATT-15.** `no_exit` — ötgan kun, kelgan, çiqiş qayd etilmagan ("Çiqiş qayd etilmagan") — admin tuzatişi uçun ogohlantiriş.

#### 4.5.3 Grace period va qayta hisoblaş

- **BR-ATT-16.** Grace period global sozlamada (standart 10 daqiqa), grafik darajasida qayta belgilaş mumkin. Grafikdagi qiymat ustuvor.
- **BR-ATT-17.** Grace period, grafik yoki biriktiriş özgarsa — tegişli davr davomati qayta hisoblanadi (fon vazifasi), natija UI'da darhol yangilanadi.
- **BR-ATT-18.** Qölda tuzatiş (keliş/ketiş vaqtini özgartiriş, sabab qöşiş) faqat admin, sabab majburiy, asl qiymat saqlanadi, audit logga yoziladi.
- **BR-ATT-19.** Davomat foizi (davr uçun) = (reja böyiça iş kunlari − kelmagan kunlar) / reja böyiça iş kunlari × 100, butun songa yaxlitlanadi. Reja kuni 0 bölsa — "—".
- **BR-ATT-20.** Sof iş soati = (ketiş − keliş) − tanaffus; reja soati = (reja tugaşi − reja boşlanişi) − tanaffus.

#### 4.5.4 Davomat monitoringi ekrani

- Sana tanlaş (bugun, keça, istalgan sana), bölim filtri, FİŞ qidiruv.
- Filtr çiplari soni bilan: Hammasi, Öz vaqtida, Keçikkan, Sababli keçikiş, Kelmadi, Erta ketdi, Qöşimça iş.
- Jadval: Xodim, Bölim, Grafik, Reja, Kiriş, Çiqiş, Kiriş holati, Çiqiş holati.
- Mas'ul şaxs faqat köradi; tuzatiş tugmalari faqat admin uçun.

### 4.6 Kameralar (CAM)

Kamera qöşiş maydonlari: nomi, IP manzil, RTSP URL (asosiy oqim + sub-stream), ONVIF orqali aniqlaş, login, parol, kamera turi (statsionar / PTZ / panoramik), hudud, nuqta turi (kiriş nuqtasi / çiqiş nuqtasi / içki hudud), AI funksiyalari, tanış joyi (`recognition_mode`), yoziş rejimi, saqlaş muddati.

AI funksiyalari: Face Recognition, Face Detection, ANPR / LPR, Motion Detection, Person Detection.

- **BR-CAM-01.** Kamera paroli şifrlangan holda saqlanadi (AES-256-GCM, kalit — muhit özgaruvçisi yoki KMS). API javobida parol HEÇ QAÇON qaytarilmaydi.
- **BR-CAM-02.** ONVIF qidiruv: lokal tarmoqda WS-Discovery orqali qurilmalar topiladi, röyxatdan tanlanganda IP va RTSP avtomatik töldiriladi.
- **BR-CAM-03.** Saqlaşdan oldin "Ulaniş testi": RTSP oqimidan 1 kadr olinadi; muvaffaqiyatsiz bölsa sababi körsatiladi (autentifikatsiya, tarmoq, kodek).
- **BR-CAM-04.** Yangi kamera saqlanganda multi-kamera monitoringda avtomatik yangi video blok paydo böladi (alohida sozlaşsiz).
- **BR-CAM-05.** `Face Recognition` yoki `ANPR` tanlansa, `recognition_mode` `off` bölişi mumkin emas (validatsiya). `recognition_mode=off` — faqat yoziş va Motion/Person detection (agar kamera özi qöllasa).
- **BR-CAM-06.** Kiriş/çiqiş nuqtasi kameralari şlagbaum yoki eşik (access point) bilan boğlanadi; davomat yönalişi (in/out) şu boğlanişdan olinadi.
- **BR-CAM-07.** Monitoring (har 30 soniya): onlayn/oflayn, bitreyt, paket yöqotiş, FPS, yoziş holati. Kamera 60 soniyadan ortiq javob bermasa — "Aloqa uzilgan" hodisasi va qorovulga bildiriş.
- **BR-CAM-08.** Kameralar hududlar böyiça guruhlanadi (Darvoza, Kiriş, Işlab çiqariş, Ombor, Hovli, Perimetr, Binolar — röyxat sozlanadi).
- **BR-CAM-09.** Kamera öçirilganda arxiv yozuvlari saqlaş muddati tugaguncha qoladi; oxirgi kamera ham öçirilişi mumkin.

### 4.7 Video, multi-kamera va pleyer (VMS)

- **BR-VMS-01.** Grid layout: 1×1, 2×2, 3×3, 4×4, Custom (1 katta + 5 kiçik va foydalanuvçi saqlagan sxemalar). Tanlangan layout foydalanuvçi uçun saqlanadi.
- **BR-VMS-02.** Drag & Drop: kamera röyxatidan yoki boşqa katakdan sudrab joylaştiriş.
- **BR-VMS-03.** Oqim boşqaruvi (interfeys qotmasligi uçun): faqat körinib turgan kataklar oqim oladi; katta (fokus/fullscreen) katak — asosiy oqim (main stream), qolganlari — sub-stream; körinmaydigan kameralar oqimi töxtatiladi. Bir klientda bir vaqtda asosiy oqimlar soni çeklangan (sozlanadi, standart 4).
- **BR-VMS-04.** Har bir video blokda: kamera nomi, joylaşuvi, LIVE belgisi, yoziş belgisi (REC), aloqa holati, sana-vaqt, sifat (HD/FHD), AI belgisi va (bölsa) aniqlaş ramkasi.
- **BR-VMS-05.** Pleyer funksiyalari: Play/Pause, Stop, Live, timeline, arxivni köriş, ±5 daqiqa (va ±10 soniya), tezlik 0.25×, 0.5×, 1×, 2×, 4×, 8×, fullscreen, snapshot, video qismini saqlaş/eksport, ovoz (kamera qöllasa), alohida oynada oçiş, sinxron köriş (bir neçta kamera bir xil vaqtda), sana-vaqt böyiça qidiruv, hodisa böyiça topiş.
- **BR-VMS-06.** Timeline'da yozilgan qismlar (yaşil), yozuv yöq qismlar (böş) va hodisa belgilari (rangli) körsatiladi. Hodisa belgisini bosiş videoni şu vaqtga olib boradi.
- **BR-VMS-07.** Arxiv: kamera, sana, vaqt oralığı, hodisa turi (xodim, avtomobil, noma'lum şaxs, şlagbaum, aloqa) böyiça filtr; natija kartalar körinişida (rasm, vaqt, davomiylik, kamera, tur).
- **BR-VMS-08.** Yoziş: segmentli fayllar (masalan 10 daqiqalik), rejimlar: doimiy / hodisa böyiça (hodisadan 10 s oldin va 20 s keyin) / jadval böyiça. Saqlaş muddati tugagan segmentlar avtomatik öçiriladi; disk 90% dan oşsa eng eski yozuvlar öçiriladi va ogohlantiriş beriladi.
- **BR-VMS-09.** Har bir arxiv köriş, snapshot va eksport audit logga yoziladi (kim, qaysi kamera, qaysi vaqt oralığı) — BR-AUD-02.
- **BR-VMS-10.** Eksport faylida suv belgisi (watermark): kim, qaçon eksport qilgan.

### 4.8 Şlagbaum boşqaruvi (GATE)

#### 4.8.1 Holatlar maşinasi

Holatlar: `closed`, `opening`, `open`, `closing`, `fault`, `unknown`. Rejim: `normal`, `fixed_open`, `fixed_closed`.

```
closed --OPEN cmd--> opening --sensor=open--> open
open   --CLOSE cmd--> closing --sensor=closed--> closed
opening/closing --timeout(3s, sozlanadi)--> fault
any --sensor yöq--> unknown
```

- **BR-GATE-01.** Qorovul panelida ikkita katta kvadrat jigarrang (#795548) tugma: OÇİŞ va YOPİŞ, mos ikonkalar bilan.
- **BR-GATE-02.** Buyruq yuborilganda UI holati: "Yuborildi — rele javobi kutilmoqda" (tugmalar vaqtinça bloklanadi).
- **BR-GATE-03.** Buyruq FAQAT qayta aloqa datçigi (limit switch) yangi holatni tasdiqlaganda "Bajarildi" deb körsatiladi. Datçik timeout içida tasdiqlamasa — "Bajarilmadi" (qizil), sababi bilan, holat `fault`, qorovulga ogohlantiriş.
- **BR-GATE-04.** Har bir buyruq (kim, qaçon, qaysi qurilmadan, natija, javob vaqti) audit logga va şlagbaum jurnaliga yoziladi.
- **BR-GATE-05. Fix rejimi:** "Oçiq holatda fix qiliş" va "Yopiq holatda fix qiliş" — oddiy buyruqlardan alohida tugmalar. Yoqişdan oldin tasdiqlaş oynasi majburiy. Fix faol bölganda: doimiy sariq indikator (barça ekranlarda yuqori panelda), oddiy OÇİŞ/YOPİŞ buyruqlari rad etiladi va sababi körsatiladi. Fixni bekor qiliş ham tasdiq bilan.
- **BR-GATE-06.** Watchdog: kontroller har 2 soniyada heartbeat yuboradi; 10 soniya kelmasa — holat `unknown`, ogohlantiriş.
- **BR-GATE-07.** Avariya: elektr yoki aloqa uzilganda şlagbaum qölda boşqariladi (mexanik kalit). Dasturiy tomonda "Avariya qölda boşqaruv" holati körsatiladi.
- **BR-GATE-08.** Avtomatik oçiş (sozlamada yoqilsa): ruxsat röyxatidagi avtomobil ANPR'da yoki ruxsatli xodim yuz tanışda aniqlansa, kiriş nuqtasidagi şlagbaum avtomatik oçiladi va N soniyadan keyin (yoki datçik avtomobil ötganini aniqlagaç) yopiladi. Fix rejimida avtomatik buyruqlar ham işlamaydi.
- **BR-GATE-09.** Mobil ilovadan buyruq faqat tasdiqlaş oynasidan keyin yuboriladi.
- **BR-GATE-10.** Rele boşqaruvi adapteri: `GateDriver { open(); close(); readSensor(): open|closed|between; heartbeat() }`, amalga oşirişlar: `MockGateDriver`, `GpioGateDriver`, `ModbusGateDriver`.

### 4.9 Yuz tanış va xodimni yuz orqali qidiriş (FACE)

- **BR-FACE-01.** Qidiruv FİŞ böyiça yoki yuklangan rasm böyiça (rasm faqat qidiruv uçun işlatiladi, saqlanmaydi).
- **BR-FACE-02.** Natija: xodim rasmi, FİŞ, oxirgi aniqlangan vaqt, kamera, hudud, işonçlilik (%), oldingi aniqlanişlar tarixi (vaqt ketma-ketligida).
- **BR-FACE-03.** İşonçlilik çegarasi sozlanadi (standart 85%).
- **BR-FACE-04.** Natija çegaradan PAST bölsa — "Tasdiqlanmagan / Tekşiruv kerak" deb körsatiladi, tasdiqlangan şaxs sifatida körsatilMAYDI va davomatga avtomatik kirmaydi. Operator "Tasdiqlaş" yoki "Rad etiş" qiladi; qaror (kim, qaçon) saqlanadi va audit logga yoziladi. Tasdiqlansa — davomat qayta hisoblanadi.
- **BR-FACE-05.** Bazada yöq yuz — "Noma'lum şaxs aniqlandi (tasdiqlanmagan)". Noma'lum şaxs heç qaçon avtomatik ravişda "jinoyatçi", "xavfli" yoki "qora röyxat" deb belgilanmaydi.
- **BR-FACE-06.** Noma'lum şaxs hodisasi rasmi saqlaş muddati çeklangan (standart 30 kun), keyin avtomatik öçiriladi.

### 4.10 Korxona xaritasi (MAP)

- **BR-MAP-01.** Administrator korxona rejasini (rasm yoki sxema) yuklaydi, hududlarni (poligon/törtburçak) va kameralarni (nuqta) xaritaga joylaştiradi.
- **BR-MAP-02.** Kamera belgisi rangi holatni körsatadi (onlayn — yaşil, oflayn — qizil); bosilganda kamera ma'lumoti.
- **BR-MAP-03.** Xodim tanlanganda uning OXİRGİ aniqlangan kamerasi joylaşgan HUDUD belgilanadi. Matnda albatta: "Aniq koordinata emas — kamera qayd etgan oxirgi hudud".
- **BR-MAP-04.** Bir neçta kameradan olingan aniqlanişlar vaqt ketma-ketligida röyxat qilib körsatiladi.

### 4.11 Avtomobillarni aniqlaş (ANPR)

- **BR-ANPR-01.** Har bir hodisa: davlat raqami, işonçlilik, avtomobil rasmi (+ raqam qismi kesib olingan rasm), vaqt, kamera, yönaliş (kirdi/çiqdi).
- **BR-ANPR-02.** Raqam normallaştiriladi (böşliqlarsiz, katta harf) va özbek raqam formatlariga moslik tekşiriladi; mos kelmasa ham saqlanadi, lekin "format noma'lum" belgisi bilan.
- **BR-ANPR-03.** Ruxsat röyxati (allowlist): raqam, egasi (xodimga boğlaş ixtiyoriy), turi (xodim / kompaniya / mehmon / yetkazib beruvçi), amal qiliş muddati (doimiy yoki sana-vaqtgaça).
- **BR-ANPR-04.** Röyxatda yöq raqam — "Röyxatda yöq" (sariq), xavfli deb belgilanmaydi. Operator: "Tekşirildi", "Ruxsat röyxatiga qöşiş", "Vaqtinçalik ruxsat".
- **BR-ANPR-05.** Muddati tugagan ruxsat — "Ruxsat muddati tugagan" holati.
- **BR-ANPR-06.** Avtomobil ruxsatli va xodimga boğlangan bölsa, sozlamaga qarab xodim davomatiga kiriş hodisasi sifatida qöşilişi mumkin (`anpr_counts_for_attendance`, standart: öçiq).
- **BR-ANPR-07.** Statistika: kirgan, çiqqan, röyxatda yöq, vaqtinçalik ruxsat; soatlar böyiça grafik.

### 4.12 Hodisalar va bildirişnomalar (EVT)

Hodisa turlari: `employee_in`, `employee_out`, `unknown_face`, `vehicle_in`, `vehicle_out`, `vehicle_unknown`, `gate_opened`, `gate_closed`, `gate_fault`, `gate_fix_on`, `gate_fix_off`, `camera_offline`, `camera_online`, `face_low_confidence`, `sync_pending`, `sync_done`.

- **BR-EVT-01.** Har bir hodisada: vaqt, kamera/qurilma, tur, tegişli rasm (snapshot) va boğliq obyekt (xodim, avtomobil).
- **BR-EVT-02.** Hodisalar real vaqtda (WebSocket) qorovul paneli "Tezkor hodisalar" blokiga va mobil ilovaga keladi; eng yangisi tepada.
- **BR-EVT-03.** Muhimlik darajasi: info / ogohlantiriş / xato. Push bildirişnoma faqat foydalanuvçi yoqgan turlar uçun (mobil profil sozlamalari).
- **BR-EVT-04.** Hodisadan bir bosişda tegişli video lahzasiga ötiş mumkin (pleyer şu vaqtni oçadi).

### 4.13 Qorovul tezkor qidiruvi (SEARCH)

- **BR-SRCH-01.** Bitta qidiruv maydoni: FİŞ yoki avtomobil raqami (böşliqlarga qaramay); "Yuz tasviri böyiça" alohida tugma.
- **BR-SRCH-02.** Natijada: xodim/avtomobil ma'lumoti va oxirgi aniqlangan holat (vaqt, kamera), hozirgi holat (Korxonada / Çiqib ketgan / Aniqlanmagan).

### 4.14 Hisobotlar (REP)

Hisobot turlari (aynan 10 ta):
1. Kunlik davomat. 2. Haftalik davomat. 3. Oylik davomat. 4. Xodimlar kesimida. 5. Bölimlar kesimida. 6. Grafik böyiça bajariliş. 7. Keçikişlar hisoboti. 8. Erta ketişlar hisoboti. 9. Avtomobillar hisoboti. 10. Xavfsizlik hodisalari hisoboti.

- **BR-REP-01.** Filtrlar: davr (sana oralığı), bölim, guruh, xodim.
- **BR-REP-02.** Eksport: PDF va Excel (.xlsx). Katta hisobotlar fon vazifasida tayyorlanadi, UI'da progress (%) va tayyor bölganda yuklab oliş havolasi.
- **BR-REP-03.** Xodim tanlanganda: iş grafigi, haqiqiy kiriş va çiqiş vaqtlari (kunlar böyiça), keçikişlar soni, kelmagan kunlar, oylik davomat foizi, grafik bilan taqqoslangan statistika (reja soat / haqiqiy soat).
- **BR-REP-04.** Grafik böyiça bajariliş: har bir grafik uçun xodimlar soni, reja soat, haqiqiy soat, bajariliş %.
- **BR-REP-05.** Keçikişlar: soni, sababli/sababsiz, umumiy daqiqa. Erta ketişlar: soni, umumiy daqiqa.
- **BR-REP-06.** Barça hisob-kitoblar 4.5 qoidalariga tayanadi (alohida hisoblaş logikasi YOZİLMAYDİ — bitta servis).
- **BR-REP-07.** Har bir eksport audit logga yoziladi.
- **BR-REP-08.** Mas'ul şaxs hisobotlarni köradi va eksport qiladi, lekin sozlamalarni özgartira olmaydi.

### 4.15 Foydalanuvçilar va rollar (USR)

- **BR-USR-01.** Maydonlar: FİŞ, login (unikal), rol, holat (faol/bloklangan), 2FA holati, oxirgi kiriş.
- **BR-USR-02.** Boşlanğiç parol administrator tomonidan beriladi yoki generatsiya qilinadi (kamida 10 belgi); birinçi kirişda parolni almaştiriş majburiy.
- **BR-USR-03.** Rollar va ruxsatlar matritsasi (3-bölim) ekranda körsatiladi.
- **BR-USR-04.** Foydalanuvçini öçiriş — soft delete; audit loglarda FİŞ saqlanadi.

### 4.16 Audit log (AUD)

- **BR-AUD-01.** Yoziladigan amallar: kiriş/çiqiş, ruxsatsiz urinışlar, şlagbaum buyruqlari va fix, video köriş/snapshot/eksport, hisobot eksporti, xodim/guruh/grafik/kamera/foydalanuvçi yaratiş-tahrirlaş-öçiriş, sozlamalar özgarişi, yuz tanış qarorlari, davomat tuzatişlari, sessiya tugatiş.
- **BR-AUD-02.** Yozuv: vaqt (UTC + lokal), foydalanuvçi, rol, IP, qurilma, toifa, amal, obyekt, eski/yangi qiymat (maxfiy maydonlarsiz), natija.
- **BR-AUD-03.** Audit log faqat qöşiladi (append-only): UI va API orqali tahrirlaş yoki öçiriş YÖQ. Saqlaş muddati kamida 1 yil.
- **BR-AUD-04.** Filtrlar: toifa (Şlagbaum, Video, Kiriş urinışlari, Sozlamalar, Hisobotlar), foydalanuvçi, sana.

### 4.17 Tizim sozlamalari (SET)

Majburiy sozlamalar va standart qiymatlar:

| Kalit | Standart | Izoh |
|---|---|---|
| `attendance.grace_minutes` | 10 | 0–60 |
| `attendance.overtime_threshold_minutes` | 30 | |
| `attendance.window_before_hours` | 4 | |
| `attendance.window_after_hours` | 6 | |
| `face.confidence_threshold` | 85 | 60–99 |
| `face.operator_confirm_required` | true | |
| `face.unknown_retention_days` | 30 | |
| `anpr.confidence_threshold` | 90 | |
| `anpr.counts_for_attendance` | false | |
| `gate.command_timeout_seconds` | 3 | |
| `gate.auto_open_enabled` | false | |
| `video.retention_days` | 30 | 14 / 30 / 60 / 90 |
| `video.max_main_streams_per_client` | 4 | |
| `security.require_2fa_for_admin` | true | |
| `security.lockout_attempts` | 5 | |

- **BR-SET-01.** Har bir özgariş audit logga yoziladi; davomatga ta'sir qiluvçi özgariş qayta hisoblaşni işga tuşiradi.
- **BR-SET-02.** Sozlamalar ekranida arxitektura holati körsatiladi: IP kameralar, Edge Gateway (onlayn / lokal rejim), lokal xotira, markaziy server (sinxronlangan / aloqa yöq), klientlar; kutilayotgan sinxronlaş yozuvlari soni va "Qölda sinxronlaş" tugmasi.
- **BR-SET-03.** Şlagbaum kontrolleri holati: rele, qayta aloqa datçigi, watchdog, avariya qölda boşqaruv.

### 4.18 Maxfiylik va xavfsizlik (PRIV)

- **BR-PRIV-01.** Biometrik şablonlar alohida jadval/omborda, AES-256-GCM bilan şifrlangan; asosiy jadvalda faqat xavfsiz identifikator (`template_ref`), oçiq ma'lumot emas.
- **BR-PRIV-02.** Biometrik ma'lumotni faqat admin roli boşqaradi; har bir murojaat audit logga yoziladi.
- **BR-PRIV-03.** Biometrik ma'lumotlarni qayta işlaş uçun xodim roziligi va huquqiy asos qayd etiladi (BR-EMP-06). Korxona tegişli qonunçilik talablarini bajariši kerakligi README'da körsatiladi.
- **BR-PRIV-04.** Xodim öçirilganda yoki rozilikni qaytarib olganda uning biometrik şablonlari va rasmlari qaytarib bölmaydigan qilib öçiriladi.
- **BR-PRIV-05.** HTTPS/WSS majburiy (lokal tarmoqda ham self-signed yoki içki CA).
- **BR-PRIV-06.** Kamera va rele parollari, JWT kalitlari faqat muhit özgaruvçilarida / secrets'da; repozitoriyga yozilmaydi.

### 4.19 Mas'ul şaxs paneli (MGR)

- **BR-MGR-01.** Boş sahifa: bugungi davomat, korxonada mavjud xodimlar, keçikkanlar, kelmaganlar, erta ketganlar, kirib-çiqqan avtomobillar, muhim xavfsizlik hodisalari.
- **BR-MGR-02.** Hisobotlar oynasi — 4.14 (faqat köriş va eksport).

### 4.20 Mobil ilova — Android (MOB)

Ekranlar: Login, Boş sahifa, Hodisalar, Kameralar, Jonli kamera/arxiv, Şlagbaum, Davomat, Qidiruv (xodim/avtomobil), Xodimning oxirgi joyi, Hisobotlar, Profil va sozlamalar.

- **BR-MOB-01.** Pastki navigatsiya: Boş, Hodisalar, Kamera, Şlagbaum, Hisobot. Qolgan ekranlar boş sahifadagi tezkor tugmalar yoki yuqori panel orqali.
- **BR-MOB-02.** Mobil qurilmada faqat tanlangan kamera asosiy oqimda, röyxat — past sifatli oqim yoki snapshot (trafik tejaş).
- **BR-MOB-03.** Şlagbaum buyruqlari faqat tasdiqlaş oynasidan keyin (BR-GATE-09); fix rejimi ham tasdiq bilan.
- **BR-MOB-04.** Push bildirişnomalar (FCM): foydalanuvçi tanlagan turlar (şlagbaum hodisalari, noma'lum şaxs/avtomobil, kamera aloqasi uzilişi).
- **BR-MOB-05.** Ruxsatlar desktop/web bilan bir xil (backend tekşiradi). Mobil ilova rolga qarab menyuni körsatadi.
- **BR-MOB-06.** Sessiya xavfsiz saqlanadi (Android Keystore); 2FA qöllab-quvvatlanadi.

---

## 5. HOLATLAR VA RANG KODLARİ (UI va API uçun yagona)

| Kod | UI matni | Rang (badge) |
|---|---|---|
| `on_time` | Öz vaqtida | yaşil (success) |
| `late` | Keçikdi +N daq | sariq (warning) |
| `late_excused` | Sababli keçikiş +N daq | kök (info) |
| `absent` | Kelmadi | qizil (danger) |
| `pending` | Hali boşlanmagan | kulrang (neutral) |
| `day_off` | Dam oliş | kulrang |
| `left_on_time` | Öz vaqtida ketdi | yaşil |
| `left_early` | Erta ketdi −N daq | sariq |
| `overtime` | Qöşimça iş +N daq | kök |
| `on_site` | Korxonada | kök |
| `no_exit` | Çiqiş qayd etilmagan | sariq |
| `face_unconfirmed` | Tekşiruv kerak | sariq |
| `plate_unknown` | Röyxatda yöq | sariq |

Rang heç qaçon yagona belgi emas — har doim matn bilan birga.

---

## 6. MA'LUMOTLAR MODELİ (minimal majburiy jadvallar)

Barça jadvallarda: `id (uuid)`, `tenant_id`, `created_at`, `updated_at`; öçiriladiganlarda `deleted_at`.

| Jadval | Asosiy maydonlar |
|---|---|
| `tenants` | code (unikal), name, timezone (Asia/Tashkent) |
| `users` | login (unikal/tenant), full_name, role (guard/admin/manager), password_hash, status, totp_secret_enc, must_change_password, last_login_at, failed_attempts, locked_until |
| `sessions` | user_id, refresh_token_hash, device, ip, created_at, revoked_at |
| `departments` | name |
| `groups` | department_id, name |
| `employees` | personnel_no (unikal/tenant), full_name, position, group_id, phone, hired_at, status (active/inactive/on_leave), photo_ref, biometric_consent, consent_at, consent_by |
| `face_templates` | employee_id, template_enc (bytea), image_ref_enc, quality_score, provider, created_by — ALOHIDA sxema/jadval, cheklangan ruxsat |
| `schedules` | name, type (weekly/shift/alternate/every_n/individual), level (general/group/individual), break_minutes, grace_minutes (nullable), params (jsonb), is_active |
| `schedule_days` | schedule_id, weekday (0–6), is_working, start_time, end_time |
| `schedule_shifts` | schedule_id, order_no, name, start_time, end_time |
| `group_schedule_assignments` | group_id, schedule_id, effective_from |
| `employee_schedule_overrides` | employee_id, schedule_id, kind (permanent/temporary), date_from, date_to (nullable), reason |
| `attendance_days` | employee_id, work_date, schedule_id, plan_start, plan_end, crosses_midnight, arrival_at, departure_at, arrival_status, departure_status, late_minutes, early_minutes, overtime_minutes, net_minutes, excused_reason, manual_override, computed_at |
| `attendance_corrections` | attendance_day_id, field, old_value, new_value, reason, by_user |
| `zones` | name, map_polygon (jsonb) |
| `cameras` | name, ip, rtsp_main, rtsp_sub, username, password_enc, type, zone_id, point_type (entry/exit/internal), access_point_id, ai_features (text[]), recognition_mode (off/on_camera/local_server), record_mode, retention_days, map_x, map_y, status, last_seen_at |
| `camera_health` | camera_id, ts, online, bitrate_kbps, packet_loss, fps, recording |
| `access_points` | name, gate_id, direction (in/out/both) |
| `gates` | name, driver (mock/gpio/modbus), config (jsonb, maxfiy maydonlar şifrlangan), state, mode (normal/fixed_open/fixed_closed), last_heartbeat_at |
| `gate_commands` | gate_id, command (open/close/fix_open/fix_closed/fix_off), source (ui/mobile/auto), by_user, sent_at, confirmed_at, result (done/failed/rejected), reason |
| `events` | type, ts, camera_id, gate_id, employee_id, vehicle_plate, confidence, direction, snapshot_ref, severity, review_status (none/pending/confirmed/rejected), reviewed_by, reviewed_at, local_id (unikal, oflayn uçun) |
| `vehicles` | plate_normalized (unikal/tenant), owner_type, employee_id, description, valid_until |
| `recordings` | camera_id, start_ts, end_ts, file_ref, size_bytes |
| `exports` | type (report/video), params, status, progress, file_ref, by_user |
| `settings` | key, value (jsonb) |
| `audit_logs` | ts, user_id, role, ip, device, category, action, object_type, object_id, old_value, new_value, result — append-only |
| `layouts` | user_id, name, grid, cells (jsonb) |
| `notification_prefs` | user_id, event_type, push_enabled |
| `sync_queue` (Edge, SQLite) | local_id, payload, created_at, synced |

---

## 7. API (REST + WebSocket) — minimal majburiy röyxat

Hammasi `/api/v1`, JSON, JWT. Har bir endpoint uçun ruxsat 3-bölim böyiça.

- **Auth:** `POST /auth/login`, `POST /auth/2fa`, `POST /auth/refresh`, `POST /auth/logout`, `GET /auth/me`, `POST /auth/change-password`
- **Users:** `GET/POST /users`, `GET/PATCH/DELETE /users/:id`, `POST /users/:id/reset-password`, `GET /sessions`, `DELETE /sessions/:id`
- **Org:** `GET/POST /departments`, `PATCH/DELETE /departments/:id`, `GET/POST /groups`, `PATCH/DELETE /groups/:id`, `POST /groups/:id/schedule`
- **Employees:** `GET/POST /employees`, `GET/PATCH/DELETE /employees/:id`, `POST /employees/import` (preview), `POST /employees/import/commit`, `GET /employees/export`, `GET /employees/import/template`, `POST /employees/:id/faces`, `DELETE /employees/:id/faces/:faceId`, `POST /employees/:id/schedule-override`, `DELETE /employees/:id/schedule-override/:oid`
- **Schedules:** `GET/POST /schedules`, `GET/PATCH/DELETE /schedules/:id`, `POST /schedules/preview` (14 kun), `GET /schedules/:id/calendar?month=YYYY-MM`, `GET /employees/:id/effective-schedule?date=`
- **Attendance:** `GET /attendance?date=&dept=&status=&q=`, `GET /attendance/summary?from=&to=`, `PATCH /attendance/:id` (tuzatiş), `POST /attendance/recalculate`
- **Cameras:** `GET/POST /cameras`, `GET/PATCH/DELETE /cameras/:id`, `POST /cameras/onvif-discover`, `POST /cameras/test-connection`, `GET /cameras/:id/health`, `GET /cameras/:id/stream?quality=main|sub` (WebRTC/HLS URL)
- **Video:** `GET /recordings?camera=&from=&to=`, `GET /cameras/:id/playback?ts=&speed=`, `POST /cameras/:id/snapshot`, `POST /exports/video`, `GET /layouts`, `POST /layouts`
- **Gates:** `GET /gates`, `GET /gates/:id`, `POST /gates/:id/commands` (`open|close|fix_open|fix_closed|fix_off`), `GET /gates/:id/commands`
- **Recognition:** `GET /faces/search?q=`, `POST /faces/search-by-image`, `GET /employees/:id/sightings`, `POST /events/:id/review` (`confirm|reject`), `POST /recognition/webhook/:provider` (kamera/terminal push)
- **ANPR / vehicles:** `GET /vehicles/events?q=&from=&to=`, `GET/POST /vehicles`, `PATCH/DELETE /vehicles/:id`, `GET /vehicles/stats`
- **Map:** `GET/POST /zones`, `PATCH/DELETE /zones/:id`, `POST /map/plan` (rasm yuklaş), `GET /map/last-seen/:employeeId`
- **Events:** `GET /events?type=&from=&to=`
- **Reports:** `GET /reports/:type?from=&to=&dept=&employee=` (10 ta tur), `POST /exports/report` (pdf|xlsx), `GET /exports/:id`
- **Audit:** `GET /audit?category=&user=&from=&to=`
- **Settings:** `GET /settings`, `PATCH /settings`, `GET /system/status` (arxitektura holati), `POST /system/sync`
- **Edge sync:** `POST /edge/sync` (paket, idempotent `local_id`), `GET /edge/config`
- **WebSocket** `/ws`: kanallar `events`, `gates`, `cameras.health`, `exports`, `sync`.

---

## 8. FON VAZİFALARİ

- Davomatni qayta hisoblaş (hodisa kelganda — tegişli kun; sozlama/grafik özgarganda — davr).
- Kun yakuni (har kuni 23:59 lokal va tungi smenalar uçun oyna tugaganda): `absent`, `no_exit` holatlarini yakunlaş.
- Kamera health monitoring (30 s).
- Gate watchdog (2 s / 10 s).
- Video segmentlarini tozalaş (saqlaş muddati, disk 90%).
- Noma'lum yuz rasmlarini tozalaş (30 kun).
- Vaqtinçalik ruxsatlar muddatini tekşiriş.
- Hisobot/video eksport navbati.
- Edge → server sinxronlaş.

---

## 9. EKRANLAR (dizayn bilan 1:1)

> Har bir ekranning töliq maketi — elementlar tartibi, tugmalarning joyi va vazifasi — `UI-SPEC.md` faylida (D01–D30, M01–M11). Quyidagi röyxat faqat qisqa mundarija.

Desktop / Web (30):
1 Login · 2 Qorovul dashboard · 3 Qorovul multi-kamera · 4 Şlagbaum control · 5 Kamera fullscreen pleyer · 6 Video arxiv · 7 Admin dashboard · 8 Xodimlar röyxati · 9 Xodim profili · 10 Xodim qöşiş/tahrirlaş · 11 Guruhlar va bölimlar · 12 İş grafiklari boşqaruvi · 13 Haftalik grafik yaratiş · 14 Smenali grafik · 15 Kunora grafik · 16 Uç kunda bir grafik · 17 İndividual grafik · 18 Kamera boşqaruvi · 19 Kamera qöşiş va sozlaş · 20 Multi-camera monitoring (admin) · 21 Face Recognition qidiruvi · 22 ANPR qidiruvi · 23 Korxona xaritasi · 24 Davomat monitoringi · 25 Hisobotlar · 26 Foydalanuvçilar va rollar · 27 Audit log · 28 Tizim sozlamalari · 29 Mas'ul şaxs dashboard · 30 Mas'ul şaxs hisobotlar.

Android (11): Login · Boş sahifa · Hodisalar · Kameralar · Jonli kamera/arxiv · Şlagbaum · Davomat · Qidiruv · Xodimning oxirgi joyi · Hisobotlar · Profil va sozlamalar.

UI talablari:
- Ranglar: Primary #172337, Secondary #334155, Background #F3F5F8, Surface #FFFFFF, Brown Control #795548, Success #16A34A, Warning #F59E0B, Danger #DC2626, Accent #2563EB. Dark Mode va Light Mode.
- Şriftlar: Inter (matn), Space Grotesk (sarlavhalar).
- Responsive: 1920×1080, 1600×900, 1366×768, 1024×768 (tablet), 390×844 va 360×800 (mobil).
- Har bir röyxatda: qidiruv, filtr, böş holat ("Natija topilmadi"), yuklanış holati, xato holati.
- Har bir yaratiş/tahrirlaş formasida validatsiya xabarlari maydon yonida.
- Har bir öçiriş tugmasi tasdiqlaş oynasi bilan ("Bu amalni ortga qaytarib bölmaydi"); boğliq ma'lumot bölsa — öçiriş bloklanadi va sababi körsatiladi.
- Toast xabarlari: muvaffaqiyat / xato.
- Kontrast kamida 4.5:1, tugmalar kamida 44px, ikonkali tugmalarda `aria-label`.
- Animatsiyalar minimal.

---

## 10. NOFUNKSİONAL TALABLAR

- Hodisa kamera → UI keçikişi: ≤ 2 soniya (lokal tarmoq).
- Şlagbaum buyruğı → rele: ≤ 500 ms; tasdiq timeout 3 s.
- 1 klientda 16 kamera gridi qotmasdan (sub-stream bilan).
- API p95 ≤ 300 ms (hisobot eksportidan taşqari).
- Barça vaqtlar bazada UTC, UI'da korxona vaqt zonasida (Asia/Tashkent).
- Loglar: strukturali JSON, maxfiy ma'lumotsiz.
- Zaxira nusxa: PostgreSQL har kuni, 14 kun saqlaş.
- Docker Compose orqali bir buyruq bilan işga tuşiriş (`docker compose up`), `.env.example` bilan.

---

## 11. TEST TALABLARİ

Majburiy unit/integration testlar (kamida):

1. Grafik: haftalik (turli kunlar turli soat), smenali aylaniş, kunora, uç kunda bir, individual vaqtinçalik → muddat tugagaç guruh grafigiga qaytiş.
2. Ierarxiya: vaqtinçalik istisno > doimiy individual > guruh; grafik biriktirilmagan holat.
3. Tungi smena 22:00–06:00: kiriş 21:55 → öz vaqtida; çiqiş ertasi 06:05 → öz vaqtida ketdi; bitta `attendance_days` yozuvi.
4. Grace: reja 09:00, grace 10 → 09:10 öz vaqtida, 09:11 keçikdi +11.
5. Erta ketiş, qöşimça iş (30 daq çegarasi), sababli keçikiş, kelmadi, hali boşlanmagan, çiqiş qayd etilmagan.
6. Grace yoki grafik özgarganda qayta hisoblaş.
7. Past işonçlilikdagi yuz davomatga kirmaydi; operator tasdiqlagaç kiradi.
8. Şlagbaum: datçik tasdiqlasa "Bajarildi"; timeout → "Bajarilmadi"; fix faol bölsa oddiy buyruq rad etiladi; avtomatik oçiş fix paytida işlamaydi.
9. Oflayn: hodisalar buferda, sinxronlaşda takrorlanmaydi (idempotent).
10. RBAC: har bir rol uçun ruxsatsiz endpoint → 403 + audit.
11. Öçiriş blokları: xodimi bor guruh, biriktirilgan grafik, öz hisobi, oxirgi admin.
12. Audit: barça BR-AUD-01 amallari yoziladi; audit yozuvini öçiriş/tahrirlaş imkonsiz.
13. Excel import: tögri, xato va takroriy qatorlar.
14. Biometrik: rozilikisiz enrollment rad etiladi; xodim öçirilganda şablon öçiriladi.

E2E (Playwright): login (3 rol) → tegişli menyu; şlagbaum oçiş/yopiş/fix; grafik yaratiş va guruhga biriktiriş → davomat özgaradi; hisobot PDF/Excel eksport.

---

## 12. BAJARİLİŞ ÇEKLİSTİ (`docs/progress.md` ga nusxala)

```
[ ] 0. Monorepo, Docker Compose, .env.example, CI (lint + test)
[ ] 1. AUTH: login, 2FA, refresh, lockout, sessiyalar, logout, parolni almaştiriş
[ ] 2. RBAC middleware + 3 rol menyulari
[ ] 3. ORG: bölimlar, guruhlar, öçiriş bloklari
[ ] 4. EMP: CRUD, profil, Excel import/eksport, yuz enrollment + rozilik, soft delete
[ ] 5. SCH: 5 tur, ierarxiya, istisnolar, validatsiya, preview, kalendar + jadval
[ ] 6. ATT: hisoblaş servisi (BR-ATT-01..20), tungi smena, qayta hisoblaş, tuzatişlar, monitoring ekrani
[ ] 7. CAM: CRUD, ONVIF, ulaniş testi, şifrlangan parol, health, recognition_mode
[ ] 8. VMS: MediaMTX integratsiya, grid layoutlar, drag&drop, oqim boşqaruvi, pleyer (barça funksiyalar), arxiv, eksport + watermark
[ ] 9. GATE: holatlar maşinasi, driver adapterlari (mock/gpio/modbus), fix, watchdog, jurnal, avtomatik oçiş
[ ] 10. FACE: provider adapterlari, qidiruv (FİŞ/rasm), çegara, operator tasdiqi, noma'lum şaxs
[ ] 11. ANPR: provider adapterlari, normallaştiriş, allowlist, statistika
[ ] 12. MAP: reja yuklaş, hududlar, kamera nuqtalari, oxirgi hudud
[ ] 13. EVT: real vaqt WebSocket, push (FCM), hodisa → video
[ ] 14. REP: 10 hisobot, filtrlar, xodim taqqoslaşi, PDF/XLSX eksport (fon)
[ ] 15. USR: CRUD, parol generatsiya, matritsa
[ ] 16. AUD: append-only log, filtrlar
[ ] 17. SET: barça sozlamalar, tizim holati, qölda sinxronlaş
[ ] 18. OFF: Edge Gateway servis, SQLite bufer, idempotent sinxronlaş
[ ] 19. Desktop EXE (Electron/Tauri) — qorovul va admin
[ ] 20. Android APK (Flutter) — 11 ekran
[ ] 21. i18n (uz), Dark/Light mode, responsive
[ ] 22. Testlar (11-bölim) yaşil, README töliq
[ ] 23. DEMO_MODE: seed ma'lumot, mock kamera/rele/tanış, "DEMO" belgisi
```

---

## 13. OÇİQ SAVOLLAR (kod yozişni töxtatmaydi — standart qiymat bilan davom et va README'da qayd et)

1. Kamera / yuz tanış terminali / ANPR kamera işlab çiqaruvçisi va modeli (vendor SDK yoki HTTP push formati). Standart: Mock + umumiy webhook adapteri.
2. Şlagbaum rele kontrolleri turi (GPIO, Modbus/TCP yoki boşqa). Standart: Mock + GPIO.
3. Backend tili: ~~NestJS yoki FastAPI~~ — **HAL QİLİNDİ: .NET 10** (ADR-001).
4. Desktop: Electron yoki Tauri. Standart: Electron.
5. Bir neçta korxona (multi-tenant) kerakmi yoki bitta korxona? Standart: `tenant_id` bor, lekin bitta tenant.
6. ANPR davomatga hisoblansinmi? Standart: yöq.
7. Push bildirişnoma servisi: FCM. Telegram bildirişnoma kerakmi? Standart: yöq.
8. Litsenziya çegaralari (kamera / AI kanal soni). Standart: çeklanmagan.
9. Talabalar davomati qanday hisoblanadi? Standart: Smart Access faqat kiriş/çiqişni qayd etib PULSE'ga uzatadi, davomatni PULSE hisoblaydi (ADR-003).
10. Talabalar röyxati manbai. Standart: Excel import, keyin PULSE'dan sinxronlaş.
11. Voyaga yetmagan talabalar biometrik roziligi. Standart: rozilik majburiy, kim bergani qayd etiladi.

**Qabul qilingan qarorlar (2026-11-11):** joylaştiriş gibrid — korxona içidagi server + alohida Hetzner server (ADR-002); kuzatiladiganlar — xodimlar VA talabalar (ADR-003); kiriş/çiqiş hodisalari PULSE'ga uzatiladi (ADR-004). Töliq: `docs/decisions.md`.


---

## Fayllar xaritasi (loyiha ildizi)

- `CLAUDE.md` — biznes qoidalari va arxitektura (shu fayl).
- `UI-SPEC.md` — har bir ekranda qaysi tugma qayerda turishi va nima qilishi. Har bir ekranni qurishdan oldin o'qing.
- `design/` — Claude Design canvas'ining to'liq nusxasi (ranglar, o'lchamlar, prototip xatti-harakati). `design/README.md` ni o'qing. Ziddiyat bo'lsa CLAUDE.md va UI-SPEC.md ustun.
