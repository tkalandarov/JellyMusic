using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using JellyMusic.Core;
using JellyMusic.EventArguments;

namespace JellyMusic.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        #region Fields and Properties
        public PlaylistsViewModel PlaylistsVM { get; private set; }
        public PlaybarViewModel PlaybarVM { get; private set; }

        private string _ratingsPath = Directory.GetCurrentDirectory() + @"\DATA\Ratings.json";

        public Dictionary<string, byte> TracksRatings;  //Contains TrackId-Rating pair
        #endregion

        public MainViewModel()
        {
            PlaybarVM = new PlaybarViewModel();
            PlaylistsVM = new PlaylistsViewModel() { EnableRatingsUpdateRequests = false };

            InitializeTracksRatings();

            PlaylistsVM.EnableRatingsUpdateRequests = true;

            PlaylistsVM.RatingsUpdateRequested += (object sender, TrackRatingUpdatedEventArgs e) =>
            {
                TracksRatings[e.TrackId] = e.NewRating;
                JsonLite.SerializeToFile(_ratingsPath, TracksRatings);
            };
        }

        private void InitializeTracksRatings()
        {
            if (!File.Exists(_ratingsPath))
            {
                TracksRatings = new Dictionary<string, byte>();
                foreach (var track in PlaylistsVM.AllTracks)
                {
                    TracksRatings.Add(track.Id, track.Rating);
                }
                JsonLite.SerializeToFile(_ratingsPath, TracksRatings);
            }
            else
            {
                LoadTracksRatings();
            }
        }
        private void LoadTracksRatings()
        {
            TracksRatings = JsonLite.DeserializeFromFile(_ratingsPath, typeof(Dictionary<string, byte>)) as Dictionary<string, byte>;
            foreach (var track in PlaylistsVM.AllTracks.Where(x => TracksRatings.ContainsKey(x.Id)))
            {
                track.Rating = TracksRatings.Single(x => x.Key == track.Id).Value;
            }
        }
    }
}
