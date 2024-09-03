using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Converters;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using FzLib.Avalonia.Converters;
using Serilog;

namespace ArchiveMaster.Views;

public class TreeFileDataGrid : SimpleFileDataGrid
{
    protected override Type StyleKeyOverride => typeof(TreeFileDataGrid);

    public static readonly StyledProperty<int> RootDepthProperty
        = AvaloniaProperty.Register<TreeFileDataGrid, int>(nameof(RootDepth), 1);

    public int RootDepth
    {
        get => GetValue(RootDepthProperty);
        set => SetValue(RootDepthProperty, value);
    }

    public TreeFileDataGrid()
    {
        DoubleTapped += DataGridDoubleTapped;
    }

    public override double ColumnPathIndex => -1;

    private void DataGridDoubleTapped(object sender, TappedEventArgs e)
    {
        if (e.Source is Visual { DataContext: TreeDirInfo dir })
        {
            if (ItemsSource is not BulkObservableCollection<SimpleFileInfo> items)
            {
                return;
            }

            if (!dir.IsExpanded)
            {
                items.InsertRange(items.IndexOf(dir) + 1, dir.Subs);
                dir.IsExpanded = true;
            }
            else
            {
                items.RemoveRange(items.IndexOf(dir) + 1, dir.Subs.Count);
                dir.IsExpanded = false;
            }
        }
        else
        {
            if (e.Source is Visual { DataContext: TreeFileInfo file })
            {
                try
                {
                    Process.Start(new ProcessStartInfo(file.Path)
                    {
                        UseShellExecute = true
                    });
                }
                catch(Exception ex)
                {
                    Log.Error(ex,"打开文件失败");
                }
            }

        }
    }

    protected override DataGridColumn GetNameColumn()
    {
        var column = new DataGridTemplateColumn
        {
            Header = ColumnNameHeader
        };
        var cellTemplate = new FuncDataTemplate<TreeFileDirInfo>((value, namescope) =>
        {
            int halfWidth = 16;
            int fullWidth = halfWidth * 2;
            int halfHeight = 16;
            int height = halfHeight * 2;
            var lines = new Canvas()
            {
                Width = fullWidth * (value.Depth - RootDepth),
                Height = height,
            };

            List<(double x1, double y1, double x2, double y2)> linePoints =
                new List<(double x1, double y1, double x2, double y2)>();

            if (value.Depth > RootDepth) //根目录不需要
            {
                //首先处理最右侧
                int i = value.Depth - RootDepth;

                if (value.IsLast()) //如果是最后一个，那么是半高的竖线和半宽的横线
                {
                    linePoints.Add((fullWidth * i - halfWidth, 0, fullWidth * i - halfWidth, halfHeight));
                }
                else //否则，是全高的竖线和半宽的横线
                {
                    linePoints.Add((fullWidth * i - halfWidth, 0, fullWidth * i - halfWidth, height));
                }

                linePoints.Add((fullWidth * i - halfWidth, halfHeight, fullWidth * i, halfHeight));

                var parent = value.Parent; //取当前项的父级
                //从右往左
                for (i = value.Depth - 1 - RootDepth; i > 0; i--)
                {
                    //如果父级不是最后一个，则在父级对应的水平位置绘制竖线
                    if (!parent.IsLast())
                    {
                        linePoints.Add((fullWidth * i - halfWidth, 0, fullWidth * i - halfWidth, height));
                    }

                    parent = parent.Parent;
                }

                //最后根据点集绘制所有线
                foreach (var point in linePoints)
                {
                    lines.Children.Add(new Line()
                    {
                        StartPoint = new Point(point.x1, point.y1), EndPoint = new Point(point.x2, point.y2),
                        [!Shape.StrokeProperty] = new DynamicResourceExtension("Foreground0"),
                    });
                }
            }

            //文件文件夹图标
            var icon = new TextBlock()
            {
                Text = value is TreeDirInfo ? "\uE8B7" : "\uE160",
                Foreground = value is TreeDirInfo ? Brushes.ForestGreen : Brushes.Coral,
                FontFamily = this.FindResource("IconFont") as FontFamily ?? throw new Exception("找不到IconFont"),
                // [!MarginProperty] = new Binding(nameof(SimpleFileInfo.Depth)) 
                // { Converter = new DirDepthMarginConverter() },
            };

            //文件名
            var tbkName = new TextBlock()
            {
                [!TextBlock.TextProperty] = new Binding(nameof(SimpleFileInfo.Name)),
                // Margin = new Thickness(0, 0, 0, 0)
            };
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { lines, icon, tbkName }
            };
        });

        column.CellTemplate = cellTemplate;
        return column;
    }

    protected override DataGridColumn GetLengthColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnLengthHeader,
            Binding = new Binding()
                { Converter = new TreeFileDirLengthConverter(), Mode = BindingMode.OneWay },
            IsReadOnly = true,
        };
    }
}