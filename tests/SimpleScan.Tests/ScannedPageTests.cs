using SimpleScan.Core.Models;

namespace SimpleScan.Tests;

public class ScannedPageTests
{
    [Fact]
    public void PageRotation_CyclesCorrectly()
    {
        var page = new ScannedPage(new byte[] { 1, 2, 3 });
        Assert.Equal(0, page.RotationDegrees);

        page.RotateClockwise();
        Assert.Equal(90, page.RotationDegrees);

        page.RotateClockwise();
        Assert.Equal(180, page.RotationDegrees);

        page.RotateClockwise();
        Assert.Equal(270, page.RotationDegrees);

        page.RotateClockwise();
        Assert.Equal(0, page.RotationDegrees);

        page.RotateCounterClockwise();
        Assert.Equal(270, page.RotationDegrees);

        page.ResetRotation();
        Assert.Equal(0, page.RotationDegrees);
    }
}
