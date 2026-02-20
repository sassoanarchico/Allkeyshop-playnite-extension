using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

            watchedGames = new ObservableCollection<WatchedGame>();
            freeGames = new ObservableCollection<FreeGame>();
            GamesDataGrid.ItemsSource = watchedGames;
            FreeGamesDataGrid.ItemsSource = freeGames;

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
                GamesDataGrid.Visibility = watchedGames.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                GameCountText.Text = $"{watchedGames.Count} giochi monitorati";

                var lastUpdate = games.Where(g => g.LastUpdate > DateTime.MinValue)
                                      .OrderByDescending(g => g.LastUpdate)
                                      .FirstOrDefault();
                LastUpdateText.Text = lastUpdate != null
                    ? $"Ultimo aggiornamento: {lastUpdate.LastUpdate:g}"
                    : "Ultimo aggiornamento: mai";
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
                FreeGamesDataGrid.Visibility = freeGames.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
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
                    var threshold = searchWindow.PriceThreshold;

                    await AddGameAsync(gameName, pageUrl, threshold);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in AddGameButton_Click");
                StatusText.Text = "Errore nell'apertura della finestra di ricerca.";
            }
        }

        private async Task AddGameAsync(string gameName, string pageUrl, decimal? threshold)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = $"Aggiunta di '{gameName}'...";

                var added = priceService.AddWatchedGame(gameName, pageUrl, threshold);
                if (added)
                {
                    LoadGames();
                    StatusText.Text = $"Aggiornamento prezzo per '{gameName}'...";

                    var game = priceService.GetWatchedGameByName(gameName);
                    if (game != null)
                    {
                        await priceService.UpdateGamePrice(game);
                        LoadGames();
                        StatusText.Text = $"'{gameName}' aggiunto con successo!";
                    }
                }
                else
                {
                    StatusText.Text = $"'{gameName}' è già nella lista.";
                    MessageBox.Show($"Il gioco '{gameName}' è già nella lista dei monitorati.",
                                   "Gioco già presente",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error adding game '{gameName}'");
                StatusText.Text = "Errore durante l'aggiunta.";
                MessageBox.Show($"Errore: {ex.Message}", "Errore",
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
                StatusText.Text = "Aggiornamento prezzi in corso...";

                await priceService.UpdateAllPrices();
                LoadGames();

                StatusText.Text = "Prezzi aggiornati!";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error refreshing prices");
                StatusText.Text = "Errore durante l'aggiornamento.";
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
                StatusText.Text = "Cercando giochi gratis...";

                await freeGamesService.CheckForDailyDeals();

                LoadFreeGames();
                FreeGamesExpander.IsExpanded = true;

                StatusText.Text = $"Trovati {freeGames.Count} giochi gratis recenti";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking free games");
                StatusText.Text = "Errore nel controllo giochi gratis.";
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
                    StatusText.Text = $"Aggiornamento '{game.GameName}'...";
                    await priceService.UpdateGamePrice(game);
                    LoadGames();
                    StatusText.Text = $"'{game.GameName}' aggiornato!";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error updating single game");
                StatusText.Text = "Errore aggiornamento.";
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is int gameId)
                {
                    var result = MessageBox.Show(
                        "Sei sicuro di voler rimuovere questo gioco dalla lista?",
                        "Conferma rimozione",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        priceService.RemoveWatchedGame(gameId);
                        LoadGames();
                        StatusText.Text = "Gioco rimosso.";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error removing game");
            }
        }

        private void GamesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (GamesDataGrid.SelectedItem is WatchedGame game)
                {
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
                logger.Error(ex, "Error opening game URL on double click");
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
    }
}
