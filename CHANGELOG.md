# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2026-02-20

### Added
- **Dual price thresholds**: Separate threshold for Key price and Account price. Each can be set independently or left empty (no notification).
- **Independent price alerts**: Key alerts (🔑) and Account alerts (👤) fire independently with dedicated notification IDs and messages.
- **Dual threshold edit dialog**: The "Edit threshold" dialog now shows two input fields (Key + Account) with current values. "Remove all" clears both.
- **Dual threshold on add**: The search window now has two threshold fields ("Key threshold" and "Account threshold") when adding a game.

### Fixed
- **Account price triggering key notifications**: Previously, a single threshold was shared between key and account prices, causing unwanted notifications when only the account price dropped. Now each price type is checked against its own threshold only.

### Changed
- **Database migration**: Added `KeyPriceThreshold` and `AccountPriceThreshold` columns. Existing single thresholds are auto-migrated to `KeyPriceThreshold`.
- **Threshold display**: Cards now show "K:15.00€ | A:20.00€" format, or just one, or "—" if none set.
- **NotificationService**: Rewritten to send separate notifications per threshold type instead of a single combined alert.

## [1.0.0] - 2026-02-20

### Added
- **Game icons in monitored list**: Each added game displays a thumbnail from AllKeyShop search. Placeholder with controller icon for games without an image.
- **`ThresholdDisplay` property**: Shows the formatted threshold (e.g. "15.00€") or "—" if not set.
- **Free games count**: The expander shows the number of free games found.

### Fixed
- **Broken button icons**: All toolbar and action buttons now use explicit `FontFamily="Segoe MDL2 Assets"` for icons. Resolved the issue of empty squares instead of icons.
- **Timer never started**: Removed erroneous guard condition in Plugin.cs that prevented auto-update timers from starting.
- **Database**: Added `ImageUrl` column with automatic migration.

### Changed
- **Complete professional UI**: Full rewrite of the sidebar with card-based layout. Each game is displayed as a card with thumbnail, name, key/account prices, threshold, update date, and action buttons with MDL2 icons.
- **Modern toolbar**: Buttons with Segoe MDL2 Assets icons + text, DockPanel layout with status text.
- **Free games section**: Converted from DataGrid to ItemsControl with compact cards.
- **Improved empty state**: Large controller icon + inviting text.
- **Loading overlay**: Sync icon + thin progress bar.
- **Clean status bar**: Separator with middle dot, reduced font.

## [0.2.4] - 2025-07-24

### Added
- **Edit price threshold**: Added ✏ (edit) button in the Actions column of the monitored games table. Clicking opens a dialog to edit the price threshold of a game. Supports save, clear threshold ("Clear"), and cancel.
- **Windows toast notifications**: Price alerts, free games, and price update notifications now also appear as Windows system notifications (toast), in addition to Playnite. Uses the `Microsoft.Toolkit.Uwp.Notifications` package. Price update toasts are sent only for price drops.

### Fixed
- `ShowInfo` and `ShowError` now include both the title and message in the notification text.

## [0.2.3] - 2025-02-20

### Fixed
- **Price notifications not working**: Price alert notifications are now sent correctly when the price drops below the configured threshold.
  - The `UpdatePrices()` method in the plugin now calls `CheckPriceAlert()` after each price update and sends notifications automatically.
  - `PriceAlertsEnabled` is now enabled by default (`true` instead of `false`).
  - `NotifyPriceAlert()` rewritten: includes the current price, seller, and threshold in the notification text (e.g. "💰 Cyberpunk 2077 - Price dropped to 8.41€ (K4G) | Threshold: 10.00€").
  - Uses stable notification IDs (`allkeyshop-price-alert-{id}`) to avoid duplicates.
  - Clicking the notification opens the purchase page.
- **Free games notifications**: The message now includes the complete list of found games.
- **Price update notifications**: Correct price format (€ instead of `ToString("C")` which could use other currencies).

### Added
- Price alerts from sidebar: the "Refresh Prices" and "Refresh" (single) buttons now check thresholds and send instant notifications.
- Detailed logging for each notification sent (`logger.Info`).

## [0.2.2] - 2025-02-20

### Fixed
- **Theme and colors**: Complete rewrite of the style system for optimal compatibility with all Playnite themes (dark and light).
  - Removed all explicit DataGrid backgrounds (`ControlBackgroundBrush`, `WindowBackgroundBrush`) that caused conflicts with themes. Now uses `Background="Transparent"` to correctly inherit from the theme background.
  - Added implicit styles in `UserControl.Resources` for `DataGridRow`, `DataGridCell`, and `DataGridColumnHeader` with triggers for hover and selection that maintain `TextBrush` as the text color in every state.
  - Used neutral semi-transparent overlays (`#18808080`, `#28808080`) for hover and selection that work on both dark and light backgrounds.
  - Added `Foreground="{DynamicResource TextBrush}"` to the Expander for the header text.
  - Added `ListBoxItem` styles with triggers in the search window to maintain correct colors during selection.
  - Loading overlay now with solid background (`#CC000000`) and explicit white text, visible on any theme.
- **Delete button not visible**: Actions column now uses `Width="Auto"` with `MinWidth="140"` and compact buttons (`FontSize="11"`, `Padding="4,1"`). Added `ScrollViewer.HorizontalScrollBarVisibility="Auto"` to the DataGrid for horizontal scrolling. Renamed "Aggiorna" to "Agg." to save space. Delete button highlighted in red (`#FF6B68`).
- **Free games (widget)**: Free games scraping now uses the official AllKeyShop widget (`widget.allkeyshop.com`) instead of the blog page `daily-game-deals`, correctly retrieving all available free games (28+ titles vs 6 articles). Each game shows platform and type (e.g. "Steam - Free to keep", "Amazon - Free with Prime", "Xbox - Gamepass").

## [0.2.1] - 2025-07-10

### Fixed
- **Fee-included pricing**: The displayed price now includes payment fees ("Lowest Fees"), calculated as the minimum between the card price and the PayPal price. Added `LowestFeePrice` property to the `OfferInfo` model.
- **Free games**: The free games list is now correctly populated from AllKeyShop's [Daily Game Deals](https://www.allkeyshop.com/blog/daily-game-deals/) page, showing free offers, deals, and active promotions. Added `GetDailyGameDeals()` and `CheckForDailyDeals()` methods.
- **Theme/Font**: The interface now correctly adapts to the active Playnite theme (dark/light). Fixed the issue of dark text on dark background. All components (DataGrid, ListBox, windows) now use Playnite theme resources (`TextBrush`, `WindowBackgroundBrush`, `ControlBackgroundBrush`, `PopupBackgroundBrush`). Added styles for DataGrid ColumnHeader and Cell.

## [0.2.0] - 2025-07-09

### Added
- **New scraping engine**: Complete rewrite of scraping based on the `gamePageTrans` JSON embedded in AllKeyShop pages, ensuring structured and reliable data.
- **Game search**: New search window (`SearchGameWindow`) that uses the AllKeyShop search API to find and add games to monitor.
- **Separate Key and Account prices**: Distinction between "key" and "account" offer types with separate prices in the monitoring table.
- **Database migration**: Automatic addition of new columns (`KeyPrice`, `AccountPrice`, `BuyUrl`, `BestMerchant`, `IsAccountOffer`) to the existing SQLite database.
- **Complete OfferInfo model**: New `OfferInfo` class with support for vouchers, editions, regions, fees (card/PayPal), and direct purchase URL.

### Changed
- Updated `GamePrice` and `WatchedGame` model structures with new fields.
- `PriceService` updated for the new scraping flow.
- Sidebar view updated with Key/Account/Best columns.

## [0.1.2] - 2025-07-08

### Fixed
- **XAML loading error**: Resolved `XamlParseException` caused by incorrect references to `SystemColors.WindowTextColorKey` in XAML templates.
- **Sidebar view**: Implemented the sidebar view for price monitoring within Playnite.

## [0.1.1] - 2025-01-22

### Fixed
- **AllKeyShop scraping**: Improved CSS selectors to correctly find price and seller
  - Added multiple selectors to handle different HTML structures of the site
  - Improved price extraction to support various formats (€12.99, 12,99€, $12.99, etc.)
  - Added detailed logging for scraping debug
- **LastUpdate handling**: Fixed the issue where the last update date was shown even when the price was not available
  - LastUpdate is now updated only when the price is actually available
- **Graphics and Theme**: Removed all hardcoded colors and adapted the interface to the Playnite theme
  - Use of DynamicResource with SystemColors to automatically adapt to dark/light theme
  - Styles for DataGrid and controls that follow the theme's color palette
  - All views (PriceMonitorView, PriceMonitorWindow, SettingsView) now adapt to the theme

### Improved
- **Scraping**: Added more CSS selectors to increase the probability of finding correct data
- **Logging**: Added detailed logging to trace the scraping process
- **Price extraction**: Improved regex to extract prices from more varied formats

## [0.1.0] - 2024-12-19

### Added

#### Core Features
- **Price Monitoring**: Complete system to monitor game prices from AllKeyShop
  - Add games to monitor through the user interface
  - Display of the lowest price found on AllKeyShop
  - Automatic price updates at configurable intervals
  - Click on a game to open the AllKeyShop page directly
  - Remove games from the monitoring list

- **Free Games Notifications**: Notification system for new available free games
  - Automatic check of free games for selected platforms
  - Notifications when new free games are found
  - Platform configuration (Steam, Epic, GOG, Xbox, PlayStation, Nintendo Switch, Origin, Uplay, Battle.net)
  - Configurable check interval

- **User Interface**
  - Main view (PriceMonitorView) with complete monitored games list
  - Separate window for price monitoring (PriceMonitorWindow)
  - Complete settings view with all configuration options
  - Loading indicators during operations
  - Empty state when no games are monitored

#### Services and Business Logic
- **AllKeyShopScraper**: Scraping service to retrieve prices and free games
  - HTML scraping with HtmlAgilityPack
  - Rate limiting to avoid bans
  - Result caching to optimize requests
  - Robust error handling

- **PriceService**: Price management service
  - Add/remove games to monitor
  - Single or batch price updates
  - Detection of games needing update
  - Price threshold support (prepared for future feature)

- **FreeGamesService**: Free games management service
  - Check for new free games per platform
  - Compare with history to detect new games
  - Filter by enabled platforms

- **NotificationService**: Playnite notification service
  - New free games notifications
  - Price alert notifications (prepared for future feature)
  - Price update notifications
  - Click on notifications to open AllKeyShop

#### Data Persistence
- **SQLite Database**: Local persistence system
  - `WatchedGames` table for monitored games
  - `FreeGamesHistory` table for free games history
  - `Settings` table for extension settings
  - Thread-safe operations
  - Automatic database creation on first run

#### Background Tasks
- **Price Update Timer**: Automatic periodic update
  - Configurable interval (default: 60 minutes)
  - Asynchronous update to avoid blocking the UI
  - Error handling with logging

- **Free Games Check Timer**: Automatic periodic check
  - Configurable interval (default: 120 minutes)
  - Check only for enabled platforms
  - Automatic notifications for new games

#### Configuration
- **Extension Settings**:
  - Enable/disable notifications
  - Enable/disable price alerts (prepared for future feature)
  - Configure price update interval
  - Configure free games check interval
  - Select platforms for free games notifications
  - Automatic settings save

#### Project Structure
- Modular architecture with separation of concerns
- Well-defined data models
- Reusable services
- Responsive WPF views
- Utilities for common operations (HttpClientHelper)

#### Documentation
- Complete README.md with installation and usage instructions
- Commented and well-structured code
- Error handling with detailed logging

### Technical Notes
- Target Framework: .NET Framework 4.8
- Dependencies: HtmlAgilityPack, Newtonsoft.Json, System.Data.SQLite.Core
- Compatibility: Playnite 10.x or later
- Thread-safe for concurrent operations
- Robust error handling with appropriate try-catch
- Rate limiting to avoid issues with AllKeyShop

### Prepared for Future Features
- Price threshold alert system (infrastructure ready)
- Price history over time
- Multi-currency support
- Steam wishlist integration
- Data export

### Notes
- AllKeyShop scraping is subject to the site's policies
- HTML selectors may need updating if AllKeyShop changes its structure
- The database is created automatically on first run
- Notifications use Playnite's native notification system
