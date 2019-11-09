using JellyMusic.Core;
using JellyMusic.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using PropertyChanged;
using System.Windows.Input;

namespace JellyMusic.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    class PlaylistsViewModel
    {
        public readonly string PlaylistsDir = Directory.GetCurrentDirectory() + @"\DATA\PLAYLISTS\";

        private readonly JsonService<Playlist> _serializer;

        public BindingList<Playlist> PlaylistsCollection;
        public List<PlaylistTrack> AllTracks { get; private set; }

        public PlaylistsViewModel()
        {
            _serializer = new JsonService<Playlist>(PlaylistsDir);

            PlaylistsCollection = new BindingList<Playlist>()
            {
                AllowEdit = true,
                AllowNew = true,
                AllowRemove = true
            };

            PlaylistsCollection.ListChanged += (sender, e) =>
            {
                Console.WriteLine("PlaylisCollection has been changed: " + e.ListChangedType.ToString());
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemAdded:

                        List<PlaylistTrack> addedItems = new List<PlaylistTrack>();

                        foreach (var item in PlaylistsCollection[e.NewIndex].Tracks)
                        {
                            if (!AllTracks.Contains(item))
                            {
                                addedItems.Add(item);
                            }
                        }

                        PerformCaching(addedItems);
                        AllTracks.AddRange(addedItems);
                        break;
                    case ListChangedType.ItemDeleted:
                        LoadAllTracks();
                        break;
                }
            };

            LoadPlaylists();
            LoadAllTracks();
            PerformCaching(AllTracks);
        }

        private void LoadPlaylists()
        {
            foreach (string elem in IOService.GetFilesByExtensions(PlaylistsDir, SearchOption.TopDirectoryOnly, ".json"))
            {
                try
                {
                    PlaylistsCollection.Add(_serializer.DeserializeFromFile(elem));
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

        private void PerformCaching(ICollection<PlaylistTrack> tracks)
        {
            PictureCachingService cachingService = new PictureCachingService(tracks);
            cachingService.CacheAndAssign();
        }

        public void SavePlaylist(Playlist playlist)
        {
            _serializer.SerializeToFile(playlist.Name + ".json", playlist);
        }
    }
}
