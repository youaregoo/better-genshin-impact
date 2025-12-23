using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.Helpers.Ui;
using BetterGenshinImpact.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace BetterGenshinImpact.ViewModel.Pages;

/// <summary>
/// 截图坐标识别页面ViewModel
/// </summary>
public partial class ScreenshotCoordinateViewModel : ObservableObject, IDisposable
{
    private readonly ILogger<ScreenshotCoordinateViewModel> _logger = App.GetLogger<ScreenshotCoordinateViewModel>();
    private readonly ScreenshotCoordinateService _coordinateService;

    [ObservableProperty]
    private string _selectedImagePath = string.Empty;

    [ObservableProperty]
    private BitmapImage? _previewImage;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private string _recognitionResult = "请选择一张游戏截图进行坐标识别";

    [ObservableProperty]
    private float _recognizedX = 0;

    [ObservableProperty]
    private float _recognizedY = 0;

    [ObservableProperty]
    private bool _hasValidResult = false;

    public AllConfig Config { get; set; }

    public ScreenshotCoordinateViewModel()
    {
        Config = TaskContext.Instance().Config;
        _coordinateService = new ScreenshotCoordinateService(_logger);
    }

    /// <summary>
    /// 选择截图文件
    /// </summary>
    [RelayCommand]
    private async Task SelectImageAsync()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择游戏截图",
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;
                await LoadPreviewImageAsync();
                RecognitionResult = "图片已加载，点击\"识别坐标\"开始处理";
                HasValidResult = false;
                RecognizedX = 0;
                RecognizedY = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "选择图片文件时发生错误");
            Toast.Error("选择图片文件失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 加载预览图片
    /// </summary>
    private async Task LoadPreviewImageAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
            {
                PreviewImage = null;
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(SelectedImagePath);
                    bitmap.DecodePixelWidth = 800; // 限制预览图片大小
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        PreviewImage = bitmap;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载预览图片失败: {Path}", SelectedImagePath);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        PreviewImage = null;
                        Toast.Error("加载图片预览失败：" + ex.Message);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载预览图片过程中发生错误");
            PreviewImage = null;
        }
    }

    /// <summary>
    /// 识别坐标
    /// </summary>
    [RelayCommand]
    private async Task RecognizeCoordinatesAsync()
    {
        if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
        {
            Toast.Warning("请先选择一张有效的图片文件");
            return;
        }

        IsProcessing = true;
        RecognitionResult = "正在识别坐标，请稍候...";
        HasValidResult = false;

        try
        {
            var result = await Task.Run(() => _coordinateService.RecognizeCoordinatesFromScreenshot(SelectedImagePath));
            
            RecognizedX = result.x;
            RecognizedY = result.y;

            if (_coordinateService.IsValidCoordinate(result.x, result.y))
            {
                RecognitionResult = $"识别成功！坐标：({result.x:F1}, {result.y:F1})";
                HasValidResult = true;
                Toast.Success("坐标识别成功！");
            }
            else
            {
                RecognitionResult = "识别失败，请检查图片是否为有效的游戏截图";
                HasValidResult = false;
                Toast.Warning("坐标识别失败，请尝试其他截图");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "识别坐标时发生错误");
            RecognitionResult = "识别过程中发生错误：" + ex.Message;
            HasValidResult = false;
            Toast.Error("识别失败：" + ex.Message);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// 复制坐标到剪贴板
    /// </summary>
    [RelayCommand]
    private void CopyCoordinates()
    {
        if (!HasValidResult)
        {
            Toast.Warning("没有有效的坐标可以复制");
            return;
        }

        try
        {
            var coordinateText = $"{RecognizedX:F1},{RecognizedY:F1}";
            System.Windows.Clipboard.SetText(coordinateText);
            Toast.Success($"坐标已复制到剪贴板：{coordinateText}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制坐标到剪贴板时发生错误");
            Toast.Error("复制失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 清除结果
    /// </summary>
    [RelayCommand]
    private void ClearResult()
    {
        SelectedImagePath = string.Empty;
        PreviewImage = null;
        RecognitionResult = "请选择一张游戏截图进行坐标识别";
        RecognizedX = 0;
        RecognizedY = 0;
        HasValidResult = false;
    }

    public void Dispose()
    {
        _coordinateService?.Dispose();
        GC.SuppressFinalize(this);
    }
}