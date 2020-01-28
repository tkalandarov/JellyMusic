using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private bool _introMuted = true;
        public bool IntroMuted
        {
            get => _introMuted;
            set
            {
                SetProperty(ref _introMuted, value);
                Save();
            }
        }

        private bool _virtualization = true;
        public bool Virtualization
        {
            get => _virtualization;
            set
            {
                SetProperty(ref _virtualization, value);
                Save();
            }
        }

        public AppSettings()
        {
            _volume = 1;
            _introEnabled = true;
            _introMuted = true;
            _virtualization = true;
        }
        public void Save()
        {  
            JsonLite.SerializeToFile(App.SettingsPath, this);
        }
    }
}
