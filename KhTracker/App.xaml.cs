using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace KhTracker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static Log Logger { get; private set; }

    public static IConfiguration Config { get; private set; }

    internal static Settings Settings { get; private set; }

    private App()
    {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        try
        {
            Logger = new Log(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + "\\KhTracker\\log.txt"
            );
            Config = new ConfigurationBuilder()
                .AddJsonFile("./KH2ArchipelagoTrackerSettings/settings.json", true)
                .Build();
            Settings = Config.Get<Settings>() ?? new Settings();
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
        if (!Directory.Exists("KH2ArchipelagoTrackerAutoSaves"))
        {
            Directory.CreateDirectory("KH2ArchipelagoTrackerAutoSaves\\");
        }
        (MainWindow as MainWindow)!.Save(
            "KH2ArchipelagoTrackerAutoSaves\\"
                + "Tracker-CrashBackup_"
                + DateTime.Now.ToString("yy-MM-dd_H-m")
                + ".tsv"
        );
    }
}
