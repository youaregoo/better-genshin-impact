using BetterGenshinImpact.Service;
using Microsoft.Extensions.Logging;
using System;

namespace BetterGenshinImpact.Examples;

/// <summary>
/// 截图坐标识别功能使用示例
/// </summary>
public class ScreenshotCoordinateExample
{
    /// <summary>
    /// 基本使用示例
    /// </summary>
    public static void BasicUsageExample()
    {
        // 创建服务实例（在实际应用中通过依赖注入获取）
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ScreenshotCoordinateService>.Instance;
        using var service = new ScreenshotCoordinateService(logger);

        // 识别截图坐标
        string screenshotPath = @"C:\Screenshots\genshin_map.png";
        var (x, y) = service.RecognizeCoordinatesFromScreenshot(screenshotPath);

        if (service.IsValidCoordinate(x, y))
        {
            Console.WriteLine($"识别成功！坐标：({x:F1}, {y:F1})");
            
            // 可以将坐标用于其他功能，比如传送、路径规划等
            Console.WriteLine($"格式化坐标：{x:F1},{y:F1}");
        }
        else
        {
            Console.WriteLine("坐标识别失败，请检查截图是否为有效的游戏地图");
        }
    }

    /// <summary>
    /// 批量处理示例
    /// </summary>
    public static void BatchProcessExample()
    {
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ScreenshotCoordinateService>.Instance;
        using var service = new ScreenshotCoordinateService(logger);

        string[] screenshots = {
            @"C:\Screenshots\location1.png",
            @"C:\Screenshots\location2.png",
            @"C:\Screenshots\location3.png"
        };

        Console.WriteLine("开始批量处理截图...");
        
        foreach (var screenshot in screenshots)
        {
            try
            {
                var (x, y) = service.RecognizeCoordinatesFromScreenshot(screenshot);
                
                if (service.IsValidCoordinate(x, y))
                {
                    Console.WriteLine($"{screenshot}: ({x:F1}, {y:F1})");
                }
                else
                {
                    Console.WriteLine($"{screenshot}: 识别失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{screenshot}: 处理错误 - {ex.Message}");
            }
        }
        
        Console.WriteLine("批量处理完成");
    }

    /// <summary>
    /// 坐标验证示例
    /// </summary>
    public static void CoordinateValidationExample()
    {
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ScreenshotCoordinateService>.Instance;
        using var service = new ScreenshotCoordinateService(logger);

        // 测试一些坐标
        (float x, float y)[] testCoordinates = {
            (100, 200),     // 有效坐标
            (-100, -200),   // 有效坐标
            (3500, 0),      // 无效坐标（超出范围）
            (0, 3500),      // 无效坐标（超出范围）
            (0, 0)          // 边界坐标
        };

        Console.WriteLine("坐标验证测试：");
        foreach (var (x, y) in testCoordinates)
        {
            bool isValid = service.IsValidCoordinate(x, y);
            Console.WriteLine($"({x}, {y}): {(isValid ? "有效" : "无效")}");
        }
    }
}