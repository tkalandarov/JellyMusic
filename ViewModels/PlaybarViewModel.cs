using JellyMusic.Core;
using JellyMusic.Models;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        public int? TrackCount => ActivePlaylist?.TrackList?.Count;
        public ImageSource ActivePlaylistPicture
        {
            get
            {
                if (ActivePlaylist != null && ActivePlaylist.TrackList.Count > 0)
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
        public bool IsQueueBeingLoaded { get; private set; }

        public AudioFile ActiveTrack
        {
            get => _activeTrack;
            set
            {
                if (value == null) return;

                if (!File.Exists(value.FilePath))
                {
                    OnTrackNotFound.Invoke(value.FilePath);
                    AudioPlayer.Pause();
                    MessageBox.Show($"Operation unsuccessful.\n\nTrack not found", "An Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);

                    _activeTrack = value;
                    if (PlayNextCommand.CanExecute(null)) PlayNextCommand.Execute(null);

                    ActivePlaylist.TrackList.Remove(value);
                    OnPropertyChanged(nameof(PlaybackQueue));
                    OnPropertyChanged(nameof(TrackCount));
                    return;
                }

                CurrentProgress = TimeSpan.Zero;
                SetProperty(ref _activeTrack, value);
                OnTrackChanged.Invoke();
            }
        }
        public bool IsActiveTrackSelected => ActiveTrack != null;
        public string ActiveTrackTitle => ActiveTrack?.Title;
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

        public string BitRate
        {
            get
            {
                if (ActiveTrack == null) return null;
                using (TagReader reader = new TagReader(ActiveTrack?.FilePath))
                {
                    return reader.BitRate + " kbps";
                }
            }
        }
        public string FileSizeInMb
        {
            get
            {
                if (ActiveTrack == null) return null;

                long bytes = new FileInfo(ActiveTrack.FilePath).Length;
                double MegaBytes = Convert.ToDouble(bytes) / (1024 * 1024);
                return Math.Round(MegaBytes, 2) + " MB";
            }
        }

        public byte ActiveTrackRating
        {
            get
            {
                if (ActiveTrack == null)
                    return 0;

                if (TracksRatings.ContainsKey(ActiveTrack.Id))
                    return TracksRatings[ActiveTrack.Id];
                else
                    return 0;
            }
            set
            {
                if (TracksRatings.ContainsKey(ActiveTrack.Id))
                {
                    TracksRatings[ActiveTrack.Id] = value;
                }
                else
                {
                    TracksRatings.Add(ActiveTrack.Id, value);
                }
                JsonLite.SerializeToFile(_ratingsPath, TracksRatings);
                OnPropertyChanged(nameof(ActiveTrackRating));
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

        private readonly string _ratingsPath = Directory.GetCurrentDirectory() + @"\DATA\Ratings.json";
        private static Dictionary<string, byte> TracksRatings;  //Contains TrackId-Rating pairs
        #endregion

        #region Events

        public event Action OnTrackChanged;
        public event Action OnPlaylistChanged;
        public event Action<string> OnTrackNotFound;

        #endregion

        #region Constructors
        public PlaybarViewModel()
        {
            AudioPlayer = new AudioPlayer();
            _syncPlayer = true;

            InitializeAudioPlayer();
            InitializeTracksRatings();

            OnTrackChanged += () =>
            {
                AudioPlayer.ChangeTrack(ActiveTrack.FilePath, CurrentVolume, IsPlaying);

                UpdateActiveTrackView();
            };
            OnPlaylistChanged += async () =>
            {
                if (ActivePlaylist == null) return;

                await InitializeTracksPictures();
                ActivePlaylist.OnSortChanged += () =>
                {
                    if (!IsShuffled)
                        OnPropertyChanged(nameof(PlaybackQueue));
                    OnPropertyChanged(nameof(NextTrackTitle));
                    OnPropertyChanged(nameof(PreviousTrackTitle));
                };

                OnPropertyChanged(nameof(ActivePlaylist));
                OnPropertyChanged(nameof(ActivePlaylistPicture));
                OnPropertyChanged(nameof(PlaybackQueue));

                OnPropertyChanged(nameof(NextTrackTitle));
                OnPropertyChanged(nameof(PreviousTrackTitle));

                OnPropertyChanged(nameof(TrackCount));
            };

            // Would be used for debugging
            DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.1) };
            dispatcherTimer.Tick += DebugOutput;
            //dispatcherTimer.Start();
        }

        #endregion

        private void DebugOutput(object sender, EventArgs e)
        {
            //Console.WriteLine(ActiveTrackPicture == null);
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
                        if (canRewind())
                        {
                            // rewind to start
                            CurrentProgress = TimeSpan.Zero;
                            return;
                        }

                        if (canPlayPrevious())
                        {
                            // go to previous track
                            ActiveTrack = GetPreviousTrack();
                            return;
                        }
                    },
                    canExecute:
                    obj => canRewind() || canPlayPrevious()
                    ));
            }
        }
        private bool canRewind()
        {
            return ActiveTrack != null && CurrentProgress.TotalSeconds >= 5;
        }
        private bool canPlayPrevious()
        {
            if (PlaybackQueue == null) return false;
            else return ActiveTrack != null && PlaybackQueue.Contains(ActiveTrack) && !IsFirstTrackPlaying();
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
                    },
                    canExecute:
                    obj =>
                    {
                        return ActiveTrack != null && !IsLastTrackPlaying() && PlaybackQueue.Contains(ActiveTrack);
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
                        OnPropertyChanged(nameof(NextTrackTitle));
                        OnPropertyChanged(nameof(PreviousTrackTitle));
                        OnPropertyChanged(nameof(PlaybackQueue));
                    },
                    canExecute:
                    obj =>
                    {
                        if (PlaybackQueue == null || PlaybackQueue.Count <= 1) return false;
                        else return true;
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
                        AudioPlayer.Play();
                    }
                    else
                    {
                        AudioPlayer.Progress = TimeSpan.Zero;
                        OnPropertyChanged(nameof(IsPlaying));
                    }
                }
            };
        }

        private async Task InitializeTracksPictures()
        {
            IsQueueBeingLoaded = true;
            OnPropertyChanged(nameof(IsQueueBeingLoaded));
            foreach (var item in PlaybackQueue)
            {
                item.AlbumPictureSource = await PictureCachingService.GetProcessedFileAsync(item);
            }
            IsQueueBeingLoaded = false;
            OnPropertyChanged(nameof(IsQueueBeingLoaded));
        }
        private void InitializeTracksRatings()
        {
            if (!File.Exists(_ratingsPath))
            {
                TracksRatings = new Dictionary<string, byte>();
            }
            else
            {
                TracksRatings = JsonLite.DeserializeFromFile(_ratingsPath, typeof(Dictionary<string, byte>)) as Dictionary<string, byte>;
            }
        }

        public bool IsFirstTrackPlaying()
        {
            if (PlaybackQueue == null || !PlaybackQueue.Contains(_activeTrack)) return true;
            return PlaybackQueue?.IndexOf(_activeTrack) == 0;
        }
        public bool IsLastTrackPlaying()
        {
            if (PlaybackQueue == null || !PlaybackQueue.Contains(_activeTrack)) return true;
            return PlaybackQueue?.IndexOf(_activeTrack) == PlaybackQueue.Count - 1;
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

        private void UpdateActiveTrackView()
        {
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(ActiveTrackPicture));
            OnPropertyChanged(nameof(NextTrackTitle));
            OnPropertyChanged(nameof(PreviousTrackTitle));
            OnPropertyChanged(nameof(BitRate));
            OnPropertyChanged(nameof(FileSizeInMb));
            OnPropertyChanged(nameof(IsActiveTrackSelected));
            OnPropertyChanged(nameof(ActiveTrackTitle));
            OnPropertyChanged(nameof(ActiveTrackRating));
        }

        #endregion

    }
}
