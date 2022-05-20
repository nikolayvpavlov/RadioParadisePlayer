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
        PlayerPage playerPage;

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;            
        }

        private async void navigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Page nextPage = null;
            switch (args.SelectedItemContainer.Name)
            {
                case "nviPlay":
                    if (playerPage is null)
                    {
                        playerPage = new PlayerPage();
                    }
                    nextPage = playerPage;
                    break;
                case "nviStop":
                    if (playerPage != null)
                    {
                        await playerPage.StopAsync();
                    }
                    break;
            }
            if (args.IsSettingsSelected)
            {                
                nextPage = new SettingsPage();
            }
            if ((navigationView.Content as Page) != nextPage)
            {
                navigationView.Content = nextPage;
                if (nextPage == playerPage)
                {
                    await playerPage.PlayAsync();
                }
            }
        }
    }
}
