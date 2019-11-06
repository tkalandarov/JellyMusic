using System;

namespace JellyMusic.EventArguments
{
    public class ProgressUpdatedEventArgs : EventArgs
    {
        public TimeSpan NewProgress { get; }

        public ProgressUpdatedEventArgs(TimeSpan NewProgress)
        {
            this.NewProgress = NewProgress;
        }
    }
}
