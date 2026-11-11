# UI-SPEC.md — Ekranlar, tugmalar joyi va vazifasi

> CLAUDE.md'ning majburiy qismi. Har bir ekran aynan şu tartibda quriladi.
> "Çap/öng/tepa/past" — ekran yönalişi. "→ Dxx / Mxx" — bosilganda oçiladigan ekran.
> Bu yerda yozilmagan tugma qöşilmaydi; yozilgan tugma taşlab ketilmaydi.

---

## A. UMUMİY QOİDALAR

### A.1 Desktop / Web karkasi (barça ekranlar, D01 dan taşqari)

```
┌────────────┬───────────────────────────────────────────────────────┐
│  SIDEBAR   │ TOP BAR: [breadcrumb + H1 sarlavha]   [fix][server][☾]│
│  (çap,     ├───────────────────────────────────────────────────────┤
│  248 px,   │ [Oflayn banner — faqat aloqa yöq bölsa]                │
│  #172337)  │                                                       │
│            │ KONTENT (ekranga qarab, pastda tasvirlangan)          │
│ [avatar]   │                                                       │
│ [Çiqiş]    │                                                       │
└────────────┴───────────────────────────────────────────────────────┘
```

**Sidebar (çap):**
| Element | Joyi | Vazifasi |
|---|---|---|
| Logo + "SMART ACCESS" + rol nomi | tepa | faqat körsatiş |
| Menyu bandlari (ikonka + matn) | tepadan pastga, CLAUDE.md 3-bölim tartibida | bosilsa tegişli ekran oçiladi; faol band kök (#2563EB) fonda; içki ekranda (masalan D09) ota menyu bandi faol turadi |
| Avatar (initsiallar) + FİŞ + server holati | eng past | körsatiş |
| "Çiqiş" ikonka tugmasi | eng past, öngda | sessiyani tugatadi → D01 |

**Top bar:**
| Element | Joyi | Vazifasi |
|---|---|---|
| Breadcrumb ("Rol / Sahifa") + H1 sarlavha | çapda | körsatiş |
| Fix indikatori (sariq badge, qulf ikonka) | öngda, birinçi | FAQAT şlagbaum fix rejimida körinadi; matn: "Fix: oçiq/yopiq holatda uşlab turilmoqda" |
| Server holati badge | öngda, ikkinçi | "Server: onlayn" (yaşil) yoki "Lokal rejim (oflayn)" (sariq) |
| Dark/Light tugmasi (oy/quyoş ikonka) | eng öngda | mavzuni almaştiradi, tanlov saqlanadi |

**Oflayn banner** — top bar ostida, töliq kenglik, sariq: "İnternet uzildi — hodisalar lokal qayd etilmoqda (N ta kutilmoqda)…".

**Telefon kengligi (<700 px):** sidebar kontent ustiga, töliq kenglikka ötadi; ustunlar bitta ustunga tuşadi; jadvallar gorizontal skroll qutisida.

### A.2 Tugma turlari va joylaşuv qoidasi

| Tur | Körinişi | Qayerda işlatiladi |
|---|---|---|
| Asosiy (primary) | kök töliq fon (#2563EB), oq matn | sahifadagi bitta asosiy amal; toolbar'da ENG ÖNGDA, formada pastki qatorda ENG ÖNGDA |
| İkkilamçi | oq fon, kulrang çegara | qolgan amallar; asosiy tugmaning çapida |
| Xavfli | qizil matn (#B91C1C) yoki qizil fon (faqat tasdiqlaş oynasida) | "Öçiriş", "Tizimdan çiqiş"; qator oxirida |
| Şlagbaum | jigarrang kvadrat (#795548), 110–220 px, katta ikonka | faqat OÇİŞ / YOPİŞ |
| Çip (chip) | yumaloq; tanlangani töq (#334155) fonda | filtrlar, grid tanlovi, tablar |
| İkonka tugma | 40×40, `aria-label` majburiy | pleyer, köz (parol), mavzu, çiqiş |

- Formalar: pastki qatorda öngga tekislangan **[Bekor qiliş] [Saqlaş]** (Saqlaş — asosiy).
- Tasdiqlaş oynasi (modal): ekran markazida; matn; pastda öngda **[Bekor qiliş] [Tasdiqlaş]** yoki **[Bekor qiliş] [Öçiriş]** (qizil).
- Toast: ekranning pastki markazida, 2.8 s.
- Har bir röyxat/jadval: böş holat matni, yuklaniş holati, xato holati.
- Barça tugmalar kamida 40 px balandlik (mobil 44–48 px).

### A.3 Mobil karkas (M02–M11)

```
┌──────────────────────────────┐
│ [←] Sarlavha    [Onlayn][👤] │  ← header, 60 px, #172337
│     Rol · 1-darvoza          │
├──────────────────────────────┤
│        KONTENT (skroll)      │
├──────────────────────────────┤
│ Boş│Hodisalar│Kamera│Şlagb.│Hisobot │ ← pastki navigatsiya, 58 px
└──────────────────────────────┘
```
| Element | Joyi | Vazifasi |
|---|---|---|
| "←" Orqaga | header çapda, FAQAT içki ekranlarda (M05, M07, M08, M09, M11) | oldingi ekranga qaytadi (M09 → M08) |
| Sarlavha + "Rol · darvoza" | header ortada | körsatiş |
| "Onlayn" badge | header öngda | aloqa holati |
| Profil ikonka | header eng öngda | → M11 |
| Pastki nav 5 band | ekran pasti | Boş→M02, Hodisalar→M03, Kamera→M04, Şlagbaum→M06, Hisobot→M10; faol band kök |
| Tasdiqlaş "bottom sheet" | pastdan çiqadi | [Bekor qiliş] çapda, [Tasdiqlaş] öngda (jigarrang) |

---

## B. DESKTOP / WEB EKRANLARİ (30)

### D01 — Login
İkki ustun (telefonda ustma-ust).
| Element | Joyi | Vazifasi |
|---|---|---|
| Logo, sarlavha, 3 qator imkoniyatlar, DEMO eslatmasi | çap ustun, töq navy fon | körsatiş |
| Forma kartasi (max 400 px) | öng ustun markazida | — |
| Korxona identifikatori | forma, 1-maydon | matn |
| Login | 2-maydon | matn |
| Parol + köz ikonka tugmasi | 3-maydon; köz — maydonning öngida | köz: parolni körsatiş/yaşiriş |
| "Eslab qoliş" checkbox | parol ostida | sessiya muddati (BR-AUTH-05) |
| Xato banneri (qizil) | checkbox ostida, faqat xatoda | "Login yoki parol notöğri…" |
| **Tizimga kiriş** (asosiy, töliq kenglik) | xato ostida | login → rolga qarab D02 / D07 / D29; 2FA yoqiq bölsa kod oynasi |
| Izoh "Login va parollarni faqat administrator yaratadi…" | tugma ostida | matn |
| Demo hisoblar çiplari (qorovul / admin / mas_ul) | eng pastda | FAQAT `DEMO_MODE`; login va parolni töldiradi |
| Enter tugmasi | — | formani yuboradi |

### D02 — Qorovul dashboard
İkki ustun: çap ≈ 2/3, öng ≈ 1/3.

Çap ustun:
| Element | Joyi | Vazifasi |
|---|---|---|
| Karta "Kameralar — jonli" + "DEMO tasvir" badge | çap ustun tepasi | — |
| **Barça kameralar** | şu kartaning sarlavha qatorida, öngda | → D03 |
| 2×2 kamera bloki | karta içida | blok bosilsa: şu kamera tanlanadi → D03 |
| Karta "Tezkor qidiruv" | kameralar ostida | — |
| Qidiruv inputi "FİŞ yoki avtomobil raqami" | karta içida, çapda | yozganda natija darhol çiqadi |
| **Yuz tasviri böyiça** | input yonida, öngda | rasm tanlaş → yuz böyiça qidiruv |
| Natijalar röyxati | input ostida | har qator: avatar, nom, "Oxirgi: vaqt · kamera", holat badge (Korxonada / Çiqib ketgan / Aniqlanmagan) |

Öng ustun:
| Element | Joyi | Vazifasi |
|---|---|---|
| Karta "Şlagbaum" + holat badge | öng ustun tepasi; badge sarlavha qatorida öngda | Yopiq / Oçiq / Oçilmoqda… / Yopilmoqda… |
| **OÇİŞ** (jigarrang kvadrat) | karta ortasida, çapda | buyruq (BR-GATE-02/03); faol holatda oq ramka + kök halqa + "FAOL — oçiq" |
| **YOPİŞ** (jigarrang kvadrat) | OÇİŞ yonida, öngda | xuddi şunday |
| Buyruq natijasi banneri | tugmalar ostida | kulrang "kutilmoqda" / sariq "yuborildi" / yaşil "bajarildi" / qizil "bajarilmadi" |
| **Oçiq holatda fix** | banner ostida, çapda (yarim kenglik) | tasdiqlaş oynasi → fix yoqiladi |
| **Yopiq holatda fix** | öngda (yarim kenglik) | xuddi şunday |
| Fix banneri (sariq) + **Bekor qiliş** | fix tugmalari ostida, FAQAT fix faol bölsa; tugma banner içida öngda | fixni öçiradi |
| Karta "Tezkor hodisalar" | şlagbaum kartasi ostida | real vaqt röyxati, eng yangisi tepada; qator: çapda kiçik rasm/ikonka, ortada matn + kamera, öngda vaqt + tur badge |

### D03 — Qorovul multi-kamera
| Element | Joyi | Vazifasi |
|---|---|---|
| Info banner (kök) | kontent tepasi | "Demo rejim… Drag & Drop mumkin" |
| Toolbar kartasi | banner ostida | — |
| "Grid" + çiplar 1×1, 2×2, 3×3, 4×4, Custom | toolbar çapida | layoutni almaştiradi (BR-VMS-01) |
| Kamera qidiruv inputi | grid çiplaridan keyin | kataklarni nom böyiça filtrlaydi |
| Hudud çiplari (Hammasi, Darvoza, Kiriş, …) | inputdan keyin | hudud böyiça filtr |
| **Fullscreen: ‹tanlangan kamera›** | toolbar öngida | → D05 şu kamera bilan |
| **Kamera qöşiş** (asosiy) | eng öngda, FAQAT admin | → D19 |
| Oqim izohi | toolbar ostida, kiçik matn | "körinadigan N ta kamera main stream…" |
| Kamera kataklari | grid | bosiş — tanlaş (kök ramka); sudrab boşqa katakka taşlaş — joy almaştiriş; Custom: birinçi katak 2×2 katta |
| Böş katak (punktir) | qolgan joylar | "Kamerani şu yerga sudrab keling" |
| Kamera bloki overleyi | katak içida | tepa-çap: nom, joylaşuv; tepa-öng: LIVE (qizil), REC; past-çap: sana-vaqt; past-öng: sifat · AI; ramka: aniqlangan obyekt; oflayn: ortada "Aloqa uzilgan" |

### D04 — Şlagbaum control
İkki ustun.
| Element | Joyi | Vazifasi |
|---|---|---|
| Sarlavha "Şlagbaum boşqaruvi — 1-darvoza" + holat badge | çap kartaning tepasi | — |
| Şlagbaum sxemasi (ustun + qizil-oq strela) | sarlavha ostida, markazda | strela holatga qarab gorizontal / ~60° / harakatda |
| **OÇİŞ**, **YOPİŞ** | sxema ostida, yonma-yon, markazda | D02 dagi kabi |
| Natija banneri | tugmalar ostida | D02 dagi kabi |
| **Oçiq holatda fix qiliş** / **Yopiq holatda fix qiliş** | banner ostida, yonma-yon | tasdiq bilan fix |
| Fix banneri + **Fixni bekor qiliş** | faqat fix faol | fixni öçiradi |
| "Demo: buyruq xatosini simulyatsiya qiliş" checkbox | eng pastda | FAQAT `DEMO_MODE` |
| Karta "Apparat holati (qayta aloqa)" | öng ustun tepasi | qatorlar: Rele kontrolleri, Qayta aloqa datçigi, Watchdog, Avariya qölda boşqaruv — har birida öngda badge |
| Karta "Buyruqlar jurnali" | öng ustun, pastda | jadval: Vaqt, Foydalanuvçi, Buyruq, Natija (badge); böş holat matni |

### D05 — Kamera fullscreen pleyer
İkki ustun: çap ≈ 3/4 (qora karta), öng ≈ 1/4.
| Element | Joyi | Vazifasi |
|---|---|---|
| Kamera nomi, joylaşuv, rejim badge (LIVE qizil / ARXIV kulrang), "DEMO tasvir" | qora karta tepa qatori, çapda | körsatiş |
| **Sinxron köriş (4 kamera)** | tepa qator öngida | 1 katta ↔ 2×2 sinxron; faol bölsa kök |
| **Alohida oyna** | eng öngda | kamerani yangi oynada oçadi |
| Video maydoni 16:9 | tepa qator ostida | overley: tepa-çap nom; tepa-öng rejim + tezlik; past-çap sana-vaqt; past-öng sifat · AI; aniqlaş ramkasi |
| Boşqaruv qatori çapdan öngga: **−5 daq**, **Play/Pause**, **Stop**, **+5 daq**, **LIVE** (qizil), tezlik çiplari **0.25× 0.5× 1× 2× 4× 8×** | video ostida, çapda | −5/+5: arxivda siljiş; Play/Pause: LIVE'da bosilsa arxiv rejimiga ötib töxtaydi; Stop: töxtatiş; LIVE: jonli efirga qaytiş; tezlik: arxiv tezligi (tanlangani kök) |
| **−10 s / +10 s** | −5/+5 yonida (CLAUDE.md BR-VMS-05) | qisqa siljiş |
| **Snapshot**, **Saqlaş (eksport)**, **Ovoz**, **Fullscreen** | şu qatorning öngida | snapshot (audit); video qismini eksport (audit, watermark); ovozni yoq/öçir; töliq ekran |
| Timeline (00–24) | boşqaruv ostida, töliq kenglik | yaşil — yozilgan qismlar; rangli çiziqlar — hodisalar; oq çiziq — joriy vaqt; bosiş/sudraş — şu vaqtga ötiş |
| Soat belgilari 00, 03 … 24 | timeline ostida | — |
| Karta "Kamera böyiça qidiriş" + input + kameralar röyxati | öng ustun tepasi | qatorni bosiş — pleyerdagi kamerani almaştiradi; holat nuqtasi yaşil/qizil |
| Karta "Hodisa böyiça topiş" | öng ustun pastda | qatorni bosiş — videoni şu hodisa vaqtiga olib boradi |

### D06 — Video arxiv
| Element | Joyi | Vazifasi |
|---|---|---|
| Filtr kartasi: **Kamera** (select), **Sana** (sana tanlagiç), **Vaqt oralığı** | kontent tepasi, çapdan öngga | natijalarni filtrlaydi |
| Hodisa turi çiplari: Barçasi, Xodim, Avtomobil, Noma'lum şaxs, Şlagbaum, Aloqa | filtr kartasining öngida | filtr |
| Natija kartalari gridi | filtr ostida | har karta: rasm 16:9 (past-çapda vaqt, past-öngda davomiylik), sarlavha, kamera nomi, pastda çapda tur badge, öngda **Pleyerda oçiş** → D05 şu vaqt bilan (audit: "Arxiv körildi") |
| Böş holat | — | "Tanlangan filtr böyiça yozuv topilmadi" |
| İzoh | eng pastda | "Arxivni kim körgani audit logga yoziladi" |

### D07 — Administrator dashboard
| Element | Joyi | Vazifasi |
|---|---|---|
| 11 ta KPI kartasi (avtomatik grid, ~190 px) | kontent tepasi | tartib: Bugun kelgan, Hozir korxonada, Keçikkanlar, Kelmaganlar, Erta ketganlar, Bugungi avtomobillar, Kirişlar, Çiqişlar, Faol kameralar, İşlamayotgan kameralar, Noma'lum şaxslar. Har karta: çapda rangli ikonka + nom, katta son, izoh. Bosilsa → tegişli filtrli ekran (davomat → D24, kameralar → D18, avtomobillar → D22, noma'lum → D21) |
| "Kiriş va çiqişlar — soatlar böyiça" (ustunli grafik, kök=kiriş, jigarrang=çiqiş) | 2-qator, çapda | — |
| "Bugungi davomat holati" (donut + legend) | 2-qator, öngda | — |
| **Davomat monitoringi** / **Multi-kamera** | donut kartasining pastida | → D24 / → D20 |
| "Oxirgi hodisalar" | 3-qator, çapda | qator: çapda tur badge, ortada matn, öngda vaqt |
| "Kameralar holati" | 3-qator, öngda | qator: holat badge, nom, öngda bitreyt |

### D08 — Xodimlar röyxati
| Element | Joyi | Vazifasi |
|---|---|---|
| Qidiruv "FİŞ böyiça" | toolbar çapida | filtr |
| Bölim çiplari | qidiruvdan keyin | filtr |
| **Excel import** | toolbar öngida, 1-tugma | şablon / yuklaş / oldindan köriş / tasdiqlaş oqimi (BR-EMP-07) |
| **Eksport** | 2-tugma | joriy filtr böyiça .xlsx |
| **Xodim qöşiş** (asosiy) | eng öngda | → D10 (yangi) |
| Jadval ustunlari: ID, FİŞ (avatar + havola), Lavozim, Bölim, Guruh, İş grafigi (+ "İndividual" badge), Holat, [amal] | toolbar ostida | FİŞ bosilsa → D09 |
| **Öçiriş** (qizil matn) | har qatorning oxirida | tasdiqlaş oynasi → soft delete |

### D09 — Xodim profili
İkki ustun: çap ≈ 1/3, öng ≈ 2/3.
| Element | Joyi | Vazifasi |
|---|---|---|
| Profil kartasi: katta avatar, FİŞ, lavozim · bölim, holat badge; ID, telefon, qabul sanasi, guruh (2×2) | çap ustun tepasi | — |
| **Tahrirlaş** (asosiy) | profil kartasi pastida, 1-tugma | → D10 töldirilgan |
| **Röyxatga qaytiş** | 2-tugma | → D08 |
| **Xodimni öçiriş** (qizil) | 3-tugma | tasdiqlaş → öçiriş → D08 |
| Karta "Yuz biometrikasi": 5 ta kvadrat (sifat %) + xavfsiz identifikator (maskalangan) | çap ustun pastda | — |
| Karta "İş grafigi: ‹nomi›" + daraja badge (Umumiy/Guruh/İndividual) | öng ustun tepasi; badge öngda | — |
| 7 kunlik hafta (Du…Ya) | şu karta içida | iş kuni kök, dam kulrang |
| "Bugun: reja" + kiriş holati + çiqiş holati badge | hafta ostida | — |
| Karta "Oylik körsatkiçlar" (davomat %, keçikiş, erta ketiş, kelmagan + progress) | öng ustun, pastda çapda | — |
| Karta "Oxirgi aniqlanişlar" (vaqt, kamera, %) | pastda öngda | — |

### D10 — Xodim qöşiş / tahrirlaş
İkki ustun: çap ≈ 2/3, öng ≈ 1/3.
| Element | Joyi | Vazifasi |
|---|---|---|
| "Asosiy ma'lumotlar" forma, 2 ustun: FİŞ · Lavozim / Bölim · İş guruhi (select) / Telefon · Qabul sanasi | çap kartada | validatsiya xabari maydon ostida |
| "Biriktirilgan iş grafigi" select (töliq kenglik; 1-variant "Guruh grafigidan meros olinadi") | forma oxirida | individual grafik tanlaş |
| **Bekor qiliş** | forma pastida, öngda (çaproq) | → D08 / D09 |
| **Saqlaş** (asosiy) | eng öngda | saqlaş → D09, toast |
| "Yuz tasvirlari" kartasi: 6 slot 3×2 (Old, Çap, Öng, Yuqoridan, Yoruğlik 1, Yoruğlik 2) | öng ustun | slot bosilsa — rasm yuklaş + sifat tekşiruvi; rozilik yöq bölsa slotlar nofaol |
| "Biometrik ma'lumotlarga yozma rozilik olingan" checkbox | slotlar ostida | BR-EMP-06 |
| **Öçiriş** (qizil) | FAQAT tahrirlaşda, forma pastida eng çapda | tasdiqlaş → öçiriş |

### D11 — Guruhlar va bölimlar
| Element | Joyi | Vazifasi |
|---|---|---|
| Ierarxiya izohi | toolbar çapida | matn |
| "Yangi guruh nomi" input | toolbar öngida | — |
| **Guruh yaratiş** (asosiy) | eng öngda | guruh yaratadi, toast |
| Bölim filtri / **Bölim qöşiş** | toolbar, input çapida | bölim CRUD (CLAUDE.md BR-ORG) |
| Guruh kartalari gridi | toolbar ostida | — |
| Karta sarlavhasi: nom, bölim badge, **Öçiriş** (qizil) | kartaning tepa qatori; badge va tugma öngda | öçiriş (içida xodim bölsa bloklanadi) |
| "N xodim" + avatarlar | sarlavha ostida | — |
| "Guruh grafigi" select | karta pastida | tanlaş darhol saqlanadi, toast "Grafik guruhga biriktirildi" |

### D12 — İş grafiklari boşqaruvi
| Element | Joyi | Vazifasi |
|---|---|---|
| 3 ierarxiya kartasi (1 Umumiy · 2 Guruh · 3 Alohida xodim) | kontent tepasi | sonlar bilan |
| **Yangi grafik yaratiş** (asosiy) | çap ustun (≤380 px) tepasi | → D13 |
| Grafik kartalari röyxati (nom, tur badge, daraja badge, "N guruh · N xodim") | şu tugma ostida | bosiş — tanlaş (kök ramka), öngda detal |
| Detal kartasi sarlavhasi | öng ustun tepasi | — |
| **Kalendar** / **Jadval** çiplari | sarlavha qatorida öngda | körinişni almaştiradi |
| **Tahrirlaş** | çiplar yonida | → D13–D17 töldirilgan |
| **Öçiriş** (qizil) | eng öngda | biriktirilgan bölsa bloklanadi |
| İzoh qatori (oy, grace, tanaffus) | sarlavha ostida | — |
| Kalendar (Du…Ya, 6+30 katak) yoki jadval (Sana, Kun, İş vaqti, Sof soat, İzoh) | kartada | bugun — kök ramka; tungi smena — töq fon "+1" |
| Legend | kalendar ostida | İş kuni / Dam oliş / Tungi smena |
| Karta "Grafikni biriktiriş" | detal ostida | 2 ustun |
| Çap: Guruh select + **Guruhga biriktiriş** | | tanlangan grafikni guruhga |
| Öng: Xodim select + **Xodimga biriktiriş** + **İstisnoni olib taşlaş** | | individual istisno qöşiş/olib taşlaş |

### D13–D17 — Grafik yaratiş (umumiy karkas)
| Element | Joyi | Vazifasi |
|---|---|---|
| Tur çiplari: Haftalik · Smenali · Kunora · 3 kunda 1 · İndividual | kontent tepasi | formani almaştiradi |
| Çap kartasi (≈2/3): "Grafik nomi", "Tanaffus (daqiqa)" (2 ustun) + turga xos blok | — | — |
| **Bekor qiliş** | forma pastida, öngda (çaproq) | → D12 |
| **Grafikni saqlaş** (asosiy) | eng öngda | validatsiya → saqlaş → D12 (yangi grafik tanlangan) |
| Öng kartasi (≈1/3): "Oldindan köriş" — 14 kun (7×2 katak) | — | formaga qarab jonli yangilanadi |

**D13 Haftalik:** tezkor çiplar **Duşanba–Juma**, **Duşanba–Şanba** (forma tepasida); jadval 7 qator: Kun · İşlaydi (checkbox) · Boşlaniş (vaqt) · Tugaş (vaqt).
**D14 Smenali:** 3 smena kartasi yonma-yon (nom, vaqt, izoh); "Şu hafta boşlanadigan smena" select; kök info banner: tungi smena 22:00 → 06:00 (+1).
**D15 Kunora:** Boşlaniş sanasi · sikl badge "1 kun iş / 1 kun dam" (1-qator); İş boşlanişi · İş tugaşi (2-qator).
**D16 Uç kunda bir:** D15 bilan bir xil, badge "Har 3 kunda 1 kun iş".
**D17 İndividual:** haftalik jadval (D13) + Xodim select · Turi (Vaqtinçalik / Doimiy) · Boşlaniş sanasi · Tugaş sanasi; saqlanganda xodimga biriktiriladi.

### D18 — Kamera boşqaruvi
| Element | Joyi | Vazifasi |
|---|---|---|
| Hudud çiplari | toolbar çapida | filtr |
| **Multi-kamera monitoring** | toolbar öngida | → D20 |
| **Kamera qöşiş** (asosiy) | eng öngda | → D19 (yangi) |
| Jadval: Kamera (nom + joylaşuv), Hudud, IP, AI funksiyalari, Oqim, Paket yöqotiş, Yoziş (badge), Holat (badge), [amallar] | toolbar ostida | — |
| **Sozlaş** | qator oxirida, 1-tugma | → D19 töldirilgan |
| **Öçiriş** (qizil) | qator oxirida, 2-tugma | tasdiqlaş → öçiriş |

### D19 — Kamera qöşiş va sozlaş
İkki ustun: çap ≈ 2/3, öng ≈ 1/3.
| Element | Joyi | Vazifasi |
|---|---|---|
| Forma 2 ustun: Kamera nomi · IP manzil / RTSP URL (töliq kenglik) / Login · Parol / Kamera turi · Hudud | çap karta | parol maydoni yaşirin; tahrirlaşda parol körsatilmaydi |
| "Nuqta turi" çiplari: Kiriş nuqtasi · Çiqiş nuqtasi · İçki hudud | forma ostida | bitta tanlov |
| "AI funksiyalari" çiplari (bir neçtasi tanlanadi): Face Recognition · Face Detection · ANPR / LPR · Motion Detection · Person Detection | şu ostida | — |
| "Tanış qayerda" select: Öçiq · Kamera içida · Lokal server | AI çiplari ostida | BR-ARC-01; Face/ANPR tanlansa "Öçiq" mumkin emas |
| Yoziş rejimi · Saqlaş muddati (select) | forma oxirida, 2 ustun | — |
| **Ulaniş testi** | pastki qatorda eng çapda | RTSP'dan kadr olib natijani körsatadi (BR-CAM-03) |
| **Bekor qiliş** / **Saqlaş** (asosiy) | pastki qatorda öngda | saqlaş → D18, toast "yangi video blok yaratildi" |
| Karta "ONVIF orqali aniqlaş": izoh, **ONVIF qurilmalarini qidiriş** | öng ustun | qidirilayotganda matn "Qidirilmoqda…" |
| Topilgan qurilmalar röyxati: nom, IP, **Tanlaş** (qator öngida) | qidiruv tugmasi ostida | IP va RTSP maydonlarini töldiradi |
| İnfo banner | öng ustun pastda | "Yangi kamera saqlanganda… yangi video blok" |

### D20 — Multi-camera monitoring (admin)
D03 bilan bir xil; farqi: toolbar'da **Kamera qöşiş** körinadi; sidebar'da "Kameralar" bandi faol.

### D21 — Face Recognition qidiruvi
İkki ustun: çap ≤ 400 px, öng — qolgani.
| Element | Joyi | Vazifasi |
|---|---|---|
| "FİŞ böyiça qidiriş" input | çap ustun tepasi | filtr |
| **Yuz tasvirini yuklab qidiriş** (punktir çegarali, katta) | input ostida | rasm tanlaş → yuz böyiça qidiruv (rasm saqlanmaydi) |
| Xodimlar röyxati (avatar, FİŞ, "oxirgi: vaqt · kamera") | şu ostida | bosiş → öngda natija; tanlangani kök ramka |
| Natija kartasi: rasm (96 px) çapda; FİŞ, lavozim, holat badge öngda | öng ustun tepasi | — |
| 4 maydon 2×2: Oxirgi vaqt · Kamera / Hudud · İşonçlilik (çegara bilan) + progress | karta içida | yaşil ≥ çegara, sariq < çegara |
| Sariq banner "İşonçlilik çegaradan past…" + **Tasdiqlaş** (asosiy) + **Rad etiş** | progress ostida, FAQAT tekşiruv kerak bölsa; tugmalar banner öngida | BR-FACE-04 (audit) |
| "Oldingi aniqlanişlar tarixi" jadval: Vaqt, Kamera, Hudud, İşonçlilik, Holat | öng ustun pastda | — |

### D22 — ANPR avtomobil qidiruvi
| Element | Joyi | Vazifasi |
|---|---|---|
| Tab çiplari: Hodisalar · Ruxsat berilgan röyxat · Statistika | toolbar çapida | tab almaştiradi |
| "Davlat raqami böyiça qidiriş" input | toolbar öngida | böşliqsiz qidiruv |
| **Hodisalar tab:** jadval (Davlat raqami — raqam plaşkasi körinişida, bosiladi; Vaqt; Yönaliş badge; Kamera; Egasi/xodim; Holat badge) | çap ≈ 2/3 | qator bosilsa öngda detal |
| Detal kartasi: avtomobil rasmi 16:9, katta raqam plaşkasi, holat + yönaliş·vaqt badge, egasi · kamera | öng ≈ 1/3 | — |
| Sariq banner "Röyxatda yöq… xavfli degani emas" + **Operator tekşiruvi** + **Ruxsat röyxatiga qöşiş** (asosiy) + **Vaqtinçalik ruxsat** | detal pastida, FAQAT röyxatda yöq bölsa | BR-ANPR-04 (audit) |
| **Ruxsat tab:** **Avtomobil qöşiş** (asosiy, tepada öngda) + jadval (Raqam, Egasi, Turi, Muddati, [Tahrirlaş] [Öçiriş]) | — | CRUD |
| **Statistika tab:** 4 KPI (Kirgan, Çiqqan, Röyxatda yöq, Vaqtinçalik) + soatlar grafigi | — | — |

### D23 — Korxona xaritasi
İkki ustun: çap ≈ 3/4, öng ≈ 1/4.
| Element | Joyi | Vazifasi |
|---|---|---|
| Sarlavha + Xodim select | çap karta tepasi; select öngda | tanlangan xodimning oxirgi hududi |
| **Rejani yuklaş** / **Hududni çiziş** | sarlavha qatorida, select çapida (faqat admin) | BR-MAP-01 |
| Xarita: hudud bloklari, kamera nuqtalari (yaşil/qizil, bosiladi), xodim markeri (avatar) | karta içida | oxirgi hudud kök fon + kök ramka |
| Legend | xarita ostida | — |
| Kök banner "Aniq koordinata emas…" | eng pastda | majburiy matn |
| Xodim kartasi: FİŞ, "Oxirgi aniqlangan hudud: …, vaqt", vaqt ketma-ketligi röyxati (vaqt, kamera, % badge) | öng ustun tepasi | — |
| Kamera kartasi (nom, joylaşuv, holat, AI) | öng ustun pastda, FAQAT kamera nuqtasi bosilganda | — |

### D24 — Davomat monitoringi
| Element | Joyi | Vazifasi |
|---|---|---|
| Sana çiplari: Bugun · Keça + sana tanlagiç | toolbar çapida | kun tanlaş |
| "Xodim qidiriş" input | keyin | filtr |
| Bölim select | keyin | filtr |
| "Grace period: N daq" | toolbar eng öngida | matn |
| Holat çiplari soni bilan: Hammasi · Öz vaqtida · Keçikkan · Sababli keçikiş · Kelmadi · Erta ketdi · Qöşimça iş | toolbar ostida | filtr |
| Jadval: Xodim, Bölim, Grafik, Reja, Kiriş, Çiqiş, Kiriş holati (badge), Çiqiş holati (badge), [amal] | çiplar ostida | — |
| **Tuzatiş** | qator oxirida, FAQAT admin | modal: vaqt / sabab (majburiy) → saqlaş (BR-ATT-18) |
| **Sabab qöşiş** | qator oxirida, keçikkanlarda, admin | "Sababli keçikiş"ga ötkazadi |

### D25 — Hisobotlar (admin)
İkki ustun: çap ≤ 300 px, öng — qolgani.
| Element | Joyi | Vazifasi |
|---|---|---|
| 10 ta hisobot röyxati (raqam + nom) | çap kartada | bosiş — tanlaş (kök fon) |
| Sarlavha kartasi: hisobot nomi; Davr (sana oralığı); Bölim select | öng tepada, çapdan | filtr |
| **PDF**, **Excel** | sarlavha kartasi öngida | eksport (fon vazifa), audit |
| Progress kartasi: "Tayyorlanmoqda (PDF)… 40%" + çiziq; tayyor bölsa **Yuklab oliş** havolasi | sarlavha ostida, FAQAT eksport paytida | — |
| Jadval (ustunlar hisobot turiga qarab); 1-ustun havola | progress ostida | xodim nomi bosilsa pastda xodim paneli |
| Xodim paneli: FİŞ + grafik badge; davomat %, keçikişlar, kelmagan kunlar; kunlar jadvali (Sana, Grafik, Haqiqiy kiriş, Haqiqiy çiqiş, Holat); "reja soat / haqiqiy soat" + progress | jadval ostida | BR-REP-03 |

### D26 — Foydalanuvçilar va rollar
İkki ustun: çap ≈ 2/3, öng ≈ 1/3.
| Element | Joyi | Vazifasi |
|---|---|---|
| Jadval: Login, FİŞ, Rol, 2FA (badge), Oxirgi kiriş, Holat, [amal] | çap tepasi | — |
| **Öçiriş** (qizil) | qator oxirida | öz hisobini / oxirgi adminni öçirib bölmaydi |
| **Parolni tiklaş** | qator oxirida, Öçirişdan çapda | yangi vaqtinçalik parol |
| "Rollar va ruxsatlar" matritsasi (Modul × Qorovul / Administrator / Mas'ul şaxs) | çap ustun ortasi | faqat körsatiş |
| "Faol sessiyalar": foydalanuvçi, qurilma · vaqt; **Tugatiş** qator öngida | çap ustun pastda | sessiyani bekor qiladi (audit) |
| "Foydalanuvçi yaratiş" forma: FİŞ, Login, Rol (select), Boşlanğiç parol + **Yaratiş** (maydon öngida) | öng ustun | Yaratiş — tasodifiy parol generatsiya qiladi |
| **Foydalanuvçi qöşiş** (asosiy, töliq kenglik) | forma pastida | saqlaş, toast |

### D27 — Audit log
| Element | Joyi | Vazifasi |
|---|---|---|
| Toifa çiplari: Barçasi · Şlagbaum · Video köriş/eksport · Kiriş urinışlari · Sozlamalar · Hisobotlar | toolbar | filtr |
| Foydalanuvçi select, sana oralığı | çiplardan keyin, öngda | filtr |
| Jadval: Vaqt, Foydalanuvçi, Toifa, Harakat, Natija (badge) | toolbar ostida | faqat öqiş; tahrirlaş/öçiriş tugmalari YÖQ |
| İzoh | eng pastda | "öçirib bölmaydigan jurnal" |

### D28 — Tizim sozlamalari
Kartalar gridi (2 ustun, telefonda 1).
| Karta | Joyi | Elementlar va vazifasi |
|---|---|---|
| Davomat hisobi | 1-qator çap | "Grace period" (son, 0–60) — özgarsa davomat darhol qayta hisoblanadi; izoh |
| Yuz tanış | 1-qator öng | "İşonçlilik çegarasi: N%" slider (60–99); kalitlar (switch, qator öngida): Operator tasdiqi · Administrator uçun 2FA · İnternet aloqasini uziş (FAQAT DEMO) |
| Arxitektura va sinxronlaş | 2-qator çap | 5 qator (IP kameralar, Edge Gateway, Lokal xotira, Markaziy server, Klientlar) — nom çapda, izoh ortada, holat badge öngda; pastda "Kutilayotgan hodisalar: N" + **Qölda sinxronlaş** (öngda) |
| Şlagbaum kontrolleri | 2-qator öng | Rele, Datçik, Watchdog, Avariya (badge öngda); "Video arxivni saqlaş (kun)" select |
| Maxfiylik va huquqiy talablar | 3-qator, töliq kenglik | matn |
| CLAUDE.md 4.17 dagi qolgan sozlamalar | qöşimça kartalar ("Şlagbaum", "ANPR", "Xavfsizlik") | har biri: nom çapda, boşqaruv öngda |

### D29 — Mas'ul şaxs dashboard
| Element | Joyi | Vazifasi |
|---|---|---|
| 5 KPI kartasi (Bugun kelgan, Hozir korxonada, Keçikkanlar, Kelmaganlar, Erta ketganlar) | tepa | bosilsa → D24 filtr bilan |
| "Keçikkanlar va erta ketganlar" (FİŞ çapda, holat badge öngda) | 2-qator çap | — |
| "Kirib-çiqqan avtomobillar" (vaqt, matn, tur badge) | 2-qator öng | — |
| "Muhim xavfsizlik hodisalari" | 3-qator | — |
| **Bugungi davomat** | şu kartaning pastida, çapda | → D24 |
| **Hisobotlar** (asosiy) | yonida | → D30 |

### D30 — Mas'ul şaxs hisobotlar
D25 bilan bir xil; farqi: faqat köriş va eksport, sozlaş tugmalari yöq; sidebar'da faqat 3 band.

---

## C. ANDROID EKRANLARİ (11)

### M01 — Login
| Element | Joyi | Vazifasi |
|---|---|---|
| Logo, "SMART ACCESS", şior | ekranning tepa 1/3 qismi, töq navy | — |
| Oq panel (tepa burçaklari yumaloq) | pastki 2/3 | — |
| "Tizimga kiriş" sarlavha; Korxona ID; Login; Parol + köz tugmasi (öngda) | panel içida, tepadan pastga | — |
| "Eslab qoliş" checkbox | parol ostida | — |
| **Kiriş** (asosiy, töliq kenglik, 48 px) | checkbox ostida | → M02 (2FA bölsa SMS/TOTP kod ekrani) |
| İzoh + "Android · vN · demo" | eng pastda markazda | — |

### M02 — Boş sahifa
| Element | Joyi | Vazifasi |
|---|---|---|
| 2×2 KPI (Bugun kelgan, Hozir korxonada, Keç qolgan, Kelmagan) | tepa | — |
| 4 ta tezkor tugma bir qatorda: **Davomat**, **Qidiruv**, **Jonli**, **Hisobot** | KPI ostida | → M07, M08, M05, M10 |
| Şlagbaum kartasi: holat çapda, **Boşqariş** (jigarrang) öngda | tugmalar ostida | → M06 |
| "Oxirgi hodisalar" (3 ta) | eng pastda | bosilsa → M03 |

### M03 — Hodisalar
| Element | Joyi | Vazifasi |
|---|---|---|
| Çiplar (gorizontal skroll): Barçasi · Muhim · Avtomobillar | tepa | filtr |
| Hodisa kartalari: ikonka çapda, matn + kamera ortada, vaqt + badge öngda | ostida | bosilsa → M05 şu vaqtda |

### M04 — Kameralar
| Element | Joyi | Vazifasi |
|---|---|---|
| İzoh | tepa | — |
| 2 ustunli kamera gridi: tepa-çap nom, tepa-öng LIVE/Oflayn, past-öng sifat | ostida | bosilsa → M05 |

### M05 — Jonli kamera / arxiv
| Element | Joyi | Vazifasi |
|---|---|---|
| Video 16:9 (overley: nom, rejim, vaqt, DEMO) | tepa | — |
| Qora panel 1-qator: **−5 daq**, **Play/Pause**, **LIVE** (qizil, kengroq), **Snapshot**, **Fullscreen** | video ostida, teng taqsimlangan | D05 dagi kabi; Fullscreen — gorizontal rejim |
| 2-qator: tezlik çiplari 0.25×…8× | — | — |
| Timeline + soat belgilari (00, 06, 12, 18, 24) | panel pastida | bosiş/sudraş — vaqtga ötiş |
| "Şu kameradagi hodisalar" röyxati | panel ostida | bosiş — vaqtga ötiş |

### M06 — Şlagbaum
| Element | Joyi | Vazifasi |
|---|---|---|
| Kiriş kamerasi jonli (16:9) | tepa | — |
| "Holat: …" çapda, fix badge öngda | video ostida | — |
| **OÇİŞ**, **YOPİŞ** (kvadrat, yonma-yon) | ostida | bottom sheet tasdiqdan keyin buyruq |
| Natija kartasi (badge + matn) | tugmalar ostida | — |
| **Oçiq holatda fix**, **Yopiq holatda fix** (yonma-yon) | ostida | tasdiq bilan; fix faol bölsa tugma "Fixni bekor qiliş" oynasini oçadi |
| İzoh | eng pastda | — |

### M07 — Davomat
| Element | Joyi | Vazifasi |
|---|---|---|
| Çiplar soni bilan: Hammasi · Keçikkan · Kelmadi · Erta ketdi | tepa | filtr |
| "Bugun · sana · grace" | çiplar ostida | — |
| Kartalar: avatar çapda; FİŞ + "Reja … · kiriş …" ortada; holat badge öngda | ostida | — |

### M08 — Qidiruv
| Element | Joyi | Vazifasi |
|---|---|---|
| Segment: **Xodim** / **Avtomobil** (teng ikki qism) | tepa | rejim |
| Qidiruv inputi | segment ostida | jonli filtr |
| Xodim rejimi: **Yuz tasviri bilan qidiriş (kamera)** (punktir) + natijalar (avatar, FİŞ, oxirgi joy, bugungi holat badge) | ostida | natija bosilsa → M09 |
| Avtomobil rejimi: raqam plaşkasi çapda, holat badge öngda, "egasi · yönaliş vaqt" ostida + izoh | ostida | — |

### M09 — Xodimning oxirgi joyi
| Element | Joyi | Vazifasi |
|---|---|---|
| Profil kartasi: avatar (64 px) çapda; FİŞ, lavozim, bugungi holat badge | tepa | — |
| "Oxirgi aniqlangan joy": Vaqt, Kamera, Hudud, İşonçlilik + progress + "Aniq koordinata emas…" | ostida | — |
| Sariq karta + **Tasdiqlaş** (asosiy, çapda) / **Rad etiş** (öngda) | FAQAT işonçlilik < çegara | qaror saqlanadi |
| "Bugungi harakat tarixi" (vaqt, kamera, %) | ostida | — |
| "İş grafigi (bugun)" çapda, "Oylik davomat" öngda | eng pastda | — |

### M10 — Hisobotlar
| Element | Joyi | Vazifasi |
|---|---|---|
| Hisobot kartalari: nom + davr çapda; **PDF**, **Excel** öngda | röyxat | eksport, progress kartasi pastda; tayyor bölsa "Yuklab oliş / Ulaşiş" |

### M11 — Profil va sozlamalar
| Element | Joyi | Vazifasi |
|---|---|---|
| Foydalanuvçi kartasi: avatar çapda; FİŞ, rol · login, "2FA yoqilgan" badge | tepa | — |
| "Bildirişnomalar" — 4 switch (Push · Şlagbaum hodisalari · Noma'lum şaxs va avtomobil · Kamera aloqasi uzilişi); switch qator öngida | ostida | BR-MOB-04 |
| "Faol sessiya" (qurilma, vaqt) | ostida | — |
| **Tizimdan çiqiş** (qizil matn, töliq kenglik) | eng pastda | → M01 |

---

## D. TEKŞİRUV

Har bir ekran qurilgandan keyin `docs/progress.md` da şu jadvalning har bir qatori uçun `[x]` qöyiladi: element bor, joyi tögri, vazifasi işlaydi, rol çeklovi tögri. Bitta qator bajarilmasa ekran tugagan hisoblanmaydi.
