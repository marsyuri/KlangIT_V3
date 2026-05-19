# KlangIT_V3 — Architecture Overview

ระบบจัดการคลัง IT (IT asset/equipment management) — ติดตามอุปกรณ์, การยืม/คืน, สถานะของแต่ละชิ้น พร้อม audit trail แบบ ledger

> เอกสารนี้สรุประดับ architecture เพื่อให้ developer ใหม่อ่านแล้วเข้าใจระบบใน 30 นาที
> สำหรับ behavioral guideline ให้ดู [CLAUDE.md](../CLAUDE.md), สำหรับ task tracking ให้ดู [TODO.md](../TODO.md)

---

## 1. Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (net10.0) |
| ORM | Entity Framework Core 10.0.5 |
| Database (main) | SQL Server — `IT_LPT_Warehouse` |
| Database (identity) | SQL Server — `IT_LPT_Identity` (เดิมเคยเป็น SQLite, ย้ายมาแล้ว) |
| Auth | ASP.NET Core Identity + `AddDefaultIdentity<ApplicationUser>` |
| Frontend | Bootstrap 5 + design system `ds-*` (อยู่ใน [_DesignSystem.cshtml](../Views/Shared/_DesignSystem.cshtml)) |
| Locale | `th-TH` + `ThaiBuddhistCalendar` (ปี พ.ศ.) |
| Date picker | Flatpickr + `IsoDateModelBinder` (ส่ง ISO yyyy-MM-dd) |

**Global authorization is ON.** [Program.cs:26-27](../Program.cs#L26-L27) ลงทะเบียน `AuthorizeFilter` ทั่วระบบ; controller ที่ต้องเปิดสาธารณะ (เช่น [HomeController](../Controllers/HomeController.cs)) ใช้ `[AllowAnonymous]`

---

## 2. โครงสร้างโฟลเดอร์ (Project Layout)

```
KlangIT_V3/
├── Areas/Identity/Pages    # Scaffolded Identity UI (Login, Register, Manage)
├── Controllers/            # 1 controller ต่อ 1 entity
├── Data/
│   ├── ApplicationDbContext.cs       # Identity DbContext
│   ├── ApplicationUser.cs            # IdentityUser + DisplayName
│   ├── Migrations/                   # Identity DB migrations
│   └── WarehouseScripts/             # ad-hoc SQL scripts (เช่น FixUnicodeAuditColumns)
├── Helpers/                # extension methods + ModelBinder + backfill helper
├── Migrations/             # Warehouse DB migrations (single snapshot)
├── Models/                 # EF entities (partial classes) + Enums
├── Services/               # Business logic — IItemService / IBorrowService / IStockService / IDataIntegrityService
├── Validation/             # Custom ValidationAttribute (NumericOnly, FixedLength, ...)
├── ViewModels/             # DTOs สำหรับแต่ละ view/action
├── Views/                  # Razor views — 1 folder ต่อ controller
│   └── Shared/             # _Layout, _DesignSystem, _ItemStateDiagram*
├── wwwroot/                # static assets
├── Program.cs              # composition root
└── appsettings.json        # connection strings
```

**Folder conventions** (จาก [CLAUDE.md §7](../CLAUDE.md)):
- 1 controller / entity (CRUD + extra actions)
- Edit & Delete ใช้ **dedicated ViewModel** ไม่ส่ง raw Model
- Index อาจใช้ ViewBag (simple case) หรือ ViewModel (cascade dropdown)

---

## 3. Domain Model

### 3.1 Entity Diagram (logical)

```
              ┌──────────────┐         ┌──────────────┐
              │  ItemType    │◄────────┤ ItemTypeTo   │
              │  (ประเภท)    │   M2M   │   Brand      │
              └──────┬───────┘         └──────┬───────┘
                     │                        │
                     │                  ┌─────▼─────┐
                     │                  │ ItemBrand │
                     │                  │ (ยี่ห้อ)   │
                     │                  └─────┬─────┘
                     │                        │
                     │                        │ 1:N
                     │                        ▼
                     │                  ┌──────────┐
                     │                  │ ItemModel│
                     │                  │ (รุ่น)    │
                     │                  └────┬─────┘
                     │                       │
                     └──────►┌──────────────┐◄────────┘
                             │   Item       │
                             │  (อุปกรณ์)    │
                             └──┬───────┬───┘
                                │       │
                       1:N      │       │   1:N
                                ▼       ▼
                   ┌────────────────┐  ┌──────────────┐
                   │ BorrowHistory  │  │   StockLog   │
                   │ (การยืม/คืน)    │  │ (ledger)     │
                   └────────┬───────┘  └──────────────┘
                            │
                            │ FK
                  ┌─────────┴───────────┐
                  ▼                     ▼
            ┌──────────┐          ┌──────────┐
            │Department│──1:N────►│ Section  │
            │ (ฝ่าย)    │          │ (หน่วยงาน)│
            └──────────┘          └──────────┘
```

### 3.2 Item (อุปกรณ์) — core entity

[Item.cs](../Models/Item.cs) — เป็น "stock card" ของอุปกรณ์ 1 รุ่น ไม่ใช่ 1 ชิ้น

| Field | ความหมาย |
|---|---|
| `AssetId` | รหัสทรัพย์สิน (composed จาก `AssetId1-4` + dash หรือ `OtherAssetId`) |
| `IsBulk` | ถ้า true หมายถึงอุปกรณ์รวมกลุ่ม (เช่น สาย LAN 100 เส้น) ใช้ `TotalAmount > 1` |
| `TotalAmount` | = Active + Disposed |
| `ActiveAmount` | = Available + Borrowed + Damaged |
| `AvailableAmount` | พร้อมใช้งาน |
| `BorrowedAmount` | ถูกยืมอยู่ |
| `DamagedAmount` | ชำรุด รอซ่อม |
| `DisposedAmount` | จำหน่ายแล้ว |
| `MinimumAmount` | จำนวนขั้นต่ำที่ต้องมี (warning threshold) |
| `ItemStatus` | enum (1=Available, 2=Borrowed, 3=Damaged, 4=Disposed) |

**Invariants** (enforced by [StockService](../Services/StockService.cs)):
- `Total = Available + Borrowed + Damaged + Disposed`
- `Active = Available + Borrowed + Damaged`
- ทุก amount ต้อง ≥ 0 (ถ้าติดลบ → throw `InvalidOperationException`)

### 3.3 BorrowHistory (ประวัติการยืม)

[BorrowHistory.cs](../Models/BorrowHistory.cs)

- `BorrowDate` / `ReturnDate` — ถ้า `ReturnDate` null = ยังยืมอยู่
- `IsPermanentBorrow` — ยืมถาวร (ไม่ต้องคืน) → จะถูก clear เมื่อมีการ return
- `IsInitial` — flag สำหรับ data migration (ปกติ false)
- `Amount` — จำนวนที่ยืม (รองรับยืม bulk บางส่วน) — จะลดลงเมื่อ return บางส่วน
- `BorrowItname` / `ReturnItname` — display name ของ IT staff ที่ทำรายการ

### 3.4 StockLog (audit ledger)

[StockLog.cs](../Models/StockLog.cs) — **append-only ledger** ของทุกการเปลี่ยน amount ใน Item

- บันทึก `Delta*` (การเปลี่ยนแปลง) + `*After` (snapshot หลังจาก apply)
- มี `LogType` (10 ค่า — ดู [StockLogTypeEnum.cs](../Models/Enums/StockLogTypeEnum.cs)): Initial*, Borrow, Return, Damage, Repair, Dispose, Adjust
- **Continuity invariant**: `prev.AvailableAfter + log.DeltaAvailable == log.AvailableAfter` (และอีก 3 fields)
  → ตรวจด้วย [DataIntegrityService.RunChecksAsync](../Services/DataIntegrityService.cs)
- `LogType ≤ 4` = Initial state, `LogType > 4` = transactional → ใช้แยกตอน `ResetToInitial`

### 3.5 Master Data (soft delete)

- `ItemType` / `ItemBrand` / `ItemModel` / `Department` / `Section`
- ทุกตัวมี `Name`, `OrderNo`, audit fields, `IsDeleted` flag
- Soft delete ผ่าน `IsDeleted = true` ไม่ลบจริง
- `ItemTypeToBrand` = junction table (M:N ระหว่าง Type ↔ Brand) → สร้าง cascade dropdown

---

## 4. Composition Root ([Program.cs](../Program.cs))

```csharp
// 2 DbContext แยกกัน
AddDbContext<ApplicationDbContext>     // Identity (IT_LPT_Identity)
AddDbContext<ItLptWarehouseContext>    // Domain (IT_LPT_Warehouse)

// Identity + global authorize filter
AddDefaultIdentity<ApplicationUser>
AddControllersWithViews(opt => opt.Filters.Add(new AuthorizeFilter()))

// DI registrations (Scoped per-request)
IStockService          → StockService
IItemService           → ItemService
IBorrowService         → BorrowService
IDataIntegrityService  → DataIntegrityService

// Thai Buddhist locale (พ.ศ. ทั่ว UI)
UseRequestLocalization(thaiCulture with ThaiBuddhistCalendar)
```

---

## 5. Service Layer (business logic)

Controllers **ไม่แตะ DbContext** โดยตรง — ทุก mutation ผ่าน service layer
**Pattern**: Controller จัดการ UI concerns (ModelState, dropdown, image upload) → ส่งงานให้ service → service ทำ DB work + business rule

### 5.1 [IStockService](../Services/IStockService.cs) — single mutation point

```csharp
ApplyStockChangeAsync(itemId, logType, deltaAvail, deltaBorr, deltaDam, deltaDisp, ...)
```

ทุก operation ที่เปลี่ยน Item.Amount **ต้อง** ผ่าน method นี้ เพื่อให้:
1. Item.Amount และ StockLog sync กันตลอด (atomic via transaction)
2. ตรวจ "amount ติดลบ" ในที่เดียว
3. คำนวณ Active/Total อัตโนมัติจาก components

### 5.2 [IItemService](../Services/IItemService.cs)

- `CreateItemAsync` — สร้าง Item + ลง `Initial*` StockLog ตาม `selectedStatus`
- `MarkDamagedAsync` / `MarkRepairedAsync` — ย้ายจำนวนระหว่าง Available ↔ Damaged
- `DeleteItemAsync` — hard delete; คืน `DeleteOutcome` enum (`Deleted`/`NotFound`/`HasDependencies`) เพื่อให้ controller redirect ได้
- `GetCascadeMapsAsync` — สร้าง 3 dictionaries สำหรับ client-side cascade dropdown (Type↔Brand, Brand↔Models)

### 5.3 [IBorrowService](../Services/IBorrowService.cs)

- `CreateBorrowAsync` — insert BH + เรียก `StockService` (Available → Borrowed) + set `Item.ItemStatus = Borrowed`
- `ProcessReturnAsync` — รองรับ **partial return**:
  - ถ้า `returnAmount < bh.Amount` → ลด `bh.Amount` เท่านั้น (BH ยังเปิด)
  - ถ้า `returnAmount == bh.Amount` → set `ReturnDate`, BH ปิด
  - กรณีไหนก็ตาม `IsPermanentBorrow` จะถูก clear

### 5.4 [IDataIntegrityService](../Services/IDataIntegrityService.cs) — admin diagnostics

`RunChecksAsync` ตรวจ 5 ประเภท:
1. **Internal math** — `Active != Avail+Borr+Dam` หรือ `Total != Active+Disp`
2. **Negative amounts** — มี field ใดติดลบ
3. **Amount vs latest StockLog** — Item.Amount ไม่ตรงกับ row ล่าสุดใน StockLog
4. **BorrowedAmount vs active BH** — `Item.BorrowedAmount != SUM(active BH amount)`
5. **StockLog continuity** — `prev.After + curr.Delta != curr.After`

`ResetToInitialAsync` — admin recovery tool:
1. ลบ BorrowHistory ทั้งหมด
2. ลบ StockLog ที่ `LogType > 4` (เก็บไว้เฉพาะ Initial*)
3. Recompute `Item.Amounts` จาก StockLog row ล่าสุด (= Initial row)
4. (optional) Pattern B: ย้าย orphan `BorrowedAmount` → `AvailableAmount` พร้อมลง `Adjust` log

---

## 6. Key Flows

### 6.1 Borrow flow (ยืม)

```
User คลิก "ยืม" ใน Item Details
   ↓
GET  Items/Details/5 → คลิกไป BorrowHistories/Create?itemId=5
   ↓
BorrowHistoriesController.Create(itemId)
   - ดึง Item.WithRelations()
   - สร้าง BorrowCreateViewModel + Itstaff = current user display name
   - PopulateBorrowCreateDropdownsAsync (Department, Section)
   ↓
POST BorrowHistories/Create
   - ModelState validation
   - BorrowService.CreateBorrowAsync()
       │
       ├─► INSERT BorrowHistory (BorrowItname = staff display, Amount = N)
       ├─► Item.ItemStatus = Borrowed
       └─► StockService.ApplyStockChangeAsync(deltaAvail=-N, deltaBorr=+N, ref="BH-{id}")
              ├─► UPDATE Item (Available -= N, Borrowed += N)
              └─► INSERT StockLog (LogType=Borrow)
   ↓
Redirect → Items/Details/5
```

### 6.2 Return flow (คืน)

```
GET  BorrowHistories/Return/{bhId}
   - ดึง active BH (ReturnDate IS NULL, IsDeleted=false)
   ↓
POST BorrowHistories/Return
   - BorrowService.ProcessReturnAsync(bhId, returnAmount, ...)
       │
       ├─► IF returnAmount == bh.Amount → set ReturnDate = NOW (BH closed)
       │   ELSE                          → bh.Amount -= returnAmount (BH ยังเปิด)
       ├─► Item.ItemStatus = Available
       └─► StockService.ApplyStockChangeAsync(deltaAvail=+R, deltaBorr=-R, ref="BH-{id}")
```

### 6.3 Damage / Repair flow

ทำคนละ action บน `ItemsController` (ไม่ใช่ผ่าน BorrowHistory):
- `Damaged`: Available → Damaged + ตั้ง `ItemStatus = Damaged`
- `Repair` : Damaged → Available + ตั้ง `ItemStatus = Available`

### 6.4 Item state diagram

ดู visual diagram ใน [Views/Shared/_ItemStateDiagram.cshtml](../Views/Shared/_ItemStateDiagram.cshtml) (แสดงผ่าน [Items/ItemStates](../Controllers/ItemsController.cs#L333))

```
              ┌──────────┐  Borrow   ┌──────────┐
              │Available │──────────►│ Borrowed │
              │          │◄──────────│          │
              └────┬─────┘  Return   └──────────┘
                   │ ▲
            Damage │ │ Repair
                   ▼ │
              ┌──────┴───┐
              │ Damaged  │
              └────┬─────┘
                   │ Dispose
                   ▼
              ┌──────────┐
              │ Disposed │  (terminal)
              └──────────┘
```

---

## 7. ViewModel Patterns

### 7.1 Create / Edit ViewModels

มีโครงสร้างคล้ายกัน:
- field สำหรับ form binding (input)
- `SelectedXxxId` (selected dropdown value)
- `Xxxs` collection (`List<SelectListItem>`) สำหรับ render `<select>`
- `XxxToYyyMap` (`Dictionary<int, List<int>>`) สำหรับ client-side cascade

ตัวอย่าง: [ItemCreateViewModel](../ViewModels/ItemCreateViewModel.cs)

### 7.2 Delete ViewModel

มี dependency-check field เช่น `BorrowHistoryCount`, `HasActiveBorrow` เพื่อแสดง UI block delete + show why

### 7.3 Details ViewModel

แสดงผล read-only + nested collection (`BHinItemDetails`, `StockTimeline`) — ใช้ใน Items/Details

### 7.4 Naming conventions

| Suffix | Purpose |
|---|---|
| `*CreateViewModel` | สำหรับ POST Create |
| `*EditViewModel` | สำหรับ POST Edit (อาจมี hidden `Id`) |
| `*DeleteViewModel` | confirmation page ก่อนลบ |
| `*ViewModel` | index/list view |
| `*DetailsViewModel` | details (read-only) |

---

## 8. Helpers

| File | ใช้ทำอะไร |
|---|---|
| [ClaimsPrincipalExtensions](../Helpers/ClaimsPrincipalExtensions.cs) | `GetUsernameLocalPart()` ตัด `@domain` ออก, `GetDisplayNameAsync()` ดึง `ApplicationUser.DisplayName` |
| [EnumExtensions](../Helpers/EnumExtensions.cs) | `GetDisplayName()` อ่าน `[Display(Name=...)]` attribute (ค่า Thai) |
| [IsoDateModelBinder](../Helpers/IsoDateModelBinder.cs) | bind input `yyyy-MM-dd` (จาก Flatpickr) เป็น `DateTime` โดยไม่ผ่าน Thai Buddhist year — แก้บั๊กเลขปี 543 |
| [StockLogBackfillHelper](../Helpers/StockLogBackfillHelper.cs) | one-time admin script: สำหรับ item ที่ยังไม่มี StockLog (legacy data) ให้สร้าง `Initial*` log ตาม amount ปัจจุบัน |

---

## 9. Validation (custom attributes)

| Attribute | Behavior |
|---|---|
| [NumericOnly](../Validation/NumericOnlyAttribute.cs) | ตัวเลขล้วน — server + client validation (`data-val-numericonly`) |
| [NumericFixedLength](../Validation/NumericFixedLengthAttribute.cs) | ตัวเลขล้วน + ความยาวที่กำหนด |
| [FixedLengthIfProvided](../Validation/FixedLengthIfProvidedAttribute.cs) | ถ้ามีค่า ต้องยาว N ตัว (allow null/empty) |

---

## 10. Admin Tools ([AdminController](../Controllers/AdminController.cs))

| Action | Purpose | Status |
|---|---|---|
| `BackfillStockLog` | สร้าง opening balance log สำหรับ legacy items | one-time helper |
| `DataIntegrityCheck` | รัน 5 checks → display report | safe to run anytime |
| `ResetToInitial` | ลบ transactional data ทั้งหมด กลับสู่ initial state | **destructive** — require confirm checkbox |

> Note: ยังไม่มี `[Authorize(Roles = "Admin")]` — มี TODO comment ใน controller รออยู่ ([AdminController.cs:8](../Controllers/AdminController.cs#L8))

---

## 11. Locale & Date Handling

**ความท้าทาย**: ระบบใช้ Thai Buddhist calendar (พ.ศ. = ค.ศ. + 543) → ปฏิทินใน UI ต้องโชว์ปี พ.ศ. แต่ database เก็บปี ค.ศ.

**แนวทาง**:
1. `app.UseRequestLocalization` → set culture เป็น `th-TH` + `ThaiBuddhistCalendar`
2. Form ใช้ **Flatpickr** (JS date picker) ส่งค่าเป็น ISO `yyyy-MM-dd` (ค.ศ.)
3. `IsoDateModelBinder` รับค่า ISO โดย bypass culture-aware parsing → ป้องกันบั๊กปี 2569 → 2026
4. Display ใน Razor view ใช้ `@DateTime.ToString()` ปกติ — culture จัดการการแสดงเป็น พ.ศ. ให้

ดู commit `a6eae76` (fix(date)) สำหรับ context

---

## 12. Design System (`ds-*` CSS)

อยู่ที่ [_DesignSystem.cshtml](../Views/Shared/_DesignSystem.cshtml) (rendered via `_Layout.cshtml`)

**Required classes** (จาก [CLAUDE.md §9](../CLAUDE.md)):
- Layout: `ds-card`, `ds-form-row`, `ds-table-wrap`, `ds-filter-bar`
- Buttons: `ds-btn-primary`, `ds-btn-ghost`, `ds-btn-danger`
- Inputs: `ds-input`, `ds-select`, `ds-input-ro`

**Status badges** — currently used **only ใน form (Create/Edit/Return)** เป็น read-only indicator
**ไม่ใช้แล้ว** ใน table/list views (Index pages ใช้ plain text เช่น `"คืนแล้ว" / "กำลังยืม"`)

---

## 13. Database Migrations

**2 contexts → 2 migration sets**:

| Context | Path | DB |
|---|---|---|
| `ApplicationDbContext` (Identity) | [Data/Migrations/](../Data/Migrations/) | `IT_LPT_Identity` |
| `ItLptWarehouseContext` (domain) | [Migrations/](../Migrations/) | `IT_LPT_Warehouse` |

```bash
# Identity
dotnet ef migrations add <name> --context ApplicationDbContext --output-dir Data/Migrations
dotnet ef database update --context ApplicationDbContext

# Warehouse
dotnet ef migrations add <name> --context ItLptWarehouseContext --output-dir Migrations
dotnet ef database update --context ItLptWarehouseContext
```

Ad-hoc fix scripts อยู่ที่ [Data/WarehouseScripts/](../Data/WarehouseScripts/) (เช่น `FixUnicodeAuditColumns.sql` แก้ collation บน CreatedBy/ModifiedBy ที่เป็น `IsUnicode(false)`)

---

## 14. Conventions ที่ควรจำ

จาก [CLAUDE.md](../CLAUDE.md) — **อ่านไฟล์นั้นก่อน contribute**

- **Soft delete** สำหรับ master data (Type/Brand/Model/Department/Section), **hard delete** สำหรับ Item & BorrowHistory (แต่ Item delete มี dependency check)
- **Audit fields** ทุก entity: `Created/ModifiedDate`, `Created/ModifiedBy`, `IsDeleted` — ใช้ `User.GetUsernameLocalPart()` populate
- **Cascade dropdown** สร้างจาก JSON map ฝั่ง client (ไม่ AJAX) — ดู `Index.cshtml` ของ [Items](../Views/Items/Index.cshtml)
- **Image upload**: เก็บใน `wwwroot/uploads/items/`, filename = `IT{yyyyMMddHHmm}_{count:00000}.jpg`, max 2MB, allowed `.jpg/.jpeg/.png/.gif/.webp` ([ItemsController.SaveImageAsync](../Controllers/ItemsController.cs#L452))

---

## 15. ลำดับอ่านโค้ดสำหรับ developer ใหม่

1. [Program.cs](../Program.cs) — DI + middleware
2. [Models/Item.cs](../Models/Item.cs) + [Models/StockLog.cs](../Models/StockLog.cs) — core entities
3. [Services/IStockService.cs](../Services/IStockService.cs) → [StockService.cs](../Services/StockService.cs) — เห็น invariant model
4. [Services/BorrowService.cs](../Services/BorrowService.cs) — flow ยืม/คืน
5. [Controllers/ItemsController.cs](../Controllers/ItemsController.cs) — controller pattern ครบทุก action
6. [Views/Items/Index.cshtml](../Views/Items/Index.cshtml) — ds-class + cascade dropdown
7. [CLAUDE.md](../CLAUDE.md) — behavioral rules
8. [TODO.md](../TODO.md) — งานคงค้าง

---

**Last updated**: 2026-05-19 (generated)
**Maintainer**: ดู git log สำหรับ active contributors
