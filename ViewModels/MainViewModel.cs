using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using JellyMusic.Core;
using JellyMusic.Models;

namespace JellyMusic.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private AppSettings _appSettings;
        public AppSettings AppSettings
        {
            get => _appSettings;
            set
            {
                if (_appSettings != null && _appSettings.Equals(value)) return;
                _appSettings = value;
                SaveAppSettings();
            }
        }
        private readonly string _settingsPath = Path.Combine(Directory.GetCurrentDirectory(), @"Data\AppSettings.json");

        PlaylistsViewModel playlistsVM;

        public PlaybarViewModel _playbarVM;
        public PlaybarViewModel PlaybarVM
        {
            get => _playbarVM;
            set
            {
                if (_playbarVM != null && _playbarVM.Equals(value)) return;

                _playbarVM = value;
                _playbarVM.OnTrackChanged += OnTrackChanged;
            }
        }

        public PlaylistTrack ActiveTrack => PlaybarVM.ActiveTrack;

        public BindingList<Playlist> PlaylistsCollection => playlistsVM?.PlaylistsCollection;

        public MainViewModel()
        {
            InitializeAppSettings();
            PlaybarVM = new PlaybarViewModel();
            PlaybarVM.CurrentVolume = AppSettings.Volume;
            PlaybarVM.OnVolumeChanged += () => AppSettings.Volume = PlaybarVM.CurrentVolume;
            InitializePlaylists();
        }

        private void InitializeAppSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                AppSettings = new AppSettings()
                {
                    LoadPlaylistOnStartupEnabled = true,
                    StartupPlaylistName = "Default",
                    Volume = 1,
                    ShowVolumeSlider = true
                };
                JsonLite.SerializeToFile(_settingsPath, AppSettings);
            }
            AppSettings = JsonLite.DeserializeFromFile(_settingsPath, typeof(AppSettings)) as AppSettings;
        }
        private void InitializePlaylists()
        {
            playlistsVM = new PlaylistsViewModel();

            if (AppSettings.LoadPlaylistOnStartupEnabled)
            {
                if (playlistsVM.PlaylistsCollection.Any(item => item.Name == AppSettings.StartupPlaylistName))
                {
                    PlaybarVM.ActivePlaylist = playlistsVM.PlaylistsCollection.Single(item => item.Name == AppSettings.StartupPlaylistName);
                }
                else
                {
                    PlaybarVM.ActivePlaylist = new FolderBasedPlaylist(Environment.UserName + "'s music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                    playlistsVM.PlaylistsCollection.Add(PlaybarVM.ActivePlaylist);
                    playlistsVM.SavePlaylist(PlaybarVM.ActivePlaylist, true);
                }
            }
        }

        public void SaveAppSettings()
        {
            JsonLite.SerializeToFile(_settingsPath, AppSettings);
        }

        private void OnTrackChanged()
        {
            OnPropertyChanged(nameof(ActiveTrack));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
