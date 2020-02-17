using JellyMusic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace JellyMusic.Core
{
    public static class PictureCachingService
    {
        private readonly static string cachePath = Directory.GetCurrentDirectory() + @"\DATA\CACHE\";
        private const int picWidth = 50;
        private const int picHeight = 50;

        static PictureCachingService()
        {
            Directory.CreateDirectory(cachePath); // does nothing if folder already exists
        }

        public static async Task<BitmapImage> GetProcessedFileAsync(AudioFile track)
        {
            Directory.CreateDirectory(cachePath); // does nothing if folder already exists

            string picPath = Path.Combine(cachePath, track.Id + ".png");
            if (File.Exists(picPath))
                return new BitmapImage(new Uri(picPath));

            using (TagReader reader = new TagReader(track.FilePath))
            {
                // If there is no picture cached, cache it
                if (!File.Exists(picPath))
                {
                    // save picture to storage
                    if (reader.HasAlbumCover)
                    {
                        await Task.Run(() => CacheImage(picPath, reader.GetAlbumCoverBitmap(picWidth, picHeight)));
                        return new BitmapImage(new Uri(picPath));
                    }
                }
                return null;
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
