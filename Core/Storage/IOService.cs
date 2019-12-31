using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace JellyMusic.Core
{
    public static class IOService
    {
        public static IEnumerable<string> GetFilesByExtensions(string FolderPath, SearchOption searchOption, params string[] extensions)
        {
            IEnumerable<string> filesPaths = Directory.EnumerateFiles(FolderPath, "*", searchOption).Where(item=> extensions.Contains(Path.GetExtension(item)));
            return filesPaths;
        }

        public static bool IsValidFilename(string fileName)
        {
            string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            return (Regex.IsMatch(fileName, sPattern, RegexOptions.CultureInvariant));
        }
    }
}
