using JellyMusic.Core;
using JellyMusic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JellyMusic.ViewModels
{
    class PlaylistsViewModel : INotifyPropertyChanged
    {
        public readonly string folderBasedPlaylistsDir = Directory.GetCurrentDirectory() + @"\Data\Folder based playlists\";
        public readonly string customPlaylistsDir = Directory.GetCurrentDirectory() + @"\Data\Custom playlists\";

        private readonly JsonService<FolderBasedPlaylist> fbSerializer;
        private readonly JsonService<CustomPlaylist> cpSerializer;

        public BindingList<Playlist> PlaylistsCollection;
        public List<PlaylistTrack> AllTracks { get; private set; }

        public PlaylistsViewModel()
        {
            fbSerializer = new JsonService<FolderBasedPlaylist>(folderBasedPlaylistsDir);
            cpSerializer = new JsonService<CustomPlaylist>(customPlaylistsDir);

            PlaylistsCollection = new BindingList<Playlist>()
            {
                AllowEdit = true,
                AllowNew = true,
                AllowRemove = true
            };

            LoadPlaylists();
            LoadAllTracks();
        }

        private void LoadPlaylists()
        {
            // Load folder-based playlists
            foreach (string elem in IOService.GetFilesByExtensions(folderBasedPlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                try
                {
                    PlaylistsCollection.Add(fbSerializer.DeserializeFromFile(elem));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            // Load custom playlists
            foreach (string elem in IOService.GetFilesByExtensions(customPlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                try
                {
                    PlaylistsCollection.Add(cpSerializer.DeserializeFromFile(elem));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void LoadAllTracks()
        {
            if (AllTracks == null) AllTracks = new List<PlaylistTrack>();

            foreach (var playlist in PlaylistsCollection)
            {
                foreach (var track in playlist.Tracks)
                {
                    if (!AllTracks.Contains(track)) AllTracks.Add(track);
                }
            }
        }

        public void SavePlaylist(Playlist playlist, bool isFolderBased)
        {
            if (isFolderBased)
            {
                fbSerializer.SerializeToFile(playlist.Name + ".json", (FolderBasedPlaylist)playlist);
            }
            else
            {
                cpSerializer.SerializeToFile(playlist.Name + ".json", (CustomPlaylist)playlist);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
