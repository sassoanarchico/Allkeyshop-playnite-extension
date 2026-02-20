using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AllKeyShopExtension.Data;
using AllKeyShopExtension.Models;
using AllKeyShopExtension.Services;
using Playnite.SDK;

namespace AllKeyShopExtension.Views
{
    public partial class AllKeyShopSidebarView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly PriceService priceService;
        private readonly FreeGamesService freeGamesService;
        private readonly Database database;
        private readonly Models.ExtensionSettings settings;
        private readonly NotificationService notificationService;
        private ObservableCollection<WatchedGame> watchedGames;
        private ObservableCollection<FreeGame> freeGames;

        public AllKeyShopSidebarView(
            IPlayniteAPI api,
            PriceService priceService,
            FreeGamesService freeGamesService,
            Database database,
            Models.ExtensionSettings settings)
        {
            InitializeComponent();
            this.playniteAPI = api;
            this.priceService = priceService;
            this.freeGamesService = freeGamesService;
            this.database = database;
            this.settings = settings;
            this.notificationService = new NotificationService(api, settings);

            watchedGames = new ObservableCollection<WatchedGame>();
            freeGames = new ObservableCollection<FreeGame>();
            GamesItemsControl.ItemsSource = watchedGames;
            FreeGamesItemsControl.ItemsSource = freeGames;

            LoadGames();
            LoadFreeGames();
        }

        private void LoadGames()
        {
            try
            {
                watchedGames.Clear();
                var games = priceService.GetAllWatchedGames();
                foreach (var game in games)
                {
                    watchedGames.Add(game);
                }

                EmptyStatePanel.Visibility = watchedGames.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                GamesScrollViewer.Visibility = watchedGames.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                GameCountText.Text = $"{watchedGames.Count} games monitored";

                var lastUpdate = games.Where(g => g.LastUpdate > DateTime.MinValue)
                                      .OrderByDescending(g => g.LastUpdate)
                                      .FirstOrDefault();
                LastUpdateText.Text = lastUpdate != null
                    ? $"Last update: {lastUpdate.LastUpdate:g}"
                    : "Last update: never";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading watched games in sidebar");
            }
        }

        private void LoadFreeGames()
        {
            try
            {
                freeGames.Clear();
                var games = freeGamesService.GetRecentFreeGames(TimeSpan.FromDays(7));
                foreach (var game in games)
                {
                    freeGames.Add(game);
                }

                NoFreeGamesText.Visibility = freeGames.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                FreeGamesItemsControl.Visibility = freeGames.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                FreeGamesCountText.Text = freeGames.Count > 0 ? $"({freeGames.Count})" : "";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading free games in sidebar");
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the search window to let user find and select the correct game
                var searchWindow = new SearchGameWindow(priceService.Scraper);
                var result = searchWindow.ShowDialog();

                if (result == true && searchWindow.SelectedResult != null)
                {
                    var selected = searchWindow.SelectedResult;
                    var gameName = selected.Title;
                    var pageUrl = selected.Url;
                    var keyThreshold = searchWindow.KeyPriceThreshold;
                    var accountThreshold = searchWindow.AccountPriceThreshold;
                    var imageUrl = selected.ImageUrl;

                    await AddGameAsync(gameName, pageUrl, keyThreshold, accountThreshold, imageUrl);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in AddGameButton_Click");
                StatusText.Text = "Error opening the search window.";
            }
        }

        private async Task AddGameAsync(string gameName, string pageUrl, decimal? keyThreshold, decimal? accountThreshold, string imageUrl = null)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = $"Adding '{gameName}'...";

                var added = priceService.AddWatchedGame(gameName, pageUrl, keyThreshold, accountThreshold, imageUrl);
                if (added)
                {
                    LoadGames();
                    StatusText.Text = $"Updating price for '{gameName}'...";

                    var game = priceService.GetWatchedGameByName(gameName);
                    if (game != null)
                    {
                        await priceService.UpdateGamePrice(game);
                        LoadGames();
                        StatusText.Text = $"'{gameName}' added successfully!";
                    }
                }
                else
                {
                    StatusText.Text = $"'{gameName}' is already in the list.";
                    MessageBox.Show($"The game '{gameName}' is already in the monitored list.",
                                   "Game already exists",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error adding game '{gameName}'");
                StatusText.Text = "Error while adding the game.";
                MessageBox.Show($"Error: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = "Updating prices...";

                await priceService.UpdateAllPrices();
                LoadGames();

                // Check for price alerts
                CheckAndNotifyPriceAlerts();

                StatusText.Text = "Prices updated!";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error refreshing prices");
                StatusText.Text = "Error while updating prices.";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void ShowFreeGamesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = "Searching for free games...";

                await freeGamesService.CheckForDailyDeals();

                LoadFreeGames();
                FreeGamesExpander.IsExpanded = true;

                StatusText.Text = $"Found {freeGames.Count} recent free games";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking free games");
                StatusText.Text = "Error checking free games.";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void OpenUrlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is WatchedGame game)
                {
                    // "Apri" opens the AllKeyShop page for comparison
                    var url = game.AllKeyShopPageUrl ?? game.LastUrl;
                    if (!string.IsNullOrEmpty(url))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening AllKeyShop page");
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is WatchedGame game)
                {
                    // "Compra" opens the best offer redirect URL
                    var url = game.LastUrl;
                    if (!string.IsNullOrEmpty(url))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening buy URL");
            }
        }

        private async void UpdateSingleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is WatchedGame game)
                {
                    StatusText.Text = $"Updating '{game.GameName}'...";
                    await priceService.UpdateGamePrice(game);
                    LoadGames();

                    // Check alert for this specific game
                    var updatedGame = priceService.GetWatchedGame(game.Id);
                    if (updatedGame != null && priceService.CheckPriceAlert(updatedGame))
                    {
                        notificationService.NotifyPriceAlert(updatedGame);
                    }

                    StatusText.Text = $"'{game.GameName}' updated!";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error updating single game");
                StatusText.Text = "Update error.";
            }
        }

        private void EditThresholdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is WatchedGame game)
                {
                    // Build a WPF dialog for editing dual thresholds (Key + Account)
                    var dialog = new Window
                    {
                        Title = $"Edit Thresholds - {game.GameName}",
                        Width = 420,
                        Height = 300,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize,
                        Background = System.Windows.Media.Brushes.DimGray,
                        Foreground = System.Windows.Media.Brushes.White
                    };

                    var stack = new StackPanel { Margin = new Thickness(20) };

                    var titleLabel = new TextBlock
                    {
                        Text = $"Price thresholds for \"{game.GameName}\":",
                        FontSize = 13,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    stack.Children.Add(titleLabel);

                    // Key threshold row
                    var keyLabel = new TextBlock
                    {
                        Text = game.KeyPriceThreshold.HasValue
                            ? $"Key threshold (current: {game.KeyPriceThreshold.Value:0.00}€):"
                            : "Key threshold (not set):",
                        FontSize = 12,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    stack.Children.Add(keyLabel);

                    var keyTextBox = new TextBox
                    {
                        Text = game.KeyPriceThreshold.HasValue ? game.KeyPriceThreshold.Value.ToString("0.00") : "",
                        FontSize = 14,
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    stack.Children.Add(keyTextBox);

                    // Account threshold row
                    var accountLabel = new TextBlock
                    {
                        Text = game.AccountPriceThreshold.HasValue
                            ? $"Account threshold (current: {game.AccountPriceThreshold.Value:0.00}€):"
                            : "Account threshold (not set):",
                        FontSize = 12,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    stack.Children.Add(accountLabel);

                    var accountTextBox = new TextBox
                    {
                        Text = game.AccountPriceThreshold.HasValue ? game.AccountPriceThreshold.Value.ToString("0.00") : "",
                        FontSize = 14,
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 0, 0, 14)
                    };
                    stack.Children.Add(accountTextBox);

                    // Buttons
                    var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                    var saveBtn = new Button { Content = "Save", Padding = new Thickness(20, 6, 20, 6), Margin = new Thickness(0, 0, 8, 0) };
                    var clearBtn = new Button { Content = "Remove all", Padding = new Thickness(16, 6, 16, 6), Margin = new Thickness(0, 0, 8, 0) };
                    var cancelBtn = new Button { Content = "Cancel", Padding = new Thickness(16, 6, 16, 6), IsCancel = true };

                    decimal? newKeyThreshold = game.KeyPriceThreshold;
                    decimal? newAccountThreshold = game.AccountPriceThreshold;
                    bool saved = false;

                    saveBtn.Click += (s2, e2) =>
                    {
                        // Parse key threshold (empty = null = no notification)
                        var keyTxt = keyTextBox.Text.Trim().Replace(",", ".");
                        if (string.IsNullOrEmpty(keyTxt))
                        {
                            newKeyThreshold = null;
                        }
                        else if (decimal.TryParse(keyTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal kVal) && kVal > 0)
                        {
                            newKeyThreshold = kVal;
                        }
                        else
                        {
                            MessageBox.Show("Enter a valid key price (e.g. 15.50) or leave empty.", "Invalid value",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Parse account threshold (empty = null = no notification)
                        var accTxt = accountTextBox.Text.Trim().Replace(",", ".");
                        if (string.IsNullOrEmpty(accTxt))
                        {
                            newAccountThreshold = null;
                        }
                        else if (decimal.TryParse(accTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal aVal) && aVal > 0)
                        {
                            newAccountThreshold = aVal;
                        }
                        else
                        {
                            MessageBox.Show("Enter a valid account price (e.g. 10.00) or leave empty.", "Invalid value",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        saved = true;
                        dialog.Close();
                    };

                    clearBtn.Click += (s2, e2) =>
                    {
                        newKeyThreshold = null;
                        newAccountThreshold = null;
                        saved = true;
                        dialog.Close();
                    };

                    btnPanel.Children.Add(saveBtn);
                    btnPanel.Children.Add(clearBtn);
                    btnPanel.Children.Add(cancelBtn);
                    stack.Children.Add(btnPanel);

                    dialog.Content = stack;
                    dialog.ShowDialog();

                    if (saved)
                    {
                        priceService.UpdateThresholds(game.Id, newKeyThreshold, newAccountThreshold);
                        LoadGames();

                        var parts = new System.Collections.Generic.List<string>();
                        if (newKeyThreshold.HasValue) parts.Add($"Key: {newKeyThreshold.Value:0.00}€");
                        if (newAccountThreshold.HasValue) parts.Add($"Account: {newAccountThreshold.Value:0.00}€");
                        StatusText.Text = parts.Count > 0
                            ? $"Thresholds for '{game.GameName}' updated: {string.Join(", ", parts)}"
                            : $"All thresholds for '{game.GameName}' removed";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error editing threshold");
                StatusText.Text = "Error editing threshold.";
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is int gameId)
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to remove this game from the list?",
                        "Confirm removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        priceService.RemoveWatchedGame(gameId);
                        LoadGames();
                        StatusText.Text = "Game removed.";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error removing game");
            }
        }

        private void OpenFreeGameUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is FreeGame game
                    && !string.IsNullOrEmpty(game.Url))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = game.Url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening free game URL");
            }
        }

        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckAndNotifyPriceAlerts()
        {
            try
            {
                var alertGames = priceService.GetGamesWithPriceAlerts();
                foreach (var game in alertGames)
                {
                    notificationService.NotifyPriceAlert(game);
                }

                if (alertGames.Count > 0)
                {
                    StatusText.Text = $"Prices updated! {alertGames.Count} price alert(s)!";
                    logger.Info($"Price alerts triggered for {alertGames.Count} game(s) from sidebar");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking price alerts from sidebar");
            }
        }
    }
}
