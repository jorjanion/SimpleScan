using SimpleScan.Core.Models;

namespace SimpleScan.Core.Scanning;

public interface IScannerService
{
    Task<IReadOnlyList<ScannerDevice>> GetDevicesAsync();

    Task<byte[]> ScanPageAsync(
        string? deviceId,
        ScanSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
