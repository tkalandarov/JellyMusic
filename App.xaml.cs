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
        public static readonly string SettingsPath = Path.Combine(Directory.GetCurrentDirectory(), @"DATA\AppSettings.json");
        public static AppSettings Settings { get; private set; }
        public static FileStream SettingsFileStream { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeAppSettings();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            SettingsFileStream.Close();
            base.OnExit(e);
        }

        private void InitializeAppSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                Settings = new AppSettings();
                JsonLite.SerializeToFile(SettingsPath, Settings);
            }
            SettingsFileStream = new FileStream(SettingsPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            Settings = JsonLite.DeserializeFromFile(SettingsFileStream, typeof(AppSettings)) as AppSettings;
        }
    }
}
