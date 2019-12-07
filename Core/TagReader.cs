using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using JellyMusic.Models;
using System.Text.RegularExpressions;

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
                return GetAlbumCoverBitmap(width, height);
            }
            else
            {
                return null;
            }
        }
        public BitmapImage GetAlbumCoverBitmap(int width, int height)
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

        public AudioFile GetPlaylistTrack()
        {
            return new AudioFile
            {
                Id = GenerateId(),
                FilePath = FilePath,
                LastModified = LastModified,

                Title = Title,
                Performer = Performer,
                Genre = Genre,
                Year = Year,

                Lyrics = Lyrics,
                TrackLength = TrackLength,

                Album = Album,
                PositionInAlbum = PositionInAlbum,
                AlbumTrackCount = AlbumTrackCount
            };
        }

        // Generate a unique identification string
        public string GenerateId()
        {
            string sourceString = Album + Title + Performer + Year;

            // only space, capital A-Z, lowercase a-z, and digits 0-9 are allowed in the string
            string result = Regex.Replace(sourceString, "[^A-Za-z0-9 ]", "");
            if(result.Length > 16) result.Substring(0, 16);
            return result;
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
