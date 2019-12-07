using JellyMusic.Core;
using JellyMusic.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using PropertyChanged;
using System.Linq;

namespace JellyMusic.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class PlaylistsViewModel
    {
        public readonly string PlaylistsDir = Directory.GetCurrentDirectory() + @"\DATA\PLAYLISTS\";

        private readonly JsonService<Playlist> _serializer;

        public BindingList<Playlist> PlaylistsCollection;
        public List<AudioFile> AllTracks { get; private set; }

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
            PerformCaching(AllTracks);
        }

        private void LoadPlaylists()
        {
            foreach (string elem in IOService.GetFilesByExtensions(PlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                PlaylistsCollection.Add(_serializer.DeserializeFromFile(elem));
            }
        }
        private void LoadAllTracks()
        {
            if (AllTracks == null) AllTracks = new List<AudioFile>();

            foreach (var playlist in PlaylistsCollection)
            {
                foreach (var track in playlist.Tracks)
                {
                    if (!AllTracks.Contains(track))
                    {
                        AllTracks.Add(track);
                    }
                }
            }
        }

        private void PerformCaching(ICollection<AudioFile> tracks)
        {
            PictureCachingService cachingService = new PictureCachingService(tracks);
            cachingService.CacheAndAssign();
        }

        public void SavePlaylist(Playlist playlist)
        {
            _serializer.SerializeToFile(playlist.Name + ".json", playlist);
        }

        public void AddPlaylist(string Name, IEnumerable<string> trackPaths)
        {
            Playlist newPlaylist = new Playlist(Name);

            foreach (var path in trackPaths)
            {
                using (TagReader reader = new TagReader(path))
                {
                    AudioFile newTrack = reader.GetPlaylistTrack();
                    if (!AllTracks.Contains(newTrack))
                    {
                        AllTracks.Add(newTrack);
                        PictureCachingService.CacheSingle(newTrack);
                    }
                    newPlaylist.Tracks.Add(newTrack);
                }
            }

            PlaylistsCollection.Add(newPlaylist);
            SavePlaylist(newPlaylist);
        }
        public void AddPlaylist(string Name, string folderPath)
        {
            Playlist newPlaylist = new Playlist(Name, folderPath);

            foreach (var track in newPlaylist.Tracks)
            {
                if (!AllTracks.Contains(track))
                {
                    AllTracks.Add(track);
                    PictureCachingService.CacheSingle(track);
                }
            }

            PlaylistsCollection.Add(newPlaylist);
            SavePlaylist(newPlaylist);
        }
    }
}
