using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class WindowBlurHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_BLURBEHIND = 3
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(
        IntPtr hwnd,
        ref WindowCompositionAttributeData data);

    public static void EnableLightBlur(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;

        var accent = new AccentPolicy
        {
            AccentState = (int)AccentState.ACCENT_ENABLE_BLURBEHIND,
            GradientColor = 0x00 // renk karıştırma yok
        };

        int size = Marshal.SizeOf(accent);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(accent, ptr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = 19,
            SizeOfData = size,
            Data = ptr
        };

        SetWindowCompositionAttribute(hwnd, ref data);
        Marshal.FreeHGlobal(ptr);
    }
    public static void DisableBlur(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;

        var accent = new AccentPolicy
        {
            AccentState = 0 // ACCENT_DISABLED
        };

        int size = Marshal.SizeOf(accent);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(accent, ptr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = 19,
            SizeOfData = size,
            Data = ptr
        };

        SetWindowCompositionAttribute(hwnd, ref data);
        Marshal.FreeHGlobal(ptr);
    }

}
