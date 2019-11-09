using System;
using System.Collections.Generic;
using JellyMusic.Core;

namespace JellyMusic.Models
{
    [Serializable]
    public class AppSettings
    {
        private string defaultPlaylistName = "Default";
        public string DefaultPlaylistName
        {
            get => defaultPlaylistName;
            set => SetProperty(ref defaultPlaylistName, value);
        }

        private float volume = 1;
        public float Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        private bool showVolumeSlider = true;
        public bool ShowVolumeSlider
        {
            get => showVolumeSlider;
            set => SetProperty(ref showVolumeSlider, value);
        }


        #region INotifyPropertyChanged
        // OnProperyChanged Wrapper
        protected void SetProperty<T>(ref T property, T value)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
                return;

            property = value;
            JsonLite.SerializeToFile(App.SettingsPath, this);
        }
        #endregion
    }
}
