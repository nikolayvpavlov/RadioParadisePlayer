using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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

        LogoPage logoPage;
        PlayerPage playerPage;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            logoPage = new LogoPage();
            nvFrame.Navigate(typeof(LogoPage), null, new EntranceNavigationTransitionInfo());
        }        

        private async void navigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            switch (args.SelectedItemContainer.Name)
            {
                case "nviPlay":
                    nvFrame.Navigate(typeof(PlayerPage), Player, new EntranceNavigationTransitionInfo());
                    break;
                case "nviStop":
                    await Player.StopAsync();
                    nvFrame.Navigate(typeof(LogoPage), null, new EntranceNavigationTransitionInfo());
                    break;
            }
            if (args.IsSettingsSelected)
            {
                nvFrame.Navigate(typeof(SettingsPage), SettingsViewModel, new EntranceNavigationTransitionInfo());
            }
        }

        private async void Window_Closed(object sender, WindowEventArgs args)
        {
            if (Player is not null) await Player.StopAsync();
        }
    }
}
