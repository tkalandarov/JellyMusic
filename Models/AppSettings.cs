using System;
using System.Collections.Generic;
using System.ComponentModel;
using JellyMusic.Core;

namespace JellyMusic.Models
{
    [Serializable]
    public class AppSettings : BaseNotifyPropertyChanged
    {
        private string defaultPlaylistName = "Default";
        public string DefaultPlaylistName
        {
            get => defaultPlaylistName;
            set
            {
                SetProperty(ref defaultPlaylistName, value);
                Save();
            }
        }

        private float volume = 1;
        public float Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
                Save();
            }
        }

        public void Save()
        {
            JsonLite.SerializeToFile(App.SettingsPath, this);
        }
    }
}
