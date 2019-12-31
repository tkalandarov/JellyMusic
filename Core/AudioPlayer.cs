using System;
using System.Windows.Threading;
using JellyMusic.EventArguments;
using NAudio.Wave;

namespace JellyMusic.Core
{
    public class AudioPlayer : IDisposable
    {
        #region Properties

        private MediaFoundationReader _mediaReader;
        private WaveOut _waveOut;
        private readonly DispatcherTimer _progressUpdateTimer;

        public bool IsInitialized { get => _mediaReader != null && _waveOut != null; }

        public PlaybackState PlaybackState { get => _waveOut?.PlaybackState ?? PlaybackState.Stopped; }

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;

        public TimeSpan Progress
        {
            get => _mediaReader == null ? TimeSpan.Zero : _mediaReader.CurrentTime;
            set
            {
                if (_mediaReader == null) return;
                _mediaReader.CurrentTime = value;
            }
        }
        public TimeSpan Duration => _mediaReader == null ? TimeSpan.Zero : _mediaReader.TotalTime;
        #endregion

        #region Constructors
        public AudioPlayer()
        {
            _progressUpdateTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.1),
                                                       DispatcherPriority.Render,
                                                       (sender, e) => ProgressUpdated?.Invoke(this, new ProgressUpdatedEventArgs(Progress)),
                                                       Dispatcher.CurrentDispatcher);
            _progressUpdateTimer.Start();
        }
        public AudioPlayer(string filePath, float volume) : this()
        {
            InitializeOutput(filePath, volume);
        }
        #endregion

        #region Methods
        public void InitializeOutput(string filePath, float volume = 1)
        {
            if (_waveOut == null)
            {
                _waveOut = new WaveOut() { Volume = volume };
                _waveOut.PlaybackStopped += OnPlaybackStopped;
            }
            _mediaReader = new MediaFoundationReader(filePath);
            _waveOut.Init(_mediaReader);
        }

        public void SetVolume(float value)
        {
            if (_waveOut != null)
            {
                _waveOut.Volume = value;
            }
        }

        public void ChangeTrack(string filePath, bool IsPlaying)
        {
            Dispose();
            InitializeOutput(filePath);

            if (IsPlaying)
                Play();
        }

        public void Stop()
        {
            _waveOut?.Stop();
            if (_mediaReader != null)
                _mediaReader.Position = 0;

            _progressUpdateTimer.Stop();
        }
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackStopped?.Invoke();
        }

        public void Play()
        {
            if (PlaybackState == PlaybackState.Paused)
            {
                PlaybackResumed.Invoke();
            }
            _waveOut.Play();
            _progressUpdateTimer.Start();
        }

        public void Pause()
        {
            _waveOut?.Pause();
            _progressUpdateTimer.Stop();

            PlaybackPaused.Invoke();
        }

        public void TogglePlay()
        {
            if (PlaybackState != PlaybackState.Playing)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        public void Dispose()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_mediaReader != null)
            {
                _mediaReader.Dispose();
                _mediaReader = null;
            }
        }
        #endregion
    }
}
