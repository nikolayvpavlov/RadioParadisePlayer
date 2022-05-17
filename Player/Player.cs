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

namespace RadioParadisePlayer.Player
{
    internal class Player : INotifyPropertyChanged
    {
        private static User user = null;
        private Playlist currentPlaylist;
        private DispatcherQueue dispatcherQueue;
        private Task playerTask;
        private CancellationTokenSource ctsPlayer;
        private System.Timers.Timer slideshowTimer;
        private SongSlideshow currentSongSlideshow;
        private object lockSlideshow = new object();

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

        private void MoveNextSong()
        {
            currentSongIndex++;

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

        
        private async Task PlayerWoker(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                if (currentPlaylist is null ||
                       (currentSongProgress >= currentSong.Duration &&
                       currentSongIndex >= currentPlaylist.Songs.Count))
                {
                    currentPlaylist = await RpApiClient.GetPlaylistAsync();
                    currentSongIndex = -1;
                    dispatcherQueue.TryEnqueue(MoveNextSong);
                }
                else if (currentSongProgress >= currentSong.Duration)
                {
                    dispatcherQueue.TryEnqueue(MoveNextSong);
                }
                dispatcherQueue.TryEnqueue(() => CurrentSongProgress += 500);
                await Task.Delay(500, cancellation);
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
            slideshowTimer = new System.Timers.Timer(8000); //10 secs minus 2 secs for fade-out / fade-in, following Jarred (RP)
            slideshowTimer.Elapsed += SlideshowTimer_Elapsed;
        }

        public async Task PlayAsync()
        {
            IsLoading = true;
            if (user is null)
            {
                user = await RpApiClient.AuthenticateAsync();
            }
            ctsPlayer = new CancellationTokenSource();
            playerTask = Task.Run(async () => await PlayerWoker(ctsPlayer.Token));
            IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
