using ArchiveMaster.Converters;
using ArchiveMaster.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;

namespace ArchiveMaster.Views;

public class SimpleFileDataGrid : DataGrid
{
    public SimpleFileDataGrid()
    {
        CanUserReorderColumns = false;
        CanUserResizeColumns = false;
        CanUserSortColumns = false;
    }
    protected override Type StyleKeyOverride => typeof(DataGrid);

    public double IsCheckedColumnIndex { get; init; } = 0.1;
    public double ProcessStatusColumnIndex { get; init; } = 0.2;
    public double NameColumnIndex { get; init; } = 0.3;
    public double PathColumnIndex { get; init; } = 0.4;
    public double TimeColumnIndex { get; init; } = 0.5;

    public double MessageColumnIndex { get; init; } = 999;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        (double Index, Func<DataGridColumn> Func)[] items =
        [
            (IsCheckedColumnIndex, GetIsCheckedColumn),
            (ProcessStatusColumnIndex, GetProcessStatusColumn),
            (NameColumnIndex, GetNameColumn),
            (PathColumnIndex, GetPathColumn),
            (TimeColumnIndex, GetTimeColumn),
            (MessageColumnIndex, GetMessageColumn),
        ];

        //插入的，从后往前插，这样不会打乱顺序
        var ordered1 = items
            .Where(p=>p.Index>=0)
            .Where(p=>p.Index<Columns.Count)
            .OrderByDescending(p=>p.Index);
        
        //追加的，按序号从小到大调用Add方法
        var ordered2 = items
            .Where(p=>p.Index>=0)
            .Where(p=>p.Index>=Columns.Count)
            .OrderBy(p=>p.Index);

        foreach (var column in ordered1)
        {
            Columns.Insert((int)column.Index,column.Func());
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
            Header = "文件名",
            Binding = new Binding(nameof(SimpleFileInfo.Name)),
            IsReadOnly = true
        };
    }
    private DataGridColumn GetMessageColumn()
    {
        return new DataGridTextColumn()
        {
            Header = "信息",
            Binding = new Binding(nameof(SimpleFileInfo.Message)),
            IsReadOnly = true,
        };
    }
    
    private DataGridColumn GetTimeColumn()
    {
        return new DataGridTextColumn()
        {
            Header = "修改时间",
            Binding = new Binding(nameof(SimpleFileInfo.Time)),
            IsReadOnly = true,
        };
    }
    
    private DataGridColumn GetPathColumn()
    {
        return new DataGridTextColumn()
        {
            Header = "路径",
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
            Header = "状态"
        };

        var cellTemplate = new FuncDataTemplate<SimpleFileInfo>((value, namescope) => new Ellipse
        {
            Width = 8,
            Height = 8,
            [!Shape.FillProperty] = new Binding(nameof(SimpleFileInfo.Status)){Converter = new ProcessStatusColorConverter()}
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
            Header = ""
        };

        var cellTemplate = new FuncDataTemplate<SimpleFileInfo>((value, namescope) => new CheckBox()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(SimpleFileInfo.IsChecked))
        });

        column.CellTemplate = cellTemplate;
        return column;
    }
}