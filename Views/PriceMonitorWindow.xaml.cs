using System.Windows;
using AllKeyShopExtension.Services;
using Playnite.SDK;

namespace AllKeyShopExtension.Views
{
    public partial class PriceMonitorWindow : Window
    {
        private PriceMonitorView priceMonitorView;

        public PriceMonitorWindow(IPlayniteAPI api, PriceService priceService)
        {
            InitializeComponent();
            priceMonitorView = new PriceMonitorView(api, priceService);
            ContentGrid.Children.Add(priceMonitorView);
        }
    }
}
