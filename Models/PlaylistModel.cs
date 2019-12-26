using JellyMusic.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace JellyMusic.Models
{
    // This attribute make serializer save only [JsonProperty] marked properties
    [JsonObject(MemberSerialization.OptIn)]
    public class Playlist : BaseNotifyPropertyChanged, IComparable
    {
        #region Fields and Properties

        [JsonProperty]
        public string Name { get; }
        public TimeSpan TotalDuration
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                foreach (var track in TrackList)
                {
                    result += track.TrackLength;
                }
                return result;
            }
        }

        [JsonProperty]
        public string BaseFolderPath { get; }
        [JsonProperty]
        public BindingList<AudioFile> TrackList { get; private set; }
        private BindingList<AudioFile> _shuffledTrackList;

        public BindingList<AudioFile> GetPlaybackQueue(bool IsShuffled)
        {
            if (IsShuffled)
            {
                return _shuffledTrackList;
            }
            else
            {
                Shuffle();
                return TrackList;
            }
        }

        private TrackSortingMethod _sortMethod;
        public TrackSortingMethod SortMethod
        {
            get => _sortMethod;
            set
            {
                SetProperty(ref _sortMethod, value);
                ChangeSortingMethod(value);
            }
        }

        #endregion

        [JsonConstructor]
        public Playlist(string Name, string BaseFolderPath = null, params AudioFile[] TracksToAdd)
        {
            this.Name = Name;
            this.BaseFolderPath = BaseFolderPath;

            TrackList = new BindingList<AudioFile>();
            _shuffledTrackList = new BindingList<AudioFile>();

            if (TracksToAdd != null)
            {
                foreach (var track in TracksToAdd)
                    TrackList.Add(track);
            }


            if (BaseFolderPath != null)
                LoadTracksFromFolder();
        }

        #region Methods
        private void Shuffle()
        {
            Random rnd = new Random();
            _shuffledTrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => rnd.Next()).ToList());
        }

        private void ChangeSortingMethod(TrackSortingMethod sortingMethod)
        {
            switch (sortingMethod)
            {
                case TrackSortingMethod.ByTitle:
                    TrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => item.Title).ToList());
                    break;
                case TrackSortingMethod.ByPerformer:
                    TrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => item.Performer).ToList());
                    break;
                case TrackSortingMethod.ByDateAdded:
                    TrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => item.LastModified).ToList());
                    break;
                case TrackSortingMethod.ByRating:
                    TrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => item.Rating).ToList());
                    break;
            }
        }

        public void LoadTracksFromFolder()
        {
            foreach (var path in IOService.GetFilesByExtensions(BaseFolderPath, SearchOption.AllDirectories, ".mp3"))
            {
                using (TagReader tagReader = new TagReader(path))
                {
                    TrackList.Add(tagReader.GetPlaylistTrack());
                }
            }
        }

        public int CompareTo(object obj)
        {
            Playlist another = obj as Playlist;
            if (another == null) return -1;
            return (this.Name.CompareTo(another.Name));
        }
        #endregion
    }
}
