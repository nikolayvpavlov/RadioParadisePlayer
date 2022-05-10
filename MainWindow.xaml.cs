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

        public MainWindow()
        {
            this.InitializeComponent();
            player = new Player.Player();
            FinalizeInitialization();
        }

        void FinalizeInitialization()
        {
            Binding imgBinding = new Binding()
            {
                Source = player,
                Path = new PropertyPath("CurrentSlideshowPictureUrl")
            };
            imgSlideshow.SetBinding(Image.SourceProperty, imgBinding);


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

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            await player.PlayAsync();
        }
    }
}
