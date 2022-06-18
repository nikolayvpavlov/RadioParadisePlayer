using Microsoft.UI.Dispatching;
using RadioParadisePlayer.Api;
using RadioParadisePlayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Storage;

namespace RadioParadisePlayer.Logic
{
    internal sealed class Player : INotifyPropertyChanged
    {
        private const int PlayerTimerGranularity = 250;

        private Playlist currentPlaylist;
        private DispatcherQueue dispatcherQueue;
        private Task playerTask;
        private CancellationTokenSource ctsPlayer;
        private DispatcherQueueTimer slideshowTimer;
        private DispatcherQueueTimer playerTimer;
        private SongSlideshow currentSongSlideshow;
        private object lockSlideshow = new object();
        private Microsoft.UI.Media.Playback.MediaPlayer mPlayer;

        internal static User User { get; private set; } = null;

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private bool isPlaying;

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }

        private string currentSlideshowPictureUrl;

        public string CurrentSlideshowPictureUrl
        {
            get { return currentSlideshowPictureUrl; }
            set
            {
                if (currentSlideshowPictureUrl != value)
                {
                    currentSlideshowPictureUrl = value;
                    OnPropertyChanged(nameof(CurrentSlideshowPictureUrl));
                }
            }
        }

        private bool slideshowEnabled = true;

        public bool SlideshowEnabled
        {
            get { return slideshowEnabled; }
            set
            {
                if (slideshowEnabled != value)
                {
                    slideshowEnabled = value;
                    OnPropertyChanged(nameof(SlideshowEnabled));
                    if (IsPlaying && SlideshowEnabled) slideshowTimer.Start();
                    (App.Current as App).AppConfig.WriteValue("SlideShow", slideshowEnabled);
                }
            }
        }

        private int currentSongIndex;
        private Song currentSong;

        public string CurrentSongCoverArtPictureUrl => currentPlaylist?.Image_Base + currentSong.Cover_Art;

        public Song CurrentSong
        {
            get { return currentSong; }
            set
            {
                if (currentSong != value)
                {
                    currentSong = value;
                    OnPropertyChanged(nameof(CurrentSong));
                }
            }
        }

        private int currentSongProgress;

        public int CurrentSongProgress
        {
            get { return currentSongProgress; }
            set
            {
                if (currentSongProgress != value)
                {
                    currentSongProgress = value;
                    OnPropertyChanged(nameof(CurrentSongProgress));
                }
            }
        }

        public double Volume
        {
            get { return mPlayer.Volume; }
            set
            {
                if (mPlayer.Volume != value)
                {
                    mPlayer.Volume = value;
                    OnPropertyChanged(nameof(Volume));
                    (App.Current as App).AppConfig.WriteValue("Volume", mPlayer.Volume);
                }
            }
        }

        public int BitRate { get; private set; }
        public string Channel { get; private set; }

        public SongInfoViewModel SongInfo { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Error(Exception x)
        {
            await StopAsync();
            OnError(x);
        }

        public event Action<Exception> OnError;

        private void PlaySong()
        {
            try
            {
                var source = Microsoft.UI.Media.Core.MediaSource.CreateFromUri(new Uri(CurrentSong.Gapless_Url));
                mPlayer.Source = source;
                mPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(CurrentSong.Cue);
                mPlayer.Play();
            }
            catch (Exception x)
            {
                Error(x);
            }
        }

        private async Task MoveToNextSong()
        {
            SongInfo.Clear();
            currentSongIndex++;
            if (currentSongIndex >= currentPlaylist.Songs.Count) return;
            CurrentSong = currentPlaylist.Songs[currentSongIndex];
            CurrentSongProgress = CurrentSong.Cue;
            PlaySong();
            //Reset the slideShow;
            slideshowTimer.Stop();
            lock (lockSlideshow)
            {
                currentSongSlideshow = new SongSlideshow(currentPlaylist, CurrentSong);
                CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            }
            if (SlideshowEnabled)
            {
                slideshowTimer.Start();
            }
            else
            {
                slideshowTimer.Stop();
            }
            //Last thing: notify the service
            await RpApiClient.NotifyServiceSongStarts(CurrentSong, Channel);
        }

        private async Task LoadPlaylist()
        {
            currentPlaylist = await RpApiClient.GetPlaylistAsync(User.User_Id, Channel, BitRate.ToString());
            currentSongIndex = -1;
        }

        private void PlayerTimer_Ticked(DispatcherQueueTimer sender, object data)
        {
            CurrentSongProgress = (int)mPlayer.PlaybackSession.Position.TotalMilliseconds;
        }

        private void SlideshowTimer_Ticked(DispatcherQueueTimer sender, object data)
        {
            lock (lockSlideshow)
            {
                currentSongSlideshow.MoveNext();
                CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            }
        }

        public Player()
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            slideshowTimer = dispatcherQueue.CreateTimer();
            slideshowTimer.Interval = TimeSpan.FromMilliseconds(10_000); //10 secs following Jarred (RP)
            slideshowTimer.Tick += SlideshowTimer_Ticked;

            playerTimer = dispatcherQueue.CreateTimer();
            playerTimer.Interval = TimeSpan.FromMilliseconds(250);
            playerTimer.Tick += PlayerTimer_Ticked;

            BitRate = (App.Current as App).AppConfig.ReadValue<int>("BitRate", 3);
            Channel = (App.Current as App).AppConfig.ReadValue<string>("Channel", "0");
            mPlayer = new() { };
            Volume = (App.Current as App).AppConfig.ReadValue<double>("Volume", 0.3);
            mPlayer.MediaEnded += MPlayer_MediaEnded;

            SongInfo = new SongInfoViewModel();
        }

        private void MPlayer_MediaEnded(Microsoft.UI.Media.Playback.MediaPlayer sender, object args)
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await MoveToNextSong();
                    if (currentSongIndex >= currentPlaylist.Songs.Count)
                    {
                        await LoadPlaylist();
                        await MoveToNextSong();
                    }
                }
                catch (Exception x)
                {
                    Error(x);
                }
            });
        }

        public async Task PlayAsync()
        {
            IsLoading = true;
            try
            {
                if (User is null)
                {
                    User = await RpApiClient.AuthenticateAsync();
                }
                await LoadPlaylist();
                await MoveToNextSong();
                playerTimer.Start();
                IsPlaying = true;
            }
            catch (Exception x)
            {
                Error(x);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task StopAsync()
        {
            if (!IsPlaying) return;
            mPlayer.Pause(); //There is no stop method.
            slideshowTimer.Stop();
            playerTimer.Stop();
            IsPlaying = false;
            await RpApiClient.NotifyServiceSongPause((int)mPlayer.PlaybackSession.Position.TotalMilliseconds, CurrentSong, Channel);
        }

        public async Task SetBitRate(int bitRate)
        {
            if (bitRate < 0 && bitRate > 4)
            {
                throw new ArgumentOutOfRangeException("BitRate must be between 0 and 4");
            }
            if (IsPlaying)
            {
                await StopAsync();
                await PlayAsync();
            }
            (App.Current as App).AppConfig.WriteValue("BitRate", bitRate);
        }

        public async Task SetChannel(string channel)
        {
            Channel = channel;
            if (IsPlaying)
            {
                await StopAsync();
                await PlayAsync();
            }
            (App.Current as App).AppConfig.WriteValue("Channel", channel);
        }

        public async Task LoadCurrentSongInfoAsync()
        {
            if (!IsPlaying || CurrentSong == null) return;
            await SongInfo.LoadSongInfo(CurrentSong, User.User_Id);
        }
    }
}
