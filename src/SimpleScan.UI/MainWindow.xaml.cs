using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SimpleScan.Core.Configuration;
using SimpleScan.Core.Export;
using SimpleScan.Core.Ocr;
using SimpleScan.Core.Scanning;
using SimpleScan.UI.ViewModels;

namespace SimpleScan.UI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var settingsService = new SettingsService();
        var scannerService = new WiaScannerService();
        var ocrService = new WindowsOcrService();
        var pdfExportService = new PdfExportService();
        var imageExportService = new ImageExportService();

        _viewModel = new MainViewModel(
            scannerService,
            ocrService,
            pdfExportService,
            imageExportService,
            settingsService);

        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void OnRefreshScannersClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.RefreshScannersAsync();
    }

    private async void OnScanPageClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.ScanPageAsync();
    }

    private void OnClearAllPagesClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearAllPages();
    }

    private async void OnSavePdfClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveAsPdfAsync(defaultName =>
        {
            var dialog = new SaveFileDialog
            {
                Title = "Guardar documento PDF",
                Filter = "Documentos PDF (*.pdf)|*.pdf",
                FileName = defaultName,
                DefaultExt = ".pdf",
                AddExtension = true
            };

            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        });
    }

    private async void OnSaveImagesClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveAsImagesAsync(() =>
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Selecciona la carpeta donde guardar las imágenes escaneadas",
                Multiselect = false
            };

            return dialog.ShowDialog(this) == true ? dialog.FolderName : null;
        });
    }

    private void OnCopyTextClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CopyOcrTextToClipboard();
    }

    private void OnRotateLeftClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedPage?.RotateCounterClockwise();
    }

    private void OnRotateRightClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedPage?.RotateClockwise();
    }

    private void OnDeleteCurrentPageClick(object sender, RoutedEventArgs e)
    {
        _viewModel.DeletePage(_viewModel.SelectedPage);
    }

    private void OnThumbnailClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PageViewModel page })
        {
            _viewModel.SelectedPage = page;
        }
    }

    private void OnToggleSettingsClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowSettings = !_viewModel.ShowSettings;
    }

    private void OnBrowseDefaultFolderClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Seleccionar carpeta predeterminada de escaneos"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.DefaultSaveDirectory = dialog.FolderName;
        }
    }
}
