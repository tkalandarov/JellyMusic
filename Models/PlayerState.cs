using System;

namespace JellyMusic.Models
{
    internal readonly struct PlayerState
    {
        internal PlayerState(int TrackIndex, TimeSpan Progress, double Volume, bool Loop, bool Shuffle)
        {
            this.TrackIndex = TrackIndex;
            this.Progress = Progress;
            this.Volume = Volume;
            this.Loop = Loop;
            this.Shuffle = Shuffle;
        }

        internal int TrackIndex { get; }
        internal TimeSpan Progress { get; }
        internal double Volume { get; }
        internal bool Loop { get; }
        internal bool Shuffle { get; }
    }
}
