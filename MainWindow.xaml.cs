using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RadioParadisePlayer.Helpers;
using RadioParadisePlayer.Logic;
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
        Logic.Player player = null;
        Logic.Player Player
        {
            get
            {
                if (player is null) player = new Logic.Player();
                return player;
            }
        }

        SettingsViewModel vmSettings;
        SettingsViewModel SettingsViewModel
        {
            get
            {
                if (vmSettings is null)
                {
                    vmSettings = new SettingsViewModel(Player);
                }
                return vmSettings;
            }
        }

        PlayerPage playerPage;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
        }

        private void NavigateToPage (Page nextPage)
        {
            if ((navigationView.Content as Page) != nextPage && nextPage != null)
            {
                navigationView.Content = nextPage;
            }
        }

        private async void navigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            switch (args.SelectedItemContainer.Name)
            {
                case "nviPlay":
                    if (playerPage is null)
                    {
                        playerPage = new PlayerPage(Player);
                    }
                    NavigateToPage(playerPage);
                    if (!player.IsPlaying) await player.PlayAsync();
                    break;
                case "nviStop":
                    if (playerPage != null)
                    {
                        navigationView.Content = null;
                        await playerPage.StopAsync();
                    }
                    break;
            }
            if (args.IsSettingsSelected)
            {
                NavigateToPage (new SettingsPage(SettingsViewModel));
            }
        }

        private async void Window_Closed(object sender, WindowEventArgs args)
        {
            await player?.StopAsync();
        }
    }
}
