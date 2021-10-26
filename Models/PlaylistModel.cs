using JellyMusic.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Input;
using JellyMusic.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using JellyMusic.UserControls;

namespace JellyMusic.Models
{
    // This attribute make serializer save only [JsonProperty] marked properties
    [JsonObject(MemberSerialization.OptIn)]
    public class Playlist : BaseNotifyPropertyChanged
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
        private BindingList<AudioFile> TracksToSerialize
        {
            get
            {
                if (String.IsNullOrEmpty(BaseFolderPath)) return TrackList;
                else return null;
            }
        }

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
                return GetSortedTrackList(SortMethod);
            }
        }

        private TrackSortingMethod _sortMethod;
        [JsonProperty]
        public TrackSortingMethod SortMethod
        {
            get => _sortMethod;
            set
            {
                SetProperty(ref _sortMethod, value);
                OnSortChanged?.Invoke();
            }
        }
        public event Action OnSortChanged;

        #endregion

        [JsonConstructor]
        public Playlist(string Name, string BaseFolderPath = null, params AudioFile[] TracksToAdd)
        {
            this.Name = Name;
            this.BaseFolderPath = BaseFolderPath;

            _sortMethod = TrackSortingMethod.ByTitle;

            TrackList = new BindingList<AudioFile>();
            _shuffledTrackList = new BindingList<AudioFile>();

            TrackList.ListChanged += (object sender, ListChangedEventArgs e) =>
            {
                OnPropertyChanged(nameof(TotalDuration));
                OnPropertyChanged(nameof(TrackList));
            };

            if (TracksToAdd != null)
            {
                foreach (var track in TracksToAdd)
                {
                    TrackList.Add(track);
                }
            }


            if (BaseFolderPath != null)
                LoadTracksFromFolder();
        }

        #region Commands

        private ICommand _sortCommand;
        public ICommand SortCommand
        {
            get
            {
                return _sortCommand ??
                    (_sortCommand = new RelayCommand(action: ChangeSort, canExecute: CanSort));
            }
        }
        private void ChangeSort(object newSort)
        {
            SortMethod = (TrackSortingMethod)newSort;
        }
        private bool CanSort(object newSort)
        {
            bool allSameArtist = !TrackList.Select(item => item.Performer)
                      .Where(x => !string.IsNullOrEmpty(x))
                      .Distinct()
                      .Skip(1)
                      .Any();

            if (allSameArtist && (TrackSortingMethod)newSort == TrackSortingMethod.ByPerformer) return false;

            if (TrackList.Count > 1 && SortMethod != (TrackSortingMethod)newSort)
                return true;
            return false;
        }

        #endregion

        #region Methods
        private void Shuffle()
        {
            Random rnd = new Random();
            _shuffledTrackList = new BindingList<AudioFile>(TrackList.OrderBy(item => rnd.Next()).ToList());
        }

        private BindingList<AudioFile> GetSortedTrackList(TrackSortingMethod sortingMethod)
        {
            switch (sortingMethod)
            {
                case TrackSortingMethod.ByTitle:
                    return new BindingList<AudioFile>(TrackList.OrderBy(item => item.Title).ToList());
                case TrackSortingMethod.ByPerformer:
                    return new BindingList<AudioFile>(TrackList.OrderBy(item => item.Performer).ToList());
                case TrackSortingMethod.ByDateAdded:
                    return new BindingList<AudioFile>(TrackList.OrderByDescending(item => item.LastModified).ToList());
                /*case TrackSortingMethod.ByRating:
                    return new BindingList<AudioFile>(TrackList.OrderByDescending(item => item.Rating).ToList());*/
                default:
                    return TrackList;
            }
        }
        public void LoadTracksFromFolder()
        {
            foreach (var path in IOService.GetFilesByExtensions(BaseFolderPath, SearchOption.AllDirectories, ".mp3"))
            {
                try
                {
                    using (TagReader tagReader = new TagReader(path))
                    {
                        TrackList.Add(tagReader.GetPlaylistTrack());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured! " + ex.Message);
                    continue;
                }
            }
        }

        #endregion
    }
}
