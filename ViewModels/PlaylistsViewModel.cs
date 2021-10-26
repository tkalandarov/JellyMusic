using JellyMusic.Core;
using JellyMusic.Models;
using JellyMusic.EventArguments;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace JellyMusic.ViewModels
{
    public class PlaylistsViewModel : BaseNotifyPropertyChanged
    {
        #region Fields and Properties
        public readonly static string PlaylistsDir = Directory.GetCurrentDirectory() + @"\DATA\PLAYLISTS\";

        private readonly JsonService<Playlist> _serializer;

        public BindingList<Playlist> PlaylistsCollection { get; set; }
        //public BindingList<AudioFile> AllTracks { get; private set; }

        private string _searchPattern;
        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                SetProperty(ref _searchPattern, value);
                OnPropertyChanged(nameof(SearchFilteredTracks));
            }
        }

        public BindingList<AudioFile> SearchFilteredTracks
        {
            get
            {
                if (String.IsNullOrEmpty(SearchPattern) || String.IsNullOrWhiteSpace(SearchPattern)) return null;

                var allTracks = new BindingList<AudioFile>();
                foreach (var playlist in PlaylistsCollection)
                {
                    foreach (var track in playlist.TrackList)
                    {
                        if (allTracks.Any(x => x.FilePath == track.FilePath)) continue;
                        allTracks.Add(track);
                    }
                }
                var result = new BindingList<AudioFile>();

                List<AudioFile> temp = new List<AudioFile>(allTracks.Where(item =>
                item.Title != null && item.Title.IndexOf(SearchPattern, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                item.Album != null && item.Album.IndexOf(SearchPattern, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                item.Performer != null && item.Performer.IndexOf(SearchPattern, StringComparison.CurrentCultureIgnoreCase) != -1).ToList());

                if (temp.Count == 0)
                {
                    temp = null;
                }
                else result = new BindingList<AudioFile>(temp);
                return result;
            }
        }

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

            Directory.CreateDirectory(PlaylistsDir);
            File.Create(Path.Combine(PlaylistsDir, "RESTART THE APP IN CASE OF CHANGES"));

            LoadPlaylists();
            //LoadAllTracks();
            //AssignAlbumPictures();
        }

        #region Methods
        private void LoadPlaylists()
        {
            foreach (string elem in IOService.GetFilesByExtensions(PlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                Playlist deserialzedPlaylist = _serializer.DeserializeFromFile(elem);

                List<AudioFile> NonExistingTracks = new List<AudioFile>();
                foreach (var track in deserialzedPlaylist.TrackList)
                {
                    if (!File.Exists(track.FilePath))
                    {
                        NonExistingTracks.Add(track);
                    }
                }
                foreach (var track in NonExistingTracks)
                {
                    deserialzedPlaylist.TrackList.Remove(track);
                }

                PlaylistsCollection.Add(deserialzedPlaylist);
            }

            if (PlaylistsCollection.Count == 0)
            {
                AddPlaylist("Default", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            }
        }
        /*private void LoadAllTracks()
        {
            if (AllTracks == null) AllTracks = new BindingList<AudioFile>();

            foreach (var playlist in PlaylistsCollection)
            {
                for (int i = 0; i < playlist.TrackList.Count; i++)
                {
                    if (File.Exists(playlist.TrackList[i].FilePath))
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
                    //}
                }
            }

            AllTracks.ListChanged += (object sender, ListChangedEventArgs e) =>
            {
                OnPropertyChanged(nameof(SearchFilteredTracks));
            };
        }*/
        /*private void AssignAlbumPictures()
        {
            PictureCachingService cachingService = new PictureCachingService(AllTracks);
            cachingService.CacheAndAssign();

            AllTracks = cachingService.Tracks;
        }*/

        public void AddPlaylist(string Name, IEnumerable<string> trackPaths)
        {
            Playlist newPlaylist = new Playlist(Name);

            foreach (var path in trackPaths)
            {
                var loadedTracks = PlaylistsCollection.Select(x => x.TrackList).SelectMany(x => x).Distinct().ToList();
                if (loadedTracks.Any(x => x.FilePath == path))
                {
                    newPlaylist.TrackList.Add(loadedTracks.First(x => x.FilePath == path));
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

            //LoadAllTracks();
            //AssignAlbumPictures();
        }
        public void AddPlaylist(string Name, string folderPath)
        {
            Playlist newPlaylist = new Playlist(Name, folderPath);

            PlaylistsCollection.Add(newPlaylist);
            SavePlaylist(newPlaylist);

            //LoadAllTracks();
            //AssignAlbumPictures();
        }
        private void SavePlaylist(Playlist playlist)
        {
            _serializer.SerializeToFile(playlist.Name + ".json", playlist);
        }

        /*private void OnTrackRatingChanged(object sender, TrackRatingUpdatedEventArgs e)
        {
            if (EnableRatingsUpdateRequests)
                RatingsUpdateRequested.Invoke(sender, e);
        }*/
        #endregion
    }
}
