using System;
using System.Windows;

namespace KhTracker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static Log Logger;

    private App()
    {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        try
        {
            Logger = new Log(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + "\\KhTracker\\log.txt"
            );
        }
        catch { }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        Logger?.Close();
    }

    private void OnDispatcherUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e
    )
    {
        (MainWindow as MainWindow)!.Save(
            "KhTrackerAutoSaves\\"
                + "Tracker-CrashBackup_"
                + DateTime.Now.ToString("yy-MM-dd_H-m")
                + ".tsv"
        );
    }
}
