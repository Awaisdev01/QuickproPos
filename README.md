# 🏪 QuickproPos

> A full-featured Point-of-Sale desktop/mobile application built with **.NET 8 MAUI (Blazor Hybrid)** — sales, purchases, inventory, customers/suppliers, accounting, reports, and cash-register management in one cross-platform app.

![.NET](https://img.shields.io/badge/.NET-8.0-blue) ![MAUI](https://img.shields.io/badge/MAUI-Blazor%20Hybrid-purple) ![platform](https://img.shields.io/badge/platforms-Android%20%7C%20iOS%20%7C%20Mac%20%7C%20Windows-lightgrey)

---

## ✅ Reviewed & runs on macOS

This repo has been **reviewed and built on Apple Silicon macOS**:

```
dotnet build -f net8.0-maccatalyst -c Debug -p:TargetFrameworks=net8.0-maccatalyst
→ 0 errors (build time ~30 s)
dotnet build -t:Run -f net8.0-maccatalyst ...
→ launches as a native Mac window ✅
```

### Setup on macOS (Apple Silicon or Intel)

```bash
# 1. .NET 8 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# 2. MacCatalyst + MAUI workloads (needs full Xcode installed)
dotnet workload install maccatalyst
dotnet workload install maui-maccatalyst

# 3. Build & run
./scripts/run-mac.sh
# or manually:
dotnet build QuickproPos/QuickproPos/QuickproPos.csproj -t:Run \
  -f net8.0-maccatalyst -c Debug -p:TargetFrameworks=net8.0-maccatalyst
```

> **Why the `-p:TargetFrameworks` override?** The `.csproj` lists
> `net8.0-android;net8.0-ios;net8.0-maccatalyst`, so a plain restore demands the
> Android/iOS workloads too. On a Mac without them, override to build only the
> MacCatalyst target. Install `maui-android` / `maui-ios` workloads if you want
> those targets.

## Features (from the bundled documentation)

| Module | What it does |
|---|---|
| 🧾 **POS / Sales** | Touch-friendly billing screen, cart, discounts, payment methods |
| 📦 **Purchases** | Supplier purchase invoices & purchase returns |
| 🏷️ **Products** | Categories, brands, units, stock, pricing, barcode |
| 👥 **Parties** | Customers & suppliers with ledgers |
| 💰 **Payments** | Payment In / Payment Out, cash-in-hand, cash register sessions |
| 🧮 **Accounting** | Account groups & ledgers, financial years, currencies |
| 📊 **Reports** | Sales, purchases, inventory, and financial reports |
| 🎟️ **Extras** | Coupons, invoice settings, company profile, backup & restore |
| 🖨️ **Printing** | Receipt printing service (Windows WMI paths — see review notes) |

More module screenshots (customers, suppliers, payment in/out, cash register,
backup & restore) are in [`docs/images/`](docs/images/).

## Screenshots

| Dashboard | POS Billing | Reports |
|:---:|:---:|:---:|
| ![Dashboard](docs/images/dashboard.png) | ![POS](docs/images/pos.png) | ![Reports](docs/images/reports.png) |

| Sales Invoices | Products | Purchases |
|:---:|:---:|:---:|
| ![Sales](docs/images/sales.png) | ![Products](docs/images/product.png) | ![Purchases](docs/images/purchase.png) |

| Customers | Suppliers | Payment In |
|:---:|:---:|:---:|
| ![Customers](docs/images/customer.png) | ![Suppliers](docs/images/supplier.png) | ![Payment In](docs/images/paymentin.png) |

| Payment Out | Cash Register | Cash in Hand |
|:---:|:---:|:---:|
| ![Payment Out](docs/images/paymentout.png) | ![Cash Register](docs/images/cashregister.png) | ![Cash in Hand](docs/images/cashinhand.png) |

| Sales Return | Purchase Return | Backup & Restore |
|:---:|:---:|:---:|
| ![Sales Return](docs/images/salesreturn.png) | ![Purchase Return](docs/images/purchasereturn.png) | ![Backup](docs/images/backupandrestore.png) |

| Product Add | Settings | |
|:---:|:---:|:---:|
| ![Product Add](docs/images/productadd.png) | ![Settings](docs/images/setting.png) | …all 27 screenshots in [`docs/images/`](docs/images/) |

## Tech stack

- **.NET 8 MAUI — Blazor Hybrid**: native shell + Razor UI rendered in a WebView (`wwwroot/`)
- **Services layer**: ~20 services (sales, purchase, products, parties, accounts, reports…) — ~8.5k lines of C#
- **SQLite** local database, initialized by `DatabaseInitializerService`
- Targets: `net8.0-android`, `net8.0-ios`, `net8.0-maccatalyst` (+ `net8.0-windows` when built on Windows)

## Project layout

```
QuickproPos/
├── QuickproPos.sln
├── scripts/
│   └── run-mac.sh               one-command macOS build + run
├── QuickproPos/
│   ├── QuickproPos.csproj
│   ├── MauiProgram.cs            app startup, DI registrations
│   ├── Data/                     entities + DbContext + HardwareInfo (Windows-only helpers)
│   ├── Services/                 business logic (sales, purchase, products, reports…)
│   ├── Components/               Blazor components/pages
│   ├── wwwroot/                  CSS/JS/fonts for the Blazor UI
│   ├── Resources/                images, fonts, splash, app icons
│   └── Platforms/                Android / iOS / MacCatalyst / Tizen / Windows glue
└── docs/
    └── images/                   27 module screenshots
```

## Review notes

Findings from the review pass — **fixed in this repo**:

- ✅ **Renamed `Grenite`/`Grantie` → `Granite`** across 13 files (services, models, pages,
  folder names) — consistent naming throughout.
- ✅ **Fixed `Services/DatabaseInitializerService .cs`** — space removed from the filename.
- ✅ **Windows-only WMI code quarantined**: `Data/HardwareInfo.cs` is now wrapped in
  `#if WINDOWS` — it's dead code on non-Windows platforms and no longer emits
  platform-compatibility warnings.
- ✅ **Windows packaging unblocked**: disabled the vendor's hardcoded MSIX code-signing
  thumbprint — `dotnet publish` for Windows now works without their certificate
  (set your own thumbprint to publish to the Microsoft Store).
- ✅ **Removed junk `wwwroot/images/css/`** — a stray duplicate of `app.css`/bootstrap
  that nothing referenced.
- ✅ Added **`scripts/run-mac.sh`** — one-command build + run on macOS.

Still open:

- ⚠️ ~590 build warnings (mostly platform-compatibility CA1416 notices across the
  shared codebase) — normal for cross-platform MAUI code but worth triaging.
- ✅ Clean layered structure (Services / Data / Components) — easy to extend.
- ✅ Builds and runs on MacCatalyst (Apple Silicon) out of the box with the setup above.

## Documentation

The original vendor documentation is bundled under [`Documentation/`](Documentation/Index.html)
(setup steps 1–9, module guides with screenshots).

## License

Provided as-is for review/educational purposes. See vendor terms before commercial use.
