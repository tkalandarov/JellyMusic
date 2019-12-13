using JellyMusic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace JellyMusic.Core
{
    public class PictureCachingService
    {
        private readonly static string cachePath = Directory.GetCurrentDirectory() + @"\DATA\CACHE\";
        private const int picWidth = 75;
        private const int picHeight = 75;

        public readonly BindingList<AudioFile> Tracks;

        public PictureCachingService(BindingList<AudioFile> Tracks)
        {
            Directory.CreateDirectory(cachePath); // does nothing if folder already exists

            this.Tracks = Tracks;
        }

        public static void CacheSingle(AudioFile track)
        {
            Directory.CreateDirectory(cachePath); // does nothing if folder already exists

            using (TagReader reader = new TagReader(track.FilePath))
            {
                string picPath = Path.Combine(cachePath, track.Id + ".png");

                // If there is picture cached, set it
                if (!File.Exists(picPath))
                {
                    // save picture to storage
                    if (reader.HasAlbumCover)
                        CacheImage(picPath, reader.GetAlbumCoverBitmap(picWidth, picHeight));
                }

                // read picture data from storage
                if (reader.HasAlbumCover)
                    track.AlbumPictureSource = new BitmapImage(new Uri(picPath));
            }
        }

        public void CacheAndAssign()
        {
            foreach (var track in Tracks)
            {
                if (track.AlbumPictureSource != null) continue;
                CacheSingle(track);
            }
        }

        // Save album cover picture in storage in .png format
        private static void CacheImage(string picPath, BitmapImage bitmapImage)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var filestream = new FileStream(picPath, FileMode.Create))
                encoder.Save(filestream);
        }
    }
}
