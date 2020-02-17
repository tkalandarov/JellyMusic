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
        public int BitRate { get => file.Properties.AudioBitrate; }

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

                TrackLength = TrackLength,

                Album = Album,
                PositionInAlbum = PositionInAlbum,
                AlbumTrackCount = AlbumTrackCount
            };
        }

        // Generate a unique identification string
        public string GenerateId()
        {
            string titlePattern = "";
            string albumPattern = "";
            string performerPattern = "";

            if (!String.IsNullOrEmpty(Title))
                titlePattern = Title.Length > 5 ? Title.Substring(0, 4) : Title;
            if (!String.IsNullOrEmpty(Album))
                albumPattern = Album.Length > 5 ? Album.Substring(0, 4) : Album;
            if (!String.IsNullOrEmpty(Performer))
                performerPattern = Performer.Length > 5 ? Performer.Substring(0, 4) : Performer;

            string sourceString = titlePattern + albumPattern + performerPattern + Year;

            // only space, capital A-Z, lowercase a-z, and digits 0-9 are allowed in the string
            string result = Regex.Replace(sourceString, "[^A-Za-z0-9А-Яа-я ]", "");
            return result.Replace(" ", "");
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
