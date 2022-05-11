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
        private ProgramBlock currentBlock;
        private int currentBlockElapsedTime;
        private DispatcherQueue dispatcherQueue;
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


        private void TransitionToBlock (ProgramBlock block)
        {
            currentBlock = block;
            currentBlockElapsedTime = 0;
            SetCurrentSong();
        }

        private void SetCurrentSong()
        {
            CurrentSong = currentBlock?.Songs.Values
                .FirstOrDefault(s => s.Elapsed >= currentBlockElapsedTime);
            //Reset the slideShow;
            slideshowTimer.Stop();
            currentSongSlideshow = new SongSlideshow(currentBlock, CurrentSong);
            CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            slideshowTimer.Start();
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
            slideshowTimer = new Timer(9_500); //Magic constant; it seems that for every song there are pics to change each 9250 secs;
            slideshowTimer.Elapsed += SlideshowTimer_Elapsed;
        }

        public async Task PlayAsync()
        {
            IsLoading = true;
            Api.RpApiClient apiClient = new Api.RpApiClient();
            var currentBlock = await apiClient.GetProgramBlockAsync(0);
            TransitionToBlock(currentBlock);
            IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
