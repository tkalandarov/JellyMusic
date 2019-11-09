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
            PlaybarVM = new PlaybarViewModel();
            InitializePlaylists();
        }

        private void InitializePlaylists()
        {
            playlistsVM = new PlaylistsViewModel();

            if (playlistsVM.PlaylistsCollection.Any(item => item.Name == App.Settings.DefaultPlaylistName))
            {
                PlaybarVM.ActivePlaylist = playlistsVM.PlaylistsCollection.Single(item => item.Name == App.Settings.DefaultPlaylistName);
            }
            else
            {
                PlaybarVM.ActivePlaylist = new Playlist(App.Settings.DefaultPlaylistName, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                playlistsVM.PlaylistsCollection.Add(PlaybarVM.ActivePlaylist);
                playlistsVM.SavePlaylist(PlaybarVM.ActivePlaylist);
            }
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
