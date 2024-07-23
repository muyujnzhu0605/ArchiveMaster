using Avalonia;
using Avalonia.Controls;
using System;

namespace ArchiveMaster.Views
{
    public partial class TwoStepPanelBase : PanelBase
    {
        public static readonly StyledProperty<object> ConfigsContentProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, object>(nameof(ConfigsContent));

        public static readonly StyledProperty<object> ResultsContentProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, object>(nameof(ResultsContent));

        public TwoStepPanelBase()
        {
            InitializeComponent();
        }
        public object ConfigsContent
        {
            get => GetValue(ConfigsContentProperty);
            set => SetValue(ConfigsContentProperty, value);
        }
        public object ResultsContent
        {
            get => GetValue(ResultsContentProperty);
            set => SetValue(ResultsContentProperty, value);
        }
    }
}
