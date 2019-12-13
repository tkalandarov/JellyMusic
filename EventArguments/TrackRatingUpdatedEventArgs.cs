using System;

namespace JellyMusic.EventArguments
{
    public class TrackRatingUpdatedEventArgs : EventArgs
    {
        public string TrackId { get; }
        public byte NewRating { get; }

        public TrackRatingUpdatedEventArgs(string TrackId, byte NewRating)
        {
            this.TrackId = TrackId;
            this.NewRating = NewRating;
        }
    }
}
