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
using WinRT;
using Windows.UI.Popups;
using Windows.Graphics;
using Windows.Devices.PointOfService;
using Windows.Graphics.Display;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Devices.Display.Core;

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
        App app;

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

        public MainWindow()
        {
            this.InitializeComponent();

            App.Current.UnhandledException += Global_UnhandledException;

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

            app = App.Current as App;

            var da = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);

            var windowSizeW = app.AppConfig.ReadValue("WindowSizeWidth", 0);
            var windowSizeH = app.AppConfig.ReadValue("WindowSizeHeight", 0);
            if (windowSizeW > 0 && da.WorkArea.Width > windowSizeW &&
                windowSizeH > 0 && da.WorkArea.Height > windowSizeH)
            {
                appWindow.Resize(new SizeInt32(windowSizeW, windowSizeH));
            }

            var windowPositionX = app.AppConfig.ReadValue("WindowPositionX", -1);
            var windowPositionY = app.AppConfig.ReadValue("WindowPositionY", -1);
            if (windowPositionX != -1 && windowPositionY != -1 &&
                windowPositionX <= da.WorkArea.Width - appWindow.Size.Width &&
                windowPositionY <= da.WorkArea.Height - appWindow.Size.Height)
            {
                appWindow.Move(new PointInt32(windowPositionX, windowPositionY));
            }

            //Listen for changes to save from now on.  
            appWindow.Changed += AppWindow_Changed_SaveChanges;

            int theme = app.AppConfig.ReadValue("AppTheme", 0);
            SettingsViewModel.AppTheme = (ElementTheme)theme;

            bool autoPlay = app.AppConfig.ReadValue("AutoPlay", false);
            SettingsViewModel.AutoPlay = autoPlay;
            if (autoPlay)
            {
                nvFrame.Navigate(typeof(PlayerPage), Player, new EntranceNavigationTransitionInfo());
            }
        }

        private void AppWindow_Changed_SaveChanges(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange)
            {
                app.AppConfig.WriteValue("WindowSizeWidth", appWindow.Size.Width);
                app.AppConfig.WriteValue("WindowSizeHeight", appWindow.Size.Height);
            }
            if (args.DidPositionChange)
            {
                app.AppConfig.WriteValue("WindowPositionX", appWindow.Position.X);
                app.AppConfig.WriteValue("WindowPositionY", appWindow.Position.Y);
            }
        }

        private async void Global_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var msgText = e.Message
                          + Environment.NewLine + Environment.NewLine
                          + e.Exception.StackTrace
                          ;
            var errorDlg = new MessageDialog(msgText, "Unhandled error");
            WinRT.Interop.InitializeWithWindow.Initialize(errorDlg, WinRT.Interop.WindowNative.GetWindowHandle(this));
            await errorDlg.ShowAsync();
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
    }
}
