using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RadioParadisePlayer.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RadioParadisePlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class SettingsPage : Page
    {
        SettingsViewModel vmSettings { get; set; }

        public SettingsPage(SettingsViewModel vm)
        {
            vmSettings = vm;
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;            
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await vmSettings.Initialize();

            //Fixes an issue in WinUI 3: https://github.com/microsoft/microsoft-ui-xaml/issues/6335
            rbChannel.UpdateLayout();
            //TO DO: Implement in a better way with proper error handling
            rbChannel.SelectedIndex = Int32.Parse(vmSettings.SelectedChannel ?? "0");

            rbBitRate.SelectedIndex = vmSettings.SelectedBitRate;
        }

        private void rbBitRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbBitRate.SelectedIndex != vmSettings.SelectedBitRate)
            {
                vmSettings.SelectedBitRate = rbBitRate.SelectedIndex;
            }
        }

        private void rbChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedChannel = rbChannel.SelectedItem as ChannelView;
            if (selectedChannel?.Chan != vmSettings.SelectedChannel)
            {
                vmSettings.SelectedChannel = selectedChannel?.Chan;
            }
        }
    }
}
