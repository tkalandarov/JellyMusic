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
    public class PlaybarViewModel : BaseNotifyPropertyChanged
    {
        #region Fields
        private BindingList<AudioFile> _trackList => ActivePlaylist?.TrackList;

        private Playlist _activePlaylist;
        private AudioFile _activeTrack;

        private bool _syncPlayer; // Used for syncronizing thumb drag control

        private TimeSpan _currentProgress;
        private bool _loop;
        #endregion

        #region Properties
        public readonly AudioPlayer AudioPlayer;

        public Playlist ActivePlaylist
        {
            get => _activePlaylist;
            set
            {
                SetProperty(ref _activePlaylist, value);
                OnPlaylistChanged.Invoke();
            }
        }
        public ImageSource ActivePlaylistPicture
        {
            get
            {
                if (ActivePlaylist != null)
                {
                    using (TagReader tagReader = new TagReader(_trackList.FirstOrDefault().FilePath))
                    {
                        return tagReader.GetAlbumPictureSource(300, 300);
                    }
                }
                return null;
            }
        }

        public BindingList<AudioFile> PlaybackQueue => ActivePlaylist?.GetPlaybackQueue(IsShuffled);

        public AudioFile ActiveTrack
        {
            get => _activeTrack;
            set
            {
                if (value == null) return;

                CurrentProgress = TimeSpan.Zero;
                SetProperty(ref _activeTrack, value);
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

        public TimeSpan CurrentProgress
        {
            get => _currentProgress;
            set
            {
                SetProperty(ref _currentProgress, value);
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
        public bool IsShuffled { get; set; }

        public string NextTrackTitle => GetNextTrack()?.Title;
        public string PreviousTrackTitle => GetPreviousTrack()?.Title;
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

                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(ActiveTrackPicture));
                OnPropertyChanged(nameof(NextTrackTitle));
                OnPropertyChanged(nameof(PreviousTrackTitle));
            };
            OnPlaylistChanged += () =>
            {
                if (PlaybackQueue.Count > 0)
                {
                    AudioPlayer.Pause();

                    CurrentProgress = TimeSpan.Zero;
                    OnPropertyChanged(nameof(CurrentProgress));
                    OnPropertyChanged(nameof(IsPlaying));

                    ActiveTrack = PlaybackQueue.FirstOrDefault();
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
           // Console.WriteLine(ActiveTrack?.Rating);
        }

        #region Commands

        // Declarations
        private ICommand _playCommand;
        private ICommand _playPreviousCommand;
        private ICommand _playNextCommand;

        private ICommand _shuffleCommand;
        private ICommand _loopCommand;
        private ICommand _volumeControlValueChangedCommand;

        // Realizations
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
                        if (CurrentProgress.TotalSeconds < 5 && !IsFirstTrackPlaying())
                        {
                            ActiveTrack = GetPreviousTrack();
                        }
                    },
                    canExecute:
                    obj => ActiveTrack != null && (!IsFirstTrackPlaying() || CurrentProgress.TotalSeconds >= 5)
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
                        ActiveTrack = GetNextTrack();

                        if (!IsPlaying) AudioPlayer.Play();
                    },
                    canExecute:
                    obj =>
                    {
                        return ActiveTrack != null && !IsLastTrackPlaying();
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
                        IsShuffled = !IsShuffled;

                        OnPropertyChanged(nameof(IsShuffled));
                        OnPropertyChanged(nameof(GetNextTrack));
                        OnPropertyChanged(nameof(GetPreviousTrack));
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
                    // Do not change property itself - it will break 'MoveToPoint' control and lead to sound stutter
                    SetProperty(ref _currentProgress, e.NewProgress);
                    OnPropertyChanged(nameof(CurrentProgress));
                }

                // Update PlayPrevious() CanExecute parameter for enabling rewinding
                if ((int)e.NewProgress.TotalSeconds == 5)
                {
                    CommandManager.InvalidateRequerySuggested();
                }
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
                    else
                    {
                        AudioPlayer.Progress = TimeSpan.Zero;
                        OnPropertyChanged(nameof(IsPlaying));
                    }
                }
            };
        }

        public bool IsFirstTrackPlaying()
        {
            return PlaybackQueue.IndexOf(_activeTrack) == 0;
        }
        public bool IsLastTrackPlaying()
        {
            return PlaybackQueue.IndexOf(_activeTrack) == PlaybackQueue.Count - 1;
        }

        public AudioFile GetNextTrack()
        {
            if (_activeTrack == null) return null;
            return IsLastTrackPlaying() ? null : PlaybackQueue[PlaybackQueue.IndexOf(_activeTrack) + 1];
        }
        public AudioFile GetPreviousTrack()
        {
            if (_activeTrack == null) return null;
            return IsFirstTrackPlaying() ? null : PlaybackQueue[PlaybackQueue.IndexOf(_activeTrack) - 1];
        }
        #endregion
    }
}
