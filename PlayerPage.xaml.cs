using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RadioParadisePlayer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RadioParadisePlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class PlayerPage : Page
    {
        Logic.Player player;

        Microsoft.UI.Media.Playback.MediaPlayer mPlayer;
        BitmapImage BitmapImageSlideshowOne { get; set; }
        BitmapImage BitmapImageSlideshowTwo { get; set; }
        BitmapImage BitmapImageCoverArt { get; set; }

        SemaphoreSlim semaphoreCoverArtImage;
        SemaphoreSlim semaphoreCoverSlideshowOne;
        SemaphoreSlim semaphoreCoverSlideshowTwo;

        public PlayerPage(Logic.Player player)
        {
            this.InitializeComponent();

            BitmapImageSlideshowOne = new BitmapImage();
            BitmapImageSlideshowTwo = new BitmapImage();
            BitmapImageCoverArt = new BitmapImage();
            BitmapImageSlideshowOne.ImageOpened += BitmapImageSlideshowOne_ImageOpened;
            BitmapImageSlideshowTwo.ImageOpened += BitmapImageSlideshowTwo_ImageOpened;
            BitmapImageCoverArt.ImageOpened += BitmapImageCoverArt_ImageOpened;
            semaphoreCoverArtImage = new SemaphoreSlim(1);
            semaphoreCoverSlideshowOne = new SemaphoreSlim(1);
            semaphoreCoverSlideshowTwo = new SemaphoreSlim(1);

            this.player = player;
            player.PropertyChanged += Player_PropertyChanged;

            mPlayer = new Microsoft.UI.Media.Playback.MediaPlayer();
            mPlayer.Volume = 0.4;
            mPlayer.MediaFailed += MPlayer_MediaFailed;
        }

        private void BitmapImageSlideshowTwo_ImageOpened(object sender, RoutedEventArgs e)
        {
            semaphoreCoverSlideshowTwo.Release();
        }

        private void BitmapImageSlideshowOne_ImageOpened(object sender, RoutedEventArgs e)
        {
            semaphoreCoverSlideshowOne.Release();
        }

        private void BitmapImageCoverArt_ImageOpened(object sender, RoutedEventArgs e)
        {
            semaphoreCoverArtImage.Release();
        }

        public async Task PlayAsync()
        {
            await player.PlayAsync();
        }

        public async Task StopAsync()
        {
            mPlayer.Pause();
            await player.StopAsync();
        }

        public bool IsPlaying => player.IsPlaying;

        private async void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSlideshowPictureUrl":
                    var stream = await Api.RpApiClient.DownloadImageAsync(player.CurrentSlideshowPictureUrl);
                    if (imgSlideshowOne.Opacity == 0)
                    {
                        await semaphoreCoverSlideshowOne.WaitAsync();
                        await BitmapImageSlideshowOne.SetSourceAsync(stream.AsRandomAccessStream());
                        imgSlideshowOne.Opacity = 1;
                        imgSlideshowTwo.Opacity = 0;
                    }
                    else
                    {
                        await semaphoreCoverSlideshowTwo.WaitAsync();
                        await BitmapImageSlideshowTwo.SetSourceAsync(stream.AsRandomAccessStream());
                        imgSlideshowOne.Opacity = 0;
                        imgSlideshowTwo.Opacity = 1;
                    }
                    break;

                case "CurrentSong":
                    Playsong();
                    await semaphoreCoverArtImage.WaitAsync();
                    await LoadCoverArtAsync();
                    break;
            }
        }

        private void Playsong()
        {
            var source = Microsoft.UI.Media.Core.MediaSource.CreateFromUri(new Uri(player.CurrentSong.Gapless_Url));
            mPlayer.Source = source;
            mPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(player.CurrentSong.Cue);
            mPlayer.Play();
        }

        private async Task LoadCoverArtAsync()
        {
            var stream = await Api.RpApiClient.DownloadImageAsync("https:" + player.CurrentSongCoverArtPictureUrl);
            await BitmapImageCoverArt.SetSourceAsync(stream.AsRandomAccessStream());
        }

        private void MPlayer_MediaFailed(Microsoft.UI.Media.Playback.MediaPlayer sender, Microsoft.UI.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            string s = args.ErrorMessage;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (spPlayerInfo.Children.Contains(gridInfo))
            {
                spPlayerInfo.Children.Remove(gridInfo);
            }
            else
            {
                spPlayerInfo.Children.Add(gridInfo);
            }
        }
    }
}
