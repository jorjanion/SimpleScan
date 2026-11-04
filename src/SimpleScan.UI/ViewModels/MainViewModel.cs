using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using SimpleScan.Core.Configuration;
using SimpleScan.Core.Export;
using SimpleScan.Core.Models;
using SimpleScan.Core.Ocr;
using SimpleScan.Core.Scanning;

namespace SimpleScan.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IScannerService _scannerService;
    private readonly IOcrService _ocrService;
    private readonly IPdfExportService _pdfExportService;
    private readonly IImageExportService _imageExportService;
    private readonly SettingsService _settingsService;

    private ScanSettings _settings;
    private ScannerDevice? _selectedScanner;
    private PageViewModel? _selectedPage;
    private bool _isBusy;
    private string _statusMessage = "Buscando escáneres...";
    private bool _enableOcr = true;
    private int _selectedDpi = 300;
    private ScanColorMode _selectedColorMode = ScanColorMode.Color;
    private bool _showSettings;
    private bool _promptSaveLocation = true;
    private string _defaultSaveDirectory = string.Empty;

    public static ScanColorMode ColorModeColor => ScanColorMode.Color;
    public static ScanColorMode ColorModeGrayscale => ScanColorMode.Grayscale;
    public static ScanColorMode ColorModeBlackAndWhite => ScanColorMode.BlackAndWhite;

    public ObservableCollection<ScannerDevice> Scanners { get; } = new();
    public ObservableCollection<PageViewModel> Pages { get; } = new();

    public ScannerDevice? SelectedScanner
    {
        get => _selectedScanner;
        set
        {
            if (SetField(ref _selectedScanner, value))
            {
                if (value != null)
                {
                    _settings.SelectedDeviceId = value.Id;
                    _settingsService.SaveSettings(_settings);
                    StatusMessage = $"Escáner seleccionado: {value.Name}";
                }
            }
        }
    }

    public PageViewModel? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (_selectedPage != null) _selectedPage.IsSelected = false;
            if (SetField(ref _selectedPage, value))
            {
                if (_selectedPage != null) _selectedPage.IsSelected = true;
                OnPropertyChanged(nameof(HasSelectedPage));
            }
        }
    }

    public bool HasSelectedPage => SelectedPage != null;

    public bool HasPages => Pages.Count > 0;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanScan));
            }
        }
    }

    public bool CanScan => !IsBusy;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public bool EnableOcr
    {
        get => _enableOcr;
        set
        {
            if (SetField(ref _enableOcr, value))
            {
                _settings.EnableOcr = value;
                _settingsService.SaveSettings(_settings);
            }
        }
    }

    public int SelectedDpi
    {
        get => _selectedDpi;
        set
        {
            if (SetField(ref _selectedDpi, value))
            {
                _settings.Dpi = value;
                _settingsService.SaveSettings(_settings);
            }
        }
    }

    public ScanColorMode SelectedColorMode
    {
        get => _selectedColorMode;
        set
        {
            if (SetField(ref _selectedColorMode, value))
            {
                _settings.ColorMode = value;
                _settingsService.SaveSettings(_settings);
            }
        }
    }

    public bool ShowSettings
    {
        get => _showSettings;
        set => SetField(ref _showSettings, value);
    }

    public bool PromptSaveLocation
    {
        get => _promptSaveLocation;
        set
        {
            if (SetField(ref _promptSaveLocation, value))
            {
                _settings.PromptSaveLocation = value;
                _settingsService.SaveSettings(_settings);
            }
        }
    }

    public string DefaultSaveDirectory
    {
        get => _defaultSaveDirectory;
        set
        {
            if (SetField(ref _defaultSaveDirectory, value))
            {
                _settings.DefaultSaveDirectory = value;
                _settingsService.SaveSettings(_settings);
            }
        }
    }

    public MainViewModel(
        IScannerService scannerService,
        IOcrService ocrService,
        IPdfExportService pdfExportService,
        IImageExportService imageExportService,
        SettingsService settingsService)
    {
        _scannerService = scannerService;
        _ocrService = ocrService;
        _pdfExportService = pdfExportService;
        _imageExportService = imageExportService;
        _settingsService = settingsService;

        _settings = _settingsService.LoadSettings();
        _selectedDpi = _settings.Dpi;
        _selectedColorMode = _settings.ColorMode;
        _enableOcr = _settings.EnableOcr;
        _promptSaveLocation = _settings.PromptSaveLocation;
        _defaultSaveDirectory = _settings.DefaultSaveDirectory;

        Pages.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasPages));
            RenumberPages();
        };
    }

    public async Task InitializeAsync()
    {
        await RefreshScannersAsync();
    }

    public async Task RefreshScannersAsync()
    {
        IsBusy = true;
        StatusMessage = "Buscando escáneres disponibles...";

        try
        {
            var devices = await _scannerService.GetDevicesAsync();
            Scanners.Clear();

            foreach (var dev in devices)
            {
                Scanners.Add(dev);
            }

            if (Scanners.Count > 0)
            {
                // Prefer last saved device, or HP device, or first device
                var target = Scanners.FirstOrDefault(s => s.Id == _settings.SelectedDeviceId)
                             ?? Scanners.FirstOrDefault(s => s.IsDefault)
                             ?? Scanners[0];

                SelectedScanner = target;
                StatusMessage = $"Listo. Escáner activo: {target.Name}";
            }
            else
            {
                SelectedScanner = null;
                StatusMessage = "No se detectaron escáneres. Comprueba que tu impresora esté encendida y conectada.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error al buscar escáneres: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ScanPageAsync()
    {
        if (SelectedScanner == null)
        {
            StatusMessage = "Selecciona un escáner antes de continuar.";
            return;
        }

        IsBusy = true;
        var progress = new Progress<string>(msg => StatusMessage = msg);

        try
        {
            var settings = new ScanSettings
            {
                SelectedDeviceId = SelectedScanner.Id,
                Dpi = SelectedDpi,
                ColorMode = SelectedColorMode,
                EnableOcr = EnableOcr
            };

            byte[] imageBytes = await _scannerService.ScanPageAsync(
                SelectedScanner.Id,
                settings,
                progress);

            int pageNumber = Pages.Count + 1;
            var scannedPage = new ScannedPage(imageBytes, pageNumber, SelectedDpi);

            // Execute OCR if enabled
            if (EnableOcr)
            {
                StatusMessage = "Aplicando reconocimiento OCR en español...";
                try
                {
                    var ocr = await _ocrService.RecognizeTextAsync(imageBytes, "es-ES");
                    scannedPage.Ocr = ocr;
                }
                catch (Exception ocrEx)
                {
                    StatusMessage = "Aviso: OCR omitido (" + ocrEx.Message + ")";
                }
            }

            var vm = new PageViewModel(scannedPage, pageNumber);
            Pages.Add(vm);
            SelectedPage = vm;

            StatusMessage = $"Página {pageNumber} escaneada correctamente ({SelectedDpi} DPI).";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Escaneo cancelado.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error al escanear: " + ex.Message;
            MessageBox.Show(ex.Message, "Error al escanear", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void DeletePage(PageViewModel? page)
    {
        if (page == null) return;
        int idx = Pages.IndexOf(page);
        Pages.Remove(page);

        if (Pages.Count > 0)
        {
            SelectedPage = idx < Pages.Count ? Pages[idx] : Pages[^1];
        }
        else
        {
            SelectedPage = null;
        }
        StatusMessage = "Página eliminada.";
    }

    public void MovePageUp(PageViewModel? page)
    {
        if (page == null) return;
        int idx = Pages.IndexOf(page);
        if (idx > 0)
        {
            Pages.Move(idx, idx - 1);
            SelectedPage = page;
        }
    }

    public void MovePageDown(PageViewModel? page)
    {
        if (page == null) return;
        int idx = Pages.IndexOf(page);
        if (idx >= 0 && idx < Pages.Count - 1)
        {
            Pages.Move(idx, idx + 1);
            SelectedPage = page;
        }
    }

    public void ClearAllPages()
    {
        if (Pages.Count == 0) return;
        var result = MessageBox.Show(
            "¿Estás seguro de que quieres eliminar todas las páginas escaneadas?",
            "Vaciar documento",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Pages.Clear();
            SelectedPage = null;
            StatusMessage = "Todas las páginas han sido eliminadas.";
        }
    }

    public async Task SaveAsPdfAsync(Func<string, string?> pickSavePath)
    {
        if (Pages.Count == 0)
        {
            MessageBox.Show("Primero escanea al menos una página.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string defaultFileName = $"Escaneo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string? targetPath = null;

        if (PromptSaveLocation)
        {
            targetPath = pickSavePath(defaultFileName);
            if (string.IsNullOrEmpty(targetPath)) return;
        }
        else
        {
            if (string.IsNullOrEmpty(DefaultSaveDirectory))
            {
                DefaultSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escaneos");
            }
            if (!Directory.Exists(DefaultSaveDirectory))
            {
                Directory.CreateDirectory(DefaultSaveDirectory);
            }
            targetPath = Path.Combine(DefaultSaveDirectory, defaultFileName);
        }

        IsBusy = true;
        var progress = new Progress<string>(msg => StatusMessage = msg);

        try
        {
            var pageModels = Pages.Select(p => p.Model).ToList();
            await _pdfExportService.ExportToPdfAsync(
                pageModels,
                targetPath,
                embedOcrText: EnableOcr,
                progress);

            StatusMessage = $"PDF guardado con éxito: {Path.GetFileName(targetPath)}";
            MessageBox.Show(
                $"PDF guardado con éxito en:\n{targetPath}",
                "Guardado completado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = "Error al guardar PDF: " + ex.Message;
            MessageBox.Show(ex.Message, "Error al exportar PDF", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsImagesAsync(Func<string?> pickFolder)
    {
        if (Pages.Count == 0)
        {
            MessageBox.Show("Primero escanea al menos una página.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string? targetFolder = null;
        if (PromptSaveLocation)
        {
            targetFolder = pickFolder();
            if (string.IsNullOrEmpty(targetFolder)) return;
        }
        else
        {
            targetFolder = DefaultSaveDirectory;
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
        }

        IsBusy = true;
        var progress = new Progress<string>(msg => StatusMessage = msg);

        try
        {
            string baseName = $"Escaneo_{DateTime.Now:yyyyMMdd_HHmmss}";
            var pageModels = Pages.Select(p => p.Model).ToList();
            var exported = await _imageExportService.ExportImagesAsync(
                pageModels,
                targetFolder,
                baseName,
                ExportImageFormat.Jpeg,
                progress);

            StatusMessage = $"Se guardaron {exported.Count} imágenes en: {targetFolder}";
            MessageBox.Show(
                $"Se guardaron {exported.Count} imágenes correctamente en:\n{targetFolder}",
                "Imágenes guardadas",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = "Error al guardar imágenes: " + ex.Message;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void CopyOcrTextToClipboard()
    {
        if (SelectedPage?.Model.Ocr != null && SelectedPage.Model.Ocr.HasText)
        {
            Clipboard.SetText(SelectedPage.Model.Ocr.FullText);
            StatusMessage = "Texto de la página copiado al portapapeles.";
        }
        else if (Pages.Any(p => p.Model.Ocr != null && p.Model.Ocr.HasText))
        {
            string combined = string.Join("\n\n--- Página ---\n\n",
                Pages.Where(p => p.Model.Ocr != null && p.Model.Ocr.HasText)
                     .Select(p => p.Model.Ocr!.FullText));

            Clipboard.SetText(combined);
            StatusMessage = "Texto completo del documento copiado al portapapeles.";
        }
        else
        {
            MessageBox.Show("No hay texto OCR detectado en las páginas escaneadas.", "Sin texto", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RenumberPages()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            Pages[i].DisplayPageNumber = i + 1;
            Pages[i].Model.PageNumber = i + 1;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
