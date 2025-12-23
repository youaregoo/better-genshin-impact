using BetterGenshinImpact.ViewModel.Pages;
using System.Windows.Controls;

namespace BetterGenshinImpact.View.Pages;

/// <summary>
/// ScreenshotCoordinatePage.xaml 的交互逻辑
/// </summary>
public partial class ScreenshotCoordinatePage : UserControl
{
    public ScreenshotCoordinateViewModel ViewModel { get; }

    public ScreenshotCoordinatePage(ScreenshotCoordinateViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}