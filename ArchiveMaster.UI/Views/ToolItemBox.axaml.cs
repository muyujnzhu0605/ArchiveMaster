using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ArchiveMaster.Views
{
    public partial class ToolItemBox : UserControl
    {
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<ToolItemBox, string>(nameof(Description));

        public static readonly StyledProperty<string> IconProperty =
            AvaloniaProperty.Register<ToolItemBox, string>(nameof(Icon));

        public static readonly StyledProperty<bool> ShowDescriptionProperty =
            AvaloniaProperty.Register<ToolItemBox, bool>(nameof(ShowDescription), true);

        public static readonly StyledProperty<string> TitleProperty =
                    AvaloniaProperty.Register<ToolItemBox, string>(nameof(Title));

        public ToolItemBox()
        {
            InitializeComponent();
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool ShowDescription
        {
            get => GetValue(ShowDescriptionProperty);
            set => SetValue(ShowDescriptionProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}
