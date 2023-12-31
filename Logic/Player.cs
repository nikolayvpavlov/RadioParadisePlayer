﻿using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Automation;
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
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace RadioParadisePlayer.Logic
{
    internal sealed class Player : INotifyPropertyChanged
    {
        private const int PlayerTimerGranularity = 250;

        private Playlist currentPlaylist;
        private DispatcherQueue dispatcherQueue;
        private DispatcherQueueTimer slideshowTimer;
        private DispatcherQueueTimer playerTimer;
        private SongSlideshow currentSongSlideshow;
        private object lockSlideshow = new object();
        private MediaPlayer mPlayer;
        private MediaPlaybackList mPlaybackList;

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

        public string CurrentSongCoverArtPictureUrl => currentPlaylist?.Image_Base + currentSong?.Cover_Art;

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
            OnError?.Invoke(x);
        }

        public event Action<Exception> OnError;

        private void PlayerTimer_Ticked(DispatcherQueueTimer sender, object data)
        {
            CurrentSongProgress = (int)mPlayer.PlaybackSession.Position.TotalMilliseconds;
            //We still have a bug that sometimes the player doesn't start new songs.  No idea why.
            if (IsPlaying && 
                (mPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None ||
                mPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused))
            {
                mPlayer.Play();
            }
        }

        private void SlideshowTimer_Ticked(DispatcherQueueTimer sender, object data)
        {
            lock (lockSlideshow)
            {
                currentSongSlideshow.MoveNext();
                CurrentSlideshowPictureUrl = currentSongSlideshow.CurrentPictureUrl;
            }
        }

        private async Task LoadPlaylist()
        {
            int currentEventId = CurrentSong?.Event_Id ?? 0;
            int attempts = 0;
            while (true)
            {
                currentPlaylist = await RpApiClient.GetPlaylistAsync(User.User_Id, Channel, BitRate.ToString(), currentEventId.ToString());
                //It is possible to get an "expired" song, where Cue is bigger than the Duration.  This causes the player to stop playing.  Skip them.
                currentPlaylist.Songs = currentPlaylist.Songs.Where(s => s.Cue < s.Duration).ToList();
                if (currentPlaylist.Songs.Any()) break;
                attempts++;
                if (attempts > 3) currentEventId = 0; //If can't get songs for the current eventId, try with zero.
            }
            currentSongIndex = -1;
            var mediaItems = currentPlaylist.Songs
                .Select(song => new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(song.Gapless_Url)))).ToArray();
            List<Task> tasks = new();
            foreach (var item in mediaItems)
            {
                var t = item.Source.OpenAsync().AsTask(); //Eliminate the gap between songs
                tasks.Add(t);
                mPlaybackList.Items.Add(item);
            }
            await Task.WhenAll(tasks); //We need to do this to catch possible errors of Source.OpenAsync
        }

        private void StartPlayer()
        {
            mPlayer.Play();
            mPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(currentPlaylist.Songs.First().Cue);
            mPlayer.AutoPlay = true;
        }

        private async Task MoveToNextSong()
        {
            SongInfo.Clear();
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
            if (SlideshowEnabled)
            {
                slideshowTimer.Start();
            }
            else
            {
                slideshowTimer.Stop();
            }
            //Check if this is the last song.  Load the next playlist, and reset the index to -1.
            if (currentSongIndex == currentPlaylist.Songs.Count - 1)
            {
                await LoadPlaylist();
            }

            //Last thing: notify the service
            await RpApiClient.NotifyServiceSongStarts(CurrentSong, Channel);
        }

        public Player()
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            slideshowTimer = dispatcherQueue.CreateTimer();
            slideshowTimer.Interval = TimeSpan.FromMilliseconds(10_000); //10 secs following Jarred (RP)
            slideshowTimer.Tick += SlideshowTimer_Ticked;

            playerTimer = dispatcherQueue.CreateTimer();
            playerTimer.Interval = TimeSpan.FromMilliseconds(PlayerTimerGranularity);
            playerTimer.Tick += PlayerTimer_Ticked;

            var app = App.Current as App;
            BitRate = app.AppConfig.ReadValue<int>("BitRate", 3);
            Channel = app.AppConfig.ReadValue<string>("Channel", "0");

            mPlaybackList = new()
            {
                MaxPlayedItemsToKeepOpen = 2,
                MaxPrefetchTime = TimeSpan.FromSeconds(3)
            };
            mPlaybackList.CurrentItemChanged += MPlaybackList_CurrentItemChanged;
            mPlayer = new() { Source = mPlaybackList };
            mPlayer.AudioStateMonitor.SoundLevelChanged += AudioStateMonitor_SoundLevelChanged;

            Volume = app.AppConfig.ReadValue<double>("Volume", 0.3);

            SongInfo = new SongInfoViewModel();
        }

        private void AudioStateMonitor_SoundLevelChanged(Windows.Media.Audio.AudioStateMonitor sender, object args)
        {
            switch (sender.SoundLevel)
            {
                case Windows.Media.SoundLevel.Low:
                    break;
                case Windows.Media.SoundLevel.Muted:
                    break;
                case Windows.Media.SoundLevel.Full:
                    break;
            }
        }

        private void MPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (args.NewItem is null) return; //We're stopping.  No need to do anything.
            dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await MoveToNextSong();
                }
                catch (Exception x)
                {
                    await StopAsync();
                    Error(x.InnerException);
                }
            });
        }

        public async Task PlayAsync()
        {
            if (IsPlaying || IsLoading) return;
            IsLoading = true;
            try
            {
                if (User is null)
                {
                    User = await RpApiClient.AuthenticateAsync();
                }
                await LoadPlaylist();
                StartPlayer();
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
            slideshowTimer.Stop();
            playerTimer.Stop();
            mPlayer.AutoPlay = false;
            mPlayer.Pause(); //There is no stop method.
            mPlayer.Source = null; //If you don't do that, the next line mPlaybackList.Items.Clear() will essentially block;
            mPlaybackList.Items.Clear();
            mPlayer.Source = mPlaybackList;
            currentPlaylist = null;
            IsPlaying = false;
            CurrentSong = null;            
            await RpApiClient.NotifyServiceSongPause((int)mPlayer.PlaybackSession.Position.TotalMilliseconds, CurrentSong, Channel);
        }

        public async Task SetBitRate(int bitRate)
        {
            if (bitRate < 0 && bitRate > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bitRate), "BitRate must be between 0 and 4");
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
