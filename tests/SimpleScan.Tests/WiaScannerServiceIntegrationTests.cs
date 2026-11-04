using SimpleScan.Core.Scanning;

namespace SimpleScan.Tests;

public class WiaScannerServiceIntegrationTests
{
    [Fact]
    public async Task GetDevices_ReturnsDeviceListWithoutThrowing()
    {
        var scannerService = new WiaScannerService();
        var devices = await scannerService.GetDevicesAsync();

        // Must always return a valid collection (never null)
        Assert.NotNull(devices);

        // If scanners are present (e.g. physical hardware connected), verify attributes
        foreach (var dev in devices)
        {
            Assert.False(string.IsNullOrWhiteSpace(dev.Id));
            Assert.False(string.IsNullOrWhiteSpace(dev.Name));
            Assert.False(string.IsNullOrWhiteSpace(dev.Manufacturer));
        }
    }
}
