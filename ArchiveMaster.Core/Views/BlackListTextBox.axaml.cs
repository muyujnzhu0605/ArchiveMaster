using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArchiveMaster.Views;

public partial class BlackListTextBox : UserControl
{
    public BlackListTextBox()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<BlackListTextBox, string>(
        nameof(Text));

    public static readonly StyledProperty<bool> UseRegexProperty = AvaloniaProperty.Register<BlackListTextBox, bool>(
        nameof(UseRegex));


    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public bool UseRegex
    {
        get => GetValue(UseRegexProperty);
        set => SetValue(UseRegexProperty, value);
    }
}