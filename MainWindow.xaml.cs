using CorexBrowser.Helpers;
using CorexBrowser.Models;
using CorexBrowser.Services;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;

using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CorexBrowser
{
    public partial class MainWindow : Window
    {
        

        private WindowState _restoreState;
        private bool _isHtmlFullscreen = false;

        private bool _isTrueFullscreen = false;




        private double _targetHorizontalOffset;
        private DispatcherTimer _scrollTimer;
        private bool _isLoading = false;
        


        private WindowState _prevState;
        private ResizeMode _prevResize;
        private WindowStyle _prevStyle;


       



        private bool IsDownloadUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.ToLower();

            return url.Contains("/download")
                || url.Contains(".rar")
                || url.Contains(".zip")
                || url.Contains(".exe")
                || url.Contains(".7z");
        }
        // Refresh / Stop ikon animasyonu için
        private void AnimateRefreshIcon()
        {
            if (RefreshStopIcon == null)
                return;

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(80)
            };

            fadeOut.Completed += (_, _) =>
            {
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(120)
                };

                RefreshStopIcon.BeginAnimation(OpacityProperty, fadeIn);
            };

            RefreshStopIcon.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            _isLoading = true;

            RefreshStopIcon.Data = (Geometry)FindResource("IconStop");
            AnimateRefreshIcon();
        }


        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isLoading = false;

            RefreshStopIcon.Data = (Geometry)FindResource("IconRefresh");
            AnimateRefreshIcon();
        }


        public ICommand NewTabCommand => new RelayCommand(_ => CreateNewTab());

        public ObservableCollection<BrowserTab> Tabs { get; } = new();
        private BrowserTab? ActiveTab;

        public class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            public RelayCommand(Action<object?> execute) => _execute = execute;

            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _execute(parameter);
        }
        private WindowStyle _restoreStyle;
        private ResizeMode _restoreResize;

        public MainWindow()
        {
            _scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(4) // ne kadar küçük = o kadar akıcı
            };
            _scrollTimer.Tick += SmoothScrollTick;
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape && _isHtmlFullscreen)
                {
                    ExitHtmlFullscreen();
                    e.Handled = true;
                }
            };


            // 🔴 KRİTİK: Ayarları açılışta yükle
            SettingsService.Load();

            InitializeComponent();
            DataContext = this;
            SourceInitialized += Window_SourceInitialized;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

        }
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isTrueFullscreen)
            {
                e.Handled = true;
            }
        }

        // 🔹 Açılış ve yeni sekmeler için
        private string GetStartupUrl()
        {
            return SettingsService.Current.SearchEngine switch
            {
                "bing" => "https://www.bing.com",
                "duckduckgo" => "https://duckduckgo.com",
                _ => "https://www.google.com"
            };
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
        private void TabScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer sv) return;

            // Chrome benzeri hız (küçült → daha yumuşak)
            _targetHorizontalOffset -= e.Delta * 0.6;

            // Limitleri aşma
            _targetHorizontalOffset = Math.Max(0,
                Math.Min(_targetHorizontalOffset, sv.ScrollableWidth));

            if (!_scrollTimer.IsEnabled)
                _scrollTimer.Start();

            e.Handled = true;
        }
        private void SmoothScrollTick(object? sender, EventArgs e)
        {
            double current = TabScrollViewer.HorizontalOffset;
            double delta = (_targetHorizontalOffset - current) * 0.18; // easing

            if (Math.Abs(delta) < 0.5)
            {
                TabScrollViewer.ScrollToHorizontalOffset(_targetHorizontalOffset);
                _scrollTimer.Stop();
                return;
            }

            TabScrollViewer.ScrollToHorizontalOffset(current + delta);
        }



        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        // 🔹 Arama motoruna göre search URL
        private string BuildSearchUrl(string query)
        {
            return SettingsService.Current.SearchEngine switch
            {
                "bing" => $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}",
                "duckduckgo" => $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}",
                _ => $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"
            };
        }

        private void OpenSettings()
        {
            SettingsOverlay.Visibility = Visibility.Visible;

            var settings = new SettingsWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            settings.ShowDialog();

            SettingsOverlay.Visibility = Visibility.Collapsed;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //WindowThemeHelper.EnableDarkTitleBar(this);

            // 🔴 PROGRAM AÇILINCA İLK SEKME
            CreateNewTab();
        }

        private async void CreateNewTab(string? url = null)
        {
            var webView = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var tab = new BrowserTab
            {
                WebView = webView
            };

            Tabs.Add(tab);
            SwitchTab(tab);

            var userDataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CorexBrowser");

            var options = new CoreWebView2EnvironmentOptions(
     "--disable-features=TranslateUI " +
     "--enable-gpu-rasterization " +
     "--enable-zero-copy " +
     "--enable-accelerated-video-decode " +
     "--ignore-gpu-blocklist"
 );

            var env = await CoreWebView2Environment.CreateAsync(
                null,
                userDataPath,
                options
            );

            await webView.EnsureCoreWebView2Async(env);
            webView.CoreWebView2.ContainsFullScreenElementChanged += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (webView.CoreWebView2.ContainsFullScreenElement)
                        EnterHtmlFullscreen();
                    else
                        ExitHtmlFullscreen();
                });
            };





            // 🔹 Refresh / Stop için navigation event’leri
            webView.NavigationStarting += WebView_NavigationStarting;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            webView.CoreWebView2.NewWindowRequested += (s, e) =>
            {
                e.Handled = true;

                // SADECE normal linkler yeni sekme olur
                if (!IsDownloadUrl(e.Uri))
                {
                    Dispatcher.Invoke(() =>
                    {
                        CreateNewTab(e.Uri);
                    });
                }
                // Download URL'lere HİÇ dokunma
            };



            webView.CoreWebView2.DownloadStarting += (s, e) =>
            {
                var download = e.DownloadOperation;

                var item = new DownloadItem
                {
                    FileName = System.IO.Path.GetFileName(download.ResultFilePath),
                    Url = download.Uri,
                    TotalBytes = (long)(download.TotalBytesToReceive ?? 0),
                    ReceivedBytes = 0,
                    Operation = download
                };

                DownloadService.Add(item);

                download.BytesReceivedChanged += (_, _) =>
                {
                    item.ReceivedBytes = (long)download.BytesReceived;
                };
            };



            webView.NavigationCompleted += (_, e) =>
            {
                if (!e.IsSuccess)
                    return;

                var currentUrl = webView.Source?.ToString();
                if (string.IsNullOrWhiteSpace(currentUrl))
                    return;

                HistoryService.Add(
                    currentUrl,
                    webView.CoreWebView2.DocumentTitle,
                    tab.Favicon?.ToString()
                );
            };

            // 🔴 EN ÖNEMLİ SATIR
            webView.Source = new Uri(
                string.IsNullOrWhiteSpace(url)
                    ? GetStartupUrl()
                    : url);

            webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                tab.Title = webView.CoreWebView2.DocumentTitle;
            };

            webView.CoreWebView2.SourceChanged += (_, _) =>
            {
                if (ActiveTab == tab)
                    AddressBar.Text = webView.Source?.ToString();
            };

            webView.CoreWebView2.FaviconChanged += async (_, _) =>
            {
                try
                {
                    var stream = await webView.CoreWebView2.GetFaviconAsync(
                        CoreWebView2FaviconImageFormat.Png);

                    if (stream != null)
                    {
                        var image = new System.Windows.Media.Imaging.BitmapImage();
                        image.BeginInit();
                        image.StreamSource = stream;
                        image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        image.EndInit();
                        image.Freeze();

                        tab.Favicon = image;
                    }
                }
                catch
                {
                    // favicon yoksa sorun değil
                }
            };
        }
        private void EnterHtmlFullscreen()
        {
            if (_isHtmlFullscreen)
                return;

            _isHtmlFullscreen = true;

            _restoreState = WindowState;
            _restoreStyle = WindowStyle;
            _restoreResize = ResizeMode;
            // UI gizle
            TabBarRow.Height = new GridLength(0);
            ToolbarRow.Height = new GridLength(0);
            WindowState = WindowState.Normal; // 🔴 KRİTİK
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;


            // 🔴 EN KRİTİK SATIR
            WindowState = WindowState.Maximized;
        }



        private void ExitHtmlFullscreen()
        {
            if (!_isHtmlFullscreen)
                return;

            _isHtmlFullscreen = false;

            WindowStyle = _restoreStyle;
            ResizeMode = _restoreResize;
            WindowState = _restoreState;
            InvalidateVisual();
            UpdateLayout();

            TabBarRow.Height = new GridLength(45);
            ToolbarRow.Height = new GridLength(52);
        }








        private void DownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            // Chromium'un kendi indirme sayfası
            CreateNewTab("edge://downloads");
        }

        private void SwitchTab(BrowserTab tab)
        {
            foreach (var t in Tabs)
                t.IsActive = false;

            BrowserHost.Children.Clear();

            ActiveTab = tab;
            tab.IsActive = true;

            BrowserHost.Children.Add(tab.WebView);
            AddressBar.Text = tab.WebView.Source?.ToString();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || ActiveTab?.WebView?.CoreWebView2 == null)
                return;

            var text = AddressBar.Text.Trim();

            bool isUrl =
                text.StartsWith("http://") ||
                text.StartsWith("https://") ||
                text.Contains(".");

            if (isUrl)
            {
                if (!text.StartsWith("http"))
                    text = "https://" + text;

                ActiveTab.WebView.CoreWebView2.Navigate(text);
            }
            else
            {
                var searchUrl = BuildSearchUrl(text);
                ActiveTab.WebView.CoreWebView2.Navigate(searchUrl);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTab?.WebView.CoreWebView2.CanGoBack == true)
                ActiveTab.WebView.CoreWebView2.GoBack();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTab?.WebView.CoreWebView2.CanGoForward == true)
                ActiveTab.WebView.CoreWebView2.GoForward();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ActiveTab?.WebView.CoreWebView2.Reload();
        }

        private void RefreshStop_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTab?.WebView?.CoreWebView2 == null)
                return;

            if (_isLoading)
            {
                ActiveTab.WebView.CoreWebView2.Stop();
            }
            else
            {
                ActiveTab.WebView.CoreWebView2.Reload();
            }
        }

        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BrowserTab tab)
                SwitchTab(tab);
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTab();
        }

        private void TabClose_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BrowserTab tab)
            {
                var wasActive = tab == ActiveTab;

                try
                {
                    tab.WebView.CoreWebView2?.Stop();
                    tab.Favicon = null;
                    tab.WebView.Dispose();
                }
                catch { }

                Tabs.Remove(tab);

                if (wasActive)
                {
                    if (Tabs.Count > 0)
                        SwitchTab(Tabs[0]);
                    else
                        CreateNewTab();
                }
            }
        }
        private void HistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is HistoryItem item)
            {
                CreateNewTab(item.Url);
                HistoryButton.IsChecked = false;
            }
        }
        private void HistoryPopup_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Popup popup &&
                popup.Child is Border border)
            {
                var storyboard = (Storyboard)FindResource("HistoryPopupOpen");
                storyboard.Begin(border);
            }
        }
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryService.Clear();
        }
        private void PauseDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is DownloadItem item &&
                item.Operation.State == CoreWebView2DownloadState.InProgress)
            {
                item.Operation.Pause();
            }
        }
        private void ResumeDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is DownloadItem item &&
                item.Operation.State == CoreWebView2DownloadState.Interrupted)
            {
                item.Operation.Resume();
            }
        }
        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is DownloadItem item)
            {
                item.Operation.Cancel();
                DownloadService.Items.Remove(item);
            }
        }
        private void OpenDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is DownloadItem item)
            {
                var path = item.Operation.ResultFilePath;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
            }
        }
        private void DownloadsPopup_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.Popup popup &&
                popup.Child is Border border)
            {
                var storyboard = (Storyboard)FindResource("HistoryPopupOpen");
                storyboard.Begin(border);
            }
        }

    }
}
