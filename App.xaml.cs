using CorexBrowser.Services;
using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SettingsService.Load();
        HistoryService.Load();
        DownloadService.Load();

    }
}
