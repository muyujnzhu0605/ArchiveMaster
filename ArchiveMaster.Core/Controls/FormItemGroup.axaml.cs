using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FzLib.Avalonia.Controls;

namespace ArchiveMaster.Controls;

public partial class FormItemGroup : WrapPanel
{
    public FormItemGroup()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<double> LabelWidthProperty =
        FormItem.LabelWidthProperty.AddOwner<FormItemGroup>();

    public double LabelWidth
    {
        get => GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }
}