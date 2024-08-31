using System.Collections;
using System.Collections.ObjectModel;
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

namespace ArchiveMaster.Views;

public class TreeFileDataGrid : SimpleFileDataGrid
{
    protected override Type StyleKeyOverride => typeof(TreeFileDataGrid);

    public TreeFileDataGrid()
    {
        DoubleTapped += DataGridDoubleTapped;
    }

    public override double ColumnPathIndex => -1;

    private void DataGridDoubleTapped(object sender, TappedEventArgs e)
    {
        if (!(e.Source is Visual { DataContext: TreeDirInfo dir }))
        {
            return;
        }

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

    protected override DataGridColumn GetNameColumn()
    {
        var column = new DataGridTemplateColumn
        {
            Header = ColumnNameHeader
        };
        var cellTemplate = new FuncDataTemplate<TreeFileDirInfo>((value, namescope) =>
        {
            var lines = new Canvas()
            {
                Width = 16 * value.Depth,
                Height = 32,
            };

            List<(double x1, double y1, double x2, double y2)> linePoints =
                new List<(double x1, double y1, double x2, double y2)>();
            for (int i = 1; i <= value.Depth; i++)
            {
                if (i != value.Depth)
                {
                    linePoints.Add((16 * i - 8, 0, 16 * i - 8, 32));
                }
                else
                {
                    if (value.Index == value.Parent.Subs.Count - 1)
                    {
                        linePoints.Add((16 * i - 8, 0, 16 * i - 8, 16));
                        linePoints.Add((16 * i - 8, 16, 16 * i, 16));
                    }
                    else
                    {
                        linePoints.Add((16 * i - 8, 0, 16 * i - 8, 32));
                        linePoints.Add((16 * i - 8, 16, 16 * i, 16));
                    }
                }
            }

            foreach (var point in linePoints)
            {
                lines.Children.Add(new Line()
                {
                    StartPoint = new Point(point.x1, point.y1), EndPoint = new Point(point.x2, point.y2),
                    [!Shape.StrokeProperty] = new DynamicResourceExtension("Foreground0"),
                });
            }

            var icon = new TextBlock()
            {
                Text = value is TreeDirInfo ? "\uE8B7" : "\uE160",
                Foreground = value is TreeDirInfo ? Brushes.ForestGreen : Brushes.Coral,
                FontFamily = this.FindResource("IconFont") as FontFamily ?? throw new Exception("找不到IconFont"),
                // [!MarginProperty] = new Binding(nameof(SimpleFileInfo.Depth)) 
                // { Converter = new DirDepthMarginConverter() },
            };
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