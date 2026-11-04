using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SimpleScan.Core.Models;
using SimpleScan.Core.Processing;

namespace SimpleScan.UI.ViewModels;

public class PageViewModel : INotifyPropertyChanged
{
    private readonly ScannedPage _model;
    private BitmapSource? _previewImage;
    private bool _isSelected;
    private int _displayPageNumber;

    public ScannedPage Model => _model;

    public Guid Id => _model.Id;

    public int DisplayPageNumber
    {
        get => _displayPageNumber;
        set => SetField(ref _displayPageNumber, value);
    }

    public int RotationDegrees => _model.RotationDegrees;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public BitmapSource? PreviewImage
    {
        get => _previewImage;
        private set => SetField(ref _previewImage, value);
    }

    public bool HasOcr => _model.Ocr != null && _model.Ocr.HasText;

    public string OcrSummary => _model.Ocr?.FullText ?? string.Empty;

    public PageViewModel(ScannedPage model, int displayPageNumber)
    {
        _model = model;
        _displayPageNumber = displayPageNumber;
        LoadPreview();
    }

    public async void RotateClockwise()
    {
        _model.RotateClockwise();
        await RefreshImageAsync();
        OnPropertyChanged(nameof(RotationDegrees));
    }

    public async void RotateCounterClockwise()
    {
        _model.RotateCounterClockwise();
        await RefreshImageAsync();
        OnPropertyChanged(nameof(RotationDegrees));
    }

    public async Task RefreshImageAsync()
    {
        byte[] processed = await ImageProcessingHelper.RotateImageAsync(_model.RawImageBytes, _model.RotationDegrees, "png");
        PreviewImage = LoadBitmapFromBytes(processed);
    }

    private void LoadPreview()
    {
        PreviewImage = LoadBitmapFromBytes(_model.RawImageBytes);
    }

    private static BitmapImage LoadBitmapFromBytes(byte[] bytes)
    {
        var bitmap = new BitmapImage();
        using var stream = new MemoryStream(bytes);
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
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
