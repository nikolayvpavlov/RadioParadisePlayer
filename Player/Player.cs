using Microsoft.UI.Dispatching;
using RadioParadisePlayer.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RadioParadisePlayer.Player
{
    internal class Player : INotifyPropertyChanged
    {
        private static User user = null;

        private Playlist currentPlaylist;
        private DispatcherQueue dispatcherQueue;
        private Timer songTimer;
        private Timer slideshowTimer;
        private SongSlideshow currentSongSlideshow;

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

        private async Task LoadPlaylist()
        {
            currentPlaylist = await RpApiClient.GetPlaylistAsync();
            currentSongIndex = -1;
            await MoveNextSongAsync();
        }

        private async Task MoveNextSongAsync()
        {
            currentSongIndex++;

            if (currentPlaylist.Songs.Count <= currentSongIndex)
            {
                await LoadPlaylist();
            }

            CurrentSong = currentPlaylist.Songs[currentSongIndex];
            CurrentSongProgress = CurrentSong.Cue;

            //Reset the slideShow;
            slideshowTimer.Stop();
            currentSongSlideshow = new SongSlideshow(currentPlaylist, CurrentSong);
            CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            slideshowTimer.Start();
        }

        private void SongTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                await MoveNextSongAsync();
            });
        }

        private void SlideshowTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                currentSongSlideshow.MoveNext();
                CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            });
        }

        public Player()
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            slideshowTimer = new Timer(8000); //10 secs minus 2 secs for fade-out / fade-in, following Jarred (RP)
            slideshowTimer.Elapsed += SlideshowTimer_Elapsed;

            songTimer = new Timer(500);
            songTimer.Elapsed += SongTimer_Elapsed;
        }


        public async Task PlayAsync()
        {
            IsLoading = true;
            if (user is null)
            {
                user = await RpApiClient.AuthenticateAsync();
            }
            await LoadPlaylist();
            IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
