using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CorexBrowser.Helpers
{
    public static class WindowThemeHelper
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        public static void EnableDarkTitleBar(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                return;

            int useDarkMode = 1;
            DwmSetWindowAttribute(
                hwnd,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref useDarkMode,
                sizeof(int));
        }
    }
}
