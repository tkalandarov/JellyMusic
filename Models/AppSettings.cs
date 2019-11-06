using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyMusic.Models
{
    [Serializable]
    public class AppSettings
    {
        public bool LoadPlaylistOnStartupEnabled { get; set; }
        public string StartupPlaylistName { get; set; }
        public float Volume { get; set; } 
        public bool ShowVolumeSlider { get; set; }
    }
}
