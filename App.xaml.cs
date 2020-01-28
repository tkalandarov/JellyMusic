using JellyMusic.Core;
using JellyMusic.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JellyMusic
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly static string SettingsPath = Path.Combine(Directory.GetCurrentDirectory(), @"DATA\AppSettings.json");
        public static AppSettings Settings { get; private set; }

        public App()
        {
            Dispatcher.UnhandledException += (sender, e) =>
            {
                e.Handled = true;
                MessageBox.Show($"Operation unsuccessful.\n\n{e.Exception.Message}", "An Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeAppSettings();
        }

        private void InitializeAppSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                CreateDedaultAppSettings();
            }
            Settings = JsonLite.DeserializeFromFile(SettingsPath, typeof(AppSettings)) as AppSettings;
        }
        public static void CreateDedaultAppSettings()
        {
            Settings = new AppSettings();
            JsonLite.SerializeToFile(SettingsPath, Settings);
        }
    }
}
