using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AllKeyShopExtension.Data;
using AllKeyShopExtension.Services;
using AllKeyShopExtension.Views;

namespace AllKeyShopExtension
{
    public class AllKeyShopPlugin : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private Database database;
        private AllKeyShopScraper scraper;
        private PriceService priceService;
        private FreeGamesService freeGamesService;
        private NotificationService notificationService;
        private Models.ExtensionSettings settings;
        private DispatcherTimer priceUpdateTimer;
        private DispatcherTimer freeGamesCheckTimer;

        public override Guid Id { get; } = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        public AllKeyShopPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            // Initialize lazily when needed
            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (database == null)
            {
                try
                {
                    logger.Info("Initializing AllKeyShop Extension...");

                    // Initialize database
                    database = new Database(PlayniteApi);
                    logger.Debug("Database initialized");

                    // Load settings
                    settings = database.GetSettings();
                    if (settings == null)
                    {
                        settings = new Models.ExtensionSettings();
                    }
                    database.SaveSettings(settings); // Save defaults if new
                    logger.Debug("Settings loaded");

                    // Initialize services
                    scraper = new AllKeyShopScraper(PlayniteApi);
                    logger.Debug("Scraper initialized");

                    priceService = new PriceService(database, scraper, PlayniteApi);
                    logger.Debug("PriceService initialized");

                    freeGamesService = new FreeGamesService(database, scraper, PlayniteApi);
                    logger.Debug("FreeGamesService initialized");

                    notificationService = new NotificationService(PlayniteApi, settings);
                    logger.Debug("NotificationService initialized");

                    // Start background tasks (delayed to avoid blocking initialization)
                    Task.Run(() =>
                    {
                        Thread.Sleep(2000); // Wait 2 seconds
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            StartTimers();
                        });
                    });

                    logger.Info("AllKeyShop Extension initialized successfully");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to initialize AllKeyShop Extension");
                    // Don't throw - allow extension to load even if initialization fails
                }
            }
        }

        public override void Dispose()
        {
            StopTimers();
            scraper?.Dispose();
            base.Dispose();
        }

        public void OpenPriceMonitor()
        {
            try
            {
                InitializeIfNeeded();
                if (priceService == null)
                {
                    logger.Error("PriceService not initialized");
                    return;
                }
                var window = new PriceMonitorWindow(PlayniteApi, priceService);
                window.Show();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening price monitor window");
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            InitializeIfNeeded();
            return new ExtensionSettingsView(PlayniteApi, database, settings);
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            InitializeIfNeeded();
            return new SettingsView(PlayniteApi, database, settings, priceService);
        }

        private void StartTimers()
        {
            try
            {
                if (settings == null)
                {
                    logger.Warn("Settings not initialized, cannot start timers");
                    return;
                }

                // Price update timer
                if (priceUpdateTimer == null)
                {
                    priceUpdateTimer = new DispatcherTimer();
                    priceUpdateTimer.Interval = TimeSpan.FromMinutes(Math.Max(1, settings.PriceUpdateIntervalMinutes));
                    priceUpdateTimer.Tick += async (s, e) => await UpdatePrices();
                }
                priceUpdateTimer.Start();

                // Free games check timer
                if (freeGamesCheckTimer == null)
                {
                    freeGamesCheckTimer = new DispatcherTimer();
                    freeGamesCheckTimer.Interval = TimeSpan.FromMinutes(Math.Max(1, settings.FreeGamesCheckIntervalMinutes));
                    freeGamesCheckTimer.Tick += async (s, e) => await CheckFreeGames();
                }
                freeGamesCheckTimer.Start();

                // Initial update (non-blocking)
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(5000); // Wait 5 seconds before first update
                        await UpdatePrices();
                        await CheckFreeGames();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error in initial update");
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error starting timers");
            }
        }

        private void StopTimers()
        {
            priceUpdateTimer?.Stop();
            freeGamesCheckTimer?.Stop();
        }

        private async Task UpdatePrices()
        {
            try
            {
                await priceService.UpdateAllPrices();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error updating prices");
            }
        }

        private async Task CheckFreeGames()
        {
            try
            {
                if (settings.EnabledPlatforms.Count == 0)
                {
                    return;
                }

                var newGames = await freeGamesService.CheckForNewFreeGames(settings.EnabledPlatforms);
                
                if (newGames.Count > 0)
                {
                    notificationService.NotifyNewFreeGames(newGames);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking free games");
            }
        }

        public void ReloadSettings()
        {
            settings = database.GetSettings();
            notificationService = new NotificationService(PlayniteApi, settings);
            
            // Restart timers with new intervals
            StopTimers();
            StartTimers();
        }
    }
}
