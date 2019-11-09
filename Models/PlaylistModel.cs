using JellyMusic.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JellyMusic.Models
{
    // This attribute make serializer save only [JsonProperty] marked properties
    [JsonObject(MemberSerialization.OptIn)]
    public class Playlist : INotifyPropertyChanged
    {
        #region Fields and Properties

        [JsonProperty]
        public string Name { get; protected set; }

        [JsonProperty]
        public string BaseFolderPath { get; private set; }

        [JsonProperty]
        public ICollection<string> TracksPaths { get; set; }

        public BindingList<PlaylistTrack> Tracks
        {
            get
            {
                if (IsShuffled)
                {
                    return ShuffledSequence;
                }
                else
                {
                    return OrderedSequence;
                }
            }
            set
            {
                if (value.Equals(OrderedSequence)) return;

                OrderedSequence = value;
                OnPropertyChanged(nameof(Tracks));
            }
        }

        public TimeSpan TotalDuration
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                foreach (var track in Tracks)
                {
                    result += track.TrackLength;
                }
                return result;
            }
        }

        private BindingList<PlaylistTrack> OrderedSequence { get; set; }
        private BindingList<PlaylistTrack> ShuffledSequence { get; set; }

        public bool IsFirstTrack(PlaylistTrack SelectedTrack)
        {
            return Tracks.IndexOf(SelectedTrack) == 0;
        }
        public bool IsLastTrack(PlaylistTrack SelectedTrack)
        {
            return Tracks.IndexOf(SelectedTrack) == Tracks.Count - 1;
        }

        public PlaylistTrack NextTrack(PlaylistTrack CurrentTrack)
        {
            if (CurrentTrack == null) return null;
            return IsLastTrack(CurrentTrack) ? null : Tracks[Tracks.IndexOf(CurrentTrack) + 1];
        }
        public PlaylistTrack PreviousTrack(PlaylistTrack CurrentTrack)
        {
            if (CurrentTrack == null) return null;
            return IsFirstTrack(CurrentTrack) ? null : Tracks[Tracks.IndexOf(CurrentTrack) - 1];
        }

        private bool _isShuffled;
        public bool IsShuffled
        {
            get => _isShuffled;

            set
            {
                if (value.Equals(_isShuffled)) return;
                _isShuffled = value;

                if (_isShuffled)
                {
                    Shuffle();
                }
                OnPropertyChanged(nameof(IsShuffled));
            }
        }

        private TrackSortingMethod _sortMethod;
        public TrackSortingMethod SortMethod
        {
            get => _sortMethod;
            set
            {
                if (_sortMethod.Equals(value)) return;

                _sortMethod = value;
                ChangeSortingMethod(value);
            }
        }

        #endregion

        public Playlist(string Name, string BaseFolderPath = null, params string[] TracksPaths)
        {
            this.Name = Name;
            this.BaseFolderPath = BaseFolderPath;
            if (String.IsNullOrEmpty(BaseFolderPath))
            {
                this.TracksPaths = TracksPaths;
            }
            LoadTracks();
        }

        #region Methods

        private void Shuffle()
        {
            Random rnd = new Random();
            ShuffledSequence = new BindingList<PlaylistTrack>(OrderedSequence.OrderBy(item => rnd.Next()).ToList());
        }

        private void ChangeSortingMethod(TrackSortingMethod sortingMethod)
        {
            switch (sortingMethod)
            {
                case TrackSortingMethod.ByTitle:
                    Tracks.OrderBy(item => item.Title);
                    break;
                case TrackSortingMethod.ByPerformer:
                    Tracks.OrderBy(item => item.Performer);
                    break;
                case TrackSortingMethod.ByDateAdded:
                    Tracks.OrderBy(item => item.LastModified);
                    break;
                case TrackSortingMethod.ByRating:
                    Tracks.OrderBy(item => item.Rating);
                    break;
            }
        }

        public void LoadTracks()
        {
            if (Tracks == null)
            {
                Tracks = new BindingList<PlaylistTrack>()
                {
                    AllowNew = true,
                    AllowRemove = true,
                    AllowEdit = true
                };
            }
            foreach (var path in String.IsNullOrEmpty(BaseFolderPath) ? 
                TracksPaths : IOService.GetFilesByExtensions(BaseFolderPath, SearchOption.AllDirectories, ".mp3"))
            {
                using (TagReader tagReader = new TagReader(path))
                {
                    Tracks.Add(tagReader.GetPlaylistTrack());
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
