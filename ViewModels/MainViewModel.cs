using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using JellyMusic.Core;
using JellyMusic.EventArguments;

namespace JellyMusic.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        #region Fields and Properties
        public PlaylistsViewModel PlaylistsVM { get; private set; }
        public PlaybarViewModel PlaybarVM { get; private set; }

        public bool _CanContentScroll => App.Settings.Virtualization;
        #endregion

        public MainViewModel()
        {
            PlaybarVM = new PlaybarViewModel();
            PlaylistsVM = new PlaylistsViewModel();
        }

        #region Methods

        

        #endregion
    }
}
