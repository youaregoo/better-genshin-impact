using System;
using System.IO;
using BetterGenshinImpact.Core.Recognition.OpenCv;
using BetterGenshinImpact.GameTask.Common.Map;
using BetterGenshinImpact.GameTask.Common.Map.Maps;
using OpenCvSharp;
using Microsoft.Extensions.Logging;

namespace BetterGenshinImpact.Service;

/// <summary>
/// 截图坐标识别服务
/// 用于从游戏截图中识别对应的游戏坐标
/// </summary>
public class ScreenshotCoordinateService : IDisposable
{
    private readonly ILogger<ScreenshotCoordinateService> _logger;
    private readonly TeyvatMap _teyvatMap;

    public ScreenshotCoordinateService(ILogger<ScreenshotCoordinateService> logger)
    {
        _logger = logger;
        _teyvatMap = new TeyvatMap();
    }

    /// <summary>
    /// 从截图文件识别坐标
    /// </summary>
    /// <param name="screenshotPath">截图文件路径</param>
    /// <returns>识别到的坐标 (x, y)，如果识别失败返回 (0, 0)</returns>
    public (float x, float y) RecognizeCoordinatesFromScreenshot(string screenshotPath)
    {
        try
        {
            if (!File.Exists(screenshotPath))
            {
                _logger.LogError("截图文件不存在: {Path}", screenshotPath);
                return (0, 0);
            }

            _logger.LogInformation("开始处理截图文件: {Path}", screenshotPath);

            using var screenshot = Cv2.ImRead(screenshotPath, ImreadModes.Color);
            if (screenshot.Empty())
            {
                _logger.LogError("无法读取截图文件: {Path}", screenshotPath);
                return (0, 0);
            }

            return RecognizeCoordinatesFromImage(screenshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理截图时发生错误: {Path}", screenshotPath);
            return (0, 0);
        }
    }

    /// <summary>
    /// 从图像识别坐标
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <returns>识别到的坐标 (x, y)，如果识别失败返回 (0, 0)</returns>
    public (float x, float y) RecognizeCoordinatesFromImage(Mat image)
    {
        try
        {
            // 1. 预处理图像 - 提取可能的地图区域
            var mapRegion = ExtractMapRegionFromScreenshot(image);
            if (mapRegion == null || mapRegion.Empty())
            {
                _logger.LogWarning("无法从截图中提取地图区域");
                return (0, 0);
            }

            // 2. 转换为灰度图
            using var greyMap = new Mat();
            Cv2.CvtColor(mapRegion, greyMap, ColorConversionCodes.BGR2GRAY);

            // 3. 使用现有的地图匹配系统识别坐标
            var imageCoords = _teyvatMap.GetBigMapPosition(greyMap);
            
            if (imageCoords.X == 0 && imageCoords.Y == 0)
            {
                _logger.LogWarning("地图匹配失败，无法识别坐标");
                return (0, 0);
            }

            // 4. 转换为游戏坐标系
            var gameCoords = _teyvatMap.ConvertImageCoordinatesToGenshinMapCoordinates(imageCoords);
            
            _logger.LogInformation("识别成功 - 图像坐标: ({X}, {Y}), 游戏坐标: ({GameX}, {GameY})", 
                imageCoords.X, imageCoords.Y, gameCoords.X, gameCoords.Y);

            return (gameCoords.X, gameCoords.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图像坐标识别过程中发生错误");
            return (0, 0);
        }
        finally
        {
            mapRegion?.Dispose();
        }
    }

    /// <summary>
    /// 从截图中提取地图区域
    /// 这个方法尝试自动检测和提取截图中的地图部分
    /// </summary>
    /// <param name="screenshot">原始截图</param>
    /// <returns>提取的地图区域图像</returns>
    private Mat? ExtractMapRegionFromScreenshot(Mat screenshot)
    {
        try
        {
            // 简单策略：假设整个截图就是地图
            // 在实际应用中，这里可以加入更复杂的地图区域检测逻辑
            
            // 如果截图过大，进行适当缩放以提高处理速度
            var maxSize = 1920;
            if (screenshot.Width > maxSize || screenshot.Height > maxSize)
            {
                var scale = Math.Min((double)maxSize / screenshot.Width, (double)maxSize / screenshot.Height);
                return ResizeHelper.Resize(screenshot, scale);
            }

            return screenshot.Clone();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提取地图区域时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 验证坐标是否合理
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>坐标是否合理</returns>
    public bool IsValidCoordinate(float x, float y)
    {
        // 原神提瓦特大陆的大致坐标范围
        // 这个范围可以根据实际需要调整
        return x >= -3000 && x <= 3000 && y >= -3000 && y <= 3000;
    }

    public void Dispose()
    {
        _teyvatMap?.Dispose();
        GC.SuppressFinalize(this);
    }
}