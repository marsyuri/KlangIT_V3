---
title: "KlangIT_V3 — Project Guide for New Programmers"
subtitle: "เอกสารแนะนำระบบจัดการคลัง IT แบบละเอียด"
date: "2026-05-19"
---

# KlangIT_V3 — Project Guide for New Programmers

เอกสารฉบับนี้เขียนไว้สำหรับ programmer ใหม่ที่เพิ่งเข้ามาในทีม
อ่านจบแล้วควรเข้าใจระบบในระดับที่สามารถแก้บั๊ก / เพิ่ม feature ขนาดเล็กได้
ไม่จำเป็นต้องอ่านเรียงลำดับ — section 1–3 จำเป็น ที่เหลือใช้เป็น reference

---

# 1. ภาพรวม (Overview)

## 1.1 ระบบนี้ทำอะไร

KlangIT_V3 = **ระบบจัดการคลัง IT** (IT Asset/Equipment Warehouse) ของหน่วยงานหนึ่ง
หน้าที่หลัก 4 อย่าง:

1. **ทะเบียนทรัพย์สิน (Asset register)** — บันทึกอุปกรณ์ IT ทุกชิ้น พร้อมรหัสครุภัณฑ์ ยี่ห้อ รุ่น จำนวน
2. **การยืม-คืน (Borrow/Return)** — บันทึกว่าใครยืมอุปกรณ์อะไรไป คืนเมื่อไหร่ ยืมถาวรไหม
3. **สถานะอุปกรณ์ (Item state)** — ติดตามจำนวนที่พร้อมใช้ / ถูกยืม / ชำรุด / จำหน่ายแล้ว
4. **Audit trail** — ทุกการเปลี่ยน amount ของอุปกรณ์ถูกบันทึกใน StockLog เพื่อตรวจสอบย้อนหลังได้

## 1.2 ใครคือ user

- **เจ้าหน้าที่ IT (IT staff)** — primary user; ทำรายการยืม/คืน/แจ้งชำรุด/ซ่อม
- **ผู้ดูแลข้อมูล (Admin)** — ดูแล master data, รัน data integrity check, reset ข้อมูล
- ยังไม่มี end-user-facing portal (เช่นพนักงานคนอื่นในหน่วยงานยืมเอง) — รายการยืมทั้งหมดต้องผ่าน IT staff

## 1.3 ภาษา

- **UI ทั้งหมดเป็นภาษาไทย** — labels, button text, validation messages, page titles
- **โค้ดเป็นภาษาอังกฤษ** — class/method/variable names, XML docs
- **comment ในโค้ดผสม** — เทคนิคใช้อังกฤษ, business rule ใช้ไทย

## 1.4 Calendar

ใช้ **พุทธศักราช (พ.ศ.)** ใน UI ทั้งหมด — ปี 2569 = ค.ศ. 2026
แต่ **database เก็บเป็น ค.ศ.** เสมอ
การแปลงทำผ่าน `ThaiBuddhistCalendar` ที่ตั้งไว้ใน `Program.cs`

---

# 2. Tech Stack

## 2.1 รายการเทคโนโลยี

| Layer | Technology | Version | Notes |
|---|---|---|---|
| Framework | ASP.NET Core MVC | net10.0 | ใช้ controller-based (ไม่ใช่ Razor Pages, ไม่ใช่ Minimal API) |
| ORM | Entity Framework Core | 10.0.5 | ใช้แบบ DbContext + DbSet ตรงไปตรงมา |
| DB engine (domain) | SQL Server | — | DB: `IT_LPT_Warehouse` |
| DB engine (auth) | SQL Server | — | DB: `IT_LPT_Identity` (เดิมเคยใช้ SQLite, ปัจจุบันย้ายมา SQL Server แล้ว) |
| Authentication | ASP.NET Core Identity | 10.0.5 | `AddDefaultIdentity<ApplicationUser>` + scaffolded UI |
| Frontend | Bootstrap 5 + custom `ds-*` design system | — | ไม่มี SPA framework |
| Date picker | Flatpickr | — | ทำงานคู่กับ `IsoDateModelBinder` |
| JSON serializer | Newtonsoft.Json | — | ใช้ในการ serialize cascade maps ไปยัง view |
| Validation (client) | jQuery Validation Unobtrusive | — | built-in กับ ASP.NET MVC |

## 2.2 NuGet packages (จาก `KlangIT_V3.csproj`)

```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.5" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.5" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="10.0.2" />
```

> **หมายเหตุ**: `Sqlite` package ยังอยู่จากตอนที่ Identity DB เป็น SQLite — ไม่ได้ใช้แล้ว แต่ยังไม่ถอนออก

## 2.3 ห้ามเพิ่ม library ใหม่โดยไม่ถาม

ดู [CLAUDE.md §11](../CLAUDE.md) — โครงการนี้ใช้ EF Core, Bootstrap, jQuery validation, Newtonsoft.Json ครบ และพยายามไม่เพิ่ม dependency

---

# 3. โครงสร้างโฟลเดอร์ (Project Layout)

```
KlangIT_V3/
├── Areas/                          # ASP.NET Core Areas
│   └── Identity/Pages/             # Scaffolded Identity UI (Login, Register, ManageAccount)
├── Controllers/                    # 1 controller ต่อ 1 entity
│   ├── AdminController.cs          # Admin tools — Backfill, IntegrityCheck, ResetToInitial
│   ├── BorrowHistoriesController.cs# ยืม/คืน/แก้ไข/ลบ borrow record
│   ├── DepartmentsController.cs    # CRUD ฝ่าย
│   ├── HomeController.cs           # หน้าแรก + Privacy + Error (มี [AllowAnonymous])
│   ├── ItemBrandsController.cs     # CRUD ยี่ห้อ
│   ├── ItemModelsController.cs     # CRUD รุ่น (FK → ItemBrand)
│   ├── ItemsController.cs          # CRUD อุปกรณ์ + Damaged/Repair actions
│   ├── ItemTypeToBrandsController.cs # M2M junction (Type ↔ Brand)
│   ├── ItemTypesController.cs      # CRUD ประเภท
│   └── SectionsController.cs       # CRUD หน่วยงาน (FK → Department)
├── Data/
│   ├── ApplicationDbContext.cs     # Identity DbContext
│   ├── ApplicationUser.cs          # IdentityUser + DisplayName
│   ├── Migrations/                 # Identity DB migrations
│   └── WarehouseScripts/
│       └── FixUnicodeAuditColumns.sql  # ad-hoc fix script
├── Helpers/
│   ├── ClaimsPrincipalExtensions.cs # User.GetUsernameLocalPart() etc.
│   ├── EnumExtensions.cs           # GetDisplayName() อ่าน [Display(Name=...)]
│   ├── IsoDateModelBinder.cs       # bind yyyy-MM-dd → DateTime โดยไม่ผ่าน Thai BE
│   └── StockLogBackfillHelper.cs   # one-time admin script
├── Migrations/                     # Warehouse DB migrations
├── Models/
│   ├── Enums/
│   │   ├── ItemStatusEnum.cs       # Available / Borrowed / Damaged / Disposed
│   │   └── StockLogTypeEnum.cs     # 10 log types
│   ├── BorrowHistory.cs            # ประวัติยืม-คืน
│   ├── Department.cs               # ฝ่าย
│   ├── Item.cs                     # อุปกรณ์ (entity หลัก)
│   ├── ItemBrand.cs                # ยี่ห้อ
│   ├── ItemModel.cs                # รุ่น
│   ├── ItemStatus.cs               # (legacy table — ปัจจุบันใช้ enum)
│   ├── ItemType.cs                 # ประเภทอุปกรณ์
│   ├── ItemTypeToBrand.cs          # junction Type ↔ Brand
│   ├── ItLptWarehouseContext.cs    # Warehouse DbContext
│   ├── Section.cs                  # หน่วยงาน
│   └── StockLog.cs                 # ledger of stock changes
├── Properties/                     # launchSettings.json (ports etc.)
├── Services/                       # Business logic layer
│   ├── IBorrowService.cs / BorrowService.cs
│   ├── IDataIntegrityService.cs / DataIntegrityService.cs
│   ├── IItemService.cs / ItemService.cs
│   └── IStockService.cs / StockService.cs
├── Validation/                     # Custom validation attributes
│   ├── FixedLengthIfProvidedAttribute.cs
│   ├── NumericFixedLengthAttribute.cs
│   └── NumericOnlyAttribute.cs
├── ViewModels/                     # DTOs per controller action
│   ├── BorrowCreateViewModel.cs
│   ├── BorrowHistoryEditViewModel.cs
│   ├── BorrowInItemDetailViewModel.cs
│   ├── DeleteViewModels.cs         # *ทั้ง DeleteVM อยู่ในไฟล์เดียว
│   ├── DepartmentEditViewModel.cs / DepartmentViewModel.cs
│   ├── ItemBrandEditViewModel.cs / ItemBrandViewModel.cs
│   ├── ItemCreateViewModel.cs / ItemEditViewModel.cs
│   ├── ItemDamagedViewModel.cs / ItemRepairViewModel.cs
│   ├── ItemDetailViewModel.cs / ItemDetailsViewModel.cs  # 2 ไฟล์คนละ purpose (ตรวจให้ดี!)
│   ├── ItemIndexViewModel.cs
│   ├── ItemModelEditViewModel.cs / ItemModelViewModel.cs
│   ├── ItemTypeEditViewModel.cs / ItemTypeViewModel.cs
│   ├── ItemTypeToBrandEditViewModel.cs / ItemTypeToBrandViewModel.cs
│   ├── ReturnCreateViewModel.cs
│   └── SectionEditViewModel.cs / SectionViewModel.cs
├── Views/
│   ├── Admin/ Items/ BorrowHistories/ Departments/ ...  # 1 folder ต่อ controller
│   └── Shared/
│       ├── _DesignSystem.cshtml    # custom ds-* CSS
│       ├── _ItemStateDiagram.cshtml + 2 SVG variants
│       ├── _Layout.cshtml + _Layout.cshtml.css
│       ├── _LoginPartial.cshtml    # navbar login section
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
├── wwwroot/                        # static assets (CSS, JS, images, uploads)
├── appsettings.json                # connection strings + logging config
├── appsettings.Development.json
├── CLAUDE.md                       # AI assistant behavioral guide
├── KlangIT_V3.csproj
├── KlangIT_V3.slnx                 # solution file
├── Program.cs                      # composition root + middleware
└── TODO.md                         # known bugs/features/tech debt (BUG-XXX, FEAT-XXX, etc.)
```

### 3.1 Folder conventions (สำคัญ)

- **1 controller ต่อ 1 entity** — มี action standard 5 ตัว: Index, Details, Create, Edit, Delete + extra actions ตามต้องการ
- **Edit & Delete ใช้ dedicated ViewModel** ไม่ส่ง raw Model
- **Index** อาจใช้ ViewBag (กรณี simple) หรือ ViewModel (กรณีมี cascade dropdown)

---

# 4. Bootstrap (`Program.cs`)

ไฟล์ [Program.cs](../Program.cs) เป็น composition root — กำหนดทุกอย่างที่ระบบต้องใช้

## 4.1 ลำดับการตั้งค่า

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1) Register 2 DbContext (Identity + Domain)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddDbContext<ItLptWarehouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WarehouseConnection")));

// 2) Migration error page (dev only)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 3) Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 4) MVC + global authorization
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AuthorizeFilter()));

// 5) DI registrations (Scoped per-request)
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IDataIntegrityService, DataIntegrityService>();

var app = builder.Build();
```

## 4.2 Middleware pipeline

```csharp
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();   // /ApplyDatabaseMigrations page เมื่อ pending migration
else
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

app.UseHttpsRedirection();
app.MapStaticAssets();             // serve wwwroot + manifest

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();               // for Identity area scaffolded pages

// Thai Buddhist locale
var thaiCulture = new CultureInfo("th-TH");
thaiCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture(thaiCulture),
    SupportedCultures   = new[] { thaiCulture },
    SupportedUICultures = new[] { thaiCulture }
});

app.UseRouting();
app.UseAuthorization();
app.Run();
```

## 4.3 ข้อสังเกตที่ต้องเข้าใจ

1. **`AddControllersWithViews` ใส่ `AuthorizeFilter()` แบบ global** → ทุก controller ต้อง login ก่อนเสมอ
   - หน้าที่เปิดให้สาธารณะ (เช่น `HomeController`) ต้องใส่ `[AllowAnonymous]` เอง
   - หน้า Login/Register ของ Identity area เปิดสาธารณะอยู่แล้วโดย default

2. **มี 2 DbContext** → 2 connection string → 2 database
   - ทำให้ migration ต้องระบุ `--context` เสมอ
   - User credentials กับ business data แยกกัน → backup/restore ง่ายขึ้น

3. **`SignIn.RequireConfirmedAccount = true`** → user ใหม่ต้องยืนยัน email ก่อน login
   - ใน dev environment อาจปิดได้เพื่อทดสอบ แต่ production ต้องเปิด

4. **Locale ตั้งหลัง `MapControllerRoute`** — order ตรงนี้สำคัญ (`UseRequestLocalization` ต้องอยู่ก่อน routing/authorization)

5. **ไม่มี `AddMemoryCache` / `AddHttpClient` / `AddCors`** — ระบบเรียบ ไม่มี API endpoint ภายนอก

---

# 5. Database & 2-Context Strategy

## 5.1 สอง database แยกกัน

### `IT_LPT_Identity`
- เก็บข้อมูล user account (ASP.NET Core Identity tables: AspNetUsers, AspNetRoles, AspNetUserRoles, ...)
- จัดการโดย `ApplicationDbContext` ที่อยู่ใน [Data/ApplicationDbContext.cs](../Data/ApplicationDbContext.cs)
- Migration อยู่ใน [Data/Migrations/](../Data/Migrations/)

### `IT_LPT_Warehouse`
- เก็บข้อมูล business: Item, BorrowHistory, StockLog, Department, Section, ItemType, ItemBrand, ItemModel, ItemTypeToBrand
- จัดการโดย `ItLptWarehouseContext` ที่อยู่ใน [Models/ItLptWarehouseContext.cs](../Models/ItLptWarehouseContext.cs)
- Migration อยู่ใน [Migrations/](../Migrations/)

## 5.2 Connection strings

ใน [appsettings.json](../appsettings.json):

```json
{
  "ConnectionStrings": {
    "IdentityConnection":  "Server=NATTLWATTF086\\SQLEXPRESS;Database=IT_LPT_Identity;Trusted_Connection=True;TrustServerCertificate=True",
    "WarehouseConnection": "Server=NATTLWATTF086\\SQLEXPRESS;Database=IT_LPT_Warehouse;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

> **หมายเหตุ**: ใน dev environment ให้ override ที่ `appsettings.Development.json` หรือใช้ `dotnet user-secrets` (มี `UserSecretsId` ใน csproj ไว้แล้ว)

## 5.3 EF Core migration commands

```bash
# Identity DB
dotnet ef migrations add <name> --context ApplicationDbContext --output-dir Data/Migrations
dotnet ef database update --context ApplicationDbContext

# Warehouse DB
dotnet ef migrations add <name> --context ItLptWarehouseContext --output-dir Migrations
dotnet ef database update --context ItLptWarehouseContext
```

## 5.4 `ItLptWarehouseContext` รายละเอียด

ไฟล์ [Models/ItLptWarehouseContext.cs](../Models/ItLptWarehouseContext.cs) — เป็น **partial class** (เปิดให้ extend ได้)

### DbSets

```csharp
DbSet<BorrowHistory>    BorrowHistories
DbSet<Department>       Departments
DbSet<Item>             Items
DbSet<ItemBrand>        ItemBrands
DbSet<ItemModel>        ItemModels
DbSet<ItemType>         ItemTypes
DbSet<ItemTypeToBrand>  ItemTypeToBrands
DbSet<Section>          Sections
DbSet<StockLog>         StockLogs
```

### Configuration หลัก (จาก `OnModelCreating`)

| Entity | Special config |
|---|---|
| ทุก entity | audit column `CreatedBy/ModifiedBy` ใช้ `IsUnicode(false)` (ASCII only — ดู `FixUnicodeAuditColumns.sql` ถ้ามีปัญหา collation) |
| ทุก entity ที่มี `OrderNo` | default value 1 (named constraint `DF_{Table}_OrderNo`) |
| `Item` | indexes: `AssetId` (filter `IS NOT NULL AND IsDeleted=0`), `SerialNo` (เหมือนกัน), `ItemBrandId`, `ItemTypeId`, `(ItemStatus, IsDeleted)` |
| `Item.CreatedDate / ModifiedDate` | default `getdate()` (server-side) |
| `BorrowHistory.BorrowItname` | column ชื่อ `BorrowITName` ใน DB (case-sensitive — สำคัญถ้าทำ raw SQL) |
| `BorrowHistory.ReturnItname` | column ชื่อ `ReturnITName` ใน DB (เหมือนกัน) |
| `StockLog` | indexes: `(ItemId, CreatedDate DESC)`, `LogType`, `ReferenceNo` (filter `IS NOT NULL`) |
| FK relationships | ส่วนใหญ่ใช้ `OnDelete(ClientSetNull)` → ถ้าลบ parent EF จะ set FK เป็น null (ไม่ cascade delete) |

### ทำไม `OnDelete(ClientSetNull)`?

ป้องกัน accidental cascade delete — เช่นถ้าลบ `ItemBrand` แล้ว `Item` ที่อ้างอิงต้องไม่หายตามไปด้วย → controller ต้องตรวจ dependency เองก่อนลบ

---

# 6. Models (Entities)

ทุก entity เป็น **partial class** เพื่อให้ scaffold tool generate ทับได้ และเราเพิ่ม method ในไฟล์อื่นได้

## 6.1 Audit fields (ทุก entity ที่ต้องตามรอย)

```csharp
public DateTime CreatedDate  { get; set; }
public DateTime ModifiedDate { get; set; }
public string   CreatedBy    { get; set; } = null!;
public string   ModifiedBy   { get; set; } = null!;
public bool     IsDeleted    { get; set; }
```

**กฎ** (ดู [CLAUDE.md §8](../CLAUDE.md)):
- ตอน INSERT: set ทั้ง `CreatedDate/By` และ `ModifiedDate/By` พร้อมกัน
- ตอน UPDATE: update เฉพาะ `ModifiedDate/By`
- ค่ามาจาก `User.GetUsernameLocalPart()` → ตัด `@domain` ออก เก็บเฉพาะส่วนหน้า (เช่น `tont` จาก `tont@example.com`)

## 6.2 `Item` (อุปกรณ์) — entity หลัก

ไฟล์ [Models/Item.cs](../Models/Item.cs)

### Fields และความหมาย

| Field | Type | Meaning |
|---|---|---|
| `Id` | int | PK |
| `OrderNo` | int | ลำดับ (default 1) — ใช้สำหรับ manual sort ถ้าต้องการ |
| `AssetId` | string? | **รหัสครุภัณฑ์ที่ประกอบจาก 4 ส่วน** เช่น `1234-12345678-12345678-12345` หรือ "Other" |
| `AssetId1`–`AssetId4` | string? | 4 ส่วนของ AssetId (4 / 8 / 8 / 5 หลัก) |
| `OtherAssetId` | string? | ใช้กรณีไม่มี AssetId เป็นรหัสมาตรฐาน |
| `SerialNo` | string? | serial number ของอุปกรณ์ |
| `ItemTypeId` | int (FK) | ประเภทอุปกรณ์ |
| `ItemBrandId` | int (FK) | ยี่ห้อ |
| `ItemModelId` | int? (FK) | รุ่น (nullable) |
| `ItemDescription` | string? | คำอธิบายอุปกรณ์ |
| `ItemImageUrl` | string? | path ของรูป (เก็บใน `/wwwroot/uploads/items/`) |
| `IsBulk` | bool | `true` = อุปกรณ์รวมกลุ่ม (เช่น สาย LAN 100 เส้น) — `TotalAmount > 1` ได้ |
| `TotalAmount` | int | = `ActiveAmount + DisposedAmount` |
| `ActiveAmount` | int | = `AvailableAmount + BorrowedAmount + DamagedAmount` |
| `AvailableAmount` | int | จำนวนพร้อมใช้งาน |
| `BorrowedAmount` | int | จำนวนที่ถูกยืมอยู่ |
| `DamagedAmount` | int | จำนวนที่ชำรุดรอซ่อม |
| `DisposedAmount` | int | จำนวนที่จำหน่ายแล้ว (จบสภาพ) |
| `MinimumAmount` | int | จำนวนขั้นต่ำที่ต้องคงไว้ (warning threshold) |
| `ItemStatus` | int | enum value (1=Available, 2=Borrowed, 3=Damaged, 4=Disposed) |
| `Remarks` | string? | บันทึก |
| audit fields | ... | (ตามด้านบน) |

### Navigation properties

```csharp
ICollection<BorrowHistory> BorrowHistories     // เกิดรายการยืม-คืน
ItemBrand                  ItemBrand           // ยี่ห้อ (required)
ItemModel?                 ItemModel           // รุ่น (optional)
ItemType                   ItemType            // ประเภท (required)
ICollection<StockLog>      StockLogs           // ledger ของการเปลี่ยน amount
```

### Invariants (ต้องเป็นจริงเสมอ)

ดู [Services/StockService.cs](../Services/StockService.cs):
```
Total  = Available + Borrowed + Damaged + Disposed
Active = Available + Borrowed + Damaged
ทุก amount ≥ 0
```

ถ้าถูก violation จะ throw `InvalidOperationException` ทันที (ไม่ silent fail)

## 6.3 `BorrowHistory` (ประวัติยืม-คืน)

ไฟล์ [Models/BorrowHistory.cs](../Models/BorrowHistory.cs)

### Fields หลัก

| Field | Type | Meaning |
|---|---|---|
| `Id` | int | PK |
| `ItemId` | int (FK) | อุปกรณ์ที่ยืม |
| `BorrowerUser` | string | ชื่อผู้ยืม (free-text — ไม่ FK กับ Identity) |
| `BorrowerDepartmentId` | int (FK) | ฝ่ายของผู้ยืม |
| `BorrowerSectionId` | int? (FK) | หน่วยงาน (optional) |
| `BorrowTel` | string? | เบอร์โทร (ASCII only) |
| `IsPermanentBorrow` | bool | `true` = ยืมถาวร (ไม่ต้องคืน). จะถูก clear เมื่อมี return |
| `IsInitial` | bool | flag สำหรับ data migration (ปกติ `false`) |
| `BorrowDate` | DateTime | วันที่ยืม |
| `DueDate` | DateTime? | กำหนดส่งคืน (optional) |
| `BorrowItname` | string | ชื่อ IT staff ที่ทำรายการยืม (จาก `User.GetDisplayNameAsync`) |
| `ReturnDate` | DateTime? | วันที่คืน (`null` = ยังไม่คืน) |
| `ReturnItname` | string | ชื่อ IT staff ที่รับคืน (เติมตอน return) |
| `Amount` | int | จำนวนที่ยืม (รองรับ partial return โดยลด amount ลง) |
| `ReferenceNo` | string? | เลขอ้างอิงเอกสาร |
| `Remarks` | string? | บันทึก |
| audit fields | ... | |

### Lifecycle

```
[CREATE]  BH.ReturnDate = null, BH.Amount = N
              ↓
[RETURN  N partial]  BH.Amount -= n  (BH ยังเปิด — ReturnDate ยัง null)
              ↓
[RETURN ทั้งหมด]  BH.ReturnDate = NOW, BH.Amount = 0
```

ดู `BorrowService.ProcessReturnAsync` สำหรับรายละเอียด

## 6.4 `StockLog` (ledger)

ไฟล์ [Models/StockLog.cs](../Models/StockLog.cs)

**เป็น append-only ledger** ของทุกการเปลี่ยน amount ของ Item — เป็นแหล่ง audit trail หลัก

### Fields

| Field | Meaning |
|---|---|
| `Id`, `ItemId` | PK + FK ไปยัง Item |
| `LogType` | enum (ดู `StockLogTypeEnum`) — 10 ค่า |
| `DeltaAvailable`, `DeltaBorrowed`, `DeltaDamaged`, `DeltaDisposed` | การเปลี่ยน (ค่าบวก/ลบ) |
| `DeltaTotal` | sum ของ 4 delta ข้างต้น |
| `AvailableAfter`, `BorrowedAfter`, `DamagedAfter`, `DisposedAfter`, `TotalAfter` | snapshot ของ Item.Amount หลัง apply |
| `ReferenceNo` | เช่น `"BH-123"` สำหรับยืม-คืน, ว่างสำหรับ initial |
| `Remarks` | คำอธิบาย human-readable |
| `CreatedDate`, `CreatedBy` | (ไม่มี ModifiedDate — log ไม่แก้ได้) |

### Continuity invariant

```
สำหรับ Item เดียวกัน, log แรก: After = Delta
สำหรับ log ถัดไป: prev.After + curr.Delta == curr.After
```

ถ้า invariant ผิด → `DataIntegrityService.RunChecksAsync` จะรายงาน

## 6.5 Master data entities

### `ItemType`, `ItemBrand`, `ItemModel`, `Department`, `Section`

- ทุกตัวมี: `Id`, `OrderNo`, `Name`, audit fields, `IsDeleted`
- `ItemModel` มี FK → `ItemBrand` (1:N)
- `Section` มี FK → `Department` (1:N)
- **ใช้ soft delete**: `IsDeleted = true` ไม่ลบจริง

### `ItemTypeToBrand` (junction)

- M:N ระหว่าง ItemType ↔ ItemBrand
- มี audit fields (สามารถรู้ได้ว่าใครเชื่อมความสัมพันธ์เมื่อไหร่)
- ใช้สร้าง cascade dropdown: เลือก Type → กรอง Brand ที่เกี่ยวข้อง

## 6.6 Enums

### [`ItemStatusEnum`](../Models/Enums/ItemStatusEnum.cs)

```csharp
Available = 1   // "ใช้งานได้"
Borrowed  = 2   // "ถูกยืมใช้งาน"
Damaged   = 3   // "ชำรุด/เสื่อมสภาพ"
Disposed  = 4   // "จำหน่ายแล้ว"
```

ใช้ `[Display(Name="...")]` attribute เก็บ Thai label
ดึงผ่าน `EnumExtensions.GetDisplayName()`

### [`StockLogTypeEnum`](../Models/Enums/StockLogTypeEnum.cs)

10 ค่า แบ่งเป็น 2 กลุ่ม:

| Value | Name | Display (Thai) | Group |
|---|---|---|---|
| 1 | InitialAvailable | "รับเข้าเริ่มต้น (พร้อมใช้)" | Initial (≤ 4) |
| 2 | InitialBorrowed | "ย้อนหลัง - ถูกยืม" | Initial |
| 3 | InitialDamaged | "ย้อนหลัง - ชำรุด" | Initial |
| 4 | InitialDisposed | "ย้อนหลัง - จำหน่ายแล้ว" | Initial |
| 5 | Borrow | "ยืม" | Transactional (> 4) |
| 6 | Return | "คืน" | Transactional |
| 7 | Damage | "ชำรุด" | Transactional |
| 8 | Repair | "ซ่อมแล้ว" | Transactional |
| 9 | Dispose | "จำหน่าย" | Transactional |
| 10 | Adjust | "ปรับปรุงจำนวน" | Transactional |

**ทำไมแยก Initial vs Transactional**: `ResetToInitialAsync` ลบเฉพาะ transactional (`LogType > 4`) เพื่อกลับสู่ initial state

---

# 7. Service Layer (Business Logic)

**Pattern หลัก**: Controllers ไม่แตะ DbContext โดยตรง — ทุก mutation ผ่าน service layer

```
Browser
   ↓
Controller (UI concerns: ModelState, dropdown, image upload)
   ↓
Service (business rules, transactions, DB access)
   ↓
DbContext → SQL Server
```

มี 4 service:
1. `IStockService` — จุดเปลี่ยน amount จุดเดียว
2. `IItemService` — จัดการ Item lifecycle
3. `IBorrowService` — จัดการ borrow/return
4. `IDataIntegrityService` — admin diagnostics

## 7.1 `IStockService` — single mutation point

ไฟล์ [Services/StockService.cs](../Services/StockService.cs)

### Signature

```csharp
Task ApplyStockChangeAsync(
    int itemId,
    StockLogTypeEnum logType,
    int deltaAvailable,
    int deltaBorrowed,
    int deltaDamaged,
    int deltaDisposed,
    string createdBy,
    string? referenceNo = null,
    string? remarks = null);
```

### สิ่งที่ทำในแต่ละ call

1. เปิด transaction
2. โหลด Item (ถ้าไม่พบ → throw)
3. apply delta ทั้ง 4 fields → recalculate `Active`, `Total`
4. ตรวจว่ามี field ใดติดลบไหม → ถ้ามี throw `InvalidOperationException`
5. update `ModifiedDate / ModifiedBy` ของ Item
6. INSERT row ใน `StockLog` พร้อม snapshot (After fields)
7. commit transaction

### ทำไมต้องผ่าน method นี้เสมอ

- **Atomic**: Item.Amount และ StockLog ถูก write ใน transaction เดียวกัน
- **Invariant enforcement**: เช็คติดลบในที่เดียว
- **Audit trail**: ไม่มีทาง bypass — ทุก mutation มี StockLog row คู่กันเสมอ

ถ้ามี method ใหม่ที่ต้องเปลี่ยน Item.Amount → **เรียก ApplyStockChangeAsync เท่านั้น**

## 7.2 `IItemService`

ไฟล์ [Services/ItemService.cs](../Services/ItemService.cs)

### Method หลัก

| Method | Purpose |
|---|---|
| `GetFilteredItemsAsync(sortOrder, typeId, brandId, modelId, search)` | query สำหรับ Items/Index — filter + sort + search ในที่เดียว |
| `GetActiveItemTypesAsync` / `Brands` / `Models` | dropdown source — เฉพาะ `!IsDeleted` |
| `GetCascadeMapsAsync()` | คืน `ItemCascadeMaps` 3 dictionaries สำหรับ client-side cascade |
| `GetItemDetailsAsync(id)` | Item + BorrowHistories (with Department, Section) — ใช้ใน Details page |
| `GetStockLogsAsync(itemId)` | log ล่าสุดก่อน — ใช้ใน Details timeline |
| `GetItemWithRelationsAsync(id)` | Item + Type/Brand/Model only |
| `GetItemAsync(id)` | `FindAsync` only — ไม่ load relation |
| `GetForDeleteAsync(id)` | Item + BorrowHistories — สำหรับ dependency check |
| `CreateItemAsync(newItem, initialAmount, status, username)` | INSERT Item + INSERT initial StockLog (Type ตาม status) |
| `UpdateItemAsync(mutated, username)` | UPDATE Item — ตั้ง audit fields, จัดการ concurrency exception |
| `DeleteItemAsync(id)` | hard delete พร้อม dependency check — return `DeleteOutcome` enum |
| `MarkDamagedAsync(itemId, amount, ...)` | Available → Damaged (ผ่าน `StockService`) + set status |
| `MarkRepairedAsync(itemId, amount, ...)` | Damaged → Available + set status |

### `DeleteOutcome` enum (ไม่ใช้ exception เพื่อบอกผล)

```csharp
public enum DeleteOutcome { Deleted, NotFound, HasDependencies }
```

Controller pattern match ค่าที่ได้:
```csharp
return outcome switch {
    DeleteOutcome.NotFound        => NotFound(),
    DeleteOutcome.HasDependencies => RedirectToAction(nameof(Delete), new { id }),
    _                             => RedirectToAction(nameof(Index))
};
```

### `ItemCascadeMaps`

```csharp
public class ItemCascadeMaps {
    public Dictionary<int, List<int>>           TypeToBrands  { get; set; }  // TypeId → [BrandIds]
    public Dictionary<int, List<int>>           BrandToTypes  { get; set; }  // BrandId → [TypeIds]
    public Dictionary<int, List<ItemModel>>     BrandToModels { get; set; }  // BrandId → [Models]
}
```

Serialize เป็น JSON แล้วใส่ใน `<script>` ใน Views → JS ฝั่ง client ใช้สร้าง cascade dropdown โดยไม่ AJAX

## 7.3 `IBorrowService`

ไฟล์ [Services/BorrowService.cs](../Services/BorrowService.cs)

### Method หลัก

| Method | Purpose |
|---|---|
| `GetIndexAsync()` | BH ทั้งหมด (with Item, Type, Brand, Department, Section) — ใช้ใน Index |
| `GetDetailsAsync(id)` | BH + Item + Model + Department + Section |
| `GetActiveBorrowAsync(id)` | BH ที่ยังไม่คืน (`ReturnDate IS NULL && !IsDeleted`) |
| `GetForEditAsync(id)` | BH raw (ไม่ include) |
| `GetForDeleteAsync(id)` | BH + Item details for confirm page |
| `GetActiveDepartmentsAsync` / `Sections` | dropdown source |
| `CreateBorrowAsync(...)` | INSERT BH + เรียก StockService (Avail→Borrowed) + set Item.Status |
| `ProcessReturnAsync(bhId, returnAmount, ...)` | partial/full return logic (ดูด้านล่าง) |
| `UpdateBorrowAsync(mutated, username)` | UPDATE BH |
| `DeleteBorrowAsync(id)` | hard delete BH |

### `ProcessReturnAsync` รายละเอียด

```csharp
bool fullyReturned = bh.Amount == returnAmount;
bh.IsPermanentBorrow = false;        // คืน → ไม่ถาวรแล้ว
if (fullyReturned) {
    bh.ReturnDate   = DateTime.Now;
    bh.ReturnItname = displayName;
}
bh.Amount -= returnAmount;            // ลดเท่ากับที่คืน

item.ItemStatus = (int)ItemStatusEnum.Available;

await _stockService.ApplyStockChangeAsync(
    bh.ItemId,
    StockLogTypeEnum.Return,
    deltaAvailable: +returnAmount,
    deltaBorrowed:  -returnAmount,
    deltaDamaged:    0,
    deltaDisposed:   0,
    createdBy:       username,
    referenceNo:     $"BH-{bh.Id}",
    remarks:         fullyReturned ? "คืนทั้งหมด" : "คืนบางส่วน");
```

> **เคสที่ต้องระวัง**: ถ้า `returnAmount > bh.Amount` ตอนนี้ยังไม่เช็คใน service — ฝั่ง view/controller ต้อง validate ก่อน

## 7.4 `IDataIntegrityService`

ไฟล์ [Services/DataIntegrityService.cs](../Services/DataIntegrityService.cs)

### `RunChecksAsync()` — 5 checks

1. **Internal math** — `Active != Avail+Borr+Dam || Total != Active+Disp`
2. **Negative amounts** — มี field ใดติดลบ
3. **Amount vs latest StockLog** — Item.Amount ไม่ตรงกับ StockLog row ล่าสุด
4. **BorrowedAmount vs active BH** — `Item.BorrowedAmount != SUM(active BH.Amount)`
5. **StockLog continuity** — `prev.After + curr.Delta != curr.After`

คืน `DataIntegrityReport` ที่มี `TotalIssueCount`, `IsConsistent`, และรายการ issue แยกตามประเภท

### `ResetToInitialAsync(username, normalizeOrphanBorrowed)`

**⚠️ destructive operation** — ใช้กู้คืนข้อมูลเสีย

ขั้นตอน:
1. `DELETE FROM BorrowHistory` (ทั้งหมด)
2. `DELETE FROM StockLog WHERE LogType > 4` (เหลือเฉพาะ Initial)
3. Recompute `Item.Amounts` จาก StockLog ล่าสุด (= Initial row); ถ้าไม่มี log ตั้งเป็น 0
4. **ถ้า `normalizeOrphanBorrowed = true`**: ย้าย orphan `BorrowedAmount` → `AvailableAmount` พร้อมลง `Adjust` log
   (orphan = `BorrowedAmount > 0` แต่ไม่มี active BH เพราะถูกลบไปข้อ 1)

ทำใน transaction เดียว — atomic

### `GetResetPreviewAsync()`

แสดงตัวเลขที่จะถูกกระทบก่อนกด reset:
- จำนวน BorrowHistory ที่จะถูกลบ
- จำนวน non-initial StockLog ที่จะถูกลบ
- จำนวน Item ที่จะ recompute
- จำนวน Item ที่จะ reset เป็น 0 (ไม่มี log)
- จำนวน orphan borrowed

---

# 8. Controllers

## 8.1 Pattern ทั่วไป

ทุก controller ที่จัดการ entity มี action standard:

| Action | HTTP | URL | Returns |
|---|---|---|---|
| Index | GET | `/{Entity}` | list view |
| Details | GET | `/{Entity}/Details/{id}` | details view |
| Create | GET | `/{Entity}/Create` | create form |
| Create | POST | `/{Entity}/Create` | redirect to Index on success |
| Edit | GET | `/{Entity}/Edit/{id}` | edit form |
| Edit | POST | `/{Entity}/Edit/{id}` | redirect on success |
| Delete | GET | `/{Entity}/Delete/{id}` | confirmation page |
| DeleteConfirmed | POST | `/{Entity}/Delete/{id}` | redirect to Index (ActionName="Delete") |

POST actions เสมอใช้ `[ValidateAntiForgeryToken]`

## 8.2 [`ItemsController`](../Controllers/ItemsController.cs) — most complex

### Constructor (DI)

```csharp
public ItemsController(
    IItemService itemService,
    IWebHostEnvironment env,        // ใช้สำหรับ wwwroot path
    UserManager<ApplicationUser> userManager)
```

### Actions เพิ่ม (นอกเหนือจาก CRUD)

- `ItemStates()` — แสดงหน้าอธิบาย state diagram
- `Damaged(int? id)` GET/POST — แจ้งชำรุดบางส่วน
- `Repair(int? id)` GET/POST — ซ่อมเสร็จกลับมาใช้ได้

### Private helpers (UI concerns)

| Helper | Purpose |
|---|---|
| `BuildAssetId(vm)` / `BuildAssetIdFromParts(p1..4, other)` | ประกอบ AssetId จาก 4 ส่วน หรือใช้ `OtherAssetId` |
| `SaveImageAsync(file)` | validate ext + size, save to `wwwroot/uploads/items/IT{yyyyMMddHHmm}_{count:00000}.jpg` |
| `PopulateCreateDropdownsAsync(vm)` | load Types/Brands/Models/Statuses + cascade maps |
| `PopulateEditDropdownsAsync(vm)` | (เหมือนข้างบนแต่ withBlank=false สำหรับ status) |
| `BuildSelectList(source, selectedId, placeholder)` | สร้าง `List<SelectListItem>` พร้อม "-- เลือก --" prefix |
| `BuildItemStatusList(selected, withBlank)` | สร้างจาก `ItemStatusEnum` ผ่าน `GetDisplayName()` |

### Image upload validation

```csharp
allowed extensions: .jpg, .jpeg, .png, .gif, .webp
max size:          2 MB
filename pattern:  IT{yyyyMMddHHmm}_{count+1:00000}.jpg
upload directory:  wwwroot/uploads/items/
```

ถ้าไฟล์ผิดเงื่อนไข → `ModelState.AddModelError("ItemImageFile", ...)` → view แสดง error

## 8.3 [`BorrowHistoriesController`](../Controllers/BorrowHistoriesController.cs)

### Actions

| Action | Purpose |
|---|---|
| `Index()` | list BH ทั้งหมด — เรียงล่าสุดก่อน |
| `Details(id)` | แสดง BH + Item |
| `Create(int? itemId)` GET/POST | ยืม — รับ itemId จาก query string (มาจาก Items/Details) |
| `Return(int? id)` GET/POST | คืน — รับ bhId, รองรับ partial return |
| `Edit(int? id)` GET/POST | แก้ไข BH (โดย admin) |
| `Delete(int? id)` GET/POST | ลบ BH (hard delete) |

### หมายเหตุสำคัญ

- ตอน Create ถ้า `BorrowDate == TODAY` → ใส่ `DateTime.Now` (มี time) → log แม่นยำขึ้น
- `Itstaff` ใน VM = display name (ดึงผ่าน `User.GetDisplayNameAsync(_userManager)`)
- เก็บลง `BorrowItname` หรือ `ReturnItname` ตามสถานะ

## 8.4 [`AdminController`](../Controllers/AdminController.cs)

### Actions

| Action | Purpose | Risk |
|---|---|---|
| `BackfillStockLog` GET/POST | สร้าง opening balance log สำหรับ legacy items | safe — idempotent (item ที่มี log อยู่แล้วจะ skip) |
| `DataIntegrityCheck` GET | รัน 5 checks → display report | safe (read-only) |
| `ResetToInitial` GET/POST | ลบ transactional data → กลับสู่ initial | **DESTRUCTIVE** — confirm checkbox required |

### TODO ของ controller นี้

```csharp
// TODO: เพิ่ม [Authorize(Roles = "Admin")] เมื่อ role-based auth พร้อมใช้
```

ปัจจุบันยังไม่มี role — ทุก authenticated user เข้าถึงได้

## 8.5 [`HomeController`](../Controllers/HomeController.cs)

```csharp
[AllowAnonymous]
public class HomeController : Controller {
    public IActionResult Index()   => View();
    public IActionResult Privacy() => View();
    public IActionResult Error()   => View(new ErrorViewModel { ... });
}
```

`[AllowAnonymous]` จำเป็นเพราะมี global `AuthorizeFilter` ใน `Program.cs`

## 8.6 Master data controllers (เรียบ ๆ ทุกตัวคล้ายกัน)

[`DepartmentsController`](../Controllers/DepartmentsController.cs), [`SectionsController`](../Controllers/SectionsController.cs), [`ItemTypesController`](../Controllers/ItemTypesController.cs), [`ItemBrandsController`](../Controllers/ItemBrandsController.cs), [`ItemModelsController`](../Controllers/ItemModelsController.cs), [`ItemTypeToBrandsController`](../Controllers/ItemTypeToBrandsController.cs)

### Pattern เหมือนกัน

- ฉีด `ItLptWarehouseContext` ตรง ๆ (ไม่มี service สำหรับ master data)
- Index รองรับ sort + search + filter (บางตัว)
- Create / Edit ใช้ ViewModel เฉพาะ
- Delete:
  - **soft delete**: `Department`, `Section`, `ItemType`, `ItemBrand`, `ItemModel`, `ItemTypeToBrand` (set `IsDeleted = true`)
  - มี dependency check ก่อนลบ (ใน controller ตอน DeleteConfirmed)

### ตัวอย่าง dependency check (จาก `ItemTypesController.DeleteConfirmed`)

```csharp
var t = await _context.ItemTypes.Include(x => x.Items).FirstOrDefaultAsync(i => i.Id == id);
if (t.Items.Any(i => !i.IsDeleted))
    return RedirectToAction(nameof(Delete), new { id });   // กลับไปหน้า confirm — มี error
t.IsDeleted = true;
// ...
```

> **NB**: `SectionsController.DeleteConfirmed` ใช้ **hard delete** (`Remove`) — ไม่สอดคล้องกับ master data อื่น ๆ — อาจเป็นบั๊ก ดู `TODO.md`

---

# 9. ViewModels

## 9.1 Naming convention

| Suffix | Purpose |
|---|---|
| `*ViewModel` | สำหรับ Create POST หรือ index list |
| `*EditViewModel` | สำหรับ Edit POST (มี `Id`) |
| `*DeleteViewModel` | confirmation page ก่อนลบ — มี dependency-check fields |
| `*DetailsViewModel` | details page (read-only + nested collections) |
| `*CreateViewModel` | บางที่ใช้ (เช่น `ItemCreateViewModel`) แต่ไม่ทุก entity |

## 9.2 Pattern: Create ViewModel

ดู [`ItemCreateViewModel`](../ViewModels/ItemCreateViewModel.cs) เป็นตัวอย่าง — มีทั้ง:

1. **Input fields** — `AssetId1..4`, `OtherAssetId`, `SerialNo`, `Selected*Id`, `IsBulk`, `TotalAmount`, `MinimumAmount`, `ItemDescription`, `ItemImageFile`, `Remarks`
2. **Dropdown SelectListItem collections** — `ItemTypes`, `ItemBrands`, `ItemModels`, `ItemStatuses`
3. **Cascade map dictionaries** — `TypeToBrandsMap`, `BrandToTypesMap`, `BrandToModelsMap`
4. **`IValidatableObject`** implementation — สำหรับ custom cross-field validation:
   ```csharp
   if (MinimumAmount >= TotalAmount)
       yield return new ValidationResult("จำนวนขั้นต่ำต้องน้อยกว่าจำนวนทั้งหมด", ...);
   ```

## 9.3 Pattern: Edit ViewModel

[`ItemEditViewModel`](../ViewModels/ItemEditViewModel.cs) คล้าย Create แต่:
- มี `Id`
- มี readonly fields: `AvailableAmount`, `BorrowedAmount`, `DamagedAmount`, `DisposedAmount` (ห้ามแก้ — แสดงเฉย ๆ)
- มี audit fields readonly: `CreatedDate/By`, `ModifiedDate/By`, `IsDeleted`
- `Selected*Id` เป็น `[Required]` (Create ใช้ nullable)

## 9.4 Pattern: Delete ViewModel

ทั้งหมดรวมในไฟล์เดียว: [`DeleteViewModels.cs`](../ViewModels/DeleteViewModels.cs)

### โครงสร้างทั่วไป

```csharp
public class XxxDeleteViewModel {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // ... readonly display fields

    // dependency check fields
    public int ChildCount { get; set; }
    public bool CanDelete => /* logic */;
}
```

### ตัวอย่าง: `ItemDeleteViewModel`

```csharp
public int  BorrowHistoryCount   { get; set; }
public bool HasActiveBorrow      { get; set; }    // มีรายการที่ยังไม่คืน
public bool CanDelete            => BorrowHistoryCount == 0;
```

→ view ใช้ `Model.CanDelete` ตัดสินว่าโชว์ปุ่ม delete หรือ block + แสดงเหตุผล

## 9.5 ViewModel แปลก ๆ ที่ควรรู้

- **[`ItemDetailViewModel`](../ViewModels/ItemDetailViewModel.cs)** vs **[`ItemDetailsViewModel`](../ViewModels/ItemDetailsViewModel.cs)** — ชื่อใกล้กันมาก (ต่างตัว s) แต่คนละ purpose:
  - `ItemDetailViewModel` (เอกพจน์) — ?
  - `ItemDetailsViewModel` (พหูพจน์) — ใช้ใน `Items/Details` page

- **[`BorrowInItemDetailViewModel`](../ViewModels/BorrowInItemDetailViewModel.cs)** — เป็น nested VM ภายใน `ItemDetailsViewModel.BHinItemDetails` (แสดง borrow history ในหน้า item details)

- **[`StockLogTimelineViewModel`](../ViewModels/ItemDetailsViewModel.cs#L72)** — nested ภายใน `ItemDetailsViewModel.StockTimeline`

## 9.6 ใช้ `[ModelBinder(typeof(IsoDateModelBinder))]` ตรงไหน

ดู `BorrowCreateViewModel.BorrowDate`, `ExpectedReturnDate`:

```csharp
[ModelBinder(BinderType = typeof(IsoDateModelBinder))]
public DateTime BorrowDate { get; set; }
```

จำเป็นเพราะ Flatpickr ส่ง `yyyy-MM-dd` (ค.ศ.) แต่ Thai culture parser จะตีเป็น พ.ศ. → ผิด

---

# 10. Views & Design System

## 10.1 Layout

[`_Layout.cshtml`](../Views/Shared/_Layout.cshtml) เป็น master layout — ทุกหน้าใช้ผ่าน `_ViewStart.cshtml`

ภายในมี:
- `<head>`: Bootstrap 5, jQuery, Flatpickr, validation scripts
- `_DesignSystem.cshtml` (rendered as `<style>`)
- Navbar + `_LoginPartial.cshtml`
- `@RenderBody()`
- footer

## 10.2 Design System CSS (`ds-*` classes)

ไฟล์ [`_DesignSystem.cshtml`](../Views/Shared/_DesignSystem.cshtml)

**ทำไมไม่ใช้ไฟล์ .css**: รวมไว้ในไฟล์ cshtml เพื่อให้ระบบ View ส่งออกเป็น `<style>` inline ใน HTML response — รับประกันได้ว่า CSS ถูก load ก่อน body content ไม่ต้องพึ่ง `MapStaticAssets`

### Class หลัก

| Group | Classes |
|---|---|
| Layout | `ds-card`, `ds-form-row`, `ds-table-wrap`, `ds-filter-bar` |
| Buttons | `ds-btn-primary`, `ds-btn-ghost`, `ds-btn-danger` |
| Inputs | `ds-input`, `ds-select`, `ds-input-ro` (readonly) |
| (badges) | เคยมี `ds-badge-*` ใช้ใน table — **ปัจจุบันถูกลบจาก list view แล้ว** เพราะ visual noise — เหลือใช้ใน form Create/Edit/Return |

### กฎการเขียน

- prefix `ds-` สำหรับ custom class — Bootstrap class ใช้ได้ปกติ
- ใส่ใน `_DesignSystem.cshtml` ไม่ใส่กระจาย

## 10.3 Item state diagram

มี 3 ไฟล์:
- [`_ItemStateDiagram.cshtml`](../Views/Shared/_ItemStateDiagram.cshtml) — wrapper เลือก SVG version
- [`_ItemStateDiagramSvg.cshtml`](../Views/Shared/_ItemStateDiagramSvg.cshtml) — horizontal version
- [`_ItemStateDiagramSvgVertical.cshtml`](../Views/Shared/_ItemStateDiagramSvgVertical.cshtml) — vertical version

แสดงผ่าน [`ItemStates`](../Controllers/ItemsController.cs#L333) action

### กฎ SVG (จาก [CLAUDE.md](../CLAUDE.md))

> **Always announce before**: Creating or editing SVG diagrams → draw all `<rect>` first, then all `<path>`/`<line>` (SVG paints in source order — lines drawn before boxes get painted over)

## 10.4 Login partial

[`_LoginPartial.cshtml`](../Views/Shared/_LoginPartial.cshtml) — แสดงในนาฟบาร์
- ถ้า login แล้ว: แสดง "สวัสดี {DisplayName}" + "ออกจากระบบ"
- ถ้ายังไม่ login: แสดงปุ่ม "เข้าสู่ระบบ" + "สมัครสมาชิก"

## 10.5 Cascade dropdown ใน Razor

ตัวอย่าง pattern (จาก `Items/Create.cshtml`):

```html
<select id="ItemType" asp-for="SelectedItemTypeId" asp-items="Model.ItemTypes"></select>
<select id="ItemBrand" asp-for="SelectedItemBrandId" asp-items="Model.ItemBrands"></select>
<select id="ItemModel" asp-for="SelectedItemModelId" asp-items="Model.ItemModels"></select>

@section Scripts {
<script>
    const typeToBrands  = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.TypeToBrandsMap));
    const brandToTypes  = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.BrandToTypesMap));
    const brandToModels = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.BrandToModelsMap));
    // เลือก Type → กรอง Brand options
    // เลือก Brand → กรอง Type options + กรอง Model options
</script>
}
```

→ ไม่มี AJAX call — map ทั้งหมด render มากับ HTML

---

# 11. Helpers

## 11.1 [`ClaimsPrincipalExtensions`](../Helpers/ClaimsPrincipalExtensions.cs)

```csharp
public static string GetUsernameLocalPart(this ClaimsPrincipal principal)
```
- คืนชื่อก่อน `@` (เช่น `"tont"` จาก `"tont@example.com"`)
- ถ้าไม่มี `@` คืนทั้งหมด
- ใช้ในทุก controller ตอน set `CreatedBy / ModifiedBy`

```csharp
public static async Task<string> GetDisplayNameAsync(this ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
```
- คืน `ApplicationUser.DisplayName` ถ้ามี — ถ้าไม่มี fallback เป็น local part
- ใช้ตอน set `BorrowItname / ReturnItname` (ที่แสดง user-facing)

**ทำไมแยก 2 method**:
- `LocalPart` = identifier (เก็บใน audit fields)
- `DisplayName` = human-readable (แสดงในรายการยืม)

## 11.2 [`EnumExtensions`](../Helpers/EnumExtensions.cs)

```csharp
public static string GetDisplayName(this Enum value)
```

ใช้ reflection อ่าน `[Display(Name="...")]` attribute → ถ้าไม่มี fallback เป็น `.ToString()`

ใช้ในทุกที่ที่ต้องแสดง enum เป็น Thai (ใน view, ใน SelectListItem)

## 11.3 [`IsoDateModelBinder`](../Helpers/IsoDateModelBinder.cs)

**Problem**: Flatpickr ส่ง `2026-05-19` (ISO ค.ศ.) แต่ Thai culture parser ตีเป็น 2026 → +543 = 2569 ใน DB → ค่าผิด

**Solution**: model binder ที่ parse `yyyy-MM-dd` ด้วย `InvariantCulture` แทน

### Flow
1. รับค่า raw จาก ValueProvider
2. ถ้าว่าง:
   - nullable → success(null)
   - not nullable → fail
3. `TryParseExact("yyyy-MM-dd", InvariantCulture)` ถ้าผ่านก็ใช้ — fallback `TryParse(InvariantCulture)`
4. ถ้า parse ไม่ผ่าน → add model error `"รูปแบบวันที่ไม่ถูกต้อง"`

### ใช้ตรงไหน

ViewModel field ที่ bind date จาก Flatpickr:
```csharp
[ModelBinder(BinderType = typeof(IsoDateModelBinder))]
public DateTime BorrowDate { get; set; }
```

## 11.4 [`StockLogBackfillHelper`](../Helpers/StockLogBackfillHelper.cs)

**Problem**: ก่อนหน้านี้ระบบไม่ได้บันทึก StockLog → Item มี Amount แต่ไม่มี log ที่ตรงกัน → DataIntegrityCheck เตือน

**Solution**: one-time admin action — สำหรับ item ที่ยังไม่มี StockLog ใด ๆ ให้สร้าง Initial log ตาม Amount ปัจจุบัน

### Logic

1. หา item ที่ `!IsDeleted && !i.StockLogs.Any()`
2. สำหรับแต่ละ item:
   - ถ้า `AvailableAmount > 0` → INSERT `InitialAvailable` log
   - ถ้า `BorrowedAmount > 0` → INSERT `InitialBorrowed` log
   - ถ้า `DamagedAmount > 0` → INSERT `InitialDamaged` log
   - ถ้า `DisposedAmount > 0` → INSERT `InitialDisposed` log
3. ทุก log ใช้ `Remarks = "Backfill opening balance"` + same timestamp

**Idempotent**: ถ้ารันซ้ำ จะ skip item ที่มี log อยู่แล้ว → ปลอดภัย

---

# 12. Validation

3 custom attributes — ทุกตัวเป็น `ValidationAttribute` + `IClientModelValidator` (รองรับทั้ง server & client side)

## 12.1 [`NumericOnlyAttribute`](../Validation/NumericOnlyAttribute.cs)

- ตรวจว่าทุกตัวอักษรเป็น digit
- ไม่ตรวจความยาว
- ว่าง = pass
- client attribute: `data-val-numericonly`

## 12.2 [`NumericFixedLengthAttribute`](../Validation/NumericFixedLengthAttribute.cs)

- ตรวจ digit-only **และ** length ตรงเป๊ะ
- รับ `length` ผ่าน constructor: `[NumericFixedLengthAttribute(4)]`
- ว่าง = pass (จะ enforce required ผ่าน `[Required]` แยก)
- client attributes: `data-val-numericfixedlength`, `data-val-numericfixedlength-length`

ใช้ใน [`ItemCreateViewModel`](../ViewModels/ItemCreateViewModel.cs) สำหรับ `AssetId1` (4 หลัก), `AssetId2` (8 หลัก), `AssetId3` (8 หลัก), `AssetId4` (5 หลัก)

## 12.3 [`FixedLengthIfProvidedAttribute`](../Validation/FixedLengthIfProvidedAttribute.cs)

- เหมือน NumericFixedLength แต่ไม่ตรวจ digit-only
- เน้น: ถ้ามีค่า ต้องยาวเท่าที่กำหนด

## 12.4 Client-side hookup (เพื่อให้ทำงาน)

ใน [`_ValidationScriptsPartial.cshtml`](../Views/Shared/_ValidationScriptsPartial.cshtml) ต้องมี JS ที่ extends jQuery Validation rules — ดูไฟล์นั้นเพื่อตรวจว่ามี handler สำหรับ `numericonly`, `numericfixedlength`, `fixedlength`

(โดยปกติเขียนใน `<script>` ภายใน partial หรือใน file JS แยก)

---

# 13. Areas / Identity

ASP.NET Core Identity ใช้ **Areas** pattern — UI scaffolded อยู่ที่ [`Areas/Identity/Pages/`](../Areas/Identity/Pages/)

### หน้าที่มี (default scaffolded)
- `/Identity/Account/Login`
- `/Identity/Account/Register`
- `/Identity/Account/Logout`
- `/Identity/Account/ConfirmEmail`
- `/Identity/Account/Manage/*` (เปลี่ยน password, email, etc.)

### Custom field: `DisplayName`

ดู [`Data/ApplicationUser.cs`](../Data/ApplicationUser.cs):

```csharp
public class ApplicationUser : IdentityUser {
    public string? DisplayName { get; set; }
}
```

หากต้องเพิ่ม field ใน Register page → scaffold page `Account/Register.cshtml.cs` แล้วแก้ + เพิ่ม migration ของ Identity DB

### Routing

`app.MapRazorPages()` ใน Program.cs ต้องเรียกเพื่อให้ Identity area routing ทำงาน

---

# 14. Migrations

## 14.1 Warehouse DB

อยู่ที่ [`Migrations/`](../Migrations/) — มี `ItLptWarehouseContextModelSnapshot.cs` เป็น snapshot ปัจจุบัน

### เพิ่ม migration ใหม่
```bash
dotnet ef migrations add AddSomeColumn --context ItLptWarehouseContext --output-dir Migrations
```

### Apply
```bash
dotnet ef database update --context ItLptWarehouseContext
```

### Rollback
```bash
dotnet ef database update <PreviousMigrationName> --context ItLptWarehouseContext
```

## 14.2 Identity DB

อยู่ที่ [`Data/Migrations/`](../Data/Migrations/)

```bash
dotnet ef migrations add ... --context ApplicationDbContext --output-dir Data/Migrations
dotnet ef database update --context ApplicationDbContext
```

## 14.3 Ad-hoc SQL scripts

โฟลเดอร์ [`Data/WarehouseScripts/`](../Data/WarehouseScripts/)

ใช้สำหรับ fix ที่ไม่เหมาะกับ migration เช่น:
- `FixUnicodeAuditColumns.sql` — แก้ collation ของ `CreatedBy/ModifiedBy` ที่ตั้งเป็น `IsUnicode(false)` ทำให้บางครั้งเก็บ Thai chars เพี้ยน

---

# 15. Static Assets (`wwwroot/`)

```
wwwroot/
├── css/                    # custom site CSS (ถ้ามี)
├── js/                     # custom site JS (ถ้ามี)
├── lib/                    # vendor libraries (Bootstrap, jQuery — ติดมากับ template)
├── uploads/
│   └── items/              # อัพรูป item (สร้างอัตโนมัติเวลามี Item ที่มีรูปแรก)
└── favicon.ico
```

`app.MapStaticAssets()` ใน Program.cs จะ serve ไฟล์เหล่านี้พร้อมทำ asset manifest

---

# 16. Key Flows (Step-by-Step)

## 16.1 Flow: ยืมอุปกรณ์

```
1. User กดปุ่ม "ยืม" ใน Items/Details/{id}
       ↓ (link → /BorrowHistories/Create?itemId={id})
2. GET BorrowHistoriesController.Create(itemId)
   - ItemService.GetItemWithRelationsAsync(id) — load item + type/brand/model
   - สร้าง BorrowCreateViewModel เริ่มต้น:
       ItemHeader  = "{TypeName} {BrandName} {ModelName}"
       ItemAssetId = item.AssetId
       BorrowDate         = NOW
       ExpectedReturnDate = NOW
       Itstaff            = await User.GetDisplayNameAsync(_userManager)
   - PopulateBorrowCreateDropdownsAsync(vm) — load Departments, Sections
   - return View(vm)
       ↓
3. User กรอกฟอร์ม → กด "บันทึก"
       ↓ POST /BorrowHistories/Create
4. POST BorrowHistoriesController.Create(BorrowCreateViewModel bhVM)
   - re-populate Itstaff (กันแก้)
   - if (!ModelState.IsValid) → re-populate dropdowns → return View(vm)
   - ถ้า BorrowDate.Date == TODAY → set bhVM.BorrowDate = NOW (มีเวลา)
   - username = User.GetUsernameLocalPart()
   - BorrowService.CreateBorrowAsync(...)
       │
       ├─► new BorrowHistory {
       │       ItemId, BorrowerUser, DepartmentId, SectionId,
       │       IsPermanentBorrow, BorrowDate, DueDate,
       │       BorrowItname = displayName, ReturnItname = "",
       │       Amount, audit fields
       │   }
       ├─► db.BorrowHistories.Add(bh); SaveChangesAsync()
       │   (ได้ bh.Id)
       ├─► item.ItemStatus = Borrowed
       └─► StockService.ApplyStockChangeAsync(
               itemId, StockLogTypeEnum.Borrow,
               deltaAvail=-N, deltaBorr=+N, deltaDam=0, deltaDisp=0,
               createdBy=username, referenceNo=$"BH-{bh.Id}", remarks="ยืม")
              │
              ├─► open transaction
              ├─► item.Avail -= N, item.Borr += N, recalc Active/Total
              ├─► check ≥ 0 → ถ้าติดลบ throw
              ├─► INSERT StockLog (LogType=Borrow, all deltas + after snapshots)
              ├─► SaveChangesAsync
              └─► commit
   - return RedirectToAction(Items.Details, new { id = bhVM.ItemId })
       ↓
5. User กลับมาที่หน้า Items/Details — เห็น item.Status=Borrowed, Amount.Borrowed += N
```

## 16.2 Flow: คืนอุปกรณ์ (รองรับ partial)

```
1. ใน Items/Details — กดปุ่ม "คืน" ของรายการยืมหนึ่ง
       ↓ link → /BorrowHistories/Return/{bhId}
2. GET BorrowHistoriesController.Return(int? id)
   - BorrowService.GetActiveBorrowAsync(id)
       → WHERE Id=id AND !IsDeleted AND ReturnDate IS NULL
       → ถ้าไม่พบ (อาจคืนไปแล้ว) → NotFound()
   - ItemService.GetItemWithRelationsAsync(bh.ItemId)
   - สร้าง ReturnCreateViewModel (มี ReturnAmount field)
   - PopulateReturnDropdownsAsync (Department, Section)
   - return View(vm)
       ↓
3. User กรอก ReturnAmount → กด "บันทึก"
       ↓ POST /BorrowHistories/Return
4. POST Return(ReturnCreateViewModel bhVM)
   - re-populate Itstaff
   - BorrowService.ProcessReturnAsync(bhVM.Id, bhVM.ReturnAmount, ...)
       │
       ├─► โหลด BH ที่ active เท่านั้น → ถ้าไม่พบ return false → NotFound()
       ├─► fullyReturned = bh.Amount == returnAmount
       ├─► bh.IsPermanentBorrow = false  (เคยถาวร? คืนแล้วไม่ถาวร)
       ├─► if (fullyReturned):
       │       bh.ReturnDate   = NOW
       │       bh.ReturnItname = displayName
       │   else: BH ยังเปิด
       ├─► bh.Amount -= returnAmount
       ├─► item.ItemStatus = Available
       └─► StockService.ApplyStockChangeAsync(
               itemId, StockLogTypeEnum.Return,
               deltaAvail=+R, deltaBorr=-R, ...)
   - return RedirectToAction(Items.Details)
```

### กรณี partial return ที่ต้องเข้าใจ

- ยืม 10 → คืน 3: BH.Amount = 7 (ยังเปิด), Item.Borrowed -= 3, Item.Avail += 3
- ยืม 7 (เหลือจากเดิม) → คืน 7: BH.Amount = 0, ReturnDate set, BH ปิด, Item.Borrowed -= 7

## 16.3 Flow: แจ้งชำรุด (Damaged)

```
1. ใน Items/Details — กด "แจ้งชำรุด"
       ↓ /Items/Damaged/{id}
2. GET Items.Damaged(id)
   - ItemService.GetItemWithRelationsAsync(id)
   - ถ้า AvailableAmount <= 0 → redirect ไป Details (ไม่มีอะไรให้แจ้ง)
   - สร้าง ItemDamagedViewModel (header, asset id, status, avail amount, itstaff)
   - return View(vm)
       ↓
3. User กรอกจำนวน + remarks → กด "ยืนยัน"
       ↓ POST
4. POST Items.Damaged(ItemDamagedViewModel vm)
   - re-populate Itstaff
   - ถ้า vm.Amount > item.AvailableAmount → AddModelError → return View
   - ItemService.MarkDamagedAsync(itemId, amount, remarks, username, itstaffDisplay)
       │
       ├─► item.ItemStatus = Damaged
       └─► StockService.ApplyStockChangeAsync(
               itemId, StockLogTypeEnum.Damage,
               deltaAvail=-amount, deltaBorr=0, deltaDam=+amount, deltaDisp=0,
               createdBy=username,
               remarks = "{remarks} (โดย {staff})" หรือ "แจ้งเสียหาย โดย {staff}")
   - return RedirectToAction(Details, new { id })
```

## 16.4 Flow: ซ่อมสำเร็จ (Repair) — สมมาตรกับ Damaged

```
GET /Items/Repair/{id} → ถ้า DamagedAmount <= 0 redirect ไป Details
POST /Items/Repair → ItemService.MarkRepairedAsync
   → item.ItemStatus = Available
   → ApplyStockChangeAsync(deltaAvail=+a, deltaDam=-a, LogType=Repair)
```

## 16.5 Flow: Item state diagram (summary)

```
                  Borrow
       ┌──────────────────────►┌───────────┐
       │                        │           │
       │           Return       │           │
   ┌───┴─────┐◄─────────────────┤ Borrowed  │
   │Available│                  │           │
   │         │                  └───────────┘
   └───┬─────┘
       │ ▲
Damage │ │ Repair
       ▼ │
   ┌─────┴───┐
   │ Damaged │
   └────┬────┘
        │ Dispose (เพิ่ม FEAT)
        ▼
   ┌─────────┐
   │Disposed │  ← terminal
   └─────────┘
```

> **NB**: ปัจจุบัน Dispose action ยังไม่ implement (ตาม `TODO.md`) — ต้องเพิ่ม controller action + service method

---

# 17. Conventions, Gotchas, ข้อระวัง

## 17.1 Soft delete vs Hard delete

| Entity | Strategy | เหตุผล |
|---|---|---|
| ItemType, ItemBrand, ItemModel | soft (`IsDeleted=true`) | ป้องกัน FK orphan ใน Item เก่า |
| Department, Section | soft | เหมือนกัน |
| ItemTypeToBrand | soft | เพื่อตามรอย |
| Item | hard (มี dependency check) | อาจเปลี่ยนเป็น soft ในอนาคต (BUG-001 ใน TODO) |
| BorrowHistory | hard | ลบได้ตามต้องการ — ไม่มี child |

## 17.2 Audit fields ต้อง populate

ทุก INSERT ตั้ง 5 fields:
```csharp
e.CreatedBy    = username;
e.ModifiedBy   = username;
e.CreatedDate  = DateTime.Now;
e.ModifiedDate = DateTime.Now;
e.IsDeleted    = false;
```

ทุก UPDATE ตั้ง 2 fields:
```csharp
e.ModifiedBy   = username;
e.ModifiedDate = DateTime.Now;
```

อย่าพึ่ง trigger หรือ EF interceptor — ไม่มี

## 17.3 `ApplyStockChangeAsync` คือทาง mutation amount เดียวเท่านั้น

**ห้าม** modify `item.AvailableAmount` หรือ field amount อื่น ๆ ตรง ๆ ใน controller / service ใหม่
ต้องเรียก `IStockService.ApplyStockChangeAsync` เสมอ

## 17.4 Date binding กับ Flatpickr

ถ้าฟอร์มไหนมี date input ที่ใช้ Flatpickr → ViewModel field ต้องมี `[ModelBinder(typeof(IsoDateModelBinder))]`
ไม่งั้นจะเจอบั๊ก ปี พ.ศ. เก็บเป็น ค.ศ. (ตัวเลขเลื่อน 543 ปี)

## 17.5 Username vs DisplayName

- **เก็บใน audit fields**: `username` (local part — `tont`)
- **แสดง user-facing** (เช่น `BorrowItname`): `displayName` (`Tont Klangnatee` หรือ fallback เป็น username)

## 17.6 Cascade dropdown สร้างจาก JSON map ฝั่ง client

ไม่ AJAX — ทั้ง map render มากับ HTML page

ถ้าจำนวน items เยอะมาก (>5000) อาจกระทบ page size — ตอนนี้ยังไม่ใช่ปัญหา

## 17.7 `[Authorize]` filter เป็น global

หน้าที่เปิดสาธารณะต้อง `[AllowAnonymous]` (ปัจจุบันมีแค่ `HomeController`)

ถ้าเพิ่ม controller / API ใหม่ → จะถูก require login โดย default

## 17.8 ห้ามแตะ `_Layout.cshtml` ถ้าไม่ถาม

ดู [CLAUDE.md §11](../CLAUDE.md) — ทำให้ทุกหน้าหลุดได้

## 17.9 SVG drawing order

วาด `<rect>` ทุกตัวก่อน แล้วค่อยวาด `<path>/<line>` — SVG paint ตามลำดับ source

## 17.10 ห้ามเพิ่ม dependency ใหม่ถ้าไม่ถาม

ดู [CLAUDE.md §11](../CLAUDE.md)

## 17.11 ไฟล์ ViewModel ชื่อใกล้กันมาก

`ItemDetailViewModel` vs `ItemDetailsViewModel` (s ตัวเดียว ต่างหน้าที่)
อ่านโค้ดต้องดูดี ๆ ว่า import ตัวไหน

---

# 18. Suggested Reading Order สำหรับ Developer ใหม่

ถ้าเข้าทีมใหม่ แนะนำลำดับนี้:

| ลำดับ | ไฟล์ | สิ่งที่จะเข้าใจ |
|---|---|---|
| 1 | [CLAUDE.md](../CLAUDE.md) | กฎการเขียนโค้ดในโครงการ |
| 2 | [Program.cs](../Program.cs) | สถาปัตยกรรมการ bootstrap |
| 3 | [Models/Item.cs](../Models/Item.cs) + [Models/StockLog.cs](../Models/StockLog.cs) | core entity + invariant |
| 4 | [Services/IStockService.cs](../Services/IStockService.cs) + [StockService.cs](../Services/StockService.cs) | จุดเปลี่ยนข้อมูลเดียว |
| 5 | [Services/BorrowService.cs](../Services/BorrowService.cs) | flow ยืม-คืน |
| 6 | [Controllers/ItemsController.cs](../Controllers/ItemsController.cs) | controller pattern ครบ |
| 7 | [Controllers/BorrowHistoriesController.cs](../Controllers/BorrowHistoriesController.cs) | borrow flow |
| 8 | [Views/Items/Index.cshtml](../Views/Items/Index.cshtml) | UI + cascade dropdown |
| 9 | [Views/Shared/_DesignSystem.cshtml](../Views/Shared/_DesignSystem.cshtml) | CSS conventions |
| 10 | [TODO.md](../TODO.md) | สิ่งที่ยังต้องทำ |
| 11 | [Services/DataIntegrityService.cs](../Services/DataIntegrityService.cs) | admin tools |

---

# 19. Future Work (จาก TODO.md ในขณะที่เขียน)

อ่าน [TODO.md](../TODO.md) เพื่อ status ปัจจุบัน หมวดที่ active:

- **BUG-001** — return amount edge case (อาจเปลี่ยน Item เป็น soft delete)
- **FEAT-XXX** — Dispose action, pagination
- **UX-XXX** — badge cleanup ใน list view
- **TECH-XXX** — refactor / migration cleanup

---

# 20. References

- [CLAUDE.md](../CLAUDE.md) — AI assistant behavioral guide (มีรายละเอียดการตั้งชื่อ class, pattern, ห้ามทำอะไร)
- [TODO.md](../TODO.md) — known bugs, missing features, tech debt
- [docs/ARCHITECTURE.md](ARCHITECTURE.md) — เอกสารระดับ architecture สั้นกว่า

---

**Document version**: 1.0
**Generated**: 2026-05-19
**Maintainer**: ดู git log สำหรับ active contributors
