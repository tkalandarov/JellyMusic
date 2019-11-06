using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using JellyMusic.Core;
using Newtonsoft.Json;

namespace JellyMusic.Models
{
    abstract public class AudioFile
    {
        public string FilePath { get; set; }
        public DateTime LastModified { get; set; }
        public TimeSpan TrackLength { get; set; }
    }
    public class PlaylistTrack : AudioFile, INotifyPropertyChanged
    {
        // General info
        public string Title { get; set; }
        public string Performer { get; set; }
        public string Genre { get; set; }
        public uint Year { get; set; }

        public string Lyrics { get; set; }
        public ImageSource AlbumPictureSource { get; set; }

        // Album info
        public string Album { get; set; }
        public uint PositionInAlbum { get; set; }
        public uint AlbumTrackCount { get; set; }

        // In-app additional info

        // Id is used for preventing caching same album pictures from different tracks several times
        // Realization of id-gen is in JellyMusic.Core.TagReader
        public string Id { get; set; }
        public byte Rating { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}