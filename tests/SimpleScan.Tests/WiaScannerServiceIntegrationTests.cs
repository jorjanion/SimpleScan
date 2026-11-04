using SimpleScan.Core.Scanning;

namespace SimpleScan.Tests;

public class WiaScannerServiceIntegrationTests
{
    [Fact]
    public async Task GetDevices_DetectsConnectedScanners()
    {
        var scannerService = new WiaScannerService();
        var devices = await scannerService.GetDevicesAsync();

        // The machine currently has the HP Smart Tank connected
        Assert.NotEmpty(devices);

        // Verify device has non-empty ID and Name
        foreach (var dev in devices)
        {
            Assert.False(string.IsNullOrWhiteSpace(dev.Id));
            Assert.False(string.IsNullOrWhiteSpace(dev.Name));
        }

        // Check if an HP device is identified
        bool foundHp = devices.Any(d => d.Name.Contains("HP", StringComparison.OrdinalIgnoreCase) ||
                                        d.Description.Contains("HP", StringComparison.OrdinalIgnoreCase));
        Assert.True(foundHp);
    }
}
