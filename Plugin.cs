using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public override Guid Id { get; } = Guid.Parse("c1d2e3f4-a5b6-7890-cdef-1234567890ab");

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
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000); // Wait 2 seconds without blocking thread pool
                        try
                        {
                            // Use Dispatcher.CurrentDispatcher or check if application is still running
                            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
                            if (dispatcher != null && !dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished)
                            {
                                dispatcher.Invoke(() =>
                                {
                                    // Double-check that plugin hasn't been disposed
                                    if (priceUpdateTimer != null || freeGamesCheckTimer != null)
                                    {
                                        StartTimers();
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, "Failed to start timers - application may be shutting down");
                        }
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

        /// <summary>
        /// Helper method to ensure all required services are initialized
        /// </summary>
        private void EnsureServicesInitialized()
        {
            InitializeIfNeeded();
            
            if (database == null)
            {
                logger.Error("Database not initialized, creating new instance");
                database = new Database(PlayniteApi);
            }
            
            if (settings == null)
            {
                logger.Warn("Settings not initialized, loading from database");
                settings = database.GetSettings();
                if (settings == null)
                {
                    settings = new Models.ExtensionSettings();
                    database.SaveSettings(settings);
                }
            }
            
            // Ensure priceService is initialized
            if (priceService == null)
            {
                logger.Warn("PriceService not initialized, initializing now");
                if (scraper == null)
                {
                    scraper = new AllKeyShopScraper(PlayniteApi);
                }
                priceService = new PriceService(database, scraper, PlayniteApi);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            try
            {
                EnsureServicesInitialized();
                return new ExtensionSettingsView(PlayniteApi, database, settings, priceService);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in GetSettings");
                // Return a basic settings view even if there's an error
                try
                {
                    EnsureServicesInitialized();
                    return new ExtensionSettingsView(PlayniteApi, database, settings, priceService);
                }
                catch
                {
                    // Last resort - return null and let Playnite handle it
                    return null;
                }
            }
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            try
            {
                EnsureServicesInitialized();
                return new SettingsView(PlayniteApi, database, settings, priceService);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in GetSettingsView");
                // Return a basic settings view even if there's an error
                try
                {
                    EnsureServicesInitialized();
                    return new SettingsView(PlayniteApi, database, settings, priceService);
                }
                catch (Exception ex2)
                {
                    logger.Error(ex2, "Critical error creating SettingsView");
                    // Return a simple error message UserControl
                    var errorControl = new UserControl();
                    errorControl.Content = new TextBlock 
                    { 
                        Text = "Errore nel caricamento delle impostazioni. Controlla i log per dettagli.",
                        Margin = new Thickness(20),
                        TextWrapping = TextWrapping.Wrap
                    };
                    return errorControl;
                }
            }
        }

        // Sidebar integration
        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            return new List<SidebarItem>
            {
                new SidebarItem
                {
                    Title = "AllKeyShop",
                    Type = SiderbarItemType.View,
                    Icon = new TextBlock
                    {
                        Text = "\uEF40",
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        FontSize = 20
                    },
                    Opened = () =>
                    {
                        try
                        {
                            EnsureServicesInitialized();
                            return new AllKeyShopSidebarView(
                                PlayniteApi,
                                priceService,
                                freeGamesService,
                                database,
                                settings);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error creating sidebar view");
                            var errorControl = new UserControl();
                            errorControl.Content = new TextBlock
                            {
                                Text = "Errore nel caricamento della sidebar AllKeyShop. Controlla i log.",
                                Margin = new Thickness(20),
                                TextWrapping = TextWrapping.Wrap
                            };
                            return errorControl;
                        }
                    }
                }
            };
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

                // Check for price alerts after updating
                try
                {
                    var alertGames = priceService.GetGamesWithPriceAlerts();
                    foreach (var game in alertGames)
                    {
                        notificationService?.NotifyPriceAlert(game);
                    }

                    if (alertGames.Count > 0)
                    {
                        logger.Info($"Price alerts triggered for {alertGames.Count} game(s)");
                    }
                }
                catch (Exception alertEx)
                {
                    logger.Error(alertEx, "Error checking price alerts");
                }
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
