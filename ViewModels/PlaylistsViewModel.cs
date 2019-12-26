using JellyMusic.Core;
using JellyMusic.Models;
using JellyMusic.EventArguments;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using System.Linq;
using System.Windows.Input;

namespace JellyMusic.ViewModels
{
    public class PlaylistsViewModel : BaseNotifyPropertyChanged
    {
        #region Fields and Properties
        public readonly string PlaylistsDir = Directory.GetCurrentDirectory() + @"\DATA\PLAYLISTS\";

        private readonly JsonService<Playlist> _serializer;

        public BindingList<Playlist> PlaylistsCollection { get; set; }
        public BindingList<AudioFile> AllTracks { get; private set; }

        public bool EnableRatingsUpdateRequests { get; set; }
        public event EventHandler<TrackRatingUpdatedEventArgs> RatingsUpdateRequested;
        #endregion

        public PlaylistsViewModel()
        {
            _serializer = new JsonService<Playlist>(PlaylistsDir);

            PlaylistsCollection = new BindingList<Playlist>()
            {
                AllowEdit = true,
                AllowNew = true,
                AllowRemove = true
            };

            LoadPlaylists();
            LoadAllTracks();
            PerformCaching();
        }

        #region Methods
        private void LoadPlaylists()
        {
            foreach (string elem in IOService.GetFilesByExtensions(PlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                PlaylistsCollection.Add(_serializer.DeserializeFromFile(elem));
            }

            if (PlaylistsCollection.Count == 0)
            {
                AddPlaylist(App.Settings.DefaultPlaylistName, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            }
        }
        private void LoadAllTracks()
        {
            if (AllTracks == null) AllTracks = new BindingList<AudioFile>();

            foreach (var playlist in PlaylistsCollection)
            {
                for (int i = 0; i < playlist.TrackList.Count; i++)
                {
                    // If AllTracks already has this track in another playlist
                    if (AllTracks.Any(x => x.FilePath == playlist.TrackList[i].FilePath))
                    {
                        // Assign this track
                        playlist.TrackList[i] = AllTracks.Single(x => x.FilePath == playlist.TrackList[i].FilePath);
                    }
                    else
                    {
                        // Add track to AllTracks
                        AllTracks.Add(playlist.TrackList[i]);
                        playlist.TrackList[i].OnRatingChanged += OnTrackRatingChanged;
                    }
                }
            }
        }
        private void PerformCaching()
        {
            PictureCachingService cachingService = new PictureCachingService(AllTracks);
            cachingService.CacheAndAssign();

            AllTracks = cachingService.Tracks;
        }

        public void AddPlaylist(string Name, IEnumerable<string> trackPaths)
        {
            Playlist newPlaylist = new Playlist(Name);

            foreach (var path in trackPaths)
            {
                if (AllTracks.Any(x => x.FilePath == path))
                {
                    newPlaylist.TrackList.Add(AllTracks.Single(x => x.FilePath == path));
                }
                else
                {
                    using (TagReader reader = new TagReader(path))
                    {
                        AudioFile newTrack = reader.GetPlaylistTrack();
                        newPlaylist.TrackList.Add(newTrack);
                    }
                }
            }

            PlaylistsCollection.Add(newPlaylist);
            SavePlaylist(newPlaylist);

            LoadAllTracks();
            PerformCaching();
        }
        public void AddPlaylist(string Name, string folderPath)
        {
            Playlist newPlaylist = new Playlist(Name, folderPath);

            PlaylistsCollection.Add(newPlaylist);
            SavePlaylist(newPlaylist);

            LoadAllTracks();
            PerformCaching();
        }
        private void SavePlaylist(Playlist playlist)
        {
            _serializer.SerializeToFile(playlist.Name + ".json", playlist);
        }

        private void OnTrackRatingChanged(object sender, TrackRatingUpdatedEventArgs e)
        {
            if (EnableRatingsUpdateRequests)
                RatingsUpdateRequested.Invoke(sender, e);
        }
        #endregion
    }
}
