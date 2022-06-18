using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using RadioParadisePlayer.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Logic
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private DispatcherQueue dispatcherQueue;
        public ObservableCollection<ChannelView> ChannelsList { get; private set; }

        public Player Player { get; private set; }

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

        private string selectedChannel;

        public string SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                if (selectedChannel != value)
                {
                    selectedChannel = value;
                    OnPropertyChanged(nameof(SelectedChannel));
                    dispatcherQueue.TryEnqueue(async () => await Player?.SetChannel(SelectedChannel));
                }
            }
        }

        private int selectedBitRate;

        public int SelectedBitRate
        {
            get { return selectedBitRate; }
            set
            {
                if (selectedBitRate != value)
                {
                    selectedBitRate = value;
                    OnPropertyChanged(nameof(SelectedBitRate));
                    dispatcherQueue.TryEnqueue(async () => await Player?.SetBitRate(SelectedBitRate));
                }
            }
        }

        private ElementTheme appTheme;

        public ElementTheme AppTheme
        {
            get { return appTheme; }
            set 
            {
                if (appTheme != value)
                {
                    appTheme = value;
                    OnPropertyChanged(nameof(AppTheme));
                    (App.Current as App).AppConfig.WriteValue("AppTheme", (int)AppTheme);
                }
                appTheme = value; 
            }
        }

        private bool autoPlay;

        public bool AutoPlay
        {
            get { return autoPlay; }
            set
            {
                if (autoPlay != value)
                {
                    autoPlay = value;
                    OnPropertyChanged(nameof(AutoPlay));
                    (App.Current as App).AppConfig.WriteValue("AutoPlay", autoPlay);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsViewModel(Player player)
        {
            ChannelsList = new ObservableCollection<ChannelView>();
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Player = player;
            selectedChannel = player.Channel;
            selectedBitRate = player.BitRate;
        }

        private async Task LoadChannels()
        {
            if (ChannelsList.Count > 0) return; //No need to reload channels all the time. Once is just enough.
            var channels = await RpApiClient.GetChannelsAsync(Logic.Player.User?.User_Id);
            foreach (var c in channels)
            {
                var ch = new ChannelView(c);
                ChannelsList.Add(ch);
                _ = Task.Run(async () =>
                {
                    var imgStream = await RpApiClient.DownloadImageAsync(ch.Image);
                    dispatcherQueue.TryEnqueue(async () => await ch.BitmapImage.SetSourceAsync(imgStream.AsRandomAccessStream()));
                });
            }
        }
        public async Task Initialize()
        {
            IsLoading = true;
            await LoadChannels();
            IsLoading = false;
        }
    }
}
