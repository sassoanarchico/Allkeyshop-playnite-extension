using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AllKeyShopExtension.Models;
using AllKeyShopExtension.Services;
using Playnite.SDK;

namespace AllKeyShopExtension.Views
{
    // Simple value converters for the search window
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public partial class SearchGameWindow : Window
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly AllKeyShopScraper scraper;
        private ObservableCollection<SearchResult> searchResults;

        /// <summary>
        /// The search result selected by the user, or null if cancelled.
        /// </summary>
        public SearchResult SelectedResult { get; private set; }

        /// <summary>
        /// Optional price threshold set by the user.
        /// </summary>
        public decimal? PriceThreshold { get; private set; }

        public SearchGameWindow(AllKeyShopScraper scraper, string initialSearch = null)
        {
            InitializeComponent();
            this.scraper = scraper;
            searchResults = new ObservableCollection<SearchResult>();
            ResultsListBox.ItemsSource = searchResults;

            if (!string.IsNullOrWhiteSpace(initialSearch))
            {
                SearchTextBox.Text = initialSearch;
                Loaded += async (s, e) => await PerformSearch(initialSearch);
            }
            else
            {
                SearchTextBox.Focus();
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch(SearchTextBox.Text.Trim());
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearch(SearchTextBox.Text.Trim());
            }
        }

        private async System.Threading.Tasks.Task PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return;

            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                SearchButton.IsEnabled = false;
                StatusText.Text = $"Ricerca di '{query}'...";
                searchResults.Clear();
                ConfirmButton.IsEnabled = false;

                var results = await scraper.SearchGamesAsync(query);

                foreach (var result in results)
                {
                    searchResults.Add(result);
                }

                StatusText.Text = results.Count > 0
                    ? $"Trovati {results.Count} risultati. Seleziona il gioco corretto."
                    : "Nessun risultato trovato. Prova con un altro nome.";
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error searching for '{query}'");
                StatusText.Text = $"Errore nella ricerca: {ex.Message}";
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = true;
            }
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmButton.IsEnabled = ResultsListBox.SelectedItem is SearchResult;
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is SearchResult)
            {
                ConfirmSelection();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedResult = null;
            DialogResult = false;
            Close();
        }

        private void ConfirmSelection()
        {
            if (ResultsListBox.SelectedItem is SearchResult result)
            {
                SelectedResult = result;

                // Parse threshold
                var thresholdText = ThresholdTextBox.Text.Trim().Replace(",", ".");
                if (decimal.TryParse(thresholdText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal t) && t > 0)
                {
                    PriceThreshold = t;
                }

                DialogResult = true;
                Close();
            }
        }
    }
}
