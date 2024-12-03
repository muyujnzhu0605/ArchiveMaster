using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ArchiveMaster.Views;

public class PopupFlyout : Flyout
{
    public new Popup Popup => base.Popup;
}