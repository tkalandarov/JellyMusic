using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using JellyMusic.Models;

namespace JellyMusic.Core
{
    public static class IOService
    {
        public static IEnumerable<string> GetFilesByExtensions(string FolderPath, SearchOption searchOption, params string[] extensions)
        {
            IEnumerable<string> filesPaths = Directory.EnumerateFiles(FolderPath, "*", searchOption).Where(item=> extensions.Contains(Path.GetExtension(item)));
            return filesPaths;
        }
    }
}
