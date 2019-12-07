using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using JellyMusic.Core;

using Newtonsoft.Json;
using PropertyChanged;

namespace JellyMusic.Models
{
    [AddINotifyPropertyChangedInterface, JsonObject(MemberSerialization.OptIn)]
    public class AudioFile
    {
        // General info
        private string filePath;
        [JsonProperty]
        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                OnFilePathChanged();
            }
        }

        public string Title { get; set; }
        public string Performer { get; set; }
        public string Genre { get; set; }
        public uint Year { get; set; }

        public string Lyrics { get; set; }
        public ImageSource AlbumPictureSource { get; set; }

        public DateTime LastModified { get; set; }
        public TimeSpan TrackLength { get; set; }

        // Album info
        public string Album { get; set; }
        public uint PositionInAlbum { get; set; }
        public uint AlbumTrackCount { get; set; }

        // In-app additional info

        // Id is used for preventing caching same album pictures from different tracks several times
        // Realization of id-gen is in JellyMusic.Core.TagReader
        public string Id { get; set; }
        public byte Rating { get; set; }

        private void OnFilePathChanged()
        {
            using (TagReader reader = new TagReader(FilePath))
            {
                Title = reader.Title;
                Performer = reader.Performer;
                Genre = reader.Genre;
                Year = reader.Year;

                Album = reader.Album;
                PositionInAlbum = reader.PositionInAlbum;
                AlbumTrackCount = reader.AlbumTrackCount;

                LastModified = reader.LastModified;
                TrackLength = reader.TrackLength;

                Id = reader.GenerateId();
            }
        }
    }
}