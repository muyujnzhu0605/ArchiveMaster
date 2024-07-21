using Avalonia;
using Avalonia.Controls;
using System;

namespace ArchiveMaster.Views
{
    public partial class TwoStepPanelBase : UserControl
    {
        public static readonly StyledProperty<object> ConfigsContentProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, object>(nameof(ConfigsContent));

        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, string>(nameof(Description));

        public static readonly StyledProperty<object> ResultsContentProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, object>(nameof(ResultsContent));

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TwoStepPanelBase, string>(nameof(Title));

        public TwoStepPanelBase()
        {
            InitializeComponent();
        }
        public object ConfigsContent
        {
            get => GetValue(ConfigsContentProperty);
            set => SetValue(ConfigsContentProperty, value);
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public object ResultsContent
        {
            get => GetValue(ResultsContentProperty);
            set => SetValue(ResultsContentProperty, value);
        }

        public string Title
        {
            get => this.GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private void ReturnButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            RequestClosing?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RequestClosing;
    }
}
