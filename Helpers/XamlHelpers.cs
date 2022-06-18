using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Interop;

namespace RadioParadisePlayer.Helpers
{
    static class XamlHelpers
    {
        public static Window GetWindow()
        {
            return (Application.Current as App).Window;
        }

        public static AppWindow GetAppWindowForWindow(Window window)
        {
            if (window == null) return null;
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
    }
}
