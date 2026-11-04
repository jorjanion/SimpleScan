namespace SimpleScan.Core.Models;

public record ScannerDevice(
    string Id,
    string Name,
    string Manufacturer,
    string Description,
    bool IsDefault = false
)
{
    public override string ToString() => Name;
}
