using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using JellyMusic.Models;

namespace JellyMusic.Core
{
    /// <summary>
    /// Wrapper for TagLib library
    /// </summary>
    public class TagReader : IDisposable
    {
        public string FilePath { get; }
        private readonly TagLib.File file;

        public TagReader(string filePath)
        {
            FilePath = filePath;
            file = TagLib.File.Create(filePath);
        }

        public string Title { get => file.Tag.Title; }
        public string Performer { get => file.Tag.FirstPerformer; }
        public string Genre { get => file.Tag.FirstGenre; }
        public uint Year { get => file.Tag.Year; }

        public TimeSpan TrackLength { get => file.Properties.Duration; }
        public DateTime LastModified { get => File.GetLastWriteTime(FilePath); }

        public string Lyrics { get => file.Tag.Lyrics; }
        public bool HasAlbumCover { get => file.Tag.Pictures.Length > 0; }

        public string Album { get => file.Tag.Album; }
        public uint PositionInAlbum { get => file.Tag.Track; }
        public uint AlbumTrackCount { get => file.Tag.TrackCount; }

        public ImageSource GetAlbumPictureSource(int width, int height)
        {
            if (HasAlbumCover)
            {
                var picture = file.Tag.Pictures[0];

                // Load image data in MemoryStream
                using (MemoryStream memoryStream = new MemoryStream(picture.Data.Data))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    bitmap.DecodePixelHeight = height;
                    bitmap.DecodePixelWidth = width;

                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    return bitmap;
                }
            }
            else
            {
                return null;
            }
        }

        public PlaylistTrack GetPlaylistTrack()
        {
            return new PlaylistTrack
            {
                Id = GenerateId(),
                FilePath = FilePath,
                LastModified = LastModified,

                Title = Title,
                Performer = Performer,
                Genre = Genre,
                Year = Year,

                Lyrics = Lyrics,
                AlbumPictureSource = GetAlbumPictureSource(300,300),
                TrackLength = TrackLength,

                Album = Album,
                PositionInAlbum = PositionInAlbum,
                AlbumTrackCount = AlbumTrackCount
            };
        }

        // Generate a unique identification string
        private string GenerateId()
        {
            // Select either album or, in case it is not specified in metadata, other properties for formatting
            string sourceString = Album ?? Title + Performer + Year;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < sourceString.Length; i++)
            {
                if (sourceString[i] != ' ') result.Append(sourceString[i]);
            }
            return result.ToString();
        }

        public void SaveChanges()
        {
            file.Save();
        }

        public void Dispose()
        {
            file?.Dispose();
        }
    }
}
