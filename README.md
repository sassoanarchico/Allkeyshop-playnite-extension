<div align="center">

# AllKeyShop Price Monitor

**Playnite extension that monitors game prices from AllKeyShop**

[![Version](https://img.shields.io/badge/version-1.2.0-blue.svg)]()
[![Platform](https://img.shields.io/badge/platform-Playnite%2010+-purple.svg)]()
[![Framework](https://img.shields.io/badge/.NET-Framework%204.8-green.svg)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg)](LICENSE)

Monitor your game prices from AllKeyShop, get Windows notifications when a price drops below your threshold, and discover free games — all directly from the Playnite sidebar.

</div>

---

## Table of Contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Settings](#settings)
- [Project Structure](#project-structure)
- [Development](#development)
- [Roadmap](#roadmap)
- [Changelog](#changelog)
- [License](#license)

---

## Features

### Price Monitoring
| Feature | Description |
|---------|-------------|
| **Watchlist** | Add games to your personal list with integrated AllKeyShop search |
| **Key + Account Prices** | Each game shows the best price for digital key and account |
| **Fee-included Pricing** | Displayed price includes payment fees (LowestFeePrice) |
| **Dual Price Thresholds** | Set separate thresholds for Key and Account prices — each triggers independent alerts |
| **Auto-refresh** | Prices update in the background at configurable intervals |
| **Game Thumbnails** | Each game displays its cover art from AllKeyShop search |

### Notifications
| Feature | Description |
|---------|-------------|
| **Windows Toast** | Native Windows 10/11 notifications (notification area + action center) |
| **Playnite Notifications** | Alerts also appear in Playnite's built-in notification system |
| **Price Alert** | When a key or account price drops below its respective threshold |
| **Free Games** | When new free games are found on any platform |
| **Price Update** | Notifications on price changes (toast only for price drops) |

### Free Games
| Feature | Description |
|---------|-------------|
| **AllKeyShop Widget** | Scrapes the official AllKeyShop widget for maximum coverage |
| **Multi-platform** | Steam, Epic, GOG, Xbox Game Pass, PlayStation, Amazon Prime, and more |
| **Offer Type** | Shows the type (Free to Keep, Free with Prime, Gamepass, Free DLC, etc.) |
| **History** | Local database to avoid duplicate notifications |

### Interface
| Feature | Description |
|---------|-------------|
| **Integrated Sidebar** | Full app in Playnite's sidebar |
| **Card Layout** | Each game is a card with thumbnail, prices, threshold, and actions |
| **Segoe MDL2 Icons** | Native Windows icons in toolbar and action buttons |
| **Adaptive Theme** | Automatically adapts to any Playnite theme (dark/light) |
| **Advanced Search** | Search window with image preview, price, and platform |

### Localization
| Feature | Description |
|---------|-------------|
| **5 Languages** | English, Italian, Spanish, French, German |
| **Auto-detection** | Automatically loads the user's Playnite language |
| **Full Coverage** | ~120 localization keys covering all UI strings, dialogs, and notifications |
| **Fallback** | Falls back to English for unsupported languages |

> **⚠️ Note:** Translations were generated using artificial intelligence and may contain errors or inaccuracies. If you find any issues, please report them or submit a correction.

---

## Screenshots

> The main sidebar shows monitored games as cards with thumbnails, prices, and quick actions.
> The bottom section (expandable) lists recent free games.

---

## Installation

### Quick Method (.pext file)

1. Download `AllKeyShopExtension-1.2.0.pext` from the [Releases](https://github.com/sassoanarchico/allkeyshop-playnite/releases) page
2. In Playnite: **Menu > Extensions > Install extension from file**
3. Select the downloaded `.pext` file
4. **Restart Playnite**
5. The AllKeyShop icon will appear in the sidebar

### Manual Installation (developers)

```powershell
# Clone the repository
git clone https://github.com/sassoanarchico/allkeyshop-playnite.git
cd allkeyshop-playnite/AllKeyShopExtension

# Build and package
.\build.ps1
```

Or copy the compiled folder to `%AppData%\Playnite\Extensions\AllKeyShopExtension\`.

---

## Quick Start

### Adding a Game

1. Click the **AllKeyShop** icon in the Playnite sidebar
2. Click the **+ Add** button in the toolbar
3. Search for the game name in the search window
4. Select the correct result (with image preview and price)
5. (Optional) Enter a **Key threshold** and/or **Account threshold** in EUR
6. Click **Add** — the price will be updated immediately

### Game Actions

Each card in the list has 5 action buttons:

| Icon | Action | Description |
|:----:|--------|-------------|
| 🌐 | **Open** | Opens the AllKeyShop page for the game in the browser |
| 🛒 | **Buy** | Go directly to the best price offer |
| ↻ | **Refresh** | Update the price for this specific game |
| ✏ | **Edit thresholds** | Opens the dialog to edit/remove the Key and Account price thresholds |
| 🗑 | **Remove** | Removes the game from the watchlist |

### Free Games

1. Click **Free** in the toolbar to check immediately
2. The list appears in the expandable section at the bottom
3. Click the link icon to open the offer page

---

## Settings

Accessible from **Menu > Extensions > Extension settings > AllKeyShop Price Monitor**.

| Setting | Default | Description |
|---------|---------|-------------|
| Notifications enabled | `Yes` | Enable/disable all notifications |
| Price alerts | `Yes` | Notifications when a price drops below threshold |
| Price update interval | `60 min` | Frequency of automatic price updates |
| Free games check interval | `120 min` | Frequency of free games checks |
| Platforms | None | Platforms to monitor for free games |

**Supported platforms:** Steam, Epic Games Store, GOG, Xbox, PlayStation, Nintendo Switch, Origin, Uplay, Battle.net

---

## Project Structure

```
AllKeyShopExtension/
├── Plugin.cs                  # Entry point: GenericPlugin, timers, sidebar
├── extension.yaml             # Playnite manifest
│
├── Models/
│   ├── WatchedGame.cs         # Watched game (prices, threshold, thumbnail)
│   ├── FreeGame.cs            # Free game found
│   ├── GamePrice.cs           # Price scraping result + OfferInfo
│   ├── SearchResult.cs        # AllKeyShop search result
│   └── ExtensionSettings.cs   # Settings model
│
├── Services/
│   ├── AllKeyShopScraper.cs   # Scraping: search API + gamePageTrans JSON
│   ├── PriceService.cs        # Price and watchlist business logic
│   ├── FreeGamesService.cs    # Free games logic (widget)
│   └── NotificationService.cs # Playnite + Windows Toast notifications
│
├── Data/
│   └── Database.cs            # SQLite: CRUD + automatic migrations
│
├── Views/
│   ├── AllKeyShopSidebarView  # Main sidebar (XAML + code-behind)
│   ├── SearchGameWindow       # Game search dialog
│   ├── SettingsView           # Settings page
│   └── PriceMonitorView/Window # Price monitor view (legacy)
│
├── Localization/
│   ├── en_US.xaml             # English (default/fallback)
│   ├── it_IT.xaml             # Italian
│   ├── es_ES.xaml             # Spanish
│   ├── fr_FR.xaml             # French
│   └── de_DE.xaml             # German
│
├── Utilities/
│   └── HttpClientHelper.cs    # HTTP client wrapper
│
├── build.ps1                  # Build + .pext packaging script
├── CHANGELOG.md               # Version history
└── TODO.md                    # Future feature roadmap
```

---

## Development

### Prerequisites

- Visual Studio 2022+ or VS Code with C# extension
- .NET Framework 4.8 Developer Pack
- Playnite installed (for Playnite.SDK.dll)

### Building

```powershell
cd AllKeyShopExtension
.\build.ps1
```

The script:
1. Automatically finds `Playnite.SDK.dll` in `%LOCALAPPDATA%\Playnite`
2. Compiles in Release mode
3. Copies required DLLs (HtmlAgilityPack, Newtonsoft.Json, SQLite, Toast Notifications)
4. Creates the `.pext` file with the version read from `extension.yaml`

### NuGet Dependencies

| Package | Version | Usage |
|---------|---------|-------|
| HtmlAgilityPack | 1.11.71 | HTML parsing for scraping |
| Newtonsoft.Json | 13.0.3 | JSON serialization (gamePageTrans, settings) |
| System.Data.SQLite.Core | 1.0.118 | Local database |
| Microsoft.Toolkit.Uwp.Notifications | 7.1.3 | Windows 10/11 Toast notifications |

### How Scraping Works

1. **Search**: `POST` to AllKeyShop's `quicksearch` AJAX endpoint → HTML result parsing
2. **Prices**: `GET` on the game page → extraction of inline `gamePageTrans` JSON
3. **Price calculation**: For each offer, the final price is the minimum between `priceCard` and `pricePaypal` (LowestFeePrice)
4. **Free games**: `GET` from `widget.allkeyshop.com` → HTML slide parsing

---

## Roadmap

See [TODO.md](TODO.md) for the full roadmap.

**Coming soon:**
- Price history with mini-chart
- Import from Playnite library
- Multi-currency support
- Steam wishlist sync
- CSV/JSON export

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for the complete list of changes.

### Latest versions

- **v1.2.0** — Multi-language localization (EN, IT, ES, FR, DE), ~120 keys, auto-detection
- **v1.1.0** — Dual price thresholds (Key + Account), independent alerts, fixed unwanted account notifications
- **v1.0.0** — Professional card-based UI, game thumbnails, MDL2 icons, timer fix, broken icons fix
- **v0.2.4** — Edit price threshold, Windows Toast notifications
- **v0.2.3** — Fix non-working price notifications
- **v0.2.2** — Theme/color rework, free games widget
- **v0.2.1** — Fee-included pricing (LowestFeePrice)
- **v0.2.0** — New gamePageTrans scraping, game search, key/account prices

---

## License

Distributed under the [MIT](LICENSE) license.

---

## Author

**Sassoanarchico** — [GitHub](https://github.com/sassoanarchico)

---

<div align="center">
<sub>Made with coffee and Python regex for Playnite</sub>
</div>
