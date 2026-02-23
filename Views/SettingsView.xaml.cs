using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AllKeyShopExtension.Data;
using AllKeyShopExtension.Models;
using AllKeyShopExtension.Services;
using AllKeyShopExtension.Views;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Views
{
    public partial class SettingsView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly Database database;
        private readonly PriceService priceService;
        private ExtensionSettings settings;
        private Dictionary<string, CheckBox> platformCheckBoxes;

        public SettingsView(IPlayniteAPI api, Database db, ExtensionSettings extensionSettings, PriceService priceSvc = null)
        {
            InitializeComponent();
            playniteAPI = api;
            database = db;
            priceService = priceSvc;
            settings = extensionSettings;
            platformCheckBoxes = new Dictionary<string, CheckBox>();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load notification settings
            NotificationsEnabledCheckBox.IsChecked = settings.NotificationsEnabled;
            PriceAlertsEnabledCheckBox.IsChecked = settings.PriceAlertsEnabled;

            // Load intervals
            PriceUpdateIntervalTextBox.Text = settings.PriceUpdateIntervalMinutes.ToString();
            FreeGamesCheckIntervalTextBox.Text = settings.FreeGamesCheckIntervalMinutes.ToString();

            // Load platforms
            LoadPlatforms();
        }

        private void LoadPlatforms()
        {
            PlatformsStackPanel.Children.Clear();
            platformCheckBoxes.Clear();

            var availablePlatforms = new List<string>
            {
                "Steam",
                "Epic Games Store",
                "GOG",
                "Xbox",
                "PlayStation",
                "Nintendo Switch",
                "Origin",
                "Uplay",
                "Battle.net"
            };

            foreach (var platform in availablePlatforms)
            {
                var checkBox = new CheckBox
                {
                    Content = platform,
                    Margin = new Thickness(0, 5, 0, 5),
                    IsChecked = settings.IsPlatformEnabled(platform)
                };

                checkBox.Checked += (s, e) => PlatformCheckBox_Changed(platform, true);
                checkBox.Unchecked += (s, e) => PlatformCheckBox_Changed(platform, false);

                platformCheckBoxes[platform] = checkBox;
                PlatformsStackPanel.Children.Add(checkBox);
            }
        }

        private void PlatformCheckBox_Changed(string platform, bool isChecked)
        {
            if (isChecked)
            {
                settings.EnablePlatform(platform);
            }
            else
            {
                settings.DisablePlatform(platform);
            }
        }

        private void NotificationsEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            settings.NotificationsEnabled = NotificationsEnabledCheckBox.IsChecked ?? false;
        }

        private void PriceAlertsEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            settings.PriceAlertsEnabled = PriceAlertsEnabledCheckBox.IsChecked ?? false;
        }

        private void PriceUpdateIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(PriceUpdateIntervalTextBox.Text, out int minutes) && minutes > 0)
            {
                settings.PriceUpdateIntervalMinutes = minutes;
            }
        }

        private void FreeGamesCheckIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(FreeGamesCheckIntervalTextBox.Text, out int minutes) && minutes > 0)
            {
                settings.FreeGamesCheckIntervalMinutes = minutes;
            }
        }

        private void OpenPriceMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (priceService == null)
                {
                    MessageBox.Show(ResourceProvider.GetString("LOCAllKeyShop_Settings_Service_Unavailable"), 
                                   ResourceProvider.GetString("LOCAllKeyShop_Settings_Service_Unavailable_Title"), 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Warning);
                    return;
                }

                var window = new PriceMonitorWindow(playniteAPI, priceService);
                window.Show();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening price monitor from settings");
                MessageBox.Show($"Error: {ex.Message}", 
                               ResourceProvider.GetString("LOCAllKeyShop_Dialog_Error_Title"), 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate intervals
                if (!int.TryParse(PriceUpdateIntervalTextBox.Text, out int priceInterval) || priceInterval <= 0)
                {
                    MessageBox.Show(ResourceProvider.GetString("LOCAllKeyShop_Settings_Save_IntervalError_Price"), 
                                   ResourceProvider.GetString("LOCAllKeyShop_Dialog_Error_Title"), 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(FreeGamesCheckIntervalTextBox.Text, out int freeGamesInterval) || freeGamesInterval <= 0)
                {
                    MessageBox.Show(ResourceProvider.GetString("LOCAllKeyShop_Settings_Save_IntervalError_FreeGames"), 
                                   ResourceProvider.GetString("LOCAllKeyShop_Dialog_Error_Title"), 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Error);
                    return;
                }

                settings.PriceUpdateIntervalMinutes = priceInterval;
                settings.FreeGamesCheckIntervalMinutes = freeGamesInterval;

                // Save to database
                database.SaveSettings(settings);

                // Settings will be reloaded on next access
                // The extension will pick up the new settings automatically

                MessageBox.Show(ResourceProvider.GetString("LOCAllKeyShop_Settings_Save_Success"), 
                               ResourceProvider.GetString("LOCAllKeyShop_Dialog_Success_Title"), 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error saving settings: {ex.Message}");
                MessageBox.Show(string.Format(ResourceProvider.GetString("LOCAllKeyShop_Settings_Save_Error"), ex.Message), 
                               ResourceProvider.GetString("LOCAllKeyShop_Dialog_Error_Title"), 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Error);
            }
        }
    }
}
