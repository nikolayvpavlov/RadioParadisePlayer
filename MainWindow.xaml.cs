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
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RadioParadisePlayer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        AppWindow appWindow;

        Logic.Player player = null;
        Logic.Player Player
        {
            get
            {
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
                    vmSettings.PropertyChanged += VmSettings_PropertyChanged;
                }
                return vmSettings;
            }
        }

        private void VmSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AppTheme":
                    (Content as Grid).RequestedTheme = vmSettings.AppTheme; 
                    break;
            }
        }

        private AppWindow getAppWindow()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            return AppWindow.GetFromWindowId(windowId);
        }

        public MainWindow()
        {
            this.InitializeComponent();
            nvFrame.Navigate(typeof(LogoPage), null, new EntranceNavigationTransitionInfo());
            player = new Logic.Player();
            player.OnError += Player_OnError;

            Title = "Radio Paradise Player";

            appWindow = XamlHelpers.GetAppWindowForWindow(this);
            appWindow.SetIcon("Assets/rp_icon.ico");
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            }
            appWindow.Closing += AppWindow_Closing;
            int theme = (App.Current as App).AppConfig.ReadValue<int>("AppTheme", 0);
            SettingsViewModel.AppTheme = (ElementTheme)theme;
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            nvFrame.Navigate(typeof(LogoPage));
            if (Player is not null) await Player.StopAsync();
        }

        private void Player_OnError(Exception obj)
        {
            nvFrame.Navigate(typeof(ErrorPage), obj);
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
            
        }
    }
}
