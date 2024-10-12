using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using FzLib.Avalonia.Dialogs;

namespace ArchiveMaster.Views;

public partial class ProgressDialog : DialogHost
{
    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<ProgressDialog>();

    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<ProgressDialog, string>(
        nameof(Message));

    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<ProgressDialog>();

    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<ProgressDialog>();

    public ProgressDialog()
    {
        InitializeComponent();
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}