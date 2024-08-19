using ArchiveMaster.Configs;
using ArchiveMaster.Converters;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using FzLib.Avalonia.Converters;

namespace ArchiveMaster.Views;

public class SimpleFileDataGrid : DataGrid
{
    public SimpleFileDataGrid()
    {
        CanUserReorderColumns = false;
        CanUserResizeColumns = false;
        CanUserSortColumns = false;
        this[!IsReadOnlyProperty] =
            new Binding(nameof(TwoStepViewModelBase<TwoStepUtilityBase<ConfigBase>, ConfigBase>.IsWorking));
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);

    public double ColumnIsCheckedIndex { get; init; } = 0.1;
    public double ColumnStatusIndex { get; init; } = 0.2;
    public double ColumnNameIndex { get; init; } = 0.3;
    public double ColumnPathIndex { get; init; } = 0.4;
    public double ColumnLengthIndex { get; init; } = 0.5;
    public double ColumnTimeIndex { get; init; } = 0.6;

    public double ColumnMessageIndex { get; init; } = 999;

    public string ColumnIsCheckedHeader { get; init; } = "";
    public string ColumnStatusHeader { get; init; } = "状态";
    public string ColumnNameHeader { get; init; } = "文件名";
    public string ColumnPathHeader { get; init; } = "路径";
    public string ColumnLengthHeader { get; init; } = "文件大小";
    public string ColumnTimeHeader { get; init; } = "修改时间";
    public string ColumnMessageHeader { get; init; } = "信息";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        (double Index, Func<DataGridColumn> Func)[] items =
        [
            (ColumnIsCheckedIndex, GetIsCheckedColumn),
            (ColumnStatusIndex, GetProcessStatusColumn),
            (ColumnNameIndex, GetNameColumn),
            (ColumnPathIndex, GetPathColumn),
            (ColumnLengthIndex, GetLengthColumn),
            (ColumnTimeIndex, GetTimeColumn),
            (ColumnMessageIndex, GetMessageColumn),
        ];

        //插入的，从后往前插，这样不会打乱顺序
        var ordered1 = items
            .Where(p => p.Index >= 0)
            .Where(p => p.Index < Columns.Count)
            .OrderByDescending(p => p.Index);

        //追加的，按序号从小到大调用Add方法
        var ordered2 = items
            .Where(p => p.Index >= 0)
            .Where(p => p.Index >= Columns.Count)
            .OrderBy(p => p.Index);

        foreach (var column in ordered1)
        {
            Columns.Insert((int)column.Index, column.Func());
        }

        foreach (var column in ordered2)
        {
            Columns.Add(column.Func());
        }
    }

    private DataGridColumn GetNameColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnNameHeader,
            Binding = new Binding(nameof(SimpleFileInfo.Name)),
            IsReadOnly = true
        };
    }

    private DataGridColumn GetMessageColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnMessageHeader,
            Binding = new Binding(nameof(SimpleFileInfo.Message)),
            IsReadOnly = true,
        };
    }

    private DataGridColumn GetTimeColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnTimeHeader,
            Binding = new Binding(nameof(SimpleFileInfo.Time)),
            IsReadOnly = true,
        };
    }

    private DataGridColumn GetLengthColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnLengthHeader,
            Binding = new Binding(nameof(SimpleFileInfo.Length))
                { Converter = new FileLength2StringConverter(), Mode = BindingMode.OneWay },
            IsReadOnly = true,
        };
    }

    private DataGridColumn GetPathColumn()
    {
        return new DataGridTextColumn()
        {
            Header = ColumnPathHeader,
            Binding = new Binding(nameof(SimpleFileInfo.Path)),
            IsReadOnly = true,
        };
    }

    private DataGridColumn GetProcessStatusColumn()
    {
        var column = new DataGridTemplateColumn
        {
            CanUserResize = false,
            CanUserReorder = false,
            CanUserSort = false,
            Header = ColumnStatusHeader
        };

        var cellTemplate = new FuncDataTemplate<SimpleFileInfo>((value, namescope) => new Ellipse
        {
            Width = 8,
            Height = 8,
            [!Shape.FillProperty] = new Binding(nameof(SimpleFileInfo.Status))
                { Converter = new ProcessStatusColorConverter() }
        });

        column.CellTemplate = cellTemplate;
        return column;
    }

    private DataGridColumn GetIsCheckedColumn()
    {
        var column = new DataGridTemplateColumn
        {
            CanUserResize = false,
            CanUserReorder = false,
            CanUserSort = false,
            Header = ColumnIsCheckedHeader
        };
        var cellTemplate = new FuncDataTemplate<SimpleFileInfo>((value, namescope) =>
        {
            var rootPanel = this.GetLogicalAncestors().OfType<TwoStepPanelBase>().FirstOrDefault();

            return new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(SimpleFileInfo.IsChecked)),
                [!IsEnabledProperty] = new Binding("DataContext.IsWorking") //执行命令时，这CheckBox不可以Enable
                    { Source = rootPanel, Converter = new InverseBoolConverter() },
            };
        });

        column.CellTemplate = cellTemplate;
        return column;
    }
}