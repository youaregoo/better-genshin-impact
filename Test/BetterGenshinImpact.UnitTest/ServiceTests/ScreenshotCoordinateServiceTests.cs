using BetterGenshinImpact.Service;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace BetterGenshinImpact.UnitTest.ServiceTests;

/// <summary>
/// 截图坐标识别服务单元测试
/// </summary>
public class ScreenshotCoordinateServiceTests : IDisposable
{
    private readonly ScreenshotCoordinateService _service;
    private readonly FakeLogger<ScreenshotCoordinateService> _logger;

    public ScreenshotCoordinateServiceTests()
    {
        _logger = new FakeLogger<ScreenshotCoordinateService>();
        _service = new ScreenshotCoordinateService(_logger);
    }

    [Fact]
    public void IsValidCoordinate_ShouldReturnTrue_ForValidCoordinates()
    {
        // Arrange & Act & Assert
        Assert.True(_service.IsValidCoordinate(100, 200));
        Assert.True(_service.IsValidCoordinate(-100, -200));
        Assert.True(_service.IsValidCoordinate(0, 0));
        Assert.True(_service.IsValidCoordinate(2999, 2999));
        Assert.True(_service.IsValidCoordinate(-2999, -2999));
    }

    [Fact]
    public void IsValidCoordinate_ShouldReturnFalse_ForInvalidCoordinates()
    {
        // Arrange & Act & Assert
        Assert.False(_service.IsValidCoordinate(3001, 0));
        Assert.False(_service.IsValidCoordinate(0, 3001));
        Assert.False(_service.IsValidCoordinate(-3001, 0));
        Assert.False(_service.IsValidCoordinate(0, -3001));
        Assert.False(_service.IsValidCoordinate(5000, 5000));
    }

    [Fact]
    public void RecognizeCoordinatesFromScreenshot_ShouldReturnZero_ForNonExistentFile()
    {
        // Arrange
        string nonExistentPath = "non_existent_file.jpg";

        // Act
        var result = _service.RecognizeCoordinatesFromScreenshot(nonExistentPath);

        // Assert
        Assert.Equal(0, result.x);
        Assert.Equal(0, result.y);
    }

    [Fact]
    public void RecognizeCoordinatesFromScreenshot_ShouldReturnZero_ForEmptyPath()
    {
        // Arrange
        string emptyPath = "";

        // Act
        var result = _service.RecognizeCoordinatesFromScreenshot(emptyPath);

        // Assert
        Assert.Equal(0, result.x);
        Assert.Equal(0, result.y);
    }

    [Fact]
    public void RecognizeCoordinatesFromScreenshot_ShouldReturnZero_ForNullPath()
    {
        // Arrange
        string? nullPath = null;

        // Act
        var result = _service.RecognizeCoordinatesFromScreenshot(nullPath!);

        // Assert
        Assert.Equal(0, result.x);
        Assert.Equal(0, result.y);
    }

    public void Dispose()
    {
        _service?.Dispose();
        GC.SuppressFinalize(this);
    }
}