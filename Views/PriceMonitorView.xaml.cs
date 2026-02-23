using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AllKeyShopExtension.Models;
using AllKeyShopExtension.Services;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Views
{
    public partial class PriceMonitorView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly PriceService priceService;
        private ObservableCollection<WatchedGame> watchedGames;

        public PriceMonitorView(IPlayniteAPI api, PriceService service)
        {
            InitializeComponent();
            playniteAPI = api;
            priceService = service;
            watchedGames = new ObservableCollection<WatchedGame>();
            GamesDataGrid.ItemsSource = watchedGames;
            LoadGames();
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

                EmptyStatePanel.Visibility = watchedGames.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                GamesDataGrid.Visibility = watchedGames.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error loading games: {ex.Message}");
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.Form
            {
                Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_AddGame"),
                Width = 400,
                Height = 150,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            };

            var label = new System.Windows.Forms.Label
            {
                Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Dialog_GameName"),
                Left = 10,
                Top = 20,
                Width = 100
            };

            var textBox = new System.Windows.Forms.TextBox
            {
                Left = 120,
                Top = 20,
                Width = 250
            };

            var addButton = new System.Windows.Forms.Button
            {
                Text = ResourceProvider.GetString("LOCAllKeyShop_Search_Confirm"),
                Left = 120,
                Top = 60,
                Width = 100,
                DialogResult = System.Windows.Forms.DialogResult.OK
            };

            var cancelButton = new System.Windows.Forms.Button
            {
                Text = ResourceProvider.GetString("LOCAllKeyShop_Search_Cancel"),
                Left = 230,
                Top = 60,
                Width = 100,
                DialogResult = System.Windows.Forms.DialogResult.Cancel
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(textBox);
            dialog.Controls.Add(addButton);
            dialog.Controls.Add(cancelButton);
            dialog.AcceptButton = addButton;
            dialog.CancelButton = cancelButton;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var gameName = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(gameName))
                {
                    await AddGameAsync(gameName);
                }
            }
        }

        private async Task AddGameAsync(string gameName)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_Adding");

                var added = priceService.AddWatchedGame(gameName);
                if (added)
                {
                    LoadGames();
                    StatusText.Text = string.Format(ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_GameAdded"), gameName);
                    
                    // Update price immediately
                    var game = priceService.GetWatchedGameByName(gameName);
                    if (game != null)
                    {
                        await priceService.UpdateGamePrice(game);
                        LoadGames();
                        StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_GameAddedSuccess");
                    }
                }
                else
                {
                    StatusText.Text = string.Format(ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_AlreadyInList"), gameName);
                    MessageBox.Show(string.Format(ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_AlreadyInList"), gameName), ResourceProvider.GetString("LOCAllKeyShop_Sidebar_Dialog_AlreadyExists_Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error adding game: {ex.Message}");
                StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_AddError");
                MessageBox.Show($"Error: {ex.Message}", ResourceProvider.GetString("LOCAllKeyShop_Dialog_Error_Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_Updating");

                await priceService.UpdateAllPrices();
                LoadGames();

                StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_Updated");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error refreshing prices: {ex.Message}");
                StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_UpdateError");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag is int gameId)
                {
                    var result = MessageBox.Show(
                        ResourceProvider.GetString("LOCAllKeyShop_Dialog_ConfirmRemoval"),
                        ResourceProvider.GetString("LOCAllKeyShop_Dialog_ConfirmRemoval_Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        priceService.RemoveWatchedGame(gameId);
                        LoadGames();
                        StatusText.Text = ResourceProvider.GetString("LOCAllKeyShop_PriceMonitor_Status_GameRemoved");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error removing game: {ex.Message}");
            }
        }

        private void GamesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (GamesDataGrid.SelectedItem is WatchedGame game && !string.IsNullOrEmpty(game.LastUrl))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = game.LastUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error opening URL: {ex.Message}");
            }
        }

        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
