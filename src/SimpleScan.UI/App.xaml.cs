using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SimpleScan.UI;

public partial class App : Application
{
    private static readonly string LogFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleScan",
        "app.log");

    public static void Log(string message)
    {
        try
        {
            string? dir = Path.GetDirectoryName(LogFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\r\n");
        }
        catch { }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log("=== SimpleScan Iniciando ===");

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        try
        {
            Log("Creando MainWindow...");
            var mainWindow = new MainWindow();
            Log("MainWindow creado con éxito. Mostrando ventana...");
            mainWindow.Show();
            Log("mainWindow.Show() completado.");
        }
        catch (Exception ex)
        {
            Log("FATAL al crear/mostrar MainWindow: " + ex);
            MessageBox.Show(
                $"No se pudo iniciar SimpleScan:\n\n{ex.Message}\n\nDetalles guardados en:\n{LogFile}",
                "SimpleScan - Error Crítico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log("DispatcherUnhandledException: " + e.Exception);
        MessageBox.Show(
            $"Error inesperado en SimpleScan:\n\n{e.Exception.Message}",
            "SimpleScan - Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log("CurrentDomain_UnhandledException: " + e.ExceptionObject);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log("TaskScheduler_UnobservedTaskException: " + e.Exception);
        e.SetObserved();
    }
}
