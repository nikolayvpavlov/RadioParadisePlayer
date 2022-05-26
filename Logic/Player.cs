using Microsoft.UI.Dispatching;
using RadioParadisePlayer.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RadioParadisePlayer.Logic
{
    internal class Player : INotifyPropertyChanged
    {
        private const int PlayerTimerGranularity = 250;

        private Playlist currentPlaylist;
        private DispatcherQueue dispatcherQueue;
        private Task playerTask;
        private CancellationTokenSource ctsPlayer;
        private System.Timers.Timer slideshowTimer;
        private SongSlideshow currentSongSlideshow;
        private object lockSlideshow = new object();

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

        public int BitRate { get; private set; }
        public string Channel { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MoveToNextSong()
        {
            currentSongIndex++;
            if (currentSongIndex >= currentPlaylist.Songs.Count) return;
            CurrentSong = currentPlaylist.Songs[currentSongIndex];
            CurrentSongProgress = CurrentSong.Cue;            
            //Reset the slideShow;
            slideshowTimer.Stop();
            lock (lockSlideshow)
            {
                currentSongSlideshow = new SongSlideshow(currentPlaylist, CurrentSong);
                CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            }
            slideshowTimer.Start();
        }

        private async Task LoadPlaylist()
        {
            currentPlaylist = await RpApiClient.GetPlaylistAsync(User.User_Id, Channel, BitRate.ToString());
            currentSongIndex = -1;
        }

        private async Task PlayerWoker(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                if (CurrentSongProgress >= currentSong.Duration)
                {
                    dispatcherQueue.TryEnqueue(MoveToNextSong);
                }
                if (currentSongIndex >= currentPlaylist.Songs.Count)
                {
                    await LoadPlaylist();
                    dispatcherQueue.TryEnqueue(MoveToNextSong);
                }
                dispatcherQueue.TryEnqueue(() => CurrentSongProgress += PlayerTimerGranularity);
                await Task.Delay(PlayerTimerGranularity);                
            }
        }

        private void SlideshowTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                lock (lockSlideshow)
                {
                    currentSongSlideshow.MoveNext();
                    CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
                }
            });
        }

        public Player()
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            slideshowTimer = new System.Timers.Timer(10_000); //10 secs following Jarred (RP)
            slideshowTimer.Elapsed += SlideshowTimer_Elapsed;
            BitRate = 3;
            Channel = "0";
        }

        public async Task PlayAsync()
        {
            IsLoading = true;
            if (User is null)
            {
                User = await RpApiClient.AuthenticateAsync();
            }
            ctsPlayer = new CancellationTokenSource();
            await LoadPlaylist();
            MoveToNextSong();
            playerTask = Task.Run(async () => await PlayerWoker(ctsPlayer.Token));
            IsLoading = false;
            IsPlaying = true;
        }

        public async Task StopAsync()
        {
            if (!IsPlaying) return;
            ctsPlayer?.Cancel();
            await playerTask;
            IsPlaying = false;
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
        }

        public async Task SetChannel(string channel)
        {            
            Channel = channel;
            if (IsPlaying)
            {
                await StopAsync();
                await PlayAsync();
            }
        }

    }
}
