using Microsoft.UI.Windowing;
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
using System.Net.Http;
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
        Logic.Player Player { get; set; }

        BitmapImage BitmapImageSlideshowOne { get; set; }
        BitmapImage BitmapImageSlideshowTwo { get; set; }
        BitmapImage BitmapImageCoverArt { get; set; }

        SemaphoreSlim semaphoreCoverArtImage;
        SemaphoreSlim semaphoreCoverSlideshowOne;
        SemaphoreSlim semaphoreCoverSlideshowTwo;

        AppWindow appWindow;

        public PlayerPage()
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

            appWindow = XamlHelpers.GetAppWindowForWindow(XamlHelpers.GetWindow());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Player = e.Parameter as Logic.Player;
            Player.PropertyChanged += Player_PropertyChanged;
            base.OnNavigatedTo(e);
            if (!Player.IsPlaying) await Player.PlayAsync();
            else
            {
                Player_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("CurrentSlideshowPictureUrl"));
                Player_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("CurrentSong"));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.Player.PropertyChanged -= Player_PropertyChanged;
            base.OnNavigatedFrom(e);
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

        private async void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSlideshowPictureUrl":
                    try
                    {
                        if ((appWindow?.Presenter as OverlappedPresenter)?.State == OverlappedPresenterState.Minimized)
                        {
                            //Don't download images if we're minimized.
                            break;
                        }
                        var stream = await Api.RpApiClient.DownloadImageAsync(Player.CurrentSlideshowPictureUrl);
                        if (imgSlideshowOne.Opacity == 0)
                        {
                            await semaphoreCoverSlideshowOne.WaitAsync();
                            try
                            {
                                await BitmapImageSlideshowOne.SetSourceAsync(stream.AsRandomAccessStream());
                                imgSlideshowOne.Opacity = 1;
                                imgSlideshowTwo.Opacity = 0;
                            }
                            catch (AggregateException)
                            {
                                //Ignore it.  Something went wrong with loading the image
                            }
                        }
                        else
                        {
                            await semaphoreCoverSlideshowTwo.WaitAsync();
                            try
                            {

                                await BitmapImageSlideshowTwo.SetSourceAsync(stream.AsRandomAccessStream());
                                imgSlideshowOne.Opacity = 0;
                                imgSlideshowTwo.Opacity = 1;
                            }
                            catch (AggregateException)
                            {
                                //Ignore it.  Something went wrong with loading the image
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        //Do nothing. Just ignore this.
                    }
                    break;

                case "CurrentSong":
                    await semaphoreCoverArtImage.WaitAsync();
                    try
                    {
                        await LoadCoverArtAsync();
                    }
                    catch (AggregateException)
                    {
                        //Ignore it.  Something went wrong with loading the image
                    }
                    break;
            }
        }

        private async Task LoadCoverArtAsync()
        {
            if (String.IsNullOrEmpty(Player.CurrentSongCoverArtPictureUrl)) return;
            try
            {
                var stream = await Api.RpApiClient.DownloadImageAsync("https:" + Player.CurrentSongCoverArtPictureUrl);
                await BitmapImageCoverArt.SetSourceAsync(stream.AsRandomAccessStream());
            }
            catch (HttpRequestException)
            {
                //Do nothing. Just ignore this.
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (spPlayerInfo.Children.Contains(gridInfo))
            {
                spPlayerInfo.Children.Remove(gridInfo);
                (toggleInfo.Content as FontIcon).Glyph = "\xE70e";
            }
            else
            {
                spPlayerInfo.Children.Add(gridInfo);
                (toggleInfo.Content as FontIcon).Glyph = "\xE70d";
            }
        }

        private async void Flyout_Opening(object sender, object e)
        {
            await Player.LoadCurrentSongInfoAsync();
        }
    }
}
