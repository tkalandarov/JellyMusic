using System;
using System.Collections.Generic;
using System.ComponentModel;
using JellyMusic.Core;

namespace JellyMusic.Models
{
    [Serializable]
    public class AppSettings : BaseNotifyPropertyChanged
    {
        private float _volume = 1;
        public float Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                Save();
            }
        }

        private bool _introEnabled = true;
        public bool IntroEnabled
        {
            get => _introEnabled;
            set
            {
                SetProperty(ref _introEnabled, value);
                Save();
            }
        }

        public void Save()
        {
            JsonLite.SerializeToFile(App.SettingsPath, this);
        }
    }
}
