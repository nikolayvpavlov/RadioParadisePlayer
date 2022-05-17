using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RadioParadisePlayer.Helpers;
using RadioParadisePlayer.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RadioParadisePlayer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        Player.Player player;

        BitmapImage bitmapImageSlideshowOne;
        BitmapImage bitmapImageSlideshowTwo;

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            player = new Player.Player();
            player.PropertyChanged += Player_PropertyChanged;
            FinalizeInitialization();
        }

        private async void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSlideshowPictureUrl":
                    var stream = await Api.RpApiClient.DownloadImageAsync(player.CurrentSlideshowPictureUrl);

                    if (imgSlideshowOne.Opacity == 0)
                    {
                        await bitmapImageSlideshowOne.SetSourceAsync(stream.AsRandomAccessStream());
                        imgSlideshowOne.Opacity = 1;
                        imgSlideshowTwo.Opacity = 0;
                    }  
                    else
                    {
                        await bitmapImageSlideshowTwo.SetSourceAsync(stream.AsRandomAccessStream());
                        imgSlideshowOne.Opacity = 0;
                        imgSlideshowTwo.Opacity = 1;
                    }
                    break;
            }
        }

        void FinalizeInitialization()
        {
            bitmapImageSlideshowOne = new BitmapImage();
            imgSlideshowOne.Source = bitmapImageSlideshowOne;

            bitmapImageSlideshowTwo = new BitmapImage();
            imgSlideshowTwo.Source = bitmapImageSlideshowTwo;

            Binding prgRingBinding = new Binding()
            {
                Source = player,
                Path = new PropertyPath("IsLoading"),
            };
            progressRing.SetBinding(ProgressRing.IsActiveProperty, prgRingBinding);

            Binding prgRingVisibilityBinding = new Binding()
            {
                Source = player,
                Path = new PropertyPath("IsLoading"),
                Converter = new BoolToVisibilityConverter()
            };
            progressRing.SetBinding(ProgressRing.VisibilityProperty, prgRingVisibilityBinding);

        }

        private async void navigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            await player.PlayAsync();
        }
    }
}
