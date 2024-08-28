using System;
using ArchiveMaster.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;

namespace ArchiveMaster.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, MainView view)
    {
        DataContext = viewModel;
        InitializeComponent();
        Content = view;
        if (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major == 10)
        {
            //SetWin10TitleBar();
        }
    }

    private void SetWin10TitleBar()
    {
        Grid grid = new Grid()
        {
            RowDefinitions =
            [
                new RowDefinition(1, GridUnitType.Auto),
                new RowDefinition(1, GridUnitType.Star)
            ],
        };

        // grid.Bind(Grid.BackgroundProperty, Resources.GetResourceObservable("Background0"));

        var content = Content as Control;
        Content = null;
        Grid.SetRow(content, 1);
        grid.Children.Add(content);
        grid.Children.Add(new FzLib.Avalonia.Controls.WindowButtons()
        {
            HorizontalAlignment = HorizontalAlignment.Right
        });

        Content = grid;

        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
    }
}