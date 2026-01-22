using System.Collections.Generic;
using System.Windows.Controls;
using AllKeyShopExtension.Data;
using AllKeyShopExtension.Models;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Views
{
    public class ExtensionSettingsView : ISettings
    {
        private readonly IPlayniteAPI playniteAPI;
        private readonly Database database;
        private ExtensionSettings settings;
        private SettingsView settingsView;

        public ExtensionSettingsView(IPlayniteAPI api, Database db, ExtensionSettings extensionSettings)
        {
            playniteAPI = api;
            database = db;
            settings = extensionSettings;
        }

        public UserControl SettingsView
        {
            get
            {
                if (settingsView == null)
                {
                    settings = database.GetSettings();
                    settingsView = new SettingsView(playniteAPI, database, settings);
                }
                return settingsView;
            }
        }

        public void BeginEdit()
        {
            // Reload settings when opening settings view
            settings = database.GetSettings();
            if (settingsView != null)
            {
                settingsView = new SettingsView(playniteAPI, database, settings);
            }
        }

        public void EndEdit()
        {
            // Settings are saved in SettingsView
        }

        public void CancelEdit()
        {
            // No action needed
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
