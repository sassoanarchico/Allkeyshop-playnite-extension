# TODO — AllKeyShop Price Monitor

Roadmap and ideas for upcoming versions.

## v1.1.0 — UX Improvements

- [ ] **Price history**: Record price trends over time and display a mini-chart (sparkline) in the game card
- [ ] **List sorting**: Allow sorting games by name, price, update date, threshold
- [ ] **Search filter**: Add a search/filter bar in the monitored games list
- [ ] **Import from Playnite Library**: Button to quickly add games already in the Playnite library to the watchlist
- [ ] **Visual price thresholds**: Highlight games below threshold in green and above in red

## v1.2.0 — Advanced Features

- [ ] **Multi-currency**: Support for currencies other than EUR (USD, GBP, etc.) with selection in settings
- [ ] **Steam wishlist sync**: Automatically import Steam wishlist to monitor prices
- [ ] **Edition comparison**: Show different available editions in the card (Standard, Deluxe, GOTY, etc.)
- [ ] **CSV/JSON export**: Export the monitored games list with current prices
- [ ] **Toast notification click**: Open the browser directly to the game page when clicking the Windows notification

## v1.3.0 — Integration & Sharing

- [ ] **List sharing**: Generate a shareable link for your watchlist
- [ ] **Discord/Telegram webhook**: Send notifications to external bots when a price drops below threshold
- [ ] **Plugin API**: Expose an internal API for other Playnite plugins

## Known Bugs / Technical Improvements

- [ ] Gracefully handle cases where AllKeyShop changes HTML structure (auto-detection failure + user notification)
- [ ] Add retry with exponential backoff for failed HTTP requests
- [ ] Smarter cache: invalidate only games that haven't received an update, not the entire cache
- [ ] Improve date parsing in the database (use DateTimeOffset for timezone handling)
- [ ] Unit tests for core services (PriceService, AllKeyShopScraper, Database)
