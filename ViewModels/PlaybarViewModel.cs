using JellyMusic.Core;
using JellyMusic.Models;
using MaterialDesignThemes.Wpf;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JellyMusic.ViewModels
{
    public class PlaybarViewModel : INotifyPropertyChanged
    {
        #region Fields and properties

        private Playlist _activePlaylist;
        public Playlist ActivePlaylist
        {
            get => _activePlaylist;
            set
            {
                if (Equals(_activePlaylist, value) || value == null) return;
                _activePlaylist = value;

                OnPlaylistChanged.Invoke();
            }
        }
        public ImageSource ActivePlaylistPicture
        {
            get
            {
                if (ActivePlaylist != null)
                {
                    using (TagReader tagReader = new TagReader(ActivePlaylist?.Tracks.FirstOrDefault().FilePath))
                    {
                        return tagReader.GetAlbumPictureSource(300, 300);
                    }
                }
                return null;
            }
        }


        private AudioFile _activeTrack;
        public AudioFile ActiveTrack
        {
            get => _activeTrack;
            set
            {
                if (value == null || value.Equals(_activeTrack)) return;

                _activeTrack = value;
                OnTrackChanged.Invoke();
            }
        }
        public ImageSource ActiveTrackPicture
        {
            get
            {
                if (ActiveTrack != null)
                {
                    using (TagReader tagReader = new TagReader(ActiveTrack.FilePath))
                    {
                        return tagReader.GetAlbumPictureSource(300, 300);
                    }
                }
                return null;
            }
        }

        public string NextTrackTitle => ActivePlaylist?.NextTrack(ActiveTrack)?.Title;
        public string PreviousTrackTitle => ActivePlaylist?.PreviousTrack(ActiveTrack)?.Title;

        public readonly AudioPlayer AudioPlayer;
        // Used for smooth thumb drag control
        private bool _syncPlayer;

        private TimeSpan _currentProgress;
        public TimeSpan CurrentProgress
        {
            get => _currentProgress;

            set
            {
                if (value.Equals(_currentProgress)) return;

                _currentProgress = value;
                OnPropertyChanged(nameof(CurrentProgress));

                if (_syncPlayer)
                {
                    AudioPlayer.Progress = value;
                }
            }
        }

        public TimeSpan Duration => AudioPlayer?.Duration ?? TimeSpan.Zero;

        public float CurrentVolume
        {
            get => App.Settings.Volume;
            set
            {

                if (value.Equals(App.Settings.Volume)) return;

                App.Settings.Volume = value;
                OnPropertyChanged(nameof(CurrentVolume));
            }
        }

        private bool _loop;
        public bool Loop
        {
            get => _loop;

            set
            {
                if (value.Equals(_loop)) return;

                _loop = value;
                OnPropertyChanged(nameof(Loop));
            }
        }

        public bool IsPlaying => AudioPlayer?.PlaybackState == PlaybackState.Playing;
        public bool IsShuffled => ActivePlaylist?.IsShuffled == true;

        #endregion

        #region Events

        public event Action OnTrackChanged;
        public event Action OnPlaylistChanged;

        #endregion

        #region Constructors
        public PlaybarViewModel()
        {
            AudioPlayer = new AudioPlayer();
            _syncPlayer = true;

            InitializeAudioPlayer();

            OnTrackChanged += () =>
            {
                AudioPlayer.ChangeTrack(ActiveTrack.FilePath, IsPlaying);

                OnPropertyChanged(nameof(ActiveTrack));
                OnPropertyChanged(nameof(ActiveTrackPicture));
                OnPropertyChanged(nameof(NextTrackTitle));
                OnPropertyChanged(nameof(PreviousTrackTitle));
            };
            OnPlaylistChanged += () =>
            {
                if (ActivePlaylist?.Tracks.Count > 0)
                {
                    AudioPlayer.Pause();

                    CurrentProgress = TimeSpan.Zero;
                    OnPropertyChanged(nameof(CurrentProgress));
                    OnPropertyChanged(nameof(IsPlaying));

                    ActiveTrack = ActivePlaylist.Tracks.First();
                }
                OnPropertyChanged(nameof(ActivePlaylist));
            };

            DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.1) };
            dispatcherTimer.Tick += DebugOutput;
            dispatcherTimer.Start();
        }

        #endregion

        private void DebugOutput(object sender, EventArgs e)
        {
            //Console.WriteLine(ActivePlaylist.Name);
        }

        #region Commands

        // Declarations
        private ICommand _playCommand;
        private ICommand _playPreviousCommand;
        private ICommand _playNextCommand;

        private ICommand _shuffleCommand;
        private ICommand _loopCommand;
        private ICommand _volumeControlValueChangedCommand;

        public ICommand PlayCommand
        {
            get
            {
                return _playCommand ??
                    (_playCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        AudioPlayer.TogglePlay();
                        OnPropertyChanged(nameof(IsPlaying));
                    },
                    canExecute:
                    obj => AudioPlayer.IsInitialized
                    ));
            }
        }
        public ICommand PlayPreviousCommand
        {
            get
            {
                return _playPreviousCommand ??
                    (_playPreviousCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        // rewind to start
                        CurrentProgress = TimeSpan.Zero;

                        // if playback progress is less than 5 sec, go to previous track
                        if (CurrentProgress.TotalSeconds < 5 && !ActivePlaylist.IsFirstTrack(ActiveTrack))
                        {
                            ActiveTrack = ActivePlaylist.PreviousTrack(ActiveTrack);
                        }
                    },
                    canExecute:
                    obj => ActiveTrack != null && (!ActivePlaylist.IsFirstTrack(ActiveTrack) || CurrentProgress.TotalSeconds >= 5)
                    ));
            }
        }
        public ICommand PlayNextCommand
        {
            get
            {
                return _playNextCommand ??
                    (_playNextCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        CurrentProgress = TimeSpan.Zero;
                        ActiveTrack = ActivePlaylist.NextTrack(ActiveTrack);
                    },
                    canExecute:
                    obj =>
                    {
                        return ActiveTrack != null && !ActivePlaylist.IsLastTrack(ActiveTrack);
                    }
                    ));
            }
        }

        public ICommand ShuffleCommand
        {
            get
            {
                return _shuffleCommand ??
                    (_shuffleCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        ActivePlaylist.IsShuffled = !ActivePlaylist.IsShuffled;

                        OnPropertyChanged(nameof(IsShuffled));
                        OnPropertyChanged(nameof(NextTrackTitle));
                        OnPropertyChanged(nameof(PreviousTrackTitle));
                    },
                    canExecute:
                    obj =>
                    {
                        return true;
                    }
                    ));
            }
        }
        public ICommand LoopCommand
        {
            get
            {
                return _loopCommand ??
                    (_loopCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        Loop = !Loop;
                    }
                    ));
            }
        }
        public ICommand VolumeControlValueChangedCommand
        {
            get
            {
                return _volumeControlValueChangedCommand ??
                    (_volumeControlValueChangedCommand = new RelayCommand(

                    action:
                    obj =>
                    {
                        AudioPlayer?.SetVolume(CurrentVolume); // set value of the slider to current volume
                    }
                    ));
            }
        }

        #endregion

        #region Methods
        public void DragStarted()
        {
            _syncPlayer = false;
        }
        public void DragCompleted()
        {
            _syncPlayer = true;
            AudioPlayer.Progress = CurrentProgress;
        }

        private void InitializeAudioPlayer()
        {
            AudioPlayer.ProgressUpdated += (sender, e) =>
            {
                if (_syncPlayer)
                {
                    _currentProgress = e.NewProgress;
                    OnPropertyChanged(nameof(CurrentProgress));
                }

                // Update PlayPrevious() CanExecute parameter for enabling rewinding
                if ((int)e.NewProgress.TotalSeconds == 5)
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            };
            AudioPlayer.TrackChanged += () =>
            {
                OnPropertyChanged(nameof(Duration));
            };

            AudioPlayer.PlaybackPaused += () => OnPropertyChanged(nameof(IsPlaying));
            AudioPlayer.PlaybackResumed += () => OnPropertyChanged(nameof(IsPlaying));
            AudioPlayer.PlaybackStopped += () =>
            {
                if (Loop)
                {
                    AudioPlayer.Stop();
                    AudioPlayer.Play();
                }
                else
                {
                    if (PlayNextCommand.CanExecute(null))
                    {
                        PlayNextCommand.Execute(null);
                    }
                    else OnPropertyChanged(nameof(IsPlaying));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
