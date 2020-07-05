using JellyMusic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JellyMusic.Core
{
    public static class PictureCachingService
    {
        private readonly static string cachePath = Directory.GetCurrentDirectory() + @"\DATA\CACHE\";
        private const int picWidth = 50;
        private const int picHeight = 50;

        public static async Task<BitmapImage> GetProcessedFileAsync(AudioFile track)
        {
            Directory.CreateDirectory(cachePath); // does nothing if folder already exists

            string picPath = Path.Combine(cachePath, track.Id + ".png");

            using (TagReader reader = new TagReader(track.FilePath))
            {
                if (reader.HasAlbumCover)
                {
                    await Task.Run(() => CacheImage(picPath, reader.GetAlbumCoverBitmap(picWidth, picHeight)));
                    return new BitmapImage(new Uri(picPath));
                }
                return null;
            }
        }

        // Save album cover picture in storage in .png format
        private static void CacheImage(string picPath, BitmapImage bitmapImage)
        {
            if (File.Exists(picPath)) return;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var filestream = new FileStream(picPath, FileMode.Create))
                encoder.Save(filestream);
        }
    }
}
